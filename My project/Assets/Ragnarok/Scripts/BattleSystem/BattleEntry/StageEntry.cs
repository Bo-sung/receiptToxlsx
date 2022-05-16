//#define SPAWN_AGENT_CHARACTER
using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class StageEntry : BattleEntry
    {
        private const string TAG_REVIVE = "StageEntry_TAG_REVIVE";
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;

        private const int CHARACTER_PLUS_MOVE_SPEED = 1000; // 강제 이속 증가량

        /// <summary>
        /// 위치가 10 미만의 경우 어셈블 가능
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 10f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;

        /// <summary>
        /// mvp 몬스터와의 간격
        /// </summary>
        private const float WARP_TO_MVP_MONSTER_PADDING = 2f;

        /// <summary>
        /// 플레이어와의 간격 (셰어링 캐릭터)
        /// </summary>
        private const float WARP_TO_PLAYER_PADDING = 4f;

        /// <summary>
        /// 몬스터 처지시 회복되는 MP량
        /// </summary>
        private const int RECOVERY_MP = 1;

        /// <summary>
        /// Mvp 몬스터 보정 시간 (10초 만큼 깎아서 보여준다)
        /// </summary>
        private const int MVP_MINUS_TIME = 10_000;

        private enum SummonMonsterType
        {
            /// <summary>
            /// 보스몬스터 소환 가능
            /// </summary>
            BossMonster,
            /// <summary>
            /// Mvp몬스터 소환 가능
            /// </summary>
            MvpMonster,
        }

        private enum ResultType
        {
            None,
            BossClear,
            PlayerDead,
        }

        /******************** Models ********************/
        private readonly DungeonModel dungeonModel;
        private readonly SharingModel sharingModel;
#if SPAWN_AGENT_CHARACTER
        private readonly AgentModel agentModel;
#endif
        private readonly CharacterModel characterModel;
        private readonly StatusModel statusModel;
        private readonly SkillModel skillModel;
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;
        private readonly TutorialModel tutorialModel;

        /******************** Repositories ********************/
        private readonly StageDataManager stageDataRepo;
        private readonly MvpDataManager mvpDataRepo;
        private readonly MonsterDataManager monsterDataRepo;
        private readonly JobDataManager jobDataRepo;
        private readonly AgentDataManager agentDataRepo;

        /******************** Managers ********************/
        private readonly SoundManager soundManager;
        private readonly UIManager uiManager;
        private readonly ConnectionManager connectionManager;

        /******************** Scene Compositions ********************/
        private readonly Buffer<UnitEntity> monsterBuffer;
        private readonly Dictionary<int, UnitEntity[]> monsterZoneDic; // 각 SpawnZone의 몬스터List
        private readonly GhostBattlePlayers sharingCharacterFactory;
        private readonly GhostBattlePlayers cloneCharacterFactory;
        private readonly Dictionary<SharingModel.CloneCharacterType, GhostPlayerEntity> cloneDic;
#if SPAWN_AGENT_CHARACTER
        private readonly GhostBattlePlayers agentCharacterFactory;
        private readonly Buffer<AgentCharacterPacket> agentPacketBuffer;
#endif
        private readonly float reviveDelayTime = 4f;

        /******************** UIs ********************/
        private UIMain uiMain;
        private UIMainTop uiMainTop;
        private UIMainShortcut uiMainShortcut;
        private UIController uiController;
        private UIDanger uiDanger;
        private UIAutoEquip uiAutoEquip;
        private UIWorldBossAlarm uiWorldBossAlarm;
        private UIGoodsView uiGoodsView;
        private UISimpleQuest uiSimpleQuest;
        private UIQuickExpandMenu uiQuickExpandMenu;
        private UIMvpTimer uiMvpTimer;
        private UIWarning uiWarning;
        private UIBossComing uiBossComing;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleFail uiBattleFail;
        private UIBattleFailBoss uiBattleFailBoss;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattleStageMenu battleStageMenu;
        private UIJobChangeMenu uiJobChangeMenu;
        private UISkillListToggle uiSkillListToggle;
        private UICamSelect uiCamSelect;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIResultClear uiResultClear;
        private UIStageInfoView uiStageInfoView;
        private UIMvpDuelResult uiMvpDuelResult;
        private UIChatPreview uiChatPreview;
        private UIPowerSavingMenu uiPowerSavingMenu;

        /******************** Temp Data ********************/
        private SummonMonsterType summonMonsterType; // 소환 몬스터 여부
        private UnitEntity summonMonster; // 소환된 몬스터 (Mvp 및 Boss)
        private Map map; // 맵
        private StageData currentData; // 현재 진행중인 스테이지 데이터
        private bool isAssemble;
        private bool isClearedBoss;
        private bool isShowStageInfo;
        private bool isSceneReady;
        private ResultType resultType;

        private RewardData[] registeredBossRewards;
        private CharacterEntity selectedEntity; // 조종 중인 캐릭터

        public static bool hasSummonMonster;
        private bool isSummoningMvp; // MVP 소환중 플래그
        private bool isMvpTutorial; // mvp튜토리얼 중
        private bool isRecoonect;

        public StageEntry() : base(BattleMode.Stage)
        {
            player.SetState(UnitEntity.UnitState.Stage);

            dungeonModel = player.Dungeon;
            sharingModel = player.Sharing;
#if SPAWN_AGENT_CHARACTER
            agentModel = player.Agent;
#endif
            characterModel = player.Character;
            statusModel = player.Status;
            skillModel = player.Skill;
            inventoryModel = player.Inventory;
            questModel = player.Quest;
            tutorialModel = player.Tutorial;

            stageDataRepo = StageDataManager.Instance;
            mvpDataRepo = MvpDataManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
            jobDataRepo = JobDataManager.Instance;
            agentDataRepo = AgentDataManager.Instance;
            soundManager = SoundManager.Instance;
            uiManager = UIManager.Instance;
            connectionManager = ConnectionManager.Instance;

            monsterBuffer = new Buffer<UnitEntity>();
            monsterZoneDic = new Dictionary<int, UnitEntity[]>(IntEqualityComparer.Default);

            sharingCharacterFactory = new GhostBattlePlayers();
            cloneCharacterFactory = new GhostBattlePlayers();
            cloneDic = new Dictionary<SharingModel.CloneCharacterType, GhostPlayerEntity>(sharingModel);

#if SPAWN_AGENT_CHARACTER
            agentCharacterFactory = new GhostBattlePlayers();
            agentPacketBuffer = new Buffer<AgentCharacterPacket>();
#endif
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            currentData = null;

            sharingCharacterFactory.Clear();
            cloneCharacterFactory.Clear();
            cloneDic.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.PlusMoveSpd); // 강제이속증가 되돌리기
            player.SetForceStatus(ForceStatusType.BuffItemOptionOff); // 버프아이템 Off

            skillModel.SetAntiSkillAuto(false); // 스킬 Auto On

#if SPAWN_AGENT_CHARACTER
            agentCharacterFactory.Clear();
            agentPacketBuffer.Clear();
#endif

            // 재접속이 이뤄지면서 Dispose 되었을 경우 스테이지 모드 초기화
            if (isRecoonect)
            {
                dungeonModel.ResetStageMode();
            }
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            dungeonModel.OnMonsterItemDrop += OnMonsterItemDrop;
            dungeonModel.OnMvpMonsterItemDrop += OnMvpMonsterItemDrop;
            dungeonModel.OnBossMonsterItemDrop += OnBossMonsterItemDrop;
            dungeonModel.OnAppearMvpMonster += OnAppearMvpMonster;
            dungeonModel.OnAppearBossMonster += OnAppearBossMonster;
            dungeonModel.OnDropCube += OnDropCube;
            sharingModel.OnUpdateSharingState += Restart;
            sharingModel.OnAddSharingCharacters += AddSharingCharacters;
            sharingModel.OnRemoveSharingCharacter += RemoveSharingCharacter;
            sharingModel.OnClearSharingCharacters += DisposeSharingCharacters;
            sharingModel.OnAddCloneCharacter += AddCloneCharacter;
            sharingModel.OnRemoveCloneCharacter += RemoveCloneCharacter;
            skillModel.OnChangeSkillStatus += UpdateMazeSkillList;
            skillModel.OnUpdateSkillSlot += UpdateMazeSkillList;
            inventoryModel.OnUpdateEquipment += UpdateMazeSkillList;
            player.OnReloadStatus += UpdateMazeSkillList;
            characterModel.OnChangedJob += OnChangedJob;
            questModel.OnUpdateMainQuest += OnUpdateGuideQuest;
            connectionManager.OnReconnect += OnReconnect;

            Tutorial.OnFinish += OnFinishedTutorial;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            dungeonModel.OnMonsterItemDrop -= OnMonsterItemDrop;
            dungeonModel.OnMvpMonsterItemDrop -= OnMvpMonsterItemDrop;
            dungeonModel.OnBossMonsterItemDrop -= OnBossMonsterItemDrop;
            dungeonModel.OnAppearMvpMonster -= OnAppearMvpMonster;
            dungeonModel.OnAppearBossMonster -= OnAppearBossMonster;
            dungeonModel.OnDropCube -= OnDropCube;
            sharingModel.OnUpdateSharingState -= Restart;
            sharingModel.OnAddSharingCharacters -= AddSharingCharacters;
            sharingModel.OnRemoveSharingCharacter -= RemoveSharingCharacter;
            sharingModel.OnClearSharingCharacters -= DisposeSharingCharacters;
            sharingModel.OnAddCloneCharacter -= AddCloneCharacter;
            sharingModel.OnRemoveCloneCharacter -= RemoveCloneCharacter;
            player.OnReloadStatus -= UpdateMazeSkillList;
            skillModel.OnChangeSkillStatus -= UpdateMazeSkillList;
            skillModel.OnUpdateSkillSlot -= UpdateMazeSkillList;
            inventoryModel.OnUpdateEquipment -= UpdateMazeSkillList;
            player.OnReloadStatus -= UpdateMazeSkillList;
            characterModel.OnChangedJob -= OnChangedJob;
            questModel.OnUpdateMainQuest -= OnUpdateGuideQuest;
            connectionManager.OnReconnect -= OnReconnect;

            Tutorial.OnFinish -= OnFinishedTutorial;
        }

        public override void OnBack()
        {
            if (uiResultClear.IsVisible)
            {
                uiResultClear.OnClickedBtnConfirm();
                return;
            }

            base.OnBack();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMain = UI.Show<UIMain>();
            uiMainTop = UI.Show<UIMainTop>();
            uiMainShortcut = UI.Show<UIMainShortcut>();
            uiController = UI.Show<UIController>();
            uiDanger = UI.Show<UIDanger>();
            uiAutoEquip = UI.Show<UIAutoEquip>();
            uiWorldBossAlarm = UI.Show<UIWorldBossAlarm>();
            uiGoodsView = UI.Show<UIGoodsView>();
            uiSimpleQuest = UI.Show<UISimpleQuest>();
            uiQuickExpandMenu = UI.Show<UIQuickExpandMenu>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiWarning = UI.Show<UIWarning>();
            uiBossComing = UI.Show<UIBossComing>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleFail = UI.Show<UIBattleFail>();
            uiBattleFailBoss = UI.Show<UIBattleFailBoss>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            battleStageMenu = UI.Show<UIBattleStageMenu>();
            uiJobChangeMenu = UI.Show<UIJobChangeMenu>();
            uiSkillListToggle = UI.Show<UISkillListToggle>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiResultClear = UI.Show<UIResultClear>();
            uiStageInfoView = UI.Show<UIStageInfoView>();
            uiMvpDuelResult = UI.Show<UIMvpDuelResult>();
            uiChatPreview = UI.Show<UIChatPreview>();
            uiPowerSavingMenu = UI.Show<UIPowerSavingMenu>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleInfo.OnClickedBattleInfo += ShowDetailBattleInfo;
            uiBattleFail.OnConfirm += RebirthPlayer;
            uiBattleFailBoss.OnConfirm += RebirthPlayer;
            uiMvpTimer.OnFinish += OnTimerFinish;
            battleStageMenu.OnSelectMvpSummon += OnSelectMvpSummon;
            battleStageMenu.OnSelectBossSummon += OnSelectBossSummon;
            battleStageMenu.OnSelectAssemble += OnSelectAssemble;
            uiSkillListToggle.OnSelectToggle += OnSelectSkillToggle;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnSelectSkillToggle;
            uiResultClear.OnFinishDungeon += OnFinishDungeon;
            uiQuickExpandMenu.OnClickItem += OnSelectSharingCharacter;
            uiQuickExpandMenu.OnSlotInit += OnExpandMenuSlotInit;

            uiManager.OnUIShow += OnShowUI;

            cameraController.SetView(DEFAULT_VIEW); // Quater로 전환

            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(true);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Buff, UIBattleMenu.MenuContent.Maze, UIBattleMenu.MenuContent.Square, UIBattleMenu.MenuContent.Duel, UIBattleMenu.MenuContent.Explore, UIBattleMenu.MenuContent.Roulette, UIBattleMenu.MenuContent.JobLevel, UIBattleMenu.MenuContent.FirstPayment, UIBattleMenu.MenuContent.CustomerReward);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattleFail.Hide();
            uiBattleFailBoss.Hide();
            uiMvpTimer.Hide();
            uiBattleBossHp.SetOffset();
            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.Hide();
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            battleStageMenu.SetActiveAssemble(false);
            uiBattleMazeSkillList.Hide();
            uiStageInfoView.Hide();
            uiMvpDuelResult.Hide();
            uiQuickExpandMenu.SetShareNoticeMode(isShareNoticeMode: true);
            uiResultClear.Hide();
            uiWarning.Initialize(LocalizeKey._90105); // MVP 등장!
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiManager.OnUIShow -= OnShowUI;

            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleInfo.OnClickedBattleInfo -= ShowDetailBattleInfo;
            uiBattleFail.OnConfirm -= RebirthPlayer;
            uiBattleFailBoss.OnConfirm -= RebirthPlayer;
            uiMvpTimer.OnFinish -= OnTimerFinish;
            battleStageMenu.OnSelectMvpSummon -= OnSelectMvpSummon;
            battleStageMenu.OnSelectBossSummon -= OnSelectBossSummon;
            battleStageMenu.OnSelectAssemble -= OnSelectAssemble;
            uiSkillListToggle.OnSelectToggle -= OnSelectSkillToggle;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnSelectSkillToggle;
            uiResultClear.OnFinishDungeon -= OnFinishDungeon;
            uiQuickExpandMenu.OnClickItem -= OnSelectSharingCharacter;
            uiQuickExpandMenu.OnSlotInit -= OnExpandMenuSlotInit;

            uiBattleBossHp.SetOffset();
            uiMvpDuelResult.BeforeClose();

            UI.Close<UIMain>();
            UI.Close<UIMainTop>();
            UI.Close<UIMainShortcut>();
            UI.Close<UIController>();
            UI.Close<UIDanger>();
            UI.Close<UIAutoEquip>();
            UI.Close<UIWorldBossAlarm>();
            UI.Close<UIGoodsView>();
            UI.Close<UISimpleQuest>();
            UI.Close<UIQuickExpandMenu>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIWarning>();
            UI.Close<UIBossComing>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleFail>();
            UI.Close<UIBattleFailBoss>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattleStageMenu>();
            UI.Close<UIJobChangeMenu>();
            UI.Close<UISkillListToggle>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIResultClear>();
            UI.Close<UIStageInfoView>();
            UI.Close<UIMvpDuelResult>();
            UI.Close<UIChatPreview>();
            UI.Close<UIPowerSavingMenu>();

            uiMain = null;
            uiMainTop = null;
            uiMainShortcut = null;
            uiController = null;
            uiDanger = null;
            uiAutoEquip = null;
            uiWorldBossAlarm = null;
            uiGoodsView = null;
            uiSimpleQuest = null;
            uiQuickExpandMenu = null;
            uiMvpTimer = null;
            uiWarning = null;
            uiBossComing = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiBattlePlayerStatus = null;
            uiBattleFail = null;
            uiBattleFailBoss = null;
            uiBattleBossHp = null;
            battleStageMenu = null;
            uiJobChangeMenu = null;
            uiSkillListToggle = null;
            uiCamSelect = null;
            uiBattleMazeSkillList = null;
            uiResultClear = null;
            uiStageInfoView = null;
            uiMvpDuelResult = null;
            uiChatPreview = null;
            uiPowerSavingMenu = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            string mainText = currentData.name_id.ToText();

            switch (dungeonModel.StageMode)
            {
                case StageMode.Normal:
                    uiBattleInfo.Set(mainText);
                    break;

                case StageMode.Event:
                case StageMode.Challenge:
                    int level = dungeonModel.GetEventStageLevel(currentData.id);
                    string title = LocalizeKey._7200.ToText() // (Lv.{LEVEL}) {NAME}
                        .Replace(ReplaceKey.LEVEL, level)
                        .Replace(ReplaceKey.NAME, mainText);
                    uiBattleInfo.Set(title);
                    break;
            }
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            if (summonMonster != null && summonMonster.type == UnitEntityType.BossMonster)
            {
                if (!isClearedBoss)
                {
                    switch (dungeonModel.StageMode)
                    {
                        case StageMode.Normal:
                            battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetStageBossFailCoolTime(currentData.chapter));
                            break;

                        case StageMode.Event:
                        case StageMode.Challenge:
                            int level = dungeonModel.GetEventStageLevel(currentData.id);
                            battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetEventStageBossCoolTime(level));
                            break;
                    }
                }
            }

            ClearNormalMonster();
            StopTimer(); // 타이머 종료
            DisposeSummonMonster(); // Mvp 몬스터 폐기
