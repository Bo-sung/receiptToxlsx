using MEC;
using Ragnarok.SceneComposition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class WorldBossEntry : BattleEntry
    {
        private const string BGM_NAME = "Prison_01";

        /******************** Models ********************/
        private readonly DungeonModel dungeonModel;
        private readonly AlarmModel alarmModel;
        private readonly CharacterModel characterModel;

        /******************** Repositories ********************/
        private readonly WorldBossDataManager worldBossDataRepo;

        /******************** Scene Compositions ********************/
        private readonly WorldBossPlayerBotEntityPool playerBotPool;

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIResultWorldBossClear uiResultWorldBossClear;
        private UIController uiController;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleInfo uiBattleInfo;
        private UIDanger uiDanger;
        private UIBattleBossHp uiBattleBossHP;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIBattleWorldBossRank uiBattleWorldBossRank;
        private UIBattleMenu uiBattleMenu;
        private UIGoodsView uiGoodsView;
        private UIBattleSkillList uiBattleSkillList;
        private UIMvpTimer uiMvpTimer;
        private UICamSelect uiCamSelect;

        /******************** Temp Data ********************/
        private Map map; // 맵
        private WorldBossData currentData; // 현재 진행중인 던전 데이터
        private UnitEntity bossEntity;
        private int worldBossCurHp;
        private WorldBossRankPacket[] rankArray;
        private int accrueDamage; // 월드보스 누적 대미지
        private bool isBossDie;
        private bool isFinishExitRoom;
        private readonly long remainTime; // 월드보스 제한 시간(밀리초)
        private int serverRandomKey;
        private readonly Dictionary<(int uid, int cid), WorldBossRankPacket> rankDict;

        public WorldBossEntry() : base(BattleMode.WorldBoss)
        {
            player.SetState(UnitEntity.UnitState.Stage);

            dungeonModel = player.Dungeon;
            alarmModel = player.AlarmModel;
            characterModel = player.Character;

            worldBossDataRepo = WorldBossDataManager.Instance;
            playerBotPool = new WorldBossPlayerBotEntityPool();

            isBossDie = false;
            remainTime = BasisType.WORLD_BOSS_BATTLE_TIME.GetInt();
            rankDict = new Dictionary<(int uid, int cid), WorldBossRankPacket>();
        }

        public override IEnumerator<float> YieldExitBattle()
        {
            if (isFinishExitRoom)
            {
                IsSuccessExit = true;
                yield break;
            }
            Task<Response> task = dungeonModel.RequestWorldBossExit(isBossDie);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessExit = response.isSuccess;

            if (!IsSuccessExit)
                yield break;

            isFinishExitRoom = true;
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            currentData = null;

            // 플레이어 봇 폐기
            foreach (var item in playerBotPool)
            {
                Dispose(item);
            }

            SetNextSingleAction(ShowDungeonUI);
        }

        private void ShowDungeonUI()
        {
            UIDungeon.viewType = UIDungeon.ViewType.WorldBoss;
            UI.Show<UIDungeon>();
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();
            Protocol.RECEIVE_WORLD_BOSS_RANK_INFO.AddEvent(OnReceiveWorldBossRankInfo);
            Protocol.RECEIVE_WORLD_BOSS_CLOSE.AddEvent(OnReceiveWorldBossDie);
            Protocol.RECEIVE_WORLD_BOSS_ROOM_JOIN.AddEvent(OnWorldBossMultiPlayerJoin);
            Protocol.RECEIVE_WORLD_BOSS_ROOM_EXIT.AddEvent(OnWorldBossMultiPlayerExit);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();
            Protocol.RECEIVE_WORLD_BOSS_RANK_INFO.RemoveEvent(OnReceiveWorldBossRankInfo);
            Protocol.RECEIVE_WORLD_BOSS_CLOSE.RemoveEvent(OnReceiveWorldBossDie);
            Protocol.RECEIVE_WORLD_BOSS_ROOM_JOIN.RemoveEvent(OnWorldBossMultiPlayerJoin);
            Protocol.RECEIVE_WORLD_BOSS_ROOM_EXIT.RemoveEvent(OnWorldBossMultiPlayerExit);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiResultWorldBossClear = UI.Show<UIResultWorldBossClear>();
            uiController = UI.Show<UIController>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiDanger = UI.Show<UIDanger>();
            uiBattleBossHP = UI.Show<UIBattleBossHp>();
            uiBattleWorldBossRank = UI.Show<UIBattleWorldBossRank>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiGoodsView = UI.Show<UIGoodsView>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiBattleSkillList = UI.Show<UIBattleSkillList>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiCamSelect = UI.Show<UICamSelect>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiResultWorldBossClear.OnExit += ExitEntry;
            uiBattleMenu.OnExit += OnClickExit;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnToggleSkill;
            uiBattleSkillList.OnSelect += OnSelectSkill;
            uiBattleSkillList.OnToggleSkill += OnToggleSkill;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);

            uiResultWorldBossClear.Hide();
            uiBattleBossHP.Hide();
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit);
            uiBattleMazeSkillList.Hide();
            uiMvpTimer.Hide();

            cameraController.SetView(CameraController.View.Quater40_2_5); // Quater로 전환
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiResultWorldBossClear.OnExit -= ExitEntry;
            uiBattleMenu.OnExit -= OnClickExit;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnToggleSkill;
            uiBattleSkillList.OnSelect -= OnSelectSkill;
            uiBattleSkillList.OnToggleSkill -= OnToggleSkill;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;

            UI.Close<UIMainTop>();
            UI.Close<UIResultWorldBossClear>();
            UI.Close<UIController>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIDanger>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattleWorldBossRank>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIGoodsView>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIBattleSkillList>();
            UI.Close<UIMvpTimer>();
            UI.Close<UICamSelect>();

            uiMainTop = null;
            uiResultWorldBossClear = null;
            uiController = null;
            uiBattleInfo = null;
            uiDanger = null;
            uiBattleBossHP = null;
            uiBattleWorldBossRank = null;
            uiBattleMenu = null;
            uiBattlePlayerStatus = null;
            uiBattleMazeSkillList = null;
            uiBattleSkillList = null;
            uiMvpTimer = null;
            uiCamSelect = null;
        }

        public override void OnBack()
        {
            OnClickExit();
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            string mainText = DungeonType.WorldBoss.ToText();
            uiBattleInfo.Set(mainText);
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            rankArray = null;

            // 보스 폐기
            Dispose(bossEntity);
            bossEntity = null;
        }

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            currentData = worldBossDataRepo.Get(id); // 현재 던전 데이터 세팅

            if (currentData == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 입장 불가능한 던전
            if (!dungeonModel.CanEnter(DungeonType.WorldBoss, id, isShowPopup: true))
            {
                IsSuccessEnter = false;
                yield break;
            }

            Task<Response> task = dungeonModel.RequestWorldBossStart(id); // 월드보스 시작
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
                yield break;

            SetRank(response.GetPacketArray<WorldBossRankPacket>("1"));
            worldBossCurHp = response.GetInt("2");
            if (response.ContainsKey("3"))
            {
                playerBotPool.EnqueueRange(response.GetPacketArray<BattleCharacterPacket>("3")); // 다른 유저 정보
            }
            accrueDamage = response.ContainsKey("4") ? (int)response.GetDouble("4") : 0;

            Analytics.TrackEvent(TrackType.InfiniteDungeon);
            Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.WorldBoss.ToIntValue()); // 특정던전 입장 횟수
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

        private async void OnClickExit()
        {
            if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                return;

            ClearDungeon(isClear: false);
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = currentData.scene_name; // 씬 이름
            string bgmName = BGM_NAME; // 배경음 이름
            bool isChangeScene = !SceneLoader.IsCheckCurrentScene(sceneName) || (map == null); // 씬 변경 여부 - 씬이 다르거나, 맵이 존재하지 않을 경우
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            impl.ResetUnitList(); // 모든 유닛 초기화

            //uiQuickMenu.Hide();
            cameraController.RemoveMask(Layer.PLAYER, Layer.CUPET, Layer.ALLIES, Layer.ENEMY, Layer.GHOST); // 카메라 레이어 설정

            uiBattlePlayerStatus.SetPlayer(player);
            uiBattleMazeSkillList.SetCharacter(player);
            uiBattleSkillList.SetCharacter(player);

            uiMvpTimer.Initialize(remainTime);
            uiMvpTimer.Show();
            uiMvpTimer.RestartTimer();
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            cameraController.AddMask(Layer.PLAYER, Layer.CUPET, Layer.ALLIES, Layer.ENEMY, Layer.GHOST); // 카메라 레이어 설정

            ShowAccrueDamage(accrueDamage); // 누적 대미지 표시

            player.GetActor().EffectPlayer.ShowUnitCircle();

            RefreshState();
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnWorldBossMonster(currentData), TAG);

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            if (impl.Add(player, isEnemy: false))
            {
                UnitActor unitActor = player.SpawnActor(); // 유닛 소환

                Vector3 playerPosition = map.GetPlayerPosition();
                unitActor.AI.SetHomePosition(playerPosition, isWarp: true); // Home 세팅

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

            if (!impl.Add(entity, isEnemy: false))
                return false;

            UnitActor unitActor = entity.SpawnActor(); // 유닛 소환

            Vector3 randomSpawnPos = map.GetRandomSpawnPosition();
            unitActor.AI.SetHomePosition(randomSpawnPos, isWarp: true); // Home 세팅
            unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출
            return true;
        }

        /// <summary>
        /// 월드보스 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnWorldBossMonster(IBossMonsterSpawnData input)
        {
            bossEntity = factory.CreateWorldBoss(input);

            UnitActor bossActor;
            if (impl.Add(bossEntity, isEnemy: true))
            {
                bossActor = bossEntity.SpawnActor(); // 유닛 소환

                Vector3 bossPosition = map.GetBossPosition();
                bossActor.AI.SetHomePosition(bossPosition, isWarp: false); // 위치 세팅
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                bossActor = bossEntity.GetActor();
            }

            // 보스 스폰 지점 설정
            Vector3 bossHomePosition = map.GetBossPosition(); // 보스 위치 (바닥, 도착점)
            Vector3 bossSpawnPosition = bossHomePosition; // 보스 위치 (공중, 시작점)

            bossSpawnPosition.y = Constants.Map.BOSS_FALLING_POSITION_Y;
            float fallTime = Constants.Battle.FALL_TIME; // 보스 2초 동안 낙하 (보스 위치 이동, 그림자 크기 조절)

            bossActor.Show();
            bossActor.AI.EndAI();
            bossActor.Movement.Fall(bossSpawnPosition, bossHomePosition, fallTime);

            ShowAccrueDamage(accrueDamage); // 누적 대미지 표시

            var bossData = MonsterDataManager.Instance.Get(currentData.monster_id);

            float hudOffset = bossData.GetHudOffset();
            PoolObject bossHud = bossActor.EffectPlayer.SetHpHudTarget(new Vector3(0, hudOffset, 0));
            Debug.Log($" 몬스터 {nameof(hudOffset)}={hudOffset}, {nameof(bossData.hud_y_value)}={bossData.hud_y_value}");

            uiBattleBossHP.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHP.SetOffset(UIBattleBossHp.Offset.BossMonster);
            uiBattleBossHP.Show(
                currentData.monster_level,
                bossData.name_id.ToText(),
                bossData.element_type.ToEnum<ElementType>(),
                UIBattleBossHp.MonsterType.NormalBoss,
                bossData.cost,
                bossHud.CachedGameObject);
            uiBattleBossHP.SetHp(worldBossCurHp, bossEntity.MaxHP);
            //ShowBossHp(worldBossCurHp, bossEntity.MaxHP); // 보스 체력 보여주기

            yield return Timing.WaitForSeconds(fallTime); // 낙하시간까지 기다림
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (impl.IsAllEnemyDead()) // 모든 적이 죽었는지 확인
            {
                ClearDungeon(isClear: true); // 현재 던전 종료
            }
            else if (impl.IsPlayerDead()) // 플레이어가 죽었는지 확인
            {
                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
                ClearDungeon(isClear: false); // 현재 던전 종료
            }
        }

        /// <summary>
        /// 실제 입은 대미지 값
        /// </summary>        
        public override async void OnDamageValue(UnitEntity unit, UnitEntity attacker, int damage)
        {
            if (unit.type != UnitEntityType.WorldBoss || attacker.type != UnitEntityType.Player)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", damage);
            sfs.PutByteArray("2", unit.damagePacket.ToByteArray()); // 대미지 패킷

            if (serverRandomKey > 0)
            {
                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("월드보스 몬스터 RandomKey 처리: key = " + serverRandomKey + ", hex = " + MathUtils.CidToHexCode(serverRandomKey));
                }

                sfs.PutUtfString("3", MathUtils.CidToHexCode(serverRandomKey));
                serverRandomKey = 0;
            }

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(unit.damagePacket);
#endif

            Response response = await Protocol.REQUEST_WORLD_BOSS_ATTACK.SendAsync(sfs);

            if (response.isSuccess)
            {
                int bossHp = response.ContainsKey("1") ? response.GetInt("1") : 0;
                int curDamage = response.ContainsKey("2") ? curDamage = response.GetInt("2") : damage;

#if UNITY_EDITOR
                if (response.ContainsKey("3"))
                {
                    debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("3")));
                    DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
                }
