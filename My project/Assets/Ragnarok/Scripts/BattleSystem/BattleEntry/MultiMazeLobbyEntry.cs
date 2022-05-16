using GamePotUnity;
using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MultiMazeLobbyEntry : BattleEntry
    {
        [System.Obsolete]
        public static class Temp
        {
            public const int DEFAULT_CHANNEL_ID = -1;
        }

        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;

        /******************** Models ********************/
        private readonly DungeonModel dungeonModel;
        private readonly SharingModel sharingModel;
        private readonly CharacterModel characterModel;
        private readonly QuestModel questModel;

        /******************** Repositories ********************/
        private readonly MultiMazeWaitingRoomDataManager multiMazeLobbyDataRepo;
        private readonly MultiMazeDataManager multiMazeDataRepo;
        private readonly StageDataManager stageDataRepo;
        private readonly GateDataManager gateDataRepo;

        /******************** Scene Compositions ********************/
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly MultiBattlePlayers multiBattlePlayers;
        private readonly IHUDPool hudPool;

        /******************** Managers ********************/
        private readonly ConnectionManager connectionManager;

        /******************** UIs ********************/
        private UIController uiController;
        private UIBattleMenu uiBattleMenu;
        private UIMain uiMain;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMainTop uiMainTop;
        private UIBattleInfo uiBattleInfo;
        private UIMainShortcut uiMainShortcut;
        private UISimpleQuest uiSimpleQuest;
        private UICamSelect uiCamSelect;
        private UIQuickExpandMenu uiQuickExpandMenu;
        private UIWorldBossAlarm uiWorldBossAlarm;
        private UIEmotion uiEmotion;
        private UIShadowPlayerSettings uiShadowPlayerSettings;
        private UIJobChangeMenu uiJobChangeMenu;

        /******************** Temp Data ********************/
        private MultiMazeWaitingRoomData currentData;
        private int channelId;
        private Vector3 savedPlayerPosition;
        private bool isFinishExitRoom;
        private HUDObject mazeLobbyField;
        private HUDObject npcQuestAlarm;
        private bool isRecoonect;
        private readonly IBattlePool battlePool;
        private readonly List<PoolObject> poolObjectList;
        private MultiMazePortalZone[] zones;

        public MultiMazeLobbyEntry() : base(BattleMode.MultiMazeLobby)
        {
            player.SetState(UnitEntity.UnitState.Maze); // 일반 플레이어는 1.2배이기 때문에 억지로 넣음

            dungeonModel = player.Dungeon;
            sharingModel = player.Sharing;
            characterModel = player.Character;
            questModel = player.Quest;

            multiMazeLobbyDataRepo = MultiMazeWaitingRoomDataManager.Instance;
            multiMazeDataRepo = MultiMazeDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;
            gateDataRepo = GateDataManager.Instance;
            connectionManager = ConnectionManager.Instance;

            multiBattlePlayers = new MultiBattlePlayers();
            playerBotPool = new PlayerBotEntityPool();
            hudPool = HUDPoolManager.Instance;
            battlePool = BattlePoolManager.Instance;
            poolObjectList = new List<PoolObject>();
        }

        public override IEnumerator<float> YieldExitBattle()
        {
            if (isFinishExitRoom)
            {
                IsSuccessExit = true;
                yield break;
            }

            var param = Protocol.NewInstance();
            BattleMode savedMode = impl.GetSaveBattleMode();
            // 서버변경을 하지 않고 로비Exit 처리: 시나리오미로 또는 멀티미로로 나갈 경우 (멀티미로로비로 나가는 경우 추가: 셰어등록에서 Restart 가 일어남)
            bool isSkipServerChange = savedMode.IsSkipServerChangeFromMultiLobby();
            param.PutInt("1", isSkipServerChange ? 1 : 0);
            Task<Response> task = Protocol.REQUEST_MULMAZE_WAITINGROOM_EXIT.SendAsync(param);
            yield return Timing.WaitUntilTrue(task.IsComplete);

            try
            {
                Response response = task.Result;
                IsSuccessExit = response.isSuccess;

                if (!IsSuccessExit)
                {
                    response.ShowResultCode();
                    yield break;
                }

                isFinishExitRoom = true;
                channelId = -1;
            }
            catch (System.AggregateException ex)
            {
                IsSuccessExit = false;
            }
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            currentData = null;
            multiBattlePlayers.Clear();
            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();

            player.SetForceStatus(ForceStatusType.BuffItemOptionOff); // 버프아이템 Off
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();
            // 다른 플레이어의 이동 수신 이벤트 추가.
            Protocol.REQUEST_MULMAZE_WAITINGROOM_TRANSFORM.AddEvent(OnRequestMultiMazeLobbyTransform);

            // 다른 플레이어 접속 시 정보를 받아오는 이벤트 추가.
            Protocol.RECEIVE_MULMAZE_WAITINGROOM_JOIN.AddEvent(OnReceiveMultiMazeLobbyJoin);

            // 다른 플레이어의 퇴장 수신 이벤트 추가
            Protocol.RECEIVE_MULMAZE_WAITINGROOM_EXIT.AddEvent(OnReceiveMultiMazeLobbyExit);

            // 다른 유저 무기,코스튬 변경 이벤트 추가 
            Protocol.RESPONSE_CHAR_UPDATE.AddEvent(OnReceiveOtherPlayerUpdateCostumeWihtWeapon);

            // 다른 유저의 이모션
            Protocol.REQUEST_EMOTION.AddEvent(OnReceiveEmotion);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();
            // 다른 플레이어의 이동 수신 이벤트 제거
            Protocol.REQUEST_MULMAZE_WAITINGROOM_TRANSFORM.RemoveEvent(OnRequestMultiMazeLobbyTransform);

            // 다른 플레이어 접속 시 정보를 받아오는 이벤트 제거
            Protocol.RECEIVE_MULMAZE_WAITINGROOM_JOIN.RemoveEvent(OnReceiveMultiMazeLobbyJoin);

            // 다른 플레이어의 퇴장 수신 이벤트 제거
            Protocol.RECEIVE_MULMAZE_WAITINGROOM_EXIT.RemoveEvent(OnReceiveMultiMazeLobbyExit);

            // 다른 유저 무기,코스튬 변경 이벤트 제거
            Protocol.RESPONSE_CHAR_UPDATE.RemoveEvent(OnReceiveOtherPlayerUpdateCostumeWihtWeapon);

            // 다른 유저의 이모션
            Protocol.REQUEST_EMOTION.RemoveEvent(OnReceiveEmotion);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnSpawnActor += OnPlayerSpawn;
            player.OnDespawnActor += OnPlayerDespawn;

            characterModel.OnUpdateJobLevel += OnUpdateJobLevel;
            sharingModel.OnUpdateSharingState += Restart;
            questModel.OnUpdateMainQuest += ShowNpcTargetingArrow;
            questModel.OnUpdateMainQuest += RefreshNpcQuestAlarm;

            connectionManager.OnReconnect += OnReconnect;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnSpawnActor -= OnPlayerSpawn;
            player.OnDespawnActor -= OnPlayerDespawn;

            characterModel.OnUpdateJobLevel -= OnUpdateJobLevel;
            sharingModel.OnUpdateSharingState -= Restart;
            questModel.OnUpdateMainQuest -= ShowNpcTargetingArrow;
            questModel.OnUpdateMainQuest -= RefreshNpcQuestAlarm;

            connectionManager.OnReconnect -= OnReconnect;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiController = UI.Show<UIController>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiMain = UI.Show<UIMain>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiMainShortcut = UI.Show<UIMainShortcut>();
            uiSimpleQuest = UI.Show<UISimpleQuest>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiQuickExpandMenu = UI.Show<UIQuickExpandMenu>();
            uiWorldBossAlarm = UI.Show<UIWorldBossAlarm>();
            uiEmotion = UI.Show<UIEmotion>();
            uiShadowPlayerSettings = UI.Show<UIShadowPlayerSettings>();
            uiJobChangeMenu = UI.Show<UIJobChangeMenu>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += OnBattleExit;
            uiQuickExpandMenu.OnSlotInit += OnExpandMenuSlotInit;
            uiEmotion.OnEmotion += OnEmotionPlayer;

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit, UIBattleMenu.MenuContent.Duel, UIBattleMenu.MenuContent.Roulette, UIBattleMenu.MenuContent.Square, UIBattleMenu.MenuContent.JobLevel, UIBattleMenu.MenuContent.FirstPayment, UIBattleMenu.MenuContent.Explore, UIBattleMenu.MenuContent.Buff, UIBattleMenu.MenuContent.CustomerReward); // 나가기, 듀얼, 룰렛
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(true);
            uiQuickExpandMenu.SetShareNoticeMode(isShareNoticeMode: true);

            cameraController.SetView(CameraController.View.Quater40_2_5); // Quater로 전환
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= OnBattleExit;
            uiQuickExpandMenu.OnSlotInit -= OnExpandMenuSlotInit;
            uiEmotion.OnEmotion -= OnEmotionPlayer;

            UI.Close<UIController>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIMain>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMainTop>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIMainShortcut>();
            UI.Close<UISimpleQuest>();
            UI.Close<UICamSelect>();
            UI.Close<UIQuickExpandMenu>();
            UI.Close<UIWorldBossAlarm>();
            UI.Close<UIEmotion>();
            UI.Close<UIShadowPlayerSettings>();
            UI.Close<UIJobChangeMenu>();

            uiController = null;
            uiBattleMenu = null;
            uiMain = null;
            uiBattlePlayerStatus = null;
            uiMainTop = null;
            uiBattleInfo = null;
            uiMainShortcut = null;
            uiSimpleQuest = null;
            uiCamSelect = null;
            uiQuickExpandMenu = null;
            uiWorldBossAlarm = null;
            uiEmotion = null;
            uiShadowPlayerSettings = null;
            uiJobChangeMenu = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            uiBattleInfo.Set(currentData.name_id.ToText());
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            multiBattlePlayers.Recycle();
            DisposeMazeLobbyField();
            DisposeNpcQuestAlarm();

            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();
        }

        void OnUpdateJobLevel(int jobLevel)
        {
            RefreshOpenMaze();
        }

        private void Restart()
        {
            impl.StartBattle(BattleMode.MultiMazeLobby, isStartFadeIn: false);
        }

        void OnReconnect()
        {
            isRecoonect = true;
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            return YieldEnterBattle(dungeonModel.LastEnterMultiMazeLobbyId);
        }

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            MultiMazeData mazeData = multiMazeDataRepo.Get(id);
            if (mazeData == null)
            {
                IsSuccessEnter = false;
#if UNITY_EDITOR
                Debug.LogError($"해당 멀티미로데이터가 존재하지 않습니다: {id}");
#endif
                yield break;
            }

            int waitingRoomId = mazeData.GetWaitingRoomId();
            currentData = multiMazeLobbyDataRepo.Get(waitingRoomId);

            if (currentData == null)
            {
                IsSuccessEnter = false;
#if UNITY_EDITOR
                Debug.LogError($"해당 멀티미로대기실데이터가 존재하지 않습니다: {waitingRoomId}");
#endif
                yield break;
            }

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    var sfs = Protocol.NewInstance();
                    sfs.PutInt("1", id); // 입장하고자 하는 mazeLobbyId
                    sfs.PutInt("2", id); // 입장하고자 하는 mazeLobbyId
                    //sfs.PutInt("2", input.multiMazeId); // 0 : 로비에 중앙 포지션으로 생성, 멀티미로ID로 입장했던 멀티 입구 앞으로 생성
                    Task<Response> task = Protocol.REQUEST_MULMAZE_WAITINGROOM_JOIN.SendAsync(sfs);
                    yield return Timing.WaitUntilTrue(task.IsComplete);
                    Response response = task.Result;
                    IsSuccessEnter = response.isSuccess;

                    if (!IsSuccessEnter)
                    {
                        if (response.resultCode == ResultCode.MULTI_SERVER_CONNECT_ERROR)
                        {
                            // 자동사냥 필드로 이동
                            impl.StartBattle(BattleMode.Stage);
                            yield break;
                        }

                        response.ShowResultCode();
                        yield break;
                    }

                    dungeonModel.SetLastEnterMultiMazeLobbyId(id);
                    channelId = response.GetInt("1");
                    if (response.ContainsKey("2"))
                    {
                        playerBotPool.EnqueueRange(response.GetPacketArray<MultiMazePlayerPacket>("2"));
                    }
                    int posX = response.GetInt("4");
                    int posZ = response.GetInt("5");
                    savedPlayerPosition = new Vector3(posX * 0.001f, 0f, posZ * 0.001f);
                    IsSkipExit = false; // 나가기 처리 필수
                    break;

                case SharingModel.SharingState.Sharing:
                    IsSuccessEnter = true;
                    IsSkipExit = true; // 나가기 처리 스킵
                    break;

                case SharingModel.SharingState.StandByReward:
                    IsSuccessEnter = true;
                    IsSkipExit = true; // 나가기 처리 스킵
                    break;
            }

            isFinishExitRoom = false;
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

            zones = null;
            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                SnowZone snowZone = item.GetComponentInChildren<SnowZone>();

                if (snowZone != null)
                {
                    bool isSnow = false;

                    if (Cheat.IS_SNOW)
                    {
                        isSnow = true;
                    }
                    else if (int.TryParse(GamePot.getConfig("SnowRate"), out int rate))
                    {
                        if (Random.Range(0, 100) < rate)
                        {
                            isSnow = true;
                        }
                    }

                    if (isSnow)
                    {
                        PoolObject snow = battlePool.SpawnSnowEffect(snowZone.transform);
                        if (snow != null)
                            AddPoolObject(snow);
                    }
                }

                if (zones == null || zones.Length == 0)
                    zones = item.GetComponentsInChildren<MultiMazePortalZone>();

                RefreshOpenMaze();
            }
        }

        /// <summary>
        /// 미로 Open 여부 세팅
        /// </summary>
        private void RefreshOpenMaze()
        {
            if (zones == null || zones.Length == 0)
                return;

            foreach (var zone in zones)
            {
                int zoneIndex = zone.GetIndex();

                switch (zoneIndex)
                {
                    case MultiMazePortalZone.CHRISTMAS_EVENT_INDEX: // 이벤트 (크리스마스)
                    case MultiMazePortalZone.DARK_MAZE_EVENT_INDEX: // 이벤트 (미궁이벤트:암흑)
                        zone.SetEvent(); // 이벤트 타입
                        break;

                    case MultiMazePortalZone.FOREST_MAZE_INDEX: // 미궁숲
                        zone.SetState(IsForestMazeOpen() ? MultiMazePortalZone.ZoneState.Open : MultiMazePortalZone.ZoneState.Lock);
                        break;

                    case MultiMazePortalZone.GATE_MAZE_1_INDEX: // 게이트 1
                    case MultiMazePortalZone.GATE_MAZE_2_INDEX: // 게이트 2
                        int gateId = currentData.GetGateId(zoneIndex);
                        GateData gateData = gateDataRepo.Get(gateId);
                        int chapter = gateData == null ? 1 : gateData.chapter;
                        zone.SetState(IsScenarioMazeOpen(chapter) ? MultiMazePortalZone.ZoneState.Open : MultiMazePortalZone.ZoneState.Lock);
                        break;

                    default:
                        int multiMazeId = currentData.GetMultiMazeId(zone.GetIndex());
                        MultiMazeData mazeData = multiMazeDataRepo.Get(multiMazeId);
                        zone.SetState(IsScenarioMazeOpen(mazeData) ? MultiMazePortalZone.ZoneState.Open : MultiMazePortalZone.ZoneState.Lock);
                        break;
                }
            }
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            uiBattlePlayerStatus.SetPlayer(player);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    break;

                case SharingModel.SharingState.Sharing:
                    if (isRecoonect)
                    {
                        UI.ShortCut<UICharacterShareWaiting>();
                    }
                    else
                    {
                        UI.Show<UICharacterShareWaiting>();
                    }
                    break;

                case SharingModel.SharingState.StandByReward:
                    if (isRecoonect)
                    {
                        UI.ShortCut<UICharacterShareReward>();
                    }
                    else
                    {
                        UI.Show<UICharacterShareReward>();
                    }
                    break;
            }

            isRecoonect = false;
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            bool needSpawn = impl.Add(player, isEnemy: false);

            UnitActor unitActor;
            if (needSpawn)
            {
                player.SetForceStatus(ForceStatusType.BuffItemOptionOn); // 버프아이템 On

                unitActor = player.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED);

                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                unitActor = player.GetActor();
            }

            unitActor.AI.SetHomePosition(savedPlayerPosition, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(savedPlayerPosition); // Home으로 강제 워프
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayerBots()
        {
            while (playerBotPool.HasQueue())
            {
                IMultiPlayerInput input = playerBotPool.Dequeue();
                if (SpawnPlayerBot(input))
                    yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(IMultiPlayerInput input)
        {
            UnitEntity entity = playerBotPool.Create(input);

            if (!impl.Add(entity, isEnemy: true))
                return false;

            UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
            unitActor.Movement.SetMode(UnitMovement.Mode.NavMesh);
            unitActor.Movement.SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED);

            Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);
            unitActor.AI.SetHomePosition(pos, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(pos); // Home으로 강제 워프
            unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출
            return true;
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);
                    break;

                case SharingModel.SharingState.Sharing:
                    break;

                case SharingModel.SharingState.StandByReward:
                    break;
            }

            RefreshNpcQuestAlarm();
            ShowNpcTargetingArrow();
            StartTutorial();
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        #region 플레이어 이벤트

        /// <summary>
        /// 플레이어 생성 이벤트
        /// </summary>
        void OnPlayerSpawn(UnitActor unitActor)
        {
            unitActor.Movement.OnPortal += OnPortal;
        }

        /// <summary>
        /// 플레이어 제거 이벤트
        /// </summary>
        void OnPlayerDespawn(UnitActor unitActor)
        {
            unitActor.Movement.OnPortal -= OnPortal;
        }

        /// <summary>
        /// 포탈 충돌
        /// </summary>
        void OnPortal(GameObject portal)
        {
            if (portal == null)
            {
                DisposeMazeLobbyField();
                return;
            }

            MultiMazePortalZone zone = portal.GetComponent<MultiMazePortalZone>();
            if (zone == null)
            {
                Debug.LogError("멀티로비 포탈에 존이 음슴");
                return;
            }

            int zoneIndex = zone.GetIndex();
            switch (zoneIndex)
            {
                case MultiMazePortalZone.CHRISTMAS_EVENT_INDEX: // 이벤트 (크리스마스)
                case MultiMazePortalZone.DARK_MAZE_EVENT_INDEX: // 이벤트 (미궁이벤트:암흑)
                    int eventMazeId = currentData.GetMultiMazeId(zoneIndex);
                    mazeLobbyField = hudPool.SpawnMazeLobbyEventField(portal.transform, eventMazeId);
                    break;

                case MultiMazePortalZone.FOREST_MAZE_INDEX: // 미궁숲
                    if (zone.GetIsLock())
                    {
                        dungeonModel.IsOpened(DungeonType.ForestMaze, isShowPopup: true);
                        return;
                    }

                    mazeLobbyField = hudPool.SpawnMazeLobbyForestMaze(portal.transform, MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_FOREST_MAZE);
                    break;

                case MultiMazePortalZone.GATE_MAZE_1_INDEX: // 게이트 1
                case MultiMazePortalZone.GATE_MAZE_2_INDEX: // 게이트 2
                    {
                        int gateId = currentData.GetGateId(zoneIndex);
                        if (zone.GetIsLock())
                        {
                            GateData gateData = gateDataRepo.Get(gateId);
                            int chapter = gateData == null ? 1 : gateData.chapter;
                            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Normal, chapter);
                            if (find == null)
                            {
                                Debug.LogError($"chapter에 해당하는 stageData가 존재하지 않음: {nameof(chapter)} = {chapter}");
                                return;
                            }

                            string message = LocalizeKey._48801.ToText() // {NAME}에 도달하면 오픈됩니다.
                                .Replace(ReplaceKey.NAME, find.name_id.ToText());

                            UI.ShowToastPopup(message);
                            return;
                        }

                        mazeLobbyField = hudPool.SpawnMazeLobbyGateMaze(portal.transform, gateId);
                    }
                    break;

                default:
                    {
                        // multiMazeId 와 chapter 가 동일 (멀티미로 입장 등에서 multiMazeId 를 chapter 로 사용하고 있음)
                        int multiMazeId = currentData.GetMultiMazeId(zoneIndex);
                        int chapter = multiMazeId;

                        if (zone.GetIsLock())
                        {
                            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Normal, chapter);
                            if (find == null)
                            {
                                Debug.LogError($"chapter에 해당하는 stageData가 존재하지 않음: {nameof(chapter)} = {chapter}");
                                return;
                            }

                            string message = LocalizeKey._48801.ToText() // {NAME}에 도달하면 오픈됩니다.
                                .Replace(ReplaceKey.NAME, find.name_id.ToText());

                            UI.ShowToastPopup(message);
                            return;
                        }

                        mazeLobbyField = hudPool.SpawnMazeLobbyField(portal.transform, chapter);
                    }
                    break;
            }
        }

        #endregion

        #region 서버 응답 이벤트

        void OnRequestMultiMazeLobbyTransform(Response response)
        {
            int cid = response.GetInt("1");
            int[] arrayPosValue = response.GetIntArray("2");

            // 방어코드
            if (arrayPosValue == null)
                return;

            bool isPlayer = cid == player.Character.Cid;
            Vector3 position = new Vector3(arrayPosValue[0] * 0.001f, arrayPosValue[1] * 0.001f, arrayPosValue[2] * 0.001f);

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
            unitActor.Appearance.PlayEmotion(false);
        }

        void OnReceiveMultiMazeLobbyJoin(Response response)
        {
            IMultiPlayerInput multiPlayer = response.GetPacket<MultiMazePlayerPacket>("1");

            // 생성 도중이라면 생성큐 추가
            if (playerBotPool.HasQueue())
            {
                playerBotPool.Enqueue(multiPlayer);
                return;
            }

            // 즉시 생성
            if (SpawnPlayerBot(multiPlayer))
                return;

#if UNITY_EDITOR
            int cid = multiPlayer.Cid;
            Debug.LogError($"생성하지 못한 플레이어 봇: {nameof(cid)} = {cid}");
#endif
        }

        void OnReceiveMultiMazeLobbyExit(Response response)
        {
            int cid = response.GetInt("1");

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

            Dispose(find); // 제거
            playerBotPool.Recycle(find); // 회수
        }

        void OnReceiveOtherPlayerUpdateCostumeWihtWeapon(Response response)
        {
            int cid = response.GetInt("1");
            string costumeIds = response.GetUtfString("2");
            int weaponItemId = response.GetInt("3");

            PlayerBotEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueChar(cid, weaponItemId, costumeIds))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 무기,코스튬 변경: {nameof(cid)} = {cid}");
