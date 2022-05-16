using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok
{
    public class ClickerDungeonEntry : BattleEntry
    {
        private const float PLAYER_CHASING_STATE_SWITCH_RATE = 0.5f; // 플레이어 추적 상태 전환 성공률 : 50%
        private const float GHOST_START_MOVE_DELAY = 2f; // 스폰 후 이동시작까지의 딜레이
        private const float GHOST_SWITCH_STATE_INTERVAL = 7f;
        private const float IGNORE_COLLISION_DURATION = 1f * 1000f; // 충돌 무시 지속시간
        private const float FREEZE_DURATION = 7f * 1000f; // 얼음모드 지속시간
        private const float FREEZE_TAP_DECREASE_TIME = 0.5f * 1000f; // 얼음모드 탭 감소 시간

        /******************** Repositories ********************/
        private readonly ClickerDungeonDataManager clickerDungeonDataRepo;

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIMazeHud uiMazeHud;
        private UIController uiController;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleInfo uiBattleInfo;
        private UIBattleBossHp uiBattleBossHp;
        private UIResultDungeon uiResultDungeon;
        private UIBattleMenu uiBattleMenu;
        private UIMvpTimer uiMvpTimer;
        private UICamSelect uiCamSelect;
        private UIBattleProgress uiBattleProgress;

        /******************** Temp Data ********************/
        private ClickerDungeonData currentData;
        private DungeonType dungeonType;
        private Map map; // 맵
        private UnitEntity monsterEntity; // 충돌중인 몬스터
        private DungeonModel dungeonModel;
        private Vector3 playerHome_Zeny = new Vector3(22f, UnitMovement.POSITION_Y, 22f); // 플레이어 생성 위치
        private Vector3 playerHome_Exp = new Vector3(31.5f, UnitMovement.POSITION_Y, 20.5f); // 플레이어 생성 위치
        private Vector3 playerHome;
        private readonly IBattlePool battlePool;
        private readonly List<PoolObject> poolObjectList;
        private long remainTime;

        private int cubeMaxCount; // 최대 큐브 수
        private int curCubeCount; // 남은 큐브 수

        private List<int> spawnMonsterCubeList; // 몬스터를 생성할 큐브들의 Index List
        private MonsterBotEntity playerChasingGhostEntity; // 플레이어를 쫓고있는 유령
        private RemainTime freezeRemainTime; // 얼음모드 남은 시간 
        private RemainTime ignoreCollisionRemainTime; // 충돌 무시 남은 시간
        private CoroutineHandle updateGhostAICoroutine;
        private readonly Buffer<int> clearCubeBuffer;

        private bool isBattleMode;
        private bool isEndBattle;
        private bool isClear;

        /******************** Scene Compositions ********************/
        private WayPointZone[] wayPointZones; // 웨이포인트 지점 목록
        private Dictionary<System.Tuple<int, int>, float> distanceMap; // 각 웨이포인트의 거리

        public ClickerDungeonEntry() : base(BattleMode.ClickerDungeon)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            clickerDungeonDataRepo = ClickerDungeonDataManager.Instance;
            dungeonModel = player.Dungeon;
            battlePool = BattlePoolManager.Instance;
            poolObjectList = new List<PoolObject>();
            spawnMonsterCubeList = new List<int>();
            distanceMap = new Dictionary<System.Tuple<int, int>, float>();
            clearCubeBuffer = new Buffer<int>();
        }

        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            monsterEntity = null;
            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();

            player.SetForceStatus(ForceStatusType.RefSkillOn); // 참조 스킬 On

            SetNextSingleAction(ShowDungeonUI);
        }

        private void ShowDungeonUI()
        {
            UIDungeon.ViewType viewType = UIDungeon.ViewType.None;
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    viewType = UIDungeon.ViewType.Zeny;
                    break;

                case DungeonType.ExpDungeon:
                    viewType = UIDungeon.ViewType.EXP;
                    break;
            }

            UIDungeon.viewType = viewType;
            UI.Show<UIDungeon>();
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnSpawnActor += OnCharacterSpawn;
            player.OnDespawnActor += OnCharacterDespawn;
            dungeonModel.OnClickerDungeonEnd += OnClickerDungeonEnd;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnSpawnActor -= OnCharacterSpawn;
            player.OnDespawnActor -= OnCharacterDespawn;
            dungeonModel.OnClickerDungeonEnd -= OnClickerDungeonEnd;
        }

        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiMazeHud = UI.Show<UIMazeHud>();
            uiController = UI.Show<UIController>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiCamSelect = UI.Show<UICamSelect>();

            UI.Show<UIGoodsView>();
            uiResultDungeon = UI.Show<UIResultDungeon>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBattleProgress = UI.Show<UIBattleProgress>();


            uiMazeHud.OnClicked += OnClickerMonsterClick;

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;

            uiBattleMenu.OnExit += OnBattleExit;

            uiResultDungeon.OnFinishDungeon += ExitDungeon;
            uiResultDungeon.OnRetryDungeon += RestartBattle;
            uiMvpTimer.OnFinish += OnTimeOver;

            uiMazeHud.Hide();

            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.NoName);

            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiMvpTimer.Hide();
            uiResultDungeon.Hide();

            cameraController.AddMask(Layer.MAZE_ENEMY);
            cameraController.SetView(CameraController.View.Quater40_5);
        }

        protected override void DisposeCanvas()
        {
            uiMazeHud.OnClicked -= OnClickerMonsterClick;

            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= OnBattleExit;

            uiResultDungeon.OnFinishDungeon -= ExitDungeon;
            uiResultDungeon.OnRetryDungeon -= RestartBattle;
            uiMvpTimer.OnFinish -= OnTimeOver;

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);

            UI.Close<UIMainTop>();
            UI.Close<UIMazeHud>();
            UI.Close<UIController>();
            UI.Close<UIGoodsView>();
            UI.Close<UIResultDungeon>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIMvpTimer>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleProgress>();

            uiMainTop = null;
            uiMazeHud = null;
            uiController = null;
            uiBattlePlayerStatus = null;
            uiBattleInfo = null;
            uiResultDungeon = null;
            uiMvpTimer = null;
            uiCamSelect = null;
            uiBattleProgress = null;
        }

        protected override void OnLocalize()
        {
            string mainText = currentData.name_id.ToText();
            uiBattleInfo.Set(mainText);
        }

        public override void OnBack()
        {
            if (uiResultDungeon.IsVisible)
                uiResultDungeon.Back();
            else
                OnBattleExit();
        }

        public override void EndBattle()
        {
            base.EndBattle();

            if (updateGhostAICoroutine.IsValid && updateGhostAICoroutine.IsRunning)
            {
                Timing.KillCoroutines(updateGhostAICoroutine);
            }

            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();
            distanceMap.Clear();
            monsterEntity = null;
            isEndBattle = false;
            clearCubeBuffer.Release();
        }

        /// <summary>
        /// 현재 던전 재도전
        /// </summary>
        private void RestartBattle()
        {
            impl.StartBattle(BattleMode.ClickerDungeon, currentData.Id);
        }

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            currentData = clickerDungeonDataRepo.Get(id);
            dungeonType = currentData.type.ToEnum<DungeonType>();

            if (currentData == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            Task<Response> task = dungeonModel.RequestClickerDungeonEnter(currentData.id);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
            {
                response.ShowResultCode();
                yield break;
            }

            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    Analytics.TrackEvent(TrackType.ZenyDungeon);
                    break;

                case DungeonType.ExpDungeon:
                    Analytics.TrackEvent(TrackType.ExpDungeon);
                    break;
            }

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, dungeonType.ToIntValue()); // 특정던전 입장 횟수
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = currentData.scene_name;
            string bgmName = currentData.bgm; // 배경음 이름
            bool isChangeScene = !SceneLoader.IsCheckCurrentScene(sceneName) || (map == null); // 씬 변경 여부 - 씬이 다르거나, 맵이 존재하지 않을 경우
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }

            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                return;
            }

            // 웨이포인트 처리
            wayPointZones = map.GetWayPointZones();

            // 웨이포인트 Distance Map 생성
            distanceMap.Clear();
            NavMeshPath path = new NavMeshPath();
            foreach (var wayPoint in wayPointZones)
            {
                foreach (var neighbor in wayPoint.NeighborWayPoints)
                {
                    // 중복 체크
                    if (distanceMap.ContainsKey(System.Tuple.Create(wayPoint.Id, neighbor.Id)))
                        continue;

                    // Path 거리 구하기
                    float? dist = GetPathDistance(wayPoint.CachedTransform.position, neighbor.CachedTransform.position, path);
                    if (!dist.HasValue) // 경로 찾기 실패.
                    {
                        Debug.LogError($"{wayPoint.Id} -> {neighbor.Id} Path Finding failed.");
                        continue;
                    }

                    distanceMap[System.Tuple.Create(wayPoint.Id, neighbor.Id)] = dist.Value;
                    distanceMap[System.Tuple.Create(neighbor.Id, wayPoint.Id)] = dist.Value;
                }
            }
        }

        protected override void OnReady()
        {
            base.OnReady();

            impl.ResetUnitList(); // 모든 유닛 초기화

            switch (currentData.type.ToEnum<DungeonType>())
            {
                case DungeonType.ExpDungeon:
                    playerHome = playerHome_Exp;
                    uiBattlePlayerStatus.SetExtraMode(UIExtraBattlePlayerStatus.ExtraMode.Exp);
                    break;
                case DungeonType.ZenyDungeon:
                    playerHome = playerHome_Zeny;
                    uiBattlePlayerStatus.SetExtraMode(UIExtraBattlePlayerStatus.ExtraMode.Zeny);
                    break;
                default:
                    playerHome = new Vector3(0f, UnitMovement.POSITION_Y, 0f);
                    break;
            }

            monsterEntity = null;
            playerChasingGhostEntity = null; // 플레이어 추적 유령 초기화

            uiBattleBossHp.Hide();

            uiBattlePlayerStatus.SetPlayer(player);

            // 남은 시간
            remainTime = currentData.limit_time;
            uiMvpTimer.Initialize(remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();
        }

        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
                yield break;

            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);

            // 몬스터 생성
            if (map != null)
            {
                yield return Timing.WaitUntilDone(YieldSpawnMazeMonster());

                // 포탈 체크
                map.SetPortalActive(true);
            }
        }

        protected override void OnAllReady()
        {
            base.OnAllReady();

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            switch (dungeonType)
            {
                case DungeonType.ExpDungeon:
                    uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Exp, 0, cubeMaxCount);
                    break;

                case DungeonType.ZenyDungeon:
                    uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Zeny, 0, cubeMaxCount);
                    break;
            }

            foreach (var unit in unitList)
            {
                if (unit.type == UnitEntityType.MazeMonster)
                {
                    unit.GetActor().EffectPlayer.HideName();
                }
            }

            player.GetActor().EffectPlayer.ShowUnitCircle();
        }      

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (attacker.type == UnitEntityType.Player)
            {
                if (unitEntity.type == UnitEntityType.MazeMonster)
                {
                    curCubeCount--;

                    PoolObject dieMark = battlePool.SpawnDieMark(unitEntity.LastPosition);
                    AddPoolObject(dieMark);

                    MazeMonsterEffectPlayer effectPlayer = unitEntity.GetActor().EffectPlayer as MazeMonsterEffectPlayer;
                    switch (dungeonType)
                    {
                        case DungeonType.ExpDungeon:
                            IMazeDropItem dropExp = effectPlayer.mazeExp;
                            dropExp.Set(0, MazeRewardType.None, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Exp));
                            effectPlayer.mazeExp.SetGravity(2f);
                            dropExp.StartEffect();
                            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Exp, cubeMaxCount - curCubeCount, cubeMaxCount);
                            break;

                        case DungeonType.ZenyDungeon:
                            IMazeDropItem dropZeny = effectPlayer.mazeZeny;
                            dropZeny.Set(0, MazeRewardType.None, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Zeny));
                            effectPlayer.mazeZeny.SetGravity(2f);
                            dropZeny.StartEffect();
                            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Zeny, cubeMaxCount - curCubeCount, cubeMaxCount);
                            break;
                    }

                    OnChangeActiveUIClicker(MazeBattleType.None);
                    monsterEntity = null;
                    EndEventBattle();

                    int cubeIndex = 1;
                    bool isMonsterSpawnCube = false;
                    if (unitEntity is MazeMonsterEntity mazeMonsterIndex)
                    {
                        cubeIndex = mazeMonsterIndex.CubeIndex;
                        if (spawnMonsterCubeList.Contains(mazeMonsterIndex.clientUID)) // 몬스터를 생성하는 큐브인가
                        {
                            isMonsterSpawnCube = true;
                        }
                    }

                    clearCubeBuffer.Add(cubeIndex);

                    if (isMonsterSpawnCube && curCubeCount != 0) // 몬스터 생성 큐브라면 몬스터를 스폰
                    {
                        Timing.RunCoroutine(SpawnGhost(unitEntity.LastPosition), TAG);
                    }

                    if (curCubeCount == 0)
                    {
                        ShowRewards(isClear: true); // 결과창 보여주기
                    }
                }
            }

            if (unitEntity.type == UnitEntityType.Player)
            {
                ShowRewards(isClear: false);
            }
        }

        /// <summary>
        /// 고스트 소환
        /// </summary>
        private IEnumerator<float> SpawnGhost(Vector3 pos)
        {
            MonsterBotEntity ghostEntity = MonsterEntity.Factory.CreateMonsterBot();
            ghostEntity.Initialize(currentData.normal_monster_id, 1);

            if (!impl.Add(ghostEntity, isEnemy: true))
                yield break;

            var actor = ghostEntity.SpawnActor();
            actor.Movement.Resume();

            Vector3 homePos = pos;
            actor.AI.SetHomePosition(homePos, isWarp: false);
            actor.Movement.ForceWarp(homePos);

            // 이벤트 등록
            ghostEntity.OnDespawnActor += OnDespawnGhost;
            actor.AI.OnTempCollisionActor += OnCollisionActorGhost;
            actor.Movement.OnWayPoint += OnGhostEnterWayPoint;
            actor.Movement.OnMoveStop += OnGhostMoveStop;

            actor.AI.ReadyToBattle();
            actor.EffectPlayer.PlayPanelBuffEffect(); // 쫄몹 소환 이펙트

            Timing.RunCoroutine(YieldGhostWaitForReady(actor), TAG);

            // Ghost 추적 AI 관리 시작
            if (!updateGhostAICoroutine.IsValid || !updateGhostAICoroutine.IsRunning)
                updateGhostAICoroutine = Timing.RunCoroutine(YieldUpdateGhostAI(), TAG);
        }

        /// <summary>
        /// 고스트 스폰 직후 잠시 깜빡이는 상태로 두었다가 복구
        /// </summary>
        private IEnumerator<float> YieldGhostWaitForReady(UnitActor actor)
        {
            actor.AI.ChangeState(AI.Transition.Groggy); // 깜빡임 상태로 전환

            yield return Timing.WaitForSeconds(GHOST_START_MOVE_DELAY);

            actor.AI.ChangeState(AI.Transition.Finished); // 배회 상태로 복구

            yield return Timing.WaitForOneFrame;

            // 가장 가까운 spawnZone으로 이동
            MonsterBotEntity ghost = actor.Entity as MonsterBotEntity;
            var nearestZone = GetNearestWayPoint(ghost.LastPosition);
            SetMonsterBotDestination(ghost, nearestZone);
        }

        /// <summary>
        /// 고스트(생성된 몬스터) 디스폰 이벤트
        /// </summary>
        void OnDespawnGhost(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnDespawnGhost;
            actor.AI.OnTempCollisionActor -= OnCollisionActorGhost;
            actor.Movement.OnWayPoint -= OnGhostEnterWayPoint;
            actor.Movement.OnMoveStop -= OnGhostMoveStop;
        }

        /// <summary>
        /// 유령 추적 이동 완료 이벤트
        /// </summary>
        /// <param name="monsterBotEntity"></param>
        void OnGhostMoveStop(UnitEntity entity)
        {
            MonsterBotEntity monsterBotEntity = entity as MonsterBotEntity;

            if (monsterBotEntity.IsDie)
                return;

            if (playerChasingGhostEntity is null || playerChasingGhostEntity != monsterBotEntity)
                return;

            // 플레이어가 근처에 있으면 직접 추격
            WayPointZone playerNearestZone = GetNearestWayPoint(player.LastPosition);
            WayPointZone nearestZone = GetNearestWayPoint(monsterBotEntity.LastPosition);
            if (playerNearestZone == nearestZone)
            {
                monsterBotEntity.GetActor().Movement.ForceSetDestination(player.LastPosition);
                return;
            }

            // 아니면 플레이어 근처로 이동
            SetMonsterBotDestination(monsterBotEntity, playerNearestZone); // 아니라면 플레이어 근처의 WayPoint를 향해 이동
        }

        /// <summary>
        /// 고스트(생성된 몬스터)와 충돌
        /// </summary>
        void OnCollisionActorGhost(UnitActor playerActor, UnitEntity ghostEntity)
        {
            // 중복 충돌 처리
            if (ignoreCollisionRemainTime.ToRemainTime() > 0)
                return;

            // 충돌 무시 시간 부여
            ignoreCollisionRemainTime = IGNORE_COLLISION_DURATION;


            if (currentData.SpawnMonsterType == ClickerDungeonMonsterType.DIE)
            {
                MonsterBotEntity monsterBotEntity = ghostEntity as MonsterBotEntity;
                player.Die(ghostEntity);
            }
            else if (currentData.SpawnMonsterType == ClickerDungeonMonsterType.FREEZE)
            {
                // 얼음모드 시작
                Timing.RunCoroutine(YieldGroggy(), TAG);
            }

            // 유령 사망 처리
            Timing.RunCoroutine(YieldKillGhost(ghostEntity), TAG);
        }

        private IEnumerator<float> YieldKillGhost(UnitEntity ghostEntity)
        {
            ghostEntity.Die(player);
            UnitActor actor = ghostEntity.GetActor();
            if (actor)
            {
                actor.AI.ChangeState(AI.Transition.Dead);
                //actor.AI.EndAI(); // AI 종료
            }

            yield return Timing.WaitForSeconds(2f);

            if (actor)
                actor.AI.ChangeState(AI.Transition.Finished);

            Dispose(ghostEntity);
        }

        /// <summary>
        /// 유령 이동 완료 이벤트
        /// </summary>
        void OnGhostEnterWayPoint(WayPointZone wayPointZone, UnitEntity entity)
        {
            MonsterBotEntity monsterBotEntity = entity as MonsterBotEntity;

            if (monsterBotEntity.IsDie)
                return;

            if (monsterBotEntity.DestWayPoint != wayPointZone) // 다른 웨이포인트는 무시하고 내 목표 웨이포인트와의 충돌만 다룬다.
                return;

            // 추적 유령의 경우
            if (ReferenceEquals(playerChasingGhostEntity, monsterBotEntity))
            {
                WayPointZone playerNearestZone = GetNearestWayPoint(player.LastPosition);
                if (playerNearestZone == wayPointZone) // 플레이어와 가장 가까운 웨이포인트가 현재 웨이포인트와 같다면 직접 추적
                {
                    monsterBotEntity.GetActor().Movement.ForceSetDestination(player.LastPosition);
                    return;
                }

                if (playerNearestZone != null) // 예외처리 (플레이어의 주변 웨이포인트가 없음)
                {
                    SetMonsterBotDestination(monsterBotEntity, playerNearestZone); // 아니라면 플레이어 근처의 WayPoint를 향해 이동
                    return;
                }
            }

            // 이동 가능한 선택지 체크
            List<WayPointZone> neighbors = new List<WayPointZone>(wayPointZone.NeighborWayPoints);
            neighbors.Remove(monsterBotEntity.LastWayPoint); // 방금 지나온 곳은 선택지에서 제외

            // 다음 웨이포인트 결정
            WayPointZone nextWayPoint = (neighbors.Count > 0) ?
                neighbors.GetRandomPick() : // 남은 선택지 중에서 랜덤으로 결정
                monsterBotEntity.LastWayPoint; // 갈 곳이 없다면 되돌아감

            SetMonsterBotDestination(monsterBotEntity, nextWayPoint);
        }

        /// <summary>
        /// 유령 AI 업데이트
        /// </summary>
        private IEnumerator<float> YieldUpdateGhostAI()
        {
            playerChasingGhostEntity = null; // 플레이어 추적 유령 초기화

            while (true)
            {
                // 추적중인 유령이 있다면 50% 확률로 추적 중지
                if (playerChasingGhostEntity != null)
                {
                    bool isWinning = (Random.value <= PLAYER_CHASING_STATE_SWITCH_RATE || playerChasingGhostEntity.IsDie); // 50%
                    if (isWinning)
                    {
                        if (!playerChasingGhostEntity.IsDie)
                        {
                            // 근처 웨이포인트로 가기
                            var neareastPoint = GetNearestWayPoint(playerChasingGhostEntity.LastPosition);
                            SetMonsterBotDestination(playerChasingGhostEntity, neareastPoint);
                        }
                        playerChasingGhostEntity = null;
                    }
                }

                // 추적중인 유령이 없다면 새로 뽑기
                if (playerChasingGhostEntity is null)
                {
                    playerChasingGhostEntity = PickNewChasingGhost();

                    // 추적 시작
                    if (playerChasingGhostEntity != null)
                    {
                        WayPointZone playerNearestZone = GetNearestWayPoint(player.LastPosition);
                        SetMonsterBotDestination(playerChasingGhostEntity, playerNearestZone);
                    }
                }

                // 상태 : 배회, 추적
                // 배회 : 7초마다 50% 확률로 플레이어 추적 시도 (동시에 여러 유령이 추적하려하면 가장 가까운 유령만 추적)
                // 추적 : 7초마다 50% 확률로 배회로 되돌아감

                // 배회 알고리즘
                // Begin : 현재 위치와 가장 가까운 WayPoint로 이동
                // Update : 인접한 WayPoint들 중 랜덤으로 이동 (최근 WayPoint는 선택하지 않음), 도착하면 LastWayPoint 갱신하고 다시 Update

                // 추적 알고리즘
                // Update : 플레이어와 가장 가까운 WayPoint로 이동하는게 주 목표. 
                // 바로 SetDestination하지 않고 Distance Map으로 A* 알고리즘 적용해서 각 WayPoint를 밟으면서 이동.
                // Path 갱신 주기는 n초

                yield return Timing.WaitForSeconds(GHOST_SWITCH_STATE_INTERVAL);
            }
        }

        /// <summary>
        /// 플레이어를 추적할 유령 뽑기
        /// </summary>
        private MonsterBotEntity PickNewChasingGhost()
        {
            var ghostEntities = unitList.FindAll(e => e is MonsterBotEntity monEntity && monEntity.MonsterType == MonsterType.Normal && !e.IsDie && e.GetActor().AI.CurrentState != AI.StateID.Defenseless).ConvertAll(e => e as MonsterBotEntity);

            var playerChaseCandidates = new List<MonsterBotEntity>(); // 플레이어 추적 후보
            foreach (var ghost in ghostEntities.OrEmptyIfNull())
            {
                bool isWinning = (Random.value < PLAYER_CHASING_STATE_SWITCH_RATE); // 50%
                if (isWinning)
                {
                    playerChaseCandidates.Add(ghost);
                }
            }

            // 후보가 여러명이라면 가장 가까운 후보가 추적하도록 함
            if (playerChaseCandidates.Count > 1)
            {
                float minDist = float.MaxValue;
                MonsterBotEntity minDistEntity = null;
                foreach (var candidate in playerChaseCandidates)
                {
                    float? dist = GetPathDistance(candidate.GetActor().CachedTransform.position, player.LastPosition);
                    if (!dist.HasValue)
                        continue;

                    if (dist.Value < minDist)
                    {
                        minDist = dist.Value;
                        minDistEntity = candidate;
                    }
                }
                return minDistEntity;
            }

            if (playerChaseCandidates.Count == 1)// 후보가 하나라면 
                return playerChaseCandidates[0];

            return null; // 후보 없음
        }

        /// <summary>
        /// 가장 가까운 WayPoint찾기
        /// </summary>
        private WayPointZone GetNearestWayPoint(Vector3 pos)
        {
            const float minDistThreshold = 1f;

            float minDist = float.MaxValue;
            WayPointZone minDistZone = null;
            NavMeshPath path = new NavMeshPath();
            foreach (var wayPoint in wayPointZones)
            {
                float? dist = GetPathDistance(pos, wayPoint.CachedTransform.position, path);
                if (!dist.HasValue)
                {
#if UNITY_EDITOR
                    Debug.LogError($"[GetNearestWayPoint] {pos} -> {wayPoint.Id} Path Finding Fail.");
#endif
                    continue;
                }

                if (dist.Value < minDist && dist.Value > minDistThreshold)
                {
                    minDist = dist.Value;
                    minDistZone = wayPoint;
                }
            }

            return minDistZone;
        }

        /// <summary>
        /// Path 거리 구하기
        /// </summary>
        private float? GetPathDistance(Vector3 A, Vector3 B, NavMeshPath path = null)
        {
            if (path is null)
                path = new NavMeshPath();

            path.ClearCorners();
            bool isSuccess = NavMesh.CalculatePath(A, B, NavMesh.AllAreas, path);
            if (!isSuccess || path.status == NavMeshPathStatus.PathInvalid || path.corners.Length == 0)
                return null;

            float dist = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                dist += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return dist;
        }

        /// <summary>
        /// 몬스터 지정 WayPoint로 이동 명령
        /// </summary>
        private void SetMonsterBotDestination(MonsterBotEntity entity, WayPointZone destZone)
        {
            if (entity.IsDie)
                return;

            var actor = entity.GetActor();
            actor.Movement.ForceSetDestination(destZone.CachedTransform.position, useRemainThreshold: true);
            entity.LastWayPoint = (entity.DestWayPoint != default) ? entity.DestWayPoint : destZone;
            entity.DestWayPoint = destZone;
        }

        /// <summary>
        /// 얼음모드 시작
        /// </summary>
        private IEnumerator<float> YieldGroggy()
        {
            UnitActor actor = player.GetActor();
            if (actor is null)
                yield break;

            // 상태이상 걸기
            actor.AI.ChangeState(AI.Transition.Groggy);
            actor.EffectPlayer.ShowRandomGroggy(isPlaySfx: true);

            // 화면을 터치하여 행동 불능에서 빠져나오세요!
            uiBattleProgress.StartTimer(FREEZE_DURATION, UIBattleProgress.IconType.Touch, LocalizeKey._3501.ToText());

            // 타이머 대기
            freezeRemainTime = FREEZE_DURATION;
            while (true)
            {
                if (freezeRemainTime.ToRemainTime() <= 0f)
                    break;

                yield return Timing.WaitForOneFrame;
            }

            // 상태이상 해제
            player.GetActor()?.AI.ChangeState(AI.Transition.Finished);
            actor.EffectPlayer.HideGroggy();
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

        async void OnBattleExit()
        {
            if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                return;

            ShowRewards(isClear: false);
        }

        void OnStartController()
        {
            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            // 얼음 상태이면
            if (actor.AI.CurrentState == AI.StateID.Defenseless)
            {
                // 카메라 흔들림
                actor.EffectPlayer.GenerateImpulse(BattlePoolManager.ImpulseType.ClickAttack);

                // 타이머 감소
                uiBattleProgress.DecreaseRemainTime(FREEZE_TAP_DECREASE_TIME);
                freezeRemainTime -= FREEZE_TAP_DECREASE_TIME;
                return;
            }

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

            // 얼음 상태이면
            if (actor.AI.CurrentState == AI.StateID.Defenseless)
                return;

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
        /// 클리커 몬스터 클릭
        /// </summary>
        void OnClickerMonsterClick()
        {
            monsterEntity.Apply(player, player.battleSkillInfo.basicActiveSkill, null);
        }

        /// <summary>
        /// 클리커 몬스터에 따른 UI 처리
        /// </summary>
        void OnChangeActiveUIClicker(MazeBattleType battleType)
        {
            switch (battleType)
            {
                case MazeBattleType.None:
                    {
                        uiController.Show();
                        uiMazeHud.Hide();
                        uiBattleBossHp.Hide();

                        cameraController.SetClearshot(CameraController.Clearshot.None);
                    }
                    break;

                case MazeBattleType.Clicker:
                    if (monsterEntity)
                    {
                        StartEventBattle();
                        uiController.Hide();
                        uiMazeHud.ShowHud(monsterEntity.GetActor().gameObject);
                        uiBattleBossHp.SetMode(UIBattleBossHp.Mode.NoName);
                        uiBattleBossHp.Show(null, monsterEntity.GetName(), monsterEntity.battleUnitInfo.UnitElementType, UIBattleBossHp.MonsterType.Normal, 0, monsterEntity.GetActor().gameObject);
                        uiBattleBossHp.SetHp(monsterEntity.CurHP, monsterEntity.MaxHP);
                    }
                    break;
            }
        }

        /// <summary>
        /// 내 캐릭터 생성 이벤트
        /// </summary>
        /// <param name="actor"></param>
        void OnCharacterSpawn(UnitActor actor)
        {
            actor.AI.OnCollisionActor += OnCollisionActor;
        }

        /// <summary>
        /// 내 캐릭터 제거 이벤트
        /// </summary>
        /// <param name="actor"></param>
        void OnCharacterDespawn(UnitActor actor)
        {
            actor.AI.OnCollisionActor -= OnCollisionActor;
        }

        /// <summary>
        /// 미로맵 몬스터와 충돌
        /// </summary>
        void OnCollisionActor(UnitActor actor)
        {
            if (!IsAllReady || isEndBattle)
                return;

            if (actor == null)
                return;

            // 전투 중인 상태
            if (isBattleMode)
                return;

            if (actor.Entity.type == UnitEntityType.MazeMonster)
            {
                // 선공 몬스터
                monsterEntity = actor.Entity;
                StartBossStageAsync();
            }
        }

        private void StartEventBattle()
        {
            SetBattleMode(true);
        }

        private void EndEventBattle()
        {
            monsterEntity = null;

            SetBattleMode(false);

            uiController.Show();
            uiMazeHud.Hide();
            uiBattleBossHp.Hide();
            cameraController.SetClearshot(CameraController.Clearshot.None);
        }

        private void SetBattleMode(bool isBattleMode)
        {
            this.isBattleMode = isBattleMode;
        }

        /// <summary>
        /// 보스 전투 시작
        /// </summary>
        private void StartBossStageAsync()
        {
            OnResetController();
            OnChangeActiveUIClicker(MazeBattleType.Clicker);
        }

        // 시간 초과 시
        void OnTimeOver()
        {
            ShowRewards(isClear: false);
        }

        /// <summary>
        /// 리워드 출력
        /// </summary>
        private void ShowRewards(bool isClear)
        {
            if (isEndBattle)
                return;

            isEndBattle = true;
            this.isClear = isClear;

            EndEventBattle();
            OnChangeActiveUIClicker(MazeBattleType.None); // 강제로 
            OnResetController();
            uiMvpTimer.StopTimer();
            uiBattleProgress.StopTimer();

            dungeonModel.RequestClickerDungeonEnd(clearCubeBuffer.GetBuffer(isAutoRelease: true)).WrapNetworkErrors();
        }

        private void OnClickerDungeonEnd(RewardData[] rewards)
        {
            // 퀘스트 체크 & 클리어 ID 갱신
            if (isClear)
                dungeonModel.SetClearedDungeonId(dungeonType, currentData.id);

            UI.Close<UIChat>();
            // ResultClear UI 보여주기
            string message = rewards.Length == 0
                ? LocalizeKey._90137.ToText() // 획득한 보상이 없습니다.
                : LocalizeKey._90138.ToText(); // 던전을 탐험하여 보상을 획득하였습니다!
            uiResultDungeon.Show(rewards, dungeonType, true, isClear ? UIResultDungeon.TitleType.Clear : UIResultDungeon.TitleType.Failed, message);
        }

        private void ExitDungeon()
        {
            impl.ExitBattle();
        }

        /// <summary>
        /// 클리커 몬스터 HP 변동
        /// </summary>
        void OnChangedClickerMonaterHp(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            if (impl.Add(player, isEnemy: false))
            {
                player.SetForceStatus(ForceStatusType.RefSkillOff); // 참조 스킬 Off

                UnitActor unitActor = player.SpawnActor(); // 유닛 소환
                unitActor.AI.SetHomePosition(playerHome, isWarp: true);
                unitActor.AI.ChangeRandomDefenselessState(); // 랜덤 그로기
                unitActor.AI.SetFindDistance(4f);

                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> YieldSpawnMazeMonster()
        {
            if (map is null)
                yield break;

            List<MazeSpawnZone> spawnZoneList = new List<MazeSpawnZone>();
            spawnZoneList.AddRange(map.GetFixedZone());
            spawnZoneList.AddRange(map.GetRandomZone(1));
            spawnZoneList.AddRange(map.GetRandomZone(2));

            if (spawnZoneList.Count < currentData.max_count)
            {
                throw new System.Exception($"맵에 스폰존 부족: spawnZone={spawnZoneList.Count}, max_count={currentData.max_count}");
            }

            cubeMaxCount = currentData.max_count;
            curCubeCount = currentData.max_count;

            int originalClickCount = currentData.click_count;
            for (int i = 0; i < cubeMaxCount; ++i)
            {
                MazeMonsterEntity coinMonster = factory.CreateMazeDungeonMonster(dungeonType);
                if (impl.Add(coinMonster, isEnemy: true))
                {
                    UnitActor coinMonsterActor = coinMonster.SpawnActor(); // 유닛 소환
                    coinMonsterActor.CachedTransform.localScale = Vector3.zero; // Vector3.one * MONSTER_SCALE; // 스케일 적용
                    coinMonsterActor.AI.SetHomePosition(spawnZoneList[i].GetCenter(), isWarp: true); // 위치 세팅
                    const int CUBE_MAX = 6;
                    int randNum = Random.Range(0, CUBE_MAX) + 1;
                    coinMonster.SetCubeIndex(randNum);
                    coinMonster.SetClickCount(originalClickCount - ((CUBE_MAX - randNum) * 2));
                    MazeBattleType randomType = randNum.ToEnum<MazeBattleType>();
                    MazeMonsterEffectPlayer effectPlayer = coinMonsterActor.EffectPlayer as MazeMonsterEffectPlayer;
                    effectPlayer.SpawnMonsterCube(randomType, isBoss: false); // 큐브 생성
                    switch (dungeonType)
                    {
                        case DungeonType.ExpDungeon:
                            effectPlayer.SpawnMazeExp();
                            break;
                        case DungeonType.ZenyDungeon:
                            effectPlayer.SpawnMazeZeny();
                            break;
                    }

                    coinMonster.OnDespawnActor += OnMonsterDespawn;
                    coinMonster.OnChangeHP += OnChangedClickerMonaterHp;
                    coinMonster.OnHit += OnMonsterHit;
                    yield return Timing.WaitForOneFrame;
                }
            }

            // 몬스터 생성 큐브 지정
            spawnMonsterCubeList.Clear();
            if (currentData.normal_monster_count > 0)
            {
                if (cubeMaxCount < currentData.normal_monster_count)
                {
                    throw new System.Exception($"몬스터 생성 큐브 수보다 큐브의 수가 부족하다 : {nameof(cubeMaxCount)}={cubeMaxCount}, {nameof(currentData.normal_monster_count)}={currentData.normal_monster_count}");
                }

                var monsterSpawnCubes = System.Array.ConvertAll(unitList.FindAll(e => e.type == UnitEntityType.MazeMonster).GetRandomPick(currentData.normal_monster_count), e => e as MazeMonsterEntity);
                foreach (var monsterSpawnCube in monsterSpawnCubes)
                {
                    spawnMonsterCubeList.Add(monsterSpawnCube.clientUID);
                }
            }
        }

        void OnMonsterDespawn(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnMonsterDespawn;
            actor.Entity.OnChangeHP -= OnChangedClickerMonaterHp;
            actor.Entity.OnHit -= OnMonsterHit;
        }

        void OnMonsterHit(UnitEntity unit, UnitEntity attacker, int value, int count, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int elementFactor)
        {
            UnitActor actor = unit.GetActor();
            if (actor)
                actor.EffectPlayer.GenerateImpulse(BattlePoolManager.ImpulseType.ClickAttack);

            if (dungeonType == DungeonType.ExpDungeon)
            {
                MazeGold particle = battlePool.SpawnMazeExp(unit.LastPosition);
                particle.CachedTransform.localScale = Vector3.one * 0.5f;
                particle.SetJumpHeight(0.5f);
                particle.SetGravity(2f);
                particle.SetSpread(true);
                PlaySfx(Constants.SFX.Battle.EXP);
                IMazeDropItem iDropItem = particle;
                iDropItem.Set(0, MazeRewardType.None, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Exp));
                iDropItem.StartEffect();
            }
            else if (dungeonType == DungeonType.ZenyDungeon)
            {
                MazeGold particle = battlePool.SpawnMazeZeny(unit.LastPosition);
                particle.CachedTransform.localScale = Vector3.one * 0.5f;
                particle.SetJumpHeight(0.35f);
                particle.SetGravity(2f);
                particle.SetSpread(true);
                PlaySfx(Constants.SFX.Battle.ZENY);
                IMazeDropItem iDropItem = particle;
                iDropItem.Set(0, MazeRewardType.None, uiBattlePlayerStatus.GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Zeny));
                iDropItem.StartEffect();
            }
        }
    }
}