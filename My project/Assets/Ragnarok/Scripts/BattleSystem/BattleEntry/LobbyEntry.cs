using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class LobbyEntry : BattleEntry
    {
        public sealed class PostAction : IBattleInput
        {
            public static readonly PostAction None = new PostAction(Type.None);
            public static readonly PostAction MoveToNpcTailing = new PostAction(Type.MoveToNpcTailing);
            public static readonly PostAction MoveToNpcSorin = new PostAction(Type.MoveToNpcSorin);

            public enum Type
            {
                None = 0,
                MoveToNpcTailing,
                MoveToNpcSorin,
            }

            public readonly Type type;

            private PostAction(Type type)
            {
                this.type = type;
            }
        }

        private const int CHANNEL_MAX_PLAYER_COUNT = 60;
        private const int CHANNEL_MAX_STORE_COUNT = 50;

        private int channel;

        private const string SCENE_NAME = "Ayothaya";
        private const string BGM_NAME = "Ayotaya";

        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;
        private const float MIN_POSITION_SYNC_THRESHOLD = 0.35f;

        /******************** Models ********************/
        private readonly TradeModel tradeModel;
        private readonly ChatModel chatModel;
        private readonly DungeonModel dungeonModel;
        private readonly SharingModel sharingModel;
        private readonly QuestModel questModel;

        /******************** Scene Compositions ********************/
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly IHUDPool hudPool;
        private readonly Buffer<NpcSpawner> npcBuffer;

        /******************** Managers ********************/
        private readonly ConnectionManager connectionManager;

        /******************** UIs ********************/
        private UIMain uiMain;
        private UIMainTop uiMainTop;
        private UIMainShortcut uiMainShortcut;
        private UIPrivateStore uiPrivateStore;
        private UIController uiController;
        private UITown uiTown;
        private UIGoodsView uiGoodsView;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UIAutoEquip uiAutoEquip;
        private UICamSelect uiCamSelect;
        private UIQuickExpandMenu uiQuickExpandMenu;
        private UISimpleQuest uiSimpleQuest;
        private UIWorldBossAlarm uiWorldBossAlarm;
        private UIEmotion uiEmotion;
        private UIShadowPlayerSettings uiShadowPlayerSettings;
        private UIDialogue uiDialogue;
        private UIJobChangeMenu uiJobChangeMenu;
        private UIPowerSavingMenu uiPowerSavingMenu;

        /******************** Temp Data ********************/
        private Vector3 savedPlayerPosition = GuildSquareManager.EmperiumPosition; // 서버가 알고있는 내 좌표
        /// <summary> 채널 인원수 정보 리스트 </summary>
        private readonly List<LobbyChannelInfo> lobbyChannelInfoList;
        private bool isRecoonect;
        private PostAction.Type postActionType;

        public LobbyEntry() : base(BattleMode.Lobby)
        {
            lobbyChannelInfoList = new List<LobbyChannelInfo>();

            tradeModel = player.Trade;
            chatModel = player.ChatModel;
            dungeonModel = player.Dungeon;
            sharingModel = player.Sharing;
            questModel = player.Quest;

            connectionManager = ConnectionManager.Instance;

            playerBotPool = new PlayerBotEntityPool();

            hudPool = HUDPoolManager.Instance;
            npcBuffer = new Buffer<NpcSpawner>();
        }

        public override IEnumerator<float> YieldExitBattle()
        {
            Task<Response> task = Protocol.REQUEST_TRADEPRIVATE_EXITROOM.SendAsync();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessExit = response.isSuccess;

            if (!IsSuccessExit)
                response.ShowResultCode();

            player.Trade.SellingState = PrivateStoreSellingState.NOT_SELLING;
            channel = -1;
        }

        protected override void Dispose()
        {
            base.Dispose();

            // 플레이어 봇 폐기
            foreach (var item in playerBotPool)
            {
                Dispose(item);
            }

            player.SetForceStatus(ForceStatusType.BuffItemOptionOff); // 버프아이템 Off
            npcBuffer.Clear();
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();
            // 다른 플레이어 접속 시 정보를 받아오는 이벤트 추가.
            Protocol.RESPONSE_TRADEPRIVATE_ENTERROOM.AddEvent(OnReceiveOtherPlayerEnter);

            // 다른 플레이어의 이동 수신 이벤트 추가.
            Protocol.REQUEST_TRADEPRIVATE_TRANSFORM.AddEvent(OnReceiveOtherPlayerMove);

            // 다른 플레이어의 개인 상점 아이템 등록 수신 이벤트 추가
            Protocol.RESPONSE_TRADEPRIVATE_REGISTER.AddEvent(OnReceiveOtherPlayerItemRegister);

            // 다른 플레이어의 거래소 퇴장 수신 이벤트 추가
            Protocol.RESPONSE_TRADEPRIVATE_EXITROOM.AddEvent(OnReceiveOtherPlayerLobbyExit);

            // 다른 플레이어가 개인 상점을 폐쇄했을 때 수신 이벤트 추가.
            Protocol.RESPONSE_TRADEPRIVATE_END.AddEvent(OnReceiveOtherPlayerStoreClose);

            // 개인상점 귓속말, 로비 채팅 관련
            Protocol.REQUEST_TRADEPRIVATE_WHISPER.AddEvent(OnReceiveChat_PrivateWhisper);

            // 다른 유저 무기,코스튬 변경 이벤트
            Protocol.RESPONSE_CHAR_UPDATE.AddEvent(OnReceiveOtherPlayerUpdateCostumeWihtWeapon);

            // 다른 유저의 이모션
            Protocol.REQUEST_EMOTION.AddEvent(OnReceiveEmotion);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();
            // 다른 플레이어 접속 시 정보를 받아오는 이벤트 제거.
            Protocol.RESPONSE_TRADEPRIVATE_ENTERROOM.RemoveEvent(OnReceiveOtherPlayerEnter);

            // 다른 플레이어의 이동 수신 이벤트 제거.
            Protocol.REQUEST_TRADEPRIVATE_TRANSFORM.RemoveEvent(OnReceiveOtherPlayerMove);

            // 다른 플레이어의 개인 상점 아이템 등록 수신 이벤트 제거
            Protocol.RESPONSE_TRADEPRIVATE_REGISTER.RemoveEvent(OnReceiveOtherPlayerItemRegister);

            // 다른 플레이어의 거래소 퇴장 수신 이벤트 제거
            Protocol.RESPONSE_TRADEPRIVATE_EXITROOM.RemoveEvent(OnReceiveOtherPlayerLobbyExit);

            // 다른 플레이어가 개인 상점을 폐쇄했을 때 수신 이벤트 제거.
            Protocol.RESPONSE_TRADEPRIVATE_END.RemoveEvent(OnReceiveOtherPlayerStoreClose);

            // 개인상점 귓속말, 로비 채팅 관련
            Protocol.REQUEST_TRADEPRIVATE_WHISPER.RemoveEvent(OnReceiveChat_PrivateWhisper);

            // 다른 유저 무기,코스튬 변경 이벤트
            Protocol.RESPONSE_CHAR_UPDATE.RemoveEvent(OnReceiveOtherPlayerUpdateCostumeWihtWeapon);

            // 다른 유저의 이모션
            Protocol.REQUEST_EMOTION.RemoveEvent(OnReceiveEmotion);
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnSpawnActor += OnPlayerSpawn;
            player.OnDespawnActor += OnPlayerDespawn;

            chatModel.SetChatMode(ChatMode.Lobby);
            chatModel.OnLobbyChat += OnLobbyChat;
            sharingModel.OnUpdateSharingState += Restart;
            questModel.OnUpdateKafra += ShowNpcTargetingArrow;
            questModel.OnUpdateKafra += ShowNpcTalk;

            connectionManager.OnReconnect += OnReconnect;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnSpawnActor -= OnPlayerSpawn;
            player.OnDespawnActor -= OnPlayerDespawn;

            chatModel.SetChatMode(ChatMode.Channel);
            chatModel.OnLobbyChat -= OnLobbyChat;
            sharingModel.OnUpdateSharingState -= Restart;
            questModel.OnUpdateKafra -= ShowNpcTargetingArrow;
            questModel.OnUpdateKafra -= ShowNpcTalk;

            connectionManager.OnReconnect -= OnReconnect;
        }

        protected override void InitCanvas()
        {
            uiMain = UI.Show<UIMain>();
            uiMainTop = UI.Show<UIMainTop>();
            uiMainShortcut = UI.Show<UIMainShortcut>();
            uiTown = UI.Show<UITown>();
            uiPrivateStore = UI.Show<UIPrivateStore>();
            uiController = UI.Show<UIController>();
            uiGoodsView = UI.Show<UIGoodsView>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiAutoEquip = UI.Show<UIAutoEquip>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiQuickExpandMenu = UI.Show<UIQuickExpandMenu>();
            uiSimpleQuest = UI.Show<UISimpleQuest>();
            uiWorldBossAlarm = UI.Show<UIWorldBossAlarm>();
            uiEmotion = UI.Show<UIEmotion>();
            uiShadowPlayerSettings = UI.Show<UIShadowPlayerSettings>();
            uiDialogue = UI.Show<UIDialogue>();
            uiJobChangeMenu = UI.Show<UIJobChangeMenu>();
            uiPowerSavingMenu = UI.Show<UIPowerSavingMenu>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += OnExitButton;
            uiQuickExpandMenu.OnSlotInit += OnExpandMenuSlotInit;
            uiEmotion.OnEmotion += OnEmotionPlayer;

            // Initialize
            uiMainTop.Initialize(showCatCoin: false, showRoPoint: true);
            uiMainTop.SetEnableButton(true);
            uiPrivateStore.Hide();
            uiController.SetActive(true);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit, UIBattleMenu.MenuContent.Trade, UIBattleMenu.MenuContent.JobLevel, UIBattleMenu.MenuContent.FirstPayment, UIBattleMenu.MenuContent.CustomerReward, UIBattleMenu.MenuContent.Roulette, UIBattleMenu.MenuContent.Explore, UIBattleMenu.MenuContent.Buff); // 나가기, 노점
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiQuickExpandMenu.SetShareNoticeMode(isShareNoticeMode: false);
            uiDialogue.Hide();

            cameraController.SetView(CameraController.View.Quater40_5); // Quater로 전환
        }

        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= OnExitButton;
            uiQuickExpandMenu.OnSlotInit -= OnExpandMenuSlotInit;
            uiEmotion.OnEmotion -= OnEmotionPlayer;

            UI.Close<UIMain>();
            UI.Close<UIMainTop>();
            UI.Close<UIMainShortcut>();
            UI.Close<UITown>();
            UI.Close<UIPrivateStore>();
            UI.Close<UIPrivateStoreProductSetting>();
            UI.Close<UIController>();
            UI.Close<UIGoodsView>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIAutoEquip>();
            UI.Close<UICamSelect>();
            UI.Close<UIQuickExpandMenu>();
            UI.Close<UISimpleQuest>();
            UI.Close<UIWorldBossAlarm>();
            UI.Close<UIEmotion>();
            UI.Close<UIShadowPlayerSettings>();
            UI.Close<UIDialogue>();
            UI.Close<UIJobChangeMenu>();
            UI.Close<UIPowerSavingMenu>();

            uiMain = null;
            uiMainTop = null;
            uiMainShortcut = null;
            uiTown = null;
            uiPrivateStore = null;
            uiController = null;
            uiGoodsView = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiAutoEquip = null;
            uiCamSelect = null;
            uiQuickExpandMenu = null;
            uiSimpleQuest = null;
            uiWorldBossAlarm = null;
            uiEmotion = null;
            uiShadowPlayerSettings = null;
            uiDialogue = null;
            uiJobChangeMenu = null;
            uiPowerSavingMenu = null;
        }

        protected override void OnLocalize()
        {
            string mainText = LocalizeKey._3044.ToText(); // 아요타야 마을
            uiBattleInfo.Set(mainText);
        }

        public override void EndBattle()
        {
            base.EndBattle();
        }

        private void Restart()
        {
            impl.StartBattle(BattleMode.Lobby, isStartFadeIn: false);
        }

        void OnReconnect()
        {
            channel = -1; // 채널 초기화
            isRecoonect = true;
        }

        public override IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
            if (battleInput is PostAction lobbyAction)
            {
                postActionType = lobbyAction.type;
            }

            return YieldEnterBattle();
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            return YieldEnterBattle(dungeonModel.LastEnterLobbyChannel);
        }

        public override IEnumerator<float> YieldEnterBattle(int channelId)
        {
            base.YieldEnterBattle(channelId);

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();

            // 동일한 채널이면 무시
            bool isSameChannel = (channel == channelId);
            if (isSameChannel)
            {
                IsSuccessEnter = true;
                yield break;
            }

            switch (sharingState)
            {
                case SharingModel.SharingState.None:

                    // 상점 폐쇄
                    player.Trade.SellingState = PrivateStoreSellingState.NOT_SELLING;

                    // 채널 변경 시도
                    Task<Response> task = tradeModel.RequestEnterChannelAsync(channelId);
                    yield return Timing.WaitUntilTrue(task.IsComplete);
                    Response response = task.Result;
                    IsSuccessEnter = response.isSuccess;

                    if (!IsSuccessEnter)
                        yield break;

                    // 다른 플레이어들 정보
                    Response multiLobbyPlayers = new Response(response.GetSFSObject("2"));
                    IMultiPlayerInput[] characterPackets = multiLobbyPlayers.ContainsKey("in") ? multiLobbyPlayers.GetPacketArray<MultiLobbyPlayerPacket>("in") : System.Array.Empty<MultiLobbyPlayerPacket>();
                    playerBotPool.EnqueueRange(characterPackets);

                    // 위치 정보
                    float playerPosX = response.GetFloat("3");
                    float playerPosZ = response.GetFloat("4");
                    savedPlayerPosition = new Vector3(playerPosX, Constants.Map.POSITION_Y, playerPosZ);

                    // 채널 정보
                    channel = response.GetInt("1");
                    chatModel.LobbyChannel = channel;
                    dungeonModel.SetLastEnterLobbyChannel(channelId);
                    string channelInfoStr = response.GetUtfString("5");
                    var channelInfoStrArr = channelInfoStr.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);

                    lobbyChannelInfoList.Clear();
                    for (int i = 0; i < channelInfoStrArr.Length / 2; i++)
                    {
                        LobbyChannelInfo channelInfo = new LobbyChannelInfo();
                        channelInfo.channel = int.Parse(channelInfoStrArr[i * 2]);
                        channelInfo.playerCount = int.Parse(channelInfoStrArr[i * 2 + 1]);
                        channelInfo.maxPlayerCount = CHANNEL_MAX_PLAYER_COUNT; // TODO: 상수
                        channelInfo.maxStoreCount = CHANNEL_MAX_STORE_COUNT; // TODO: 상수

                        lobbyChannelInfoList.Add(channelInfo);
                    }
                    lobbyChannelInfoList.Sort((A, B) => A.channel < B.channel ? -1 : (A.channel == B.channel ? 0 : 1)); // 정렬

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
        }

        public override void Ready()
        {
            string sceneName = GetSceneName();
            string bgmName = BGM_NAME;
            bool isChangeScene = true;
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            npcBuffer.Clear(); // 중복해서 씬이 켜질 수 있다.
            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        private string GetSceneName()
        {
            int selectedServerId = connectionManager.GetSelectServerGroupId();
            string sceneName = BasisType.TOWN_LOBBY_SCENE_NAME.GetString(selectedServerId); // 서버에 해당하는 씬 이름

            // 방어코드 1
            if (string.IsNullOrEmpty(sceneName))
                sceneName = BasisType.TOWN_LOBBY_SCENE_NAME.GetString(0); // 첫번째 서버의 씬 이름

            // 방어코드 2
            if (string.IsNullOrEmpty(sceneName))
                sceneName = SCENE_NAME;

            return sceneName;
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        /// <param name="roots"></param>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (GameObject root in roots)
            {
                NpcSpawner[] finds = root.GetComponentsInChildren<NpcSpawner>();
                if (finds == null || finds.Length == 0)
                    continue;

                npcBuffer.AddRange(finds);
            }
        }

        protected override void OnReady()
        {
            base.OnReady();

            uiTown.SetChannelList(lobbyChannelInfoList);
            uiTown.SetChannel(channel);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator<float> YieldComposeScene()
        {
            // 플레이어 Spawn
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnNpc(), TAG);

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
        /// <returns></returns>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            bool needSpawn = impl.Add(player, isEnemy: false);

            UnitActor unitActor;
            if (needSpawn)
            {
                player.SetForceStatus(ForceStatusType.BuffItemOptionOn); // 버프아이템 On

                unitActor = player.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MOVE_SPEED);

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
        /// Npc 소환
        /// </summary>
        private IEnumerator<float> YieldSpawnNpc()
        {
            foreach (NpcSpawner spawner in npcBuffer)
            {
                if (!IsLobbyNpc(spawner.GetNpcType()))
                    continue;

                spawner.Spawn(); // npc 소환
                yield return Timing.WaitForOneFrame;
            }
        }

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

            entity.OnSpawnActor += OnPlayerBotSpawn;
            entity.OnDespawnActor += OnPlayerBotDespawn;

            if (!impl.Add(entity, isEnemy: true))
                return false;

            UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
            unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MOVE_SPEED);
            unitActor.Radar.SetSenseDistance(4);
            unitActor.Radar.SetSenseTargetType(TargetType.EnemyCharacter);
            unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);

            Vector3 pos = new Vector3(input.PosX, Constants.Map.POSITION_Y, input.PosZ);
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

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                playerActor.EffectPlayer.ShowUnitCircle();
                playerActor.AI.EndAI();
            }
            ShowNpcTargetingArrow();

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);

                    // 인구수, 노점수 Refresh
                    RefreshChannelView();

                    int sellerCid = connectionManager.GetSellerCid();
                    if (sellerCid > 0)
                        Timing.RunCoroutine(YieldPrivateStoreProductList(sellerCid), TAG);

                    Tutorial.Run(TutorialType.Trade);
                    break;

                case SharingModel.SharingState.Sharing:
                    break;

                case SharingModel.SharingState.StandByReward:
                    break;
            }

            switch (postActionType)
            {
                case PostAction.Type.MoveToNpcTailing:
                    WarpToNpc(NpcType.Tailing, isAction: true);
                    break;

                case PostAction.Type.MoveToNpcSorin:
                    WarpToNpc(NpcType.Sorin, isAction: true);
                    break;
            }

            postActionType = PostAction.Type.None; // ActionType 초기화
        }

        private void WarpToNpc(NpcType npcType, bool isAction)
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
            {
                Debug.LogError("PlayerActor 가 음슴");
                return;
            }

            NpcSpawner npcSpawner = GetNpcSpawner(npcType);
            if (npcSpawner == null)
            {
                Debug.LogError($"NpcSpawner 가 음슴 {nameof(npcType)} = {npcType}");
                return;
            }

            const float DISTANCE = 3f;
            Vector3 playerPos = player.LastPosition;
            Vector3 npcPos = npcSpawner.GetPosition();
            Vector3 direction = npcPos - playerPos;
            Vector3 dest = Vector3.MoveTowards(npcPos, playerPos, DISTANCE);
            actor.Movement.ForceWarp(dest);
            actor.CachedTransform.rotation = Quaternion.LookRotation(direction);
            SendPlayerCurrentPosition();

            if (isAction)
            {
                OnNpcClicked(npcType);
            }
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        /// <param name="unitEntity"></param>
        /// <param name="attacker"></param>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        /// <summary>
        /// 인구수, 노점수 변동 이벤트 발동
        /// </summary>
        private void RefreshChannelView()
        {
            if (uiTown)
                uiTown.RefreshChannelView();
        }


        #region 플레이어 이벤트

        /// <summary>
        /// 플레이어 생성 이벤트
        /// </summary>
        void OnPlayerSpawn(UnitActor unitActor)
        {
            unitActor.Movement.OnPortal += OnPortal;
            unitActor.Movement.OnPortalOut += OnPortalOut;
        }

        /// <summary>
        /// 플레이어 제거 이벤트
        /// </summary>
        void OnPlayerDespawn(UnitActor unitActor)
        {
            tradeModel.SellingState = PrivateStoreSellingState.NOT_SELLING;

            unitActor.Movement.OnPortal -= OnPortal;
            unitActor.Movement.OnPortalOut -= OnPortalOut;
        }

        /// <summary>
        /// 포탈 충돌
        /// </summary>
        void OnPortal(GameObject portal)
        {
            if (portal == null)
                return;

            NpcSpawner npcSpanwer = portal.GetComponent<NpcSpawner>();
            if (npcSpanwer == null)
                return;

            npcSpanwer.SpawnSign(OnNpcClicked);
        }

        /// <summary>
        /// 포탈 충돌
        /// </summary>
        void OnPortalOut(GameObject portal)
        {
            NpcSpawner npcSpanwer = portal.GetComponent<NpcSpawner>();
            if (npcSpanwer == null)
                return;

            npcSpanwer.DespawnSign();
        }

        void OnNpcClicked(NpcType npcType)
        {
            switch (npcType)
            {
                case NpcType.Tailing:
                    UI.Show<UIExchangeShop>();
                    break;

                case NpcType.Sorin:
                    UI.Show<UIKafraDelivery>(); // 카프라 운송 퀘스트 UI
                    break;

                case NpcType.Sortie:
                    Timing.RunCoroutine(YieldShowNpcSortieTalk(questModel.CurKafraType, questModel.KafraCompleteType), TAG);
                    break;

                case NpcType.Nyankun:
                    UI.Show<UINabiho>();
                    break;
            }
        }

        #endregion

        #region 플레이어 봇 이벤트

        void OnPlayerBotSpawn(UnitActor unitActor)
        {
            unitActor.EffectPlayer.SetUnitInfo();
            unitActor.EffectPlayer.SetStoreBalloon();
            unitActor.EffectPlayer.ShowShadow(); // 그림자 생성

            unitActor.Radar.OnTargetActor += OnPlayerBotTarget;
            unitActor.EffectPlayer.OnInfo += OnPlayerBotInfo;
            unitActor.EffectPlayer.OnChat += OnPlayerBotChat;
            unitActor.EffectPlayer.OnStore += OnPlayerBotStore;
        }

        void OnPlayerBotDespawn(UnitActor unitActor)
        {
            unitActor.Radar.OnTargetActor -= OnPlayerBotTarget;
            unitActor.EffectPlayer.OnInfo -= OnPlayerBotInfo;
            unitActor.EffectPlayer.OnChat -= OnPlayerBotChat;
            unitActor.EffectPlayer.OnStore -= OnPlayerBotStore;

            unitActor.Entity.OnSpawnActor -= OnPlayerBotSpawn;
            unitActor.Entity.OnDespawnActor -= OnPlayerBotDespawn;
        }

        void OnPlayerBotTarget(UnitActor target, UnitActor unitActor)
        {
            if (target)
            {
                unitActor.EffectPlayer.ShowBattleHUD();
            }
            else
            {
                unitActor.EffectPlayer.HideBattleHUD();
            }
        }

        /// <summary>
        /// 다른 플레이어 정보 보기
        /// </summary>
        void OnPlayerBotInfo(UnitActor unitActor)
        {
            PlayerBotEntity entity = unitActor.Entity as PlayerBotEntity;
            int cid = entity.Character.Cid;
            int uid = entity.Uid;
            player.User.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        /// <summary>
        /// 다른 플레이어 귓속말하기
        /// </summary>
        void OnPlayerBotChat(UnitActor unitActor)
        {
            PlayerBotEntity entity = unitActor.Entity as PlayerBotEntity;
            int cid = entity.Character.Cid;
            int uid = entity.Uid;
            string nickname = entity.GetName();

            OpenWhisperUI(uid, cid, nickname);
        }

        /// <summary>
        /// 귓속말 상대 추가하고 UI 열기
        /// </summary>
        void OpenWhisperUI(int uid, int cid, string nickname)
        {
            var info = player.ChatModel.AddWhisperInfo(uid, cid, nickname);
            player.ChatModel.SetWhisperInfo(info);

            UI.Show<UIChat>()
                .Show(ChatMode.Whisper, whisperCid: cid);
        }

        /// <summary>
        /// 다른 플레이어 개인상점 보기
        /// </summary>
        /// <param name="unitActor"></param>
        void OnPlayerBotStore(UnitActor unitActor)
        {
            if (unitActor.Entity is PlayerBotEntity entity)
            {
                Timing.RunCoroutine(YieldPrivateStoreProductList(entity.Character.Cid), TAG);
            }
        }

        /// <summary>
        /// 상점 품목 리스트 조회
        /// </summary>
        IEnumerator<float> YieldPrivateStoreProductList(int sendCid)
        {
            if (uiPrivateStore.IsMyStoreSelling) // 판매중이라면 다른 유저의 상점 조회 불가.
                yield break;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", sendCid);
            Task<Response> task = Protocol.REQUEST_TRADEPRIVATE_REGLIST.SendAsync(sfs);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                yield break;
            }

            int cid = response.GetInt("1");
            PlayerBotEntity find = playerBotPool.Find(sendCid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"없는 플레이어 상점목록 요청. cid={cid}");
#endif
                yield break;
            }

            var dataList = response.GetSFSArray("2");
            string nickName = find.GetName();
            string storeComment = find.Trade.StallName;
            int storeCID = cid;
            string CIDHex = MathUtils.CidToHexCode(cid);
            PrivateStoreItemList itemList = new PrivateStoreItemList();
            for (int i = 0; i < dataList.Count; ++i)
            {
                var data = dataList.GetSFSObject(i);
                var resItemNo = data.GetLong("1");
                var resCID2 = data.GetInt("2");
                var resItemID = data.GetInt("3");
                var resItemCount = data.GetInt("4");
                var resItemTierPer = data.GetInt("5"); // [카드] 카드레벨
                var resItemLevel = data.GetByte("6"); // [장비] 장비레벨
                var resItemPos = data.GetByte("7");
                resItemPos = 0; /// Pos는 항상 0이어야 한다.
                var resCardID1 = data.GetLong("8"); // [장비] 장착된 카드, [카드] 옵션스탯
                var resCardID2 = data.GetLong("9");
                var resCardID3 = data.GetLong("10");
                var resCardID4 = data.GetLong("11");
                var resPrice = data.GetLong("12");
                var resTranscend = data.GetInt("13");
                var resElementChanged = data.ContainsKey("14") ? data.GetInt("14") : 0;
                byte index = data.GetByte("15"); // 노점 상품 고유 인덱스
                int resElementLevel = data.GetInt("16");

                PrivateStoreItemData privateItemData = new PrivateStoreItemData(resItemID);
                privateItemData.SetInfo(resItemNo, resItemCount, (int)resPrice, resItemTierPer, resItemLevel, resItemPos.ToEnum<ItemEquipmentSlotType>(), resCardID1, resCardID2, resCardID3, resCardID4, resTranscend, resElementChanged, resElementLevel);
                privateItemData.SetIndex(index);
                itemList.Add(privateItemData);
            }

            var storeData = new PrivateStoreData(itemList, storeCID, CIDHex, nickName, storeComment);
            UI.Show<UIPrivateStore>().Show(storeData);
        }

        #endregion

        #region 서버 응답 이벤트

        /// <summary>
        /// REQUEST_TRADEPRIVATE_TRANSFORM 프로토콜의 Response.
        /// 다른 유저가 이동하면 그 정보를 받아온다.
        /// </summary>
        void OnReceiveOtherPlayerMove(Response response)
        {
            int cid = response.GetInt("1");
            if (cid == 0) // 가끔씩 cid가 0으로 오는 에러메시지가 있다. 예외처리 해주자.
                return;

            UnitEntity find = playerBotPool.Find(cid);

            int[] pos_xz_int = response.GetIntArray("2");

            // 방어코드
            if (pos_xz_int == null)
                return;

            Vector3 position = new Vector3(pos_xz_int[0] * 0.001f, Constants.Map.POSITION_Y, pos_xz_int[1] * 0.001f);

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

            unitActor.Movement.SetDestination(position, useRemainThreshold: false, fixedYPos: false);
            unitActor.Appearance.PlayEmotion(false);
        }

        /// <summary>
        /// <see cref="Protocol.RESPONSE_TRADEPRIVATE_ENTERROOM"/> 프로토콜의 Response.
        /// 다른 유저가 입장하면 그 유저의 정보를 받아온다.
        /// </summary>
        void OnReceiveOtherPlayerEnter(Response res)
        {
            Response multiLobbyPlayers = new Response(res.GetSFSObject("1"));
            IMultiPlayerInput[] multiPlayers = multiLobbyPlayers.ContainsKey("in") ? multiLobbyPlayers.GetPacketArray<MultiLobbyPlayerPacket>("in") : System.Array.Empty<MultiLobbyPlayerPacket>();

            // 생성 도중이라면 생성큐 추가
            if (playerBotPool.HasQueue())
            {
                playerBotPool.EnqueueRange(multiPlayers);
                return;
            }

            foreach (var multiPlayer in multiPlayers)
            {
                // 같은 CID의 플레이어가 이미 존재한다면 제거한다.
                var existPlayerBot = playerBotPool.Find(multiPlayer.Cid);
                if (existPlayerBot != null)
                {
                    Dispose(existPlayerBot); // 제거
                    playerBotPool.Recycle(existPlayerBot); // 회수
                }

                // 즉시 생성
                if (!SpawnPlayerBot(multiPlayer))
                {
#if UNITY_EDITOR
                    int cid = multiPlayer.Cid;
                    Debug.LogError($"생성하지 못한 플레이어 봇: {nameof(cid)} = {cid}");
#endif
                }
            }

            RefreshChannelView();
        }

        /// <summary>
        /// RESPONSE_TRADEPRIVATE_EXITROOM
        /// 다른 유저가 거래소에서 퇴장하면 호출된다.
        /// </summary>
        void OnReceiveOtherPlayerLobbyExit(Response response)
        {
            int cid = response.GetInt("1");

            if (uiPrivateStore.GetCurrentCID() == cid)
            {
                UI.ShowToastPopup(LocalizeKey._90193.ToText()); // 상점이 닫혔습니다.
                uiPrivateStore.Hide();
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

            Dispose(find); // 제거
            playerBotPool.Recycle(find); // 회수

            // 인구수, 노점수 Refresh
            RefreshChannelView();
        }

        /// <summary>
        /// RESPONSE_TRADEPRIVATE_REGISTER
        /// 다른 유저가 개인상점에 아이템을 등록하면 호출된다.
        /// </summary>
        void OnReceiveOtherPlayerItemRegister(Response response)
        {
            int cid = response.GetInt("1");
            string comment = response.GetUtfString("2");
            int pos_x_int = response.GetInt("3");
            int pos_z_int = response.GetInt("4");
            Vector3 position = new Vector3(pos_x_int * 0.001f, Constants.Map.POSITION_Y, pos_z_int * 0.001f);

            PlayerBotEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue()
                    && playerBotPool.UpdateQueueMove(cid, position)
                    && playerBotPool.UpdateQueueSellingState(cid, PrivateStoreSellingState.SELLING, comment))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 상점 개설: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor)
            {
                unitActor.Movement.Warp(position);
                find.Trade.UpdateData(PrivateStoreSellingState.SELLING, comment);

                // 인구수, 노점수 Refresh
                RefreshChannelView();
            }
        }

        /// <summary>
        /// RESPONSE_TRADEPRIVATE_END
        /// 다른 유저가 개인상점을 닫으면 호출된다.
        /// </summary>
        void OnReceiveOtherPlayerStoreClose(Response response)
        {
            int cid = response.GetInt("1");

            if (uiPrivateStore.GetCurrentCID() == cid)
            {
                UI.ShowToastPopup(LocalizeKey._90193.ToText()); // 상점이 닫혔습니다.
                uiPrivateStore.Hide();
            }

            PlayerBotEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueSellingState(cid, PrivateStoreSellingState.NOT_SELLING, string.Empty))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 상점 닫기: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.Trade.UpdateData(PrivateStoreSellingState.NOT_SELLING, string.Empty);

            // 인구수, 노점 수 변경 이벤트
            RefreshChannelView();
        }

        /// <summary>
        /// <see cref="Protocol.REQUEST_TRADEPRIVATE_WHISPER"/>
        /// 다른 유저의 개인상점 귓속말을 받았을 때.
        /// </summary>
        /// <param name="response"></param>
        void OnReceiveChat_PrivateWhisper(Response response)
        {
            int speakerCID = response.GetInt("1");
            string speakerNickname = response.GetUtfString("2");
            string msg = response.GetUtfString("3");
            //int speakerUserID = response.GetInt("4"); // 계정아이디
            byte jobID = response.GetByte("4");
            short jobLevel = response.GetShort("5");

            // 에러 방지 (내 전송 프로토콜의 success가 여기로 들어온다.)
            if (speakerCID == 0)
                return;

            /// TODO: 귓속말 채팅 내용 띄우기.

            /// TODO: 임시 코드
            //chatModel.OnTradeShopChat(speakerCID, speakerNickname, $"[귓말{speakerCID}] {msg}");
        }

        void OnLobbyChat(ChatInfo info)
        {
            bool isPlayer = info.cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(info.cid);

            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                //if (playerBotPool.HasQueue())
                //    return;
                return;
            }

            UnitActor unitActor = find.GetActor();

            if (!unitActor)
                return;

            unitActor.EffectPlayer.ShowLobbyChatBallon(info.message);
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

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            actor.Movement.Stop();
            actor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
            if (position == Vector2.zero)
                return;

            // 상점 개설 중일 시, 움직이지 않음
            if (tradeModel.SellingState == PrivateStoreSellingState.SELLING)
                return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

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

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();

            SendPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        void OnExitButton()
        {
            impl.ExitBattle();
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
            PlayEmotion(player, type); // 이모션 표시

            // 이모션 전송
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", (int)RoomType.Lobby);
            sfs.PutInt("2", (int)type);
            Protocol.REQUEST_EMOTION.SendAsync(sfs).WrapNetworkErrors();
        }

        #endregion      

        /// <summary>
        /// 현재 위치 서버로 보내기
        /// </summary>
        private void SendPlayerCurrentPosition()
        {
            savedPlayerPosition = player.LastPosition;

            var sfs = Protocol.NewInstance();
            int[] posArray = new int[2] { (int)(savedPlayerPosition.x * 1000), (int)(savedPlayerPosition.z * 1000) };
            sfs.PutIntArray("2", posArray);
            Protocol.REQUEST_TRADEPRIVATE_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
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

        private bool IsLobbyNpc(NpcType npcType)
        {
            switch (npcType)
            {
                case NpcType.Tailing:
                case NpcType.Sorin:
                case NpcType.Sortie:
                    return true;

                case NpcType.Nyankun:
                    return BasisType.NABIHO_OPEN_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId()) == 0;
            }

            return false;
        }

        /// <summary>
        /// Npc 찾기
        /// </summary>
        private NpcSpawner GetNpcSpawner(NpcType type)
        {
            for (int i = 0; i < npcBuffer.size; i++)
            {
                if (npcBuffer[i].GetNpcType() == type)
                    return npcBuffer[i];
            }

            return null;
        }

        private void ShowNpcTargetingArrow()
        {
            KafraCompleteType kafraCompleteType = questModel.KafraCompleteType;

            UnitActor actor = player.GetActor();
            if (!actor)
                return;

            actor.EffectPlayer.ReleaseNpcTargetingArrow();

            if (kafraCompleteType != KafraCompleteType.InProgress)
                return;

            NpcSpawner npcSpawner = GetNpcSpawner(NpcType.Sortie);
            if (!npcSpawner)
                return;

            actor.EffectPlayer.ShowNpcTargetingArrow(npcSpawner.transform);
        }

        private void ShowNpcTalk()
        {
            Timing.RunCoroutine(YieldShowNpcSorinTalk(questModel.CurKafraType, questModel.KafraCompleteType), TAG);
        }

        private IEnumerator<float> YieldShowNpcSorinTalk(KafraType kafraType, KafraCompleteType kafraCompleteType)
        {
            if (kafraCompleteType != KafraCompleteType.InProgress)
                yield break;

            if (kafraType == KafraType.RoPoint)
            {
                // 카프라 기념 물건이네요!\n상태가 괜찮은데요?
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORIN, LocalizeKey._19526.ToText()), TAG);
                // 소티에게 가져가 보세요.\n분명 종아할 거예요.
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORIN, LocalizeKey._19527.ToText()), TAG);
            }
            else
            {
                // 서신을 전달해주셔서 고마워요.\n내용을 보니 물류 센터에 급하게 전달할 물건이 생겼어요.
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORIN, LocalizeKey._19528.ToText()), TAG);
                // 도움을 요청해도 될까요?\n소티에게 이 영수증을 전해주세요.\n고맙습니다!
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORIN, LocalizeKey._19529.ToText()), TAG);
            }
        }

        public void GoToNpc(NpcType npcType)
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
            {
                Debug.LogError("PlayerActor 가 음슴");
                return;
            }

            NpcSpawner spawner = GetNpcSpawner(npcType);
            if (spawner == null)
            {
                Debug.LogError($"NpcSpawner 가 음슴: {nameof(npcType)} = {npcType}");
                return;
            }

            const float DISTANCE = 3f;
            Vector3 playerPos = player.LastPosition;
            Vector3 npcPos = spawner.transform.position;
            Vector3 direction = npcPos - playerPos;
            Vector3 dest = Vector3.MoveTowards(npcPos, playerPos, DISTANCE);
            actor.Movement.ForceSetDestination(dest);
            actor.CachedTransform.rotation = Quaternion.LookRotation(direction);
        }

        public void RequestKafraDeliveryReward()
        {
            Timing.RunCoroutine(YieldRequestKafraDeliveryReward(questModel.CurKafraType, questModel.KafraCompleteType), TAG);
        }

        private IEnumerator<float> YieldShowNpcSortieTalk(KafraType kafraType, KafraCompleteType kafraCompleteType)
        {
            if (kafraCompleteType == KafraCompleteType.InProgress)
            {
                if (kafraType == KafraType.RoPoint)
                {
                    // 보여주실 게 있다고요?\n어! 이거 제가 찾던 거에요!
                    yield return Timing.WaitUntilDone(YieldDialog(Npc.SORTIE, LocalizeKey._19517.ToText()), TAG);
                    // 이 보석이랑 바꾸지 않을래요?\n소린에게 가져가면 원하는 걸로 교환해 줄 거에요.
                    yield return Timing.WaitUntilDone(YieldDialog(Npc.SORTIE, LocalizeKey._19518.ToText()), TAG);
                }
                else
                {
                    // 이 물건은 뭐죠?
                    yield return Timing.WaitUntilDone(YieldDialog(Npc.SORTIE, LocalizeKey._19519.ToText()), TAG);
                    // 기다리던 물건이 왔네요. 감사합니다.\n소린에게도 잘 받았다고 전해주세요.
                    yield return Timing.WaitUntilDone(YieldDialog(Npc.SORTIE, LocalizeKey._19520.ToText()), TAG);
                }

                // 카프라 보상 수락
                questModel.RequestKafraDeliveryAccept().WrapNetworkErrors();
            }
            else
            {
                // 무슨 일이세요?
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORTIE, LocalizeKey._19515.ToText()), TAG);
                // 그냥 본 거라고요?\n다음에 꼭 저희 물류 서비스를 이용해주세요~
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORTIE, LocalizeKey._19516.ToText()), TAG);
            }
        }

        private IEnumerator<float> YieldRequestKafraDeliveryReward(KafraType kafraType, KafraCompleteType kafraCompleteType)
        {
            if (kafraType == KafraType.Exchange)
                yield break;

            if (kafraCompleteType != KafraCompleteType.StandByReward)
                yield break;

            if (kafraType == KafraType.RoPoint)
            {
                // 좋은 보석이네요!\n아까 거래소 재화가 필요하시다고 하셨죠?\n보석이랑 교환해드릴게요~
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORIN, LocalizeKey._19521.ToText()), TAG);
            }
            else
            {
                // 도와주셔서 고마워요.\n감사의 마음을 담아 선물을 준비했어요.
                yield return Timing.WaitUntilDone(YieldDialog(Npc.SORIN, LocalizeKey._19522.ToText()), TAG);
            }

            // 카프라 보상 받기
            questModel.RequestKafraDeliveryReward().WrapNetworkErrors();
        }

        private IEnumerator<float> YieldDialog(Npc npc, string dialog)
        {
            uiDialogue.Show();
            uiDialogue.SetActiveBtnSkip(false);
            string[] dialogs = dialog.Split(Constants.NPC.NPC_DIALOG_DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
            yield return uiDialogue.UntilShow(npc, dialogs);
            uiDialogue.Hide();
        }
    }
}