#endif
                // 서버에서 받은 몬스터 랜덤 키
                if (response.ContainsKey("4"))
                {
                    serverRandomKey = response.GetInt("4");

                    if (DebugUtils.IsMonsterDropKey)
                    {
                        Debug.Log("서버에서 받은 월드보스 RandomKey 값: " + serverRandomKey);
                    }
                }

                accrueDamage += curDamage;
                ShowAccrueDamage(accrueDamage);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 던전 종료
        /// </summary>
        private void ClearDungeon(bool isClear)
        {
            uiBattleWorldBossRank.Hide();
            uiBattleBossHP.Hide();
            uiMvpTimer.StopTimer();
            uiMvpTimer.Hide();
            EndUnitAI();

            if (isClear)
            {
                var group = (IDungeonGroup)currentData;
                var infos = group.GetRewardInfos();
                List<RewardData> rewardList = new List<RewardData>();
                for (int i = 0; i < infos.Length; ++i)
                    rewardList.Add(infos[i].info.data);
                rewardList.Add(rewardList[rewardList.Count - 1]); // join reward 를 하나 더 넣는다.

                int realRank = -1;
                int rank = int.MaxValue;
                if (rankArray != null)
                {
                    int playerCid = characterModel.Cid;
                    for (int i = 0; i < rankArray.Length; ++i)
                    {
                        if (rankArray[i].cid == playerCid)
                        {
                            rank = i;
                            realRank = rankArray[i].ranking;
                            break;
                        }
                    }
                }

                rank = Mathf.Min(4, rank);
                var playerReward = rewardList[rank];

                uiResultWorldBossClear.Show(realRank, player.GetName(), accrueDamage, dungeonModel.WorldBossMaxHp, true, rankArray, rewardList.ToArray(), playerReward);
                alarmModel.AddAlarm(AlarmType.MailCharacter); // 우편함 빨간점 표시
            }
            else
            {
                int realRank = -1;
                if (rankArray != null)
                {
                    int playerCid = characterModel.Cid;
                    for (int i = 0; i < rankArray.Length; ++i)
                    {
                        if (rankArray[i].cid == playerCid)
                        {
                            realRank = rankArray[i].ranking;
                            break;
                        }
                    }
                }

                uiResultWorldBossClear.Show(realRank, player.GetName(), accrueDamage, dungeonModel.WorldBossMaxHp, false, rankArray, null, null);
            }
        }

        private void OnReceiveWorldBossRankInfo(Response response)
        {
            if (isBossDie) // 결과창을 표시하는 도중에는 랭킹 갱신하면 안될듯..
                return;

            if (response.ContainsKey("1"))
            {
                SetRank(response.GetPacketArray<WorldBossRankPacket>("1"));
                Array.Sort(rankArray, SortByOrderRank);

                ShowRank(rankArray);
            }

            worldBossCurHp = response.GetInt("2");

            ShowBossHp(worldBossCurHp, dungeonModel.WorldBossMaxHp);
        }

        private void OnReceiveWorldBossDie(Response response)
        {
            isBossDie = true; // 보스 사망

            if (response.ContainsKey("1"))
            {
                SetRank(response.GetPacketArray<WorldBossRankPacket>("1"));
                Array.Sort(rankArray, SortByOrderRank);
            }

            RefreshState();
        }

        private void RefreshState()
        {
            if (!isBossDie)
                return;

            if (!IsAllReady)
                return;

            ShowRank(rankArray);

            EndUnitAI();
            ShowBossHp(0, dungeonModel.WorldBossMaxHp);
            ClearDungeon(isClear: true);
        }

        private void EndUnitAI()
        {
            UnitActor getActor = player.GetActor();
            if (getActor)
            {
                getActor.AI.ChangeState(AI.Transition.EndBattle);
                getActor.CancelSkill(); // 모든 스킬 중지 (보스 죽은 후 스킬 들어가는 버그 수정)
            }

            UnitActor bossActor = bossEntity.GetActor();
            if (bossActor)
                bossActor.AI.ChangeState(AI.Transition.Dead);
        }

        private int SortByOrderRank(WorldBossRankPacket a, WorldBossRankPacket b)
        {
            return a.ranking.CompareTo(b.ranking);
        }

        /// <summary>
        /// 다른 플레이어 입장
        /// </summary>
        private void OnWorldBossMultiPlayerJoin(Response response)
        {
            IMultiPlayerInput multiPlayer = response.GetPacket<BattleCharacterPacket>();

            // 생성 도중이라면 생성큐 추가
            if (playerBotPool.HasQueue())
            {
                playerBotPool.Enqueue(multiPlayer);
                return;
            }

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

        /// <summary>
        /// 다른 플레이어 퇴장
        /// </summary>
        private void OnWorldBossMultiPlayerExit(Response response)
        {
            int cid = response.GetInt("1");

            GhostPlayerEntity find = playerBotPool.Find(cid);
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
        }

        private void ShowBossHp(int curHp, int maxHp)
        {
            if (uiBattleBossHP == null)
                return;

            uiBattleBossHP.TweenHp(curHp, maxHp);
        }

        /// <summary>
        /// 누적 대미지 표시
        /// </summary>
        private void ShowAccrueDamage(int accrueDamage)
        {
            int realRank = -1;

            if (rankArray != null)
            {
                int playerCid = characterModel.Cid;
                for (int i = 0; i < rankArray.Length; ++i)
                {
                    if (rankArray[i].cid == playerCid)
                    {
                        realRank = rankArray[i].ranking;
                        break;
                    }
                }
            }

            uiBattleWorldBossRank.ShowMyRank(realRank, player.GetName(), MathUtils.GetProgress(accrueDamage, dungeonModel.WorldBossMaxHp));
            ShowRank(rankArray);
        }

        private void ShowRank(WorldBossRankPacket[] rankArray)
        {
            if (uiBattleWorldBossRank == null)
                return;

            int length = rankArray == null ? 0 : rankArray.Length;

            const int MAX_RANK_COUNT = 3;
            for (int i = 0; i < MAX_RANK_COUNT; i++)
            {
                if (i < length)
                {
                    uiBattleWorldBossRank.SetWorldBossRankSlot(i, rankArray[i].ranking, rankArray[i].char_name, MathUtils.GetProgress(rankArray[i].score, dungeonModel.WorldBossMaxHp));
                }
                else
                {
                    uiBattleWorldBossRank.SetActiveWorldBossRankSlot(i, false);
                }
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
                    //skillModel.RequestBuySkillSlot().WrapNetworkErrors();
                    break;
            }
        }

        private void OnToggleSkill()
        {
            if (uiBattleSkillList.IsVisible)
            {
                uiBattleSkillList.Hide();
                uiBattleMazeSkillList.Show();
            }
            else
            {
                uiBattleSkillList.Show();
                uiBattleMazeSkillList.Hide();
            }
        }

        void OnFinishedRemainTimer()
        {
            ClearDungeon(isClear: false);
        }

        void SetRank(WorldBossRankPacket[] packets)
        {
            rankDict.Clear();
            foreach (var packet in packets)
            {
                // 중복
                if (rankDict.ContainsKey((packet.uid, packet.cid)))
                    continue;

                rankDict.Add((packet.uid, packet.cid), packet);
            }
            rankArray = rankDict.Values.ToArray();
        }
    }
}