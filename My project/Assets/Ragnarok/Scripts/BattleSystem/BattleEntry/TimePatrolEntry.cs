//#define TEST_TUTORIAL
using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TimePatrolEntry : BattleEntry
    {
        private const string TAG_REVIVE = "TimePatrolEntry_TAG_REVIVE";
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;

        private const int CHARACTER_PLUS_MOVE_SPEED = 1000; // 강제 이속 증가량

        /// <summary>
        /// 몬스터 처지시 회복되는 MP량
        /// </summary>
        private const int RECOVERY_MP = 1;

        /// <summary>
        /// 마지막 존 ID
        /// </summary>
        private const int LAST_ZONE_ID = 6;

        private enum ResultType
        {
            None,
            BossClear,
            PlayerDead,
        }

        public enum NextState
        {
            NextZone = 1,
            CurrentZone = 2,
            NextLevel = 3,
        }

        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly SharingModel sharingModel;
        private readonly CharacterModel characterModel;
        private readonly StatusModel statusModel;
        private readonly SkillModel skillModel;
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;
        private readonly TutorialModel tutorialModel;

        // <!-- Repositories --!>
        private readonly TimePatrolStageDataManager timePatrolStageDataRepo;
        private readonly TimePatrolBossDataManager timePatrolBossDataRepo;
        private readonly int timePatrolOpenStageId;

        // <!-- Managers --!>
        private readonly SoundManager soundManager;
        private readonly ConnectionManager connectionManager;

        // <!-- Scene Compositions --!>
        private readonly IBattlePool battlePool;
        private readonly List<PoolObject> poolObjectList;
        private readonly Buffer<UnitEntity> monsterBuffer;
        private readonly Dictionary<int, UnitEntity[]> monsterZoneDic; // 각 SpawnZone의 몬스터List

        private readonly float reviveDelayTime = 4f;

        // <!-- UIs --!>
        private UIMain uiMain;
        private UIMainTop uiMainTop;
        private UIMainShortcut uiMainShortcut;
        private UIController uiController;
        private UIDanger uiDanger;
        private UIAutoEquip uiAutoEquip;
        private UIWorldBossAlarm uiWorldBossAlarm;
        private UIGoodsView uiGoodsView;
        private UIMvpTimer uiMvpTimer;
        private UIBossComing uiBossComing;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleFail uiBattleFail;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattleFailBoss uiBattleFailBoss;
        private UIJobChangeMenu uiJobChangeMenu;
        private UISkillListToggle uiSkillListToggle;
        private UICamSelect uiCamSelect;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIChatPreview uiChatPreview;
        private UISummonTimer uiSummonTimer;
        private UIDialogue uiDialogue;
        private UIChoice uiChoice;
        private UITimePatrolQuest uiTimePatrolQuest;
        private UIBattleShare2nd uiBattleShare2nd;
        private UIPowerSavingMenu uiPowerSavingMenu;

        // <!-- Temp Data --!>
        private Map map; // 맵
        private readonly Dictionary<int, TimePatrolZone> zoneDic;
        private readonly Buffer<NpcSpawner> npcBuffer;
        private TimePatrolStageData currentData;
        private UnitEntity summonMonster; // 보스 몬스터
        private ResultType resultType;
        private long bossCoolTime;
        private bool isRecoonect;
        private bool isSummoningBoss; //소환중 플래그
        private bool isClearedBoss; // 보스 처치 여부
        private bool isOutBattleZone;
        private bool isOutNonBattleZone;
        private bool isNextLevel;
        private TutorialPlayerZone tutorialPlayerZone;
        private TutorialObstacleZone tutorialObstacleZone;
        private TutorialNpcZone[] tutorialNpcZones;
        private bool isWarp;
#if TEST_TUTORIAL
        bool isNeedTutorial = true;
#endif
        private bool isMoveNPC;

        public TimePatrolEntry() : base(BattleMode.TimePatrol)
        {
            player.SetState(UnitEntity.UnitState.Stage);

            dungeonModel = player.Dungeon;
            sharingModel = player.Sharing;
            characterModel = player.Character;
            statusModel = player.Status;
            skillModel = player.Skill;
            inventoryModel = player.Inventory;
            questModel = player.Quest;
            tutorialModel = player.Tutorial;

            timePatrolStageDataRepo = TimePatrolStageDataManager.Instance;
            timePatrolBossDataRepo = TimePatrolBossDataManager.Instance;
            timePatrolOpenStageId = BasisType.TP_OPEN_STAGE_ID.GetInt();

            soundManager = SoundManager.Instance;
            connectionManager = ConnectionManager.Instance;

            battlePool = BattlePoolManager.Instance;
            poolObjectList = new List<PoolObject>();
            monsterBuffer = new Buffer<UnitEntity>();
            monsterZoneDic = new Dictionary<int, UnitEntity[]>(IntEqualityComparer.Default);
            npcBuffer = new Buffer<NpcSpawner>();

            zoneDic = new Dictionary<int, TimePatrolZone>(IntEqualityComparer.Default);
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            // <!-- 모든 데이터 폐기 --!>
            map = null;
            zoneDic.Clear();
            currentData = null;
            npcBuffer.Clear();
            monsterBuffer.Clear();

            // 오브젝트 풀 폐기
            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.PlusMoveSpd); // 강제이속증가 되돌리기
            player.SetForceStatus(ForceStatusType.BuffItemOptionOff); // 버프아이템 Off

            skillModel.SetAntiSkillAuto(false); // 스킬 Auto On
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            // <!-- 이벤트 추가 --!>
            dungeonModel.OnMonsterItemDrop += OnMonsterItemDrop;
            dungeonModel.OnTimePatrolBossMonsterItemDrop += OnBossMonsterItemDrop;
            dungeonModel.OnAppearTimePatrolBossMonster += OnAppearBossMonster;
            sharingModel.OnUpdateSharingState += Restart;
            skillModel.OnChangeSkillStatus += UpdateMazeSkillList;
            skillModel.OnUpdateSkillSlot += UpdateMazeSkillList;
            inventoryModel.OnUpdateEquipment += UpdateMazeSkillList;
            player.OnReloadStatus += UpdateMazeSkillList;
            characterModel.OnChangedJob += OnChangedJob;
            connectionManager.OnReconnect += OnReconnect;
            player.OnSpawnActor += OnSpawnPlayer;
            player.OnDespawnActor += OnDespawnPlayer;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            // <!-- 이벤트 제거 --!>
            dungeonModel.OnMonsterItemDrop -= OnMonsterItemDrop;
            dungeonModel.OnTimePatrolBossMonsterItemDrop -= OnBossMonsterItemDrop;
            dungeonModel.OnAppearTimePatrolBossMonster -= OnAppearBossMonster;
            sharingModel.OnUpdateSharingState -= Restart;
            skillModel.OnChangeSkillStatus -= UpdateMazeSkillList;
            skillModel.OnUpdateSkillSlot -= UpdateMazeSkillList;
            inventoryModel.OnUpdateEquipment -= UpdateMazeSkillList;
            player.OnReloadStatus -= UpdateMazeSkillList;
            characterModel.OnChangedJob -= OnChangedJob;
            connectionManager.OnReconnect -= OnReconnect;
            player.OnSpawnActor -= OnSpawnPlayer;
            player.OnDespawnActor -= OnDespawnPlayer;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            // <!-- UI 생성, 이벤트 추가, 초기화 --!>
            uiMain = UI.Show<UIMain>();
            uiMainTop = UI.Show<UIMainTop>();
            uiMainShortcut = UI.Show<UIMainShortcut>();
            uiController = UI.Show<UIController>();
            uiDanger = UI.Show<UIDanger>();
            uiAutoEquip = UI.Show<UIAutoEquip>();
            uiWorldBossAlarm = UI.Show<UIWorldBossAlarm>();
            uiGoodsView = UI.Show<UIGoodsView>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBossComing = UI.Show<UIBossComing>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleFail = UI.Show<UIBattleFail>();
            uiBattleFailBoss = UI.Show<UIBattleFailBoss>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiJobChangeMenu = UI.Show<UIJobChangeMenu>();
            uiSkillListToggle = UI.Show<UISkillListToggle>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiChatPreview = UI.Show<UIChatPreview>();
            uiSummonTimer = UI.Show<UISummonTimer>();
            uiDialogue = UI.Show<UIDialogue>();
            uiChoice = UI.Show<UIChoice>();
            uiTimePatrolQuest = UI.Show<UITimePatrolQuest>();
            uiBattleShare2nd = UI.Show<UIBattleShare2nd>();
            uiPowerSavingMenu = UI.Show<UIPowerSavingMenu>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleFail.OnConfirm += RebirthPlayer;
            uiBattleFailBoss.OnConfirm += RebirthPlayer;
            uiMvpTimer.OnFinish += OnTimerFinish;
            uiSkillListToggle.OnSelectToggle += OnSelectSkillToggle;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnSelectSkillToggle;
            uiSummonTimer.OnFinish += OnSummonTimerFinish;
            uiBattleMenu.OnNpcMove += OnNpcMove;
            uiBattleMenu.OnExit += OnBattleExit;
            uiChoice.OnChoice += OnChoice;

            cameraController.SetView(DEFAULT_VIEW); // Quater로 전환

            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(true);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit, UIBattleMenu.MenuContent.Buff, UIBattleMenu.MenuContent.Maze, UIBattleMenu.MenuContent.Square, UIBattleMenu.MenuContent.NpcMove, UIBattleMenu.MenuContent.Explore, UIBattleMenu.MenuContent.Roulette, UIBattleMenu.MenuContent.JobLevel, UIBattleMenu.MenuContent.FirstPayment, UIBattleMenu.MenuContent.CustomerReward);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattleFail.Hide();
            uiBattleFailBoss.Hide();
            uiMvpTimer.Hide();
            uiBattleBossHp.SetOffset();
            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.Hide();
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattleMazeSkillList.Hide();
            uiSummonTimer.Hide();
            uiDialogue.Hide();
            uiChoice.Hide();
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            // <!-- UI 삭제, 이벤트 해제 --!>

            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleFail.OnConfirm -= RebirthPlayer;
            uiBattleFailBoss.OnConfirm -= RebirthPlayer;
            uiMvpTimer.OnFinish -= OnTimerFinish;
            uiSkillListToggle.OnSelectToggle -= OnSelectSkillToggle;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnSelectSkillToggle;
            uiSummonTimer.OnFinish -= OnSummonTimerFinish;
            uiBattleMenu.OnNpcMove -= OnNpcMove;
            uiBattleMenu.OnExit -= OnBattleExit;
            uiChoice.OnChoice -= OnChoice;

            uiBattleBossHp.SetOffset();

            UI.Close<UIMain>();
            UI.Close<UIMainTop>();
            UI.Close<UIMainShortcut>();
            UI.Close<UIController>();
            UI.Close<UIDanger>();
            UI.Close<UIAutoEquip>();
            UI.Close<UIWorldBossAlarm>();
            UI.Close<UIGoodsView>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBossComing>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleFail>();
            UI.Close<UIBattleFailBoss>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIJobChangeMenu>();
            UI.Close<UISkillListToggle>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIChatPreview>();
            UI.Close<UISummonTimer>();
            UI.Close<UIDialogue>();
            UI.Close<UITimePatrolQuest>();
            UI.Close<UIBattleShare2nd>();
            UI.Close<UIPowerSavingMenu>();

            uiMain = null;
            uiMainTop = null;
            uiMainShortcut = null;
            uiController = null;
            uiDanger = null;
            uiAutoEquip = null;
            uiWorldBossAlarm = null;
            uiGoodsView = null;
            uiMvpTimer = null;
            uiBossComing = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiBattlePlayerStatus = null;
            uiBattleFail = null;
            uiBattleFailBoss = null;
            uiBattleBossHp = null;
            uiJobChangeMenu = null;
            uiSkillListToggle = null;
            uiCamSelect = null;
            uiBattleMazeSkillList = null;
            uiChatPreview = null;
            uiSummonTimer = null;
            uiDialogue = null;
            uiTimePatrolQuest = null;
            uiBattleShare2nd = null;
            uiPowerSavingMenu = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            // <!-- 시작 및 언어 변경 시 호출 --!>
            string mainText = currentData.name_id.ToText();
            uiBattleInfo.Set(mainText);
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            // <!-- 기존 전투에서 사용한 데이터 폐기 --!>

            ClearNormalMonster();
            StopTimer(); // 타이머 종료
            DisposeSummonMonster(); // 보스 몬스터 폐기
            StopSummonTimer();
            resultType = ResultType.None;
            HideFailUI();

            Debug.Log($"[타임패트롤] EndBattle");
        }

        private void Restart()
        {
            isSummoningBoss = false;
            impl.StartBattle(BattleMode.TimePatrol, isStartFadeIn: false);
        }

        void OnReconnect()
        {
            isRecoonect = true;
            isWarp = true; // 재접속 시 처음 위치로 워프 필요
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            return base.YieldExitBattle();
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            // <!-- 전투 시작 --!>
            int id = Mathf.Max(1, dungeonModel.LastEnterTimePatrolId);
            return YieldEnterBattle(id);
        }

        /// <summary>
        /// 특정 id로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            // <!-- 특정 id로 전투 시작 --!>
            TimePatrolStageData data = timePatrolStageDataRepo.Get(id);
            if (data == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 입장 조건 체크
            if (dungeonModel.FinalStageId < timePatrolOpenStageId)
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 사용 중인 쉐어 캐릭터 해제
            if (sharingModel.HasSharingCharacters())
                sharingModel.RequestShareCharacterRelease(isSave: false).WrapNetworkErrors();

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            switch (sharingState)
            {
                case SharingModel.SharingState.None:
                    if (IsNeedTutorial())
                    {
                        IsSuccessEnter = true; // 튜토리얼이 필요한 경우 입장 통과
                        break;
                    }

                    Task<Response> task = dungeonModel.RequestTimePatrolStart(id); // 전투 시작시 요청
                    yield return Timing.WaitUntilTrue(task.IsComplete);
                    Response response = task.Result;
                    IsSuccessEnter = response.isSuccess;
                    isOutNonBattleZone = false;

                    if (IsSuccessEnter)
                    {
                        bossCoolTime = response.GetLong("1"); // 보스 소환까지 남은시간 - 클라에서 자동으로 소환 요청
                        Debug.Log($"보스 소환 남은시간={bossCoolTime}");
                    }

                    Debug.Log($"[타임패트롤] YieldEnterBattle");
                    break;

                case SharingModel.SharingState.Sharing:
                    IsSuccessEnter = true;
                    break;

                case SharingModel.SharingState.StandByReward:
                    IsSuccessEnter = true;
                    break;
            }

            if (!IsSuccessEnter)
                yield break;

            // 입장 성공시 데이터 세팅
            // 미리 세팅할 경우 입장 실패해도 데이터가 변경됨
            if (IsSuccessEnter)
            {
                if (currentData != null)
                {
                    // 타임 패트롤 레벨이 달라질때 캐릭터 위치 워프
                    if (currentData.level != data.level)
                    {
                        isWarp = true;
                    }
                }

                currentData = data;
            }
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            string sceneName = currentData.scene_name; // 씬 이름
            string bgmName = currentData.bgm;
            bool isChangeScene = !SceneLoader.IsCheckCurrentScene(sceneName) || (map == null); // 씬 변경 여부 - 씬이 다르거나, 맵이 존재하지 않을 경우
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            if (isChangeScene)
                npcBuffer.Clear();

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            // <!-- 맵 정보 가져오기 --!>
            foreach (GameObject root in roots)
            {
                if (map == null)
                    map = root.GetComponent<Map>();

                TimePatrolZone[] zones = root.GetComponentsInChildren<TimePatrolZone>();
                foreach (var zone in zones)
                {
                    int zoneIndex = zone.GetIndex();
                    if (!zoneDic.ContainsKey(zoneIndex))
                        zoneDic.Add(zoneIndex, zone);
                }

                var npcs = root.GetComponentsInChildren<NpcSpawner>();
                if (npcs != null)
                    npcBuffer.AddRange(npcs);

                if (tutorialPlayerZone == null)
                    tutorialPlayerZone = root.GetComponentInChildren<TutorialPlayerZone>();

                if (tutorialObstacleZone == null)
                    tutorialObstacleZone = root.GetComponentInChildren<TutorialObstacleZone>();

                if (tutorialNpcZones == null || tutorialNpcZones.Length == 0)
                    tutorialNpcZones = root.GetComponentsInChildren<TutorialNpcZone>();
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

            uiBattleMenu.SetNpcMoveIcon(isGrayScale: true);
            uiBattlePlayerStatus.SetPlayer(player);

            // 튜토리얼 중
            if (IsNeedTutorial())
                return;

            isSummoningBoss = false;
            isClearedBoss = false;

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();
            if (sharingState == SharingModel.SharingState.None)
            {
                RestartSummonTimer(bossCoolTime);
            }

            CloseDoorAll();

            if (isNextLevel)
            {
                isNextLevel = false;
                var actor = player.GetActor();
                if (zoneDic.TryGetValue(currentData.zone_id, out TimePatrolZone zone))
                {
                    Vector3 playerPosition = zone.GetPlayerPosition();
                    if (actor != null)
                        actor.AI.SetHomePosition(playerPosition, isWarp: true); // Home 세팅
                }
            }

            Debug.Log($"[타임패트롤] OnReady");
        }

        private void RestartSummonTimer(long time)
        {
            if (uiSummonTimer != null)
            {
                Debug.Log($"[타임패트롤] RestartSummonTimer={time}");
                uiSummonTimer.Initialize(time);
                uiSummonTimer.Show();
                uiSummonTimer.RestartTimer();
            }
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            // <!-- FadeUI로 가려진 상태에서 씬 구성 (유닛 배치 등) --!>
            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG); // 캐릭터 생성       

            // 튜토리얼 중
            if (IsNeedTutorial())
            {
                tutorialObstacleZone.Open(); // 튜토리얼 길막해제

                // 튜토리얼 Npc 소환
                if (tutorialNpcZones != null)
                {
                    foreach (var item in tutorialNpcZones)
                    {
                        item.Spawn();
                    }
                }
            }
            else
            {
                tutorialObstacleZone.Close(); // 튜토리얼 길막설정

                // 튜토리얼 Npc 소환 해제
                if (tutorialNpcZones != null)
                {
                    foreach (var item in tutorialNpcZones)
                    {
                        item.Despawn();
                    }
                }

                // Npc 소환
                foreach (NpcSpawner spawner in npcBuffer)
                {
                    spawner.Spawn();
                }

                yield return Timing.WaitUntilDone(YieldSpawnTimeSaveZone(), TAG);

                SharingModel.SharingState sharingState = sharingModel.GetSharingState();

                switch (sharingState)
                {
                    case SharingModel.SharingState.None:
                        cameraController.RemoveMask(Layer.ALLIES, Layer.ENEMY); // 카메라 레이어 설정
                        yield return Timing.WaitUntilDone(YieldSpawnMonster(), TAG); // 몬스터 생성
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

            Debug.Log($"[타임패트롤] YieldComposeScene");
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            isMoveNPC = false;
            isSummoningBoss = false;
            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            UpdateMazeSkillList();
            UpdateSkillToggle();

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                playerActor.EffectPlayer.ShowUnitCircle();
            }

            // 튜토리얼
            if (IsNeedTutorial())
            {
                OpenDoor(currentData.zone_id);
                Timing.RunCoroutine(YieldTutorialTalk(default), TAG);
            }
            else
            {
                Tutorial.Run(TutorialType.ShareVice2ndOpen);
            }

            Debug.Log($"[타임패트롤] OnAllReady");
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity.type == UnitEntityType.Player)
            {
                UpdateMazeSkillList();

                uiBattleFail.Show(UIBattleFail.ConfirmType.Rebirth, player); // 플레이어 부활 처리

                if (summonMonster != null)
                {
                    if (IsFocus)
                    {
                        // 중복 프로토콜을 막기 위함
                        if (resultType == ResultType.None)
                        {
                            resultType = ResultType.PlayerDead; // 플레이어 사망

                            dungeonModel.RequestTimePatrolBossClear(false, currentData.id, summonMonster.clientUID, summonMonster.damagePacket).WrapNetworkErrors();
                        }
                    }
                }
            }
            else if (unitEntity.type == UnitEntityType.NormalMonster)
            {
                MonsterEntity monsterEntity = unitEntity as MonsterEntity;

                // 보스가 소환한 몬스터면 아무것도 하지 않는다.
                if (!monsterEntity.IsBossSpawnMonster)
                {
                    RebirthMonster(unitEntity); // 일반몬스터 부활 처리

                    if (IsFocus)
                    {
                        dungeonModel.RequestTimePatrolNormalMonsterDrop(unitEntity.battleUnitInfo.Id, unitEntity.clientUID, monsterEntity.stageId, unitEntity.damagePacket).WrapNetworkErrors();
                    }
                }
            }
            else if (unitEntity.type == UnitEntityType.BossMonster)
            {
                StopTimer();

                if (IsFocus)
                {
                    // 중복 프로토콜을 막기 위함
                    if (resultType == ResultType.None)
                    {
                        resultType = ResultType.BossClear;
                        dungeonModel.RequestTimePatrolBossClear(true, currentData.id, unitEntity.clientUID, unitEntity.damagePacket).WrapNetworkErrors();
                    }
                }
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

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            if (actor.IsPlayingActiveSkill()) // 액티브스킬 사용 중 이동 불가
                return;

            if (actor.AI.IsInputMove() == false) // OnStartController를 거치지 않고 Drag상태로 시작한 경우, 프로펠러 현상을 방지.
            {
                actor.AI.SetInputMove(isControl: true);
            }

            if (isMoveNPC)
            {
                isMoveNPC = false;
                actor.AI.StartAI();
            }

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);
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
        }

        /// <summary>
        /// 플레이어 부활
        /// </summary>
        private void RebirthPlayer()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.AI.ChangeState(AI.Transition.Rebirth);

            player.SetCurrentMp(player.MaxMp);

            soundManager.PlaySfx("[SYSTEM] Gacha_Result_Window");
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

        private void UpdateMazeSkillList()
        {
            if (uiBattleMazeSkillList)
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
            skillModel.SetAntiSkillAuto(!skillModel.IsAntiSkillAuto);
            UpdateSkillToggle();
        }

        private void UpdateSkillToggle()
        {
            uiSkillListToggle.Show(player.Skill.IsAntiSkillAuto);
            if (player.Skill.IsAntiSkillAuto)
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
            switch (slotType)
            {
                case UIBattleNormalSkillSlot.SlotType.Skill:
                    if (player.IsDie)
                        return;

                    UnitActor unitActor = player.GetActor();
                    if (unitActor == null)
                        return;

                    int mpCost = player.CurMp - info.MpCost;
                    if (mpCost < 0)
                    {
                        UI.ShowToastPopup(LocalizeKey._90142.ToText()); // 스킬 포인트가 부족합니다.
                        return;
                    }

                    unitActor.AI.SetInputSkill(info);
                    break;

                case UIBattleNormalSkillSlot.SlotType.Lock:

                    skillModel.RequestBuySkillSlot().WrapNetworkErrors();
                    break;

                case UIBattleNormalSkillSlot.SlotType.Empty:

                    if (!questModel.IsOpenContent(ContentType.Skill, isShowPopup: true))
                        return;

                    UI.Show<UISkill>();
                    break;
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            UnitActor actor;
            if (impl.Add(player, isEnemy: false))
            {
                isWarp = true; // 플레이어 생성시 초기 위로 워프
                // 최초 한 번만 하기 위해 여기에 세팅
                player.SetExtraOption(ExtraBattleOptionType.PlusMoveSpd, CHARACTER_PLUS_MOVE_SPEED); // 강제이속증가
                player.SetForceStatus(ForceStatusType.BuffItemOptionOn); // 버프아이템 On

                actor = player.SpawnActor(); // 유닛 소환
                actor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);

                cameraController.SetPlayer(actor.CachedTransform); // 카메라 타겟 세팅
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                actor = player.GetActor();
            }

            Vector3 playerPos = Vector3.zero;
            if (IsNeedTutorial())
            {
                isWarp = true; // 튜토리얼 시작시 플레이어 위치 배로 워프
                if (tutorialPlayerZone == null)
                {
                    Debug.LogError("튜토리얼 플레이어 Zone이 음슴");
                }
                else
                {
                    playerPos = tutorialPlayerZone.GetPosition();
                }
            }
            else
            {
                if (zoneDic.TryGetValue(currentData.zone_id, out TimePatrolZone zone))
                {
                    playerPos = zone.GetPlayerPosition();
                }
                else
                {
                    Debug.LogError($"해당 Zone 이 음슴: {nameof(currentData.zone_id)} = {currentData.zone_id}");
                }
            }

            actor.AI.SetHomePosition(playerPos, isWarp: false); // Home 세팅
            if (isWarp)
            {
                isWarp = false;
                actor.AI.WarpToHomePosition(); // 바로 Warp
            }
        }

        /// <summary>
        /// 문지역 이펙트 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnTimeSaveZone()
        {
            foreach (TimePatrolZone zone in zoneDic.Values)
            {
                if (zone.IsDoorEffect())
                    continue;

                PoolObject timeSaveZone = battlePool.SpawnTimeSaveZone(zone.GetSpawn());
                AddPoolObject(timeSaveZone);
                yield return Timing.WaitForOneFrame;
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
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster()
        {
            if (zoneDic.TryGetValue(currentData.zone_id, out TimePatrolZone zone))
            {
                int zoneCount = zone.ZoneCount;
                for (int i = 0; i < zoneCount; i++)
                {
                    yield return Timing.WaitUntilDone(YieldSpawnMonster(i), TAG);
                }
            }
        }

        private void YieldSpawnMonsterInstantly()
        {
            if (zoneDic.TryGetValue(currentData.zone_id, out TimePatrolZone zone))
            {
                int zoneCount = zone.ZoneCount;
                for (int i = 0; i < zoneCount; i++)
                {
                    var enumerator = YieldSpawnMonster(i);
                    while (enumerator.MoveNext()) ;

                    foreach (UnitEntity item in monsterZoneDic[i])
                    {
                        UnitActor actor = item.GetActor();
                        if (actor == null)
                            continue;

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
        }

        /// <summary>
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster(int zoneIndex)
        {
            int monsterLevel = currentData.normal_monster_level;

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

            if (zoneDic.TryGetValue(currentData.zone_id, out TimePatrolZone zone))
            {
                UnitEntity[] entities = monsterBuffer.GetBuffer(isAutoRelease: true);
                Vector3[] positions = zone.GetMonsterPositions(zoneIndex, entities.Length); // 위치 반환
                for (int i = 0; i < positions.Length; i++)
                {
                    UnitActor monsterActor = entities[i].SpawnActor(); // 유닛 소환
                    monsterActor.AI.SetHomePosition(positions[i], isWarp: true); // 위치 세팅

                    if (IsPause)
                    {
                        monsterActor.AI.Pause();
                        monsterActor.Movement.Pause();
                    }

                    yield return Timing.WaitForOneFrame;
                }

                monsterZoneDic.Add(zoneIndex, entities);
            }
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

                actor.AI.ReadyToBattle();
                item.ReadyToBattle(); // 바로 전투 시작

                if (IsPause)
                {
                    actor.AI.Pause();
                    actor.Movement.Pause();
                }
            }
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
        /// 보스몬스터 아이템 드랍 결과
        /// </summary>
        void OnBossMonsterItemDrop(UnitEntity unit, RewardData[] rewards, RewardData[] wasted, long coolTime, bool isClear)
        {
            bossCoolTime = coolTime;

            if (isClear)
            {
                isClearedBoss = true;

                YieldSpawnMonsterInstantly();
                uiBattleMenu.SetNpcMoveIcon(isGrayScale: false);
                var ui = UI.Show<UIMvpResult>();
                ui.SetMode(UIMvpResult.Mode.TimePatrolBoss);
                ui.SetData(rewards, wasted);
                RestartSummonTimer(bossCoolTime); // 보스 소환 쿨타임 재시작
                RecoveryMp(unit); // MP 회복
                OpenDoor(currentData.zone_id); // 보스 클리어 문 개방          

                Quest.QuestProgress(QuestType.TIME_PATROL_ZONE_BOOS_KILL_COUNT, conditionValue: currentData.zone_id); // 타임패트롤 보스 처치
            }
            else
            {
                OnTimerFinish();
            }

        }

        void OpenDoor(int zoneId)
        {
            if (zoneDic.ContainsKey(zoneId))
                zoneDic[zoneId].OpenDoor();

            var actor = player.GetActor();
            if (actor != null)
                actor.Movement.AddMask(NavMeshArea.Door);
        }

        void OpenDoorAll()
        {
            foreach (var item in zoneDic.Values)
            {
                item.OpenDoor();
            }

            var actor = player.GetActor();
            if (actor != null)
                actor.Movement.AddMask(NavMeshArea.Door);
        }

        void CloseDoor(int index)
        {
            if (zoneDic.ContainsKey(index))
                zoneDic[index].CloseDoor();
        }

        void CloseDoorAll()
        {
            foreach (var item in zoneDic.Values)
            {
                item.CloseDoor();
            }
            var actor = player.GetActor();
            if (actor != null)
                actor.Movement.RemoveMask(NavMeshArea.Door);
        }

        void OnAppearBossMonster(int id, int remainTime)
        {
            TimePatrolBossData bossData = timePatrolBossDataRepo.Get(id);

            if (bossData == null)
                return;

            isSummoningBoss = false;
            bossCoolTime = bossData.boss_monster_cooltime;

            // 방어코드 기존의 소환 몬스터 제거
            if (summonMonster.IsValid())
            {
                StopTimer();
                DisposeSummonMonster();
            }

            ClearNormalMonster();

            uiBossComing.ShowBoss(bossData);
            uiMvpTimer.Initialize(remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();
            summonMonster = factory.CreateBossMonster(bossData);
            SpawnSummonMonster();
            StopSummonTimer();

            soundManager.PlaySfx("[SYSTEM] Congratulation");
        }

        private void SpawnSummonMonster()
        {
            if (!zoneDic.TryGetValue(currentData.zone_id, out TimePatrolZone zone))
                return;

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

            Vector3 homePosition = zone.GetMonsterPosition(Random.Range(0, zone.ZoneCount));
            summonMonsterActor.AI.SetHomePosition(homePosition, isWarp: true);
            summonMonsterActor.AI.ReadyToBattle();
            summonMonsterActor.EffectPlayer.ShowUnitCircle();

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
                UIBattleBossHp.MonsterType.NormalBoss,
                monsteData.cost,
                bossHud.CachedGameObject, 0);
            uiBattleBossHp.SetHp(summonMonster.CurHP, summonMonster.MaxHP);

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                playerActor.EffectPlayer.ShowBossTargetingArrow(summonMonster);
            }
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

            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ReleaseBossTargetingArrow();

            cameraController.SetView(DEFAULT_VIEW);
            cameraController.SetEffect(CameraController.CameraEffect.None);
        }

        void OnChangedSummonMonster(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }

        private void HideFailUI()
        {
            if (uiBattleFail && uiBattleFail.IsVisible)
                uiBattleFail.Hide();

            if (uiBattleFailBoss && uiBattleFailBoss.IsVisible)
                uiBattleFailBoss.Hide();
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
            YieldSpawnMonsterInstantly();
            RestartSummonTimer(bossCoolTime);
            StopTimer();
            DisposeSummonMonster();
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
        }

        private void OnSummonTimerFinish()
        {
            if (isSummoningBoss)
                return;

            isSummoningBoss = true;
            dungeonModel.RequestSummonTimePatrolBoss().WrapNetworkErrors();
        }

        private void StopSummonTimer()
        {
            if (uiSummonTimer != null)
            {
                uiSummonTimer.StopTimer();
                uiSummonTimer.Hide();
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        void OnSpawnPlayer(UnitActor actor)
        {
            actor.Movement.AddMask(NavMeshArea.Door); // 플레이어만 문 통과 가능 츠기
            actor.Movement.SetAvoidancePriority(Constants.Movement.PLAYER_AVOIDANCE_PRIORITY);
            actor.Movement.SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance);
            actor.Movement.OnPortal += OnPortal;
            actor.Movement.OnPortalOut += OnPortalOut;
        }

        /// <summary>
        /// 플레이어 소멸
        /// </summary>
        void OnDespawnPlayer(UnitActor actor)
        {
            actor.Movement.RemoveMask(NavMeshArea.Door); // 플레이어만 문 통과 가능 제거
            actor.Movement.SetAvoidancePriority(Constants.Movement.ETC_AVOIDANCE_PRIORITY);
            actor.Movement.SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance);
            actor.Movement.OnPortal -= OnPortal;
            actor.Movement.OnPortalOut -= OnPortalOut;
        }

        /// <summary>
        /// 포탈 충돌
        /// </summary>
        void OnPortal(GameObject portal)
        {
            if (portal == null)
                return;

            if (IsNeedTutorial())
            {
                TutorialNpcZone npcZone = portal.GetComponent<TutorialNpcZone>();
                if (npcZone != null)
                {
                    Timing.RunCoroutine(YieldTutorialTalk(npcZone.GetNpcType()), TAG);
                }

                return;
            }

            SharingModel.SharingState sharingState = sharingModel.GetSharingState();

            // 셰어 공유중이면 충돌 처리 리턴
            if (sharingState != SharingModel.SharingState.None)
                return;

            Debug.Log($"[타임패트롤] OnPortal={portal.name}");

            NpcSpawner npcSpanwer = portal.GetComponent<NpcSpawner>();
            if (npcSpanwer != null)
            {
                npcSpanwer.SpawnSign(OnNpcClicked);
                var actor = player.GetActor();
                actor.Movement.Stop();
                return;
            }

            BattleZone battleZone = portal.GetComponent<BattleZone>();

            if (battleZone != null)
            {
                BattleZoneType battleZoneType = battleZone.GetBattleZoneType();

                if (battleZoneType == BattleZoneType.BattleZone)
                {
                    EnterBattleZone(battleZone.GetIndex());
                }
                else if (battleZoneType == BattleZoneType.NonBattleZone)
                {
                    EnterNonBattleZone();
                }
            }
        }

        /// <summary>
        /// 배들존 입장
        /// </summary>
        void EnterBattleZone(int index)
        {
            if (IsAllReady)
                return;

            if (isOutBattleZone)
                return;

            TimePatrolStageData data = timePatrolStageDataRepo.Get(currentData.level, index);

            if (data != null)
                impl.StartBattle(BattleMode.TimePatrol, data.id, isStartFadeIn: false);
        }

        /// <summary>
        /// NPC 존 입장
        /// </summary>
        void EnterNonBattleZone()
        {
            Debug.Log($"[타임패트롤] EnterNonBattleZone, isClearedBoss={isClearedBoss}");
            if (!isClearedBoss)
                return;

            Debug.Log($"[타임패트롤] EnterNonBattleZone, IsAllReady={IsAllReady}");

            if (!IsAllReady)
                return;

            Debug.Log($"[타임패트롤] EnterNonBattleZone, isOutNonBattleZone={isOutNonBattleZone}");

            if (isOutNonBattleZone)
                return;

            isOutBattleZone = false;
            EndBattle();
            CloseDoorAll();
        }

        /// <summary>
        /// 포탈 충돌
        /// </summary>
        void OnPortalOut(GameObject portal)
        {
            Debug.Log($"[타임패트롤] OnPortalOut={portal.name}");

            if (IsNeedTutorial())
                return;

            NpcSpawner npcSpanwer = portal.GetComponent<NpcSpawner>();

            if (npcSpanwer != null)
                npcSpanwer.DespawnSign();

            BattleZone battleZone = portal.GetComponent<BattleZone>();

            if (battleZone != null)
            {
                BattleZoneType battleZoneType = battleZone.GetBattleZoneType();

                if (battleZoneType == BattleZoneType.BattleZone)
                {
                    isOutBattleZone = true;
                }
                else if (battleZoneType == BattleZoneType.NonBattleZone)
                {
                    isOutNonBattleZone = true;
                }
            }
        }

        void OnNpcClicked(NpcType npcType)
        {
            Debug.Log($"OnNpcClicked={npcType}");
            if (npcType == NpcType.ClockTower)
            {
                CloseDoorAll();
                Timing.RunCoroutine(YieldNpcTalk(), TAG);
            }
        }

        private IEnumerator<float> YieldNpcTalk()
        {
            yield return Timing.WaitUntilDone(YieldPlayerDialog(LocalizeKey._40300.ToText()), TAG); // 타임 키퍼를 물리쳤어!

            yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._40301.ToText()), TAG); // // 앞으로 그들은 끊임없이 너를 쫗아다닐거다...\n다음 스테이지로 넘어가겠나?

            bool isNextLevel = currentData.zone_id == LAST_ZONE_ID;
            bool isUpdate = currentData.level + 1 > BasisType.TP_MAX_LEVEL.GetInt();

            uiChoice.Show();
            uiChoice.SetTitle(LocalizeKey._40303.ToText()); // 스테이지 이동                       

            if (isNextLevel)
            {
                uiChoice.Set(LocalizeKey._40304.ToText(), LocalizeKey._40305.ToText(), LocalizeKey._40306.ToText(), isUpdate); // 다음 구역으로 이동한다., 현재 구역에 남아있는다., 다음 레벨로 이동한다.
            }
            else
            {
                uiChoice.Set(LocalizeKey._40304.ToText(), LocalizeKey._40305.ToText()); // 다음 구역으로 이동한다., 현재 구역에 남아있는다.
            }
        }

        private IEnumerator<float> YieldTutorialTalk(NpcType type)
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
            {
                Debug.LogError("Player Actor 음슴");
                yield break;
            }

            actor.EffectPlayer.ReleaseBossTargetingArrow();

            if (type == default)
            {
                uiController.Hide();

                yield return Timing.WaitForSeconds(1f);

                // 이곳은…내가 처음 눈을 뜬 곳이잖아?
                yield return Timing.WaitUntilDone(YieldPlayerDialog(LocalizeKey._26122.ToText()), TAG);
                // 앞에 누가 있을지 몰라..\n[62AEE4][c](앞으로 나가보자…)[/c][-]
                yield return Timing.WaitUntilDone(YieldPlayerDialog(LocalizeKey._26123.ToText()), TAG);

                uiController.Show();

                // 데비루치를 향한 이동 유도
                NpcEntity npcEntity = GetTutorialNpc(NpcType.Deviruchi);
                actor.EffectPlayer.ShowBossTargetingArrow(npcEntity);
            }
            else if (type == NpcType.Deviruchi)
            {
                DisableTutorialNpcPortal(NpcType.Deviruchi); // 충돌 해제

                uiController.Hide();

                // 안녕! 너에게 말해주고 싶은 말이 있어서 급하게 실체화를 시켰어!
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26124.ToText()), TAG);
                // Time Patrol을 가동했더니, 쉐어 바이스가 과부하 걸리면서 시공간이 일그러졌고, 결국 이곳에 오고 말았어...
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26125.ToText()), TAG);
                // 더 이상, 이 쉐어 바이스는 사용할 수 없을 것 같군..
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26126.ToText()), TAG);
                // 쉐어 바이스를 고칠 순 없는 거야?
                yield return Timing.WaitUntilDone(YieldPlayerDialog(LocalizeKey._26127.ToText()), TAG);
                // 쉐어 바이스를 고칠 수 있는 사람은 그 남자밖에 없어..
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26128.ToText()), TAG);
                // 지금 우리에게 가장 빠른 방법은\n고치는 것보다 더 좋은 2세대 쉐어 바이스를 얻는 것이 훨씬 빠르다구!
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26129.ToText()), TAG);
                // 2세대 쉐어 바이스는 시공간 중심부에 있는 시계탑 관리자한테만 얻을 수 있을 거야.
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26130.ToText()), TAG);
                // 내가 없어도 넌 잘 해낼 수 있다고 믿는다구!쿡쿡쿡..\n언젠가 또 만나자….!!
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, LocalizeKey._26131.ToText()), TAG);

                uiController.Show();

                DespawnTutorialNpc(NpcType.Deviruchi); // 소환 해제

                // 시계탑관리자를 향한 이동 유도
                NpcEntity npcEntity = GetTutorialNpc(NpcType.ClockTower);
                actor.EffectPlayer.ShowBossTargetingArrow(npcEntity);
            }
            else if (type == NpcType.ClockTower)
            {
                DisableTutorialNpcPortal(NpcType.ClockTower); // 충돌 해제
                RequestTutorial(); // 튜토리얼 완료

                uiController.Hide();

                // 난 시공간을 지키고 있는 수호자일세.\n자네가..Time Patrol을 가동한 미래의 영웅인가?
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26132.ToText()), TAG);
                // 그대가 강제로 미래로 향하려 한 걸 내가 막았다네.\n미래에는 1세대 쉐어 바이스를 사용할 수 없기 때문이지.
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26133.ToText()), TAG);
                // 2세대 쉐어 바이스는 어떻게 얻을 수 있죠?
                yield return Timing.WaitUntilDone(YieldPlayerDialog(LocalizeKey._26134.ToText()), TAG);
                // 지금이라도 그대를 위해 넘겨주고 싶지만, 시공간이 불안정해지면서\n몬스터들이 개입되어 모든 시공간을 멈췄기 때문에 줄 수가 없네.
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26135.ToText()), TAG);
                // 그러면 미래로 갈 수 없는 건가요?
                yield return Timing.WaitUntilDone(YieldPlayerDialog(LocalizeKey._26136.ToText()), TAG);
                // 꼭 그렇지 않네.\n시공간에 침투된 Lv.1 몬스터를 모두 처치하고, 나에게 오면 2세대 쉐어 바이스를 주겠네...
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26137.ToText()), TAG);
                // 주의 해야 할 것은 자네는 이곳에 허가 없이\nTime Patrol에 들어왔기 때문에 타임 키퍼가 자네의 시간을 뺏으러 올 걸세
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26138.ToText()), TAG);
                // 타임 키퍼를 처치하면 타임 슈트를 얻을 수 있지. 그 슈트야말로 미래에 생존하기 위한 필수품이라고 할 수 있다네!
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26139.ToText()), TAG);
                // 타임 슈트를 모두 모으면 자네의 능력이 더욱 강해질걸세.\n부디 그 남자의 말처럼 이루어지길… 
                yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._26140.ToText()), TAG);

                // 튜토리얼 완료될 때까지 기다림
                yield return Timing.WaitUntilFalse(IsNeedTutorial);

                uiController.Show();

                // 1초 대기
                yield return Timing.WaitForSeconds(1f);

                isWarp = true; // 플레이어 캐릭터 전투존으로 워프

                // 타임패트롤 재시작
                impl.StartBattle(BattleMode.TimePatrol);
            }
        }

        private void OnChoice(int index)
        {
            NextState nextState = index.ToEnum<NextState>();
            Timing.RunCoroutine(YieldEndTalk(nextState), TAG);
        }

        private IEnumerator<float> YieldEndTalk(NextState nextState)
        {
            yield return Timing.WaitUntilDone(YieldDialog(Npc.CLOCK_TOWER, LocalizeKey._40302.ToText()), TAG); // 알겠네 타임 키퍼를 물리치고 돌아오게나...

            int nextZone = currentData.zone_id + 1;
            if (nextZone > LAST_ZONE_ID)
                nextZone = 1;

            switch (nextState)
            {
                case NextState.NextZone:
                    OpenDoor(nextZone);
                    break;

                case NextState.CurrentZone:
                    OpenDoor(currentData.zone_id);
                    break;

                case NextState.NextLevel:
                    EnterNextLevel();
                    break;
            }
        }

        private IEnumerator<float> YieldDialog(string spriteName, string name, string dialog)
        {
            uiDialogue.Show();
            uiDialogue.SetActiveBtnSkip(false);
            string[] dialogs = dialog.Split(Constants.NPC.NPC_DIALOG_DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
            yield return uiDialogue.UntilShow(spriteName, name, dialogs);
            uiDialogue.Hide();
        }

        private IEnumerator<float> YieldDialog(Npc npc, string dialog)
        {
            return YieldDialog(npc.imageName, npc.nameLocalKey.ToText(), dialog);
        }

        private IEnumerator<float> YieldPlayerDialog(string dialog)
        {
            Npc npc = characterModel.Gender == Gender.Male ? Npc.NOVICE_MALE : Npc.NOVICE_FEMALE;
            return YieldDialog(npc.imageName, characterModel.Name, dialog);
        }

        /// <summary>
        /// NPC로 이동
        /// </summary>
        void OnNpcMove()
        {
            if (!isClearedBoss)
            {
                UI.ShowToastPopup(LocalizeKey._90292.ToText()); // 보스를 처지해야 NPC로 이동이 가능합니다.
                return;
            }

            if (player.IsDie)
                return;

            foreach (var npc in npcBuffer)
            {
                if (npc.GetNpcType() == NpcType.ClockTower)
                {
                    isMoveNPC = true;
                    var actor = player.GetActor();
                    actor.AI.EndAI();
                    actor.Movement.ForceSetDestination(npc.GetPosition());
                    break;
                }
            }
        }

        /// <summary>
        /// 다음 레벨 입장
        /// </summary>
        void EnterNextLevel()
        {
            int nextLevel = currentData.level + 1;

            if (nextLevel > BasisType.TP_MAX_LEVEL.GetInt())
                return;

            TimePatrolStageData data = timePatrolStageDataRepo.Get(nextLevel, zoneId: 1);

            if (data != null)
            {
                isNextLevel = true;
                impl.StartBattle(BattleMode.TimePatrol, data.id, isStartFadeIn: true);
            }
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
        /// 튜토리얼 진행 필요여부
        /// </summary>
        private bool IsNeedTutorial()
        {
#if TEST_TUTORIAL
            return isNeedTutorial;
#else
            return !Tutorial.HasAlreadyFinished(TutorialType.TimePatrolFirstEnter);
#endif
        }

        private void RequestTutorial()
        {
#if TEST_TUTORIAL
            isNeedTutorial = false;
#else
            Tutorial.RequestTutorialStep(TutorialType.TimePatrolFirstEnter); // 튜토리얼 완료
#endif
        }

        /// <summary>
        /// 튜토리얼 Npc 찾기
        /// </summary>
        private NpcEntity GetTutorialNpc(NpcType type)
        {
            if (tutorialNpcZones == null)
                return null;

            for (int i = 0; i < tutorialNpcZones.Length; i++)
            {
                if (tutorialNpcZones[i].GetNpcType() == type)
                    return tutorialNpcZones[i].GetNpcEntity();
            }

            return null;
        }

        /// <summary>
        /// 튜토리얼 Npc 충돌 해제
        /// </summary>
        private void DisableTutorialNpcPortal(NpcType type)
        {
            if (tutorialNpcZones == null)
                return;

            for (int i = 0; i < tutorialNpcZones.Length; i++)
            {
                if (tutorialNpcZones[i].GetNpcType() == type)
                {
                    tutorialNpcZones[i].SetEnablePortal(false);
                    break;
                }
            }
        }

        /// <summary>
        /// 튜토리얼 Npc 소환 해제
        /// </summary>
        /// <param name="type"></param>
        private void DespawnTutorialNpc(NpcType type)
        {
            if (tutorialNpcZones == null)
                return;

            for (int i = 0; i < tutorialNpcZones.Length; i++)
            {
                if (tutorialNpcZones[i].GetNpcType() == type)
                {
                    tutorialNpcZones[i].Despawn();
                    break;
                }
            }
        }

        async void OnBattleExit()
        {
            if (!await UI.SelectPopup(LocalizeKey._90316.ToText())) // 타임 패트롤을 나가시겠습니까?
                return;

            impl.StartBattle(BattleMode.Stage);
        }
    }
}