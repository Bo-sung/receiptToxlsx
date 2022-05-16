using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ChristmasMatchMultiMazeEntry : BattleEntry
    {
        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;

        /// <summary>
        /// 몬스터 스피드 체크 (서버 위치와 이 이상 멀어졌을 때 강제로 Speed 변경)
        /// </summary>
        private const float MONSTER_CHANGE_SPEED_MIN_DISTANCE = 1f;

        /// <summary>
        /// 몬스터 스피드 체크 (서버 위치와 이 이상 멀어졌을 때 강제로 Warp 시킨다)
        /// </summary>
        private const float MONSTER_CHANGE_SPEED_MAX_DISTANCE = 8f;

        /// <summary>
        /// 기본 속도 (이속, 공속)
        /// </summary>
        private const int DEFAULT_SPEED = 10000;

        /// <summary>
        /// 행동불능 상태 시간: 7초
        /// </summary>
        private const float GROGGY_TIME = 7f * 1000;

        /// <summary>
        /// 기본 보상 아이템 (눈덩이)
        /// </summary>
        private const int BASE_REWARD_ITEM_NAME = 94178;

        /// <summary>
        /// 기본 카메라 뷰
        /// </summary>
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;

        private enum PlayerState : byte
        {
            General = 1, // 일반
            BattleBoss = 5, // 보스 전투 중
            Defenseless = 7, // 무방비 상태
            Dead = 9, // 죽어있음
            PowerUp = 100, // 강탈가능상태
        }

        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly MultiMazeDataManager multiMazeDataRepo;

        // <!-- Scene Compositions --!>
        private readonly IBattlePool battlePool;
        private readonly List<PoolObject> poolObjectList;
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly MonsterBotEntityPool monsterBotPool;
        private readonly MazeCubeStatePool mazeSnowballPool;
        private readonly MazeCubeStatePool mazeRudolphPool;
        private readonly MazeCubeStatePool mazePowerUpPotionPool;
        private readonly Dictionary<int, MatchMazeMonsterPacket.MatchMazeMonsterType> monsterTypeDic;

        // <!-- UIs --!>
        private UIController uiController;
        private UIBattleMenu uiBattleMenu;
        private UIResultDungeon uiResultDungeon;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMainTop uiMainTop;
        private UIMvpTimer uiMvpTimer;
        private UIBattleInfo uiBattleInfo;
        private UIBattleProgress uiBattleProgress;
        private UICamSelect uiCamSelect;
        private UIStartingPistol uiStartingPistol;
        private UIBattleDescription uiBattleDescription;

        // <!-- Temp Data --!>
        private MultiMazeData currentData;
        private Vector3 savedPlayerPosition;
        private RemainTime remainTime;
        private RemainTime countdownTime;
        private bool isExitBattle;
        private float characterSpeed;
        private int snowballCount; // 내가 먹은 눈덩이 수
        private int rudolphCount;
        private bool isFinishedTimer;
        private bool isTryExitMultiMaze;
        private bool isPowerUp;

        public ChristmasMatchMultiMazeEntry() : base(BattleMode.ChristmasMatchMultiMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            dungeonModel = player.Dungeon;

            multiMazeDataRepo = MultiMazeDataManager.Instance;

            battlePool = BattlePoolManager.Instance;
            poolObjectList = new List<PoolObject>();
            playerBotPool = new PlayerBotEntityPool();
            monsterBotPool = new MonsterBotEntityPool();
            mazeSnowballPool = new MazeCubeStatePool();
            mazeRudolphPool = new MazeCubeStatePool();
            mazePowerUpPotionPool = new MazeCubeStatePool();
            monsterTypeDic = new Dictionary<int, MatchMazeMonsterPacket.MatchMazeMonsterType>(IntEqualityComparer.Default);
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            // 오브젝트 풀 폐기
            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();

            // 플레이어 봇 폐기
            foreach (var item in playerBotPool)
            {
                item.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
                item.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
                Dispose(item);
            }

            // 몬스터 봇 폐기
            foreach (var item in monsterBotPool)
            {
                Dispose(item);
            }

            playerBotPool.Clear();
            monsterBotPool.Clear();
            mazeSnowballPool.Clear();
            mazeRudolphPool.Clear();
            mazePowerUpPotionPool.Clear();
            monsterTypeDic.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            player.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();

            Protocol.REQUEST_MULMAZE_ROOM_TRANSFORM.AddEvent(OnRequestMultiMazeRoomTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_EXIT.AddEvent(OnReceiveMultiMazeRoomExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.AddEvent(OnReceiveMultiMazeRoomMonsterMove);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.AddEvent(OnReceiveMultiMazeMonsterRegen);
            Protocol.RECEIVE_MULMAZE_GET_QUESTCOIN.AddEvent(OnReceiveMultiMazeGetSnowball);
            Protocol.RECEIVE_MULMAZE_QUESTCOIN_REGEN.AddEvent(OnReceiveMultiMazeSnowballRegen);
            Protocol.REQUEST_MULMAZE_TABFREEZE.AddEvent(OnRequestMultiMazeTabFreeze);
            Protocol.RECEIVE_MULMAZE_FREEZEEND.AddEvent(OnReceiveMultiMazeFreezeEnd);
            Protocol.RECEIVE_MULMAZE_FREEZESTART.AddEvent(OnReceiveMultiMazeFreezeStart);
            Protocol.RECEIVE_MULMAZE_EVENT1ITEM_REGEN.AddEvent(OnReceiveMultiMazeRudolphRegen);
            Protocol.RECEIVE_MULMAZE_EVENT2ITEM_REGEN.AddEvent(OnReceiveMultiMazePowerUpRegen);
            Protocol.RECEIVE_MULMAZE_GET_EVENT1ITEM.AddEvent(OnReceiveMultiMazeGetRudolph);
            Protocol.RECEIVE_MULMAZE_GET_EVENT2ITEM.AddEvent(OnReceiveMultiMazeGetPowerUp);
            Protocol.RECEIVE_MULMAZE_MONKILL.AddEvent(OnReceiveMultiMazeMonsterKill);
            Protocol.RECEIVE_POWERUP_END.AddEvent(OnPowerUpEnd);
            Protocol.RECEIVE_MULMAZE_TAKE_EVENTITEM.AddEvent(OnMultiMazeTakeSnowball);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();

            Protocol.REQUEST_MULMAZE_ROOM_TRANSFORM.RemoveEvent(OnRequestMultiMazeRoomTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_EXIT.RemoveEvent(OnReceiveMultiMazeRoomExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.RemoveEvent(OnReceiveMultiMazeRoomMonsterMove);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.RemoveEvent(OnReceiveMultiMazeMonsterRegen);
            Protocol.RECEIVE_MULMAZE_GET_QUESTCOIN.RemoveEvent(OnReceiveMultiMazeGetSnowball);
            Protocol.RECEIVE_MULMAZE_QUESTCOIN_REGEN.RemoveEvent(OnReceiveMultiMazeSnowballRegen);
            Protocol.REQUEST_MULMAZE_TABFREEZE.RemoveEvent(OnRequestMultiMazeTabFreeze);
            Protocol.RECEIVE_MULMAZE_FREEZEEND.RemoveEvent(OnReceiveMultiMazeFreezeEnd);
            Protocol.RECEIVE_MULMAZE_FREEZESTART.RemoveEvent(OnReceiveMultiMazeFreezeStart);
            Protocol.RECEIVE_MULMAZE_EVENT1ITEM_REGEN.RemoveEvent(OnReceiveMultiMazeRudolphRegen);
            Protocol.RECEIVE_MULMAZE_EVENT2ITEM_REGEN.RemoveEvent(OnReceiveMultiMazePowerUpRegen);
            Protocol.RECEIVE_MULMAZE_GET_EVENT1ITEM.RemoveEvent(OnReceiveMultiMazeGetRudolph);
            Protocol.RECEIVE_MULMAZE_GET_EVENT2ITEM.RemoveEvent(OnReceiveMultiMazeGetPowerUp);
            Protocol.RECEIVE_MULMAZE_MONKILL.RemoveEvent(OnReceiveMultiMazeMonsterKill);
            Protocol.RECEIVE_POWERUP_END.RemoveEvent(OnPowerUpEnd);
            Protocol.RECEIVE_MULMAZE_TAKE_EVENTITEM.RemoveEvent(OnMultiMazeTakeSnowball);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            dungeonModel.OnMultiJoinRewardEvent += OnMultiJoinRewardEvent;

            player.OnSpawnActor += OnPlayerSpawn;
            player.OnDespawnActor += OnPlayerDespawn;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            dungeonModel.OnMultiJoinRewardEvent -= OnMultiJoinRewardEvent;

            player.OnSpawnActor -= OnPlayerSpawn;
            player.OnDespawnActor -= OnPlayerDespawn;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiController = UI.Show<UIController>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiResultDungeon = UI.Show<UIResultDungeon>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiMainTop = UI.Show<UIMainTop>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattleProgress = UI.Show<UIBattleProgress>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiStartingPistol = UI.Show<UIStartingPistol>();
            uiBattleDescription = UI.Show<UIBattleDescription>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += GiveUp;
            uiResultDungeon.OnFinishDungeon += ExitMultiMaze;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;
            uiStartingPistol.OnFinish += StartMvpTimer;

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattlePlayerStatus.SetExtraMode(UIExtraBattlePlayerStatus.ExtraMode.Snowball, UIExtraBattlePlayerStatus.ExtraMode.Rudolph);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiMvpTimer.Hide();
            uiResultDungeon.Hide();
            uiBattleDescription.Initialize(LocalizeKey._90275, BASE_REWARD_ITEM_NAME);

            cameraController.SetView(DEFAULT_VIEW);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= GiveUp;
            uiResultDungeon.OnFinishDungeon -= ExitMultiMaze;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;
            uiStartingPistol.OnFinish -= StartMvpTimer;

            UI.Close<UIController>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultDungeon>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMainTop>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattleProgress>();
            UI.Close<UICamSelect>();
            UI.Close<UIStartingPistol>();
            UI.Close<UIBattleDescription>();

            uiController = null;
            uiBattleMenu = null;
            uiResultDungeon = null;
            uiBattlePlayerStatus = null;
            uiMainTop = null;
            uiMvpTimer = null;
            uiBattleInfo = null;
            uiBattleProgress = null;
            uiCamSelect = null;
            uiStartingPistol = null;
            uiBattleDescription = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            string title = currentData == null ? string.Empty : currentData.name_id.ToText();
            uiBattleInfo.Set(title);
        }

        public override void OnBack()
        {
            GiveUp();
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            Task<bool> task = RequestExit();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            IsSuccessExit = task.Result;
        }

        /// <summary>
        /// 특정 데이터로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
            if (!(battleInput is MatchMultiMazePacket))
            {
                IsSuccessEnter = false;
                yield break;
            }

            MatchMultiMazePacket packet = battleInput as MatchMultiMazePacket;
            int id = packet.multiMazeId;
            currentData = multiMazeDataRepo.Get(id);
            if (currentData == null)
            {
                IsSuccessEnter = false;
                Debug.LogError($"해당 id가 존재하지 않습니다: {nameof(id)} = {id}");
                yield break;
            }

            UI.HideIndicator(); // 서버 변경으로 들어왔을 경우에 대비하여 인디케이터 제거
            IsSuccessEnter = true; // 이미 프로토콜을 통해서 왔기 때문에 무조건 성공

            Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.EventMultiMaze.ToIntValue()); // 특정던전 입장 횟수
            dungeonModel.SetLastEnterMultiMazeLobbyId(currentData.id); // 마지막 입장한 Lobby도 변경해 줌 (이벤트 던전의 경우에는 chapter 가 아니라 id로 처리)

            int channelId = packet.channelId;
            MultiMazePlayerPacket[] arrMultiMazePlayerPacket = packet.arrMultiMazePlayerPacket;
            MatchMazeMonsterPacket[] arrMazeMonsterPacket = packet.arrMazeMonsterPacket;
            int mapId = packet.mapId;
            MazeCubePacket[] arrMazeCubePacket = packet.arrMazeCubePacket;
            MazeCubePacket[] arrMazeItem1Packet = packet.arrMazeItem1Packet;
            MazeCubePacket[] arrMazeItem2Packet = packet.arrMazeItem2Packet;

            // 추가 데이터 세팅
            float normalMonsterSpeed = currentData.GetNormalMonsterSpeed();
            foreach (var item in arrMazeMonsterPacket)
            {
                MonsterType type = item.monsterType;
                int level = currentData.GetLevel(type);
                float scale = currentData.GetScale(type);
                item.SetLevel(level);
                item.SetScale(scale);
                item.SetMoveSpeed(normalMonsterSpeed);

                monsterTypeDic.Add(item.index, item.matchMazeMonsterType);
            }

            playerBotPool.EnqueueRange(arrMultiMazePlayerPacket);
            monsterBotPool.EnqueueRange(arrMazeMonsterPacket);
            mazeSnowballPool.EnqueueRange(arrMazeCubePacket);
            mazeRudolphPool.EnqueueRange(arrMazeItem1Packet);
            mazePowerUpPotionPool.EnqueueRange(arrMazeItem2Packet);
            remainTime = packet.remainTime;
            countdownTime = packet.countdownTime;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = currentData.scene_name; // 씬 이름
            string bgmName = currentData.bgm; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            characterSpeed = currentData.GetCharacterSpeed();

            uiBattlePlayerStatus.SetPlayer(player);

            player.SetCurrentMp(player.MaxMp);

            snowballCount = 0;
            rudolphCount = 0;

            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Snowball, snowballCount);
            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Rudolph, rudolphCount);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnCharacters(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMonsterBots(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMazeSnowball(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMazeRudolph(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMazePowerUpPotiion(), TAG);
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCharacters()
        {
            while (playerBotPool.HasQueue())
            {
                IMultiPlayerInput input = playerBotPool.Dequeue();
                bool isPlayer = input.Cid == player.Character.Cid;

                if (isPlayer)
                {
                    // 플레이어
                    if (impl.Add(player, isEnemy: false))
                    {
                        player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
                        player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

                        UnitActor unitActor = player.SpawnActor(); // 유닛 소환
                        unitActor.Movement.SetDefaultSpeed(characterSpeed);
                        unitActor.Radar.SetSenseDistance(2f);
                        unitActor.Radar.SetSenseTargetType(TargetType.EnemyCharacter);

                        Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);
                        savedPlayerPosition = FindClosestEdge(pos); // 씬 로드 후에 네비 위치로 변경
                        unitActor.AI.SetHomePosition(savedPlayerPosition, isWarp: false); // Home 세팅
                        unitActor.AI.ChangeFrozenDefenselessState(); // 빙결 그로기
                        unitActor.Movement.ForceWarp(savedPlayerPosition); // Home으로 강제 워프

                        cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅

                        player.ReadyToBattle();
                        yield return Timing.WaitForOneFrame;
                    }
                }
                else
                {
                    // 플레이어 봇
                    PlayerBotEntity entity = playerBotPool.Create(input);
                    if (impl.Add(entity, isEnemy: true))
                    {
                        entity.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
                        entity.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

                        UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
                        unitActor.Movement.SetDefaultSpeed(characterSpeed);
                        Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);

                        Vector3 position = FindClosestEdge(pos);
                        unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
                        unitActor.AI.ChangeFrozenDefenselessState(); // 빙결 그로기
                        unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

                        PlayerState state = input.State.ToEnum<PlayerState>();
                        // 상태 변경
                        switch (state)
                        {
                            case PlayerState.Defenseless:
                                unitActor.AI.ChangeState(AI.Transition.Groggy); // 그로기상태
                                break;

                            case PlayerState.Dead:
                                unitActor.AI.ChangeState(AI.Transition.Dead); // 죽은상태
                                break;
                        }

                        entity.ReadyToBattle();
                        yield return Timing.WaitForOneFrame;
                    }
                }
            }
        }

        /// <summary>
        /// 몬스터 봇 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonsterBots()
        {
            while (monsterBotPool.HasQueue())
            {
                IMonsterBotInput input = monsterBotPool.Dequeue();
                UnitEntity entity = monsterBotPool.Create(input);

                if (!impl.Add(entity, isEnemy: true))
                    continue;

                UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
                if (input.MoveSpeed.HasValue)
                    unitActor.Movement.SetDefaultSpeed(input.MoveSpeed.Value);

                Vector3 pos = new Vector3(input.PosX, input.PosY, input.PosZ);
                unitActor.AI.SetHomePosition(pos, isWarp: false); // Home 세팅
                unitActor.Movement.ForceWarp(pos); // Home으로 강제 워프
                unitActor.AI.ReadyToBattle(); // 몬스터 봇의 hp를 맞춰주기 위해 ReadyToBattle 호출
                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것

                // Hp 가 존재하는 몬스터의 경우
                if (input.HasCurHp)
                    entity.SetCurrentHp(input.CurHp);

                // 상태 변경
                MonsterBotState state = input.State.ToEnum<MonsterBotState>();
                switch (state)
                {
                    case MonsterBotState.MazeBattle:
                        unitActor.AI.ChangeState(AI.Transition.Match); // 전투매칭상태
                        break;

                    case MonsterBotState.Die:
                        unitActor.AI.ChangeState(AI.Transition.Dead); // 죽은상태
                        break;
                }

                MatchMazeMonsterPacket.MatchMazeMonsterType type = monsterTypeDic[input.Index];
                if (type.HasFlag(MatchMazeMonsterPacket.MatchMazeMonsterType.LosePlayerCoin))
                    unitActor.EffectPlayer.ShowUnitAura(UnitAura.AuraType.MonsterBlue);

                if (type.HasFlag(MatchMazeMonsterPacket.MatchMazeMonsterType.FreezePlayer))
                    unitActor.EffectPlayer.ShowUnitAura(UnitAura.AuraType.MonsterRed);

                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 눈덩이 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMazeSnowball()
        {
            while (mazeSnowballPool.HasQueue())
            {
                IMazeCubeStateInfo input = mazeSnowballPool.Dequeue();
                MazeCubeStatePool.Info info = mazeSnowballPool.Create(input);
                info.serverIndex = input.Index;
                info.state = input.State;
                info.position = FindClosestEdge(new Vector3(input.PosX, input.PosY, input.PosZ));

                switch (info.state)
                {
                    case MazeCubeState.General:
                        if (SpawnMazeSnowball(info))
                            yield return Timing.WaitForOneFrame;
                        break;

                    case MazeCubeState.StandByRespawn:
                        break;
                }
            }
        }

        /// <summary>
        /// 루돌프 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMazeRudolph()
        {
            while (mazeRudolphPool.HasQueue())
            {
                IMazeCubeStateInfo input = mazeRudolphPool.Dequeue();
                MazeCubeStatePool.Info info = mazeRudolphPool.Create(input);
                info.serverIndex = input.Index;
                info.state = input.State;
                info.position = FindClosestEdge(new Vector3(input.PosX, input.PosY, input.PosZ));

                switch (info.state)
                {
                    case MazeCubeState.General:
                        if (SpawnMazeRudolph(info))
                            yield return Timing.WaitForOneFrame;
                        break;

                    case MazeCubeState.StandByRespawn:
                        break;
                }
            }
        }

        /// <summary>
        /// 강탈포션 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMazePowerUpPotiion()
        {
            while (mazePowerUpPotionPool.HasQueue())
            {
                IMazeCubeStateInfo input = mazePowerUpPotionPool.Dequeue();
                MazeCubeStatePool.Info info = mazePowerUpPotionPool.Create(input);
                info.serverIndex = input.Index;
                info.state = input.State;
                info.position = FindClosestEdge(new Vector3(input.PosX, input.PosY, input.PosZ));

                switch (info.state)
                {
                    case MazeCubeState.General:
                        if (SpawnMazePowerUpPotion(info))
                            yield return Timing.WaitForOneFrame;
                        break;

                    case MazeCubeState.StandByRespawn:
                        break;
                }
            }
        }

        /// <summary>
        /// 눈덩이 생성
        /// </summary>
        private bool SpawnMazeSnowball(MazeCubeStatePool.Info info)
        {
            IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(info.position, MazeRewardType.Snowball);
            mazeDropItem.Set(info.serverIndex, MazeRewardType.Snowball, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Snowball));
            info.SetMazeDropItem(mazeDropItem);
            return true;
        }

        /// <summary>
        /// 루돌프 생성
        /// </summary>
        private bool SpawnMazeRudolph(MazeCubeStatePool.Info info)
        {
            IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(info.position, MazeRewardType.Rudolph);
            mazeDropItem.Set(info.serverIndex, MazeRewardType.Rudolph, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Rudolph));
            info.SetMazeDropItem(mazeDropItem);
            return true;
        }

        /// <summary>
        /// 강탈물약 생성
        /// </summary>
        private bool SpawnMazePowerUpPotion(MazeCubeStatePool.Info info)
        {
            IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(info.position, MazeRewardType.PowerUpPotion);
            mazeDropItem.Set(info.serverIndex, MazeRewardType.PowerUpPotion, null);
            info.SetMazeDropItem(mazeDropItem);
            return true;
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            System.TimeSpan timeSpan = System.TimeSpan.FromMilliseconds(countdownTime); // 카운트다운 시간
            uiStartingPistol.Run((float)timeSpan.TotalSeconds);
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        void OnMultiJoinRewardEvent(int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            if (droppedZeny > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.Zeny), droppedZeny, rewardCount: 0, rewardOption: 0));

            if (droppedLevelExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.LevelExp), droppedLevelExp, rewardCount: 0, rewardOption: 0));

            if (droppedJobExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.JobExp), droppedJobExp, rewardCount: 0, rewardOption: 0));

            if (rewards != null)
                rewardBuffer.AddRange(rewards);

            ShowResultUI();
        }


        #region 플레이어 이벤트

        /// <summary>
        /// 플레이어 생성 이벤트
        /// </summary>
        void OnPlayerSpawn(UnitActor unitActor)
        {
            unitActor.Movement.OnMazeDropItem += OnMazeDropItem;
            unitActor.Radar.OnTarget += OnTarget;
        }

        /// <summary>
        /// 플레이어 제거 이벤트
        /// </summary>
        void OnPlayerDespawn(UnitActor unitActor)
        {
            unitActor.Movement.OnMazeDropItem -= OnMazeDropItem;
            unitActor.Radar.OnTarget -= OnTarget;
        }

        /// <summary>
        /// 미로 오브젝트 충돌
        /// </summary>
        void OnMazeDropItem(IMazeDropItem mazeDropItem)
        {
            if (mazeDropItem == null)
                return;

            switch (mazeDropItem.RewardType)
            {
                case MazeRewardType.Snowball:
                    PlaySfx(Constants.SFX.Battle.SNOWBALL);
                    SendGetMazeSnowball(mazeDropItem);
                    break;

                case MazeRewardType.Rudolph:
                    PlaySfx(Constants.SFX.Battle.SNOWBALL);
                    SendGetMazeRudolph(mazeDropItem);
                    break;

                case MazeRewardType.PowerUpPotion:
                    PlaySfx(Constants.SFX.Battle.POWER_UP_POTION);
                    SendGetMazePowerUpPotion(mazeDropItem);
                    break;
            }
        }

        void OnTarget(UnitActor unitActor)
        {
            // 파워업 상태가 아님
            if (!isPowerUp)
                return;

            if (unitActor == null)
                return;

            if (unitActor.Entity is PlayerBotEntity playerBotEntity)
            {
                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", playerBotEntity.Character.Cid);
                Protocol.REQUEST_MULMAZE_TAKE_EVENTITEM.SendAsync(sfs).WrapNetworkErrors();
            }
        }

        #endregion

        #region 서버 응답 이벤트

        void OnRequestMultiMazeRoomTransform(Response response)
        {
            int cid = response.GetInt("1");
            int[] arrayPosValue = response.GetIntArray("2");

            // 방어코드
            if (arrayPosValue == null)
                return;

            Vector3 position = new Vector3(arrayPosValue[0] * 0.001f, arrayPosValue[1] * 0.001f, arrayPosValue[2] * 0.001f);
            UpdatePlayerMove(cid, position); // 플레이어 움직임
        }

        void OnReceiveMultiMazeRoomExit(Response response)
        {
            const byte USER_EXIT = 1; // 단순 나감 (only 멀티플레이어)
            const byte PLAYER_OUT_SYSTEM = 6; // 플레이어 죽음: 단순 시스템 띄우기

            int cid = response.GetInt("1");
            byte exitType = response.GetByte("2");

            switch (exitType)
            {
                case USER_EXIT: // 단순 나감 (타 플레이어)
                    UpdatePlayerExit(cid); // 플레이어 퇴장
                    break;

                case PLAYER_OUT_SYSTEM:
                    UpdatePlayerExit(cid); // 플레이어 퇴장
                    break;
            }
        }

        void OnReceiveMultiMazeRoomMonsterMove(Response response)
        {
            int monsterIndex = response.GetByte("1");
            float prePosX = response.GetFloat("2");
            float prePosZ = response.GetFloat("3");
            float targetPosX = response.GetFloat("4");
            float targetPosZ = response.GetFloat("5");
            Vector3 prePos = new Vector3(prePosX, 0f, prePosZ);
            Vector3 targetPos = new Vector3(targetPosX, 0f, targetPosZ);

            UpdateMonsterGeneralState(monsterIndex); // 몬스터 상태 - 일반
            UpdateMonsterMove(monsterIndex, prePos, targetPos); // 몬스터 움직임
        }

        void OnReceiveMultiMazeMonsterRegen(Response response)
        {
            int monsterIndex = response.GetByte("1");
            int? bossHp = null;
            float posX = response.GetFloat("3");
            float posZ = response.GetFloat("4");

            if (response.ContainsKey("2"))
                bossHp = response.GetInt("2");

            Vector3 targetPos = new Vector3(posX, 0f, posZ);

            UpdateMonsterGeneralState(monsterIndex); // 몬스터 상태 - 일반

            if (bossHp.HasValue)
                UpdateMonsterHp(monsterIndex, bossHp.Value); // 몬스터 체력

            UpdateMonsterWarp(monsterIndex, targetPos); // 몬스터 워프
        }

        void OnReceiveMultiMazeGetSnowball(Response response)
        {
            int cid = response.GetInt("1");
            int mazeCubeIndex = response.GetByte("2");
            UpdateMazeSnowballGained(cid, mazeCubeIndex); // 눈덩이 획득
            UpdateSnowballCount(cid, snowballCount + 1); // 눈덩이 획득 (cid 가 플레이어의 경우에만 처리하기 때문에 snowballCount 보냄)
        }

        void OnReceiveMultiMazeSnowballRegen(Response response)
        {
            int mazeCubeIndex = response.GetByte("1");
            UpdateMazeSnowball(mazeCubeIndex); // 눈덩이 리젠
        }

        void OnReceiveMultiMazeRudolphRegen(Response response)
        {
            int mazeCubeIndex = response.GetByte("1");
            UpdateMazeRudolph(mazeCubeIndex); // 루돌프 리젠
        }

        void OnReceiveMultiMazePowerUpRegen(Response response)
        {
            int mazeCubeIndex = response.GetByte("1");
            UpdateMazePowerUpPotion(mazeCubeIndex); // 강탈물약 리젠
        }

        void OnReceiveMultiMazeGetRudolph(Response response)
        {
            int cid = response.GetInt("1");
            int mazeCubeIndex = response.GetByte("2");
            UpdateMazeRudolphGained(cid, mazeCubeIndex); // 루돌프 획득
            UpdateRudolphCount(cid, rudolphCount + 1); // 루돌프 획득 (cid 가 플레이어의 경우에만 처리하기 때문에 rudolphCount 보냄)
        }

        void OnReceiveMultiMazeGetPowerUp(Response response)
        {
            int cid = response.GetInt("1");
            int mazeCubeIndex = response.GetByte("2");
            UpdateMazePowerUpPotionGained(cid, mazeCubeIndex); // 강탈포션 획득
            UpdatePlayerPowerUpStart(cid); // 파워업 상태
        }

        void OnRequestMultiMazeTabFreeze(Response response)
        {
            long endTime = response.GetLong("1");
            long startTime = response.GetLong("2");
            uiBattleProgress.SetRemainTime(endTime - startTime);
        }

        void OnReceiveMultiMazeFreezeEnd(Response response)
        {
            int cid = response.GetInt("1"); // 충돌당한 사람
            int? monsterIndex = null; // 충돌한 몬스터 인덱스
            byte? monsterState = null; // 충돌한 몬스터 상태

            if (response.ContainsKey("5"))
                monsterIndex = response.GetByte("5");

            if (response.ContainsKey("6"))
                monsterState = response.GetByte("6");

            UpdatePlayerGeneralState(cid); // 평상시 상태

            // 몬스터 상태 변경
            if (monsterIndex.HasValue && monsterState.HasValue)
            {
                MonsterBotState monsterBotState = monsterState.Value.ToEnum<MonsterBotState>();
                switch (monsterBotState)
                {
                    case MonsterBotState.Patrol:
                        UpdateMonsterGeneralState(monsterIndex.Value);
                        break;

                    case MonsterBotState.Die:
                        UpdateMonsterDie(monsterIndex.Value);
                        break;
                }
            }
        }

        void OnReceiveMultiMazeFreezeStart(Response response)
        {
            int cid = response.GetInt("1");
            int monsterIndex = response.GetByte("4");
            byte? monsterState = null; // 충돌한 몬스터 상태

            if (response.ContainsKey("5"))
                monsterState = response.GetByte("5");

            // 몬스터 상태 변경
            if (monsterState.HasValue)
            {
                MonsterBotState monsterBotState = monsterState.Value.ToEnum<MonsterBotState>();
                switch (monsterBotState)
                {
                    case MonsterBotState.Patrol:
                        UpdateMonsterGeneralState(monsterIndex);
                        break;

                    case MonsterBotState.Die:
                        UpdateMonsterDie(monsterIndex);
                        break;
                }
            }

            UpdatePlayerDefenselessState(cid); // 행동불능 상태
        }

        void OnReceiveMultiMazeMonsterKill(Response response)
        {
            //int cid = response.GetInt("1"); // 플레이어는 변화 없음 (강탈물약 상태)
            int monsterIndex = response.GetByte("4");
            UpdateMonsterDie(monsterIndex);
        }

        void OnPowerUpEnd(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerPowerUpEnd(cid);
        }

        void OnMultiMazeTakeSnowball(Response response)
        {
            int targetCid = response.GetInt("1");
            int attackerCid = response.GetInt("2");
            int targetSnowballCount = response.GetInt("3");
            int attackerSnowballCount = response.GetInt("4");
            UpdateSnowballCount(targetCid, targetSnowballCount);
            UpdateSnowballCount(attackerCid, attackerSnowballCount);
            UpdateSnowballTake(targetCid, attackerCid);
        }

        #endregion

        #region UI 이벤트

        void OnStartController()
        {
            if (isFinishedTimer)
                return;

            if (player.IsDie)
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            // 행동 불능 상태
            if (unitActor.AI.CurrentState == AI.StateID.Defenseless)
            {
                // 카메라 흔들림
                unitActor.EffectPlayer.GenerateImpulse(BattlePoolManager.ImpulseType.ClickAttack);

                SendTabInDefenseless();
                return;
            }

            CameraUtils.InvokePlayerTrackingEffect();

            unitActor.Movement.Stop();
            unitActor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
            if (isFinishedTimer)
                return;

            if (position == Vector2.zero)
                return;

            Camera mainCamera = Camera.main;

            if (player.IsDie)
                return;

            if (player.battleCrowdControlInfo.GetCannotMove())
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            // 행동 불능 상태
            if (unitActor.AI.CurrentState == AI.StateID.Defenseless)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();

            unitActor.Movement.Move(motion);

            Vector3 dist = player.LastPosition - savedPlayerPosition;
            if (dist.sqrMagnitude < SEND_POSITION_SQR_MAGNITUDE)
                return;

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnResetController()
        {
            if (isFinishedTimer)
                return;

            if (player.IsDie)
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            // 행동 불능 상태
            if (unitActor.AI.CurrentState == AI.StateID.Defenseless)
                return;

            CameraUtils.Zoom(CameraZoomType.None);

            unitActor.AI.SetInputMove(isControl: false);
            unitActor.Movement.Stop();

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        private void GiveUp()
        {
            AsyncShowGiveUpMessage().WrapNetworkErrors();
        }

        private async Task AsyncShowGiveUpMessage()
        {
            if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                return;

            RequestExit().WrapNetworkErrors(); // 퇴장
        }

        void OnFinishedRemainTimer()
        {
            isFinishedTimer = true;

            uiMvpTimer.StopTimer();
            uiMvpTimer.Hide();
        }

        void StartMvpTimer()
        {
            uiMvpTimer.Initialize((long)remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();
        }

        #endregion

        /// <summary>
        /// 나가기 처리
        /// </summary>
        private async Task<bool> RequestExit()
        {
            // 이미 퇴장
            if (isExitBattle)
                return true;

            for (int i = 0; i < unitList.size; i++)
            {
                UnitActor actor = unitList[i].GetActor();
                if (actor == null)
                    continue;

                actor.AI.EndAI(); // 전투 중지
            }

            Response response = await Protocol.REQUEST_MULMAZE_ROOM_EXIT.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            isExitBattle = true; // 퇴장 완료
            return true;
        }

        /// <summary>
        /// 현재 위치 서버로 보내기
        /// </summary>
        private void SendPlayerCurrentPosition()
        {
            savedPlayerPosition = player.LastPosition;

            int[] posArray = { (int)savedPlayerPosition.x * 1000, (int)savedPlayerPosition.y * 1000, (int)savedPlayerPosition.z * 1000 };
            var sfs = Protocol.NewInstance();
            sfs.PutIntArray("2", posArray);
            Protocol.REQUEST_MULMAZE_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 행동불능상태 중 터치
        /// </summary>
        private void SendTabInDefenseless()
        {
            Protocol.REQUEST_MULMAZE_TABFREEZE.SendAsync().WrapNetworkErrors();
        }

        /// <summary>
        /// 눈덩이 획득
        /// </summary>
        private void SendGetMazeSnowball(IMazeDropItem mazeDropItem)
        {
            mazeDropItem.Hit();
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)mazeDropItem.PointId);
            Protocol.REQUEST_MULMAZE_GET_QUESTCOIN.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 루돌프 획득
        /// </summary>
        private void SendGetMazeRudolph(IMazeDropItem mazeDropItem)
        {
            mazeDropItem.Hit();
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)mazeDropItem.PointId);
            Protocol.REQUEST_MULMAZE_GET_EVENT1ITEM.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 강탈물약 획득
        /// </summary>
        private void SendGetMazePowerUpPotion(IMazeDropItem mazeDropItem)
        {
            mazeDropItem.Hit();
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)mazeDropItem.PointId);
            Protocol.REQUEST_MULMAZE_GET_EVENT2ITEM.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 멀티 미로 퇴장
        /// </summary>
        private void ExitMultiMaze()
        {
            if (isTryExitMultiMaze)
                return;

            isTryExitMultiMaze = true;
            impl.StartBattle(BattleMode.MultiMazeLobby);
        }

        /// <summary>
        /// 플레이어 움직임
        /// </summary>
        private void UpdatePlayerMove(int cid, Vector3 position)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueMove(cid, position))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 움직임: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            if (isPlayer)
            {
                unitActor.Movement.ForceWarp(position);
            }
            else
            {
                unitActor.Movement.ForceSetDestination(position);
            }
        }

        /// <summary>
        /// 플레이어 퇴장
        /// </summary>
        private void UpdatePlayerExit(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;
            if (isPlayer)
            {
                isExitBattle = true; // 이미 퇴장했기 때문에 서버에 Exit를 날리지 않아야 함
                return;
            }

            PlayerBotEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueLeave(cid))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 나감: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor actor = find.GetActor();
            if (actor)
                actor.EffectPlayer.SpawnUnitTeleport();

            find.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            find.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
            Dispose(find); // 제거
            playerBotPool.Recycle(find); // 회수
        }

        /// <summary>
        /// 플레이어 상태 - 일반
        /// </summary>
        private void UpdatePlayerGeneralState(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte GENERAL_STATE = (byte)PlayerState.General;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, GENERAL_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 일반 상태로 전환: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.AI.ChangeState(AI.Transition.Finished);

            if (isPlayer)
                uiBattleProgress.SetRemainTime(0L);
        }

        /// <summary>
        /// 플레이어 상태 - 행동불능
        /// </summary>
        private void UpdatePlayerDefenselessState(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte DEFENSELESS_STATE = (byte)PlayerState.Defenseless;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, DEFENSELESS_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 행동불능 상태: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.AI.ChangeState(AI.Transition.Groggy);

            if (isPlayer)
            {
                // 화면을 터치하여 행동 불능에서 빠져나오세요!
                uiBattleProgress.StartTimer(GROGGY_TIME, UIBattleProgress.IconType.Touch, LocalizeKey._3501.ToText());
            }
        }

        /// <summary>
        /// 플레이어 파워업 상태 시작
        /// </summary>
        private void UpdatePlayerPowerUpStart(int cid)
        {
            // 그로기 상태일 때 파워업 포션을 먹게 되면
            // 서버에서 그로기 End가 따로 오지 않는 버그가 존재하기 때문에
            // 강제로 평상시 상태로 변경한다.
            UpdatePlayerGeneralState(cid);

            bool isPlayer = cid == player.Character.Cid;

            // 파워업 상태
            if (isPlayer)
            {
                isPowerUp = true;
            }

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte POWER_UP_STATE = (byte)PlayerState.PowerUp;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, POWER_UP_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 파워업: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.EffectPlayer.ShowPowerUpEffect();
        }

        /// <summary>
        /// 플레이어 파워업 상태 종료
        /// </summary>
        private void UpdatePlayerPowerUpEnd(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;

            // 파워업 상태 종료
            if (isPlayer)
            {
                isPowerUp = false;
            }

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte IDLE_STATE = (byte)PlayerState.General;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, IDLE_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 파워업 종료: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.EffectPlayer.ReleasePowerUpEffect();
        }

        /// <summary>
        /// 몬스터 상태 - 일반
        /// </summary>
        private void UpdateMonsterGeneralState(int monsterIndex)
        {
            MonsterBotEntity find = monsterBotPool.Find(monsterIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueState(monsterIndex, (byte)MonsterBotState.General))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터가 상태변경: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.AI.ChangeState(AI.Transition.Finished);
        }

        /// <summary>
        /// 몬스터 죽음
        /// </summary>
        private void UpdateMonsterDie(int monsterIndex)
        {
            MonsterBotEntity find = monsterBotPool.Find(monsterIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueState(monsterIndex, (byte)MonsterBotState.Die))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터가 충돌: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.AI.ChangeState(AI.Transition.Dead);
        }

        /// <summary>
        /// 몬스터 움직임
        /// </summary>
        private void UpdateMonsterMove(int monsterIndex, Vector3 prePos, Vector3 targetPos)
        {
            MonsterBotEntity find = monsterBotPool.Find(monsterIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueMove(monsterIndex, targetPos))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터가 움직임: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            if (find.MoveSpeed.HasValue)
            {
                const float CHECK_MIN = MONSTER_CHANGE_SPEED_MIN_DISTANCE * MONSTER_CHANGE_SPEED_MIN_DISTANCE;
                const float CHECK_MAX = MONSTER_CHANGE_SPEED_MAX_DISTANCE * MONSTER_CHANGE_SPEED_MAX_DISTANCE;

                Vector3 pos = unitActor.CachedTransform.position;
                Vector3 dist = pos - prePos;
                float sqrDistance = dist.sqrMagnitude;

                float rate;
                if (sqrDistance < CHECK_MIN)
                {
                    rate = 1f; // 클라이언트와 서버가 크게 차이가 나지 않음
                }
                else if (sqrDistance < CHECK_MAX)
                {
                    float clientDistance = (targetPos - pos).magnitude;
                    float serverDistance = (targetPos - prePos).magnitude;
                    rate = clientDistance / serverDistance; // 약간의 스피드 조정
                }
                else
                {
                    rate = 1f;
                    unitActor.Movement.ForceWarp(prePos); // 너무 멀리 떨어져 있으므로, 몬스터 강제 Warp
                }

                float speed = find.MoveSpeed.Value;
                unitActor.Movement.SetDefaultSpeed(speed * rate);
            }
            unitActor.Movement.ForceSetDestination(targetPos, useRemainThreshold: false);
            unitActor.Animator.PlayRun();
        }

        /// <summary>
        /// 몬스터 워프
        /// </summary>
        private void UpdateMonsterWarp(int monsterIndex, Vector3 targetPos)
        {
            MonsterBotEntity find = monsterBotPool.Find(monsterIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueMove(monsterIndex, targetPos))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터가 워프함: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.Movement.Warp(targetPos);
        }

        /// <summary>
        /// 몬스터 체력
        /// </summary>
        private void UpdateMonsterHp(int monsterIndex, int remainHp)
        {
            MonsterBotEntity find = monsterBotPool.Find(monsterIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueHp(monsterIndex, remainHp))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터가 Hp변경: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            find.SetCurrentHp(remainHp); // Hp 업데이트
        }

        /// <summary>
        /// 눈덩이 획득
        /// </summary>
        private void UpdateMazeSnowballGained(int cid, int itemIndex)
        {
            bool isPlayer = cid == player.Character.Cid;
            MazeCubeStatePool.Info find = mazeSnowballPool.Find(itemIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeSnowballPool.HasQueue() && mazeSnowballPool.UpdateQueueState(itemIndex, MazeCubeState.StandByRespawn))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 눈덩이를 획득함: {nameof(cid)} = {cid}, {nameof(itemIndex)} = {itemIndex}");
#endif
                return;
            }

            if (isPlayer)
            {
                find.StartEffect(); // 내가 먹은 효과
            }
            else
            {
                find.ReleaseMazeDropItem(); // 상대방이 먹었을 때
            }
        }

        /// <summary>
        /// 루돌프 획득
        /// </summary>
        private void UpdateMazeRudolphGained(int cid, int itemIndex)
        {
            bool isPlayer = cid == player.Character.Cid;
            MazeCubeStatePool.Info find = mazeRudolphPool.Find(itemIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeRudolphPool.HasQueue() && mazeRudolphPool.UpdateQueueState(itemIndex, MazeCubeState.StandByRespawn))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 루돌프를 획득함: {nameof(cid)} = {cid}, {nameof(itemIndex)} = {itemIndex}");
#endif
                return;
            }

            if (isPlayer)
            {
                find.StartEffect(); // 내가 먹은 효과
            }
            else
            {
                find.ReleaseMazeDropItem(); // 상대방이 먹었을 때
            }
        }

        /// <summary>
        /// 강탈포션 획득
        /// </summary>
        private void UpdateMazePowerUpPotionGained(int cid, int itemIndex)
        {
            bool isPlayer = cid == player.Character.Cid;
            MazeCubeStatePool.Info find = mazePowerUpPotionPool.Find(itemIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazePowerUpPotionPool.HasQueue() && mazePowerUpPotionPool.UpdateQueueState(itemIndex, MazeCubeState.StandByRespawn))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 강탈포션을 획득함: {nameof(cid)} = {cid}, {nameof(itemIndex)} = {itemIndex}");
#endif
                return;
            }

            if (isPlayer)
            {
                find.StartEffect(); // 내가 먹은 효과
            }
            else
            {
                find.ReleaseMazeDropItem(); // 상대방이 먹었을 때
            }
        }

        /// <summary>
        /// 플레이어 눈덩이 개수
        /// </summary>
        private void UpdateSnowballCount(int cid, int count)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어의 눈덩이 수 변경: {nameof(cid)} = {cid}");
