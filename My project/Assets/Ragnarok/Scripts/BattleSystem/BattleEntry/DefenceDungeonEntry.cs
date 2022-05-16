using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class DefenceDungeonEntry : BattleEntry
    {
        private const string BGM_NAME = "Endless_tower";

        /******************** Models ********************/
        private readonly DungeonModel dungeonModel;

        /******************** Repositories ********************/
        private readonly DefenceDungeonDataManager defenceDungeonDataRepo;

        /******************** Scene Compositions ********************/
        private readonly float waveDelayTime; // 웨이브 딜레이
        private readonly int buyDungeonTicketCatCoin; // 던전 유료 입장 비용
        private readonly int buyDungeonTicketIncCatCoin; // 던전 유료 입장 비용 가중치

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIResultDungeon uiResultDungeon;
        private UIController uiController;
        private UIBattleInfo uiBattleInfo;
        private UIDanger uiDanger;
        private UIAutoEquip uiAutoEquip;
        private UIWave uiWave;
        private UIBattleMonsterCount uiBattleMonsterCount;
        private UIBattleExit uiBattleExit;
        private UIGoodsView uiGoodsView;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIBattleSkillList uiBattleSkillList;
        private UICamSelect uiCamSelect;

        /******************** Temp Data ********************/
        private readonly BetterList<CoroutineHandle> monsterSpawnHandles; // 몬스터 소환 코루틴 리스트
        private Map map; // 맵
        private DefenceDungeonData currentData; // 현재 진행중인 던전 데이터
        private int waveIndex; // 웨이브
        private int monsterMaxCount; // 최대 몬스터 수
        private int monsterRemainCount; // (앞으로 죽여야 하는) 남은 몬스터 수
        private int monsterRemainSpawnCount; // 전체 남은 소환 수
        private int droppedZeny; // 누적 드랍 제니
        private int droppedLevelExp; // 누적 드랍 레벨 경험치
        private int droppedJobExp; // 누적 드랍 직업 경험치

        public DefenceDungeonEntry() : base(BattleMode.Defence)
        {
            dungeonModel = player.Dungeon;
            defenceDungeonDataRepo = DefenceDungeonDataManager.Instance;
            monsterSpawnHandles = new BetterList<CoroutineHandle>();
            waveDelayTime = BasisType.STORY_FIELD_WAVE_COOL_TIME.GetInt() * 0.001f;
            buyDungeonTicketCatCoin = BasisType.BUY_DUNGEON_TICKET_CAT_COIN.GetInt();
            buyDungeonTicketIncCatCoin = BasisType.BUY_DUNGEON_TICKET_INC_CAT_COIN.GetInt();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            currentData = null;
            waveIndex = 0;
            monsterMaxCount = 0;
            monsterRemainCount = 0;
            monsterRemainSpawnCount = 0;
            monsterSpawnHandles.Clear(); // 몬스터 소환 코루틴 초기화

            SetNextSingleAction(ShowDungeonUI);
        }

        private void ShowDungeonUI()
        {
            UIDungeon.viewType = UIDungeon.ViewType.Defence;
            UI.Show<UIDungeon>();
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
            uiMainTop = UI.Show<UIMainTop>();
            uiResultDungeon = UI.Show<UIResultDungeon>();
            uiController = UI.Show<UIController>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiDanger = UI.Show<UIDanger>();
            uiAutoEquip = UI.Show<UIAutoEquip>();
            uiWave = UI.Show<UIWave>();
            uiBattleMonsterCount = UI.Show<UIBattleMonsterCount>();
            uiBattleExit = UI.Show<UIBattleExit>();
            uiGoodsView = UI.Show<UIGoodsView>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiBattleSkillList = UI.Show<UIBattleSkillList>();
            uiCamSelect = UI.Show<UICamSelect>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiResultDungeon.OnRetryDungeon += RestartBattle;
            uiResultDungeon.OnFinishDungeon += ExitEntry;
            uiResultDungeon.Hide();
            uiBattleExit.OnExit += OnBattleExit;

            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnToggleSkill;

            uiBattleSkillList.OnSelect += OnSelectSkill;
            uiBattleSkillList.OnToggleSkill += OnToggleSkill;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattleExit.SetMode(UIBattleExit.Mode.TYPE_2);
            uiBattleMazeSkillList.Hide();

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
            uiResultDungeon.OnRetryDungeon -= RestartBattle;
            uiResultDungeon.OnFinishDungeon -= ExitEntry;
            uiBattleExit.OnExit -= OnBattleExit;

            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnToggleSkill;

            uiBattleSkillList.OnSelect -= OnSelectSkill;
            uiBattleSkillList.OnToggleSkill -= OnToggleSkill;

            UI.Close<UIMainTop>();
            UI.Close<UIResultDungeon>();
            UI.Close<UIController>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIDanger>();
            UI.Close<UIAutoEquip>();
            UI.Close<UIWave>();
            UI.Close<UIBattleMonsterCount>();
            UI.Close<UIBattleExit>();
            UI.Close<UIGoodsView>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIBattleSkillList>();
            UI.Close<UICamSelect>();

            uiMainTop = null;
            uiResultDungeon = null;
            uiController = null;
            uiBattleInfo = null;
            uiDanger = null;
            uiAutoEquip = null;
            uiWave = null;
            uiBattleMonsterCount = null;
            uiBattleExit = null;
            uiGoodsView = null;
            uiBattlePlayerStatus = null;
            uiBattleMazeSkillList = null;
            uiBattleSkillList = null;
            uiCamSelect = null;
        }

        async void OnBattleExit()
        {
            if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                return;

            EndBattle();
            EndDungeonAsync(currentData.id, isClear: false).WrapNetworkErrors(); // 현재 던전 종료
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            string mainText = LocalizeKey._7030.ToText() // {DUNGEON} 난이도{DIFFICULTY}
                .Replace(ReplaceKey.DUNGEON, currentData.name_id.ToText())
                .Replace(ReplaceKey.DIFFICULTY, currentData.difficulty);

            uiBattleInfo.Set(mainText);
        }

        public override void OnBack()
        {
            if (uiResultDungeon.IsVisible)
                uiResultDungeon.Back();
            else
                OnBattleExit();
        }

        /// <summary>
        /// 보상 스택 초기화
        /// </summary>
        public override void ResetRewardStack()
        {
            base.ResetRewardStack();

            droppedZeny = 0;
            droppedLevelExp = 0;
            droppedJobExp = 0;
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            waveIndex = 0;
            monsterMaxCount = 0;
            monsterRemainCount = 0;
            monsterRemainSpawnCount = 0;
            monsterSpawnHandles.Clear(); // 몬스터 소환 코루틴 초기화
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

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            currentData = defenceDungeonDataRepo.Get(id); // 현재 던전 데이터 세팅
            if (currentData == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 입장 불가능한 던전
            if (!dungeonModel.CanEnter(DungeonType.Defence, id, isShowPopup: true))
            {
                IsSuccessEnter = false;
                yield break;
            }

            Task<bool> task = dungeonModel.RequestDefenceDungeonStart(id);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            IsSuccessEnter = task.Result;

            if (!IsSuccessEnter)
                yield break;

            Analytics.TrackEvent(TrackType.AirshipAssault);
            Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.Defence.ToIntValue()); // 특정던전 입장 횟수
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
        /// 씬 로드 시 호출
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

        protected override void OnReady()
        {
            base.OnReady();

            if (waveIndex == 0)
            {
                impl.ResetUnitList(); // 모든 유닛 초기화
            }
            else
            {
                impl.ResetEnemyUnitList(); // 적군 유닛 초기화
            }

            uiBattlePlayerStatus.SetPlayer(player);
            uiBattleMazeSkillList.SetCharacter(player);
            uiBattleSkillList.SetCharacter(player);
        }

        /// <summary>
        /// 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 처음 한 번만 처리
            if (waveIndex == 0)
            {
                player.SetCurrentHp(player.MaxHP);
                player.SetCurrentMp(player.MaxMp);
            }

            int wave = waveIndex + 1;
            int maxWave = DefenceDungeonData.MAX_WAVE;
            uiWave.Show(wave, maxWave); // 웨이브 표시

            Timing.RunCoroutine(YieldCheckSpawnMonster(), TAG); // 몬스터 남은 소환 수 체크하여 종료

            player.GetActor().EffectPlayer.ShowUnitCircle();
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

            if (waveIndex == 0)
            {
                ShowRemainMonsterCount(monsterMaxCount, monsterMaxCount);

                yield return Timing.WaitUntilDone(YieldSpawnCharacter(), TAG);
                yield return Timing.WaitUntilDone(YieldSpawnGuardian(currentData.guardian_id, currentData.guardian_level), TAG);
            }
            else
            {
                yield return Timing.WaitForSeconds(waveDelayTime);
            }

            // 몬스터 생성
            DefenceDungeonData.WaveInfo waveInfo = currentData.GetWaveInfo(waveIndex);
            SetMonsterMaxCount(waveInfo.monsterCount); // 최대 소환 수 세팅
            // Zone 은 4개로 고정
            for (int i = 0; i < DefenceDungeonData.WaveInfo.ZONE_COUNT; i++)
            {
                monsterSpawnHandles.Add(Timing.RunCoroutine(YieldSpawnMonster(map.GetDefenceSpawnZone(i), waveInfo.GetMonsterId(i), waveInfo.monsterLevel), TAG)); // 몬스터 소환
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCharacter()
        {
            if (impl.Add(player, isEnemy: false))
            {
                UnitActor actor = player.SpawnActor(); // 유닛 소환
                cameraController.SetPlayer(actor.CachedTransform); // 카메라 타겟 세팅
                Vector3 playerPosition = map.GetPlayerPosition();
                actor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 수호자 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnGuardian(int guardianId, int guardianLevel)
        {
            // 추가 실패 (이미 등록 되어있음)
            UnitEntity guardian = factory.CreateGuardian(guardianId, guardianLevel);
            if (!impl.Add(guardian, isEnemy: false))
                yield break;

            UnitActor guardianActor = guardian.SpawnActor(); // 유닛 소환
            Vector3 guardianPosition = map.GetGuardianPosition();
            guardianActor.AI.SetHomePosition(guardianPosition, isWarp: true); // 위치 세팅
            yield return Timing.WaitForOneFrame;
        }

        /// <summary>
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster(DefenceSpawnZone zone, int monsterId, int monsterLevel)
        {
            yield return Timing.WaitForSeconds(zone.startDelay);

            int spawnCount = 0; // 해당 zone 의 몬스터 소환 수
            while (true)
            {
                // 한 꺼번에 소환될 몬스터 수
                for (int i = 0; i < zone.spawnSize; i++)
                {
                    // 추가 실패 (이미 등록 되어있음)
                    UnitEntity monster = factory.CreateGuardianDestroyer(monsterId, monsterLevel);
                    if (!impl.Add(monster, isEnemy: true))
                        continue;

                    UnitActor monsterActor = monster.SpawnActor(); // 유닛 소환
                    Vector3 pos = zone.GetCenter();
                    monsterActor.AI.SetHomePosition(pos, isWarp: true); // 홈 세팅

                    monsterActor.AI.ReadyToBattle();
                    monsterActor.EffectPlayer.PlayPanelBuffEffect(); // 몬스터 소환 이펙트

                    --monsterRemainSpawnCount; // 전체 남은 소환 수 줄임
                    ++spawnCount; // 소환 수 증가

                    yield return Timing.WaitForOneFrame;
                }

                if (spawnCount < zone.spawnCount)
                {
                    yield return Timing.WaitForSeconds(zone.spawnDelay); // 소환 딜레이
                }
                else // 사이클 종료
                {
                    spawnCount = 0; // 해당 zone 의 몬스터 소환 수 초기화
                    yield return Timing.WaitForSeconds(zone.spawnCooldown); // 사이클 소환 쿨타임
                }
            }
        }

        /// <summary>
        /// 몬스터 소환 체크 (모든 소환이 완료되었을 때, 몬스터 소환 종료하는 역할
        /// </summary>
        private IEnumerator<float> YieldCheckSpawnMonster()
        {
            while (monsterRemainSpawnCount > 0)
            {
                yield return Timing.WaitForOneFrame;
            }

            // 몬스터 소환 코루틴 종료
            foreach (var item in monsterSpawnHandles)
            {
                Timing.KillCoroutines(item);
            }

            monsterSpawnHandles.Clear();
        }

        /// <summary>
        /// 최대 몬스터 세팅
        /// </summary>
        private void SetMonsterMaxCount(int value)
        {
            monsterMaxCount = value; // 최대 몬스터 세팅
            monsterRemainCount = value; // 몬스터 죽인 수 초기화
            monsterRemainSpawnCount = value; // 남은 소환 수 세팅

            ShowRemainMonsterCount(monsterRemainCount, monsterMaxCount);
        }

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (attacker.type == UnitEntityType.Player || attacker.type == UnitEntityType.PlayerCupet)
            {
                if (unitEntity.type == UnitEntityType.GuardianDestroyer)
                {
                    dungeonModel.RequestDefenceDungeonMonsterDrop(currentData.id, waveIndex + 1, unitEntity.battleUnitInfo.Id, unitEntity.clientUID).WrapNetworkErrors();
                }
            }

            if (impl.IsPlayerDead())
            {
                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
            }

            if (unitEntity.type == UnitEntityType.GuardianDestroyer)
            {
                ShowRemainMonsterCount(--monsterRemainCount, monsterMaxCount); // 죽여야 할 몬스터 수 감소
            }

            if (monsterRemainCount == 0) // 남은 몬스터 수가 없을 때
            {
                int wave = waveIndex + 1;
                if (wave < DefenceDungeonData.MAX_WAVE) // 최대 웨이브에 도달하지 않음
                {
                    ++waveIndex; // 웨이브 증가
                    Ready();
                }
                else
                {
                    EndDungeonAsync(currentData.id, isClear: true).WrapNetworkErrors(); // 현재 던전 종료
                }
            }
            else if (impl.IsPlayerDead() || impl.IsDead(UnitEntityType.Guardian)) // 플레이어 또는 수모물이 죽었는지 확인
            {
                EndBattle(); // 던전 종료
                EndDungeonAsync(currentData.id, isClear: false).WrapNetworkErrors(); // 현재 던전 종료
            }
        }

        /// <summary>
        /// 디펜스 던전 실패
        /// </summary>
        private void ShowFailPopup()
        {
            string message = LocalizeKey._90139.ToText(); // 방어에 실패하여 보상을 획득할 수 없습니다.
            uiResultDungeon.Show(new RewardData[0], currentData.DungeonType, true, UIResultDungeon.TitleType.Failed, message);
        }

        /// <summary>
        /// 디펜스 던전 성공
        /// </summary>
        private void ShowClearPopup()
        {
            string message = LocalizeKey._90140.ToText(); // 방어에 성공하여 보상을 획득하였습니다!
            uiResultDungeon.Show(rewardBuffer.GetBuffer(isAutoRelease: true), currentData.DungeonType, true, UIResultDungeon.TitleType.Clear, message);
        }

        /// <summary>
        /// 던전 종료
        /// </summary>
        private async Task EndDungeonAsync(int id, bool isClear)
        {
            bool isSuccess = await dungeonModel.RequestDefenceDungeonEnd(id, isClear);
            if (!isSuccess)
                return;

            if (isClear)
            {
                ShowClearPopup();
            }
            else
            {
                ShowFailPopup();
            }
        }

        /// <summary>
        /// 몬스터 아이템 드랍 결과
        /// </summary>
        void OnMonsterItemDrop(UnitEntity unit, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            // 보상 스택 추가
            this.droppedZeny += droppedZeny;
            this.droppedLevelExp += droppedLevelExp;
            this.droppedJobExp += droppedJobExp;

            if (rewards != null)
                rewardBuffer.AddRange(rewards);
        }

        /// <summary>
        /// 남은 몬스터 수 보여주기
        /// </summary>
        private void ShowRemainMonsterCount(int remainCount, int maxCount)
        {
            uiBattleMonsterCount.UpdateMonsterCount(remainCount, maxCount);
        }

        /// <summary>
        /// 현재 전투 다시 시작
        /// </summary>
        private void RestartBattle()
        {
            if (currentData == null)
                return;

            impl.StartBattle(BattleMode.Defence, currentData.id);
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
    }
}