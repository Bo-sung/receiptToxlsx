using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GuildLobbyEntry : BattleEntry
    {
        private const string SCENE_NAME = "Ayothaya";
        private const string BGM_NAME = "Ayotaya";

        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;
        private const float MIN_POSITION_SYNC_THRESHOLD = 0.35f;

        // <!-- Models --!>
        private readonly ChatModel chatModel;
        private readonly GuildModel guildModel;

        private readonly IHUDPool hudPool;

        // <!-- Managers --!>
        private readonly ConnectionManager connectionManager;
        private readonly GuildSquareManager guildSquareManager;

        // <!-- Scene Compositions --!>
        private readonly PlayerBotCreateQueue playerBotCreateQueue; // 플레이어봇 생성 대기열

        // <!-- UIs --!>
        private UIMain uiMain;
        private UIMainTop uiMainTop;
        private UIMainShortcut uiMainShortcut;
        private UIController uiController;
        private UIGuildLobby uiGuildLobby;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UIAutoEquip uiAutoEquip;
        private UICamSelect uiCamSelect;
        private UIQuickExpandMenu uiQuickExpandMenu;
        private UISimpleQuest uiSimpleQuest;
        private UIEmotion uiEmotion;
        private UIShadowPlayerSettings uiShadowPlayerSettings;
        private UIJobChangeMenu uiJobChangeMenu;

        public GuildLobbyEntry() : base(BattleMode.GuildLobby)
        {
            player.SetState(UnitEntity.UnitState.GVG);

            chatModel = player.ChatModel;
            guildModel = player.Guild;

            connectionManager = ConnectionManager.Instance;
            guildSquareManager = GuildSquareManager.Instance;

            hudPool = HUDPoolManager.Instance;

            playerBotCreateQueue = new PlayerBotCreateQueue();
        }

        protected override void Dispose()
        {
            base.Dispose();

            playerBotCreateQueue.Clear(); // 플레이어봇 생성 대기열 삭제
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            guildSquareManager.OnPlayerMove += OnPlayerMove;
            guildSquareManager.OnMultiPlayerMove += OnMultiPlayerMove;

            guildSquareManager.OnMultiPlayerJoin += OnMultiPlayerJoin;
            guildSquareManager.OnMultiPlayerExit += OnMultiPlayerExit;
            guildSquareManager.OnPlayerGuildOut += ExitEntry;
            guildSquareManager.OnUpdateSquareState += OnUpdateSquareState;

            player.OnSpawnActor += OnPlayerSpawn;
            player.OnDespawnActor += OnPlayerDespawn;

            chatModel.SetChatMode(ChatMode.Guild);
            chatModel.OnGuildChat += OnGuildChat;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            guildSquareManager.OnPlayerMove -= OnPlayerMove;
            guildSquareManager.OnMultiPlayerMove -= OnMultiPlayerMove;

            guildSquareManager.OnMultiPlayerJoin -= OnMultiPlayerJoin;
            guildSquareManager.OnMultiPlayerExit -= OnMultiPlayerExit;
            guildSquareManager.OnPlayerGuildOut -= ExitEntry;
            guildSquareManager.OnUpdateSquareState -= OnUpdateSquareState;

            player.OnSpawnActor -= OnPlayerSpawn;
            player.OnDespawnActor -= OnPlayerDespawn;

            chatModel.SetChatMode(ChatMode.Channel);
            chatModel.OnGuildChat -= OnGuildChat;
        }

        protected override void AddProtocalEvent()
        {
            // 다른 유저의 이모션
            Protocol.REQUEST_EMOTION.AddEvent(OnReceiveEmotion);
        }

        protected override void RemoveProtocolEvent()
        {
            // 다른 유저의 이모션
            Protocol.REQUEST_EMOTION.RemoveEvent(OnReceiveEmotion);
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMain = UI.Show<UIMain>();
            uiMainTop = UI.Show<UIMainTop>();
            uiMainShortcut = UI.Show<UIMainShortcut>();
            uiGuildLobby = UI.Show<UIGuildLobby>();
            uiController = UI.Show<UIController>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiAutoEquip = UI.Show<UIAutoEquip>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiQuickExpandMenu = UI.Show<UIQuickExpandMenu>();
            uiSimpleQuest = UI.Show<UISimpleQuest>();
            uiEmotion = UI.Show<UIEmotion>();
            uiShadowPlayerSettings = UI.Show<UIShadowPlayerSettings>();
            uiJobChangeMenu = UI.Show<UIJobChangeMenu>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += ExitEntry;
            uiQuickExpandMenu.OnSlotInit += OnExpandMenuSlotInit;
            uiEmotion.OnEmotion += OnEmotionPlayer;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(true);
            uiController.SetActive(true); // 컨트롤러 제어
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit, UIBattleMenu.MenuContent.Cupet, UIBattleMenu.MenuContent.GuildAgit, UIBattleMenu.MenuContent.Square, UIBattleMenu.MenuContent.JobLevel, UIBattleMenu.MenuContent.FirstPayment, UIBattleMenu.MenuContent.CustomerReward); // 나가기, 큐펫, 길드아지트
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiQuickExpandMenu.SetShareNoticeMode(isShareNoticeMode: false);

            cameraController.SetView(CameraController.View.Quater40_5); // Quater로 전환
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= ExitEntry;
            uiQuickExpandMenu.OnSlotInit -= OnExpandMenuSlotInit;
            uiEmotion.OnEmotion -= OnEmotionPlayer;

            UI.Close<UIMain>();
            UI.Close<UIMainTop>();
            UI.Close<UIMainShortcut>();
            UI.Close<UIGuildLobby>();
            UI.Close<UIController>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIAutoEquip>();
            UI.Close<UICamSelect>();
            UI.Close<UIQuickExpandMenu>();
            UI.Close<UISimpleQuest>();
            UI.Close<UIEmotion>();
            UI.Close<UIShadowPlayerSettings>();
            UI.Close<UIJobChangeMenu>();

            uiMain = null;
            uiMainTop = null;
            uiMainShortcut = null;
            uiGuildLobby = null;
            uiController = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiAutoEquip = null;
            uiCamSelect = null;
            uiQuickExpandMenu = null;
            uiSimpleQuest = null;
            uiEmotion = null;
            uiShadowPlayerSettings = null;
            uiJobChangeMenu = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            string mainText = LocalizeKey._38001.ToText(); // 길드 스퀘어
            uiBattleInfo.Set(mainText);
        }

        /// <summary>
        /// 나가기
        /// </summary>
        protected override void ExitEntry()
        {
            base.ExitEntry();

            UI.Close<UIGuildShop>();
            UI.Close<UIGuildTaming>();
            UI.Close<UICupet>();
        }

        /// <summary>
        /// 해제
        /// </summary>
        protected override void Dispose(UnitEntity entity)
        {
            base.Dispose(entity);

            entity.ResetExtraOption(ExtraBattleOptionType.MaxHp);
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            // 이미 퇴장한 상태
            if (!guildSquareManager.IsJoined())
            {
                IsSuccessExit = true;
                yield break;
            }

            Task<bool> task = guildSquareManager.Exit();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            IsSuccessExit = task.Result;
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            playerBotCreateQueue.Ready();

            // 이미 입장한 상태
            if (guildSquareManager.IsJoined())
            {
                IsSuccessEnter = true;
                yield break;
            }

            Task<bool> task = guildSquareManager.Enter();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            IsSuccessEnter = task.Result;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            if (ChangeState())
                return;

            string sceneName = GetSceneName(); // 씬 이름
            string bgmName = BGM_NAME; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비

            playerBotCreateQueue.Enqueue(guildSquareManager.GetPlayers()); // 플레이어 Queue 시작
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
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (GameObject root in roots)
            {
                NpcSpawner[] find = root.GetComponentsInChildren<NpcSpawner>();
                foreach (NpcSpawner spawner in find)
                {
                    if (!IsGuildNpc(spawner.GetNpcType()))
                        continue;

                    spawner.Spawn(); // npc 소환
                }
            }
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG); // 플레이어 Spawn
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            if (ChangeState())
                return;

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG); // 길드원 Spawn

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                playerActor.EffectPlayer.ShowUnitCircle();
                playerActor.AI.EndAI();
            }
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        /// <summary>
        /// 플레이어 생성 이벤트
        /// </summary>
        void OnPlayerSpawn(UnitActor unitActor)
        {
            unitActor.Movement.SetAvoidancePriority(Constants.Movement.PLAYER_AVOIDANCE_PRIORITY);
            unitActor.Movement.SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance);

            unitActor.Movement.OnPortal += OnPortal;
            unitActor.Movement.OnPortalOut += OnPortalOut;
        }

        /// <summary>
        /// 플레이어 제거 이벤트
        /// </summary>
        void OnPlayerDespawn(UnitActor unitActor)
        {
            unitActor.Movement.SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance);

            unitActor.Movement.OnPortal -= OnPortal;
            unitActor.Movement.OnPortalOut -= OnPortalOut;
        }

        /// <summary>
        /// 채팅 이벤트
        /// </summary>
        void OnGuildChat(ChatInfo info)
        {
            UnitEntity find = guildSquareManager.FindPlayer(info.cid);

            if (find == null)
                return;

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.EffectPlayer.ShowLobbyChatBallon(info.message);
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
                case NpcType.W:
                    UI.Show<UIGuildShop>();
                    break;

                case NpcType.Tamami:
                    UI.Show<UIGuildTaming>();
                    break;

                case NpcType.Emperium:
                    UI.Show<UIGuildAttack>();
                    break;

                case NpcType.Guard:
                    guildModel.RequestGuildBattleSeasonInfo().WrapNetworkErrors();
                    break;

                case NpcType.SpecialAgent:
                case NpcType.SecretAgent:
                    UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
                    break;
            }
        }

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

            Vector3 dist = player.LastPosition - guildSquareManager.PlayerPosition;
            if (dist.sqrMagnitude < SEND_POSITION_SQR_MAGNITUDE)
                return;

            RequestPlayerCurrentPosition(); // 플레이어 위치 동기화
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

            RequestPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        /// <summary>
        /// ExpandMenu Slot SetData 이벤트
        /// </summary>
        void OnExpandMenuSlotInit(int index)
        {
            // 무조건 부활상태, 풀피로 만들어주기.
            uiQuickExpandMenu.SetShareCharacterReviveTime(index, 0f);
            uiQuickExpandMenu.SetShareCharacterCurrentHp(index, 1, 1, skipAnim: true);
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            if (!impl.Add(player, isEnemy: false))
                yield break;

            UnitActor unitActor = player.SpawnActor(); // 유닛 소환
            unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MOVE_SPEED);

            Vector3 pos = guildSquareManager.PlayerPosition;
            Vector3 position = FindClosestEdge(pos);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

            cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
            yield return Timing.WaitForOneFrame;
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayerBots()
        {
            while (playerBotCreateQueue.HasQueue())
            {
                if (!SpawnPlayerBot(playerBotCreateQueue.Dequeue()))
                    continue;

                yield return Timing.WaitForOneFrame;
            }

            playerBotCreateQueue.Finish(); // 생성 완료
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(PlayerBotEntity entity)
        {
            if (!impl.Add(entity, isEnemy: true))
                return false;

            entity.OnSpawnActor += OnPlayerBotSpawn;
            entity.OnDespawnActor += OnPlayerBotDespawn;

            UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
            unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MOVE_SPEED);
            unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);
            unitActor.Radar.SetSenseDistance(4);
            unitActor.Radar.SetSenseTargetType(TargetType.EnemyCharacter);

            Vector3 position = FindClosestEdge(entity.BotPosition);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프
            unitActor.AI.ReadyToBattle(); // OnAllReady 이후에 생성하므로 생성과 동시에 ReadyToBattle 호출

            return true;
        }

        /// <summary>
        /// 현재 위치 서버로 보내기
        /// </summary>
        private void RequestPlayerCurrentPosition()
        {
            guildSquareManager.RequestPlayerMove(player.LastPosition);
        }

        /// <summary>
        /// 플레이어 움직임
        /// </summary>
        void OnPlayerMove()
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.Movement.ForceWarp(guildSquareManager.PlayerPosition);
        }

        /// <summary>
        /// 플레이어 봇 움직임
        /// </summary>
        void OnMultiPlayerMove(PlayerBotEntity entity)
        {
            UnitActor actor = entity.GetActor();
            if (actor == null)
                return;

            actor.Movement.ForceSetDestination(entity.BotPosition);
            actor.Appearance.PlayEmotion(false);
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        void OnMultiPlayerJoin(PlayerBotEntity entity)
        {
            // 생성 중
            if (playerBotCreateQueue.IsInProgress)
            {
                playerBotCreateQueue.Enqueue(entity);
                return;
            }

            SpawnPlayerBot(entity);
        }

        /// <summary>
        /// 플레이어 봇 퇴장
        /// </summary>
        void OnMultiPlayerExit(PlayerBotEntity entity)
        {
            // 생성 중
            if (playerBotCreateQueue.IsInProgress)
            {
                playerBotCreateQueue.Remove(entity);
                return;
            }

            UnitActor actor = entity.GetActor();
            if (actor)
                actor.EffectPlayer.SpawnUnitTeleport();

            entity.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            Dispose(entity);
        }

        void OnPlayerBotSpawn(UnitActor unitActor)
        {
            unitActor.EffectPlayer.SetUnitInfo();
            unitActor.EffectPlayer.SetStoreBalloon();
            unitActor.EffectPlayer.ShowShadow(); // 그림자 생성

            unitActor.Radar.OnTargetActor += OnPlayerBotTarget;
            unitActor.EffectPlayer.OnInfo += OnPlayerBotInfo;
            unitActor.EffectPlayer.OnChat += OnPlayerBotChat;
        }

        void OnPlayerBotDespawn(UnitActor unitActor)
        {
            unitActor.Radar.OnTargetActor -= OnPlayerBotTarget;
            unitActor.EffectPlayer.OnInfo -= OnPlayerBotInfo;
            unitActor.EffectPlayer.OnChat -= OnPlayerBotChat;

            unitActor.Entity.OnSpawnActor -= OnPlayerBotSpawn;
            unitActor.Entity.OnDespawnActor -= OnPlayerBotDespawn;
        }

        void OnPlayerBotTarget(UnitActor target, UnitActor unitActor)
        {
            if (target == null)
            {
                unitActor.EffectPlayer.HideBattleHUD();
            }
            else
            {
                unitActor.EffectPlayer.ShowBattleHUD();
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

            // 귓속말 상대 추가하고 UI 열기
            var info = player.ChatModel.AddWhisperInfo(uid, cid, nickname);
            player.ChatModel.SetWhisperInfo(info);
            UI.Show<UIChat>().Show(ChatMode.Whisper, whisperCid: cid);
        }

        void OnUpdateSquareState()
        {
            ChangeState();
        }

        private bool ChangeState()
        {
            if (guildSquareManager.State == GuildSquareState.GuildAttack)
            {
                IsSkipExit = true; // 나가기 처리 skip
                impl.StartBattle(BattleMode.GuildAttack); // 바로 GuidAttack 으로 이동
                return true;
            }

            return false;
        }

        void OnReceiveEmotion(Response response)
        {
            int cid = response.GetInt("1");
            int emotionType = response.GetInt("2");
            CharacterEntity find = guildSquareManager.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어는 무시");
#endif
                return;
            }

            PlayEmotion(find, emotionType.ToEnum<EmotionType>()); // 이모션 표시
        }

        /// <summary>
        /// 이모션 표시
        /// </summary>
        private void OnEmotionPlayer(EmotionType type)
        {
            PlayEmotion(player, type); // 이모션 표시

            // 이모션 전송
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", (int)RoomType.Guild);
            sfs.PutInt("2", (int)type);
            Protocol.REQUEST_EMOTION.SendAsync(sfs).WrapNetworkErrors();
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

        private bool IsGuildNpc(NpcType npcType)
        {
            switch (npcType)
            {
                case NpcType.W:
                case NpcType.Tamami:
                case NpcType.Emperium:
                case NpcType.SpecialAgent:
                case NpcType.SecretAgent:
                case NpcType.Guard:
                    return true;
            }

            return false;
        }
    }
}