#endif
                return;
            }

            if (isPlayer)
            {
                snowballCount = count;
                uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Snowball, snowballCount);
            }
        }

        /// <summary>
        /// 눈덩이 강탈 처리
        /// </summary>
        private void UpdateSnowballTake(int targetCid, int attackerCid)
        {
            bool isShowEffect = false;

            if (targetCid == player.Character.Cid)
            {
                isShowEffect = true;

                string message = LocalizeKey._90271.ToText() // {NAME}님에 의해 눈덩이를 빼앗겼습니다.
                    .Replace(ReplaceKey.NAME, GetUserName(attackerCid));
                ShowToastPopup(message);
            }
            else if (attackerCid == player.Character.Cid)
            {
                isShowEffect = true;

                string message = LocalizeKey._90274.ToText() // {NAME}님의 눈덩이를 빼앗았습니다.
                    .Replace(ReplaceKey.NAME, GetUserName(targetCid));
                ShowToastPopup(message);
            }

            if (isShowEffect)
            {
                IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(player.LastPosition, MazeRewardType.Snowball);
                mazeDropItem.Set(pointId: default, MazeRewardType.Snowball, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Snowball));
                AddPoolObject(mazeDropItem as PoolObject);
                mazeDropItem.StartEffect();
            }
        }

        /// <summary>
        /// 유저 이름 반환
        /// </summary>
        private string GetUserName(int cid, string defaultName = "nameless")
        {
            bool isPlayer = cid == player.Character.Cid;

            if (isPlayer)
                return player.GetName();

            PlayerBotEntity find = playerBotPool.Find(cid);

            if (find == null)
                return playerBotPool.GetUserNameFromQueue(cid, defaultName);

            return find.GetName();
        }

        /// <summary>
        /// 플레이어 루돌프 개수
        /// </summary>
        private void UpdateRudolphCount(int cid, int count)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어의 눈덩이 수 변경: {nameof(cid)} = {cid}");