#if SPAWN_AGENT_CHARACTER
            DisposeAgentCharacters(); // 동료 플레이어 폐기
#endif

            resultType = ResultType.None;
            HideFailUI();
            isMvpTutorial = false;
        }

        private void Restart()
        {
            isSummoningMvp = false;
            isMvpTutorial = false;
            impl.StartBattle(BattleMode.Stage, isStartFadeIn: false);
        }

        void OnReconnect()
        {
            isRecoonect = true;
        }

        public override IEnumerator<float> YieldExitBattle()
        {
            Debug.Log("스테이지 종료!");
            return base.YieldExitBattle();
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            int id = Mathf.Max(1, dungeonModel.LastEnterStageId);
            return YieldEnterBattle(id);
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            // 방어코드 : 최대 스테이지 방어코드
            if (dungeonModel.RequestStageMode == StageMode.Normal)
            {
                int serverIndex = connectionManager.GetSelectServerGroupId();
                int maxStage = BasisType.ENTERABLE_MAXIMUM_STAGE_BY_SERVER.GetInt(serverIndex);
                if (id >= maxStage)
                {
                    id = Mathf.Max(1, maxStage);
                }
            }

            StageData data = stageDataRepo.Get(id); // 현재 스테이지 데이터
            if (data == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 입장 불가능한 스테이지
            if (!dungeonModel.IsStageOpend(id))
            {
                IsSuccessEnter = false;
                yield break;
            }

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    Task<Response> task = dungeonModel.RequestStageStart(id); // 스테이지 시작 - 서버 호출
                    yield return Timing.WaitUntilTrue(task.IsComplete);
                    Response response = task.Result;
                    IsSuccessEnter = response.isSuccess;
                    break;

                case SharingModel.SharingState.Sharing:
                    IsSuccessEnter = true;
                    break;

                case SharingModel.SharingState.StandByReward:
                    IsSuccessEnter = true;
                    break;
            }

            // 입장 성공시 데이터 세팅
            // 미리 세팅할 경우 입장 실패해도 데이터가 변경됨
            if (IsSuccessEnter)
                currentData = data;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            switch (dungeonModel.StageMode)
            {
                case StageMode.Normal:
                    bool isNextStageOpen = dungeonModel.IsStageOpend(currentData.id + 1); // 다음 스테이지를 입장 여부
                    summonMonsterType = isNextStageOpen ? SummonMonsterType.MvpMonster : SummonMonsterType.BossMonster;
                    break;

                case StageMode.Event:
                case StageMode.Challenge:
                    summonMonsterType = SummonMonsterType.BossMonster;
                    break;
            }

            string sceneName = currentData.scene_name; // 씬 이름
            string bgmName = currentData.bgm; // 배경음 이름
            bool isChangeScene = !SceneLoader.IsCheckCurrentScene(sceneName) || (map == null); // 씬 변경 여부 - 씬이 다르거나, 맵이 존재하지 않을 경우
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;
            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (GameObject item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }

            // 위치 세팅
            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                Vector3 playerPosition = map.GetPlayerPosition();
                playerActor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
            }
        }

        protected override void StopAllCoroutine()
        {
            base.StopAllCoroutine();

            Timing.KillCoroutines(TAG_REVIVE);
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            isSummoningMvp = false;
            isMvpTutorial = false;

            isSceneReady = false;
            registeredBossRewards = null;
            uiBattlePlayerStatus.SetPlayer(player);
            uiMvpDuelResult.SetCurrenChapter(currentData.chapter);
            UpdateStageMenu();

            // 씬을 이동하면 무조건 쿨타임이 도는 사양으로 변경되었습니다.
            switch (dungeonModel.StageMode)
            {
                case StageMode.Normal:
                    {
                        isClearedBoss = false;
                        battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetStageBossClearCoolTime(currentData.chapter));
                        battleStageMenu.SetActiveEventBossIcon(false);
                        break;
                    }
                case StageMode.Event:
                    {
                        // 보스를 클리어 후 넘어왔을 경우 쿨타임 0초
                        if (isClearedBoss)
                        {
                            battleStageMenu.SetBossCoolTimeIfGreater(0);
                            battleStageMenu.SetActiveEventBossIcon(true);
                        }
                        else
                        {
                            int level = dungeonModel.GetEventStageLevel(currentData.id);
                            battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetEventStageBossCoolTime(level));
                            battleStageMenu.SetActiveEventBossIcon(true);
                        }
                        isClearedBoss = false;
                        break;
                    }
                case StageMode.Challenge:
                    {
                        isClearedBoss = false;
                        int level = dungeonModel.GetEventStageLevel(currentData.id);
                        battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetEventStageBossCoolTime(level));
                        battleStageMenu.SetActiveEventBossIcon(true);
                        break;
                    }
            }

            UI.ShowIndicator();
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            yield return Timing.WaitUntilDone(YieldSpawnCharacter(), TAG);

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    cameraController.RemoveMask(Layer.ALLIES, Layer.ENEMY); // 카메라 레이어 설정
                    yield return Timing.WaitUntilDone(YieldSpawnMonster(), TAG);
                    yield return Timing.WaitUntilDone(YieldSpawnSharingCharacters(sharingModel.GetSharingCharacters(), sharingModel.GetCloneCharacters()), nameof(YieldSpawnSharingCharacters));
                    cameraController.AddMask(Layer.PLAYER, Layer.CUPET, Layer.ALLIES, Layer.ENEMY, Layer.GHOST); // 카메라 레이어 설정
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
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            isSummoningMvp = false;
            isMvpTutorial = false;

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            uiBattleMazeSkillList.SetCharacter(GetCurrentPlayer());
            UpdateSkillToggle();

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                playerActor.EffectPlayer.ShowUnitCircle();
            }

            if (isShowStageInfo)
            {
                // 스테이지 최초 입장시에만 연출
                isShowStageInfo = false;
                MonsterData bossMonster = monsterDataRepo.Get(currentData.boss_monster_id);
                string bossName = string.Empty;
                string prefabName = string.Empty;

                if (bossMonster != null)
                {
                    bossName = bossMonster.name_id.ToText();
                    prefabName = bossMonster.prefab_name;
                }

                uiStageInfoView.Show(currentData.name_id.ToText(), bossName, prefabName);
            }

            StartTutorial();
            isSceneReady = true;

            UI.HideIndicator();

            // MVP 튜토리얼 체크
            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            if (sharingState == SharingModel.SharingState.None)
            {
                var curQuestInfo = questModel.GetMaintQuest();
                if (curQuestInfo == null)
                    return;

                if (curQuestInfo.IsInvalidData)
                    return;

                if (curQuestInfo.ShortCutType == ShortCutType.MvpTutorial && curQuestInfo.CompleteType == QuestInfo.QuestCompleteType.InProgress)
                {
                    RequestSummonMvp();
                }
            }
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (isClearedBoss)
                return;

            if (unitEntity.type == UnitEntityType.Player)
            {
                SetCurrentPlayer(player);

                UpdateMazeSkillList();

                if (summonMonster == null)
                {
                    uiBattleFail.Show(UIBattleFail.ConfirmType.Rebirth, player); // 플레이어 부활 처리
                }
                else
                {
                    uiBattleFailBoss.Show(player);

                    if (IsFocus)
                    {
                        // 중복 프로토콜을 막기 위함
                        if (resultType == ResultType.None)
                        {
                            resultType = ResultType.PlayerDead; // 플레이어 사망

                            if (summonMonster.type == UnitEntityType.BossMonster)
                            {
                                dungeonModel.RequestStageBossClear(false, currentData.id, summonMonster.clientUID, summonMonster.damagePacket).WrapNetworkErrors();
                            }
                            else
                            {
                                dungeonModel.RequestStageMvpMonsterDrop(false, unitEntity.clientUID, unitEntity.damagePacket).WrapNetworkErrors();
                            }
                        }
                    }

                    OnFailToChallengeBoss();
                }
            }
            else if (unitEntity.type == UnitEntityType.GhostPlayer)
            {
                // 조종하던 셰어 캐릭터가 죽으면 플레이어로 이동
                if (ReferenceEquals(GetCurrentPlayer(), unitEntity))
                {
                    SetCurrentPlayer(player);
                }
            }
            else if (unitEntity.type == UnitEntityType.NormalMonster)
            {
                MonsterEntity monsterEntity = unitEntity as MonsterEntity;

                // 보스가 소환한 몬스터면 아무것도 하지 않는다.
                if (!monsterEntity.IsBossSpawnMonster)
                {
                    RebirthMonster(unitEntity); // 일반몬스터 부활 처리

                    //// 죽은 몬스터의 stage Id와 현재 진입한 stageId가 같을 때에만 처리 (죽기 전에 다른 스테이지로 진입할 때 생길 수 있는 오류 막기)
                    //if (monsterEntity.stageId == dungeonModel.LastEnterStageId)
                    //    dungeonModel.RequestStageNormalMonsterDrop(unitEntity.battleUnitInfo.Id, unitEntity.clientUID, monsterEntity.stageId).WrapNetworkErrors();

                    if (IsFocus)
                    {
                        dungeonModel.RequestStageNormalMonsterDrop(unitEntity.battleUnitInfo.Id, unitEntity.clientUID, monsterEntity.stageId, unitEntity.damagePacket).WrapNetworkErrors();
                    }
                }
            }
            else if (unitEntity.type == UnitEntityType.MvpMonster)
            {
                isMvpTutorial = false;
                StopTimer();

                if (IsFocus)
                {
                    // 중복 프로토콜을 막기 위함
                    if (resultType == ResultType.None)
                    {
                        resultType = ResultType.BossClear;
                        dungeonModel.RequestStageMvpMonsterDrop(true, unitEntity.clientUID, unitEntity.damagePacket).WrapNetworkErrors();
                    }
                }
            }
            else if (unitEntity.type == UnitEntityType.BossMonster)
            {
                isClearedBoss = true;

                impl.ResetEnemyUnitList(); // 적군 캐릭터 제거
                StopTimer();

                if (IsFocus)
                {
                    // 중복 프로토콜을 막기 위함
                    if (resultType == ResultType.None)
                    {
                        resultType = ResultType.BossClear;
                        dungeonModel.RequestStageBossClear(true, currentData.id, unitEntity.clientUID, unitEntity.damagePacket).WrapNetworkErrors();
                    }
                }

                StopAllCoroutine(); // 모든 코루틴 제거 (부활 몬스터)
            }
        }

        /// <summary>
        /// 셰어 캐릭터 사망(부활 시작) 시
        /// </summary>
        private void OnShareCharacterDie(UnitActor actor, float remainReviveTime)
        {
            int index = -1;
            GhostPlayerEntity ghostEntity = actor.Entity as GhostPlayerEntity;
            if (ghostEntity.CloneType == default)
            {
                var find = sharingCharacterFactory.Find(ghostEntity.Character.Cid);
                if (find == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"목록에 없는 셰어 캐릭터가 사망 = CID {ghostEntity.Character.Cid}");
#endif
                    return;
                }

                index = sharingCharacterFactory.IndexOf(find);
            }
            else
            {
                index = sharingModel.GetCloneCharacterIndex(ghostEntity.CloneType);
            }

            if (index == -1)
                return;

            uiQuickExpandMenu.SetShareCharacterReviveTime(index, remainReviveTime);
        }

        /// <summary>
        /// ExpandMenu Slot SetData 이벤트
        /// </summary>
        private void OnExpandMenuSlotInit(int index)
        {
            GhostPlayerEntity entity = null;
            if (index < Constants.Size.SHARE_SLOT_SIZE)
            {
                if (index < sharingCharacterFactory.size)
                {
                    entity = sharingCharacterFactory[index];
                }
            }
            else
            {
                SharingModel.CloneCharacterType cloneType = sharingModel.GetCloneCharacterType(index);
                if (cloneDic.ContainsKey(cloneType))
                {
                    entity = cloneDic[cloneType];
                }
            }

            if (entity == null)
                return;

            GhostPlayerActor ghostActor = entity.GetActor() as GhostPlayerActor;
            if (ghostActor == null)
                return;

            if (index == -1)
                return;

            bool isSelectedPlayer = ReferenceEquals(entity, GetCurrentPlayer());
            uiQuickExpandMenu.SetShareCharacterReviveTime(index, ghostActor.RemainReviveTime.GetRemainTime());
            uiQuickExpandMenu.SetShareCharacterCurrentHp(index, entity.CurHP, entity.MaxHP, skipAnim: true);
            uiQuickExpandMenu.SetShareCharacterSelectState(index, isSelectedPlayer);
        }

        void OnStartController()
        {
            // 셰어캐릭터가 조종 불가능한 상황일때
            if (!IsPlayer(GetCurrentPlayer()) && !questModel.IsOpenContent(ContentType.ShareControl))
            {
                uiSkillListToggle.ActiveDeviPopup(true, true);
                return;
            }

            CharacterEntity current = GetCurrentPlayer();

            if (current.IsDie)
                return;

            UnitActor actor = current.GetActor();
            if (actor == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            actor.Movement.Stop();
            actor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
            CharacterEntity current = GetCurrentPlayer();

            if (position == Vector2.zero)
                return;

            // 셰어캐릭터가 조종 불가능한 상황일때
            if (!IsPlayer(GetCurrentPlayer()) && !questModel.IsOpenContent(ContentType.ShareControl, false))
                return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            if (current.IsDie)
                return;

            if (current.battleCrowdControlInfo.GetCannotMove())
                return;

            UnitActor actor = current.GetActor();
            if (actor == null)
                return;

            if (actor.IsPlayingActiveSkill()) // 액티브스킬 사용 중 이동 불가
                return;

            if (actor.AI.IsInputMove() == false) // OnStartController를 거치지 않고 Drag상태로 시작한 경우, 프로펠러 현상을 방지.
            {
                actor.AI.SetInputMove(isControl: true);
            }

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);
        }

        void OnResetController()
        {
            // 셰어캐릭터가 조종 불가능한 상황일때
            if (!IsPlayer(GetCurrentPlayer()) && !questModel.IsOpenContent(ContentType.ShareControl, false))
            {
                uiSkillListToggle.ActiveDeviPopup(false);
                return;
            }

            CharacterEntity current = GetCurrentPlayer();

            CameraUtils.Zoom(CameraZoomType.None);

            if (current.IsDie)
                return;

            UnitActor actor = current.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();
        }

        /// <summary>
        /// 전투 정보 클릭했을 때 처리
        /// </summary>
        void ShowDetailBattleInfo()
        {
            UI.Show<UIStageInfoPopup>().Show(currentData, false);
        }

        /// <summary>
        /// 플레이어 부활
        /// </summary>
        private void RebirthPlayer()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.AI.ChangeState(AI.Transition.Rebirth);

            UpdateMazeSkillList();
            player.SetCurrentMp(player.MaxMp);

            soundManager.PlaySfx("[SYSTEM] Gacha_Result_Window");

            // MVP 튜토리얼 체크
            var curQuestInfo = questModel.GetMaintQuest();
            if (curQuestInfo == null)
                return;

            if (curQuestInfo.IsInvalidData)
                return;

            if (curQuestInfo.ShortCutType == ShortCutType.MvpTutorial && curQuestInfo.CompleteType == QuestInfo.QuestCompleteType.InProgress)
            {
                RequestSummonMvp();
            }
        }

        /// <summary>
        /// 몬스터 부활
        /// </summary>
        private void RebirthMonster(UnitEntity entity)
        {
            int monsterZoneIndex = GetMonsterZoneIndex(entity);
            if (monsterZoneIndex == -1)
                return;

            // 모두 죽어있지 않을 경우
            if (!IsAllDead(monsterZoneDic[monsterZoneIndex]))
                return;

            // 몬스터 유닛 해제
            foreach (UnitEntity item in monsterZoneDic[monsterZoneIndex])
            {
                impl.Remove(item);
            }

            monsterZoneDic.Remove(monsterZoneIndex);

            Timing.RunCoroutine(YieldReviveMonster(monsterZoneIndex), TAG_REVIVE);
        }

        /// <summary>
        /// 해당 유닛이 속한 zone Index 값을 반환 (없을 경우 -1 반환)
        /// </summary>
        private int GetMonsterZoneIndex(UnitEntity entity)
        {
            if (entity.type != UnitEntityType.NormalMonster)
                return -1;

            foreach (KeyValuePair<int, UnitEntity[]> item in monsterZoneDic)
            {
                foreach (UnitEntity unit in item.Value)
                {
                    if (unit.Equals(entity))
                        return item.Key;
                }
            }

            return -1;
        }

        /// <summary>
        /// 모두 죽었는지 체크
        /// </summary>
        private bool IsAllDead(UnitEntity[] entities)
        {
            foreach (UnitEntity item in entities)
            {
                // 죽지않은 몬스터가 존재한다
                if (!item.IsDie)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCharacter()
        {
            if (impl.Add(player, isEnemy: false))
            {
                // 최초 한 번만 하기 위해 여기에 세팅
                player.SetExtraOption(ExtraBattleOptionType.PlusMoveSpd, CHARACTER_PLUS_MOVE_SPEED); // 강제이속증가
                player.SetForceStatus(ForceStatusType.BuffItemOptionOn); // 버프아이템 On

                UnitActor actor = player.SpawnActor(); // 유닛 소환
                actor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);

                cameraController.SetPlayer(actor.CachedTransform); // 카메라 타겟 세팅
                Vector3 playerPosition = map.GetPlayerPosition();
                actor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster()
        {
            int zoneCount = map.ZoneCount;
            for (int i = 0; i < zoneCount; i++)
            {
                yield return Timing.WaitUntilDone(YieldSpawnMonster(i), TAG);
            }
        }

        private void YieldSpawnMonsterInstantly()
        {
            int zoneCount = map.ZoneCount;
            for (int i = 0; i < zoneCount; i++)
            {
                var enumerator = YieldSpawnMonster(i);
                while (enumerator.MoveNext()) ;

                foreach (UnitEntity item in monsterZoneDic[i])
                {
                    UnitActor actor = item.GetActor();
                    if (actor == null)
                        continue;

                    // 하드 & 챌린지 모드 몬스터 아우라 이펙트 표시
                    if (dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge)
                        actor.EffectPlayer.ShowAura(UnitAuraType.HardModeNormalMonster);

                    actor.AI.ReadyToBattle();
                    item.ReadyToBattle(); // 바로 전투 시작                    

                    if (IsPause)
                    {
                        actor.AI.Pause();
                        actor.Movement.Pause();
                    }
                }
            }
        }

        /// <summary>
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster(int zoneIndex)
        {
            int monsterLevel = currentData.normal_monster_level;
            switch (dungeonModel.StageMode)
            {
                // 하드 & 챌린지 모드시 몬스터 레벨 변경
                case StageMode.Event:
                case StageMode.Challenge:
                    int level = dungeonModel.GetEventStageLevel(currentData.id);
                    int increase = BasisType.HARD_CHELLENGE_MONSTER_LEVEL.GetInt();
                    monsterLevel += (level * increase);
                    break;
            }

            IEnumerable<UnitEntity> monsters = factory.CreateNormalMonster(currentData, monsterLevel);
            foreach (UnitEntity item in monsters)
            {
                // 추가 실패 (이미 등록 되어있음)
                if (!impl.Add(item, isEnemy: true))
                    continue;

                monsterBuffer.Add(item);

                // 현재 스테이지 Id 세팅
                if (item is MonsterEntity monsterEntity)
                    monsterEntity.SetStateId(currentData.id);
            }

            UnitEntity[] entities = monsterBuffer.GetBuffer(isAutoRelease: true);
            Vector3[] positions = map.GetMonsterPositions(zoneIndex, entities.Length); // 위치 반환
            for (int i = 0; i < positions.Length; i++)
            {
                UnitActor monsterActor = entities[i].SpawnActor(); // 유닛 소환
                monsterActor.AI.SetHomePosition(positions[i], isWarp: true); // 위치 세팅
                // 하드 & 챌린지 모드 몬스터 아우라 이펙트 표시
                if (dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge)
                    monsterActor.EffectPlayer.ShowAura(UnitAuraType.HardModeNormalMonster);

                if (IsPause)
                {
                    monsterActor.AI.Pause();
                    monsterActor.Movement.Pause();
                }

                yield return Timing.WaitForOneFrame;
            }

            monsterZoneDic.Add(zoneIndex, entities);
        }

        /// <summary>
        /// 몬스터 부활
        /// </summary>
        IEnumerator<float> YieldReviveMonster(int zoneIndex)
        {
            yield return Timing.WaitForSeconds(reviveDelayTime); // 부활 대기
            yield return Timing.WaitUntilDone(YieldSpawnMonster(zoneIndex), TAG_REVIVE);

            if (!monsterZoneDic.ContainsKey(zoneIndex))
            {
#if UNITY_EDITOR
                Debug.LogError($"몬스터 부활 시도 실패. {string.Join(", ", monsterZoneDic)}에는 [{zoneIndex}]가 없다");
#endif
                yield break;
            }

            foreach (UnitEntity item in monsterZoneDic[zoneIndex])
            {
                UnitActor actor = item.GetActor();
                if (actor == null)
                    continue;

                // 하드 & 챌린지 모드 몬스터 아우라 이펙트 표시
                if (dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge)
                    actor.EffectPlayer.ShowAura(UnitAuraType.HardModeNormalMonster);

                actor.AI.ReadyToBattle();
                item.ReadyToBattle(); // 바로 전투 시작

                if (IsPause)
                {
                    actor.AI.Pause();
                    actor.Movement.Pause();
                }
            }
        }

        private void ClearNormalMonster()
        {
            Timing.KillCoroutines(TAG_REVIVE);

            for (int i = unitList.size - 1; i >= 0; i--)
            {
                if (!unitList[i].IsEnemy)
                    continue;

                if (unitList[i].type != UnitEntityType.NormalMonster)
                    continue;

                Dispose(unitList[i]); // 폐기
            }

            monsterZoneDic.Clear();
        }

        /// <summary>
        /// 중간보스 시간 종료
        /// </summary>
        private void OnTimerFinish()
        {
            if (summonMonster != null)
                OnFailToChallengeBoss();
        }

        private void OnFailToChallengeBoss()
        {
            if (summonMonster.type == UnitEntityType.BossMonster)
            {
                switch (dungeonModel.StageMode)
                {
                    case StageMode.Normal:
                        battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetStageBossFailCoolTime(currentData.chapter));
                        break;

                    case StageMode.Event:
                    case StageMode.Challenge:
                        int level = dungeonModel.GetEventStageLevel(currentData.id);
                        battleStageMenu.SetBossCoolTimeIfGreater(dungeonModel.GetEventStageBossCoolTime(level));
                        break;
                }
                YieldSpawnMonsterInstantly();
            }

            StopTimer();
            DisposeSummonMonster();
        }

        private void OnSelectMvpSummon()
        {
            if (!isSceneReady)
                return;

            if (player.IsDie)
            {
                UI.ShowToastPopup(LocalizeKey._90084.ToText());
                return;
            }

            if (dungeonModel.SummonMvpTicketCount == 0)
            {
                UI.ShowToastPopup(LocalizeKey._90144.ToText()); // 소환권이 부족합니다.
                return;
            }

            if (summonMonster.IsValid())
            {
                if (summonMonsterType == SummonMonsterType.MvpMonster)
                {
                    UI.ShowToastPopup(LocalizeKey._90143.ToText()); // 이미 소환되어 있는 MVP 몬스터가 존재합니다.
                }

                return;
            }

            RequestSummonMvp();
        }

        private void OnSelectBossSummon()
        {
            if (!isSceneReady)
                return;

            if (player.IsDie)
            {
                UI.ShowToastPopup(LocalizeKey._90084.ToText());
                return;
            }

            // 마지막 스테이지가 아닌곳에서 보스도전 방어 코드
            if (currentData.id != dungeonModel.FinalStageId && dungeonModel.StageMode == StageMode.Normal)
            {
                battleStageMenu.UpdateOpenContent();
                return;
            }

            // 보스소환 튜토리얼을 하지 않았다.
            if (Cheat.USE_TUTORIAL)
            {
                if (!Tutorial.HasAlreadyFinished(TutorialType.BossSummon))
                {
                    UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                    return;
                }

                if (isMvpTutorial)
                {
                    UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                    return;
                }
            }

            if (dungeonModel.StageMode == StageMode.Normal)
            {
                // 마지막 스테이지의 경우에 쿠폰 보여주기 체크
                int serverIndex = connectionManager.GetSelectServerGroupId();
                int maxStage = BasisType.ENTERABLE_MAXIMUM_STAGE_BY_SERVER.GetInt(serverIndex);
                if (currentData.id == maxStage)
                {
                    if (dungeonModel.ShowStageCoupon())
                        return;
                }
            }

            if (summonMonster.IsValid())
            {
                if (summonMonster.type == UnitEntityType.MvpMonster)
                {
                    AsyncSummonBossWithMvpBoss().WrapNetworkErrors();
                }
                else if (summonMonster.type == UnitEntityType.BossMonster)
                {
                    UI.ShowToastPopup(LocalizeKey._90168.ToText()); // 이미 보스 도전 진행중입니다.
                }

                return;
            }

            RequestSummonBoss(); // 보스 소환
        }

        /// <summary>
        /// 강제로 보스 도전 (MVP 몬스터가 존재할 경우)
        /// </summary>
        private async Task AsyncSummonBossWithMvpBoss()
        {
            int serverIndex = connectionManager.GetSelectServerGroupId();
            int maxStage = BasisType.ENTERABLE_MAXIMUM_STAGE_BY_SERVER.GetInt(serverIndex);

            // 마지막 스테이지 아닐 경우에
            if (currentData.id < maxStage)
            {
                string message = LocalizeKey._90182.ToText(); // 보스 소환 시 MVP 몬스터가 사라집니다.\n그래도 도전하시겠습니까?
                if (!await UI.SelectPopup(message))
                    return;

                // 기존의 소환 몬스터 제거
                if (summonMonster.IsValid())
                {
                    StopTimer();
                    DisposeSummonMonster();
                }
            }

            RequestSummonBoss(); // 보스 소환
        }

        /// <summary>
        /// 보스 소환
        /// </summary>
        private void RequestSummonBoss()
        {
            questModel.RemoveNewOpenContent(ContentType.Boss); // 신규 컨텐츠 플래그 제거 (보스)
            dungeonModel.RequestSummonStageBoss().WrapNetworkErrors();
        }

        /// <summary>
        /// MVP 소환
        /// </summary>
        private void RequestSummonMvp()
        {
            if (!isSceneReady)
                return;

            if (player.IsDie)
                return;

            if (summonMonster.IsValid())
                return;

            if (isSummoningMvp)
                return;

            isSummoningMvp = true;
            dungeonModel.SummonMvpMonster().WrapNetworkErrors();
        }

        /// <summary>
        /// 퀘스트 업데이트 이벤트
        /// </summary>
        private void OnUpdateGuideQuest()
        {
            Timing.RunCoroutine(YieldOnUpdateGuideQuest(), TAG);
        }

        private IEnumerator<float> YieldOnUpdateGuideQuest()
        {
            // 퀘스트 리워드 UI 종료 대기
            var uiQuestReward = UI.GetUI<UIQuestReward>();
            yield return Timing.WaitUntilFalse(() => uiQuestReward != null && uiQuestReward.IsVisible);

            // MVP 튜토리얼 체크
            var curQuestInfo = questModel.GetMaintQuest();

            if (curQuestInfo == null)
                yield break;

            if (curQuestInfo.IsInvalidData)
                yield break;

            if (curQuestInfo.ShortCutType == ShortCutType.MvpTutorial && curQuestInfo.CompleteType == QuestInfo.QuestCompleteType.InProgress)
            {
                RequestSummonMvp();
            }
        }

        private void OnSelectAssemble()
        {
            if (summonMonster.IsInvalid())
            {
                UI.ShowToastPopup(LocalizeKey._90145.ToText()); // 출현한 MVP 몬스터가 없습니다.
                return;
            }

            if (!CanAssemble())
            {
                UI.ShowToastPopup(LocalizeKey._90146.ToText()); // 집결할 셰어 캐릭터가 없습니다.
                return;
            }

            bool isPlayer = IsPlayer(GetCurrentPlayer());
            if (!isPlayer)
            {
                UI.ShowToastPopup(LocalizeKey._90205.ToText()); // 셰어 캐릭터로는 집결을 할 수 없습니다.
                return;
            }

            Vector3 dist = summonMonster.LastPosition - player.LastPosition;
            if (dist.sqrMagnitude > SEND_POSITION_SQR_MAGNITUDE)
            {
                if (summonMonster.type == UnitEntityType.MvpMonster)
                {
                    UI.ShowToastPopup(LocalizeKey._90147.ToText()); // 좀 더 MVP 몬스터와 가까운 거리에서 사용하세요.
                }
                else if (summonMonster.type == UnitEntityType.BossMonster)
                {
                    UI.ShowToastPopup(LocalizeKey._90183.ToText()); // 좀 더 보스 몬스터와 가까운 거리에서 사용하세요.
                }

                return;
            }

            dungeonModel.RequestAutoStageSummonPlayer(currentData.id).WrapNetworkErrors(); // 서버 호출
#if SPAWN_AGENT_CHARACTER
            SpawnAgentCharacters(); // 동료캐릭터 소환
#endif
            InitializeCharactersStatus(); // 캐릭터 스탯 초기화
            WarpSharingCharactersToMvpMonster(); // 셰어링캐릭터 워프

            isAssemble = true;
            cameraController.SetView(CameraController.View.MvpBattle);
            UpdateStageMenu();
        }

        private bool CanAssemble()
        {
            if (sharingModel.HasSharingCharacters())
                return true;

#if SPAWN_AGENT_CHARACTER
            if (agentModel.HasEquippedCombatAgentCharacters())
                return true;
#endif

            return false;
        }

        private void StopTimer()
        {
            if (uiMvpTimer != null)
            {
                uiMvpTimer.StopTimer();
                uiMvpTimer.Hide();
            }
        }

        private void DisposeSummonMonster()
        {
            Dispose(summonMonster);
            summonMonster = null;

            UpdateStageMenu(); // 집결 버튼 업데이트
        }

#if SPAWN_AGENT_CHARACTER
        /// <summary>
        /// 동료 캐릭터 폐기
        /// </summary>
        private void DisposeAgentCharacters()
        {
            foreach (GhostPlayerEntity item in agentCharacterFactory)
            {
                item.ResetExtraOption(ExtraBattleOptionType.PlusMoveSpd); // 강제이속증가 되돌리기

                Dispose(item);
                agentCharacterFactory.Recycle(item);
            }
            agentCharacterFactory.Clear();
            //Debug.LogError($"동료 캐릭터 폐기={agentCharacterFactory.size}");
            Timing.KillCoroutines(nameof(YieldSpawnAgentCharacters));
        }
#endif

        /// <summary>
        /// 셰어링 캐릭터 폐기
        /// </summary>
        private void DisposeSharingCharacters()
        {
            SetCurrentPlayer(player);

            while (sharingCharacterFactory.Size > 0)
            {
                var item = sharingCharacterFactory[0];
                item.Skill.SetAntiSkillAuto(false); // 스킬 수동 모드 해제
                item.ResetExtraOption(ExtraBattleOptionType.PlusMoveSpd); // 강제이속증가 되돌리기
                item.OnChangeHP -= OnShareCharacterChangeHp;

                Dispose(item);
                sharingCharacterFactory.Recycle(item);
            }

            while (cloneCharacterFactory.Size > 0)
            {
                var item = cloneCharacterFactory[0];
                item.Skill.SetAntiSkillAuto(false); // 스킬 수동 모드 해제
                item.ResetExtraOption(ExtraBattleOptionType.PlusMoveSpd); // 강제이속증가 되돌리기
                item.OnChangeHP -= OnShareCharacterChangeHp;

                Dispose(item);
                cloneCharacterFactory.Recycle(item);
            }

            cloneDic.Clear();

            Timing.KillCoroutines(nameof(YieldSpawnSharingCharacters));
        }

        /// <summary>
        /// 몬스터 아이템 드랍 결과
        /// </summary>
        void OnMonsterItemDrop(UnitEntity unit, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            if (unit == null)
                return;

            UnitActor actor = unit.GetActor();
            if (actor == null) // 서버 통신이 늦는 경우, 그 사이에 actor가 죽어버리면 아래에서 충돌이 일어난다.
                return;

            if (MathUtils.IsCheckPermyriad(5000)) // 50.00% 확률로 골드 표시
                actor.EffectPlayer.SpawnGold(unit.LastPosition);

            RecoveryMp(unit); // MP 회복
        }

        /// <summary>
        /// Mvp몬스터 아이템 드랍 결과
        /// </summary>
        void OnMvpMonsterItemDrop(UnitEntity unit, RewardData[] rewards, RewardData[] wasted, int duelAlphabetIndex, bool isGainedArenaPoint)
        {
            if (unit == null)
                return;

            Quest.QuestProgress(QuestType.FIELD_ID_MVP_CLEAR_COUNT, currentData.id); // 특정 필드에서 MVP 처치 횟수
            MvpData[] array = mvpDataRepo.GetArrayGroup(currentData.mvp_group_id);
            foreach (MvpData item in array)
            {
                if (item.rare_type != 3)
                    continue;

                if (item.monster_id == unit.battleUnitInfo.Id)
                {
                    Quest.QuestProgress(QuestType.FILED_ID_RARE_3_MVP_CLEAR_COUNT, currentData.id); // 특정 필드에서 rare_type 3번 MVP 처치 횟수
                    break;
                }
            }

            var ui = UI.Show<UIMvpResult>();
            ui.SetMode(UIMvpResult.Mode.MVP);
            ui.SetData(rewards, wasted);
            if (duelAlphabetIndex >= 0 || isGainedArenaPoint)
                EventDelegate.Add(ui.onClose, () => { uiMvpDuelResult.StartAnim(duelAlphabetIndex, isGainedArenaPoint); }, true);

            RecoveryMp(unit); // MP 회복
        }

        /// <summary>
        /// 보스몬스터 아이템 드랍 결과
        /// </summary>
        void OnBossMonsterItemDrop(UnitEntity unit, RewardData[] rewards)
        {
            if (rewards != null)
            {
                if (dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge)
                {
                    // 이벤트모드 또는 챌린지모드에서 CatCoin 이 나올 경오에는 Event 형식으로 보여주어야 함
                    foreach (var item in rewards)
                    {
                        item.SetIsEvent(item.RewardType == RewardType.CatCoin || item.RewardType == RewardType.CatCoinFree || item.RewardType == RewardType.ROPoint);
                    }
                }
            }

            if (Tutorial.isInProgress)
                registeredBossRewards = rewards;
            else
                uiResultClear.Show(rewards, default, false);

            RecoveryMp(unit); // MP 회복
        }

        void OnFinishDungeon()
        {
            int stageId = currentData.id;

            switch (dungeonModel.StageMode)
            {
                case StageMode.Normal:
                    int nextId = stageId + 1;
                    if (stageDataRepo.IsExists(nextId)) // 다음 스테이지 존재
                        stageId = nextId;
                    break;

                case StageMode.Event:

                    Debug.Log($"이벤트 모드 레벨 = {dungeonModel.GetEventStageLevel(stageId)}, 최대레벨 = {BasisType.HARD_CHELLENGE_MAX_LEVEL.GetInt()}");

                    // 최대레벨 도달 시 일반 스테이지로 이동
                    if (dungeonModel.GetEventStageLevel(stageId) > BasisType.HARD_CHELLENGE_MAX_LEVEL.GetInt())
                    {
                        SetNextSingleAction(ShowEventAdventureUI);
                    }
                    else
                    {
                        dungeonModel.StartBattleStageMode(StageMode.Event, stageId); // 다름레벨 스테이지로 이동
                        return;
                    }
                    break;

                // 챌린지 모드 챕터의 첫번째 일반 스테이지로 입장
                case StageMode.Challenge:
                    stageId = currentData.challenge_return_stage;
                    SetNextSingleAction(ShowEventAdventureUI);
                    break;
            }

            dungeonModel.StartBattleStageMode(StageMode.Normal, stageId);
        }

        private void ShowEventAdventureUI()
        {
            UI.Show<UIAdventureMap>().SetEventMode(isEvent: true);
        }

        void OnDropCube(UnitEntity unit)
        {
            if (unit == null)
                return;

            UnitActor actor = unit.GetActor();
            if (actor == null) // 서버 통신이 늦는 경우, 그 사이에 actor가 죽어버리면 아래에서 충돌이 일어난다.
                return;

            Vector3 position = unit.LastPosition;
            actor.EffectPlayer.SpawnDropCube(position, uiBattleMenu.GetDuelTarget()).Play();
        }

        void OnAppearMvpMonster(int id, long remainTime)
        {
            if (DebugUtils.IsLogBossMonster)
            {
                Debug.LogError($"{nameof(OnAppearMvpMonster)}: {nameof(id)} = {id}, {nameof(remainTime)} = {remainTime}");
            }

            if (remainTime > MVP_MINUS_TIME)
                remainTime -= MVP_MINUS_TIME;

            MvpData mvpData = mvpDataRepo.Get(id);
            if (mvpData == null)
            {
                Debug.LogError($"Mvp 몬스터가 존재하지 않습니다: {nameof(id)} = {id}");
                return;
            }

            isSummoningMvp = false;

            // MVP 튜토리얼 체크
            var curQuestInfo = questModel.GetMaintQuest();
            if (curQuestInfo != null && curQuestInfo.ShortCutType == ShortCutType.MvpTutorial && curQuestInfo.CompleteType == QuestInfo.QuestCompleteType.InProgress)
            {
                isMvpTutorial = true;
                Tutorial.Run(TutorialType.Mvp);
            }

            // 방어코드 기존의 소환 몬스터 제거
            if (summonMonster.IsValid())
            {
                StopTimer();
                DisposeSummonMonster();
            }

            uiWarning.PlayWarning();
            if (mvpData.rare_type == 3)
            {
                uiBossComing.ShowBoss(mvpData);
                soundManager.PlaySfx("[SYSTEM] Congratulation");
            }

            if (remainTime > 0)
            {
                uiMvpTimer.Initialize(remainTime);
                uiMvpTimer.Show();
                uiMvpTimer.RestartTimer();
            }
            else // remainTime이 -1 이면 시간 무제한
            {
                StopTimer();
            }
            summonMonster = factory.CreateMvpMonster(mvpData.monster_id, currentData.mvp_level, mvpData.GetScale());
            SpawnSummonMonster(mvpData.rare_type);

            if (mvpData.IsUniqueMvpMonsterBattleView())
            {
                cameraController.SetView(CameraController.View.UniqueMvpMonsterBattle);
                cameraController.SetEffect(CameraController.CameraEffect.UniqueMvpBoss);
            }
            else
            {
                cameraController.SetView(DEFAULT_VIEW);
                cameraController.SetEffect(CameraController.CameraEffect.None);
            }

        }

        void OnAppearBossMonster(int remainTime)
        {
            // 방어코드 기존의 소환 몬스터 제거
            if (summonMonster.IsValid())
            {
                StopTimer();
                DisposeSummonMonster();
            }

            ClearNormalMonster();

            uiBossComing.ShowBoss(currentData);
            uiMvpTimer.Initialize(remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();
            switch (dungeonModel.StageMode)
            {
                case StageMode.Normal:
                    summonMonster = factory.CreateBossMonster(currentData);
                    break;
                case StageMode.Event:
                case StageMode.Challenge:
                    int level = dungeonModel.GetEventStageLevel(currentData.id);
                    int increase = BasisType.HARD_CHELLENGE_MONSTER_LEVEL.GetInt();
                    int monsterLevel = currentData.boss_monster_level + (level * increase);
                    summonMonster = factory.CreateBossMonster(currentData, monsterLevel);
                    break;
            }
            SpawnSummonMonster(0);
            InitializeCharactersStatus(); // 캐릭터 스탯 초기화

            soundManager.PlaySfx("[SYSTEM] Congratulation");
        }

        public override void OnMonsterRequestSpawn(MonsterEntity bossMonster, int spawnID, int spawnCount)
        {
            IEnumerable<UnitEntity> monsters = factory.CreateSpawnMonster(bossMonster, spawnID, spawnCount);

            foreach (UnitEntity item in monsters)
            {
                // 추가 실패 (이미 등록 되어있음)
                if (!impl.Add(item, isEnemy: true))
                    continue;

                monsterBuffer.Add(item);

                // 현재 스테이지 Id 세팅
                if (item is MonsterEntity monsterEntity)
                {
                    monsterEntity.SetStateId(currentData.id);
                    monsterEntity.SetIsBossSpawnMonster(true);
                }
            }

            UnitEntity[] entities = monsterBuffer.GetBuffer(isAutoRelease: true);
            Vector3[] positions = null;

            for (int i = 0; i < entities.Length; i++)
            {
                UnitActor monsterActor = entities[i].SpawnActor(); // 유닛 소환

                if (positions == null)
                    positions = map.GetMonsterPositions(bossMonster.LastPosition, bossMonster.GetActor().Appearance.GetRadius() + 1 + monsterActor.Appearance.GetRadius(), entities.Length); // 위치 반환

                // 하드 & 챌린지 모드 몬스터 아우라 이펙트 표시
                if (dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge)
                    monsterActor.EffectPlayer.ShowAura(UnitAuraType.HardModeNormalMonster);

                monsterActor.AI.SetHomePosition(positions[i], isWarp: true); // 위치 세팅
                entities[i].GetActor().AI.ReadyToBattle();
                entities[i].ReadyToBattle();
            }
        }

        private void SpawnSummonMonster(int rareType)
        {
            if (summonMonster == null)
                return;

            // 소환과 동시에 죽었을 경우에
            if (player.IsDie)
            {
                HideFailUI();
                RebirthPlayer();
            }

            resultType = ResultType.None;
            summonMonster.OnSpawnActor += OnSpawnSummonMonster;

            UnitActor summonMonsterActor;
            if (impl.Add(summonMonster, isEnemy: true))
            {
                summonMonsterActor = summonMonster.SpawnActor(); // 유닛 소환
            }
            else
            {
                summonMonsterActor = summonMonster.GetActor();
            }

            Vector3 homePosition = map.GetMonsterPosition(Random.Range(0, map.ZoneCount));
            summonMonsterActor.AI.SetHomePosition(homePosition, isWarp: true);
            summonMonsterActor.AI.ReadyToBattle();
            summonMonsterActor.EffectPlayer.ShowUnitCircle();

            if (summonMonster.type == UnitEntityType.BossMonster)
            {
                // 하드 & 챌린지 모드 몬스터 아우라 이펙트 표시
                if (dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge)
                    summonMonsterActor.EffectPlayer.ShowAura(UnitAuraType.HardModeBossMonster);
            }

            if (IsPause)
            {
                summonMonsterActor.AI.Pause();
                summonMonsterActor.Movement.Pause();
            }

            MonsterData monsteData = MonsterDataManager.Instance.Get(summonMonster.battleUnitInfo.Id);
            float hudOffset = monsteData.GetHudOffset();
            PoolObject bossHud = summonMonsterActor.EffectPlayer.SetHpHudTarget(new Vector3(0, hudOffset, 0));

            if (DebugUtils.IsLogBossMonster)
            {
                Debug.Log($"소환 몬스터 {nameof(hudOffset)}={hudOffset}, {nameof(monsteData.hud_y_value)}={monsteData.hud_y_value}");
            }

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.SetOffset(UIBattleBossHp.Offset.BossMonster);
            uiBattleBossHp.Show(
                summonMonster.battleUnitInfo.Level,
                summonMonster.GetName(),
                summonMonster.battleUnitInfo.UnitElementType,
                summonMonster.type == UnitEntityType.BossMonster ? UIBattleBossHp.MonsterType.NormalBoss : UIBattleBossHp.MonsterType.MVP,
                monsteData.cost,
                bossHud.CachedGameObject, rareType);
            uiBattleBossHp.SetHp(summonMonster.CurHP, summonMonster.MaxHP);

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                if (summonMonster.type == UnitEntityType.BossMonster)
                {
                    playerActor.EffectPlayer.ShowBossTargetingArrow(summonMonster);
                }
                else
                {
                    playerActor.EffectPlayer.ShowBossTargetingArrow(summonMonster);
                }
            }

            isAssemble = false;
            UpdateStageMenu(); // 집결 버튼 업데이트
        }

        void OnSpawnSummonMonster(UnitActor actor)
        {
            var monsterEntity = actor.Entity;
            if (monsterEntity != null)
            {
                monsterEntity.OnDespawnActor += OnDespawSummonMonster;
                monsterEntity.OnChangeHP += OnChangedSummonMonster;
            }
        }

        void OnDespawSummonMonster(UnitActor actor)
        {
            var monsterEntity = actor.Entity;
            actor.AI.Resume(); // 튜토리얼로 인해 Pause된 몬스터가 그대로 사망할 시, 리스폰되어도 Pause상태를 유지할 수 있음.

            if (monsterEntity != null)
            {
                monsterEntity.OnSpawnActor -= OnSpawnSummonMonster;
                monsterEntity.OnDespawnActor -= OnDespawSummonMonster;
                monsterEntity.OnChangeHP -= OnChangedSummonMonster;
            }

            // 현재 또 다른 보스 몬스터를 소환하여 진행중인 경우 루틴 무시
            if (summonMonster != monsterEntity)
                return;

            if (uiBattleBossHp)
                uiBattleBossHp.Hide();

#if SPAWN_AGENT_CHARACTER
            DisposeAgentCharacters(); // 동료 캐릭터 폐기
#endif

            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ReleaseBossTargetingArrow();

            isAssemble = false;
            UpdateStageMenu();
            cameraController.SetView(DEFAULT_VIEW);
            cameraController.SetEffect(CameraController.CameraEffect.None);
        }

        void OnChangedSummonMonster(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }

#if SPAWN_AGENT_CHARACTER
        private void SpawnAgentCharacters()
        {
            DisposeAgentCharacters(); // 동료 캐릭터 폐기

            foreach (CombatAgent item in agentModel.GetEquipedCombatAgents())
            {
                if (item.ID == 0 || item.AgentData == null)
                    continue;

                Debug.LogError($"1.장착 중인 동료={item.AgentData.name_id.ToText()}");

                agentPacketBuffer.Add(new AgentCharacterPacket(item.AgentData));
            }

            int jobLevel = characterModel.JobLevel;
            int totalStatPoint = statusModel.TotalStatusPoint;
            for (int i = 0; i < agentPacketBuffer.size; i++)
            {
                int jobId = agentPacketBuffer[i].job;
                JobData agentJobData = jobDataRepo.Get(jobId);
                if (agentJobData == null)
                {
                    Debug.LogError($"JobData가 존재하지 않습니다: {nameof(jobId)} = {jobId}");
                    continue;
                }

                short[] plusGuidStats = agentJobData.GetAutoStatGuidePoints(0, 0, 0, 0, 0, 0, totalStatPoint);
                agentPacketBuffer[i].UpdateStatus(jobLevel, plusGuidStats[0], plusGuidStats[1], plusGuidStats[2], plusGuidStats[3], plusGuidStats[4], plusGuidStats[5]);
            }

            SetAgentPlayers(agentPacketBuffer.GetBuffer(isAutoRelease: true)); // 멀티 플레이어 세팅
            Timing.RunCoroutine(YieldSpawnAgentCharacters(), nameof(YieldSpawnAgentCharacters));
        }
#endif

        private void WarpSharingCharactersToMvpMonster()
        {
            Vector3 posCenter;
            float distance;
            UnitActor mvpMonsterActor = summonMonster.GetActor();
            if (mvpMonsterActor)
            {
                posCenter = summonMonster.LastPosition;
                distance = mvpMonsterActor.Appearance.GetRadius() + WARP_TO_MVP_MONSTER_PADDING;
            }
            else
            {
                posCenter = player.LastPosition;
                distance = SEND_POSITION_MAGNITUDE;
            }

            int size = sharingCharacterFactory.size + cloneDic.Count;
            Vector3[] spawnPositions = map.GetAroundPosition(posCenter, distance, size);
            int index = 0;
            for (int i = 0; i < sharingCharacterFactory.size; i++)
            {
                if (sharingCharacterFactory[i].IsDie)
                    continue;

                UnitActor actor = sharingCharacterFactory[i].GetActor();
                if (actor == null)
                    continue;

                actor.Movement.Warp(spawnPositions[index]);
                actor.AI.SetTarget(mvpMonsterActor);

                ++index;
            }

            foreach (var item in cloneDic.Values)
            {
                if (item.IsDie)
                    continue;

                UnitActor actor = item.GetActor();
                if (actor == null)
                    continue;

                actor.Movement.Warp(spawnPositions[index]);
                actor.AI.SetTarget(mvpMonsterActor);

                ++index;
            }
        }

        private void AddSharingCharacters(IBattleSharingCharacter[] infos)
        {
            Timing.RunCoroutine(YieldSpawnSharingCharacters(infos, null), TAG);
        }

        private bool SpawnSharingCharacter(IMultiPlayerInput input, SharingModel.CloneCharacterType cloneType)
        {
            if (input == null)
                return false;

            int cid = input.Cid;

            GhostPlayerEntity entity = null;
            if (cloneType == default)
            {
                // 이미 존재하는 셰어링캐릭터
                CharacterEntity finded = sharingCharacterFactory.Find(cid);
                if (finded && finded.GetActor())
                    return false;

                entity = sharingCharacterFactory.AddGhostPlayer(input);
            }
            else
            {
                // 이미 존재하는 셰어링캐릭터
                CharacterEntity finded = cloneDic.ContainsKey(cloneType) ? cloneDic[cloneType] : null;
                if (finded && finded.GetActor())
                    return false;

                entity = cloneCharacterFactory.AddGhostPlayer(input);
            }

            if (entity == null)
                return false;

            if (!impl.Add(entity, isEnemy: false))
                return false;

            if (cloneType == default)
            {
                // Do Nothing
            }
            else
            {
                cloneDic.Add(cloneType, entity);
            }

            entity.SetExtraOption(ExtraBattleOptionType.PlusMoveSpd, CHARACTER_PLUS_MOVE_SPEED); // 강제이속증가
            entity.SetCloneType(cloneType);
            UnitActor characterActor = entity.SpawnActor(); // 유닛 소환

            GhostPlayerActor ghostActor = characterActor as GhostPlayerActor;
            ghostActor.OnStartReviveTimer += OnShareCharacterDie;
            entity.OnChangeHP += OnShareCharacterChangeHp;
            entity.OnDespawnActor += OnShareCharacterDespawnActor;

            int randCount = Random.Range(0, 4) + 1;
            Vector3[] spawnPositions = map.GetAroundPosition(player.LastPosition, WARP_TO_PLAYER_PADDING, randCount);
            characterActor.AI.SetHomePosition(spawnPositions[0], isWarp: true); // 위치 세팅
            characterActor.AI.ReadyToBattle();
            characterActor.AI.ChangeAutoRebirthDieState(); // 죽었을 때 자동 부활

            entity.ReadyToBattle();

            if (IsPause)
            {
                characterActor.AI.Pause();
                characterActor.Movement.Pause();
            }

            return true;
        }

        private void RemoveSharingCharacter(int cid)
        {
            GhostPlayerEntity finded = sharingCharacterFactory.Find(cid);
            if (finded == null)
                return;

            finded.Skill.SetAntiSkillAuto(false); // 스킬 수동 모드 해제
            if (ReferenceEquals(GetCurrentPlayer(), finded))
            {
                SetCurrentPlayer(player); // 조종 대상이었다면 플레이어로 이동
            }
            finded.OnChangeHP -= OnShareCharacterChangeHp;

            Dispose(finded);
            sharingCharacterFactory.Recycle(finded);
        }

        private void AddCloneCharacter(IBattleSharingCharacter info)
        {
            SpawnSharingCharacter(info, info.CloneCharacterType);
        }

        private void RemoveCloneCharacter(SharingModel.CloneCharacterType type)
        {
            GhostPlayerEntity finded = cloneDic.ContainsKey(type) ? cloneDic[type] : null;
            if (finded == null)
                return;

            finded.Skill.SetAntiSkillAuto(false); // 스킬 수동 모드 해제
            if (ReferenceEquals(GetCurrentPlayer(), finded))
            {
                SetCurrentPlayer(player); // 조종 대상이었다면 플레이어로 이동
            }
            finded.OnChangeHP -= OnShareCharacterChangeHp;

            Dispose(finded);
            cloneCharacterFactory.Recycle(finded);
            cloneDic.Remove(type);
        }

        private void OnShareCharacterChangeHp(int current, int max)
        {
            foreach (var shareCharacter in sharingCharacterFactory)
            {
                int index = sharingCharacterFactory.IndexOf(shareCharacter);
                if (index == -1)
                    continue;

                uiQuickExpandMenu.SetShareCharacterCurrentHp(index, shareCharacter.CurHP, shareCharacter.MaxHP);
            }

            foreach (var item in cloneDic)
            {
                int index = sharingModel.GetCloneCharacterIndex(item.Key);
                if (index == -1)
                    continue;

                uiQuickExpandMenu.SetShareCharacterCurrentHp(index, item.Value.CurHP, item.Value.MaxHP);
            }
        }

        private void OnShareCharacterDespawnActor(UnitActor actor)
        {
            actor.AI.ChangeDieEmptyState(); // 죽었을 때 아무것도 하지 않음
            actor.Entity.OnDespawnActor -= OnShareCharacterDespawnActor;

            // 부활 타이머 이벤트 제거
            GhostPlayerActor ghostActor = actor as GhostPlayerActor;
            ghostActor.OnStartReviveTimer -= OnShareCharacterDie;

            // 공격 시 카메라 흔들림 설정 제거
            CharacterEffectPlayer charaEffectPlayer = actor.EffectPlayer as CharacterEffectPlayer;
            charaEffectPlayer?.SetAttackImpulse(false);
        }

#if SPAWN_AGENT_CHARACTER
        private void SetAgentPlayers(IMultiPlayerInput[] packets)
        {
            if (packets == null)
                return;

            foreach (IMultiPlayerInput item in packets)
            {
                agentCharacterFactory.AddGhostPlayer(item, DamagePacket.DamageUnitType.Agent, item.Cid, item.JobLevel);
            }
        }

        /// <summary>
        /// 동료 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnAgentCharacters()
        {
            Vector3 posCenter;
            float distance;

            UnitActor summonMonsterActor = summonMonster.GetActor();
            if (summonMonsterActor)
            {
                posCenter = summonMonsterActor.LastPosition;
                distance = summonMonsterActor.Appearance.GetRadius() + WARP_TO_MVP_MONSTER_PADDING;
            }
            else
            {
                posCenter = player.LastPosition;
                distance = SEND_POSITION_MAGNITUDE;
            }

            int size = agentCharacterFactory.size;
            Vector3[] spawnPositions = map.GetAroundPosition(posCenter, distance, size);
            for (int i = 0; i < size; i++)
            {
                //Debug.LogError($"2.장착 중인 동료={agentCharacterFactory[i].GetName()}");
                if (impl.Add(agentCharacterFactory[i], isEnemy: false))
                {
                    //Debug.LogError($"3.장착 중인 동료={agentCharacterFactory[i].GetName()}");
                    agentCharacterFactory[i].SetExtraOption(ExtraBattleOptionType.PlusMoveSpd, CHARACTER_PLUS_MOVE_SPEED); // 강제이속증가
                    UnitActor characterActor = agentCharacterFactory[i].SpawnActor(); // 유닛 소환
                    characterActor.AI.SetHomePosition(spawnPositions[i], isWarp: true); // 위치 세팅
                    characterActor.AI.ReadyToBattle();
                    agentCharacterFactory[i].ReadyToBattle();
                    characterActor.AI.SetTarget(summonMonsterActor);

                    if (isPause)
                    {
                        characterActor.AI.Pause();
                        characterActor.Movement.Pause();
                    }

                    yield return Timing.WaitForOneFrame;
                }
            }
        }
#endif

        /// <summary>
        /// 셰어캐릭터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnSharingCharacters(IBattleSharingCharacter[] shareInfos, IBattleSharingCharacter[] cloneInfos)
        {
            foreach (IBattleSharingCharacter item in shareInfos)
            {
                if (SpawnSharingCharacter(item, item.CloneCharacterType))
                    yield return Timing.WaitForOneFrame;
            }

            foreach (IBattleSharingCharacter item in cloneInfos.OrEmptyIfNull())
            {
                if (SpawnSharingCharacter(item, item.CloneCharacterType))
                    yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 스테이지 메뉴 버튼 업데이트
        /// </summary>
        private void UpdateStageMenu()
        {
            SetHasSummonMonster(summonMonster != null && summonMonster.IsValid());

            if (battleStageMenu == null)
                return;

            battleStageMenu.SetActiveBossSummon(summonMonsterType == SummonMonsterType.BossMonster);
            battleStageMenu.SetActiveMvpSummon(summonMonsterType == SummonMonsterType.MvpMonster);
            battleStageMenu.SetActiveAssemble(!isAssemble && summonMonster != null && summonMonster.IsValid() && summonMonster.type == UnitEntityType.MvpMonster); // 셰어링 기능이 오픈되었을 때
        }

        private void UpdateMazeSkillList()
        {
            if (uiBattleMazeSkillList && IsPlayer(GetCurrentPlayer()))
                uiBattleMazeSkillList.SetCharacter(player);
        }

        private void OnChangedJob(bool isInit)
        {
            if (isInit)
                return;

            // 만약 Fail UI가 켜져있을 경우에 대한 처리
            // Tutorial 에 한하여 죽어도 전직을 할 수 있기 때문에
            if (uiBattleFail.IsVisible)
                uiBattleFail.Confirm();
            if (uiBattleFailBoss.IsVisible)
                uiBattleFailBoss.Confirm();
        }

        private void OnSelectSkillToggle()
        {
            var currentPlayer = GetCurrentPlayer();
            bool isPlayer = IsPlayer(currentPlayer);

            if (!isPlayer) // 셰어링 캐릭터 조작중이라면 내 플레이어로 돌아감
            {
                SetCurrentPlayer(null);
                return;
            }

            skillModel.SetAntiSkillAuto(!skillModel.IsAntiSkillAuto);
            UpdateSkillToggle();
        }

        private void UpdateSkillToggle()
        {
            var currentPlayer = GetCurrentPlayer();
            bool isPlayer = IsPlayer(currentPlayer);

            uiSkillListToggle.Show(isPlayer ? currentPlayer.Skill.IsAntiSkillAuto : true, isControl: !isPlayer);
            if (currentPlayer.Skill.IsAntiSkillAuto)
            {
                uiBattleMazeSkillList.Show();
                uiBattleMazeSkillList.SetDraggableArea(UIBattleMazeSkillList.DraggableAreaType.Stage);
                uiBattleMazeSkillList.SetActivePlus(true);
            }
            else
            {
                uiBattleMazeSkillList.Hide();
            }
        }

        private void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
        {
            var currentPlayer = GetCurrentPlayer();
            bool isPlayer = IsPlayer(currentPlayer);

            switch (slotType)
            {
                case UIBattleNormalSkillSlot.SlotType.Skill:
                    if (currentPlayer.IsDie)
                        return;

                    UnitActor unitActor = currentPlayer.GetActor();
                    if (unitActor == null)
                        return;

                    int mpCost = currentPlayer.CurMp - info.MpCost;
                    if (mpCost < 0)
                    {
                        UI.ShowToastPopup(LocalizeKey._90142.ToText()); // 스킬 포인트가 부족합니다.
                        return;
                    }

                    unitActor.AI.SetInputSkill(info);
                    break;

                case UIBattleNormalSkillSlot.SlotType.Lock:
                    if (!isPlayer)
                        return;

                    skillModel.RequestBuySkillSlot().WrapNetworkErrors();
                    break;

                case UIBattleNormalSkillSlot.SlotType.Empty:
                    if (!isPlayer)
                        return;

                    if (!questModel.IsOpenContent(ContentType.Skill, isShowPopup: true))
                        return;

                    UI.Show<UISkill>();
                    break;
            }
        }

        private bool StartTutorial()
        {
            Pause();

            if (Tutorial.Run(TutorialType.FirstStage))
                return true;

            if (Tutorial.Run(TutorialType.SharingCharacterEquip))
                return true;

            Resume();

            if (Tutorial.Run(TutorialType.SkillLearn))
                return true;

            if (Tutorial.Run(TutorialType.SkillEquip))
                return true;

            if (Tutorial.Run(TutorialType.BossSummon))
                return true;

            if (Tutorial.Run(TutorialType.ItemEnchant))
                return true;

            if (Tutorial.Run(TutorialType.JobChange))
                return true;

            if (Tutorial.Run(TutorialType.Duel))
                return true;

            if (Tutorial.Run(TutorialType.MazeEnter)) // 미로입장 튜토리얼
                return true;

            if (Tutorial.Run(TutorialType.ShareControl2))
                return true;

            if (Tutorial.Run(TutorialType.TimePatrolOpen))
                return true;

            if (Tutorial.Run(TutorialType.GateOpen))
                return true;

            return false;
        }

        void OnFinishedTutorial(TutorialType tutorialType)
        {
            if (!StartTutorial() && registeredBossRewards != null)
            {
                uiResultClear.Show(registeredBossRewards, default, false);
                registeredBossRewards = null;
            }
        }

        /// <summary>
        /// 몬스터 처치시 MP 회복
        /// </summary>
        private void RecoveryMp(UnitEntity unit)
        {
            if (player == null)
                return;

            player.RecoveryMp(RECOVERY_MP);

            UnitActor actor = unit.GetActor();
            if (actor == null)
                return;
            actor.EffectPlayer.SpawnDropMana(unit.LastPosition, uiBattlePlayerStatus.GetMPTarget()).Play(); // MP 회복 연출
        }

        /// <summary>
        /// 캐릭터 스탯 초기화 (죽은 캐릭 부활)
        /// </summary>
        private void InitializeCharactersStatus()
        {
            for (int i = 0; i < unitList.size; i++)
            {
                if (unitList[i].IsEnemy)
                    continue;

                UnitActor actor = unitList[i].GetActor();
                if (actor == null)
                    continue;

                if (unitList[i].IsDie)
                {
                    actor.AI.ChangeState(AI.Transition.Rebirth); // 강제 부활
                }
                else
                {
                    unitList[i].ReadyToBattle(); // 초기화
                }

                actor.EffectPlayer.ShowHealEffect(); // Heal 이펙트 추가
            }
        }

        void OnShowUI(ICanvas canvas)
        {
            // 캐릭터 정보가 켜질 경우에는 셰어링캐릭터 조작 해제
            if (canvas is UICharacterInfo)
            {
                SetCurrentPlayer(null, isSkillAutoSetting: false);
            }
        }

        /// <summary>
        /// 셰어링 캐릭터 클릭 이벤트
        /// </summary>
        private void OnSelectSharingCharacter(int index, int cid)
        {
            GhostPlayerEntity finded;
            if (index < Constants.Size.SHARE_SLOT_SIZE)
            {
                finded = sharingCharacterFactory.Find(cid);
            }
            else
            {
                SharingModel.CloneCharacterType cloneType = sharingModel.GetCloneCharacterType(index);
                finded = cloneDic.ContainsKey(cloneType) ? cloneDic[cloneType] : null;
            }

            if (finded is null)
            {
#if UNITY_EDITOR
                Debug.LogError("찾는 셰어링 캐릭터가 없다. = " + cid);
#endif
                return;
            }

            if (player.IsDie) // 내 플레이어가 죽어있다면 조종 대상 이동 불가
                return;

            if (finded.IsDie || finded.GetActor() is null) // 죽어있는 캐릭터로는 이동 불가
                return;

            SetCurrentPlayer(finded);
        }

        /// <summary>
        /// 현재 조종 중인 캐릭터 Entity 반환
        /// </summary>
        private CharacterEntity GetCurrentPlayer()
        {
            return selectedEntity ?? player;
        }

        private bool IsPlayer(CharacterEntity entity)
        {
            return ReferenceEquals(entity, player);
        }

        /// <summary>
        /// 조종 대상 변경
        /// </summary>
        private void SetCurrentPlayer(CharacterEntity entity, bool isSkillAutoSetting = true)
        {
            if (this.selectedEntity == null) // 이전에 선택되었던 Entity가 null이면 player를 의미한다.
                this.selectedEntity = player;

            if (ReferenceEquals(this.selectedEntity, entity))
                return;

            OnResetController(); // 이동 도중 플레이어가 바뀌면 허공에서 계속 달리게 된다.

            CharacterEffectPlayer charaEffectPlayer;

            // 이전 조종 캐릭터에 대한 처리
            if (this.selectedEntity != null)
            {
                // 내캐릭터가 아니면 무조건 자동 모드로 변경
                if (isSkillAutoSetting || !IsPlayer(selectedEntity))
                    selectedEntity.Skill.SetAntiSkillAuto(false); // 스킬 자동 모드로 변경

                // 공격 시 카메라흔들림 제거
                charaEffectPlayer = selectedEntity.GetActor()?.EffectPlayer as CharacterEffectPlayer;
                charaEffectPlayer?.SetAttackImpulse(false);

                // UnitCircle 제거
                charaEffectPlayer.HideUnitCircle();
            }

            this.selectedEntity = entity ?? player;
            bool isPlayer = IsPlayer(selectedEntity);
            charaEffectPlayer = selectedEntity.GetActor()?.EffectPlayer as CharacterEffectPlayer;

            // 조종 대상 변경
            uiDanger.ChangeCharacterControl(isPlayer);

            // 공격 시 카메라 흔들림 설정
            charaEffectPlayer?.SetAttackImpulse(true);

            // 자동 스킬 여부 설정
            if (isSkillAutoSetting)
                skillModel.SetAntiSkillAuto(false); // 플레이어 자동 모드로 변경

            if (!isPlayer)
            {
                if (questModel.IsOpenContent(ContentType.ShareControl, false)) // 조종이 가능할때만..
                    selectedEntity.Skill.SetAntiSkillAuto(true); // 조종 대상 수동 모드로 변경
            }

            // 카메라 대상 변경
            cameraController.SetPlayer(selectedEntity.GetActor()?.CachedTransform);

            // 스킬 조작 UI 업데이트
            uiBattleMazeSkillList.SetCharacter(selectedEntity);
            UpdateSkillToggle();

            // UnitCircle 설정
            charaEffectPlayer.ShowUnitCircle();

            // UIQuickExpandMenu 조종 아이콘 업데이트
            int slotCount = uiQuickExpandMenu.GetSlotCount();
            for (int i = 0; i < slotCount; ++i)
            {
                GhostPlayerEntity ghostEntity;
                if (i < Constants.Size.SHARE_SLOT_SIZE)
                {
                    ghostEntity = sharingCharacterFactory.size > i ? sharingCharacterFactory[i] : null;
                }
                else
                {
                    SharingModel.CloneCharacterType cloneCharacterType = sharingModel.GetCloneCharacterType(i);
                    ghostEntity = cloneDic.ContainsKey(cloneCharacterType) ? cloneDic[cloneCharacterType] : null;
                }

                bool isCurrentPlayer = !isPlayer && ReferenceEquals(ghostEntity, selectedEntity);
                uiQuickExpandMenu.SetShareCharacterSelectState(i, isCurrentPlayer);
            }
        }

        private void SetHasSummonMonster(bool hasSummonMonster)
        {
            StageEntry.hasSummonMonster = hasSummonMonster;
        }

        private void HideFailUI()
        {
            if (uiBattleFail && uiBattleFail.IsVisible)
                uiBattleFail.Hide();

            if (uiBattleFailBoss && uiBattleFailBoss.IsVisible)
                uiBattleFailBoss.Hide();
        }

        public bool IsAssemble()
        {
            return isAssemble;
        }

#if UNITY_EDITOR
        public void Test()
        {
            for (int i = unitList.size - 1; i >= 0; i--)
            {
                if (!unitList[i].IsEnemy)
                    continue;

                unitList[i].Die(null);
            }
        }
#endif
    }
}