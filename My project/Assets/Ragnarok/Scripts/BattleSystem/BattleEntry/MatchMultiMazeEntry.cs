//#define LIMIT_MAZE_CUBE

using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MatchMultiMazeEntry : BattleEntry
    {
        private enum HpUpdateResult
        {
            None,
            PlayerDamaged,
            PlayerRecovery,
            OtherDamaged,
            OtherRecovery,
        }

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
        /// 행동불능 상태 시간: 7초
        /// </summary>
        private const float GROGGY_TIME = 7f * 1000;

        /// <summary>
        /// 기본 속도 (이속, 공속)
        /// </summary>
        private const int DEFAULT_SPEED = 10000;

        /// <summary>
        /// 기본 카메라 뷰
        /// </summary>
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;
        private const CameraController.View ZOOM_VIEW = CameraController.View.Quater40_10;
        private const float CAMERA_NOISE_FOCUS_ENEMY = 2f;

        private const string TAG_OPPOSITE_CONTROLLER = nameof(YieldEffectOppositeController);
        private const string TAG_CAMERA_ZOOM_IN = nameof(YieldEffectCameraZoomIn);

        /******************** Models ********************/
        private readonly CharacterModel characterModel;
        private readonly DungeonModel dungeonModel;

        /******************** Repositories ********************/
        private readonly MultiMazeDataManager multiMazeDataRepo;

        /******************** Scene Compositions ********************/
        private readonly IBattlePool battlePool;
        private readonly List<PoolObject> poolObjectList;
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly MonsterBotEntityPool monsterBotPool;
        private readonly MazeCubeStatePool mazeCubePool;
        private readonly MazeCubeStatePool mazeItemPool;
        private readonly float bombCameraZoomTime;
        private readonly float bombCameraControlTime;
        private readonly Dictionary<int, MatchMazeMonsterPacket.MatchMazeMonsterType> monsterTypeDic;
        private readonly Buffer<CharacterEntity> agentBuffer;

        /******************** UIs ********************/
        private UIController uiController;
        private UIBattleMenu uiBattleMenu;
        private UIResultClear uiResultClear;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMainTop uiMainTop;
        private UIMvpTimer uiMvpTimer;
        private UIBattleInfo uiBattleInfo;
        private UIBattleProgress uiBattleProgress;
        private UICamSelect uiCamSelect;
        private UIMatchMultiMazeStatus uiMatchMultiMazeStatus;
        private UIStartingPistol uiStartingPistol;

        /******************** Temp Data ********************/
        private MultiMazeData currentData;
        private int channelId;
        private Vector3 savedPlayerPosition;
        private int mapId;
        private int maxZenyCount;
        private int maxItemCount;
        private int questCoinCount; // 내가 먹은 퀘스트코인 수
        private int remainQuestCoinCount;
        private bool isOppositeController;
        private bool isMonsterSpeedDown;
        private float characterSpeed;
        private RemainTime remainTime;
        private RemainTime countdownTime;
        private bool isExitBattle;
        private bool isTryExitMultiMaze;
        private int maxQuestCoinCount;

        public MatchMultiMazeEntry() : base(BattleMode.MatchMultiMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            characterModel = player.Character;
            dungeonModel = player.Dungeon;

            multiMazeDataRepo = MultiMazeDataManager.Instance;

            battlePool = BattlePoolManager.Instance;
            poolObjectList = new List<PoolObject>();
            playerBotPool = new PlayerBotEntityPool();
            monsterBotPool = new MonsterBotEntityPool();
            mazeCubePool = new MazeCubeStatePool();
            mazeItemPool = new MazeCubeStatePool();
            monsterTypeDic = new Dictionary<int, MatchMazeMonsterPacket.MatchMazeMonsterType>(IntEqualityComparer.Default);
            agentBuffer = new Buffer<CharacterEntity>();

            bombCameraZoomTime = BasisType.BOMB_MAZE_TIME.GetInt() * 0.001f;
            bombCameraControlTime = BasisType.BOMB_MAZE_TIME.GetInt() * 0.001f;
        }

        /// <summary>
        /// 퇴장 처리
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            Task<bool> task = RequestExit();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            IsSuccessExit = task.Result;
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
                item.ResetExtraOption(ExtraBattleOptionType.MaxHp);
                Dispose(item);
            }

            // 몬스터 봇 폐기
            foreach (var item in monsterBotPool)
            {
                Dispose(item);
            }

            playerBotPool.Clear();
            monsterBotPool.Clear();
            mazeCubePool.Clear();
            mazeItemPool.Clear();
            monsterTypeDic.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.MaxHp); // 강제 Hp 제거
            player.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            player.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            Timing.KillCoroutines(TAG_OPPOSITE_CONTROLLER); // 코루틴 종료
            Timing.KillCoroutines(TAG_CAMERA_ZOOM_IN); // 코루틴 종료
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();
            Protocol.REQUEST_MULMAZE_ROOM_TRANSFORM.AddEvent(OnRequestMultiMazeRoomTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_EXIT.AddEvent(OnReceiveMultiMazeRoomExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.AddEvent(OnReceiveMultiMazeRoomMonsterMove);
            Protocol.RECEIVE_MULMAZE_NOMALMON_CRASH.AddEvent(OnReceiveMultiMazeNormalMonsterCrash);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.AddEvent(OnReceiveMultiMazeMonsterRegen);
            Protocol.RECEIVE_MULMAZE_GET_QUESTCOIN.AddEvent(OnReceiveMultiMazeGetMazeCube);
            Protocol.RECEIVE_MULMAZE_QUESTCOIN_REGEN.AddEvent(OnReceiveMultiMazeMazeCubeRegen);
            Protocol.REQUEST_MULMAZE_GET_ITEM.AddEvent(OnRequestMultiMazeGetItem);
            Protocol.RECEIVE_MULMAZE_GET_ITEM.AddEvent(OnReceiveMultiMazeGetItem);
            Protocol.RECEIVE_MULMAZE_USERDIE.AddEvent(OnReceiveMultiMazeUserDie);
            Protocol.REQUEST_MULMAZE_TABFREEZE.AddEvent(OnRequestMultiMazeTabFreeze);
            Protocol.RECEIVE_MULMAZE_FREEZEEND.AddEvent(OnReceiveMultiMazeFreezeEnd);
            Protocol.RECEIVE_MULMAZE_FREEZESTART.AddEvent(OnReceiveMultiMazeFreezeStart);
            Protocol.RECEIVE_MULMAZE_TOUCH_ITEM.AddEvent(OnReceiveMultiMazeGetMazeItem);
            Protocol.RECEIVE_MULMAZE_ITEM_REGEN.AddEvent(OnReceiveMultiMazeMazeItemRegen);
            Protocol.RECEIVE_MATCHMULMAZE_BOSSBATTLE_START.AddEvent(OnReceiveMatchMultuMazeBossBattleStart);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();
            Protocol.REQUEST_MULMAZE_ROOM_TRANSFORM.RemoveEvent(OnRequestMultiMazeRoomTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_EXIT.RemoveEvent(OnReceiveMultiMazeRoomExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.RemoveEvent(OnReceiveMultiMazeRoomMonsterMove);
            Protocol.RECEIVE_MULMAZE_NOMALMON_CRASH.RemoveEvent(OnReceiveMultiMazeNormalMonsterCrash);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.RemoveEvent(OnReceiveMultiMazeMonsterRegen);
            Protocol.RECEIVE_MULMAZE_GET_QUESTCOIN.RemoveEvent(OnReceiveMultiMazeGetMazeCube);
            Protocol.RECEIVE_MULMAZE_QUESTCOIN_REGEN.RemoveEvent(OnReceiveMultiMazeMazeCubeRegen);
            Protocol.REQUEST_MULMAZE_GET_ITEM.RemoveEvent(OnRequestMultiMazeGetItem);
            Protocol.RECEIVE_MULMAZE_GET_ITEM.RemoveEvent(OnReceiveMultiMazeGetItem);
            Protocol.RECEIVE_MULMAZE_USERDIE.RemoveEvent(OnReceiveMultiMazeUserDie);
            Protocol.REQUEST_MULMAZE_TABFREEZE.RemoveEvent(OnRequestMultiMazeTabFreeze);
            Protocol.RECEIVE_MULMAZE_FREEZEEND.RemoveEvent(OnReceiveMultiMazeFreezeEnd);
            Protocol.RECEIVE_MULMAZE_FREEZESTART.RemoveEvent(OnReceiveMultiMazeFreezeStart);
            Protocol.RECEIVE_MULMAZE_TOUCH_ITEM.RemoveEvent(OnReceiveMultiMazeGetMazeItem);
            Protocol.RECEIVE_MULMAZE_ITEM_REGEN.RemoveEvent(OnReceiveMultiMazeMazeItemRegen);
            Protocol.RECEIVE_MATCHMULMAZE_BOSSBATTLE_START.RemoveEvent(OnReceiveMatchMultuMazeBossBattleStart);
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
            uiResultClear = UI.Show<UIResultClear>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiMainTop = UI.Show<UIMainTop>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattleProgress = UI.Show<UIBattleProgress>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiMatchMultiMazeStatus = UI.Show<UIMatchMultiMazeStatus>();
            uiStartingPistol = UI.Show<UIStartingPistol>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnBoss += OnBattleBoss;
            uiBattleMenu.OnExit += GiveUp;
            uiResultClear.OnRetryDungeon += ExitMultiMaze;
            uiResultClear.OnFinishDungeon += ExitMultiMaze;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;
            uiStartingPistol.OnFinish += StartMvpTimer;

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit, UIBattleMenu.MenuContent.Boss); // 나가기, 보스
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiMvpTimer.Hide();
            uiResultClear.Hide();

            cameraController.SetView(DEFAULT_VIEW);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            cameraController.SetView(DEFAULT_VIEW);

            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnBoss -= OnBattleBoss;
            uiBattleMenu.OnExit -= GiveUp;
            uiResultClear.OnRetryDungeon -= ExitMultiMaze;
            uiResultClear.OnFinishDungeon -= ExitMultiMaze;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;
            uiStartingPistol.OnFinish -= StartMvpTimer;

            UI.Close<UIController>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMainTop>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattleProgress>();
            UI.Close<UICamSelect>();
            UI.Close<UIMatchMultiMazeStatus>();
            UI.Close<UIStartingPistol>();

            uiController = null;
            uiBattleMenu = null;
            uiResultClear = null;
            uiBattlePlayerStatus = null;
            uiMainTop = null;
            uiMvpTimer = null;
            uiBattleInfo = null;
            uiBattleProgress = null;
            uiCamSelect = null;
            uiMatchMultiMazeStatus = null;
            uiStartingPistol = null;
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

            Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.MultiMaze.ToIntValue()); // 특정던전 입장 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_ENTER_COUNT, currentData.chapter); // 특정 멀티 미로 도전 횟수
            dungeonModel.SetLastEnterMultiMazeLobbyId(currentData.chapter); // 마지막 입장한 Lobby도 변경해 줌

            const byte MONSTER_SPEED_DOWN = 1; // 몬스터 디버프 상태 (몬스터 스피드 70%로 변경)

            int channelId = packet.channelId;
            MultiMazePlayerPacket[] arrMultiMazePlayerPacket = packet.arrMultiMazePlayerPacket;
            MatchMazeMonsterPacket[] arrMazeMonsterPacket = packet.arrMazeMonsterPacket;
            int mapId = packet.mapId;
            byte monsterSpeedType = packet.monsterSpeedType;
            MazeCubePacket[] arrMazeCubePacket = packet.arrMazeCubePacket;
            MazeCubePacket[] arrMazeItemPacket = packet.arrMazeItemPacket;

            // 추가 데이터 세팅
            float normalMonsterSpeed = currentData.GetNormalMonsterSpeed();
            foreach (var item in arrMazeMonsterPacket)
            {
                MonsterType type = item.monsterType;
                int level = currentData.GetLevel(type);
                float scale = currentData.GetScale(type);
                item.SetLevel(level);
                item.SetScale(scale);

                monsterTypeDic.Add(item.index, item.matchMazeMonsterType);
            }

            this.channelId = channelId;
            playerBotPool.EnqueueRange(arrMultiMazePlayerPacket);
            monsterBotPool.EnqueueRange(arrMazeMonsterPacket);
            this.mapId = mapId;
            mazeCubePool.EnqueueRange(arrMazeCubePacket);
            mazeItemPool.EnqueueRange(arrMazeItemPacket);
            maxZenyCount = currentData.zeny_max_count;
            maxItemCount = currentData.radom_item_max_count;
            maxQuestCoinCount = currentData.GetMaxQuestCoinCount();
            remainTime = packet.remainTime;
            countdownTime = packet.countdownTime;
            UpdateMonsterMazeSpeedDown(monsterSpeedType == MONSTER_SPEED_DOWN);
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

        protected override void OnReady()
        {
            base.OnReady();

            characterSpeed = currentData.GetCharacterSpeed();

            uiBattlePlayerStatus.SetPlayer(player);
            SetRemainQuestCount(maxQuestCoinCount);

            player.SetCurrentMp(player.MaxMp);

            questCoinCount = 0;
            uiBattleMenu.UpdateBossButtonSprite(questCoinCount);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnCharacters(), TAG);

            MatchMultiBossMazeEntry.SetPlayerHp(player.MaxHP);
            CharacterEntity[] players = agentBuffer.GetBuffer(isAutoRelease: true);
            uiMatchMultiMazeStatus.SetPlayers(players);

            yield return Timing.WaitUntilDone(YieldSpawnMonsterBots(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMazeCube(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMazeItem(), TAG);
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
                    player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
                    player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

                    // 플레이어
                    if (impl.Add(player, isEnemy: false))
                    {
                        UnitActor unitActor = player.SpawnActor(); // 유닛 소환
                        unitActor.Movement.SetDefaultSpeed(characterSpeed);
                        unitActor.Radar.SetSenseDistance(2f);
                        unitActor.Radar.SetSenseTargetType(TargetType.EnemyCharacter);

                        Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);
                        savedPlayerPosition = FindClosestEdge(pos); // 씬 로드 후에 네비 위치로 변경
                        unitActor.AI.SetHomePosition(savedPlayerPosition, isWarp: false); // Home 세팅
                        unitActor.AI.ChangeRandomDefenselessState(); // 랜덤 그로기
                        unitActor.Movement.ForceWarp(savedPlayerPosition); // Home으로 강제 워프

                        cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅

                        if (input.HasMaxHp)
                            player.SetExtraOption(ExtraBattleOptionType.MaxHp, input.MaxHp);

                        player.ReadyToBattle();

                        agentBuffer.Add(player);
                        yield return Timing.WaitForOneFrame;
                    }
                }
                else
                {
                    // 플레이어 봇
                    PlayerBotEntity entity = playerBotPool.Create(input);
                    entity.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
                    entity.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

                    if (impl.Add(entity, isEnemy: true))
                    {
                        UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
                        unitActor.Movement.SetDefaultSpeed(characterSpeed);
                        Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);

                        Vector3 position = FindClosestEdge(pos);
                        unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
                        unitActor.AI.ChangeRandomDefenselessState(); // 랜덤 그로기
                        unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

                        PlayerBotState state = input.State.ToEnum<PlayerBotState>();
                        // 상태 변경
                        switch (state)
                        {
                            case PlayerBotState.BattleBoss:
                                unitActor.AI.ChangeState(AI.Transition.Match); // 전투매칭상태
                                break;

                            case PlayerBotState.Defenseless:
                                unitActor.AI.ChangeState(AI.Transition.Groggy); // 그로기상태
                                break;

                            case PlayerBotState.Dead:
                                unitActor.AI.ChangeState(AI.Transition.Dead); // 죽은상태
                                break;
                        }

                        if (input.HasMaxHp)
                            entity.SetExtraOption(ExtraBattleOptionType.MaxHp, input.MaxHp);

                        entity.ReadyToBattle();

                        agentBuffer.Add(entity);
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

                entity.SetForceStatus(isMonsterSpeedDown ? ForceStatusType.MazeMoveSpdDownOn : ForceStatusType.MazeMoveSpdDownOff);

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
        /// 큐브 조각 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMazeCube()
        {
            while (mazeCubePool.HasQueue())
            {
                IMazeCubeStateInfo input = mazeCubePool.Dequeue();
                MazeCubeStatePool.Info info = mazeCubePool.Create(input);
                info.serverIndex = input.Index;
                info.state = input.State;
                info.position = FindClosestEdge(new Vector3(input.PosX, input.PosY, input.PosZ));

                switch (info.state)
                {
                    case MazeCubeState.General:
                        if (SpawnMazeCube(info))
                            yield return Timing.WaitForOneFrame;
                        break;

                    case MazeCubeState.StandByRespawn:
                        break;
                }
            }
        }

        /// <summary>
        /// 랜덤 아이템 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMazeItem()
        {
            while (mazeItemPool.HasQueue())
            {
                IMazeCubeStateInfo input = mazeItemPool.Dequeue();
                MazeCubeStatePool.Info info = mazeItemPool.Create(input);
                info.serverIndex = input.Index;
                info.state = input.State;
                info.position = FindClosestEdge(new Vector3(input.PosX, input.PosY, input.PosZ));

                switch (info.state)
                {
                    case MazeCubeState.General:
                        if (SpawnMazeItem(info))
                            yield return Timing.WaitForOneFrame;
                        break;

                    case MazeCubeState.StandByRespawn:
                        break;
                }
            }
        }

        /// <summary>
        /// 큐브 조각 생성
        /// </summary>
        private bool SpawnMazeCube(MazeCubeStatePool.Info info)
        {
            IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(info.position, MazeRewardType.MultiMazeCube);
            mazeDropItem.Set(info.serverIndex, MazeRewardType.MultiMazeCube, uiMatchMultiMazeStatus.GetWidget(characterModel.Cid));
            info.SetMazeDropItem(mazeDropItem);
            return true;
        }

        /// <summary>
        /// 랜덤 아이템 생성
        /// </summary>
        private bool SpawnMazeItem(MazeCubeStatePool.Info info)
        {
            IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(info.position, MazeRewardType.RandomBox);
            mazeDropItem.Set(info.serverIndex, MazeRewardType.RandomBox, uiMatchMultiMazeStatus.GetWidget(characterModel.Cid));
            info.SetMazeDropItem(mazeDropItem);
            return true;
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            foreach (var item in unitList)
            {
                UnitActor unitActor = item.GetActor();
                if (unitActor == null)
                    continue;

                unitActor.AI.StopHpRegen();
            }

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

            ShowResultUI(UIResultClear.ResultType.Result);
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
        private void OnMazeDropItem(IMazeDropItem mazeDropItem)
        {
            if (mazeDropItem == null)
                return;

            switch (mazeDropItem.RewardType)
            {
                case MazeRewardType.RandomBox:
                    PlaySfx(Constants.SFX.Battle.SPEED_POTION);
                    SendGetMazeItem(mazeDropItem);
                    break;

                case MazeRewardType.MultiMazeCube:
                    PlaySfx(Constants.SFX.Battle.QUEST_COIN);
#if LIMIT_MAZE_CUBE
                    // 큐브 조각이 부족한 상태일 경우에만
                    if (mazeCubeCount < maxMazeCubeCount)
                    {
                        SendGetMazeCubeItem(mazeDropItem);
                    }
                    else
                    {
                        ShowToastPopup(LocalizeKey._90129.ToText()); // 큐브 조각을 전부 획득했습니다.\n보스와 전투가 가능합니다
                    }
#else
                    SendGetMazeCubeItem(mazeDropItem);
#endif
                    break;
            }
        }

        private void OnTarget(UnitActor unitActor)
        {
            if (unitActor == null)
                return;

            if (unitActor.AI.CurrentState != AI.StateID.Defenseless)
                return;

            if (unitActor.Entity is PlayerBotEntity playerBotEntity)
            {
                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", playerBotEntity.Character.Cid);
                Protocol.REQUEST_MULMAZE_FREEZETAG.SendAsync(sfs).WrapNetworkErrors();
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
            const byte KILLED_BY_BOSS_CRASH = 2; // 보스충돌로 인해 죽음 (본인 포함)
            const byte KILLED_BY_LOW_HP = 3; // HP부족으로 인해 죽음 (본인 포함)
            const byte LOSE_BOSS_BATTLE = 4; // 보스전투패배로 인해 죽음 (본인이 포함되긴 하지만, MultiMazeEntry에서는 only 멀티플레이어)
            const byte WIN_BOSS_BATTLE = 5; // 보스전투승리로 인해 나감 (본인이 포함되긴 하지만, MultiMazeEntry에서는 only 멀티플레이어)
            const byte PLAYER_OUT_SYSTEM = 6; // 플레이어 죽음: 단순 시스템 띄우기

            int cid = response.GetInt("1");
            byte exitType = response.GetByte("2");

            switch (exitType)
            {
                case USER_EXIT: // 단순 나감 (타 플레이어)
                    uiMatchMultiMazeStatus.SetState(cid, View.UIMatchPlayerSlot.State.Failed);
                    UpdatePlayerExit(cid); // 플레이어 퇴장
                    break;

                case KILLED_BY_BOSS_CRASH: // 보스충돌로 인해 죽음
                    uiMatchMultiMazeStatus.SetState(cid, View.UIMatchPlayerSlot.State.Failed);
                    UpdatePlayerDie(cid); // 플레이어 사망
                    break;

                case KILLED_BY_LOW_HP: // HP부족으로 인해 죽음
                    uiMatchMultiMazeStatus.SetState(cid, View.UIMatchPlayerSlot.State.Failed);
                    UpdatePlayerDie(cid); // 플레이어 사망
                    break;

                case LOSE_BOSS_BATTLE: // 보스전투패배로 인해 죽음
                    uiMatchMultiMazeStatus.SetState(cid, View.UIMatchPlayerSlot.State.Failed);
                    UpdatePlayerDie(cid); // 플레이어 사망
                    break;

                case WIN_BOSS_BATTLE:
                    uiMatchMultiMazeStatus.SetState(cid, View.UIMatchPlayerSlot.State.Clear);
                    UpdatePlayerExit(cid); // 플레이어 퇴장
                    break;

                case PLAYER_OUT_SYSTEM:
                    uiMatchMultiMazeStatus.SetState(cid, View.UIMatchPlayerSlot.State.Failed);
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

        void OnReceiveMultiMazeNormalMonsterCrash(Response response)
        {
            int cid = response.GetInt("1");
            int? remainHp = null;
            int monsterIndex = response.GetByte("4");
            Vector3? warpPos = null;

            if (response.ContainsKey("3"))
                remainHp = response.GetInt("3");

            if (response.ContainsKey("5") && response.ContainsKey("6"))
            {
                float posX = response.GetFloat("5");
                float posZ = response.GetFloat("6");
                warpPos = new Vector3(posX, 0f, posZ);
            }

            if (remainHp.HasValue)
            {
                HpUpdateResult result = UpdatePlayerHp(cid, remainHp.Value); // 플레이어 체력

                if (result == HpUpdateResult.PlayerDamaged)
                    ShowToastPopup(LocalizeKey._90119.ToText()); // 몬스터와 충돌해 체력이 소폭 감소됩니다.
            }

            // 플레이어의 움직임 존재
            if (warpPos.HasValue)
            {
                UpdatePlayerWarp(cid, warpPos.Value); // 플레이어 워프
            }
            else // 플레이어의 움직임이 존재하지 않을 경우에는 몬스터가 죽음 처리
            {
                UpdateMonsterDie(monsterIndex); // 몬스터 죽음
            }
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

        void OnReceiveMatchMultuMazeBossBattleStart(Response response)
        {
            int cid = response.GetInt("1");

            bool isFinished = cid == -1; // 모두 20개를 먹었거나 타임 종료
            bool isPlayer = cid == player.Character.Cid;
            if (isFinished || isPlayer)
            {
                UI.HideIndicator(); /// <see cref="Protocol.REQUEST_MATCHMULMAZE_BOSSBATTLE_START"/>

                OnResetController();
                uiController.Hide();

                isExitBattle = true; // 서버에 Exit를 날리지 않아야 함
                impl.StartBattle(BattleMode.MatchMultiMazeBoss, currentData.id); // 매칭-멀티 보스 전투로 입장
                return;
            }

            UpdatePlayerMatchState(cid); // 플레이어 상태 - 전투
        }

        void OnReceiveMultiMazeGetMazeCube(Response response)
        {
            int cid = response.GetInt("1");
            int mazeCubeIndex = response.GetByte("2");
            int questCoinCount = response.GetInt("q");
            byte remainCoinCount = response.GetByte("r");

            UpdateMazeCubeGained(cid, mazeCubeIndex); // 큐브 조각 획득
            UpdateQuestCoinCount(cid, questCoinCount); // 획득한 코인 처리
            SetRemainQuestCount(remainCoinCount); // 남아있는 퀘스트코인 처리
        }

        void OnReceiveMultiMazeMazeCubeRegen(Response response)
        {
            int mazeCubeIndex = response.GetByte("1");
            UpdateMazeCubeRegen(mazeCubeIndex); // 큐브 조각 리젠
        }

        void OnReceiveMultiMazeGetMazeItem(Response response)
        {
            int cid = response.GetInt("1");
            int mazeItemIndex = response.GetByte("2");
            UpdateMazeItemGained(cid, mazeItemIndex); // 아이템 획득
        }

        void OnReceiveMultiMazeMazeItemRegen(Response response)
        {
            int mazeItemIndex = response.GetByte("1");
            UpdateMazeItemRegen(mazeItemIndex); // 아이템 리젠
        }

        void OnRequestMultiMazeGetItem(Response response)
        {
#if UNITY_EDITOR
            const int DAMAGED = 1;
            const int OPPOSITE_CONTROLLER = 2;
            const int CAMERA_ZOOM_IN = 3;
            const int DAMAGED_BY_OTHER = 4;
            const int OPPOSITE_CONTROLLER_BY_OTHER = 5;
            const int CAMERA_ZOOM_IN_BY_OTHER = 6;
            const int REGEN_HP = 7;
            const int MONSTER_SPEED_DOWN = 8;
#endif

            int? mazeCubeType = null;
            int? remainHp = null;
            int cid = player.Character.Cid;
            int mazeItemResultType = response.GetByte("4");

            if (response.ContainsKey("1"))
                mazeCubeType = response.GetInt("1");

            if (response.ContainsKey("3"))
                remainHp = response.GetInt("3");

            if (mazeCubeType.HasValue)
                UpdateMazeItemEffect(cid, mazeCubeType.Value); // 플레이어의 미로 아이템으로 인한 효과

            if (remainHp.HasValue)
            {
                HpUpdateResult result = UpdatePlayerHp(cid, remainHp.Value); // 플레이어 체력

                switch (result)
                {
                    case HpUpdateResult.PlayerDamaged:
                        ShowToastPopup(LocalizeKey._90120.ToText()); // 일정량의 체력이 감소됩니다.
                        break;

                    case HpUpdateResult.PlayerRecovery:
                        ShowToastPopup(LocalizeKey._90123.ToText()); // 일정량의 체력이 회복되었습니다.
                        break;
                }
            }

            UnitActor unitActor = player.GetActor();

            if (unitActor)
                unitActor.EffectPlayer.ShowMazeItemFinish(mazeItemResultType);
        }

        void OnReceiveMultiMazeGetItem(Response response)
        {
            int mazeCubeType = response.GetInt("1");
            int? remainHp = null;
            int cid = response.GetInt("4");

            if (response.ContainsKey("3"))
                remainHp = response.GetInt("3");

            UpdateMazeItemEffect(cid, mazeCubeType); // 플레이어의 미로 아이템으로 인한 효과

            if (remainHp.HasValue)
            {
                HpUpdateResult result = UpdatePlayerHp(cid, remainHp.Value); // 플레이어 체력

                switch (result)
                {
                    case HpUpdateResult.PlayerDamaged:
                        ShowToastPopup(LocalizeKey._90120.ToText()); // 일정량의 체력이 감소됩니다.
                        break;

                    case HpUpdateResult.PlayerRecovery:
                        ShowToastPopup(LocalizeKey._90123.ToText()); // 일정량의 체력이 회복되었습니다.
                        break;
                }
            }
        }

        void OnReceiveMultiMazeUserDie(Response response)
        {
            int cid = response.GetInt("1");
            int? monsterIndex = null; // 충돌한 몬스터 인덱스
            byte? monsterState = null; // 충돌한 몬스터 상태
            int? playerQuestCoin = null; // 충돌한 플레이의 퀘스트 코인 수
            byte? remainQuestCoin = null;

            if (response.ContainsKey("2"))
                monsterIndex = response.GetByte("2");

            if (response.ContainsKey("3"))
                monsterState = response.GetByte("3");

            if (response.ContainsKey("q"))
                playerQuestCoin = response.GetInt("q");

            if (response.ContainsKey("r"))
                remainQuestCoin = response.GetByte("r");

            UpdatePlayerDie(cid); // 플레이어 죽음

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

            if (playerQuestCoin.HasValue)
                UpdateQuestCoinCount(cid, playerQuestCoin.Value);

            if (remainQuestCoin.HasValue)
                SetRemainQuestCount(remainQuestCoin.Value);
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
            int? attackerCid = null; // 훔친 사람
            int? cubeCount = null; // 충돌당한 사람의 큐브조각 개수
            int? attackerCubeCount = null; // 훔친 사람의 큐브조각 개수
            int? monsterIndex = null; // 충돌한 몬스터 인덱스
            byte? monsterState = null; // 충돌한 몬스터 상태
            int? playerQuestCoin = null; // 충돌한 플레이의 퀘스트 코인 수
            byte? remainQuestCoin = null;

            if (response.ContainsKey("2"))
                attackerCid = response.GetInt("2");

            if (response.ContainsKey("3"))
                cubeCount = response.GetInt("3");

            if (response.ContainsKey("4"))
                attackerCubeCount = response.GetInt("4");

            if (response.ContainsKey("5"))
                monsterIndex = response.GetByte("5");

            if (response.ContainsKey("6"))
                monsterState = response.GetByte("6");

            if (response.ContainsKey("q"))
                playerQuestCoin = response.GetInt("q");

            if (response.ContainsKey("r"))
                remainQuestCoin = response.GetByte("r");

            UpdatePlayerGeneralState(cid); // 평상시 상태

            // 맞은 사람의 큐브 개수 세팅
            if (cubeCount.HasValue)
            {
                if (cid == player.Character.Cid)
                    player.GetActor()?.EffectPlayer.ShowSurprise();
                else
                    playerBotPool.Find(cid)?.GetActor()?.EffectPlayer.ShowSurprise();

                UpdateQuestCoinCount(cid, cubeCount.Value);
            }

            // 훔친 사람의 큐브 개수 세팅
            if (attackerCid.HasValue && attackerCubeCount.HasValue)
                UpdateQuestCoinCount(attackerCid.Value, attackerCubeCount.Value);

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

            if (playerQuestCoin.HasValue)
                UpdateQuestCoinCount(cid, playerQuestCoin.Value);

            if (remainQuestCoin.HasValue)
                SetRemainQuestCount(remainQuestCoin.Value);
        }

        void OnReceiveMultiMazeFreezeStart(Response response)
        {
            int cid = response.GetInt("1");
            int? remainHp = null;
            int monsterIndex = response.GetByte("4");
            byte? monsterState = null; // 충돌한 몬스터 상태
            int? playerQuestCoin = null; // 충돌한 플레이의 퀘스트 코인 수
            byte? remainQuestCoin = null; // 남은 퀘스트 코인 수

            if (response.ContainsKey("3"))
                remainHp = response.GetInt("3");

            if (response.ContainsKey("5"))
                monsterState = response.GetByte("5");

            if (response.ContainsKey("r"))
                remainQuestCoin = response.GetByte("r");

            if (response.ContainsKey("q"))
                playerQuestCoin = response.GetInt("q");

            if (remainHp.HasValue)
            {
                HpUpdateResult result = UpdatePlayerHp(cid, remainHp.Value); // 플레이어 체력

                if (result == HpUpdateResult.PlayerDamaged)
                    ShowToastPopup(LocalizeKey._90119.ToText()); // 몬스터와 충돌해 체력이 소폭 감소됩니다.
            }

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

            if (playerQuestCoin.HasValue)
                UpdateQuestCoinCount(cid, playerQuestCoin.Value);

            if (remainQuestCoin.HasValue)
                SetRemainQuestCount(remainQuestCoin.Value);
        }

        #endregion

        #region UI 이벤트

        void OnStartController()
        {
            if (player.IsDie)
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            // 행동 불능 상태
            if (unitActor.AI.CurrentState == AI.StateID.Defenseless)
            {
                // 카메라 흔들림
                unitActor.EffectPlayer.GenerateImpulse(BattlePoolManager.ImpulseType.ClickAttack);

                SendTabInDefenseless();
                return;
            }

            unitActor.Movement.Stop();
            unitActor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
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

            // 컨트롤러 방해
            if (isOppositeController)
                motion *= -1;

            unitActor.Movement.Move(motion);

            Vector3 dist = player.LastPosition - savedPlayerPosition;
            if (dist.sqrMagnitude < SEND_POSITION_SQR_MAGNITUDE)
                return;

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnResetController()
        {
            CameraUtils.Zoom(CameraZoomType.None);

            if (player.IsDie)
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            // 행동 불능 상태
            if (unitActor.AI.CurrentState == AI.StateID.Defenseless)
                return;

            unitActor.AI.SetInputMove(isControl: false);
            unitActor.Movement.Stop();

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnBattleBoss()
        {
            if (questCoinCount < Constants.Battle.MATCH_MULTI_NEED_TO_BOSS_BATTLE)
            {
                ShowToastPopup(LocalizeKey._49100.ToText());
                return;
            }

            // 강제 보스 도전
            Protocol.REQUEST_MATCHMULMAZE_BOSSBATTLE_START.SendAsync().WrapNetworkErrors();
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

        void OnFinishedRemainTimer()
        {
            uiMvpTimer.StopTimer();
            uiMvpTimer.Hide();
            UpdatePlayerDie(player.Character.Cid);
        }

        void StartMvpTimer()
        {
            uiMvpTimer.Initialize((long)remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();
        }

        #endregion

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
        /// 큐브조각 획득
        /// </summary>
        private void SendGetMazeCubeItem(IMazeDropItem mazeDropItem)
        {
            mazeDropItem.Hit();
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)mazeDropItem.PointId);
            Protocol.REQUEST_MULMAZE_GET_QUESTCOIN.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 아이템 획득
        /// </summary>
        private void SendGetMazeItem(IMazeDropItem mazeDropItem)
        {
            mazeDropItem.Hit();
            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)mazeDropItem.PointId);
            Protocol.REQUEST_MULMAZE_TOUCH_ITEM.SendAsync(sfs).WrapNetworkErrors();
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

            find.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            find.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            find.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            Dispose(find); // 제거
            playerBotPool.Recycle(find); // 회수
        }

        /// <summary>
        /// 플레이어 사망
        /// </summary>
        private void UpdatePlayerDie(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte DEAD_STATE = (byte)PlayerBotState.Dead;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, DEAD_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 죽음: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.Die(null); // 강제 사망처리

            if (isPlayer)
            {
                uiController.Hide();
            }
        }

        /// <summary>
        /// 플레이어 워프
        /// </summary>
        private void UpdatePlayerWarp(int cid, Vector3 targetPos)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueMove(cid, targetPos))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 워프: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.Movement.Warp(targetPos);
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
                const byte GENERAL_STATE = (byte)PlayerBotState.General;

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
        /// 플레이어 상태 - 전투
        /// </summary>
        private void UpdatePlayerMatchState(int cid)
        {
            UnitEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
                const byte BATTLE_BOSS_STATE = (byte)PlayerBotState.BattleBoss;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, BATTLE_BOSS_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 보스몬스터와 전투: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            uiMatchMultiMazeStatus.ShowBossBattle(cid); // 보스 전투 중
            unitActor.EffectPlayer.SpawnUnitTeleport();

            find.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            find.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            find.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
            Dispose(find); // 제거
            // playerBotPool.Recycle(find); // 회수는 하지 않는다. => 플레이어 전투 후에 퀘스트코인 수 업데이트가 날아오기 때문에 (UpdateMazeCubeGained, UpdateQuestCoinCount)
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
                const byte DEFENSELESS_STATE = (byte)PlayerBotState.Defenseless;

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
        /// 플레이어 체력
        /// </summary>
        private HpUpdateResult UpdatePlayerHp(int cid, int remainHp)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어의 체력 변경: {nameof(cid)} = {cid}");
#endif
                return HpUpdateResult.None;
            }

            if (find.CurHP == remainHp)
                return HpUpdateResult.None;

            bool isDamaged = remainHp < find.CurHP;

            find.SetCurrentHp(remainHp); // 플레이어 체력
            uiMatchMultiMazeStatus.UpdateHp(cid, remainHp, find.MaxHP);

            if (isPlayer)
            {
                MatchMultiBossMazeEntry.SetPlayerHp(remainHp); // 보스 전에서의 플레이어 Hp 세팅
                return isDamaged ? HpUpdateResult.PlayerDamaged : HpUpdateResult.PlayerRecovery;
            }

            return isDamaged ? HpUpdateResult.OtherDamaged : HpUpdateResult.OtherRecovery;
        }

        /// <summary>
        /// 플레이어 퀘스트코인 개수
        /// </summary>
        private void UpdateQuestCoinCount(int cid, int count)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어의 퀘스트코인 변경: {nameof(cid)} = {cid}");
#endif
                return;
            }

            uiMatchMultiMazeStatus.SetCoin(cid, count);

            if (isPlayer)
            {
                bool isDecrease = questCoinCount > count;
                questCoinCount = count;
                uiBattleMenu.UpdateBossButtonSprite(questCoinCount);
                if (isDecrease)
                {
                    IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(player.LastPosition, MazeRewardType.MultiMazeCube);
                    mazeDropItem.Set(pointId: default, MazeRewardType.MultiMazeCube, uiMatchMultiMazeStatus.GetWidGet());
                    AddPoolObject(mazeDropItem as PoolObject);
                    mazeDropItem.StartEffect();
                    UI.ShowToastPopup(LocalizeKey._90190.ToText()); // 코인을 빼앗겼습니다.
                }
            }
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

            //#if UNITY_EDITOR
            //            Debug.LogError($"몬스터 이동: [{monsterIndex}({pos.x}, {pos.z})] ({prePos.x}, {prePos.z}) => ({targetPos.x}, {targetPos.z})");
            //#endif
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
        /// 몬스터 이동속도 감소 여부
        /// </summary>
        private void UpdateMonsterMazeSpeedDown(bool isOn)
        {
            isMonsterSpeedDown = isOn;
            for (int i = 0; i < unitList.size; i++)
            {
                if (!unitList[i].IsEnemy)
                    continue;

                unitList[i].SetForceStatus(isMonsterSpeedDown ? ForceStatusType.MazeMoveSpdDownOn : ForceStatusType.MazeMoveSpdDownOff);
            }
        }

        /// <summary>
        /// 큐브 조각 획득
        /// </summary>
        private void UpdateMazeCubeGained(int cid, int mazeCubeIndex)
        {
            bool isPlayer = cid == player.Character.Cid;
            MazeCubeStatePool.Info find = mazeCubePool.Find(mazeCubeIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeCubePool.HasQueue() && mazeCubePool.UpdateQueueState(mazeCubeIndex, MazeCubeState.StandByRespawn))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 큐브조각을 획득함: {nameof(cid)} = {cid}, {nameof(mazeCubeIndex)} = {mazeCubeIndex}");
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
        /// 큐브 조각 소환
        /// </summary>
        private void UpdateMazeCubeRegen(int mazeCubeIndex)
        {
            MazeCubeStatePool.Info info = mazeCubePool.Find(mazeCubeIndex);
            if (info == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeCubePool.HasQueue() && mazeCubePool.UpdateQueueState(mazeCubeIndex, MazeCubeState.General))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 큐브조각이 리젠됨: {nameof(mazeCubeIndex)} = {mazeCubeIndex}");
#endif
                return;
            }

            SpawnMazeCube(info);
        }

        /// <summary>
        /// 아이템 획득
        /// </summary>
        private void UpdateMazeItemGained(int cid, int itemIndex)
        {
            bool isPlayer = cid == player.Character.Cid;
            MazeCubeStatePool.Info find = mazeItemPool.Find(itemIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeItemPool.HasQueue() && mazeItemPool.UpdateQueueState(itemIndex, MazeCubeState.StandByRespawn))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 아이템을 획득함: {nameof(cid)} = {cid}, {nameof(itemIndex)} = {itemIndex}");
#endif
                return;
            }

            if (isPlayer)
            {
                find.StartEffect(); // 내가 먹은 효과
                Timing.RunCoroutine(YieldItemGet(), TAG);
            }
            else
            {
                find.ReleaseMazeDropItem(); // 상대방이 먹었을 때
            }
        }

        /// <summary>
        /// 아이템 소환
        /// </summary>
        private void UpdateMazeItemRegen(int itemIndex)
        {
            MazeCubeStatePool.Info info = mazeItemPool.Find(itemIndex);
            if (info == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (mazeItemPool.HasQueue() && mazeItemPool.UpdateQueueState(itemIndex, MazeCubeState.General))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 아이템이 리젠됨: {nameof(itemIndex)} = {itemIndex}");
#endif
                return;
            }

            SpawnMazeItem(info);
        }

        /// <summary>
        /// 플레이어의 미로 아이템으로 인한 효과
        /// </summary>
        private void UpdateMazeItemEffect(int cid, int mazeCubeType)
        {
            const int DAMAGE = 0; // 대미지
            const int OPPOSITE_CONTROLLER = 1; // 컨트롤방해
            const int CAMERA_ZOOM_IN = 2; // 시야방해
            const int HP_REGEN = 3; // hp 증가
            const int MONSTER_DECRESE_SPEED = 4; // 몬스터 속도 감소
            const int MONSTER_NORMAL_SPEED = 5; // 몬스터 일반 속도로 복귀

            bool isPlayer = cid == player.Character.Cid;

            string message = null;
            if (isPlayer)
            {
                switch (mazeCubeType)
                {
                    case DAMAGE:
                        message = LocalizeKey._90120.ToText(); // 일정량의 체력이 감소됩니다.
                        break;

                    case OPPOSITE_CONTROLLER:
                        message = LocalizeKey._90121.ToText() // {SECONDS}초간 조작이 반대로 됩니다.
                            .Replace(ReplaceKey.SECONDS, MathUtils.RoundToInt(bombCameraControlTime));
                        break;

                    case CAMERA_ZOOM_IN:
                        message = LocalizeKey._90122.ToText() // {SECONDS}초간 시야가 제한됩니다.
                            .Replace(ReplaceKey.SECONDS, MathUtils.RoundToInt(bombCameraZoomTime));
                        break;

                    case HP_REGEN:
                        message = LocalizeKey._90123.ToText(); // 일정량의 체력이 회복되었습니다.
                        break;

                    case MONSTER_DECRESE_SPEED:
                        message = LocalizeKey._90124.ToText() // {NAME}님에 의해 몬스터들의 이동 속도가 느려집니다.
                            .Replace(ReplaceKey.NAME, GetUserName(cid));
                        break;
                }
            }
            else
            {
                switch (mazeCubeType)
                {
                    case DAMAGE:
                        message = LocalizeKey._90125.ToText() // {NAME}님에 의해 체력이 감소됩니다.
                            .Replace(ReplaceKey.NAME, GetUserName(cid));
                        break;

                    case OPPOSITE_CONTROLLER:
                        message = LocalizeKey._90126.ToText() // {NAME}님에 의해 {SECONDS}초간 조작이 반대로 됩니다.
                            .Replace(ReplaceKey.NAME, GetUserName(cid))
                            .Replace(ReplaceKey.SECONDS, MathUtils.RoundToInt(bombCameraControlTime));
                        break;

                    case CAMERA_ZOOM_IN:
                        message = LocalizeKey._90127.ToText() // {NAME}님에 의해 {SECONDS}초간 시야가 제한됩니다.
                            .Replace(ReplaceKey.NAME, GetUserName(cid))
                            .Replace(ReplaceKey.SECONDS, MathUtils.RoundToInt(bombCameraZoomTime));
                        break;

                    case MONSTER_DECRESE_SPEED:
                        message = LocalizeKey._90124.ToText() // {NAME}님에 의해 몬스터들의 이동 속도가 느려집니다.
                            .Replace(ReplaceKey.NAME, GetUserName(cid));
                        break;
                }
            }

            ShowToastPopup(message);

            switch (mazeCubeType)
            {
                case DAMAGE: // 따로 처리
                    break;

                case OPPOSITE_CONTROLLER:
                    Timing.KillCoroutines(TAG_OPPOSITE_CONTROLLER);
                    Timing.RunCoroutine(YieldEffectOppositeController(), TAG_OPPOSITE_CONTROLLER);
                    break;

                case CAMERA_ZOOM_IN:
                    Timing.KillCoroutines(TAG_CAMERA_ZOOM_IN);
                    Timing.RunCoroutine(YieldEffectCameraZoomIn(), TAG_CAMERA_ZOOM_IN);
                    break;

                case MONSTER_DECRESE_SPEED:
                    UpdateMonsterMazeSpeedDown(true);
                    break;

                case MONSTER_NORMAL_SPEED:
                    UpdateMonsterMazeSpeedDown(false);
                    break;
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
        /// Toast 팝업 띄우기
        /// </summary>
        private void ShowToastPopup(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            UI.ShowToastPopup(message);
        }

        private void SetRemainQuestCount(int remainQuestCoinCount)
        {
            this.remainQuestCoinCount = remainQuestCoinCount;
            uiMatchMultiMazeStatus.SetRemainCoin(remainQuestCoinCount, maxQuestCoinCount);
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

        IEnumerator<float> YieldEffectOppositeController()
        {
            isOppositeController = true;
            yield return Timing.WaitForSeconds(bombCameraControlTime);
            isOppositeController = false;
        }

        IEnumerator<float> YieldEffectCameraZoomIn()
        {
            cameraController.SetView(CameraController.View.RightAngle);
            yield return Timing.WaitForSeconds(bombCameraZoomTime);
            cameraController.SetView(DEFAULT_VIEW);
        }

        IEnumerator<float> YieldItemGet()
        {
            UnitActor unitActor = player.GetActor();

            if (unitActor)
                unitActor.EffectPlayer.ShowMazeItemLoop();

            yield return Timing.WaitForSeconds(1f);
            Protocol.REQUEST_MULMAZE_GET_ITEM.SendAsync().WrapNetworkErrors();
        }

        private UIWidget GetWidget(UIExtraBattlePlayerStatus.ExtraMode extraMode)
        {
            return uiBattlePlayerStatus.GetWidget(extraMode);
        }

        private UIWidget GetWidget(UIMainTop.MenuContent content)
        {
            return uiMainTop.GetWidget(content);
        }

        /// <summary>
        /// 결과창 띄우기
        /// </summary>
        private void ShowResultUI(UIResultClear.ResultType resultType)
        {
            ShowResultUIAsync(resultType).WrapNetworkErrors();
        }

        private async Task ShowResultUIAsync(UIResultClear.ResultType resultType)
        {
            var buffer = rewardBuffer.GetBuffer(isAutoRelease: true);

            // 카드 보상 있을 경우 특별 연출
            if (buffer.Length > 0)
            {
                // 카드 보상 찾기
                List<CardItemInfo.ICardInfoSimple> rewardList = new List<CardItemInfo.ICardInfoSimple>();
                foreach (var reward in buffer)
                {
                    if (reward.ItemGroupType != ItemGroupType.Card)
                        continue;

                    CardItemInfo param = new CardItemInfo();
                    param.SetData(reward.ItemData);
                    if (!param.IsInvalidData)
                        rewardList.Add(param);
                }

                // 연출 재생
                if (rewardList.Count > 0)
                {
                    UICardReward uiCardReward = UI.Show<UICardReward>();
                    uiCardReward.Show(rewardList.ToArray());
                    await Awaiters.Until(() => uiCardReward == null || !uiCardReward.IsVisible);
                }
            }

            uiResultClear.Show(buffer, DungeonType.MultiMaze, false, resultType, showRewardFX: true);
            uiResultClear.SetDescription(string.Empty);
        }



#if UNITY_EDITOR
        public void Test(int index)
        {
            switch (index)
            {
                case 0:
                    isExitBattle = true; // 서버에 Exit를 날리지 않아야 함
                    impl.StartBattle(BattleMode.MatchMultiMazeBoss, currentData.id); // 매칭-멀티 보스 전투로 입장
                    break;

                case 1:
                    player.SetForceStatus(ForceStatusType.MaxRegenOn);
                    break;

                case 2:
                    player.SetForceStatus(ForceStatusType.MaxRegenOff);
                    break;
            }
        }
#endif
    }
}