#endif
                return;
            }

            if (isPlayer)
            {
                rudolphCount = count;
                uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Rudolph, rudolphCount);
            }
        }

        /// <summary>
        /// 눈덩이 소환
        /// </summary>
        private void UpdateMazeSnowball(int mazeCubeIndex)
        {
            MazeCubeStatePool.Info info = mazeSnowballPool.Find(mazeCubeIndex);
            if (info == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeSnowballPool.HasQueue() && mazeSnowballPool.UpdateQueueState(mazeCubeIndex, MazeCubeState.General))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 눈덩이 리젠됨: {nameof(mazeCubeIndex)} = {mazeCubeIndex}");
#endif
                return;
            }

            SpawnMazeSnowball(info);
        }

        /// <summary>
        /// 루돌프 소환
        /// </summary>
        private void UpdateMazeRudolph(int mazeCubeIndex)
        {
            MazeCubeStatePool.Info info = mazeRudolphPool.Find(mazeCubeIndex);
            if (info == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeRudolphPool.HasQueue() && mazeRudolphPool.UpdateQueueState(mazeCubeIndex, MazeCubeState.General))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 루돌프 리젠됨: {nameof(mazeCubeIndex)} = {mazeCubeIndex}");
#endif
                return;
            }

            SpawnMazeRudolph(info);
        }

        /// <summary>
        /// 강탈물약 소환
        /// </summary>
        private void UpdateMazePowerUpPotion(int mazeCubeIndex)
        {
            MazeCubeStatePool.Info info = mazePowerUpPotionPool.Find(mazeCubeIndex);
            if (info == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazePowerUpPotionPool.HasQueue() && mazePowerUpPotionPool.UpdateQueueState(mazeCubeIndex, MazeCubeState.General))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 강탈물약 리젠됨: {nameof(mazeCubeIndex)} = {mazeCubeIndex}");
#endif
                return;
            }

            SpawnMazePowerUpPotion(info);
        }

        /// <summary>
        /// Toast 팝업 띄우기
        /// </summary>
        private void ShowToastPopup(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            UI.ShowToastPopup(message);
        }

        private void AddPoolObject(PoolObject obj)
        {
            obj.OnRelease += RemovePoolObject;
            poolObjectList.Add(obj);
        }

        private void RemovePoolObject(PoolObject obj)
        {
            poolObjectList.Remove(obj);
            obj.OnRelease -= RemovePoolObject;
        }

        /// <summary>
        /// 결과창 띄우기
        /// </summary>
        private void ShowResultUI()
        {
            var buffer = rewardBuffer.GetBuffer(isAutoRelease: true);
            string message = buffer.Length == 0
                ? LocalizeKey._90137.ToText() // 획득한 보상이 없습니다.
                : LocalizeKey._90272.ToText(); // 다음 보상을 획득하였습니다.
            uiResultDungeon.Show(buffer, DungeonType.EventMultiMaze, false, UIResultDungeon.TitleType.Result, message);
        }
    }
}