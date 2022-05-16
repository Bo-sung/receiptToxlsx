using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace Ragnarok
{
    public sealed class ScenarioMazeEntry : BattleEntry
    {
        public const float SPEED = 10f;
        private const float GHOST_SPAWN_DISTANCE = 5f;
        private const float GHOST_START_MOVE_DELAY = 2f; // 스폰 후 이동시작까지의 딜레이
        private const float GHOST_SWITCH_STATE_INTERVAL = 7f;
        private const float GHOST_SPEED_RATE = 0.7f; // 노말몹의 이동속도는 플레이어의 70%
        private const float BOSS_SPEED_RATE = 0.6f; // 보스의 이동속도는 플레이어의 60%
        private const float PLAYER_CHASING_STATE_SWITCH_RATE = 0.5f; // 플레이어 추적 상태 전환 성공률 : 50%

        private const float FREEZE_DURATION = 7f * 1000f; // 얼음모드 지속시간
        private const float FREEZE_TAP_DECREASE_TIME = 0.5f * 1000f; // 얼음모드 탭 감소 시간

        private const float IGNORE_COLLISION_DURATION = 1f * 1000f; // 충돌 무시 지속시간
        private const float COIN_RESPAWN_DELAY = 1f; // 코인 재생성 딜레이
        private const float GHOST_RESPAWN_DELAY = 1f; // 코인 재생성 딜레이

        private const int CUBE_CLICK_COUNT = 5; // 큐브 체력 5방

        private const CameraController.View NPC_VIEW = CameraController.View.Front;
        private const CameraController.View GUIDE_VIEW = CameraController.View.Guide;
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;
        private const CameraController.View ZOOM_VIEW = CameraController.View.Quater40_10;
        private const float CAMERA_NOISE_FOCUS_ENEMY = 2f;

        /******************** Models ********************/
        private readonly DungeonModel dungeonModel;
        private readonly CharacterModel characterModel;
        private readonly IBattlePool battlePool;

        /******************** Repositories ********************/
        private readonly ScenarioMazeDataManager scenarioMazeRepo;

        /******************** Scene Compositions ********************/
        private Map map;
        private Dictionary<NpcEntity, GameObject> npcActorDic; // 씬의 NPC Object와 NPC Entity 매칭
        private WayPointZone[] wayPointZones; // 웨이포인트 지점 목록
        private Dictionary<System.Tuple<int, int>, float> distanceMap; // 각 웨이포인트의 거리

        /******************** UIs ********************/
        private UIController uiController;
        private UIDialogue uiDialogue;
        private UIBattleFail uiBattleFail;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleMenu uiBattleMenu;
        private UIMainTop uiMainTop;
        private UIEnemyFocus uiEnemyFocus;
        private UIBattleProgress uiBattleProgress;
        private UIResultClear uiResultClear;
        private UIBattleBossHp uiBattleBossHp;
        private UIMazeHud uiMazeHud;
        private UICamSelect uiCamSelect;
        private UIBattleInfo uiBattleInfo;
        private UIMatchMultiMazeStatus uiMatchMultiMazeStatus;

        /******************** Temp Data ********************/
        private ScenarioMazeData currentData;
        private float savedDefaultSpeed;
        private UnitMovement.Mode savedMove;
        private int savedHp;
        private int droppedZeny; // 일반 제니
        private int droppedCubePiece; // 퀘스트코인
        private int droppedZenyCube; // 제니큐브
        private int droppedExpCube; // 경험치큐브
        private int droppedSpeedItem; // 이속포션
        private Dictionary<MonsterBotEntity, (UnitMovement.Mode mode, float speed)> savedGhostDataDic; // 고스트 
        private BetterList<UnitEntity> collisionMonsterList; // 부딪힌 몬스터 목록 (중복 충돌 방지)
        private RemainTime freezeRemainTime; // 얼음모드 남은 시간 
        private MonsterBotEntity playerChasingGhostEntity; // 플레이어를 쫓고있는 유령
        private bool isBossTurn; // 보스 방향전환
        private Dictionary<MonsterBotEntity, MonsterCCType> monsterCCTypeDic; // [8. 상태이상미로] 몬스터 상태이상 정보 Dictionary
        private UnitEntity battleCubeEntity; // 현재 전투중인 큐브
        private MonsterBotEntity bossEntity;
        private NpcEntity npcEntity;
        private readonly List<PoolObject> poolObjectList;
        private RemainTime ignoreCollisionRemainTime; // 충돌 무시 남은 시간

        private readonly List<WayPointZone> zenySpawnPointList; // 제니 스폰 위치 목록 
        private readonly List<WayPointZone> pieceSpawnPointList; // 아이템/큐브 스폰 위치 목록 

        private CoroutineHandle resetPlayerSpeedCoroutine;
        private CoroutineHandle updateGhostAICoroutine;

        private bool isMonsterItemDrop;
        private bool isRequestBattleEnd;
        private bool isBattleStart;
        private bool isCheckOpenContent; // 오픈컨텐츠 연출 보여줄지 여부

        private enum Guideline
        {
#pragma warning disable CA1712 // Do not prefix enum values with type name
            None, Guideline1, Guideline2, Guideline3,
#pragma warning restore CA1712 // Do not prefix enum values with type name
        }

        /// <summary>
        /// [8. 상태이상미로] 몬스터 CC타입
        /// </summary>
        [System.Flags]
        private enum MonsterCCType
        {
            None = 0,
            Die = 1 << 0, // 몬스터 사망 후 재생성
            Damage = 1 << 1, // 플레이어의 체력 감소
            Coin = 1 << 2, // 플레이어의 코인 차감
            Freeze = 1 << 3, // 플레이어 경직
        }

        private GameObject goGuideline1, goGuideline2, goGuideline3;
        private GameObject tempMazeCoin;

        public ScenarioMazeEntry() : base(BattleMode.ScenarioMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);
            dungeonModel = Entity.player.Dungeon;
            characterModel = player.Character;
            battlePool = BattlePoolManager.Instance;

            scenarioMazeRepo = ScenarioMazeDataManager.Instance;

            npcActorDic = new Dictionary<NpcEntity, GameObject>();
            savedGhostDataDic = new Dictionary<MonsterBotEntity, (UnitMovement.Mode mode, float speed)>();
            collisionMonsterList = new BetterList<UnitEntity>();
            distanceMap = new Dictionary<System.Tuple<int, int>, float>();
            monsterCCTypeDic = new Dictionary<MonsterBotEntity, MonsterCCType>();
            poolObjectList = new List<PoolObject>();
            zenySpawnPointList = new List<WayPointZone>();
            pieceSpawnPointList = new List<WayPointZone>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            cameraController.RemoveMask(Layer.NPC, Layer.MAZE_ENEMY);
            cameraController.SetView(DEFAULT_VIEW);
            cameraController.SetCameraNoise();

            CameraUtils.Zoom(CameraZoomType.None);

            map = null;
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();
            dungeonModel.OnMonsterItemDrop += OnMonsterItemDrop;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();
            dungeonModel.OnMonsterItemDrop -= OnMonsterItemDrop;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiController = UI.Show<UIController>();
            uiDialogue = UI.Show<UIDialogue>();
            uiBattleFail = UI.Show<UIBattleFail>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiMainTop = UI.Show<UIMainTop>();
            uiEnemyFocus = UI.Show<UIEnemyFocus>();
            uiBattleProgress = UI.Show<UIBattleProgress>();
            uiResultClear = UI.Show<UIResultClear>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiMazeHud = UI.Show<UIMazeHud>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiMatchMultiMazeStatus = UI.Show<UIMatchMultiMazeStatus>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
#if UNITY_EDITOR
            uiController.OnSuccessKonamiCommand += OnSuccessKonamiCommand;
#endif

            uiBattleFail.OnConfirm += OnBattleFailConfirm;
            uiBattleMenu.OnExit += OnBattleExit;
            uiResultClear.OnFinishDungeon += OnFinishDungeon;

            uiMazeHud.OnClicked += OnClickerMonsterClick;

            uiDialogue.Hide();
            uiBattleFail.Hide();
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiEnemyFocus.Hide();
            uiBattleBossHp.Hide();
            uiMazeHud.Hide();
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiResultClear.Hide();
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
#if UNITY_EDITOR
            uiController.OnSuccessKonamiCommand -= OnSuccessKonamiCommand;
#endif

            uiBattleFail.OnConfirm -= OnBattleFailConfirm;
            uiBattleMenu.OnExit -= OnBattleExit;
            uiResultClear.OnFinishDungeon -= OnFinishDungeon;

            uiMazeHud.OnClicked -= OnClickerMonsterClick;

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);

            UI.Close<UIController>();
            UI.Close<UIBattleFail>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIMainTop>();
            UI.Close<UIEnemyFocus>();
            UI.Close<UIBattleProgress>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIMazeHud>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIMatchMultiMazeStatus>();

            uiController = null;
            uiBattleFail = null;
            uiBattlePlayerStatus = null;
            uiBattleMenu = null;
            uiMainTop = null;
            uiEnemyFocus = null;
            uiBattleProgress = null;
            uiResultClear = null;
            uiBattleBossHp = null;
            uiMazeHud = null;
            uiCamSelect = null;
            uiBattleInfo = null;
            uiMatchMultiMazeStatus = null;
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
            if (uiResultClear.IsVisible)
            {
                uiResultClear.OnClickedBtnConfirm();
                return;
            }

            OnBattleExit();
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            npcActorDic.Clear();
            savedGhostDataDic.Clear();
            collisionMonsterList.Clear();
            monsterCCTypeDic.Clear();

            distanceMap.Clear();

            droppedZeny = 0;
            droppedCubePiece = 0;
            droppedZenyCube = 0;
            droppedExpCube = 0;
            droppedSpeedItem = 0;

            zenySpawnPointList.Clear();
            pieceSpawnPointList.Clear();

            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();

            HideBossTargetArrow();
            HideNpcTargetArrow();

            if (resetPlayerSpeedCoroutine != null)
                Timing.KillCoroutines(resetPlayerSpeedCoroutine);
            if (updateGhostAICoroutine != null)
                Timing.KillCoroutines(updateGhostAICoroutine);
            player.SetForceStatus(ForceStatusType.MazeMoveSpdUpOff);
        }

        /// <summary>
        /// 특정 id로 전투 시작
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            currentData = scenarioMazeRepo.Get(id);

            if (currentData == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            var param = Protocol.NewInstance();
            param.PutInt("1", id);

            Task<Response> task = Protocol.REQUEST_SCENARIO_MAZE_START.SendAsync(param);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
            {
                response.ShowResultCode();
                yield break;
            }

            savedHp = response.GetInt("1"); // 플레이어 최대 체력

            dungeonModel.SetLastEnterMultiMazeLobbyId(currentData.Chapter); // 마지막 입장한 Lobby도 변경해 줌

            if (savedHp > 0)
            {
                player.SetExtraOption(ExtraBattleOptionType.MaxHp, savedHp);
                ScenarioMazeBossEntry.SetPlayerHp(savedHp);
            }

            savedHp -= 1000;
            player.ResetSkillCooldown();
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

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            // Clear MapData
            map = null;
            goGuideline1 = null;
            goGuideline2 = null;
            goGuideline3 = null;
            tempMazeCoin = null;

            foreach (var item in roots)
            {
                if (map == null)
                    map = item.GetComponent<Map>();

                if (goGuideline1 == null && item.name.Equals(nameof(Guideline.Guideline1)))
                    goGuideline1 = item;

                if (goGuideline2 == null && item.name.Equals(nameof(Guideline.Guideline2)))
                    goGuideline2 = item;

                if (goGuideline3 == null && item.name.Equals(nameof(Guideline.Guideline3)))
                    goGuideline3 = item;

                if (tempMazeCoin == null && item.name.Equals("TempMazeCoin"))
                    tempMazeCoin = item;
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

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            droppedZeny = 0;
            droppedCubePiece = 0;
            droppedZenyCube = 0;
            droppedExpCube = 0;
            droppedSpeedItem = 0;

            battleCubeEntity = null;
            playerChasingGhostEntity = null;
            isBossTurn = false;
            isBattleStart = false;
            bossEntity = null;
            npcEntity = null;

            cameraController.AddMask(Layer.NPC, Layer.MAZE_ENEMY);

            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattlePlayerStatus.SetExtraMode(GetCurrentExtraMode());
            uiBattlePlayerStatus.SetPlayer(player);
            uiBattlePlayerStatus.SetExtraCount(GetCurrentExtraMode(), 0, GetNeedPiece());

            player.SetCurrentMp(player.MaxMp);

            cameraController.SetView(NPC_VIEW);

            ScenarioMazeMode currentMode = GetCurrentMode();
            if (currentMode == ScenarioMazeMode.FirstTutorial || currentMode == ScenarioMazeMode.SecondTutorial)
            {
                uiBattleMenu.Hide();
            }

            // [8. 상태이상미로] 전용 UI
            if (IsShowUIMatchMultiMazeStatus())
            {
                uiMatchMultiMazeStatus.SetRemainCoin(currentData.boss_condition_value, currentData.boss_condition_value);
                uiMatchMultiMazeStatus.ShowRemainCoin(false);
            }
            else
            {
                uiMatchMultiMazeStatus.Hide();
            }

            // 오픈 컨텐츠가 필요한 모드가 아님
            if (currentMode.GetOpenContentType() == default)
            {
                isCheckOpenContent = false;
            }
            else
            {
                isCheckOpenContent = !dungeonModel.IsCleared(currentMode); // 클리어한 적이 없는 컨텐츠
            }
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            // 플레이어 생성
            yield return Timing.WaitUntilDone(YieldSpawnCharacter(player), TAG);

            // NPC 생성
            var NpcSpawnZones = map.GetNpcSpawnZones();
            yield return Timing.WaitUntilDone(YieldSpawnNPC(NpcSpawnZones), TAG);
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCharacter(CharacterEntity character)
        {
            UnitActor characterActor;
            if (impl.Add(character, isEnemy: false))
            {
                character.OnSpawnActor += OnCharacterSpawn;
                character.OnDespawnActor += OnCharacterDespawn;

                characterActor = character.SpawnActor();
                //cameraController.SetPlayer(characterActor.CachedTransform); // 카메라 타겟 세팅
                characterActor.AI.SetFindDistance(4f);

                yield return Timing.WaitForOneFrame;
            }
            else
            {
                characterActor = character.GetActor();
            }

            Vector3 homePos = map.GetPlayerPosition();
            characterActor.AI.SetHomePosition(homePos, isWarp: false); // 위치 세팅
            characterActor.Movement.ForceWarp(homePos);
        }

        /// <summary>
        /// 유령(보스+쫄몹) 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnGhost(bool spawnBossGhost, int spawnNormalGhostCount)
        {
            ScenarioMazeMode mode = GetCurrentMode();

            if (!spawnBossGhost && spawnNormalGhostCount == 0)
                yield break;

            // 보스 스폰
            if (spawnBossGhost)
            {
                bossEntity = MonsterEntity.Factory.CreateMonsterBot();
                bossEntity.Initialize(currentData.boss_monster_id, currentData.boss_monster_level, MonsterType.Boss, MathUtils.ToPercentValue(currentData.boss_monster_scale));

                if (impl.Add(bossEntity, isEnemy: true))
                {
                    var actor = bossEntity.SpawnActor();

                    actor.Movement.Resume();

                    savedGhostDataDic[bossEntity] = (
                        mode: actor.Movement.GetMode(),
                        speed: actor.Movement.GetDefaultSpeed()
                        );
                    actor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                    actor.Movement.SetDefaultSpeed(SPEED * BOSS_SPEED_RATE);

                    Vector3 homePos = map.GetBossPosition();
                    actor.AI.SetHomePosition(homePos, isWarp: false);
                    actor.Movement.ForceWarp(homePos);

                    // 프로텍트 오라 발생
                    actor.EffectPlayer.ShowAura(UnitAuraType.MazeBossMonsterProtect);

                    // 이벤트 등록
                    bossEntity.OnDespawnActor += OnEnemyDespawnBoss;
                    actor.AI.OnTempCollisionActor += OnCollisionActorBoss;
                    actor.Movement.OnWayPoint += OnGhostEnterWayPoint;

                    actor.AI.ReadyToBattle();

                    yield return Timing.WaitForOneFrame;
                }
            }

            // 쫄몹 스폰
            if (spawnNormalGhostCount > 0)
            {
                // 스폰 위치 결정
                Vector3[] ghostHomePosArr = null;
                switch (mode)
                {
                    case ScenarioMazeMode.CrowdControl:
                        ghostHomePosArr = map.GetWayPointZones().GetRandomPick(spawnNormalGhostCount).Select(e => e.CachedTransform.position).ToArray();
                        break;
                    default:
                        ghostHomePosArr = map.GetAroundPosition(pivot: map.GetBossPosition(), GHOST_SPAWN_DISTANCE, spawnNormalGhostCount); // 보스 주변
                        break;
                }

                for (int i = 0; i < spawnNormalGhostCount; ++i)
                {
                    MonsterBotEntity ghostEntity = MonsterEntity.Factory.CreateMonsterBot();
                    ghostEntity.Initialize(currentData.normal_monster_id, currentData.normal_monster_level);

                    if (impl.Add(ghostEntity, isEnemy: true))
                    {
                        var actor = ghostEntity.SpawnActor();

                        actor.Movement.Resume();

                        savedGhostDataDic[ghostEntity] = (
                            mode: actor.Movement.GetMode(),
                            speed: actor.Movement.GetDefaultSpeed()
                            );
                        actor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                        actor.Movement.SetDefaultSpeed(SPEED * GHOST_SPEED_RATE);

                        Vector3 homePos = ghostHomePosArr[i];
                        actor.AI.SetHomePosition(homePos, isWarp: false);
                        actor.Movement.ForceWarp(homePos);

                        // 이벤트 등록
                        ghostEntity.OnDespawnActor += OnEnemyDespawnGhost;
                        actor.AI.OnTempCollisionActor += OnCollisionActorGhost;
                        actor.Movement.OnWayPoint += OnGhostEnterWayPoint;
                        actor.Movement.OnMoveStop += OnGhostMoveStop;

                        actor.AI.ReadyToBattle();
                        actor.EffectPlayer.PlayPanelBuffEffect(); // 쫄몹 소환 이펙트
                                                                  //bossEntity.SetCurrentHp(100);

                        // [8. 상태이상미로] 몬스터에게 CC속성 부여
                        if (mode == ScenarioMazeMode.CrowdControl)
                        {
                            MonsterCCType ccType = GenerateMonsterCCType(); // 랜덤 CCType 생성
                            monsterCCTypeDic[ghostEntity] = ccType;

                            // 코인타입 -> Blue
                            if (ccType.HasFlagEnum(MonsterCCType.Coin))
                            {
                                actor.EffectPlayer.ShowUnitAura(UnitAura.AuraType.MonsterBlue);
                            }

                            // 경직타입 -> Red
                            if (ccType.HasFlagEnum(MonsterCCType.Freeze))
                            {
                                actor.EffectPlayer.ShowUnitAura(UnitAura.AuraType.MonsterRed);
                            }
                        }

                        yield return Timing.WaitForOneFrame;
                    }
                }
            }

            // Ghost AI 업데이트
            if (updateGhostAICoroutine != null)
                Timing.KillCoroutines(updateGhostAICoroutine);
            updateGhostAICoroutine = Timing.RunCoroutine(YieldUpdateGhostAI(), TAG);
            yield break;

            // CCType 랜덤 생성
            MonsterCCType GenerateMonsterCCType()
            {
                const float ccRate = 0.25f; // 각 속성 부여 확률

                MonsterCCType retCCType = MonsterCCType.None;

                // 속성 부여
                while (retCCType == MonsterCCType.None)
                {
                    var ccTypes = System.Enum.GetValues(typeof(MonsterCCType)).Cast<MonsterCCType>();
                    foreach (var ccType in ccTypes)
                    {
                        if (Random.value <= ccRate)
                        {
                            retCCType.AddFlagEnum(ccType);
                        }
                    }
                }

                return retCCType;
            }
        }

        /// <summary>
        /// 유령 AI 업데이트
        /// </summary>
        private IEnumerator<float> YieldUpdateGhostAI()
        {
            playerChasingGhostEntity = null; // 플레이어 추적 유령 초기화

            yield return Timing.WaitForSeconds(GHOST_START_MOVE_DELAY); // 2초간 대기 후 이동시작

            var bossEntity = unitList.Find(e => e is MonsterBotEntity monEntity && monEntity.MonsterType == MonsterType.Boss) as MonsterBotEntity;
            var ghostEntities = unitList.FindAll(e => e is MonsterBotEntity monEntity && monEntity.MonsterType == MonsterType.Normal).ConvertAll(e => e as MonsterBotEntity);

            // 보스 출발
            if (bossEntity && bossEntity.DestWayPoint == null)
            {
                var nearestZone = GetNearestWayPoint(bossEntity.LastPosition);
                SetMonsterBotDestination(bossEntity, nearestZone);
            }

            foreach (var ghost in ghostEntities.OrEmptyIfNull())
            {
                if (ghost.DestWayPoint != null)
                    continue;

                var nearestZone = GetNearestWayPoint(ghost.LastPosition);
                SetMonsterBotDestination(ghost, nearestZone);
            }

            // 미로타입별 분기
            ScenarioMazeMode mode = GetCurrentMode();
            if (mode == ScenarioMazeMode.CrowdControl) // [8. 상태이상미로] 추적 없이 배회만 하도록..
                yield break;

            while (true)
            {
                // 유령 AI 업데이트
                ghostEntities = unitList.FindAll(e => e is MonsterBotEntity monEntity && monEntity.MonsterType == MonsterType.Normal).ConvertAll(e => e as MonsterBotEntity);

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

                // 보스 방향 전환
                isBossTurn = true;


                // 상태 : 배회, 추적
                // 배회 : 7초마다 50% 확률로 플레이어 추적 시도 (동시에 여러 유령이 추적하려하면 가장 가까운 유령만 추적)
                // 추적 : 7초마다 50% 확률로 배회로 되돌아감
                // 보스 : 7초마다 방향 전환 

                // 배회 알고리즘
                // Begin : 현재 위치와 가장 가까운 WayPoint로 이동
                // Update : 인접한 WayPoint들 중 랜덤으로 이동 (최근 WayPoint는 선택하지 않음), 도착하면 LastWayPoint 갱신하고 다시 Update

                // 추적 알고리즘
                // Update : 플레이어와 가장 가까운 WayPoint로 이동하는게 주 목표. 
                // 바로 SetDestination하지 않고 Distance Map으로 A* 알고리즘 적용해서 각 WayPoint를 밟으면서 이동.
                // Path 갱신 주기는 n초

                // 보스 방향전환 알고리즘
                // WayPoint도착 이벤트 발생 시, 목표 WayPoint를 최근 WayPoint로 강제 지정.

                yield return Timing.WaitForSeconds(GHOST_SWITCH_STATE_INTERVAL);
            }
        }

        /// <summary>
        /// 플레이어를 추적할 유령 뽑기
        /// </summary>
        private MonsterBotEntity PickNewChasingGhost()
        {
            var ghostEntities = unitList.FindAll(e => e is MonsterBotEntity monEntity && monEntity.MonsterType == MonsterType.Normal && !e.IsDie).ConvertAll(e => e as MonsterBotEntity);

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

            // 보스인 경우, 방향전환 체크
            if (monsterBotEntity.MonsterType == MonsterType.Boss)
            {
                if (isBossTurn)
                {
                    isBossTurn = false;
                    if (monsterBotEntity.LastWayPoint != default && monsterBotEntity.LastWayPoint != monsterBotEntity.DestWayPoint)
                    {
                        SetMonsterBotDestination(monsterBotEntity, monsterBotEntity.LastWayPoint);
                        return;
                    }
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
        /// 보스 디스폰 이벤트
        /// </summary>
        void OnEnemyDespawnBoss(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnEnemyDespawnBoss;
            actor.AI.OnTempCollisionActor -= OnCollisionActorBoss;
            actor.Movement.OnWayPoint -= OnGhostEnterWayPoint;

            if (actor.Entity is MonsterBotEntity entity && savedGhostDataDic.ContainsKey(entity))
            {
                actor.Movement.SetMode(savedGhostDataDic[entity].mode);
                actor.Movement.SetDefaultSpeed(savedGhostDataDic[entity].speed);
            }

            HideBossTargetArrow();
        }

        /// <summary>
        /// 쫄몹 디스폰 이벤트
        /// </summary>
        void OnEnemyDespawnGhost(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnEnemyDespawnGhost;
            actor.AI.OnTempCollisionActor -= OnCollisionActorGhost;
            actor.Movement.OnWayPoint -= OnGhostEnterWayPoint;
            actor.Movement.OnMoveStop -= OnGhostMoveStop;

            if (actor.Entity is MonsterBotEntity entity && savedGhostDataDic.ContainsKey(entity))
            {
                actor.Movement.SetMode(savedGhostDataDic[entity].mode);
                actor.Movement.SetDefaultSpeed(savedGhostDataDic[entity].speed);
            }
        }

        /// <summary>
        /// 미로맵 보스와 충돌
        /// </summary>
        void OnCollisionActorBoss(UnitActor playerActor, UnitEntity bossEntity)
        {
            if (collisionMonsterList.Contains(bossEntity))
                return;

            // 보스와 충돌
            collisionMonsterList.Add(bossEntity);

            // 필요 아이템/큐브 수집 여부에 따른 처리
            var mode = GetCurrentMode();
            if (mode == ScenarioMazeMode.QuestCoin || mode == ScenarioMazeMode.SecondTutorial)
            {
                if (CheckPieceCondition())
                {
                    Timing.RunCoroutine(YieldEnterBossBattle(bossEntity), TAG);
                    return;
                }
            }

            // 큐브조각 부족할 시 사망
            player.Die(null);
        }

        private void ShowBossTargetArrow()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ShowBossTargetingArrow(bossEntity);
        }

        private void HideBossTargetArrow()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ReleaseBossTargetingArrow();
        }

        private void ShowNpcTargetArrow()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ShowBossTargetingArrow(npcEntity);
        }

        private void HideNpcTargetArrow()
        {
            UnitActor playerActor = player.GetActor();
            if (playerActor)
                playerActor.EffectPlayer.ReleaseBossTargetingArrow();
        }

        /// <summary>
        /// <see cref="ScenarioMazeBossEntry"/>
        /// 보스전투 진입
        /// </summary>
        private IEnumerator<float> YieldEnterBossBattle(UnitEntity bossEntity)
        {
            var actor = bossEntity?.GetActor();

            // 보스 몬스터 정지
            actor?.Movement.Stop();

            yield return Timing.WaitUntilDone(ShowEnemyFocusFX(actor), TAG);

            // NPC대사와 함께 보스와 충돌하면 대사UI를 강제로 종료해준다.
            if (uiDialogue && uiDialogue.IsVisible)
                uiDialogue.Hide();

            BattleManager.Instance.StartBattle(BattleMode.ScenarioMazeBoss, currentData.id);
        }

        /// <summary>
        /// 카메라 확대 연출
        /// </summary>
        private IEnumerator<float> ShowEnemyFocusFX(UnitActor bossActor)
        {
            OnResetController();
            uiController.Hide();
            cameraController.SetView(DEFAULT_VIEW);
            if (bossActor != null)
            {
                cameraController.SetPlayer(bossActor.CachedTransform);
                bossActor.AI.ChangeState(AI.Transition.EndBattle);
                bossActor.EffectPlayer.HideBattleHUD();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"대상 몬스터가 없다. {bossActor}");
#endif
            }

            foreach (var unit in unitList)
            {
                unit.GetActor()?.AI.EndAI();
            }

            yield return Timing.WaitForOneFrame;

            uiEnemyFocus.Show(showNeedleFX: false, showSpreadFX: true);
            cameraController.SetView(ZOOM_VIEW);
            cameraController.SetCameraNoise(CAMERA_NOISE_FOCUS_ENEMY);
            CameraUtils.Zoom(CameraZoomType.ZoomIn);

            yield return Timing.WaitForSeconds(0.8f);
        }

        /// <summary>
        /// 미로맵 몬스터와 충돌
        /// </summary>
        void OnCollisionActorGhost(UnitActor playerActor, UnitEntity ghostEntity)
        {
            ScenarioMazeMode mode = GetCurrentMode();

            // 중복 충돌 처리
            if (mode == ScenarioMazeMode.CrowdControl)
            {
                if (ignoreCollisionRemainTime.ToRemainTime() > 0)
                    return;

                // 충돌 무시 시간 부여
                ignoreCollisionRemainTime = IGNORE_COLLISION_DURATION;
            }
            else
            {
                if (collisionMonsterList.Contains(ghostEntity))
                    return;

                // 유령과 충돌
                collisionMonsterList.Add(ghostEntity);
            }


            bool isKillGhost = (mode != ScenarioMazeMode.CrowdControl);
            if (mode == ScenarioMazeMode.FirstTutorial)
            {
                playerActor.EffectPlayer.SpawnSpeechBalloon(SpeechBalloon.BalloonType.Shake, LocalizeKey._81002.ToText()); // 아야!
            }
            else if (mode == ScenarioMazeMode.CrowdControl)
            {
                MonsterBotEntity monsterBotEntity = ghostEntity as MonsterBotEntity;
                if (!monsterCCTypeDic.ContainsKey(monsterBotEntity))
                {
                    Debug.LogError("MonsterCCType이 없는 고스트와 충돌");
                    return;
                }

                // MonsterCCType에 따른 행동
                MonsterCCType ccType = monsterCCTypeDic[monsterBotEntity];
                if (ccType.HasFlagEnum(MonsterCCType.Die)) // 고스트 사망
                {
                    isKillGhost = true;
                    Timing.RunCoroutine(YieldSpawnGhost(spawnBossGhost: false, 1), TAG); // 고스트 추가 생성
                }
                if (ccType.HasFlagEnum(MonsterCCType.Damage)) // 플레이어 대미지
                {
                    GetDamageAsync(monsterBotEntity).WrapNetworkErrors();
                }
                if (ccType.HasFlagEnum(MonsterCCType.Coin)) // 코인 차감
                {
                    if (droppedCubePiece > 0)
                        Timing.RunCoroutine(YieldLostCoin(), TAG);
                }
                if (ccType.HasFlagEnum(MonsterCCType.Freeze)) // 플레이어 경직
                {
                    Timing.RunCoroutine(YieldGroggy(), TAG);
                }
            }
            else
            {
                // 얼음모드 시작
                Timing.RunCoroutine(YieldGroggy(), TAG);

                if (GetCurrentMode() != ScenarioMazeMode.FirstTutorial) // 튜토리얼에선 대미지 입지 않음
                {
                    GetDamageAsync(ghostEntity as MonsterBotEntity).WrapNetworkErrors();
                }
            }


            // 유령 사망 처리
            if (isKillGhost)
            {
                Timing.RunCoroutine(YieldDieGhost(ghostEntity), TAG);
            }
        }

        private IEnumerator<float> YieldDieGhost(UnitEntity ghostEntity)
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

        /// <summary>
        /// 코인 차감 및 코인 재생성
        /// </summary>
        private IEnumerator<float> YieldLostCoin()
        {
            var param = Protocol.NewInstance();
            param.PutByte("1", (byte)2);
            param.PutBool("2", false); // 차감
            Task<Response> task = Protocol.REQUEST_SCENARIO_MAZE_GET_COIN.SendAsync(param);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                ApplyCharUpdateData(charUpdateData);
            }

            UpdateDropCubePiece(droppedCubePiece - 1);

            // 코인 잃는 연출
            if (IsShowUIMatchMultiMazeStatus())
            {
                IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(player.LastPosition, MazeRewardType.MultiMazeCube);
                mazeDropItem.Set(pointId: default, MazeRewardType.CubePiece, uiMatchMultiMazeStatus.GetWidGet());
                AddPoolObject(mazeDropItem as PoolObject);
                mazeDropItem.StartEffect();
            }

            UI.ShowToastPopup(LocalizeKey._90190.ToText()); // 코인을 빼앗겼습니다.

            yield return Timing.WaitForSeconds(COIN_RESPAWN_DELAY); // 코인 재생성 딜레이 대기

            yield return Timing.WaitUntilDone(YieldSpawnQuestCoin(spawnCount: 1), TAG); // 코인 재생성
        }

        /// <summary>
        /// 고스트와의 충돌로 대미지
        /// </summary>
        private async Task GetDamageAsync(MonsterBotEntity monEntity)
        {
            if (monEntity is null)
                return;

            var response = await Protocol.REQUEST_SCENARIO_MAZE_NORMAL_MONSTER_DAMAGE.SendAsync();
            if (!response.isSuccess)
                return;

            int curHp = response.GetInt("1");
            int damage = response.GetInt("2");

            // 대미지 스펙 처리
            int dmgPerHit = damage;
            int blowCount = 1;
            bool isBasicActiveSkill = false;
            ElementType elementType = ElementType.Neutral;
            bool isCritical = true;

            player.ApplyDamage(attacker: monEntity, dmgPerHit, blowCount, isCritical, isBasicActiveSkill, elementType, totalDamage: damage);
            if (IsShowUIMatchMultiMazeStatus())
                uiMatchMultiMazeStatus.UpdateHp(GetPlayerCID(), player.CurHP, player.MaxHP);

            ScenarioMazeBossEntry.SetPlayerHp(curHp);

            UI.ShowToastPopup(LocalizeKey._90191.ToText()); // 피해를 입었습니다.
        }

        /// <summary>
        /// 아이템/큐브조각 전부 수집했는지 여부
        /// </summary>
        private bool CheckPieceCondition()
        {
            switch (GetCurrentMode())
            {
                case ScenarioMazeMode.QuestCoin:
                case ScenarioMazeMode.FirstTutorial:
                case ScenarioMazeMode.SecondTutorial:
                case ScenarioMazeMode.CrowdControl:
                    return (droppedCubePiece >= GetNeedPiece());
                case ScenarioMazeMode.ZenyCube:
                    return (droppedZenyCube >= GetNeedPiece());
                case ScenarioMazeMode.ExpCube:
                    return (droppedExpCube >= GetNeedPiece());
                case ScenarioMazeMode.SpeedItem:
                    return (droppedSpeedItem >= GetNeedPiece());
            }
            return false;
        }

        /// <summary>
        /// NPC 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnNPC(MazeNpcSpawnZone[] npcList)
        {
            foreach (var npc in npcList.OrEmptyIfNull())
            {
                npcEntity = NpcEntity.Factory.CreateNPC(NpcType.Deviruchi);
                npcActorDic[npcEntity] = npc.Actor;

                if (impl.Add(npcEntity, isEnemy: true))
                {
                    var actor = npcEntity.SpawnActor();
                    Vector3 homePos = npc.GetCenter();
                    actor.AI.SetHomePosition(homePos, isWarp: false);
                    actor.Movement.ForceWarp(homePos);
                    actor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                    actor.Movement.SetDefaultSpeed(SPEED);
                    actor.AI.SetFindDistance(npc.radius);
                    actor.AI.ReadyToBattle();

                    actor.Hide(); // NPC 숨김. 씬에서 NPC캐릭터를 정해주기 때문에 Entity만 유지하고 Actor는 숨김.

                    cameraController.SetNpc(actor.CachedTransform); // Npc 세팅

                    yield return Timing.WaitForOneFrame;
                }
            }
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 플레이어 이동속도, 모드 설정
            var actor = player.GetActor();
            if (actor != null)
            {
                savedMove = actor.Movement.GetMode();
                savedDefaultSpeed = actor.Movement.GetDefaultSpeed();

                actor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                actor.Movement.SetDefaultSpeed(SPEED);
            }

            // NPC Actor 다시 숨김. (씬에서 NPC캐릭터를 정해주기 때문에 Entity만 유지하고 Actor는 숨김)
            foreach (var unit in unitList)
            {
                if (unit.type != UnitEntityType.NPC)
                    continue;

                unit.GetActor().Hide();
            }

            // [튜토리얼 모드] 튜토리얼 시작
            var mode = GetCurrentMode();
            if (mode == ScenarioMazeMode.FirstTutorial)
            {
                Timing.RunCoroutine(YieldTutorial(), TAG);
            }
            else
            {
                Timing.RunCoroutine(YieldPlayBeginingAnimation(), TAG); // 최초 연출 재생
                ShowNpcTargetArrow(); // Npc 타겟팅 추가
            }

            if (IsShowUIMatchMultiMazeStatus())
                uiMatchMultiMazeStatus.SetPlayers(new CharacterEntity[] { player });
        }

        /// <summary>
        /// 최초 연출 재생 (데비루치 -> 플레이어)
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldPlayBeginingAnimation()
        {
            uiController.Hide(); // 이동 입력 제한

            // 출석체크 사라질 때까지 대기
            UIDailyCheck uiDailyCheck = UI.GetUI<UIDailyCheck>();
            yield return Timing.WaitUntilTrue(() => uiDailyCheck == null || !uiDailyCheck.IsVisible);

            // 룰렛 팝업 사라질 때까지 대기
            UIRouletteEvent uiRouletteEvent = UI.GetUI<UIRouletteEvent>();
            yield return Timing.WaitUntilTrue(() => uiRouletteEvent == null || !uiRouletteEvent.IsVisible);

            yield return Timing.WaitForSeconds(1f); // 딜레이

            UnitActor npcActor = unitList.Find(e => e is NpcEntity)?.GetActor();
            UnitActor playerActor = player.GetActor();

            cameraController.SetView(DEFAULT_VIEW);
            cameraController.SetPlayer(playerActor.CachedTransform);

            uiController.Show(); // 이동 입력 제한 해제

            ShowQuestionMark(); // 데비루치 노란색 물음표 띄우기
        }

        /// <summary>
        /// 튜토리얼 연출
        /// </summary>
        private IEnumerator<float> YieldTutorial()
        {
            SetGuideline(Guideline.Guideline1); // 가이드라인 처리
            yield return Timing.WaitUntilDone(YieldPlayBeginingAnimation(), TAG); // 최초 연출 재생
            yield return Timing.WaitForSeconds(1f); // 딜레이

            // 대사 
            string dialog = LocalizeKey._17600.ToText(); // 으.. 처음 보는 곳인데..\n여긴 어디지..?$$저기 보이는 녀석에게 물어봐야겠어.
            yield return Timing.WaitUntilDone(YieldPlayerDialog(dialog), TAG);

            ShowNpcTargetArrow(); // Npc 타겟팅 추가
        }

        private void ShowQuestionMark()
        {
            Transform tfDeviruchiActor = map.GetNpcSpawnZones()[0].Actor.transform;
            UnitActor npcActor = unitList.Find(e => e is NpcEntity)?.GetActor();
            var effectPlayer = npcActor.EffectPlayer as NpcEffectPlayer;
            //effectPlayer.ShowQuestionMark_Yellow(tfDeviruchiActor);
            effectPlayer.ShowQuestionMark_Gray(tfDeviruchiActor);
        }

        private void ShowExclamationMark()
        {
            ScenarioMazeMode mode = GetCurrentMode();
            if (IsHideNpc(mode))
                return;

            Transform tfDeviruchiActor = map.GetNpcSpawnZones()[0].Actor.transform;
            UnitActor npcActor = unitList.Find(e => e is NpcEntity)?.GetActor();
            var effectPlayer = npcActor.EffectPlayer as NpcEffectPlayer;
            effectPlayer.HideQuestionMark_Gray();
            effectPlayer.ShowExclamationMark(tfDeviruchiActor);
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity.type == UnitEntityType.Player)
            {
                uiBattleProgress.StopTimer();

                ScenarioMazeMode currentMode = GetCurrentMode();
                bool isRetry = currentMode == ScenarioMazeMode.FirstTutorial || currentMode == ScenarioMazeMode.SecondTutorial;
                uiBattleFail.Show(isRetry ? UIBattleFail.ConfirmType.Retry : UIBattleFail.ConfirmType.Exit);
                if (IsShowUIMatchMultiMazeStatus())
                    uiMatchMultiMazeStatus.SetState(GetPlayerCID(), View.UIMatchPlayerSlot.State.Failed);
                return;
            }

            if (attacker.type == UnitEntityType.Player)
            {
                if (unitEntity.type == UnitEntityType.MazeMonster)
                {
                    var param = Protocol.NewInstance();
                    param.PutByte("1", 2);
                    param.PutBool("2", true);
                    Protocol.REQUEST_SCENARIO_MAZE_GET_COIN.SendAsync(param).WrapNetworkErrors();

                    PoolObject dieMark = battlePool.SpawnDieMark(unitEntity.LastPosition);
                    AddPoolObject(dieMark);

                    MazeMonsterEffectPlayer effectPlayer = unitEntity.GetActor().EffectPlayer as MazeMonsterEffectPlayer;
                    ScenarioMazeMode mode = GetCurrentMode();
                    switch (mode)
                    {
                        case ScenarioMazeMode.ExpCube:
                            IMazeDropItem dropExp = effectPlayer.mazeExp;
                            dropExp.Set(0, MazeRewardType.None, GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Exp));
                            effectPlayer.mazeExp.SetGravity(2f);
                            dropExp.StartEffect();
                            droppedExpCube += 1;
                            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.Exp, droppedExpCube, GetNeedPiece());
                            break;
                        case ScenarioMazeMode.ZenyCube:
                            IMazeDropItem dropZeny = effectPlayer.mazeZeny;
                            dropZeny.Set(0, MazeRewardType.None, GetWidget(UIExtraBattlePlayerStatus.ExtraMode.YellowZeny));
                            effectPlayer.mazeZeny.SetGravity(2f);
                            dropZeny.StartEffect();
                            droppedZenyCube += 1;
                            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.YellowZeny, droppedZenyCube, GetNeedPiece());
                            break;
                    }

                    SetClickerUI(false);
                    battleCubeEntity = null;

                    // 큐브조각을 충분히 모았으면 보스 프로텍트 해제
                    if (CheckPieceCondition())
                    {
                        ShowExclamationMark(); // 데비루치 ? 마커 제거, ! 마커 추가

                        if (IsShowBossArrow(mode))
                            ShowBossTargetArrow();

                        if (IsShowNpcArrow(mode))
                            ShowNpcTargetArrow();

                        Timing.RunCoroutine(YieldShowSecondConditionTalk(), TAG);
                    }
                }
            }
        }

        /// <summary>
        /// NPC 조건 알려주기
        /// </summary>
        private IEnumerator<float> YieldShowSecondConditionTalk()
        {
            player.GetActor().AI.ChangeState(AI.Transition.Hold);
            OnResetController();
            uiController.Hide();

            foreach (var unit in unitList)
            {
                var actor = unit.GetActor();

                if (!unit.IsEnemy || actor is null)
                    continue;

                actor.Movement.Pause();
            }

            ScenarioMazeMode mode = GetCurrentMode();
            if (IsHideNpc(mode))
            {
                UnitEntity bossEntity = unitList.Find(e =>
                {
                    MonsterBotEntity entity = e as MonsterBotEntity;
                    return entity != null && entity.MonsterType == MonsterType.Boss;
                });

                UnitActor bossActor = bossEntity?.GetActor();
                if (bossActor)
                {
                    // 보스 프로텍트 오라 제거
                    bossActor.EffectPlayer.ReleaseAura(UnitAuraType.MazeBossMonsterProtect);

                    cameraController.SetNpc(bossActor.CachedTransform);
                }

                // 쫄몹 모두 사망 처리
                var monsters = unitList.FindAll(e =>
                {
                    MonsterBotEntity entity = e as MonsterBotEntity;
                    return entity != null && entity.MonsterType == MonsterType.Normal;
                });
                monsters.ForEach(e => Timing.RunCoroutine(YieldDieGhost(e), TAG));

            }
            else
            {
                UnitActor npcActor = npcEntity.GetActor();
                if (npcActor)
                {
                    cameraController.SetNpc(npcActor.CachedTransform);
                }
            }

            cameraController.SetView(GUIDE_VIEW);

            // [튜토리얼 모드] 튜토리얼의 경우 클리어 대사
            string talk = GetSecondConditionNpcTalk();
            yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, talk), TAG); // #참 잘했어요.$$#참 잘했어요. 2

            cameraController.SetView(DEFAULT_VIEW);

            foreach (var unit in unitList)
            {
                var actor = unit.GetActor();

                if (!unit.IsEnemy || actor is null)
                    continue;

                actor.Movement.Resume();
            }

            // 플레이어 이동 가능
            player.GetActor().AI.ChangeState(AI.Transition.Finished);
            uiController.Show();
        }

        /// <summary>
        /// NPC, 큐브와 충돌
        /// </summary>
        void OnCollisionActor(UnitActor actor)
        {
            if (actor is null)
                return;

            if (battleCubeEntity != null) // 큐브와 전투중이면 무시
                return;

            // NPC
            if (actor.Entity.type == UnitEntityType.NPC)
            {
                Timing.RunCoroutine(YieldNPCTalk(actor.Entity as NpcEntity), TAG);
                return;
            }

            // 제니 큐브, 경험치 큐브
            if (actor.Entity.type == UnitEntityType.MazeMonster)
            {
                battleCubeEntity = actor.Entity;
                SetClickerUI(true);
                return;
            }
        }

        /// <summary>
        /// 클리커(큐브 전투) UI 설정
        /// </summary>
        private void SetClickerUI(bool isActive)
        {
            if (isActive)
            {
                OnResetController();
                uiController.Hide();
                uiMazeHud.ShowHud(battleCubeEntity.GetActor().gameObject);
                uiBattleBossHp.SetMode(UIBattleBossHp.Mode.NoName);
                uiBattleBossHp.Show(
                    0,
                    battleCubeEntity.GetName(),
                    battleCubeEntity.battleUnitInfo.UnitElementType,
                    UIBattleBossHp.MonsterType.Normal,
                    0,
                    battleCubeEntity.GetActor().gameObject);
                uiBattleBossHp.SetHp(battleCubeEntity.CurHP, battleCubeEntity.MaxHP);
            }
            else
            {
                uiController.Show();
                uiMazeHud.Hide();
                uiBattleBossHp.Hide();
            }
        }

        /// <summary>
        /// NPC와 대화
        /// </summary>
        private IEnumerator<float> YieldNPCTalk(NpcEntity npcEntity)
        {
            if (npcEntity is null)
                yield break;

            HideNpcTargetArrow(); // 타겟팅 표시 숨기기

            // 플레이어 멈춤. (대화 상태로 변경)
            if (!isBattleStart || CheckPieceCondition())
            {
                player.GetActor().AI.ChangeState(AI.Transition.Hold);
                OnResetController();
                uiController.Hide();
            }

            // 게임 클리어 조건 체크
            if (CheckPieceCondition())
            {
                // 몬스터 AI 전부 종료
                foreach (var unit in unitList)
                {
                    var actor = unit.GetActor();

                    if (!unit.IsEnemy || actor is null)
                        continue;

                    actor.Movement.Stop();
                    actor.AI.EndAI();
                }

                // [튜토리얼 모드] 튜토리얼의 경우 클리어 대사
                string clearScript = LocalizeKey._17601.ToText(); // 꽤 하네 너. 다음으로 넘어가볼까?
                yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, clearScript), TAG);

                RequestBattleEnd();
                yield break;
            }

            // 전투 중이면 대사 무시
            if (isBattleStart)
                yield break;

            isBattleStart = true;

            // 더미 퀘스트 코인 제거
            SetActiveTempMazeCoin(false);

            // 대사 출력 (대사할 때만 줌인)
            Transform tfDeviruchiActor = map.GetNpcSpawnZones()[0].Actor.transform;
            Vector3 originalRot = tfDeviruchiActor.localEulerAngles;
            tfDeviruchiActor.LookAt(player.GetActor().CachedTransform); // 데비루치가 플레이어를 쳐다봄

            cameraController.SetView(ZOOM_VIEW);
            CameraUtils.Zoom(CameraZoomType.ZoomIn);

            string script = currentData.script_id.ToText();
            yield return Timing.WaitUntilDone(YieldDialog(Npc.DEVIRUCHI, script), TAG); // 대사 출력

            CameraUtils.Zoom(CameraZoomType.None);
            cameraController.SetView(DEFAULT_VIEW);
            tfDeviruchiActor.localEulerAngles = originalRot; // 데비루치가 다시 원래 방향을 쳐다봄

            SetGuideline(Guideline.Guideline2); // 가이드라인 처리

            // 시나리오 미로 모드에 따라 분기
            ScenarioMazeMode mode = GetCurrentMode();

            // 플레이어 이동 가능
            bool canMove = (mode != ScenarioMazeMode.Dialog);
            if (canMove)
            {
                player.GetActor().AI.ChangeState(AI.Transition.Finished);
                uiController.Show();
            }

            // NPC 제거
            bool isHideNpc = IsHideNpc(mode);
            if (isHideNpc)
            {
                npcEntity.DespawnActor();
                npcActorDic[npcEntity].SetActive(false);
            }

            // 유령 생성
            bool spawnBossGhost = IsSpawnBossGhost(mode);
            int spawnGhostCount = IsSpawnNormalGhost(mode) ? (int)currentData.normal_monster_count : 0;
            yield return Timing.WaitUntilDone(YieldSpawnGhost(spawnBossGhost, spawnGhostCount), TAG);

            ////////////////// 대화 모드 //////////////////
            if (mode == ScenarioMazeMode.Dialog)
            {
                RequestBattleEnd();
                yield break;
            }
            ////////////////// 제니 큐브 모드 / 경험치 큐브 모드 //////////////////
            else if (mode == ScenarioMazeMode.ZenyCube || mode == ScenarioMazeMode.ExpCube)
            {
                yield return Timing.WaitUntilDone(YieldSpawnCube(mode), TAG);
            }
            ////////////////// 전투(유령 스폰) 모드 //////////////////
            else if (mode == ScenarioMazeMode.QuestCoin || mode == ScenarioMazeMode.CrowdControl)
            {
                // 퀘스트 코인 생성
                int spawnCount = GetNeedPiece();
                yield return Timing.WaitUntilDone(YieldSpawnQuestCoin(spawnCount), TAG);

                if (IsShowUIMatchMultiMazeStatus())
                    uiMatchMultiMazeStatus.ShowRemainCoin(true);
            }
            ////////////////// 이속포션 모드 //////////////////
            else if (mode == ScenarioMazeMode.SpeedItem)
            {
                yield return Timing.WaitUntilDone(YieldSpawnSpeedItem(), TAG);
            }
            ////////////////// 튜토리얼 모드 //////////////////
            else if (mode == ScenarioMazeMode.FirstTutorial || mode == ScenarioMazeMode.SecondTutorial)
            {
                // 퀘스트 코인 생성
                int spawnCount = GetNeedPiece();
                yield return Timing.WaitUntilDone(YieldSpawnQuestCoin(spawnCount), TAG);
            }

            // 제니 스폰 (위의 퀘스트코인/큐브 조각/이속포션 과 위치가 겹치지 않게)
            yield return Timing.WaitUntilDone(YieldSpawnCoin(pieceSpawnPointList.ToArray()), TAG);
        }

        /// <summary>
        /// 동전 랜덤 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCoin(WayPointZone[] exceptZones = null)
        {
            int spawnCount = currentData.zeny_max_count; // 생성 개수
            var totalSpawnableZones = map.GetWayPointZones().Where(e => e.IsSpawnable); // 스폰 가능한 전체 스폰존 리스트
            var spawnableZones = totalSpawnableZones.Except(exceptZones); // 중복위치 제거한 스폰존 리스트
            if (spawnCount > spawnableZones.Count()) // 스폰 카운트 체크
            {
#if UNITY_EDITOR
                Debug.LogError($"WayPoint({spawnableZones.Count()})보다 SpawnCount({spawnCount})가 더 많음");
#endif
                yield break;
            }

            var spawnZones = spawnableZones.GetRandomPick(spawnCount); // 랜덤 스폰 지점 확정
            zenySpawnPointList.AddRange(spawnZones); // 그냥 기록용.

            // 생성
            foreach (var spawnZone in spawnZones.OrEmptyIfNull())
            {
                IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(spawnZone.CachedTransform.position, MazeRewardType.Zeny);
                mazeDropItem.Set(pointId: default, MazeRewardType.Zeny, GetWidget(UIMainTop.MenuContent.Zeny));
                AddPoolObject(mazeDropItem as PoolObject);
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 퀘스트 코인 랜덤 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnQuestCoin(int spawnCount)
        {
            var spawnableZones = map.GetWayPointZones().Where(e => e.IsSpawnable); // 스폰 가능한 전체 스폰존 리스트
            if (spawnCount > spawnableZones.Count()) // 스폰 카운트 체크
            {
#if UNITY_EDITOR
                Debug.LogError($"WayPoint({spawnableZones.Count()})보다 SpawnCount({spawnCount})가 더 많음");
#endif
                yield break;
            }

            var spawnZones = spawnableZones.GetRandomPick(spawnCount); // 랜덤 스폰 지점 확정
            pieceSpawnPointList.AddRange(spawnZones); // 제니를 겹치지 않게 랜덤 생성하기 위해 기록.

            // 생성
            foreach (var spawnZone in spawnZones.OrEmptyIfNull())
            {
                IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(spawnZone.CachedTransform.position, MazeRewardType.MultiMazeCube);
                if (IsShowUIMatchMultiMazeStatus())
                {
                    mazeDropItem.Set(pointId: default, MazeRewardType.CubePiece, uiMatchMultiMazeStatus.GetWidget(GetPlayerCID()));
                }
                else
                {
                    mazeDropItem.Set(pointId: default, MazeRewardType.CubePiece, GetWidget(UIExtraBattlePlayerStatus.ExtraMode.CubePiece));
                }
                AddPoolObject(mazeDropItem as PoolObject);
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 제니큐브 또는 경험치큐브 랜덤 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCube(ScenarioMazeMode mode)
        {
            // 스폰 존 랜덤으로 지정
            var wayPoints = map.GetWayPointZones().Where(e => e.IsSpawnable); // 스폰 가능한 전체 스폰존 리스트
            int spawnCount = GetNeedPiece();
            if (spawnCount > wayPoints.Count())
            {
#if UNITY_EDITOR
                Debug.LogError($"WayPoint({wayPoints.Count()})보다 SpawnCount({spawnCount})가 더 많음");
#endif
                yield break;
            }
            var spawnZones = wayPoints.GetRandomPick(spawnCount); // 랜덤 스폰 지점 확정
            pieceSpawnPointList.AddRange(spawnZones); // 제니를 겹치지 않게 랜덤 생성하기 위해 기록.


            foreach (var spawnZone in spawnZones)
            {
                MazeMonsterEntity coinMonster = factory.CreateMazeDungeonMonster(dungeonType: default); // 던전타입 쓰이지 않음
                if (impl.Add(coinMonster, isEnemy: true))
                {
                    UnitActor coinMonsterActor = coinMonster.SpawnActor(); // 유닛 소환
                    coinMonsterActor.CachedTransform.localScale = Vector3.zero; // 몬스터 Actor 숨기기
                    coinMonsterActor.AI.SetHomePosition(spawnZone.CachedTransform.position, isWarp: false); // 위치 세팅  
                    coinMonsterActor.Movement.ForceWarp(spawnZone.CachedTransform.position);
                    const int CUBE_MAX = 6;
                    //int randNum = Random.Range(1, CUBE_MAX);
                    int randNum = 4;
                    coinMonster.SetCubeIndex(randNum);
                    coinMonster.SetClickCount(CUBE_CLICK_COUNT);
                    MazeBattleType randomType = randNum.ToEnum<MazeBattleType>();
                    MazeMonsterEffectPlayer effectPlayer = coinMonsterActor.EffectPlayer as MazeMonsterEffectPlayer;
                    effectPlayer.SpawnMonsterCube(randomType, isBoss: false); // 큐브 생성
                    switch (mode)
                    {
                        case ScenarioMazeMode.ZenyCube:
                            effectPlayer.SpawnMazeYellowZeny();
                            break;
                        case ScenarioMazeMode.ExpCube:
                            effectPlayer.SpawnMazeExp();
                            break;
                    }

                    coinMonster.OnDespawnActor += OnCubeDespawn;
                    coinMonster.OnChangeHP += OnChangedClickerMonaterHp;
                    coinMonster.OnHit += OnCubeHit;

                    coinMonsterActor.AI.ReadyToBattle();

                    coinMonsterActor.EffectPlayer.HideName();

                    yield return Timing.WaitForOneFrame;
                }
            }


        }

        /// <summary>
        /// 이속포션 랜덤 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnSpeedItem()
        {
            int spawnCount = GetNeedPiece(); // 생성 개수
            var spawnableZones = map.GetWayPointZones().Where(e => e.IsSpawnable); // 스폰 가능한 전체 스폰존 리스트
            if (spawnCount > spawnableZones.Count()) // 스폰 카운트 체크
            {
#if UNITY_EDITOR
                Debug.LogError($"WayPoint({spawnableZones.Count()})보다 SpawnCount({spawnCount})가 더 많음");
#endif
                yield break;
            }

            var spawnZones = spawnableZones.GetRandomPick(spawnCount); // 랜덤 스폰 지점 확정
            pieceSpawnPointList.AddRange(spawnZones); // 제니를 겹치지 않게 랜덤 생성하기 위해 기록.

            // 생성
            foreach (var spawnZone in spawnZones.OrEmptyIfNull())
            {
                IMazeDropItem mazeDropItem = battlePool.SpawnMazeDropItem(spawnZone.CachedTransform.position, MazeRewardType.SpeedItem);
                mazeDropItem.Set(pointId: default, MazeRewardType.SpeedItem, GetWidget(UIExtraBattlePlayerStatus.ExtraMode.SpeedItem));
                AddPoolObject(mazeDropItem as PoolObject);
                yield return Timing.WaitForOneFrame;
            }

            yield return 0;
        }

        private void OnDropSpeedItem(IMazeDropItem dropItem)
        {
            dropItem.StartEffect(); // Effect 발생 먼저.
            PlaySfx(Constants.SFX.Battle.SPEED_POTION);

            droppedSpeedItem += 1;
            uiBattlePlayerStatus.SetExtraCount(GetCurrentExtraMode(), droppedSpeedItem, GetNeedPiece());

            player.SetForceStatus(ForceStatusType.MazeMoveSpdUpOn);

            if (resetPlayerSpeedCoroutine != null)
                Timing.KillCoroutines(resetPlayerSpeedCoroutine);
            resetPlayerSpeedCoroutine = Timing.RunCoroutine(ResetPlayerSpeed(), TAG);

            // 큐브조각을 충분히 모았으면 보스 프로텍트 해제
            if (CheckPieceCondition())
            {
                SetGuideline(Guideline.Guideline3);

                ScenarioMazeMode mode = GetCurrentMode();

                ShowExclamationMark(); // 데비루치 ? 마커 제거, ! 마커 추가

                if (IsShowBossArrow(mode))
                    ShowBossTargetArrow();

                if (IsShowNpcArrow(mode))
                    ShowNpcTargetArrow();

                Timing.RunCoroutine(YieldShowSecondConditionTalk(), TAG);
            }
        }

        private IEnumerator<float> ResetPlayerSpeed()
        {
            yield return Timing.WaitForSeconds(3.0f);
            player.SetForceStatus(ForceStatusType.MazeMoveSpdUpOff);
        }

        /// <summary>
        /// 제니/큐브조각 획득
        /// </summary>
        private async Task GetRewardAsync(IMazeDropItem dropItem)
        {
            if (dropItem is null)
                return;

            dropItem.StartEffect(); // Effect 발생 먼저.

            if (dropItem.RewardType == MazeRewardType.CubePiece)
                PlaySfx(Constants.SFX.Battle.QUEST_COIN);

            var param = Protocol.NewInstance();
            param.PutByte("1", (byte)(dropItem.RewardType == MazeRewardType.Zeny ? 1 : 2));
            param.PutBool("2", true);
            var response = await Protocol.REQUEST_SCENARIO_MAZE_GET_COIN.SendAsync(param);
            if (!response.isSuccess)
                return;

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                ApplyCharUpdateData(charUpdateData);
            }
            if (dropItem.RewardType != MazeRewardType.Zeny)
            {
                UpdateDropCubePiece(droppedCubePiece + 1);

                // 큐브조각을 충분히 모았으면 보스 프로텍트 해제
                if (CheckPieceCondition())
                {
                    SetGuideline(Guideline.Guideline3);

                    ScenarioMazeMode mode = GetCurrentMode();

                    ShowExclamationMark(); // 데비루치 ? 마커 제거, ! 마커 추가                   

                    if (IsShowBossArrow(mode))
                        ShowBossTargetArrow();

                    if (IsShowNpcArrow(mode))
                        ShowNpcTargetArrow();

                    if (mode == ScenarioMazeMode.CrowdControl)
                    {
                        if (IsShowUIMatchMultiMazeStatus())
                            uiMatchMultiMazeStatus.SetState(GetPlayerCID(), View.UIMatchPlayerSlot.State.Clear);
                        Timing.RunCoroutine(YieldEnterBossBattle(bossEntity: null), TAG); // 보스전 입장
                    }
                    else
                    {
                        Timing.RunCoroutine(YieldShowSecondConditionTalk(), TAG);
                    }
                }
            }
        }

        private void ApplyCharUpdateData(CharUpdateData charUpdateData)
        {
            if (charUpdateData.changedZeny.HasValue)
            {
                droppedZeny += charUpdateData.changedZeny.Value;
            }
            if (charUpdateData.zeny.HasValue)
            {
                player.Goods.Update(charUpdateData.zeny, charUpdateData.catCoin, charUpdateData.changedZeny, charUpdateData.guildCoin, charUpdateData.normalQuestCoin, charUpdateData.roPoint, charUpdateData.onBuffPoint);
            }
        }

        /// <summary>
        /// 동전, 큐브 조각 획득 이벤트
        /// </summary>
        private void OnMazeDropItem(IMazeDropItem dropItem)
        {
            if (dropItem == null)
                return;

            switch (dropItem.RewardType)
            {
                case MazeRewardType.Zeny:
                    PlaySfx(Constants.SFX.Battle.ZENY);
                    break;

                case MazeRewardType.RandomBox:
                case MazeRewardType.SpeedItem:
                    PlaySfx(Constants.SFX.Battle.SPEED_POTION);
                    break;

                case MazeRewardType.NormalQuest:
                case MazeRewardType.CubePiece:
                case MazeRewardType.MultiMazeCube:
                    PlaySfx(Constants.SFX.Battle.QUEST_COIN);
                    break;
            }

            if (dropItem.RewardType == MazeRewardType.SpeedItem)
            {
                // 이속포션은 로컬에서 처리한다.
                OnDropSpeedItem(dropItem);
            }
            else
            {
                GetRewardAsync(dropItem).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 플레이어 스폰 이벤트
        /// </summary>
        void OnCharacterSpawn(UnitActor actor)
        {
            actor.AI.OnCollisionActor += OnCollisionActor;
            actor.Movement.OnMazeDropItem += OnMazeDropItem;
        }

        /// <summary>
        /// 플레이어 디스폰 이벤트
        /// </summary>
        void OnCharacterDespawn(UnitActor actor)
        {
            actor.Movement.SetMode(savedMove);
            actor.Movement.SetDefaultSpeed(savedDefaultSpeed);

            actor.AI.OnCollisionActor -= OnCollisionActor;
            actor.Movement.OnMazeDropItem -= OnMazeDropItem;

            actor.Entity.OnSpawnActor -= OnCharacterSpawn;
            actor.Entity.OnDespawnActor -= OnCharacterDespawn;
        }

        /// <summary>
        /// 제니 큐브, 경험치 큐브 디스폰 이벤트
        /// </summary>
        /// <param name="actor"></param>
        void OnCubeDespawn(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnCubeDespawn;
            actor.Entity.OnChangeHP -= OnChangedClickerMonaterHp;
            actor.Entity.OnHit -= OnCubeHit;
        }

        /// <summary>
        /// 클리커 몬스터 HP 변동
        /// </summary>
        void OnChangedClickerMonaterHp(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }


        /// <summary>
        /// 제니 큐브, 경험치 큐브 히트 이벤트
        /// </summary>
        void OnCubeHit(UnitEntity unit, UnitEntity attacker, int value, int count, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int elementFactor)
        {
            UnitActor actor = unit.GetActor();
            if (actor)
                actor.EffectPlayer.GenerateImpulse(BattlePoolManager.ImpulseType.ClickAttack);

            var mode = GetCurrentMode();

            if (mode == ScenarioMazeMode.ExpCube)
            {
                MazeGold particle = battlePool.SpawnMazeExp(unit.LastPosition);
                particle.CachedTransform.localScale = Vector3.one * 0.5f;
                particle.SetJumpHeight(0.5f);
                particle.SetGravity(2f);
                particle.SetSpread(true);
                PlaySfx(Constants.SFX.Battle.EXP);
                IMazeDropItem iDropItem = particle;
                iDropItem.Set(0, MazeRewardType.None, GetWidget(UIExtraBattlePlayerStatus.ExtraMode.Exp));
                iDropItem.StartEffect();
            }
            if (mode == ScenarioMazeMode.ZenyCube)
            {
                MazeGold particle = battlePool.SpawnMazeZenyYellow(unit.LastPosition);
                particle.CachedTransform.localScale = Vector3.one * 0.5f;
                particle.SetJumpHeight(0.35f);
                particle.SetGravity(2f);
                particle.SetSpread(true);
                PlaySfx(Constants.SFX.Battle.ZENY);
                IMazeDropItem iDropItem = particle;
                iDropItem.Set(0, MazeRewardType.None, GetWidget(UIExtraBattlePlayerStatus.ExtraMode.YellowZeny));
                iDropItem.StartEffect();
            }
        }

        /// <summary>
        /// 클리커 몬스터 클릭
        /// </summary>
        void OnClickerMonsterClick()
        {
            if (battleCubeEntity is null)
                return;

            battleCubeEntity.Apply(player, player.battleSkillInfo.basicActiveSkill, null);
        }

        /// <summary>
        /// 사망 팝업 확인버튼 이벤트
        /// </summary>
        private void OnBattleFailConfirm()
        {
            ScenarioMazeMode currnetMode = GetCurrentMode();
            if (currnetMode == ScenarioMazeMode.FirstTutorial || currnetMode == ScenarioMazeMode.SecondTutorial)
            {
                foreach (var unit in unitList)
                {
                    var actor = unit.GetActor();

                    if (!unit.IsEnemy || actor is null)
                        continue;

                    actor.AI.ChangeState(AI.Transition.Finished);
                }

                impl.ResetUnitList();
                impl.StartBattle(BattleMode.ScenarioMaze, currentData.id); // 현재 미로 재시작
                return;
            }

            ExitScenarioMazeEntry();
        }

        /// <summary>
        /// 나가기 버튼 이벤트
        /// </summary>
        private void OnBattleExit()
        {
            // 데비루치와 대화중에는 백버튼 막기
            if (uiDialogue.IsVisible)
                return;

            ExitScenarioMazeEntry();
        }

        private void ExitScenarioMazeEntry()
        {
            player.ResetExtraOption(ExtraBattleOptionType.MaxHp);

            impl.StartBattle(BattleMode.MultiMazeLobby);
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

            // 얼음 상태이면
            if (actor.AI.CurrentState == AI.StateID.Defenseless)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();
        }


        ////////////// 유틸 //////////////

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

        private void RequestBattleEnd()
        {
            if (isRequestBattleEnd)
                return;

            isRequestBattleEnd = true;
            dungeonModel.RequestScenarioMazeBossBattleEnd(clientMonsterId: 0, randomKey: 0).WrapNetworkErrors();
        }

        void OnMonsterItemDrop(UnitEntity unit, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            if (!isRequestBattleEnd)
                return;

            if (droppedZeny > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.Zeny), droppedZeny, rewardCount: 0, rewardOption: 0));

            if (droppedLevelExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.LevelExp), droppedLevelExp, rewardCount: 0, rewardOption: 0));

            if (droppedJobExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.JobExp), droppedJobExp, rewardCount: 0, rewardOption: 0));

            if (rewards != null)
                rewardBuffer.AddRange(rewards);

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.SCENARIO_MAZE_ID_CLEAR_COUNT, currentData.id); // 특정 시나리오 미로 클리어           

            dungeonModel.AddCleardScenarioMazeId(currentData.id);

            isMonsterItemDrop = true;
            ShowResultClear();
        }

        /// <summary>
        /// 클리어 UI 띄우기
        /// </summary>
        private void ShowResultClear()
        {
            ShowResultClearAsync().WrapNetworkErrors();
        }

        async Task ShowResultClearAsync()
        {
            if (isMonsterItemDrop)
            {
                var buffer = rewardBuffer.GetBuffer(isAutoRelease: true);

                // 카드 보상 있을 경우 특별 연출
                if (buffer.Length > 0)
                {
                    // 카드 보상 찾기
                    List<CardItemInfo.ICardInfoSimple> rewardList = new List<CardItemInfo.ICardInfoSimple>();
                    foreach (var reward in buffer)
                    {
                        if (reward.ItemGroupType != ItemGroupType.Card)
                            continue;

                        CardItemInfo param = new CardItemInfo();
                        param.SetData(reward.ItemData);
                        if (!param.IsInvalidData)
                            rewardList.Add(param);
                    }

                    // 연출 재생
                    if (rewardList.Count > 0)
                    {
                        UICardReward uiCardReward = UI.Show<UICardReward>();
                        uiCardReward.Show(rewardList.ToArray());
                        await Awaiters.Until(() => uiCardReward == null || !uiCardReward.IsVisible);
                    }
                }

                if (buffer.Length > 0)
                    uiResultClear.Show(buffer, default, false, showRewardFX: true);
                else
                    uiResultClear.Show(LocalizeKey._17602.ToText(), default, false, showRewardFX: true);
            }

            if (isCheckOpenContent)
            {
                ScenarioMazeMode mode = GetCurrentMode();
                ContentType openContentType = mode.GetOpenContentType();
                if (!openContentType.IsContentsUnlockViaTutorial())
                    UI.Show<UIContentsUnlock>().Set(openContentType);
            }
        }

        void OnFinishDungeon()
        {
            ExitScenarioMazeEntry();
        }

        /// <summary>
        /// 현재 시나리오 미로 모드 반환
        /// </summary>
        private ScenarioMazeMode GetCurrentMode()
        {
            return currentData.scenario_maze_type.ToEnum<ScenarioMazeMode>();
        }

        /// <summary>
        /// 필요한 조각 개수 반환
        /// </summary>
        /// <returns></returns>
        private int GetNeedPiece()
        {
            return currentData.boss_condition_value;
        }

        /// <summary>
        /// 수집할 조각 타입 반환
        /// </summary>
        private UIExtraBattlePlayerStatus.ExtraMode GetCurrentExtraMode()
        {
            var mode = GetCurrentMode();

            switch (mode)
            {
                case ScenarioMazeMode.QuestCoin:
                case ScenarioMazeMode.FirstTutorial:
                case ScenarioMazeMode.SecondTutorial:
                    return UIExtraBattlePlayerStatus.ExtraMode.CubePiece;
                case ScenarioMazeMode.ZenyCube:
                    return UIExtraBattlePlayerStatus.ExtraMode.YellowZeny;
                case ScenarioMazeMode.ExpCube:
                    return UIExtraBattlePlayerStatus.ExtraMode.Exp;
                case ScenarioMazeMode.SpeedItem:
                    return UIExtraBattlePlayerStatus.ExtraMode.SpeedItem;
            }

            return UIExtraBattlePlayerStatus.ExtraMode.None;
        }

        private string GetSecondConditionNpcTalk()
        {
            const int _214991 = 214991; // 보스와 전투가 가능해졌어!
            const int _214992 = 214992; // -
            const int _214993 = 214993; // 모은 제니를 들고 데비군에게 오도록!
            const int _214994 = 214994; // 모은 경험치를 들고 데비군에게 오도록!
            const int _214995 = 214995; // 데비군에게 빨리 돌아오도록!
            const int _214996 = 214996; // -
            const int _214997 = 214997; // 보스와 전투가 가능해졌어!

            ScenarioMazeMode mode = GetCurrentMode();
            switch (mode)
            {
                case ScenarioMazeMode.QuestCoin: return _214991.ToText();
                case ScenarioMazeMode.Dialog: return _214992.ToText();
                case ScenarioMazeMode.ZenyCube: return _214993.ToText();
                case ScenarioMazeMode.ExpCube: return _214994.ToText();
                case ScenarioMazeMode.SpeedItem: return _214995.ToText();
                case ScenarioMazeMode.FirstTutorial: return _214996.ToText();
                case ScenarioMazeMode.SecondTutorial: return _214997.ToText();
            }
            return string.Empty;
        }

        /// <summary>
        /// 대사 출력
        /// </summary>
        private IEnumerator<float> YieldDialog(Npc npc, string dialog)
        {
            return YieldDialog(npc.imageName, npc.nameLocalKey.ToText(), dialog);
        }

        /// <summary>
        /// 대사 출력
        /// </summary>
        private IEnumerator<float> YieldPlayerDialog(string dialog)
        {
            Npc npc = characterModel.Gender == Gender.Male ? Npc.NOVICE_MALE : Npc.NOVICE_FEMALE;
            return YieldDialog(npc.imageName, characterModel.Name, dialog);
        }

        /// <summary>
        /// 대사 출력
        /// </summary>
        private IEnumerator<float> YieldDialog(string spriteName, string name, string dialog)
        {
            uiDialogue.Show();
            uiDialogue.SetActiveBtnSkip(true);
            string[] dialogs = dialog.Split(Constants.NPC.NPC_DIALOG_DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);
            yield return uiDialogue.UntilShow(spriteName, name, dialogs);
            uiDialogue.Hide();
        }

        /// <summary>
        /// 가이드라인 세팅
        /// </summary>
        private void SetGuideline(Guideline guideline)
        {
            NGUITools.SetActive(goGuideline1, guideline == Guideline.Guideline1);
            NGUITools.SetActive(goGuideline2, guideline == Guideline.Guideline2);
            NGUITools.SetActive(goGuideline3, guideline == Guideline.Guideline3);
        }

        private void SetActiveTempMazeCoin(bool isActive)
        {
            NGUITools.SetActive(tempMazeCoin, isActive);
        }

        private bool IsHideNpc(ScenarioMazeMode mode)
        {
            switch (mode)
            {
                case ScenarioMazeMode.Dialog:
                case ScenarioMazeMode.ZenyCube:
                case ScenarioMazeMode.ExpCube:
                case ScenarioMazeMode.SpeedItem:
                case ScenarioMazeMode.FirstTutorial:
                    return false;

                case ScenarioMazeMode.QuestCoin:
                case ScenarioMazeMode.SecondTutorial:
                case ScenarioMazeMode.CrowdControl:
                    return true;
            }

            return false;
        }

        private bool IsSpawnBossGhost(ScenarioMazeMode mode)
        {
            switch (mode)
            {
                case ScenarioMazeMode.QuestCoin:
                case ScenarioMazeMode.ZenyCube:
                case ScenarioMazeMode.ExpCube:
                case ScenarioMazeMode.SpeedItem:
                case ScenarioMazeMode.SecondTutorial:
                    return true;

                case ScenarioMazeMode.Dialog:
                case ScenarioMazeMode.FirstTutorial:
                case ScenarioMazeMode.CrowdControl:
                    return false;
            }

            return false;
        }

        private bool IsSpawnNormalGhost(ScenarioMazeMode mode)
        {
            switch (mode)
            {
                case ScenarioMazeMode.QuestCoin:
                case ScenarioMazeMode.FirstTutorial:
                case ScenarioMazeMode.SecondTutorial:
                case ScenarioMazeMode.CrowdControl:
                    return true;

                case ScenarioMazeMode.ZenyCube:
                case ScenarioMazeMode.ExpCube:
                case ScenarioMazeMode.SpeedItem:
                case ScenarioMazeMode.Dialog:
                    return false;
            }

            return false;
        }

        private bool IsShowBossArrow(ScenarioMazeMode mode)
        {
            switch (mode)
            {
                case ScenarioMazeMode.QuestCoin:
                case ScenarioMazeMode.SecondTutorial:
                    return true;
            }

            return false;
        }

        private bool IsShowNpcArrow(ScenarioMazeMode mode)
        {
            switch (mode)
            {
                case ScenarioMazeMode.ZenyCube:
                case ScenarioMazeMode.ExpCube:
                case ScenarioMazeMode.SpeedItem:
                case ScenarioMazeMode.FirstTutorial:
                    return true;
            }

            return false;
        }

        private UIWidget GetWidget(UIMainTop.MenuContent content)
        {
            return uiMainTop.GetWidget(content);
        }

        private UIWidget GetWidget(UIExtraBattlePlayerStatus.ExtraMode extraMode)
        {
            return uiBattlePlayerStatus.GetWidget(extraMode);
        }

        private void UpdateDropCubePiece(int newDropCubePiece)
        {
            droppedCubePiece = newDropCubePiece;
            uiBattlePlayerStatus.SetExtraCount(UIExtraBattlePlayerStatus.ExtraMode.CubePiece, droppedCubePiece, GetNeedPiece());
            if (IsShowUIMatchMultiMazeStatus())
            {
                uiMatchMultiMazeStatus.SetCoin(GetPlayerCID(), droppedCubePiece);
                uiMatchMultiMazeStatus.SetRemainCoin(currentData.boss_condition_value - droppedCubePiece, currentData.boss_condition_value);
            }
        }

        private bool IsShowUIMatchMultiMazeStatus() => (GetCurrentMode() == ScenarioMazeMode.CrowdControl);

        private int GetPlayerCID() => player.Character.Cid;

#if UNITY_EDITOR
        void OnSuccessKonamiCommand()
        {
            for (int i = 0; i < poolObjectList.Count; i++)
            {
                if (poolObjectList[i] is IMazeDropItem mazeDropItem)
                {
                    if (mazeDropItem.RewardType == MazeRewardType.CubePiece)
                    {
                        poolObjectList[i].CachedTransform.position = player.LastPosition;
                    }
                }
            }
        }
#endif
    }
}