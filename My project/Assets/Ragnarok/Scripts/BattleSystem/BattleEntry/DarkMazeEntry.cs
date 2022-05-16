using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DarkMazeEntry : BattleEntry
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
        /// 기본 카메라 뷰
        /// </summary>
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;

        private enum PlayerState : byte
        {
            General = 1, // 일반
        }

        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly MultiMazeDataManager multiMazeDataRepo;

        // <!-- Scene Compositions --!>
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly MonsterBotEntityPool monsterBotPool;
        private readonly List<IMonsterBotInput> standbyMonsterBotInputList;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIController uiController;
        private UIBattleInfo uiBattleInfo;
        private UIBattleCountdown uiBattleCountdown;
        private UIResultDungeon uiResultDungeon;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMvpTimer uiMvpTimer;
        private UICamSelect uiCamSelect;
        private UIBattleFog uiBattleFog;
        private UIBattleDarkMaze uiBattleDarkMaze;
        private UIBossComing uiBossComing;

        // <!-- Temp Data --!>
        private bool isFinishedStartingPistol;
        private MultiMazeData currentData;
        private Vector3 savedPlayerPosition;
        private RemainTime remainTime;
        private RemainTime countdownTime;
        private bool isExitBattle;
        private float characterSpeed;
        private int monsterKillCount;
        private int maxMonsterKillCount;
        private bool isFinishedTimer;
        private bool isTryExitMultiMaze;
        private bool isShowReward;

        public DarkMazeEntry() : base(BattleMode.DarkMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            dungeonModel = player.Dungeon;

            multiMazeDataRepo = MultiMazeDataManager.Instance;

            playerBotPool = new PlayerBotEntityPool();
            monsterBotPool = new MonsterBotEntityPool();
            standbyMonsterBotInputList = new List<IMonsterBotInput>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

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
            standbyMonsterBotInputList.Clear();

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
            Protocol.RECEIVE_MULMAZE_MONKILL.AddEvent(OnReceiveMultiMazeMonsterKill);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();

            Protocol.REQUEST_MULMAZE_ROOM_TRANSFORM.RemoveEvent(OnRequestMultiMazeRoomTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_EXIT.RemoveEvent(OnReceiveMultiMazeRoomExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.RemoveEvent(OnReceiveMultiMazeRoomMonsterMove);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.RemoveEvent(OnReceiveMultiMazeMonsterRegen);
            Protocol.RECEIVE_MULMAZE_MONKILL.RemoveEvent(OnReceiveMultiMazeMonsterKill);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            dungeonModel.OnEventDarkMazeRewardEvent += OnEventDarkMazeRewardEvent;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            dungeonModel.OnEventDarkMazeRewardEvent -= OnEventDarkMazeRewardEvent;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiController = UI.Show<UIController>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattleCountdown = UI.Show<UIBattleCountdown>();
            uiResultDungeon = UI.Show<UIResultDungeon>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiBattleFog = UI.Show<UIBattleFog>();
            uiBattleDarkMaze = UI.Show<UIBattleDarkMaze>();
            uiBossComing = UI.Show<UIBossComing>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += GiveUp;
            uiBattleCountdown.OnFinish += OnFinishedCountdown;
            uiResultDungeon.OnFinishDungeon += ExitMultiMaze;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;

            cameraController.SetView(DEFAULT_VIEW); // 카메라 뷰 세팅

            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattleCountdown.Hide();
            uiResultDungeon.Hide();
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiMvpTimer.Hide();
            uiBattleDarkMaze.Hide();
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
            uiBattleCountdown.OnFinish -= OnFinishedCountdown;
            uiResultDungeon.OnFinishDungeon -= ExitMultiMaze;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIController>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattleCountdown>();
            UI.Close<UIResultDungeon>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMvpTimer>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleFog>();
            UI.Close<UIBattleDarkMaze>();
            UI.Close<UIBossComing>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiController = null;
            uiBattleInfo = null;
            uiBattleCountdown = null;
            uiResultDungeon = null;
            uiBattlePlayerStatus = null;
            uiMvpTimer = null;
            uiCamSelect = null;
            uiBattleFog = null;
            uiBattleDarkMaze = null;
            uiBossComing = null;
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
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            isFinishedStartingPistol = false;
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

            //Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.EventMultiMaze.ToIntValue()); // 특정던전 입장 횟수
            dungeonModel.SetLastEnterMultiMazeLobbyId(currentData.id); // 마지막 입장한 Lobby도 변경해 줌 (이벤트 던전의 경우에는 chapter 가 아니라 id로 처리)

            int channelId = packet.channelId;
            MultiMazePlayerPacket[] arrMultiMazePlayerPacket = packet.arrMultiMazePlayerPacket;
            MatchMazeMonsterPacket[] arrMazeMonsterPacket = packet.arrMazeMonsterPacket;
            int mapId = packet.mapId;

            // 추가 데이터 세팅
            float bossMonsterSpeed = currentData.GetBossMonsterSpeed();
            float normalMonsterSpeed = currentData.GetNormalMonsterSpeed();
            foreach (var item in arrMazeMonsterPacket)
            {
                MonsterType type = item.monsterType;
                int level = currentData.GetLevel(type);
                float scale = currentData.GetScale(type);
                item.SetLevel(level);
                item.SetScale(scale);
                item.SetMoveSpeed(type == MonsterType.Boss ? bossMonsterSpeed : normalMonsterSpeed);
            }

            playerBotPool.EnqueueRange(arrMultiMazePlayerPacket);
            monsterBotPool.EnqueueRange(arrMazeMonsterPacket);
            remainTime = packet.remainTime;
            countdownTime = packet.countdownTime;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

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
            maxMonsterKillCount = currentData.normal_monster_count;

            uiBattlePlayerStatus.SetPlayer(player);

            player.SetCurrentMp(player.MaxMp);

            monsterKillCount = 0;
            uiBattleDarkMaze.Show();
            uiBattleDarkMaze.Set(UIBattleDarkMaze.MonsterType.Lude, 0, maxMonsterKillCount);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnCharacters(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnMonsterBots(), TAG);
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

                // 아무상태도 아님 or 죽은상태
                MonsterBotState state = input.State.ToEnum<MonsterBotState>();
                if (state == MonsterBotState.None || state == MonsterBotState.Die)
                {
                    standbyMonsterBotInputList.Add(input); // 대기
                    continue;
                }

                if (!SpawnMonster(input))
                    continue;

                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 몬스터 봇 생성
        /// </summary>
        private bool SpawnMonster(IMonsterBotInput input)
        {
            if (input == null)
                return false;

            UnitEntity entity = monsterBotPool.Create(input);
            if (!impl.Add(entity, isEnemy: true))
                return false;

            entity.OnDespawnActor += OnDespawnMonsterActor;

            UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
            if (input.MoveSpeed.HasValue)
                unitActor.Movement.SetDefaultSpeed(input.MoveSpeed.Value);

            Vector3 pos = new Vector3(input.PosX, input.PosY, input.PosZ);
            unitActor.AI.SetHomePosition(pos, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(pos); // Home으로 강제 워프
            unitActor.AI.ReadyToBattle(); // 몬스터 봇의 hp를 맞춰주기 위해 ReadyToBattle 호출
            unitActor.AI.ChangeAutoDespawnDieState(); // 죽었을 때 자동 사라짐

            return true;
        }

        void OnDespawnMonsterActor(UnitActor actor)
        {
            actor.AI.ChangeDieEmptyState(); // 죽었을 때 아무것도 하지 않음
            actor.Entity.OnDespawnActor -= OnDespawnMonsterActor;
        }

        /// <summary>
        /// 씬 추가 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeSceneAfter()
        {
            uiBattleCountdown.Show(countdownTime); // 남은시간 카운트다운
            yield return Timing.WaitUntilTrue(IsFinishedStartingPistol);
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            uiMvpTimer.Initialize((long)remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();

            UnitActor actor = player.GetActor();
            if (actor)
            {
                actor.EffectPlayer.ShowUnitCircle();
            }
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

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
            float posX = response.GetFloat("3");
            float posZ = response.GetFloat("4");

            Vector3 targetPos = new Vector3(posX, 0f, posZ);
            UpdateMonsterGeneralState(monsterIndex); // 몬스터 상태 - 일반
            UpdateMonsterWarp(monsterIndex, targetPos); // 몬스터 워프

            // 안개 모두 걷힘 (몬스터가 리젠되는 경우는 보스가 있을 경우밖에 음슴)
            uiBattleFog.ClearFog();

            // 보스 컷씬
            uiBossComing.ShowBoss(currentData);
        }

        void OnReceiveMultiMazeMonsterKill(Response response)
        {
            int cid = response.GetInt("1"); // 플레이어는 변화 없음 (강탈물약 상태)
            int monsterIndex = response.GetByte("4");
            UpdateMonsterDie(monsterIndex);

            if (monsterKillCount >= maxMonsterKillCount)
                return;

            string userName = GetUserName(cid);
            string message = LocalizeKey._39710.ToText() // {NAME} 플레이어가 숨어있던 몬스터를 찾았다!
                .Replace(ReplaceKey.NAME, userName);
            ShowToastPopup(message);

            // 안개가 조금씩 걷힌다.
            uiBattleFog.PlusFog();

            // 포획 몬스터 증가
            ++monsterKillCount;
            uiBattleDarkMaze.Tween(monsterKillCount, maxMonsterKillCount);
        }

        void OnEventDarkMazeRewardEvent(int cid, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            if (droppedZeny > 0)
                rewardBuffer.Add(new RewardData(RewardType.Zeny, droppedZeny, rewardCount: 0, rewardOption: 0));

            if (droppedLevelExp > 0)
                rewardBuffer.Add(new RewardData(RewardType.LevelExp, droppedLevelExp, rewardCount: 0, rewardOption: 0));

            if (droppedJobExp > 0)
                rewardBuffer.Add(new RewardData(RewardType.JobExp, droppedJobExp, rewardCount: 0, rewardOption: 0));

            if (rewards != null)
                rewardBuffer.AddRange(rewards);

            if (cid > 0)
            {
                string userName = GetUserName(cid);
                string message = LocalizeKey._39711.ToText() // {NAME} 플레이어가 오크 베이비를 포획했습니다.
                    .Replace(ReplaceKey.NAME, userName);
                ShowToastPopup(message);
            }

            ShowResultUI();
        }

        void OnStartController()
        {
            if (!IsAllReady)
                return;

            if (isFinishedTimer)
                return;

            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            actor.Movement.Stop();
            actor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
            if (!IsAllReady)
                return;

            if (isFinishedTimer)
                return;

            if (position == Vector2.zero)
                return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);

            Vector3 dist = player.LastPosition - savedPlayerPosition;
            if (dist.sqrMagnitude < SEND_POSITION_SQR_MAGNITUDE)
                return;

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnResetController()
        {
            if (!IsAllReady)
                return;

            if (isFinishedTimer)
                return;

            CameraUtils.Zoom(CameraZoomType.None);

            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnFinishedCountdown()
        {
            isFinishedStartingPistol = true;
        }

        private bool IsFinishedStartingPistol()
        {
            return isFinishedStartingPistol;
        }

        private void GiveUp()
        {
            // 이미 퇴장
            if (isExitBattle)
            {
                ShowResultUI();
                return;
            }

            AsyncShowGiveUpMessage().WrapNetworkErrors();
        }

        private async Task AsyncShowGiveUpMessage()
        {
            if (!await UI.SelectPopup(LocalizeKey._39712.ToText())) // 이벤트 던전을 종료하시겠습니까?
                return;

            RequestExit().WrapNetworkErrors(); // 퇴장
        }

        void OnFinishedRemainTimer()
        {
            isFinishedTimer = true;

            uiMvpTimer.StopTimer();
            uiMvpTimer.Hide();
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
            ShowResultUI();

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
        /// 몬스터 상태 - 일반
        /// </summary>
        private void UpdateMonsterGeneralState(int monsterIndex)
        {
            // 대기중인 목록에 포함된 몬스터의 경우 (보스 몬스터: 오크베이비)
            IMonsterBotInput input = standbyMonsterBotInputList.Find(a => a.Index == monsterIndex);
            if (SpawnMonster(input))
            {
                standbyMonsterBotInputList.Remove(input);
                return;
            }

            MonsterBotEntity find = monsterBotPool.Find(monsterIndex);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                const byte STATE = (byte)MonsterBotState.General;
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueState(monsterIndex, STATE))
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
                const byte STATE = (byte)MonsterBotState.Die;
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueState(monsterIndex, STATE))
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

        /// <summary>
        /// 결과창 띄우기
        /// </summary>
        private void ShowResultUI()
        {
            if (isShowReward)
                return;

            isShowReward = true;

            if (!isFinishedTimer)
            {
                uiMvpTimer.StopTimer();
                uiMvpTimer.Hide();
            }

            var buffer = rewardBuffer.GetBuffer(isAutoRelease: true);
            bool isEmptyReward = buffer.Length == 0;
            string message = isEmptyReward
                ? LocalizeKey._90137.ToText() // 획득한 보상이 없습니다.
                : LocalizeKey._90272.ToText(); // 다음 보상을 획득하였습니다.

            UIResultDungeon.TitleType type = isEmptyReward ? UIResultDungeon.TitleType.Failed : UIResultDungeon.TitleType.Clear;
            uiResultDungeon.Show(buffer, default, false, type, message);
        }
    }
}