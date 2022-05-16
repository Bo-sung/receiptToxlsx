using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ForestMazeEntry : BattleEntry
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
        private readonly ForestMazeManager forestMazeManager;

        // <!-- Scene Compositions --!>
        private readonly PlayerBotCreateQueue playerBotCreateQueue; // 플레이어봇 생성 대기열
        private readonly MonsterBotCreateQueue monsterBotCreateQueue; // 몬스터봇 생성 대기열
        private readonly MazeObjectCreateQueue hpPotionCreateQueue; // 체력포션 생성 대기열
        private readonly MazeObjectCreateQueue emperiumCreateQueue; // 엠펠리움 생성 대기열

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
        private UIBattleForestMaze uiBattleForestMaze;

        // <!-- Temps --!>
        private bool isFloorMove;

        public ForestMazeEntry() : base(BattleMode.ForestMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            forestMazeManager = ForestMazeManager.Instance;

            playerBotCreateQueue = new PlayerBotCreateQueue();
            monsterBotCreateQueue = new MonsterBotCreateQueue();
            hpPotionCreateQueue = new MazeObjectCreateQueue();
            emperiumCreateQueue = new MazeObjectCreateQueue();

            forestMazeManager.Initialize();
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
            hpPotionCreateQueue.Clear(); // 체력포션 생성 대기열 삭제
            emperiumCreateQueue.Clear(); // 엠펠리움 생성 대기열 삭제
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnSpawnActor += OnPlayerSpawn;
            player.OnDespawnActor += OnPlayerDespawn;

            forestMazeManager.OnUpdateState += OnUpdateState;

            forestMazeManager.OnPlayerMove += OnPlayerMove;
            forestMazeManager.OnPlayerDie += OnPlayerDie;
            forestMazeManager.OnPlayerHp += OnPlayerHp;

            forestMazeManager.OnMultiPlayerJoin += OnMultiPlayerJoin;
            forestMazeManager.OnMultiPlayerExit += OnMultiPlayerExit;
            forestMazeManager.OnMultiPlayerMove += OnMultiPlayerMove;
            forestMazeManager.OnMultiPlayerWarp += OnMultiPlayerWarp;
            forestMazeManager.OnMultiPlayerState += OnMultiPlayerState;
            forestMazeManager.OnMultiPlayerHp += OnMultiPlayerHp;

            forestMazeManager.OnMultiMonsterStatus += OnMultiMonsterStatus;

            forestMazeManager.OnGainedHpPotion += OnGainedHpPotion;
            forestMazeManager.OnRegenHpPotion += OnRegenHpPotion;
            forestMazeManager.OnGainedEmperium += OnGainedEmperium;
            forestMazeManager.OnRegenEmperium += OnRegenEmperium;

            forestMazeManager.OnUpdateEmperium += RefreshEmperiumCount;
            forestMazeManager.OnUpdateFloor += OnUpdateFloor;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnSpawnActor -= OnPlayerSpawn;
            player.OnDespawnActor -= OnPlayerDespawn;

            forestMazeManager.OnUpdateState -= OnUpdateState;

            forestMazeManager.OnPlayerMove -= OnPlayerMove;
            forestMazeManager.OnPlayerDie -= OnPlayerDie;
            forestMazeManager.OnPlayerHp -= OnPlayerHp;

            forestMazeManager.OnMultiPlayerJoin -= OnMultiPlayerJoin;
            forestMazeManager.OnMultiPlayerExit -= OnMultiPlayerExit;
            forestMazeManager.OnMultiPlayerMove -= OnMultiPlayerMove;
            forestMazeManager.OnMultiPlayerWarp -= OnMultiPlayerWarp;
            forestMazeManager.OnMultiPlayerState -= OnMultiPlayerState;
            forestMazeManager.OnMultiPlayerHp -= OnMultiPlayerHp;

            forestMazeManager.OnMultiMonsterStatus -= OnMultiMonsterStatus;

            forestMazeManager.OnGainedHpPotion -= OnGainedHpPotion;
            forestMazeManager.OnRegenHpPotion -= OnRegenHpPotion;
            forestMazeManager.OnGainedEmperium -= OnGainedEmperium;
            forestMazeManager.OnRegenEmperium -= OnRegenEmperium;

            forestMazeManager.OnUpdateEmperium -= RefreshEmperiumCount;
            forestMazeManager.OnUpdateFloor -= OnUpdateFloor;
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
            uiBattleForestMaze = UI.Show<UIBattleForestMaze>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += GiveUp;
            uiResultClear.OnFinishDungeon += ExitEntry;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiEnemyFocus.Hide();
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
            uiBattleMenu.OnExit -= GiveUp;
            uiResultClear.OnFinishDungeon -= ExitEntry;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;

            UI.Close<UIController>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMainTop>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIEnemyFocus>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleForestMaze>();

            uiController = null;
            uiBattleMenu = null;
            uiResultClear = null;
            uiBattlePlayerStatus = null;
            uiMainTop = null;
            uiMvpTimer = null;
            uiBattleInfo = null;
            uiEnemyFocus = null;
            uiCamSelect = null;
            uiBattleForestMaze = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            uiBattleInfo.Set(forestMazeManager.CurrentData.name_id.ToText()); // 미궁숲
        }

        /// <summary>
        /// 뒤로가기
        /// </summary>
        public override void OnBack()
        {
            GiveUp();
        }

        public override void EndBattle()
        {
            base.EndBattle();

            if (uiController == null)
                return;

            uiController.Hide(); // 이동 입력 제한
        }

        /// <summary>
        /// 현재 전투 나가기
        /// </summary>
        protected override void ExitEntry()
        {
            forestMazeManager.Dispose();
            impl.StartBattle(BattleMode.MultiMazeLobby); // 멀티로비로 되돌아감
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            // 층 정보가 음슴
            if (forestMazeManager.Floor == 0)
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 이미 입장하지 않은 상태
            if (!forestMazeManager.IsJoined())
            {
                IsSuccessEnter = false;
                yield break;
            }

            IsSuccessEnter = true; // 무조건 성공
        }

        /// <summary>
        /// 특정 id로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            playerBotCreateQueue.Ready();
            monsterBotCreateQueue.Ready();
            hpPotionCreateQueue.Ready();
            emperiumCreateQueue.Ready();

            // 이미 입장한 상태
            if (forestMazeManager.IsJoined())
            {
                IsSuccessEnter = true;
                yield break;
            }

            Task<bool> task = forestMazeManager.Enter(id);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            IsSuccessEnter = task.Result;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            ForestBaseData data = forestMazeManager.CurrentData;
            string sceneName = data.scene_name; // 씬 이름
            string bgmName = data.bgm; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;
            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비

            playerBotCreateQueue.Enqueue(forestMazeManager.GetPlayers());
            monsterBotCreateQueue.Enqueue(forestMazeManager.GetMonsters());
            hpPotionCreateQueue.Enqueue(forestMazeManager.GetHpPotions());
            emperiumCreateQueue.Enqueue(forestMazeManager.GetEmperiums());
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
            System.TimeSpan endTimeSpan = forestMazeManager.EndTime - ServerTime.Now;
            float milliseconds = Mathf.Max(0f, (float)endTimeSpan.TotalMilliseconds);
            uiMvpTimer.Initialize((long)milliseconds);
            uiMvpTimer.RestartTimer();

            // 전투 UI 세팅
            uiBattleForestMaze.Initialize(forestMazeManager.CurrentData.group_id);
            uiBattleForestMaze.SetCount(forestMazeManager.EmperiumCount);

            // 엠펠리움 수 새로고침
            RefreshEmperiumCount();
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
            // ExtraOption - MaxHp 의 경우에는 ReadyToBattle 전에 설정
            player.SetExtraOption(ExtraBattleOptionType.MaxHp, forestMazeManager.PlayerMaxHp);
            player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
            player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);
            player.SetCurrentHp(forestMazeManager.PlayerHp); // MaxHp 가 더 커질 경우에는 값이 이상해 보일 수 있음

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

            Vector3 pos = forestMazeManager.PlayerPosition;
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
        /// 체력포션 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnHpPotion()
        {
            while (hpPotionCreateQueue.HasQueue())
            {
                if (!SpawnHpPotion(hpPotionCreateQueue.Dequeue()))
                    continue;

                yield return Timing.WaitForOneFrame;
            }

            hpPotionCreateQueue.Finish(); // 생성 완료
        }

        /// <summary>
        /// 엠펠리움 조각 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnEmperium()
        {
            while (emperiumCreateQueue.HasQueue())
            {
                if (!SpawnEmperium(emperiumCreateQueue.Dequeue()))
                    continue;

                yield return Timing.WaitForOneFrame;
            }

            emperiumCreateQueue.Finish(); // 생성 완료
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
                    // 보스의 경우
                    if (entity.BotServerIndex == forestMazeManager.BossMonsterIndex)
                    {
                        unitActor.EffectPlayer.ShowAura(UnitAuraType.MazeBossMonsterProtect); // 미로보스
                        RefreshBossAura(entity, isShowMessage: false);
                    }
                    else
                    {
                        unitActor.EffectPlayer.ShowAura(UnitAuraType.ForestMazeMiddleBossMonster); // 중간보스
                    }
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
        /// 체력포션 생성
        /// </summary>
        private bool SpawnHpPotion(MazeObjectEntity info)
        {
            if (info.State != MazeCubeState.General)
                return false;

            Vector3 pos = FindClosestEdge(info.Position);
            IMazeDropItem mazeDropItem = info.Spawn(pos);
            mazeDropItem.Set(info.ServerIndex, MazeRewardType.HpPotion, null);
            return true;
        }

        /// <summary>
        /// 엠펠리움 조각 생성
        /// </summary>
        private bool SpawnEmperium(MazeObjectEntity info)
        {
            if (info.State != MazeCubeState.General)
                return false;

            Vector3 pos = FindClosestEdge(info.Position);
            IMazeDropItem mazeDropItem = info.Spawn(pos);
            mazeDropItem.Set(info.ServerIndex, MazeRewardType.Emperium, null);
            return true;
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(PlayerBotEntity entity)
        {
            PlayerState state = entity.BotState.ToEnum<PlayerState>();
            if (state == PlayerState.Dead)
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

            isFloorMove = false;

            uiController.Show(); // 이동 입력 제한 해제

            player.SetCurrentHp(forestMazeManager.PlayerHp);

            foreach (var item in unitList)
            {
                UnitActor unitActor = item.GetActor();
                if (unitActor == null)
                    continue;

                unitActor.AI.StopHpRegen();
            }

            Timing.RunCoroutine(YieldSpawnHpPotion(), TAG);
            Timing.RunCoroutine(YieldSpawnEmperium(), TAG);
            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);
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
            unitActor.Movement.OnMazeDropItem += OnMazeDropItem;
            unitActor.Movement.OnPortal += OnPortal;
            unitActor.Radar.OnTarget += OnTarget;
        }

        /// <summary>
        /// 플레이어 제거 이벤트
        /// </summary>
        void OnPlayerDespawn(UnitActor unitActor)
        {
            unitActor.Movement.OnMazeDropItem -= OnMazeDropItem;
            unitActor.Movement.OnPortal -= OnPortal;
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
                case MazeRewardType.HpPotion:
                    SendGetHpPotion(mazeDropItem);
                    break;

                case MazeRewardType.Emperium:
                    SendGetEmperium(mazeDropItem);
                    break;
            }
        }

        void OnPortal(GameObject go)
        {
            // (방어코드: 씬 이동과 동시에 내려가는 현상 방지)
            if (!IsAllReady)
                return;

            if (go == null)
                return;

            ForestMazeFloorZone zone = go.GetComponent<ForestMazeFloorZone>();
            if (zone == null)
                return;

            //zone.Hit();
            ForestMazeFloorZone.FloorType type = zone.GetFloorType();
            switch (type)
            {
                case ForestMazeFloorZone.FloorType.Up:
                    isFloorMove = true;
                    Protocol.REQUEST_FOREST_UPSTORY.SendAsync().WrapNetworkErrors();
                    break;

                case ForestMazeFloorZone.FloorType.Down:
                    isFloorMove = true;
                    Protocol.REQUEST_FOREST_DOWNSTORY.SendAsync().WrapNetworkErrors();
                    break;
            }
        }

        void OnTarget(UnitActor unitActor)
        {
            if (unitActor == null)
                return;

            AI.StateID state = unitActor.AI.CurrentState;
            if (state == AI.StateID.Match)
            {
                if (unitActor.Entity is MonsterBotEntity monsterBotEntity)
                {
                    if (monsterBotEntity.MonsterType == MonsterType.Boss)
                    {
                        string message = LocalizeKey._90240.ToText(); // 보스 몬스터가 다른 유저와 전투 중입니다.
                        UI.ShowToastPopup(message);
                    }
                }
            }
        }

        void OnDespawnMonsterActor(UnitActor actor)
        {
            actor.AI.ChangeDieEmptyState(); // 죽었을 때 아무것도 하지 않음
            actor.Entity.OnDespawnActor -= OnDespawnMonsterActor;
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
            // 층 이동 중
            if (isFloorMove)
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

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            unitActor.Movement.Move(motion);

            Vector3 dist = player.LastPosition - forestMazeManager.PlayerPosition;
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

            if (!await forestMazeManager.Exit()) // 퇴장
                return;
        }

        void OnFinishedRemainTimer()
        {
            uiMvpTimer.StopTimer();
            uiMvpTimer.Hide();

            OnPlayerDie();
        }

        #endregion

        /// <summary>
        /// 현재 위치 서버로 보내기
        /// </summary>
        private void RequestPlayerCurrentPosition()
        {
            if (!IsAllReady)
                return;

            // 층 움직이고 있을 경우에는 서버로 보내면 안된다.
            if (isFloorMove)
                return;

            forestMazeManager.RequestPlayerMove(player.LastPosition);
        }

        /// <summary>
        /// 체력포션 획득
        /// </summary>
        private void SendGetHpPotion(IMazeDropItem mazeDropItem)
        {
            mazeDropItem.Hit();
            PlaySfx(Constants.SFX.Battle.HP_POTION);

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)mazeDropItem.PointId);
            Protocol.REQUEST_FOREST_GET_ITEM.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 엠펠리움 획득
        /// </summary>
        private void SendGetEmperium(IMazeDropItem mazeDropItem)
        {
            // 부족한 상태일 경우에만
            if (forestMazeManager.EmperiumCount < forestMazeManager.MaxEmperiumCount)
            {
                mazeDropItem.Hit();
                PlaySfx(Constants.SFX.Battle.EMPERIUM);

                var sfs = Protocol.NewInstance();
                sfs.PutByte("1", (byte)mazeDropItem.PointId);
                Protocol.REQUEST_FOREST_GET_EMPAL.SendAsync(sfs).WrapNetworkErrors();
            }
            else
            {
                ShowToastPopup(LocalizeKey._90291.ToText()); // 엠펠리움을 전부 획득했습니다.\n보스와 전투가 가능합니다.
            }
        }

        /// <summary>
        /// 보스 타겟팅 표시
        /// </summary>
        private void ShowBossTargetArrow(UnitEntity entity)
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ShowBossTargetingArrow(entity);
        }

        /// <summary>
        /// 보스 타겟팅 숨김
        /// </summary>
        private void HideBossTargetArrow()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ReleaseBossTargetingArrow();
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

                    int monsterIndex = forestMazeManager.BattleMonsterIndex;
                    MonsterBotEntity find = forestMazeManager.FindMonster(monsterIndex);
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

                    int cid = forestMazeManager.BattlePlayerCid;
                    CharacterEntity find = forestMazeManager.FindPlayer(cid);
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
                Debug.LogError($"대상 몬스터가 없다.");
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

            impl.StartBattle(BattleMode.ForestMazeBoss, bossEntity, true); // 멀티 보스 전투로 입장
        }

        /// <summary>
        /// 상태 변화
        /// </summary>
        void OnUpdateState()
        {
            switch (forestMazeManager.State)
            {
                case ForestMazeState.None:
                    UIResultClear.ResultType resultType = forestMazeManager.BattleResult == ForestMazeBattleResult.Succees ? UIResultClear.ResultType.Clear : UIResultClear.ResultType.Result;
                    uiResultClear.Show(forestMazeManager.TotalRewards, DungeonType.ForestMaze, false, resultType);
                    uiResultClear.SetDescription(string.Empty);
                    break;

                case ForestMazeState.MiddleBossBattle:
                case ForestMazeState.BossBattle:
                    int monsterIndex = forestMazeManager.BattleMonsterIndex;
                    MonsterBotEntity find = forestMazeManager.FindMonster(monsterIndex);
                    if (find == null)
                        return;

                    Timing.RunCoroutine(YieldEnterBossBattle(find), TAG); // 멀티 보스 전투로 입장
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

            actor.Movement.ForceWarp(forestMazeManager.PlayerPosition);
        }

        /// <summary>
        /// 플레이어 죽음
        /// </summary>
        void OnPlayerDie()
        {
            UpdatePlayerState(player, PlayerState.Dead);

            uiController.Hide();

            // 지연 퇴장(플레이어가 사망했을 때에 바로 결과 창이 나오지 않으므로 억지로 퇴장시킴)
            AsyncExit(isShowGiveUpMessage: false).WrapNetworkErrors();
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
        /// 체력 포션 획득
        /// </summary>
        void OnGainedHpPotion(HpPotionEntity entity, bool isPlayer)
        {
            IMazeDropItem mazeObjectItem = entity.GetMazeObjectItem();
            if (mazeObjectItem == null)
                return;

            if (isPlayer)
            {
                mazeObjectItem.StartEffect(); // 내가 먹은 효과
            }
            else
            {
                mazeObjectItem.Release(); // 상대방이 먹었을 때
            }
        }

        /// <summary>
        /// 체력 포션 리젠
        /// </summary>
        void OnRegenHpPotion(HpPotionEntity entity)
        {
            SpawnHpPotion(entity);
        }

        /// <summary>
        /// 엠펠리움 획득
        /// </summary>
        void OnGainedEmperium(EmperiumEntity entity, bool isPlayer)
        {
            IMazeDropItem mazeObjectItem = entity.GetMazeObjectItem();
            if (mazeObjectItem == null)
                return;

            if (isPlayer)
            {
                mazeObjectItem.StartEffect(); // 내가 먹은 효과
            }
            else
            {
                mazeObjectItem.Release(); // 상대방이 먹었을 때
            }
        }

        /// <summary>
        /// 엠펠리움 리젠
        /// </summary>
        void OnRegenEmperium(EmperiumEntity entity)
        {
            SpawnEmperium(entity);
        }

        /// <summary>
        /// 엠펠리움 수 업데이트
        /// </summary>
        private void RefreshEmperiumCount()
        {
            int emperiumCount = forestMazeManager.EmperiumCount;
            uiBattleForestMaze.SetCount(emperiumCount);

            int monsterIndex = forestMazeManager.BossMonsterIndex;
            MonsterBotEntity bossEntity = forestMazeManager.FindMonster(monsterIndex);
            RefreshBossAura(bossEntity, isShowMessage: true);
        }

        /// <summary>
        /// 보스 이펙트 새로고침
        /// </summary>
        private void RefreshBossAura(MonsterBotEntity bossEntity, bool isShowMessage)
        {
            if (bossEntity == null)
                return;

            // 방어코드 (진짜 보스인지 확인)
            if (bossEntity.BotServerIndex != forestMazeManager.BossMonsterIndex)
                return;

            // 아직 보스와 전투 불가능
            if (forestMazeManager.EmperiumCount < forestMazeManager.NeedBossBattleEmperiumCount)
            {
                HideBossTargetArrow(); // 타겟팅 지우기

                UnitActor actor = bossEntity.GetActor();
                if (actor != null)
                {
                    actor.EffectPlayer.ShowAura(UnitAuraType.ForestMazeBossMonsterShield); // 방어막 표시
                }
            }
            else
            {
                ShowBossTargetArrow(bossEntity); // 타겟팅 띄우기

                UnitActor actor = bossEntity.GetActor();
                if (actor != null)
                {
                    actor.EffectPlayer.ReleaseAura(UnitAuraType.ForestMazeBossMonsterShield); // 방어막 해제
                }

                if (isShowMessage)
                    ShowToastPopup(LocalizeKey._90128.ToText()); // 보스와 전투가 가능합니다
            }
        }

        /// <summary>
        /// 층 업데이트
        /// </summary>
        void OnUpdateFloor()
        {
            impl.StartBattle(BattleMode.ForestMaze);
        }
    }
}