#endif
                return;
            }

            string[] results = costumeIds.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            int[] equipCostumeIds = new int[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                equipCostumeIds[i] = int.Parse(results[i]);
            }

            find.Inventory.UpdateCostumeWithWeapon(weaponItemId, equipCostumeIds);
        }

        void OnReceiveEmotion(Response response)
        {
            int cid = response.GetInt("1");
            int emotionType = response.GetInt("2");
            PlayerBotEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어는 무시");
#endif
                return;
            }

            PlayEmotion(find, emotionType.ToEnum<EmotionType>()); // 이모션 표시
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

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            unitActor.Movement.Move(motion);
            unitActor.Appearance.PlayEmotion(false);

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

            unitActor.AI.SetInputMove(isControl: false);
            unitActor.Movement.Stop();

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnBattleExit()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            ExitEntry(); // 멀티 미로 로비 퇴장
        }

        /// <summary>
        /// ExpandMenu Slot SetData 이벤트
        /// </summary>
        private void OnExpandMenuSlotInit(int index)
        {
            // 무조건 부활상태, 풀피로 만들어주기.
            uiQuickExpandMenu.SetShareCharacterReviveTime(index, 0f);
            uiQuickExpandMenu.SetShareCharacterCurrentHp(index, 1, 1, skipAnim: true);
        }

        /// <summary>
        /// 이모션 표시
        /// </summary>
        private void OnEmotionPlayer(EmotionType type)
        {
            PlayEmotion(player, type);

            // 이모션 전송
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", (int)RoomType.MultiMaze);
            sfs.PutInt("2", (int)type);
            Protocol.REQUEST_EMOTION.SendAsync(sfs).WrapNetworkErrors();
        }

        #endregion

        private void ShowNpcTargetingArrow()
        {
            QuestInfo mainQuest = questModel.GetMaintQuest();
            if (mainQuest == null)
                return;

            // 진행중이 아님
            if (mainQuest.CompleteType != QuestInfo.QuestCompleteType.InProgress)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
            {
                Debug.LogError("Player Actor 음슴");
                return;
            }

            actor.EffectPlayer.ReleaseNpcTargetingArrow();
            if (mainQuest.QuestType == QuestType.SCENARIO_MAZE_ID_CLEAR_COUNT || mainQuest.QuestType == QuestType.MULTI_MAZE_CHAPTER_CLEAR_COUNT || mainQuest.QuestType == QuestType.MULTI_MAZE_ENTER_COUNT)
            {
                MultiMazePortalZone zone = FindCurrentZone(mainQuest.ConditionValue);
                if (zone == null)
                {
                    Debug.LogError($"MultiMazePortalZone 가 음슴: id = {currentData.id}");
                    return;
                }

                actor.EffectPlayer.ShowNpcTargetingArrow(zone.transform);
            }
        }

        private void RefreshNpcQuestAlarm()
        {
            DisposeNpcQuestAlarm();

            QuestInfo mainQuest = questModel.GetMaintQuest();
            if (mainQuest == null)
                return;

            // 진행중이 아님
            if (mainQuest.CompleteType != QuestInfo.QuestCompleteType.InProgress)
                return;

            if (mainQuest.QuestType == QuestType.SCENARIO_MAZE_ID_CLEAR_COUNT || mainQuest.QuestType == QuestType.MULTI_MAZE_CHAPTER_CLEAR_COUNT || mainQuest.QuestType == QuestType.MULTI_MAZE_ENTER_COUNT)
            {
                MultiMazePortalZone zone = FindCurrentZone(mainQuest.ConditionValue);
                if (zone == null)
                {
                    Debug.LogError($"MultiMazePortalZone 가 음슴: id = {currentData.id}");
                    return;
                }

                npcQuestAlarm = hudPool.SpawnNpcQuestAlarm(zone.transform);
            }
        }

        private bool StartTutorial()
        {
            if (Tutorial.Run(TutorialType.SingleMazeEnter))
                return true;

            if (Tutorial.Run(TutorialType.MultiMazeEnter))
                return true;

            if (Tutorial.Run(TutorialType.MazeExit))
                return true;

            if (Tutorial.Run(TutorialType.PronteraScenario))
                return true;

            if (Tutorial.Run(TutorialType.ShareControl1))
                return true;

            if (Tutorial.Run(TutorialType.ShareLevelUp))
                return true;

            if (Tutorial.Run(TutorialType.ChangeElement))
                return true;

            if (Tutorial.Run(TutorialType.TierUp))
                return true;

            if (Tutorial.Run(TutorialType.ManageCard))
                return true;

            if (Tutorial.Run(TutorialType.StoryBook8Open))
                return true;

            if (Tutorial.Run(TutorialType.StoryBook9Open))
                return true;

            if (Tutorial.Run(TutorialType.ShareClone))
                return true;

            if (Tutorial.Run(TutorialType.StoryBook11Open))
                return true;

            if (Tutorial.Run(TutorialType.StoryBook12Open))
                return true;

            if (Tutorial.Run(TutorialType.StoryBook13Open))
                return true;

            if (Tutorial.Run(TutorialType.GateEnter))
                return true;

            return false;
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
            Protocol.REQUEST_MULMAZE_WAITINGROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        private new void ExitEntry()
        {
            impl.StartBattle(BattleMode.Stage);
        }

        private void DisposeMazeLobbyField()
        {
            if (mazeLobbyField == null)
                return;

            mazeLobbyField.Release();
        }

        private void DisposeNpcQuestAlarm()
        {
            if (npcQuestAlarm == null)
                return;

            npcQuestAlarm.Release();
        }

        private bool IsScenarioMazeOpen(MultiMazeData mazeData)
        {
            if (mazeData == null)
            {
                Debug.LogError("MazeData is Null");
                return false;
            }

            return IsScenarioMazeOpen(mazeData.chapter);
        }

        private bool IsScenarioMazeOpen(int chapter)
        {
            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Normal, chapter);
            if (find == null)
            {
                Debug.LogError($"chapter에 해당하는 stageData가 존재하지 않음: {nameof(chapter)} = {chapter}");
                return false;
            }

            // 해당 챕터의 스테이지 도달
            return find.id <= dungeonModel.FinalStageId;
        }

        /// <summary>
        /// 미궁숲 오픈 조건
        /// </summary>
        private bool IsForestMazeOpen()
        {
            return dungeonModel.IsOpened(DungeonType.ForestMaze, isShowPopup: false);
        }

        /// <summary>
        /// 이모션 표시
        /// </summary>
        private void PlayEmotion(CharacterEntity entity, EmotionType type)
        {
            UnitActor actor = entity.GetActor();
            if (actor == null)
                return;

            float remainTime = actor.Animator.PlayEmotion(type, entity.Character.Gender);
            actor.Appearance.PlayEmotion(true, remainTime);
            hudPool.SpawnEmotion(actor.CachedTransform, type);
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

        private MultiMazePortalZone FindCurrentZone(int conditionValue)
        {
            if (zones == null || zones.Length == 0)
                return null;

            foreach (var item in zones)
            {
                int chapter = currentData.GetMultiMazeId(item.GetIndex());
                if (chapter == conditionValue)
                    return item;
            }

            return null;
        }

        #region Tutorial
        public void GoToCurrentNpc(int chapter)
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
            {
                Debug.LogError("PlayerActor 가 음슴");
                return;
            }

            MultiMazePortalZone zone = FindCurrentZone(chapter);
            if (zone == null)
            {
                Debug.LogError($"MultiMazePortalZone 가 음슴: id = {chapter}");
                return;
            }

            const float DISTANCE = 3f;
            Vector3 playerPos = player.LastPosition;
            Vector3 npcPos = zone.transform.position;
            Vector3 direction = npcPos - playerPos;
            Vector3 dest = Vector3.MoveTowards(npcPos, playerPos, DISTANCE);
            actor.Movement.ForceSetDestination(dest);
            actor.CachedTransform.rotation = Quaternion.LookRotation(direction);
        }

        public bool IsMoveFinishedToNpc()
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
                return false;

            // 움직이는 중
            if (!actor.Movement.IsStopped)
                return false;

            if (mazeLobbyField == null)
                return false;

            return true;
        }

        public UIWidget GetMazeLobbyFieldWidget()
        {
            return mazeLobbyField.GetComponentInChildren<UIWidget>();
        }
        #endregion
    }
}