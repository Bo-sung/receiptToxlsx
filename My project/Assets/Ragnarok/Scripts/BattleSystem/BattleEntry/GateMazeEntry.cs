using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GateMazeEntry : BattleEntry
    {
        public enum PlayerState : byte
        {
            General = 1, // 일반
            BattleBoss = 5, // 보스 전투 중
            Dead = 9, // 죽어있음
        }

        public enum MonsterState
        {
            General = 1, // 일반
            Patrol = 3, // 순찰
            MazeBattle = 5, // 전투중 (보스 몹 전투 중)
            Die = 9, // 죽고난 후 리스폰까지의 대기 상태
        }

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

        /// <summary>
        /// 기본 카메라 뷰
        /// </summary>
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;
        private const CameraController.View ZOOM_VIEW = CameraController.View.Quater40_10;
        private const float CAMERA_NOISE_FOCUS_ENEMY = 2f;

        // <!-- Managers --!>
        private readonly GateMazeManager gateMazeManager;

        // <!-- Scene Compositions --!>
        private readonly PlayerBotCreateQueue playerBotCreateQueue; // 플레이어봇 생성 대기열
        private readonly MonsterBotCreateQueue monsterBotCreateQueue; // 몬스터봇 생성 대기열
        private readonly BetterList<CharacterEntity> characterList;

        // <!-- UIs --!>
        private UIController uiController;
        private UIBattleMenu uiBattleMenu;
        private UIResultClear uiResultClear;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMainTop uiMainTop;
        private UIMvpTimer uiMvpTimer;
        private UIBattleInfo uiBattleInfo;
        private UIEnemyFocus uiEnemyFocus;
        private UICamSelect uiCamSelect;
        private UIBattleMazeAgentList uiBattleMazeAgentList;
        private UIBattleDarkMaze uiBattleDarkMaze;

        public GateMazeEntry() : base(BattleMode.GateMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            gateMazeManager = GateMazeManager.Instance;

            playerBotCreateQueue = new PlayerBotCreateQueue();
            monsterBotCreateQueue = new MonsterBotCreateQueue();
            characterList = new BetterList<CharacterEntity>();

            gateMazeManager.Initialize();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            player.ResetExtraOption(ExtraBattleOptionType.MaxHp); // 강제 Hp 제거
            player.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            player.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            playerBotCreateQueue.Clear(); // 플레이어봇 생성 대기열 삭제
            monsterBotCreateQueue.Clear(); // 몬스터봇 생성 대기열 삭제
            characterList.Clear();
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            gateMazeManager.OnUpdateState += OnUpdateState;

            gateMazeManager.OnPlayerMove += OnPlayerMove;
            gateMazeManager.OnPlayerDie += OnPlayerDie;
            gateMazeManager.OnPlayerHp += OnPlayerHp;

            gateMazeManager.OnMultiPlayerExit += OnMultiPlayerExit;
            gateMazeManager.OnMultiPlayerMove += OnMultiPlayerMove;
            gateMazeManager.OnMultiPlayerWarp += OnMultiPlayerWarp;
            gateMazeManager.OnMultiPlayerState += OnMultiPlayerState;
            gateMazeManager.OnMultiPlayerHp += OnMultiPlayerHp;

            gateMazeManager.OnMultiMonsterStatus += OnMultiMonsterStatus;

            gateMazeManager.OnMonsterKillCount += RefreshMonsterCount;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            gateMazeManager.OnUpdateState -= OnUpdateState;

            gateMazeManager.OnPlayerMove -= OnPlayerMove;
            gateMazeManager.OnPlayerDie -= OnPlayerDie;
            gateMazeManager.OnPlayerHp -= OnPlayerHp;

            gateMazeManager.OnMultiPlayerExit -= OnMultiPlayerExit;
            gateMazeManager.OnMultiPlayerMove -= OnMultiPlayerMove;
            gateMazeManager.OnMultiPlayerWarp -= OnMultiPlayerWarp;
            gateMazeManager.OnMultiPlayerState -= OnMultiPlayerState;
            gateMazeManager.OnMultiPlayerHp -= OnMultiPlayerHp;

            gateMazeManager.OnMultiMonsterStatus -= OnMultiMonsterStatus;

            gateMazeManager.OnMonsterKillCount -= RefreshMonsterCount;
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
            uiEnemyFocus = UI.Show<UIEnemyFocus>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiBattleMazeAgentList = UI.Show<UIBattleMazeAgentList>();
            uiBattleDarkMaze = UI.Show<UIBattleDarkMaze>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += GiveUp;
            uiResultClear.OnFinishDungeon += ExitEntry;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;
            uiBattleMazeAgentList.OnSelectAgent += OnSelectAgent;

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiEnemyFocus.Hide();
            uiResultClear.Hide();
            uiBattleDarkMaze.Hide();
            uiBattleMazeAgentList.Hide();

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
            uiBattleMenu.OnExit -= GiveUp;
            uiResultClear.OnFinishDungeon -= ExitEntry;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;
            uiBattleMazeAgentList.OnSelectAgent -= OnSelectAgent;

            UI.Close<UIController>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMainTop>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIEnemyFocus>();
            UI.Close<UICamSelect>();
            UI.Close<UIMatchMultiMazeStatus>();
            UI.Close<UIBattleDarkMaze>();
            UI.Close<UIBattleMazeAgentList>();

            uiController = null;
            uiBattleMenu = null;
            uiResultClear = null;
            uiBattlePlayerStatus = null;
            uiMainTop = null;
            uiMvpTimer = null;
            uiBattleInfo = null;
            uiEnemyFocus = null;
            uiCamSelect = null;
            uiBattleMazeAgentList = null;
            uiBattleDarkMaze = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            uiBattleInfo.Set(gateMazeManager.CurrentData.name_id.ToText());
        }

        public override void OnBack()
        {
            GiveUp();
        }

        protected override void ExitEntry()
        {
            gateMazeManager.Dispose();
            impl.StartBattle(BattleMode.MultiMazeLobby); // 멀티로비로 되돌아감
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            // 이미 입장하지 않은 상태
            if (!gateMazeManager.IsJoined())
            {
                IsSuccessEnter = false;
                yield break;
            }

            IsSuccessEnter = true; // 무조건 성공
        }

        /// <summary>
        /// 특정 id로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
            playerBotCreateQueue.Ready();
            monsterBotCreateQueue.Ready();

            // 이미 입장한 상태
            if (gateMazeManager.IsJoined())
            {
                IsSuccessEnter = true;
                yield break;
            }

            if (battleInput is GateMultiMazePacket packet)
            {
                IsSuccessEnter = gateMazeManager.Enter(packet);
            }
            else
            {
                IsSuccessEnter = false;
            }
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            GateData data = gateMazeManager.CurrentData;
            string sceneName = data.scene_name; // 씬 이름
            string bgmName = data.bgm; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비

            playerBotCreateQueue.Enqueue(gateMazeManager.GetPlayers());
            monsterBotCreateQueue.Enqueue(gateMazeManager.GetMonsters());
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

            uiBattlePlayerStatus.SetPlayer(player);

            // 남은 시간 세팅
            System.TimeSpan endTimeSpan = gateMazeManager.EndTime - ServerTime.Now;
            float milliseconds = Mathf.Max(0f, (float)endTimeSpan.TotalMilliseconds);
            uiMvpTimer.Initialize((long)milliseconds);
            uiMvpTimer.RestartTimer();

            uiBattleDarkMaze.Show();

            if (gateMazeManager.CurrentData.id == 1) // 게이트 1
            {
                uiBattleDarkMaze.Set(UIBattleDarkMaze.MonsterType.Gate1, gateMazeManager.MonsterKillCount, gateMazeManager.MonsterKillMaxCount);
            }
            else if (gateMazeManager.CurrentData.id == 2) // 게이트 2
            {
                uiBattleDarkMaze.Set(UIBattleDarkMaze.MonsterType.Gate2, gateMazeManager.MonsterKillCount, gateMazeManager.MonsterKillMaxCount);
            }
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG); // 플레이어 Spawn
            yield return Timing.WaitUntilDone(YieldSpawnMonsterBots(), TAG); // 몬스터 Spawn
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            characterList.Add(player);

            // ExtraOption - MaxHp 의 경우에는 ReadyToBattle 전에 설정
            player.SetExtraOption(ExtraBattleOptionType.MaxHp, gateMazeManager.PlayerMaxHp);
            player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
            player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);
            player.SetCurrentHp(gateMazeManager.PlayerHp); // MaxHp 가 더 커질 경우에는 값이 이상해 보일 수 있음

            UnitActor unitActor;
            if (impl.Add(player, isEnemy: false))
            {
                unitActor = player.SpawnActor(); // 유닛 소환

                unitActor.Movement.SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED);
                unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);

                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                unitActor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것

                unitActor.Radar.SetSenseDistance(2f);
                unitActor.Radar.SetSenseTargetType(TargetType.Enemy);

                unitActor.EffectPlayer.ShowUnitCircle(); // 그림자 생성

                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                unitActor = player.GetActor();
            }

            Vector3 pos = gateMazeManager.PlayerPosition;
            Vector3 position = FindClosestEdge(pos);
            unitActor.AI.SetHomePosition(position, isWarp: true); // Home 세팅
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
            RefreshMazeCharacters();
        }

        /// <summary>
        /// 몬스터 봇 생성
        /// </summary>
        private bool SpawnMonsterBot(MonsterBotEntity entity)
        {
            // 생성 대기 중
            MonsterState state = entity.BotState.ToEnum<MonsterState>();
            if (state == MonsterState.Die)
            {
                UpdateMonsterState(entity, state);
                return false;
            }

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

                if (entity.MoveSpeed.HasValue)
                {
                    unitActor.Movement.SetDefaultSpeed(entity.MoveSpeed.Value);
                }
                else
                {
                    unitActor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MONSTER_MOVE_SPEED);
                }

                unitActor.Movement.SetDistanceLimit(MIN_POSITION_SYNC_THRESHOLD);

                unitActor.AI.ReadyToBattle(); // OnAllReady 이후에 생성하므로 생성과 동시에 ReadyToBattle 호출
                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                unitActor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것
                unitActor.AI.ChangeAutoDespawnDieState(); // 죽었을 때 자동 사라짐

                // 보스 이펙트
                if (entity.MonsterType == MonsterType.Boss)
                {
                    unitActor.EffectPlayer.ShowAura(UnitAuraType.ForestMazeMiddleBossMonster); // 중간보스
                }
            }

            Vector3 pos = entity.BotPosition;

            // 타겟 지점이 존재할 경우
            if (state == MonsterState.Patrol && entity.BotTargetPosition.HasValue)
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
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(PlayerBotEntity entity)
        {
            characterList.Add(entity);

            PlayerState state = entity.BotState.ToEnum<PlayerState>();

            // 죽은 상태로 보여주기 위함
            //if (state == PlayerState.Dead)
            //    return false;

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

                unitActor.Movement.SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED);
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
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 씬 변경되면서 미로 상태가 아닐 경우에 대한 처리
            if (gateMazeManager.State != GateMazeState.Maze)
            {
                OnUpdateState();
                return;
            }

            player.SetCurrentHp(gateMazeManager.PlayerHp);

            foreach (var item in unitList)
            {
                UnitActor unitActor = item.GetActor();
                if (unitActor == null)
                    continue;

                unitActor.AI.StopHpRegen();
            }

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);

            // 플레이어 죽음 처리
            if (gateMazeManager.PlayerHp == 0)
                player.Die(null);
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        void OnDespawnMonsterActor(UnitActor actor)
        {
            actor.AI.ChangeDieEmptyState(); // 죽었을 때 아무것도 하지 않음
            actor.Entity.OnDespawnActor -= OnDespawnMonsterActor;
        }

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

            Vector3 dist = player.LastPosition - gateMazeManager.PlayerPosition;
            if (dist.sqrMagnitude < SEND_POSITION_SQR_MAGNITUDE)
                return;

            RequestPlayerCurrentPosition(); // 플레이어 위치 동기화
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

            RequestPlayerCurrentPosition(); // 플레이어 위치 동기화
        }

        private void GiveUp()
        {
            AsyncExit(isShowGiveUpMessage: true).WrapNetworkErrors();
        }

        private async Task AsyncExit(bool isShowGiveUpMessage)
        {
            if (isShowGiveUpMessage)
            {
                if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                    return;
            }
            else
            {
                await Task.Delay(System.TimeSpan.FromSeconds(1f)); // 1초 지연
            }

            if (!await gateMazeManager.Exit()) // 퇴장
                return;
        }

        void OnFinishedRemainTimer()
        {
            uiMvpTimer.StopTimer();
            uiMvpTimer.Hide();

            OnPlayerDie();
        }

        void OnSelectAgent(CharacterEntity entity)
        {
            if (entity.IsDie)
                return;

            if (!player.IsDie)
                return;

            UnitActor unitActor = entity.GetActor();
            if (unitActor == null)
                return;

            cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
        }

        #endregion

        /// <summary>
        /// 현재 위치 서버로 보내기
        /// </summary>
        private void RequestPlayerCurrentPosition()
        {
            if (!IsAllReady)
                return;

            gateMazeManager.RequestPlayerMove(player.LastPosition);
        }

        /// <summary>
        /// 플레이어 상태 변화
        /// </summary>
        private void UpdatePlayerState(CharacterEntity entity, PlayerState state)
        {
            UnitActor actor = entity.GetActor();
            switch (state)
            {
                case PlayerState.General:
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
                    break;

                case PlayerState.BattleBoss:
                    if (actor == null)
                        return;

                    int monsterIndex = gateMazeManager.BattleMonsterIndex;
                    MonsterBotEntity find = gateMazeManager.FindMonster(monsterIndex);
                    actor.AI.SetTarget(find == null ? null : find.GetActor());
                    actor.AI.ChangeState(AI.Transition.Match); // 전투매칭상태
                    break;

                case PlayerState.Dead:
                    entity.Die(null); // 강제 사망처리

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Dead); // 죽은상태
                    break;
            }
        }

        /// <summary>
        /// 몬스터 상태 변화
        /// </summary>
        private void UpdateMonsterState(MonsterBotEntity entity, MonsterState state)
        {
            UnitActor actor = entity.GetActor();

            switch (state)
            {
                case MonsterState.General:
                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Finished); // Finished 상태로 변경
                    actor.EffectPlayer.PlayPanelBuffEffect(); // 몬스터 소환 이펙트
                    break;

                case MonsterState.Patrol:
                    // 방어코드
                    if (entity.IsDie)
                    {
#if UNITY_EDITOR
                        Debug.LogError("방어코드 발동! 죽어있는 경우");
#endif
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

                    if (entity.MoveSpeed.HasValue)
                    {
                        actor.Movement.SetDefaultSpeed(entity.MoveSpeed.Value * rate);
                    }
                    else
                    {
                        actor.Movement.SetDefaultSpeed(Constants.Battle.LOBBY_MONSTER_MOVE_SPEED * rate);
                    }

                    actor.Movement.ForceSetDestination(targetPos, useRemainThreshold: false);
                    actor.Animator.PlayRun();
                    break;

                case MonsterState.MazeBattle:
                    if (actor == null)
                        return;

                    int cid = gateMazeManager.BattlePlayerCid;
                    CharacterEntity find = gateMazeManager.FindPlayer(cid);
                    actor.AI.SetTarget(find == null ? null : find.GetActor());
                    actor.AI.ChangeState(AI.Transition.Match); // 전투매칭상태
                    break;

                case MonsterState.Die:
                    entity.Die(null); // 강제 사망처리

                    if (actor == null)
                        return;

                    actor.AI.ChangeState(AI.Transition.Dead); // 죽은상태
                    break;
            }
        }

        /// <summary>
        /// 보스 전투
        /// </summary>
        private IEnumerator<float> YieldEnterBossBattle(MonsterBotEntity bossEntity)
        {
            OnResetController();
            uiController.Hide();
            cameraController.SetView(DEFAULT_VIEW);

            UnitActor bossActor = bossEntity.GetActor();
            if (bossActor == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"대상 몬스터가 없다.");
#endif
            }
            else
            {
                bossActor.Movement.Stop(); // 보스 몬스터 정지

                cameraController.SetPlayer(bossActor.CachedTransform);
                bossActor.AI.ChangeState(AI.Transition.EndBattle);
                bossActor.EffectPlayer.HideBattleHUD();
            }

            yield return Timing.WaitForOneFrame;

            uiEnemyFocus.Show(showNeedleFX: false, showSpreadFX: true);
            cameraController.SetView(ZOOM_VIEW);
            cameraController.SetCameraNoise(CAMERA_NOISE_FOCUS_ENEMY);
            CameraUtils.Zoom(CameraZoomType.ZoomIn);
            yield return Timing.WaitForSeconds(0.8f);

            impl.StartBattle(BattleMode.GateBoss, bossEntity, isStartFadeIn: true); // 중간 보스 전투로 입장
        }

        /// <summary>
        /// 상태 변화
        /// </summary>
        void OnUpdateState()
        {
            switch (gateMazeManager.State)
            {
                case GateMazeState.None:
                    UIResultClear.ResultType resultType = gateMazeManager.BattleResult == GateMazeBattleResult.Succees ? UIResultClear.ResultType.Clear : UIResultClear.ResultType.Result;
                    uiResultClear.Show(gateMazeManager.TotalRewards, DungeonType.Gate, false, resultType);
                    uiResultClear.SetDescription(string.Empty);
                    break;

                case GateMazeState.MiddleBossBattle:
                    int monsterIndex = gateMazeManager.BattleMonsterIndex;
                    MonsterBotEntity find = gateMazeManager.FindMonster(monsterIndex);
                    if (find == null)
                        return;

                    Timing.RunCoroutine(YieldEnterBossBattle(find), TAG); // 중간보스 전투
                    break;

                case GateMazeState.WorldBossBattle:
                    AsyncStartBattle(BattleMode.GateWorldBoss).WrapNetworkErrors(); // 월드보스 진행
                    break;
            }
        }

        /// <summary>
        /// 플레이어 움직임
        /// </summary>
        void OnPlayerMove()
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.Movement.ForceWarp(gateMazeManager.PlayerPosition);
        }

        /// <summary>
        /// 플레이어 죽음
        /// </summary>
        void OnPlayerDie()
        {
            UpdatePlayerState(player, PlayerState.Dead);

            uiController.Hide();
        }

        /// <summary>
        /// 플레이어 대미지
        /// </summary>
        void OnPlayerHp(int remainHp)
        {
            // 체력 재조정
            if (player.CurHP == remainHp)
                return;

            player.SetCurrentHp(remainHp);
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

            // Refresh 및 카메라 다시 세팅
            int index = FindIndex(entity);
            if (index != -1)
            {
                characterList.RemoveAt(index);
                RefreshMazeCharacters();

                // Focus 된 플레이어가 나갈 경우
                Transform focus = cameraController.GetPlayer();
                if (focus == actor.CachedTransform)
                {
                    UnitActor playerActor = player.GetActor();
                    cameraController.SetPlayer(playerActor.CachedTransform);
                }
            }

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
        /// 플레이어 봇 워프
        /// </summary>
        void OnMultiPlayerWarp(PlayerBotEntity entity)
        {
            UnitActor actor = entity.GetActor();
            if (actor == null)
                return;

            actor.Movement.ForceWarp(entity.BotPosition);
        }

        /// <summary>
        /// 플레이어 봇 상태변화
        /// </summary>
        void OnMultiPlayerState(PlayerBotEntity entity)
        {
            PlayerState state = entity.BotState.ToEnum<PlayerState>();
            UpdatePlayerState(entity, state);
        }

        /// <summary>
        /// 유닛 체력
        /// </summary>
        void OnMultiPlayerHp(PlayerBotEntity entity, int remainHp)
        {
            // 체력 재조정
            if (entity.CurHP == remainHp)
                return;

            entity.SetCurrentHp(remainHp);
        }

        /// <summary>
        /// 멀티몬스터 업데이트
        /// </summary>
        void OnMultiMonsterStatus(MonsterBotEntity entity)
        {
            SpawnMonsterBot(entity);
        }

        /// <summary>
        /// 엠펠리움 수 업데이트
        /// </summary>
        private void RefreshMonsterCount()
        {
            uiBattleDarkMaze.Tween(gateMazeManager.MonsterKillCount, gateMazeManager.MonsterKillMaxCount);
        }

        /// <summary>
        /// 캐릭터 인덱스 반환
        /// </summary>
        private int FindIndex(PlayerBotEntity entity)
        {
            for (int i = 0; i < characterList.size; i++)
            {
                if (characterList[i].Character.Cid == entity.Character.Cid)
                    return i;
            }

            return -1;
        }

        private void RefreshMazeCharacters()
        {
            if (uiBattleMazeAgentList == null)
                return;

            if (!uiBattleMazeAgentList.IsVisible)
                uiBattleMazeAgentList.Show();

            uiBattleMazeAgentList.SetShareCharacters(characterList.ToArray());
        }

        private async Task AsyncStartBattle(BattleMode mode)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(1f));
            impl.StartBattle(mode);
        }
    }
}