using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class TamingMazeEntry : BattleEntry
    {
        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;

        private const float SENSE_DISTANCE = 3f;
        private const float CAMERA_NOISE_FOCUS_ENEMY = 2f;
        private const float MONSTR_PATROL_RANKGE = 2;

        private const float PRE_GAMEOVER_NOTICE_TIME = 5000f; // 미로 종료 5초 전에 미리 알림
        private const float POPUP_TIMEOUT = 5f;

        /// <summary>
        /// 기본 속도 (이속, 공속)
        /// </summary>
        private const int DEFAULT_SPEED = 10000;

        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater50_5;

        /// <summary>
        /// 테이밍 결과
        /// </summary>
        public enum TamingResult
        {
            Fail = 0,           // 실패
            Success = 1,        // 성공
            GreatSuccess = 2,   // 대성공
        }

        /******************** Models ********************/
        private readonly DungeonModel dungeonModel;
        private readonly GuildModel guildModel;

        /******************** Repositories ********************/
        private readonly TamingDataManager tamingDataRepo;

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UIController uiController;
        private UIEnemyFocus uiEnemyFocus;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UICamSelect uiCamSelect;
        private UIFeedUse uiFeedUse;
        private UIMvpTimer uiMvpTimer;
        private UIFeedView uiFeedView;
        private UIBattleTaming uiBattleTaming;

        /******************** Scene Compositions ********************/
        private readonly IBattlePool battlePool;
        private readonly List<PoolObject> poolObjectList;
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly TamingMonsterBotEntityPool monsterBotPool;

        /******************** Temp Data ********************/
        private Map map;
        private TamingData currentTamingData;
        private Vector3 savedPlayerPosition;
        private int sendMonsterIndex;
        private bool isTamingReady;
        private bool isGameOver;
        private int guildCoin; // 획득한 길드 코인 수량
        private int cupetPiece; // 길드에 기여한 큐펫 조각 수량

        // 테이밍 시도
        private bool isThrowFeedItem; // 먹이를 던졌는지 안 던졌는지 여부
        private TamingResult tamingResult; // 테이밍 결과 (실패/성공/대성공)
        private RewardData guildReward; // 길드 보상 (큐펫 조각)
        private RewardData myReward; // 개인 보상 (길드 코인)

        public TamingMazeEntry() : base(BattleMode.TamingMaze)
        {
            player.SetState(UnitEntity.UnitState.Maze);

            dungeonModel = player.Dungeon;
            guildModel = player.Guild;

            tamingDataRepo = TamingDataManager.Instance;
            battlePool = BattlePoolManager.Instance;
            poolObjectList = new List<PoolObject>();
            playerBotPool = new PlayerBotEntityPool();
            monsterBotPool = new TamingMonsterBotEntityPool();
            sendMonsterIndex = -1;
        }

        public override IEnumerator<float> YieldExitBattle()
        {
            Task<Response> task = Protocol.REQUEST_TAMING_MAZE_ROOM_EXIT.SendAsync();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessExit = response.isSuccess;

            if (!IsSuccessExit)
                response.ShowResultCode();
        }

        protected override void Dispose()
        {
            base.Dispose();

            map = null;

            // 오브젝트 풀 폐기
            for (int i = poolObjectList.Count - 1; i >= 0; i--)
            {
                poolObjectList[i].Release();
            }
            poolObjectList.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            player.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            // 플레이어 봇 폐기
            foreach (var item in playerBotPool)
            {
                Dispose(item);
            }

            // 몬스터 봇 폐기
            foreach (var item in monsterBotPool)
            {
                Dispose(item);
            }

            playerBotPool.Clear();
            monsterBotPool.Clear();
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();

            // 다른 플레이어 입장 시 정보 이벤트 추가
            Protocol.RECEIVE_TAMING_MAZE_ROOM_JOIN.AddEvent(OnReceiveTamingDungeonJoin);

            // 다른 플레이어의 이동 수신 이벤트 추가.
            Protocol.REQUEST_TAMING_MAZE_ROOM_TRANSFORM.AddEvent(OnRequestTamingDungeonTransform);

            // 다른 플레이어의 퇴장 수신 이벤트 추가
            Protocol.RECEIVE_TAMING_MAZE_ROOM_EXIT.AddEvent(OnReceiveTamingDungeonExit);

            // 몬스터 상태 변경 이벤트 추가
            Protocol.RECEIVE_TAMING_MONSTER_UPDATE.AddEvent(OnUpdateMonsterState);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();

            // 다른 플레이어 입장 시 정보 이벤트 제거
            Protocol.RECEIVE_TAMING_MAZE_ROOM_JOIN.RemoveEvent(OnReceiveTamingDungeonJoin);

            // 다른 플레이어의 이동 수신 이벤트 제거
            Protocol.REQUEST_TAMING_MAZE_ROOM_TRANSFORM.RemoveEvent(OnRequestTamingDungeonTransform);

            // 다른 플레이어의 퇴장 수신 이벤트 제거
            Protocol.RECEIVE_TAMING_MAZE_ROOM_EXIT.RemoveEvent(OnReceiveTamingDungeonExit);

            // 몬스터 상태 변경 이벤트 제거
            Protocol.RECEIVE_TAMING_MONSTER_UPDATE.RemoveEvent(OnUpdateMonsterState);
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnSpawnActor += OnPlayerSpawn;
            player.OnDespawnActor += OnPlayerDespawn;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnSpawnActor -= OnPlayerSpawn;
            player.OnDespawnActor -= OnPlayerDespawn;
        }

        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiController = UI.Show<UIController>();
            uiEnemyFocus = UI.Show<UIEnemyFocus>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiFeedUse = UI.Show<UIFeedUse>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiFeedView = UI.Show<UIFeedView>();
            uiBattleTaming = UI.Show<UIBattleTaming>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += ExitEntry;
            uiFeedUse.OnUseFeed += OnUseFeed;
            uiMvpTimer.OnFinish += OnTimeOver;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiEnemyFocus.Hide();
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiFeedUse.Hide();
            uiBattleTaming.Set(guildCoin: 0, cupetPice: 0);

            cameraController.SetView(DEFAULT_VIEW);
            cameraController.AddMask(Layer.MAZE_OTHER_PLAYER, Layer.MAZE_ENEMY);
        }

        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= ExitEntry;
            uiFeedUse.OnUseFeed -= OnUseFeed;
            uiMvpTimer.OnFinish -= OnTimeOver;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIController>();
            UI.Close<UIEnemyFocus>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UICamSelect>();
            UI.Close<UIFeedUse>();
            UI.Close<UIMvpTimer>();
            UI.Close<UITamingResultPopup>();
            UI.Close<UIFeedView>();
            UI.Close<UIBattleTaming>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiController = null;
            uiEnemyFocus = null;
            uiBattlePlayerStatus = null;
            uiCamSelect = null;
            uiFeedUse = null;
            uiMvpTimer = null;
            uiFeedView = null;
            uiBattleTaming = null;
        }

        protected override void OnLocalize()
        {
            string mainText = currentTamingData.name_id.ToText();
            uiBattleInfo.Set(mainText);
        }

        public override void EndBattle()
        {
            base.EndBattle();
        }

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            currentTamingData = tamingDataRepo.Get(id);

            if (currentTamingData == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            if (!player.Guild.HaveGuild || !player.Guild.IsTamingMazeInProgress)
            {
                IsSuccessEnter = false;
                yield break;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id);
            Task<Response> task = Protocol.REQUEST_TAMING_MAZE_ROOM_JOIN.SendAsync(sfs);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;

            IsSuccessEnter = response.isSuccess;

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                yield break;
            }
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = currentTamingData.scene_name; // 씬 이름
            string bgmName = currentTamingData.bgm; // 배경음 이름
            bool isChangeScene = true;
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                if (map is null)
                    map = item.GetComponent<Map>();
            }
        }

        protected override void OnReady()
        {
            base.OnReady();

            uiBattlePlayerStatus.SetPlayer(player);

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            float mazeRemainTime = player.Guild.TamingMazeRemainTime.ToRemainTime();
            uiMvpTimer.Initialize((long)mazeRemainTime);
            uiMvpTimer.RestartTimer();

            uiFeedView.SetItem(currentTamingData.use_item_id);

            isGameOver = false;

            Timing.RunCoroutine(YieldTimer(mazeRemainTime - PRE_GAMEOVER_NOTICE_TIME), TAG); // 미로 종료 미리 알림 타이머
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator<float> YieldComposeScene()
        {
            // 풀숲 생성           
            yield return Timing.WaitUntilDone(YieldSpawnGrass(), TAG);
            // 플레이어 생성
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);
            // 몬스터 생성
            yield return Timing.WaitUntilDone(YieldSpawnMonsterBots(), TAG);
        }

        protected override void OnAllReady()
        {
            base.OnAllReady();

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG); // 다른 플레이어 생성

            RefreshState();
        }

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        /// <summary>
        /// 테이밍 던전 시간 종료
        /// </summary>
        void RefreshState()
        {
            float mazeRemainTime = player.Guild.TamingMazeRemainTime.ToRemainTime();

            if (mazeRemainTime <= 0)
            {
                OnTimeOver();
            }
        }

        #region 생성

        /// <summary>
        /// 풀숲 생성
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldSpawnGrass()
        {
            foreach (TamingSpawnZone zone in map.GetTamingSpawnZones())
            {
                PoolObject grass = battlePool.SpawnGrass(zone.GetCenter(), currentTamingData.prefab_name);
                AddPoolObject(grass);
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
            player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

            if (impl.Add(player, isEnemy: false))
            {
                UnitActor unitActor = player.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                unitActor.Movement.SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED);
                unitActor.Radar.SetSenseDistance(SENSE_DISTANCE);
                unitActor.Radar.SetSenseTargetType(TargetType.Enemy);

                savedPlayerPosition = FindClosestEdge(savedPlayerPosition); // 씬 로드 후에 네비 위치로 변경
                unitActor.AI.SetHomePosition(savedPlayerPosition, isWarp: false); // Home 세팅
                unitActor.Movement.ForceWarp(savedPlayerPosition); // Home으로 강제 워프

                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
                yield return Timing.WaitForOneFrame;
            }
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
            entity.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
            entity.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

            if (!impl.Add(entity, isEnemy: true))
                return false;

            UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
            unitActor.Movement.SetMode(UnitMovement.Mode.NavMesh);
            unitActor.Movement.SetDefaultSpeed(Constants.Battle.MAZE_MOVE_SPEED);

            Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);
            Vector3 position = FindClosestEdge(pos);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프
            unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출
            return true;
        }

        /// <summary>
        /// 몬스터 봇 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonsterBots()
        {
            while (monsterBotPool.HasQueue())
            {
                ITamingMonsterPotInput input = monsterBotPool.Dequeue();

                if (SpawnMonsterBots(input))
                    yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 몬스터 봇 생성
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool SpawnMonsterBots(ITamingMonsterPotInput input)
        {
            MonsterBotEntity entity = monsterBotPool.Create(input);

            if (!impl.Add(entity, isEnemy: true))
                return false;

            if (map.TryGetSpawnZone(input.Index, out TamingSpawnZone zone))
            {
                UnitActor unitActor = entity.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetMode(UnitMovement.Mode.NavMesh);

                Vector3 pos = zone.GetCenter();
                unitActor.AI.SetPatrolRange(MONSTR_PATROL_RANKGE);
                unitActor.AI.SetHomePosition(pos, isWarp: false); // Home 세팅
                unitActor.Movement.ForceWarp(pos); // Home으로 강제 워프                 
                unitActor.AI.ReadyToBattle();

                // 상태 변경
                TamingMonsterState state = input.State.ToEnum<TamingMonsterState>();
                switch (state)
                {
                    case TamingMonsterState.Create:
                        unitActor.AI.ChangeState(AI.Transition.MoveAround); // 순회
                        break;

                    case TamingMonsterState.Ready:
                        unitActor.AI.ChangeState(AI.Transition.EndBattle); // 
                        break;

                    case TamingMonsterState.Delete: // 죽은 상태
                        Dispose(entity);
                        monsterBotPool.Recycle(entity);
                        break;
                }
            }
            return true;
        }

        private IEnumerator<float> YieldTimer(float timer)
        {
            RemainTime remainTime = timer;

            yield return Timing.WaitUntilTrue(() => remainTime.ToRemainTime() <= 0);

            UI.ShowToastPopup(LocalizeKey._90218.ToText()); // 5초 뒤 테이밍 미로가 종료됩니다.
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

            if (mainCamera == null)
                return;

            if (player.IsDie)
                return;

            if (player.battleCrowdControlInfo.GetCannotMove())
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            // 테이밍 진행중
            if (isTamingReady)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();

            unitActor.Movement.Move(motion);

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

        protected override void ExitEntry()
        {
            // 길드가 존재하지 않을 경우
            if (!guildModel.HaveGuild)
            {
                base.ExitEntry(); // 스테이지로 입장
            }
            else
            {
                impl.StartBattle(BattleMode.GuildLobby);
            }
        }

        /// <summary>
        /// 테이밍 시작
        /// </summary>
        private void OnUseFeed()
        {
            SendTamingMonster();
        }

        /// <summary>
        /// 시간 초과
        /// </summary>
        private void OnTimeOver()
        {
            OnResetController();
            uiController.Hide();
            uiMvpTimer.StopTimer();

            isGameOver = true;
            UI.ConfirmPopup(LocalizeKey._90215.ToText(), ExitEntry, timeout: POPUP_TIMEOUT); // 테이밍 미로가 종료되었습니다. (5초)
        }

        #endregion      

        #region 플레이어 이벤트

        /// <summary>
        /// 플레이어 생성 이벤트
        /// </summary>
        void OnPlayerSpawn(UnitActor unitActor)
        {
            unitActor.Radar.OnTarget += OnTarget;
        }

        /// <summary>
        /// 플레이어 제거 이벤트
        /// </summary>
        void OnPlayerDespawn(UnitActor unitActor)
        {
            unitActor.Radar.OnTarget -= OnTarget;
        }

        /// <summary>
        /// 테이밍 몬스터 충돌
        /// </summary>
        /// <param name="unitActor"></param>
        private async void OnTarget(UnitActor unitActor)
        {
            if (isGameOver)
                return;
            if (unitActor == null)
                return;

            if (unitActor.AI.CurrentState != AI.StateID.Patrol)
                return;

            if (isTamingReady) // 테이밍 중일 때 중복 충돌 방지
            {
                return;
            }

            if (unitActor.Entity is MonsterBotEntity monsterBotEntity)
            {
                // 먹이 없는 상황 예외 처리
                if (!HasFeedItem())
                {
                    UI.ShowToastPopup(LocalizeKey._90216.ToText()); // 먹이가 없습니다.
                    return;
                }

                isTamingReady = true;
                sendMonsterIndex = monsterBotEntity.BotServerIndex;
                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", monsterBotEntity.BotServerIndex);
                var response = await Protocol.REQUEST_TAMING_MONSTER_READY.SendAsync(sfs);
                if (!response.isSuccess)
                {
                    isTamingReady = false;
                    sendMonsterIndex = -1;

                    ResetFocusFX(); // 카메라 줌 아웃
                    ResetController();
                    return;
                }
            }
        }

        #endregion       

        #region 몬스터 상태 업데이트

        void UpdateMonsterCreate(ITamingMonsterPotInput input)
        {
            // 생성 도중이라면 생성큐 업데이트
            if (monsterBotPool.HasQueue())
            {
                monsterBotPool.Enqueue(input);
                return;
            }

            // 즉시 생성
            if (SpawnMonsterBots(input))
                return;

#if UNITY_EDITOR
            Debug.LogError($"생성하지 못한 몬스터: {nameof(input.Index)} = {input.Index}");
#endif
        }

        void UpdateMonsterReady(ITamingMonsterPotInput input)
        {
            MonsterBotEntity find = monsterBotPool.Find(input.Index);

            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueState(input.Index, input.State))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터가 테이밍 진행중 상태로 변경: {nameof(input.Index)} = {input.Index}");
#endif
                return;
            }

            if (sendMonsterIndex == input.Index)
            {
                isThrowFeedItem = false;
                Timing.WaitUntilDone(ShowEnemyFocusFX(find.GetActor()));
            }

            find.GetActor().AI.ChangeState(AI.Transition.EndBattle);
        }

        void UpdateMonsterDelete(ITamingMonsterPotInput input)
        {
            MonsterBotEntity find = monsterBotPool.Find(input.Index);

            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (monsterBotPool.HasQueue() && monsterBotPool.UpdateQueueDelete(input.Index))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 몬스터 제거: {nameof(input.Index)} = {input.Index}");
#endif
                return;
            }

            Debug.Log($"테이밍 몬스터 제거 sendMonsterIndex={sendMonsterIndex}, input.Index={input.Index}");
            if (sendMonsterIndex == input.Index)
            {
                if (isThrowFeedItem) // 먹이를 던졌다면
                {
                    Timing.RunCoroutine(YieldTamingAnimation(find), TAG);
                    return;
                }
                else // 시간 초과인 경우
                {
                    isTamingReady = false;
                    sendMonsterIndex = -1;

                    Debug.Log($"몬스터가 도망갔습니다 sendMonsterIndex={sendMonsterIndex}, input.Index={input.Index}");
                    UI.ShowToastPopup(LocalizeKey._90217.ToText()); // 몬스터가 도망갔습니다.
                    ResetFocusFX();
                    ResetController();
                }
            }

            Dispose(find); // 제거
            monsterBotPool.Recycle(find); // 회수
        }

        #endregion

        #region 서버 응답 이벤트

        /// <summary>
        /// 테이밍 던전 다른 플레이어 입장
        /// </summary>
        /// <param name="response"></param>
        void OnReceiveTamingDungeonJoin(Response response)
        {
            if (response.ContainsKey("1"))
            {
                IMultiPlayerInput multiPlayer = response.GetPacket<MazePlayerPacket>("1");

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

            if (response.ContainsKey("2"))
            {
                // 플레이어 위치
                savedPlayerPosition = new Vector3(response.GetInt("4") * 0.001f, UnitMovement.POSITION_Y, response.GetInt("5") * 0.001f);

                // 다른 플레이어 정보
                if (response.ContainsKey("3"))
                {
                    playerBotPool.EnqueueRange(response.GetPacketArray<MazePlayerPacket>("3"));
                }

                // 테이밍 몬스터 정보
                if (response.ContainsKey("6"))
                {
                    monsterBotPool.EnqueueRange(response.GetPacketArray<TamingMonsterInfoPacket>("6"));
                }
            }
        }

        /// <summary>
        /// 테이밍 던전 다른 플레이어 나감
        /// </summary>
        /// <param name="response"></param>
        void OnReceiveTamingDungeonExit(Response response)
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

            find.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            find.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            Dispose(find); // 제거
            playerBotPool.Recycle(find); // 회수
        }

        /// <summary>
        /// 캐릭터 이동 동기화
        /// </summary>
        /// <param name="response"></param>
        void OnRequestTamingDungeonTransform(Response response)
        {
            int cid = response.GetInt("1");
            int[] arrayPosValue = response.GetIntArray("2");

            // 방어코드
            if (arrayPosValue == null)
                return;

            bool isPlayer = cid == player.Character.Cid;
            Vector3 position = new Vector3(arrayPosValue[0] * 0.001f, UnitMovement.POSITION_Y, arrayPosValue[1] * 0.001f);

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
        }

        /// <summary>
        /// 몬스터 상태 변경
        /// </summary>
        /// <param name="response"></param>
        void OnUpdateMonsterState(Response response)
        {
            ITamingMonsterPotInput input = response.GetPacket<TamingMonsterInfoPacket>();
            TamingMonsterState state = input.State.ToEnum<TamingMonsterState>();
            switch (state)
            {
                case TamingMonsterState.Create: // 생성
                    Debug.Log($"몬스터 생성 Index={input.Index}");
                    UpdateMonsterCreate(input);
                    break;

                case TamingMonsterState.Ready: // 테이밍 대기
                    Debug.Log($"몬스터 레디 Index={input.Index}");
                    UpdateMonsterReady(input);
                    break;

                case TamingMonsterState.Delete: // 제거
                    Debug.Log($"몬스터 제거 Index={input.Index}");
                    UpdateMonsterDelete(input);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 현재 위치 서버로 보내기
        /// </summary>
        private void SendPlayerCurrentPosition()
        {
            savedPlayerPosition = player.LastPosition;

            int[] posArray = { (int)savedPlayerPosition.x * 1000, (int)savedPlayerPosition.z * 1000 };
            var sfs = Protocol.NewInstance();
            sfs.PutIntArray("2", posArray);
            Protocol.REQUEST_TAMING_MAZE_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 테이밍 요청
        /// </summary>
        private async void SendTamingMonster()
        {
            Debug.Log($"테이밍 시도 Index={sendMonsterIndex}");

            if (sendMonsterIndex == -1)
            {
                Debug.LogError($"없는 몬스터 테이밍 시도 Index={sendMonsterIndex}");
                isTamingReady = false;
                UI.ShowToastPopup(LocalizeKey._90217.ToText()); // 몬스터가 도망갔습니다.
                ResetFocusFX();
                ResetController();
                return;
            }

            isThrowFeedItem = true; // 테이밍 시도했음을 기록 (시간초과로 인한 몬스터 제거인지, 테이밍 시도로 인한 몬스터 제거인지 구분에 사용)

            long originGuildCoin = player.Goods.GuildCoin; // 테이밍 이전 길드코인 저장
            var response = await dungeonModel.RequestTamingStart(); // 테이밍 요청
            if (!response.isSuccess)
            {
                isThrowFeedItem = false;
                ResetFocusFX(); // 카메라 줌 아웃
                ResetController();

                isTamingReady = false;
                sendMonsterIndex = -1;
                return;
            }

            // 테이밍 결과
            byte result = response.GetByte("3"); // 0:실패, 1:성공, 2:대성공
            tamingResult = result.ToEnum<TamingResult>();

            // 길드 보상
            int drop_item_id = response.GetInt("1"); // 얻은 큐펫조각 (길드보상)
            int count = response.GetInt("2");
            guildReward = count > 0 ? new RewardData(RewardType.Item, drop_item_id, count) : null;

            // 길드가 있을경우에만 획득 카운드 증가 테이밍 도중 강퇴 될수 있음
            if (guildModel.HaveGuild && guildReward != null)
            {
                cupetPiece += count;
            }

            // 개인 보상
            var cud = response.GetPacket<CharUpdateData>("cud");
            int changedGuildCoin = (int)(cud.guildCoin.HasValue ? cud.guildCoin.Value - originGuildCoin : 0);
            myReward = new RewardData(RewardType.GuildCoin, changedGuildCoin, default);
            guildCoin += changedGuildCoin;
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

        private IEnumerator<float> ShowEnemyFocusFX(UnitActor unitActor)
        {
            OnResetController();
            uiController.Hide();
            cameraController.SetView(DEFAULT_VIEW);
            if (unitActor != null)
            {
                unitActor.AI.ChangeState(AI.Transition.EndBattle);
            }

            // 서로 마주보게 하기
            var playerActor = player.GetActor();
            playerActor.CachedTransform.LookAt(unitActor.CachedTransform);
            unitActor.CachedTransform.LookAt(playerActor.CachedTransform);

            yield return Timing.WaitForOneFrame;

            uiEnemyFocus.Show(showNeedleFX: false, showSpreadFX: true);
            cameraController.SetView(CameraController.View.Quater50_17);
            cameraController.SetCameraNoise(CAMERA_NOISE_FOCUS_ENEMY);
            CameraUtils.Zoom(CameraZoomType.ZoomIn);

            yield return Timing.WaitForSeconds(0.8f);

            var itemData = ItemDataManager.Instance.Get(currentTamingData.use_item_id);
            uiFeedUse.Show();
            uiFeedUse.SetIcon(itemData.icon_name);
            uiFeedView.Hide();
            cameraController.SetCameraNoise();
            uiEnemyFocus.Hide();
            //uiController.Show();
            UI.ShowToastPopup(LocalizeKey._90156.ToText()); // 위로 드래그 시 테이밍 시작합니다.
        }

        void ResetFocusFX()
        {
            uiFeedUse.Hide();
            uiFeedView.Show();
            uiEnemyFocus.Hide();
            cameraController.SetView(DEFAULT_VIEW);
        }

        void ResetController()
        {
            OnResetController();
            uiController.Show();
        }

        /// <summary>
        /// 먹이를 주는 연출 (약 4초)
        /// </summary>
        private IEnumerator<float> YieldTamingAnimation(MonsterBotEntity monster)
        {
            Debug.Log($"테이밍 결과={tamingResult}");
            uiFeedUse.Hide();

            yield return Timing.WaitForSeconds(0.25f);

            // 플레이어 먹이 주는 애니메이션 재생
            var animator = player.GetActor().Animator as CharacterAnimator;
            animator.PlayThrowStone();

            yield return Timing.WaitForSeconds(0.75f);

            // 몬스터 이펙트 재생
            var actor = monster.GetActor();
            IBattlePool battlePool = BattlePoolManager.Instance;
            var healEffect = battlePool.SpawnHeal(actor.CachedTransform);

            yield return Timing.WaitForSeconds(1.5f);

            if (tamingResult != TamingResult.Fail)
            {
                healEffect.Release();

                // 몬스터 아이콘 미리 저장
                MonsterData monData = MonsterDataManager.Instance.Get(monster.Monster.MonsterID);
                string monsterIconName = monData.icon_name;

                Debug.Log($"테이밍 결과 후 몬스터 제거 Index={sendMonsterIndex}");
                Dispose(monster); // 제거
                monsterBotPool.Recycle(monster); // 회수

                yield return Timing.WaitForSeconds(0.25f);

                ResetFocusFX(); // 카메라 줌 아웃

                yield return Timing.WaitForSeconds(0.75f);

                // 팝업
                int guildContributionPoint = BasisType.GUILD_CONTRIBUTION_POINT.GetInt(5);
                UI.Show<UITamingResultPopup>().Show(tamingResult, monsterIconName, guildReward, myReward, guildContributionPoint);
                ResetController();

                // 획득한 길드코인, 큐펫 조각 갱신
                uiBattleTaming.Set(guildCoin, cupetPiece);

                isTamingReady = false;
                sendMonsterIndex = -1;
            }
            else
            {
                // battlePool.SpawnTamingFail(actor.CachedTransform); // TODO: 실패 이펙트 발생
                UI.ShowToastPopup(LocalizeKey._90217.ToText()); // 몬스터가 도망갔습니다.

                healEffect.Release();
                Debug.Log($"테이밍 결과 후 몬스터 제거 Index={sendMonsterIndex}");
                Dispose(monster); // 제거
                monsterBotPool.Recycle(monster); // 회수

                ResetFocusFX(); // 카메라 줌 아웃
                ResetController();

                isTamingReady = false;
                sendMonsterIndex = -1;
            }
        }

        /// <summary>
        /// 먹이 아이템 보유 여부 반환
        /// </summary>
        private bool HasFeedItem()
        {
            return player.Inventory.GetItemCount(currentTamingData.use_item_id) > 0;
        }
    }
}