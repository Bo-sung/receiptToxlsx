using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GuildAttackEntry : BattleEntry
    {
        public enum PlayerState : byte
        {
            Idle = 0,
            Ready = 1, // 부활 대기 상태
            Die = 2,
        }

        public enum MonsterState
        {
            None = 0,
            Start = 1, // 시작
            Freeze = 7, // 상태이상 등으로 멈춰있음
            StartWait = 8, // Start 로 바뀌기 전에 잠시 대기
            Die = 9, // 죽음
            MoveTo = 11, // 이동
            AttackReady = 12, // 공격 모션 중 (Attack 으로 변경)
            Attack = 13, // 공격 (AttackReady 으로 변경)
        }

        public enum TrapState : byte
        {
            None = 0,
            Ready = 1,
            Appear = 2,
            Appeared = 3,
        }

        private const string SCENE_NAME = "Ayothaya";
        private const string BGM_NAME = "Prison_01";

        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;
        private const float MIN_POSITION_SYNC_THRESHOLD = 0.35f;

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

        // <!-- Models --!>
        private readonly GuildModel guildModel;
        private readonly CharacterModel characterModel;

        // <!-- Managers --!>
        private readonly ConnectionManager connectionManager;
        private readonly GuildSquareManager guildSquareManager;
        private readonly GameGraphicSettings graphicSettings;

        // <!-- Repository --!>
        private readonly int rebirthCooldownTime;

        // <!-- Scene Compositions --!>
        private readonly PlayerBotCreateQueue playerBotCreateQueue; // 플레이어봇 생성 대기열
        private readonly MonsterBotCreateQueue monsterBotCreateQueue; // 몬스터봇 생성 대기열
        private readonly Buffer<int> intBuffer;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIController uiController;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UICamSelect uiCamSelect;
        private UIMvpTimer uiMvpTimer;
        private UIWarning uiWarning;
        private UIBattleGuildAttackSkillList uiBattleGuildAttackSkillList;
        private UIBattleEmperium uiBattleEmperium;
        private UIBattlePotion uiBattlePotion;
        private UIBattleRebirth uiBattleRebirth;
        private UIShadowPlayerSettings uiShadowPlayerSettings;

        // <!-- Temp Data --!>=
        private bool isGiveUp; // 포기
        private UnitEntity emperium; // 엠펠리움

        public GuildAttackEntry() : base(BattleMode.GuildAttack)
        {
            player.SetState(UnitEntity.UnitState.GVG);

            guildModel = player.Guild;
            characterModel = player.Character;

            connectionManager = ConnectionManager.Instance;
            guildSquareManager = GuildSquareManager.Instance;
            graphicSettings = GameGraphicSettings.Instance;

            System.TimeSpan timeSpan = System.TimeSpan.FromMilliseconds(BasisType.GUILD_ATTACK_REBIRTH_COOL_TIME.GetInt());
            rebirthCooldownTime = (int)timeSpan.TotalSeconds;

            playerBotCreateQueue = new PlayerBotCreateQueue();
            monsterBotCreateQueue = new MonsterBotCreateQueue();
            intBuffer = new Buffer<int>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            if (isGiveUp)
            {
                // 엠펠리움 폐기
                if (emperium != null)
                {
                    emperium.ResetExtraOption(ExtraBattleOptionType.MaxHp);
                    Dispose(emperium);

                    emperium = null;
                }
            }

            player.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            player.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            player.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            playerBotCreateQueue.Clear(); // 플레이어봇 생성 대기열 삭제
            monsterBotCreateQueue.Clear(); // 몬스터봇 생성 대기열 삭제
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            guildSquareManager.OnPlayerGuildOut += GoToStage;
            guildSquareManager.OnPlayerMove += OnPlayerMove;
            guildSquareManager.OnPlayerHeal += OnPlayerHeal;
            guildSquareManager.OnPlayerDamage += OnPlayerDamage;
            guildSquareManager.OnPlayerDie += OnPlayerDie;
            guildSquareManager.OnPlayerAppear += OnPlayerAppear;
            guildSquareManager.OnPlayerChainAttack += OnPlayerChainAttack;

            guildSquareManager.OnMultiPlayerJoin += OnMultiPlayerJoin;
            guildSquareManager.OnMultiPlayerExit += OnMultiPlayerExit;
            guildSquareManager.OnMultiPlayerMove += OnMultiPlayerMove;
            guildSquareManager.OnMultiPlayerHeal += UpdateUnitHeal;
            guildSquareManager.OnMultiPlayerAttackMotion += OnMultiPlayerAttackMotion;
            guildSquareManager.OnMultiPlayerDamage += UpdateUnitDamage;
            guildSquareManager.OnMultiPlayerDie += OnMultiPlayerDie;
            guildSquareManager.OnMultiPlayerAppear += OnMultiPlayerAppear;
            guildSquareManager.OnMultiPlayerBuff += OnMultiPlayerBuff;

            guildSquareManager.OnMultiMonsterStatus += OnMultiMonsterStatus;
            guildSquareManager.OnMultiMonsterExit += OnMultiMonsterExit;
            guildSquareManager.OnMultiMonsterDamage += UpdateUnitDamage;
            guildSquareManager.OnMultiMonsterDie += OnMultiMonsterDie;
            guildSquareManager.OnMultiMonsterCrowdControl += OnMultiMonsterCrowdControl;
            guildSquareManager.OnMultiMonsterDotDamage += UpdateUnitDamage;

            guildSquareManager.OnUpdateSquareState += RefreshSquareState;
            guildSquareManager.OnUpdateEmperiumDamage += UpdateEmperiumHp;

            guildModel.OnUseGuildAttackPotion += ResetMp;

            player.OnUseSkill += RequestSkillMotion;
            player.OnApplySkill += RequestSkill;
            player.OnSpawnActor += OnSpawnPlayer;
            player.OnDespawnActor += OnDespawnPlayer;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            guildSquareManager.OnPlayerGuildOut -= GoToStage;
            guildSquareManager.OnPlayerMove -= OnPlayerMove;
            guildSquareManager.OnPlayerHeal -= OnPlayerHeal;
            guildSquareManager.OnPlayerDamage -= OnPlayerDamage;
            guildSquareManager.OnPlayerDie -= OnPlayerDie;
            guildSquareManager.OnPlayerAppear -= OnPlayerAppear;
            guildSquareManager.OnPlayerChainAttack -= OnPlayerChainAttack;

            guildSquareManager.OnMultiPlayerJoin -= OnMultiPlayerJoin;
            guildSquareManager.OnMultiPlayerExit -= OnMultiPlayerExit;
            guildSquareManager.OnMultiPlayerMove -= OnMultiPlayerMove;
            guildSquareManager.OnMultiPlayerHeal -= UpdateUnitHeal;
            guildSquareManager.OnMultiPlayerAttackMotion -= OnMultiPlayerAttackMotion;
            guildSquareManager.OnMultiPlayerDamage -= UpdateUnitDamage;
            guildSquareManager.OnMultiPlayerDie -= OnMultiPlayerDie;
            guildSquareManager.OnMultiPlayerAppear -= OnMultiPlayerAppear;
            guildSquareManager.OnMultiPlayerBuff -= OnMultiPlayerBuff;

            guildSquareManager.OnMultiMonsterStatus -= OnMultiMonsterStatus;
            guildSquareManager.OnMultiMonsterExit -= OnMultiMonsterExit;
            guildSquareManager.OnMultiMonsterDamage -= UpdateUnitDamage;
            guildSquareManager.OnMultiMonsterDie -= OnMultiMonsterDie;
            guildSquareManager.OnMultiMonsterCrowdControl -= OnMultiMonsterCrowdControl;
            guildSquareManager.OnMultiMonsterDotDamage -= UpdateUnitDamage;

            guildSquareManager.OnUpdateSquareState -= RefreshSquareState;
            guildSquareManager.OnUpdateEmperiumDamage -= UpdateEmperiumHp;

            guildModel.OnUseGuildAttackPotion -= ResetMp;

            player.OnUseSkill -= RequestSkillMotion;
            player.OnApplySkill -= RequestSkill;
            player.OnSpawnActor -= OnSpawnPlayer;
            player.OnDespawnActor -= OnDespawnPlayer;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiController = UI.Show<UIController>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiWarning = UI.Show<UIWarning>();
            uiBattleGuildAttackSkillList = UI.Show<UIBattleGuildAttackSkillList>();
            uiBattleEmperium = UI.Show<UIBattleEmperium>();
            uiBattlePotion = UI.Show<UIBattlePotion>();
            uiBattleRebirth = UI.Show<UIBattleRebirth>();
            uiShadowPlayerSettings = UI.Show<UIShadowPlayerSettings>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += GoToStage;
            uiMvpTimer.OnFinish += OnTimeOver;
            uiBattleGuildAttackSkillList.OnSelect += OnSelectSkill;
            uiBattlePotion.OnSelect += OnSelectPotion;
            uiBattleRebirth.OnConfirm += OnRebirth;

            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiController.SetActive(true); // 컨트롤러 제어
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattlePotion.SetMode(UIBattlePotion.MenuContent.MpPotion);
            uiBattleRebirth.Hide();
            uiWarning.Initialize(LocalizeKey._38101); // 몬스터 습격 경보!!
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit += GoToStage;
            uiMvpTimer.OnFinish -= OnTimeOver;
            uiBattleGuildAttackSkillList.OnSelect -= OnSelectSkill;
            uiBattlePotion.OnSelect -= OnSelectPotion;
            uiBattleRebirth.OnConfirm -= OnRebirth;

            UI.Close<UIMainTop>();
            UI.Close<UIController>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UICamSelect>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIWarning>();
            UI.Close<UIBattleGuildAttackSkillList>();
            UI.Close<UIBattleEmperium>();
            UI.Close<UIBattlePotion>();
            UI.Close<UIBattleRebirth>();
            UI.Close<UIShadowPlayerSettings>();

            uiMainTop = null;
            uiController = null;
            uiBattlePlayerStatus = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiCamSelect = null;
            uiMvpTimer = null;
            uiBattleGuildAttackSkillList = null;
            uiBattleEmperium = null;
            uiBattlePotion = null;
            uiBattleRebirth = null;
            uiWarning = null;
            uiShadowPlayerSettings = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            string mainText = LocalizeKey._38002.ToText(); // 길드 습격
            uiBattleInfo.Set(mainText);
        }

        /// <summary>
        /// 나가기
        /// </summary>
        protected override void ExitEntry()
        {
            impl.StartBattle(BattleMode.GuildLobby);
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            // 포기로 인한 나가기의 경우에는 Stage 로 입장하기 때문에 Exit 호출 필요
            if (isGiveUp)
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
                yield break;
            }

            IsSuccessExit = true;
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            playerBotCreateQueue.Ready();
            monsterBotCreateQueue.Ready();

            IsSuccessEnter = true; // 무조건 성공
            yield break;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            string sceneName = GetSceneName(); // 씬 이름
            string bgmName = BGM_NAME; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비

            playerBotCreateQueue.Enqueue(guildSquareManager.GetPlayers());
            monsterBotCreateQueue.Enqueue(guildSquareManager.GetMonsters());
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
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            // 남은 시간 세팅
            System.TimeSpan endTimeSpan = guildSquareManager.GuildAttackEndTime - ServerTime.Now;
            uiMvpTimer.Initialize((long)endTimeSpan.TotalMilliseconds);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();

            const byte READY_STATE = (byte)PlayerState.Ready;
            if (guildSquareManager.PlayerState == READY_STATE)
            {
                const int DESC_LOCAL_KEY = LocalizeKey._38203; // {TIME}초 후 자동으로 길드 습격에 참여합니다.
                const int BTN_LOCAL_KEY = LocalizeKey._38204; // 바로 참여
                int needZeny = BasisType.REBIRTH_ZENY_BY_JOB_LEVEL.GetInt(characterModel.JobLevel);
                uiBattleRebirth.Initialize(DESC_LOCAL_KEY, BTN_LOCAL_KEY);

                System.TimeSpan rebirthTimeSpan = guildSquareManager.RebirthTime - ServerTime.Now;
                uiBattleRebirth.Show(needZeny, (int)rebirthTimeSpan.TotalSeconds);

                // actor 가 없어서 AI.ReadyToBattle 이 일어나지 않음
                player.ReadyToBattle();
            }

            player.ResetSkillCooldown(); // 쿨타임 초기화

            uiBattlePlayerStatus.SetPlayer(player);
            uiBattleGuildAttackSkillList.SetCharacter(player);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            // (플레이어를 나중에 카메라세팅하기 위해 엠펠리움을 먼저 생성)
            yield return Timing.WaitUntilDone(YieldSpawnEmperium(), TAG); // 엠펠리움 Spawn
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG); // 플레이어 Spawn
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG); // 길드원 Spawn
            Timing.RunCoroutine(YieldSpawnMonsterBots(), TAG); // 몬스터 Spawn
            Timing.RunCoroutine(YieldSpawnTraps(), TAG); // 트랩 Spawn

            UnitActor playerActor = player.GetActor();
            if (playerActor)
            {
                playerActor.EffectPlayer.ShowUnitCircle();
            }

            RefreshSquareState();

            // Set EmperiumHp
            int emperiumLevel = guildSquareManager.EmperiumLevel;
            int emperiumHp = guildSquareManager.EmperiumHp;
            int emperiumMaxHp = guildSquareManager.EmperiumMaxHp;
            uiBattleEmperium.SetLevel(emperiumLevel);
            uiBattleEmperium.SetHp(emperiumHp, emperiumMaxHp);

            for (int i = 0; i < unitList.size; i++)
            {
                if (unitList[i] == null)
                    continue;

                UnitActor actor = unitList[i].GetActor();
                if (actor == null)
                    continue;

                actor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                actor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것
            }

            uiWarning.PlayWarning();
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        protected override void Dispose(UnitEntity entity)
        {
            base.Dispose(entity);

            entity.SetIgnoreTarget(false); // 무시 상태 되돌림
            entity.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            entity.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            entity.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
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

        void OnTimeOver()
        {

        }

        /// <summary>
        /// 스킬 선택 사용
        /// </summary>
        void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
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
            }
        }

        void OnSelectPotion(UIBattlePotion.MenuContent content)
        {
            switch (content)
            {
                case UIBattlePotion.MenuContent.MpPotion:
                    if (player.IsDie)
                        return;

                    if (player.IsMaxMp)
                        return;

                    const byte READY_STATE = (byte)PlayerState.Ready;
                    if (guildSquareManager.PlayerState == READY_STATE)
                        return;

                    guildModel.RequestUseGuildAttackPotion().WrapNetworkErrors();
                    break;
            }
        }

        void OnRebirth()
        {
            guildModel.RequestRebirthGuildAttack().WrapNetworkErrors();
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            if (!SpawnPlayer())
                yield break;

            yield return Timing.WaitForOneFrame;
        }

        /// <summary>
        /// 엠펠리움 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnEmperium()
        {
            int emperiumLevel = guildSquareManager.EmperiumLevel;
            int id = BasisType.GUILD_ATTACK_EMPERIUM_MONSTER_ID.GetInt(emperiumLevel);
            int level = emperiumLevel * 10;
            emperium = factory.CreateGuardian(id, level);

            if (!impl.Add(emperium, isEnemy: false))
                yield break;

            Vector3 pos = GuildSquareManager.EmperiumPosition;
            UnitActor unitActor = emperium.SpawnActor(); // 유닛 소환
            unitActor.AI.SetHomePosition(pos, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(pos); // Home 세팅

            emperium.SetExtraOption(ExtraBattleOptionType.MaxHp, guildSquareManager.EmperiumMaxHp);
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
        /// 몬스터 봇 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonsterBots()
        {
            while (monsterBotCreateQueue.HasQueue())
            {
                if (!SpawnMonsterBot(monsterBotCreateQueue.Dequeue()))
                    continue;

                yield return Timing.WaitForOneFrame;
            }

            monsterBotCreateQueue.Finish(); // 생성 완료
        }

        /// <summary>
        /// 트랩 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnTraps()
        {
            while (true)
            {
                BattleTrap trap = guildSquareManager.DequeueTrapTuple();
                if (trap == null)
                    yield break;

                if (!SpawnTrap(trap))
                    continue;

                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private bool SpawnPlayer()
        {
            PlayerState state = guildSquareManager.PlayerState.ToEnum<PlayerState>();
            if (state == PlayerState.Ready)
                return false;

            bool needSpawn = impl.Add(player, isEnemy: false);

            // ExtraOption - MaxHp 의 경우에는 ReadyToBattle 전에 설정
            player.SetExtraOption(ExtraBattleOptionType.MaxHp, guildSquareManager.PlayerMaxHp);
            player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
            player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

            UnitActor unitActor;
            if (needSpawn)
            {
                unitActor = player.SpawnActor(); // 유닛 소환

                unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MOVE_SPEED);
                unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);

                unitActor.AI.ReadyToBattle(); // 생성과 동시에 ReadyToBattle 호출
                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                unitActor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것

                unitActor.EffectPlayer.ShowShadow(); // 그림자 생성

                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
            }
            else
            {
                unitActor = player.GetActor();
            }

            Vector3 pos = guildSquareManager.PlayerPosition;
            Vector3 position = FindClosestEdge(pos);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

            UpdatePlayerState(player, state);
            return needSpawn;
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(PlayerBotEntity entity)
        {
            PlayerState state = entity.BotState.ToEnum<PlayerState>();
            if (state == PlayerState.Ready)
                return false;

            bool needSpawn = impl.Add(entity, isEnemy: false);

            // ExtraOption - MaxHp 의 경우에는 ReadyToBattle 전에 설정
            // MaxHp 가 존재하는 경우
            if (entity.BotMaxHp.HasValue)
            {
                entity.SetExtraOption(ExtraBattleOptionType.MaxHp, entity.BotMaxHp.Value);
            }
            else
            {
                entity.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            }

            entity.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
            entity.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

            UnitActor unitActor;
            if (needSpawn)
            {
                unitActor = entity.SpawnActor(); // 유닛 소환

                unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MOVE_SPEED);
                unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);

                unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출
                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                unitActor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것
            }
            else
            {
                unitActor = entity.GetActor();
            }

            Vector3 position = FindClosestEdge(entity.BotPosition);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

            // CurHp 가 존재하는 경우
            if (entity.BotCurHp.HasValue)
                entity.SetCurrentHp(entity.BotCurHp.Value);

            UpdatePlayerState(entity, state);
            return needSpawn;
        }

        /// <summary>
        /// 몬스터 봇 생성
        /// </summary>
        private bool SpawnMonsterBot(MonsterBotEntity entity)
        {
            // 생성 대기 중
            MonsterState state = entity.BotState.ToEnum<MonsterState>();
            if (state == MonsterState.None || state == MonsterState.StartWait)
                return false;

            UnitActor unitActor = entity.GetActor();
            bool needSpawn = impl.Add(entity, isEnemy: true) || (unitActor == null); // 몬스터의 경우 AutoDespawnDieState 가 존재하기 때문에

            // ExtraOption - MaxHp 의 경우에는 ReadyToBattle 전에 설정
            // MaxHp 가 존재하는 경우
            if (entity.BotMaxHp.HasValue)
            {
                entity.SetExtraOption(ExtraBattleOptionType.MaxHp, entity.BotMaxHp.Value);
            }
            else
            {
                entity.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            }

            if (needSpawn)
            {
                entity.OnDespawnActor += OnDespawnMonsterActor;

                unitActor = entity.SpawnActor(); // 유닛 소환

                unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MONSTER_MOVE_SPEED);
                unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);

                unitActor.AI.ReadyToBattle(); // OnAllReady 이후에 생성하므로 생성과 동시에 ReadyToBattle 호출
                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                unitActor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것

                unitActor.AI.ChangeAutoDespawnDieState(); // 죽었을 때 자동 사라짐
            }

            Vector3 pos = entity.BotPosition;

            // 타겟 지점이 존재할 경우
            if (state == MonsterState.MoveTo && entity.BotTargetPosition.HasValue)
            {
                Vector3 targetPos = entity.BotTargetPosition ?? Vector3.zero;
                float interval = Time.realtimeSinceStartup - entity.BotSavedTargetingTime; // 걸린 시간
                float speed = unitActor.Movement.GetSpeed();
                pos = Vector3.MoveTowards(pos, targetPos, interval * speed);
            }

            Vector3 position = FindClosestEdge(pos);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

            // CurHp 가 존재하는 경우
            if (entity.BotCurHp.HasValue)
                entity.SetCurrentHp(entity.BotCurHp.Value);

            UpdateMonsterState(entity, state);
            return needSpawn;
        }

        /// <summary>
        /// 트랩 생성
        /// </summary>
        private bool SpawnTrap(BattleTrap trap)
        {
            IBattleTrapInput input = trap.Input;
            TrapState state = input.State.ToEnum<TrapState>();

            if (state == TrapState.None)
            {
                // 정보를 초기화 하지 않고 고유 id trap으로 돌려쓰기
                //guildSquareManager.Recycle(trap);
                return false;
            }

            Vector3 position = guildSquareManager.GetCellPosition(input.IndexX, input.IndexZ);
            Vector3 pos = FindClosestEdge(position);
            trap.SetPosition(pos);

            switch (state)
            {
                case TrapState.Ready:
                    trap.ShowReady();
                    break;

                case TrapState.Appear:
                    trap.Appear();
                    break;
            }

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
        /// 플레이어 회복
        /// </summary>
        void OnPlayerHeal(int plusHp, int remainHp)
        {
            UpdateUnitHeal(player, plusHp, remainHp);
        }

        /// <summary>
        /// 플레이어 대미지
        /// </summary>
        void OnPlayerDamage(int damage, int remainHp)
        {
            UpdateUnitDamage(player, damage, remainHp);
        }

        /// <summary>
        /// 플레이어 죽음
        /// </summary>
        void OnPlayerDie(int damage)
        {
            UpdateUnitDie(player, damage);
            UpdatePlayerState(player, PlayerState.Die);

            const int TITLE_LOCAL_KEY = LocalizeKey._38200; // 캐릭터가 사망했습니다.
            const int DESC_LOCAL_KEY = LocalizeKey._38201; // {TIME}초 후 자동으로 부활합니다.
            const int BTN_LOCAL_KEY = LocalizeKey._38202; // 바로 부활
            int needZeny = BasisType.REBIRTH_ZENY_BY_JOB_LEVEL.GetInt(characterModel.JobLevel);
            uiBattleRebirth.Initialize(TITLE_LOCAL_KEY, DESC_LOCAL_KEY, BTN_LOCAL_KEY);
            uiBattleRebirth.Show(needZeny, rebirthCooldownTime);
        }

        /// <summary>
        /// 플레이어 등장
        /// </summary>
        void OnPlayerAppear()
        {
            SpawnPlayer();

            if (uiBattleRebirth)
            {
                uiBattleRebirth.Hide();
            }

            if (uiBattlePotion)
            {
                uiBattlePotion.ResetCooldownTime();
            }
        }

        /// <summary>
        /// 플레이어 연계스킬
        /// </summary>
        void OnPlayerChainAttack(MonsterBotEntity target, SkillInfo skill)
        {
            // 스킬이 없거나 이미 연계스킬인 경우
            if (skill == null || skill.IsChainedSkill())
                return;

            // 타겟이 없거나 이미 죽어있을 경우
            if (target == null || target.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            ElementType skillElementType = (skill.IsBasicActiveSkill && skill.ElementType == default) ? player.battleItemInfo.WeaponElementType : skill.ElementType;
            actor.UseChainAttackSkill(target, skillElementType, skill.SkillId); // 연계 스킬 사용
        }

        /// <summary>
        /// 플레이어 봇 입장
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
            entity.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            entity.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
            Dispose(entity);
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
        }

        /// <summary>
        /// 멀티플레이어 공격모션
        /// </summary>
        void OnMultiPlayerAttackMotion(PlayerBotEntity entity, MonsterBotEntity target, SkillInfo skill)
        {
            UnitActor actor = entity.GetActor();
            if (actor == null)
                return;

            UnitActor targetActor = target.GetActor();
            if (targetActor == null)
                return;

            actor.UseSkill(targetActor, skill, isChainableSkill: false, queueIdleMotion: true); // 스킬 사용
        }

        /// <summary>
        /// 멀티플레이어 죽음
        /// </summary>
        void OnMultiPlayerDie(PlayerBotEntity entity, int damage)
        {
            UpdateUnitDie(entity, damage);
            UpdatePlayerState(entity, PlayerState.Die);
        }

        /// <summary>
        /// 유닛 회복
        /// </summary>
        private void UpdateUnitHeal(UnitEntity entity, int plusHp, int remainHp)
        {
            // 회복
            if (IsShowDamageEffect(entity))
            {
                entity.RecoveryHp(plusHp, 1);
            }

            // 체력 재조정
            if (entity.CurHP != remainHp)
                entity.SetCurrentHp(remainHp);

            UnitActor actor = entity.GetActor();
            if (actor == null)
                return;

            actor.EffectPlayer.ShowHealEffect();
        }

        /// <summary>
        /// 유닛 대미지
        /// </summary>
        private void UpdateUnitDamage(UnitEntity entity, int damage, int remainHp)
        {
            UpdateUnitDamage(entity, null, damage, 1, false, false, ElementType.Neutral, damage, false, remainHp);
        }

        /// <summary>
        /// 유닛 대미지
        /// </summary>
        private void UpdateUnitDamage(UnitEntity entity, UnitEntity attacker, int dmgPerHit, int blowCount, bool isCriticalHit, bool isBasicActiveSkill, ElementType elementType, int damage, bool isKnockBack, int remainHp)
        {
            // 대미지
            if (IsShowDamageEffect(entity) || IsShowDamageEffect(attacker))
            {
                entity.ApplyDamage(attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage);
            }

            // 체력 재조정
            if (entity.CurHP != remainHp)
                entity.SetCurrentHp(remainHp);

            //// 넉백 처리
            //if (isKnockBack)
            //{
            //    UnitActor actor = entity.GetActor();
            //    if (actor != null && attacker != null)
            //    {
            //        Vector3 dir = (entity.LastPosition - attacker.LastPosition).normalized;
            //        actor.Movement.KnockBack(dir, Constants.Battle.RushKnockBackPower);
            //    }
            //}
        }

        /// <summary>
        /// 유닛 사망
        /// </summary>
        private void UpdateUnitDie(UnitEntity entity, int damage)
        {
            UpdateUnitDie(entity, null, damage, 1, false, false, ElementType.Neutral, damage);
        }

        /// <summary>
        /// 유닛 사망
        /// </summary>
        private void UpdateUnitDie(UnitEntity entity, UnitEntity attacker, int dmgPerHit, int blowCount, bool isCriticalHit, bool isBasicActiveSkill, ElementType elementType, int damage)
        {
            // 대미지
            if (IsShowDamageEffect(entity) || IsShowDamageEffect(attacker))
            {
                entity.ApplyDamage(attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage);
            }
        }

        /// <summary>
        /// 멀티플레이어 등장
        /// </summary>
        void OnMultiPlayerAppear(PlayerBotEntity entity)
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
        /// 버프 스킬 적용
        /// </summary>
        void OnMultiPlayerBuff(int[] targetCids, SkillInfo skill)
        {
            foreach (var targetCid in targetCids)
            {
                UnitEntity target = guildSquareManager.FindPlayer(targetCid);

                if (target == null)
                {
                    Debug.LogError($"방어코드! 존재하지 않는 플레이어가 버프 타겟팅 됨: {nameof(targetCid)} = {targetCid}");
                    continue;
                }

                if (target.IsDie)
                {
                    // 죽은 캐릭터
                    continue;
                }

                target.AddBattleBuff(skill); // 버프 적용
            }
        }

        /// <summary>
        /// 멀티몬스터 업데이트
        /// </summary>
        void OnMultiMonsterStatus(MonsterBotEntity entity)
        {
            SpawnMonsterBot(entity);
        }

        /// <summary>
        /// 몬스터 나감
        /// </summary>
        void OnMultiMonsterExit(MonsterBotEntity entity)
        {
            UpdateMonsterState(entity, MonsterState.StartWait);
        }

        /// <summary>
        /// 멀티몬스터 죽음
        /// </summary>
        void OnMultiMonsterDie(MonsterBotEntity entity, UnitEntity attacker, int dmgPerHit, int blowCount, bool isCriticalHit, bool isBasicActiveSkill, ElementType elementType, int damage)
        {
            UpdateUnitDie(entity, attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage);
            UpdateMonsterState(entity, MonsterState.Die);
        }

        /// <summary>
        /// 멀티몬스터 상태이상
        /// </summary>
        void OnMultiMonsterCrowdControl(MonsterBotEntity entity, CrowdControlType type)
        {
            entity.battleCrowdControlInfo.Apply(type);
        }

        void OnDespawnMonsterActor(UnitActor actor)
        {
            actor.AI.ChangeDieEmptyState(); // 죽었을 때 아무것도 하지 않음
            actor.Entity.OnDespawnActor -= OnDespawnMonsterActor;
        }

        /// <summary>
        /// 스테이지로 이동
        /// </summary>
        private void GoToStage()
        {
            isGiveUp = true;
            impl.StartBattle(BattleMode.Stage); // 스테이지로 이동
        }

        /// <summary>
        /// 길드 스퀘어 상태 새로고침
        /// </summary>
        private void RefreshSquareState()
        {
            // (길드습격 => 길드광장)
            if (guildSquareManager.State != GuildSquareState.Square)
                return;

            // 전투 결과가 없을 때
            if (guildSquareManager.BattleResult == GuildBattleResult.None)
                return;

            // AI 종료
            for (int i = 0; i < unitList.size; i++)
            {
                if (unitList[i] == null)
                    continue;

                UnitActor actor = unitList[i].GetActor();
                if (actor == null)
                    continue;

                actor.AI.EndAI();
            }

            // 결과 보여주기
            switch (guildSquareManager.BattleResult)
            {
                case GuildBattleResult.Fail:
                    UI.Show<UIGuildAttackFail>().Show(guildSquareManager.PreEmperiumLevel, guildSquareManager.EmperiumLevel, ExitEntry);
                    break;

                case GuildBattleResult.Succees:
                    UI.Show<UIGuildAttackClear>().Show(guildSquareManager.PreEmperiumLevel, guildSquareManager.EmperiumLevel, ExitEntry);
                    break;
            }

            if (uiBattleRebirth)
            {
                uiBattleRebirth.Hide();
            }
        }

        /// <summary>
        /// 엠펠리움 hp 변화
        /// </summary>
        private void UpdateEmperiumHp(int damage)
        {
            int remainHp = guildSquareManager.EmperiumHp;

            if (emperium != null)
            {
                emperium.ApplyDamage(null, damage, 1, false, false, ElementType.Neutral, damage);

                if (emperium.CurHP != remainHp)
                {
                    emperium.SetCurrentHp(remainHp); // 체력 재조정
                }
            }

            if (uiBattleEmperium != null)
            {
                uiBattleEmperium.TweenHp(remainHp, guildSquareManager.EmperiumMaxHp);
            }
        }

        private void UpdatePlayerState(CharacterEntity entity, PlayerState state)
        {
            UnitActor actor = entity.GetActor();

            switch (state)
            {
                case PlayerState.Idle:
                    if (actor == null)
                        return;

                    if (actor.AI.ChangeState(AI.Transition.Rebirth))
                    {
                        // Rebirth 상태로 변경 성공
                    }
                    else if (actor.AI.ChangeState(AI.Transition.Finished))
                    {
                        // Finished 상태로 변경 성공
                    }

                    actor.EffectPlayer.ShowRebirthEffect(); // 플레이어 부활 이펙트
                    break;

                case PlayerState.Ready:
                    if (actor)
                    {
                        actor.AI.EndAI();
                    }

                    Dispose(entity); // 폐기
                    break;

                case PlayerState.Die:
                    entity.Die(null); // 강제 사망처리

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Dead);
                    break;
            }
        }

        private void UpdateMonsterState(MonsterBotEntity entity, MonsterState state)
        {
            UnitActor actor = entity.GetActor();

            switch (state)
            {
                case MonsterState.Start:
                    entity.SetIgnoreTarget(true); // 무시 상태

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Finished); // Finished 상태로 변경
                    actor.EffectPlayer.PlayPanelBuffEffect(); // 몬스터 소환 이펙트
                    break;

                case MonsterState.Freeze:
                    entity.SetIgnoreTarget(false); // 무시 상태 off

                    // 방어코드
                    if (entity.IsDie)
                    {
                        Debug.LogError("방어코드 발동! 죽어있는 경우");
                        entity.SetCurrentHp(1);
                    }

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Finished);
                    actor.Movement.Stop();
                    break;

                case MonsterState.None:
                case MonsterState.StartWait:
                    if (actor)
                    {
                        actor.AI.EndAI();
                    }

                    Dispose(entity); // 폐기
                    break;

                case MonsterState.Die:
                    entity.Die(null); // 강제 사망처리

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Dead);
                    break;

                case MonsterState.MoveTo:
                    entity.SetIgnoreTarget(false); // 무시 상태 off

                    // 방어코드
                    if (entity.IsDie)
                    {
                        Debug.LogError("방어코드 발동! 죽어있는 경우");
                        entity.SetCurrentHp(1);
                    }

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Finished);

                    if (!entity.BotTargetPosition.HasValue)
                        return;

                    Vector3 prePos = entity.BotPosition;
                    Vector3 targetPos = entity.BotTargetPosition.Value;

                    const float CHECK_MIN = MONSTER_CHANGE_SPEED_MIN_DISTANCE * MONSTER_CHANGE_SPEED_MIN_DISTANCE;
                    const float CHECK_MAX = MONSTER_CHANGE_SPEED_MAX_DISTANCE * MONSTER_CHANGE_SPEED_MAX_DISTANCE;

                    Vector3 pos = actor.CachedTransform.position;
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
                        actor.Movement.ForceWarp(prePos); // 너무 멀리 떨어져 있으므로, 몬스터 강제 Warp
                    }

                    actor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MONSTER_MOVE_SPEED * rate);
                    actor.Movement.ForceSetDestination(targetPos, useRemainThreshold: false);
                    actor.Animator.PlayRun();
                    break;

                case MonsterState.AttackReady:
                    entity.SetIgnoreTarget(false); // 무시 상태 off

                    // 방어코드
                    if (entity.IsDie)
                    {
                        Debug.LogError("방어코드 발동! 죽어있는 경우");
                        entity.SetCurrentHp(1);
                    }

                    if (actor == null)
                        return;

                    actor.AI.SetTarget(emperium == null ? null : emperium.GetActor());
                    actor.AI.ChangeState(AI.Transition.Match);
                    break;

                case MonsterState.Attack:
                    entity.SetIgnoreTarget(false); // 무시 상태 off

                    // 방어코드
                    if (entity.IsDie)
                    {
                        Debug.LogError("방어코드 발동! 죽어있는 경우");
                        entity.SetCurrentHp(1);
                    }

                    if (actor == null)
                        return;

                    actor.AI.SetTarget(emperium == null ? null : emperium.GetActor());
                    actor.AI.ChangeState(AI.Transition.Match);
                    break;
            }
        }

        /// <summary>
        /// 스킬 모션 서버 요청
        /// </summary>
        private void RequestSkillMotion(UnitEntity target, SkillInfo skillInfo)
        {
            if (target is MonsterBotEntity monsterBotEntity)
            {
                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", player.Character.Cid); // 공격자 아이디
                sfs.PutInt("2", monsterBotEntity.BotServerIndex); // 피격자 아이디
                sfs.PutInt("3", skillInfo.SkillId); // 스킬 아이디
                Protocol.REQUEST_GA_ATTACK_MOT.SendAsync(sfs).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 스킬 서버 요청
        /// </summary>
        private void RequestSkill(UnitEntity[] targets, SkillInfo skillInfo)
        {
            switch (skillInfo.ActiveSkillType)
            {
                case ActiveSkill.Type.Attack:
                    {
                        foreach (var item in targets)
                        {
                            if (item is MonsterBotEntity monsterBotEntity)
                            {
                                intBuffer.Add(monsterBotEntity.BotServerIndex);
                            }
                        }

                        int[] targetMonsterIndexes = intBuffer.GetBuffer(isAutoRelease: true);
                        if (targetMonsterIndexes.Length == 0)
                            return;

                        var sfs = Protocol.NewInstance();
                        sfs.PutLong("2", skillInfo.SkillId);
                        sfs.PutIntArray("3", targetMonsterIndexes);
                        sfs.PutLong("4", skillInfo.SlotNo);

                        if (skillInfo.SlotNo > 0)
                        {
                            sfs.PutInt("5", 0); // 정상적인 스킬
                        }
                        else
                        {
                            sfs.PutInt("5", (int)skillInfo.RefBattleOption); // 참조되어 나간 스킬
                        }

                        Protocol.REQUEST_GA_ATTACK.SendAsync(sfs).WrapNetworkErrors();
                    }
                    break;

                case ActiveSkill.Type.RecoveryHp:
                case ActiveSkill.Type.Buff:
                    {
                        foreach (var item in targets)
                        {
                            if (item is CharacterEntity characterEntity)
                            {
                                intBuffer.Add(characterEntity.Character.Cid);
                            }
                        }

                        int[] targetCids = intBuffer.GetBuffer(isAutoRelease: true);
                        if (targetCids.Length == 0)
                            return;

                        var sfs = Protocol.NewInstance();
                        sfs.PutIntArray("3", targetCids);
                        sfs.PutLong("4", skillInfo.SlotNo);
                        Protocol.REQUEST_GA_ACTIVEBUFSKILL.SendAsync(sfs).WrapNetworkErrors();
                    }
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(ActiveSkill.Type)} = {skillInfo.ActiveSkillType}");
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        void OnSpawnPlayer(UnitActor actor)
        {
            actor.Movement.SetAvoidancePriority(Constants.Movement.PLAYER_AVOIDANCE_PRIORITY);
            actor.Movement.SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance);
        }

        /// <summary>
        /// 플레이어 소멸
        /// </summary>
        void OnDespawnPlayer(UnitActor actor)
        {
            actor.Movement.SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance);
        }

        /// <summary>
        /// Mp 채우기
        /// </summary>
        private void ResetMp()
        {
            player.SetCurrentMp(player.MaxMp);
        }

        /// <summary>
        /// 대미지 이펙트 보여주기 여부
        /// </summary>
        private bool IsShowDamageEffect(UnitEntity entity)
        {
            if (entity == null)
                return false;

            // 플레이어의 경우에만 대미지 이펙트 보여주기
            if (graphicSettings.ShadowMultiPlayerQualityLevel == ShadowMultiPlayerQuality.Shadow)
                return entity.type == UnitEntityType.Player;

            return true;
        }
    }
}