using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CentralLabEntry : BattleEntry
    {
        public class BattleInput : IBattleInput
        {
            public int labId;
            public Job cloneJob;
        }

        private const string SCENE_NAME = "Scene_OrcVillage_4";
        private const string BGM = "Endless_tower";

        private const int PLAYER_PLUS_MOVE_SPEED = 3000;
        private const int MAX_ITEM_SKILL_POINT = 10000; // 최대 아이템스킬 포인트 (만분율)

        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly CharacterModel characterModel;
        private readonly SharingModel sharingModel;

        // <!-- Repositories --!>
        private readonly CentralLabDataManager centralLabDataRepo;
        private readonly CentralLabMonsterDataManager centralLabMonsterDataRepo;
        private readonly CentralLabSkillDataManager centralLabSkillDataRepo;
        private readonly SkillDataManager skillDataRepo;

        // <!-- Scene Compositions --!>
        private readonly CharacterEntity clonePlayer; // 클론 플레이어
        private readonly CloneCharacterSettings cloneSettings; // 클론 정보
        private readonly BetterList<CentralLabSkillPacket> skillList; // 보유 스킬 리스트
        private readonly BetterList<SkillInfo> itemSkillList; // 보유 아이템 스킬 리스트

        // <!-- UIs --!>
        private UIController uiController;
        private UIBattlePause uiBattlePause;
        private UIBattleWave uiBattleWave;
        private UIBattleMonsterCount uiBattleMonsterCount;
        private UIBattleCentralLab uiBattleCentralLab;
        private UICentralLabSkill uiCentralLabSkill;
        private UICentralLabItemSkill uiCentralLabItemSkill;
        private UIResultDungeon uiResultDungeon;
        private UIBattleBossHp uiBattleBossHp;

        // <!-- Temp Data --!>
        private Map map; // 맵
        private int labId;
        private Job cloneJob;
        private int curDataIndex;
        private int bonus;
        private int waveIndex;
        private CentralLabSkillPacket[] selectSkills; // 선택할 스킬
        private int itemSkillId; // 받은 아이템스킬
        private int monsterRemainCount, monsterMaxCount;
        private bool isFinishedSelectSkill;
        private MonsterType[] arrMonsterType;
        private UnitEntity bossEntity; // 혹시나 있을 보스
        private PoolObject bossHud; // 보스 offset

        public CentralLabEntry() : base(BattleMode.CentralLab)
        {
            dungeonModel = player.Dungeon;
            characterModel = player.Character;
            sharingModel = player.Sharing;

            centralLabDataRepo = CentralLabDataManager.Instance;
            centralLabMonsterDataRepo = CentralLabMonsterDataManager.Instance;
            centralLabSkillDataRepo = CentralLabSkillDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;

            clonePlayer = CharacterEntity.Factory.CreateGhostPlayer();
            cloneSettings = new CloneCharacterSettings();
            skillList = new BetterList<CentralLabSkillPacket>();
            itemSkillList = new BetterList<SkillInfo>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            skillList.Release();
            itemSkillList.Release();
            RemoveBossEvent();
            bossHud = null;

            Dispose(clonePlayer);

            SetNextSingleAction(ShowDungeonUI);
        }

        private void ShowDungeonUI()
        {
            UIDungeon.viewType = UIDungeon.ViewType.CentralLab;
            UI.Show<UIDungeon>();
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            dungeonModel.OnCentralLabMonsterKill += OnCentralLabMonsterKill;
            dungeonModel.OnCentralLabExit += ShowResultFail;
            clonePlayer.OnUseSkill += OnUseSkill;
            Protocol.REQUEST_CLAB_SKILL_COOLTIME_CHECK.AddEvent(OnRequestSkillCooltimeCheck);
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            dungeonModel.OnCentralLabMonsterKill -= OnCentralLabMonsterKill;
            dungeonModel.OnCentralLabExit -= ShowResultFail;
            clonePlayer.OnUseSkill -= OnUseSkill;
            Protocol.REQUEST_CLAB_SKILL_COOLTIME_CHECK.RemoveEvent(OnRequestSkillCooltimeCheck);
        }

        /// <summary>
        /// 백버튼
        /// </summary>
        public override void OnBack()
        {
            GamePause();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiController = UI.Show<UIController>();
            uiBattlePause = UI.Show<UIBattlePause>();
            uiBattleWave = UI.Show<UIBattleWave>();
            uiBattleMonsterCount = UI.Show<UIBattleMonsterCount>();
            uiBattleCentralLab = UI.Show<UIBattleCentralLab>();
            uiCentralLabSkill = UI.Show<UICentralLabSkill>();
            uiCentralLabItemSkill = UI.Show<UICentralLabItemSkill>();
            uiResultDungeon = UI.Show<UIResultDungeon>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattlePause.OnPause += GamePause;
            uiCentralLabSkill.OnSelect += OnSelectSkill;
            uiBattleCentralLab.OnSelect += OnSelectItemSkill;
            uiResultDungeon.OnFinishDungeon += ExitEntry;

            uiBattleWave.Hide();
            uiBattleMonsterCount.Hide();
            uiBattleCentralLab.Hide();
            uiCentralLabSkill.Hide();
            uiCentralLabItemSkill.Hide();
            uiResultDungeon.Hide();
            uiBattleBossHp.Hide();

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.SetOffset(UIBattleBossHp.Offset.BossMonster);

            cameraController.SetView(CameraController.View.Quater40_10);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattlePause.OnPause -= GamePause;
            uiCentralLabSkill.OnSelect -= OnSelectSkill;
            uiBattleCentralLab.OnSelect -= OnSelectItemSkill;
            uiResultDungeon.OnFinishDungeon -= ExitEntry;

            UI.Close<UIController>();
            UI.Close<UIBattlePause>();
            UI.Close<UIBattleWave>();
            UI.Close<UIBattleMonsterCount>();
            UI.Close<UIBattleCentralLab>();
            UI.Close<UICentralLabSkill>();
            UI.Close<UICentralLabItemSkill>();
            UI.Close<UIResultDungeon>();
            UI.Close<UIBattleBossHp>();

            uiController = null;
            uiBattlePause = null;
            uiBattleWave = null;
            uiBattleMonsterCount = null;
            uiBattleCentralLab = null;
            uiCentralLabSkill = null;
            uiCentralLabItemSkill = null;
            uiResultDungeon = null;
            uiBattleBossHp = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            // Level 세팅
            string levelText = LocalizeKey._48301.ToText() // LEVEL {VALUE}
                .Replace(ReplaceKey.VALUE, curDataIndex + 1);
            uiBattleWave.SetLevelText(levelText);

            // 보너스 세팅
            bool hasBonus = bonus > 0;
            string buffText;
            if (hasBonus)
            {
                buffText = LocalizeKey._48305.ToText() // 스탯 보너스 +{VALUE}%
                    .Replace(ReplaceKey.VALUE, bonus);
            }
            else
            {
                buffText = LocalizeKey._48304.ToText(); // 스탯 보너스 없음
            }

            uiBattleWave.SetBuffText(buffText, bonus > 0 ? UIBattleWave.BuffType.Positive : UIBattleWave.BuffType.Nagative);
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            RemoveBossEvent();
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            sharingModel.ReuseShareCharacters(); // 셰어캐릭터 공유 재연결
            IsSuccessExit = true;
            yield break;
        }

        public override IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
            if (battleInput == null)
            {
                IsSuccessEnter = false; // battleInput 음슴
                yield break;
            }

            if (battleInput is BattleInput input)
            {
                labId = input.labId;
                cloneJob = input.cloneJob;
            }
            else
            {
                IsSuccessEnter = false; // BattleInput Type 이 아님
                yield break;
            }

            CentralLabData currentData = centralLabDataRepo.Get(labId);
            curDataIndex = centralLabDataRepo.GetIndex(labId);
            if (currentData == null)
            {
                IsSuccessEnter = false; // 데이터 음슴
                yield break;
            }

            if (cloneJob == default)
            {
                IsSuccessEnter = false; // 클론 직업 음슴
                yield break;
            }

            Task<Response> task = dungeonModel.RequsetCentralLabStart(labId, cloneJob);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;
            if (!IsSuccessEnter)
                yield break;

            Analytics.TrackEvent(TrackType.CentralLaboratory);

            Task pauseShareTask = sharingModel.RequestPauseShareCharacter(); // 셰어캐릭터 공유 일시정지
            yield return Timing.WaitUntilTrue(pauseShareTask.IsComplete);

            CentralLabCharacterPacket characterPacket = response.GetPacket<CentralLabCharacterPacket>("1"); // 캐릭터 정보
            selectSkills = response.GetPacketArray<CentralLabSkillPacket>("2"); // 선택 스킬 정보

            // 스킬 초기화
            foreach (var item in selectSkills)
            {
                item.Initialize(skillDataRepo);
            }

            waveIndex = 0; // wave 초기화
            bonus = currentData.GetStatBonus(characterModel.JobLevel);
            cloneSettings.Initialize(characterModel.Cid, characterModel.Name, cloneJob, characterModel.Gender, currentData.stage_level, characterModel.ProfileId, characterPacket); // 캐릭터 기본 세팅
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            string sceneName = SCENE_NAME; // 씬 이름
            string bgmName = BGM; // 배경음 이름
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

            // 맨 처음 Index 에만 Wave 세팅
            if (waveIndex == 0)
            {
                arrMonsterType = centralLabMonsterDataRepo.GetWaveInfo(labId);
                uiBattleWave.Show(arrMonsterType);

                uiBattleCentralLab.Show();
                uiBattleCentralLab.SetPoint(0, MAX_ITEM_SKILL_POINT); // 초기화
                UpdateItemSkill(); // 아이템 스킬 업데이트

                uiBattleMonsterCount.Show();
                //impl.ResetUnitList(); // 모든 유닛 초기화
            }

            impl.ResetEnemyUnitList(); // 적군 유닛 초기화
            uiBattleWave.SetWave(waveIndex);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            cameraController.RemoveMask(Layer.ENEMY); // 카메라 레이어 설정

            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            // 맨 처음 Index 에만 캐릭터 생성
            if (waveIndex == 0)
                yield return Timing.WaitUntilDone(YieldSpawnClonePlayer(), TAG);

            CentralLabMonsterData[] arrMonsterData = centralLabMonsterDataRepo.GetArray(labId, waveIndex);
            if (arrMonsterData == null)
            {
                Debug.LogError($"몬스터 정보가 존재하지 않습니다: {nameof(labId)} = {labId}, {nameof(waveIndex)} = {waveIndex}");
            }
            else
            {
                yield return Timing.WaitUntilDone(YieldSpawnMonster(arrMonsterData), TAG);
            }

            int monsterCount = arrMonsterData == null ? 0 : arrMonsterData.Length;
            SetMonsterMaxCount(monsterCount); // 몬스터 수 세팅

            MonsterType monsterType = GetMonsterType(waveIndex);
            uiBattleMonsterCount.SetMonsterType(monsterType); // 몬스터 타입 세팅
        }

        /// <summary>
        /// 씬 세팅
        /// </summary>
        protected override IEnumerator<float> YieldComposeSceneAfter()
        {
            BattleTime.IsPause = true; // 일시정지

            // 아이템 스킬 존재
            if (itemSkillId > 0)
            {
                SkillData data = skillDataRepo.Get(itemSkillId, level: 1);
                if (data == null)
                {
                    Debug.LogError($"ItemSkillData is Null: {nameof(itemSkillId)} = {itemSkillId}");
                }
                else
                {
                    SkillInfo info = new ItemSkill();
                    info.SetSkillRate(10000); // 스킬 발동확률: value
                    info.SetData(data);
                    itemSkillList.Add(info);
                    uiCentralLabItemSkill.Show(centralLabSkillDataRepo.GetSkills(cloneJob, CentralLabSkillType.PoringBless), info);
                    yield return Timing.WaitUntilFalse(IsShowItemSkillUI); // 창이 닫힐때까지 기다림

                    UpdateItemSkill(); // 아이템 스킬 업데이트
                }

                itemSkillId = 0; // 아이템 스킬 초기화
            }

            isFinishedSelectSkill = false;
            uiCentralLabSkill.Show(selectSkills, skillList.ToArray()); // 스킬 선택 UI 표시
            yield return Timing.WaitUntilTrue(IsFinishedSelectSkill); // 스킬 선택 준비가 모두 끝날때까지 기다림

            selectSkills = null; // 선택할 스킬 초기화
            uiCentralLabSkill.Hide();

            BattleTime.IsPause = false; // 일시정지 해제
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            cameraController.AddMask(Layer.ENEMY); // 카메라 레이어 설정

            // 스킬 세팅
            clonePlayer.Skill.Initialize((SkillModel.ISkillValue[])skillList.ToArray());
            clonePlayer.Skill.Initialize((SkillModel.ISlotValue[])skillList.ToArray());

            AddBossEvent();

            if (bossEntity)
            {
                uiBattleBossHp.Show(
                    bossEntity.battleUnitInfo.Level,
                    bossEntity.GetName(),
                    bossEntity.battleUnitInfo.UnitElementType,
                    UIBattleBossHp.MonsterType.NormalBoss,
                    (int)bossEntity.battleUnitInfo.UnitSizeType,
                    bossHud.CachedGameObject,
                    0);
                uiBattleBossHp.SetHp(bossEntity.CurHP, bossEntity.MaxHP);
            }
            else
            {
                uiBattleBossHp.Hide();
            }

            if (waveIndex == 0)
            {
                UnitActor actor = clonePlayer.GetActor();
                if (actor)
                    actor.EffectPlayer.ShowUnitCircle();
            }
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity is MonsterEntity monsterEntity)
            {
                ShowRemainMonsterCount(--monsterRemainCount, monsterMaxCount); // 죽여야 할 몬스터 수 감소
                dungeonModel.RequestCentralLabMonsterKill(labId, monsterEntity.CentralLabMonId, monsterEntity.damagePacket).WrapNetworkErrors();
            }
            else
            {
                EndCloneAI(); // AI 종료
                dungeonModel.RequestCentralLabExit().WrapNetworkErrors();
            }
        }

        void OnCentralLabMonsterKill(RewardData[] rewards, int itemSkillPoint, int itemSkillId, CentralLabSkillPacket[] selectSkills)
        {
            uiBattleCentralLab.SetPoint(itemSkillPoint, MAX_ITEM_SKILL_POINT);

            // Clear
            if (rewards != null)
            {
                ShowResultClear(rewards);
                return;
            }

            if (itemSkillId > 0)
            {
                this.itemSkillId = itemSkillId;
            }

            if (selectSkills != null)
            {
                foreach (var item in selectSkills)
                {
                    item.Initialize(skillDataRepo);
                }

                this.selectSkills = selectSkills;

                NextWaveStart(); // 다음 웨이브 시작
            }
        }

        void OnUseSkill(UnitEntity target, SkillInfo skillInfo)
        {
            // 아이템 스킬은 따로 처리
            if (skillInfo is ItemSkill)
                return;

            // 평타는 쿨타임 체크를 하지 않는다.
            if (skillInfo.IsBasicActiveSkill)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", skillInfo.SkillId);
            skillInfo.SetResponseCooldownCheckState(); // 쿨타임 대기 상태
            Protocol.REQUEST_CLAB_SKILL_COOLTIME_CHECK.SendAsync(sfs).WrapNetworkErrors();
        }

        void OnRequestSkillCooltimeCheck(Response response)
        {
            if (!response.isSuccess)
            {
                if (Issue.SHOW_SKILL_COOLTIME_CHECK_ERROR)
                    response.ShowResultCode();
                return;
            }

            long remainCoolTime = response.GetLong("1");
            int skillId = response.GetInt("2");
            long skillNo = skillId; // 중앙실험실은 고유값이 id와 동일

            SkillInfo info = clonePlayer.Skill.GetSkill(skillNo, isBattleSkill: true);

#if UNITY_EDITOR
            if (DebugUtils.IsLogSkillCoolTime)
            {
                Debug.LogError($"서버가 생각하는 쿨타임: {remainCoolTime}");
            }
#endif

            if (info == null)
            {
#if UNITY_EDITOR
                Debug.LogError("존재하지 않은 스킬"); 
#endif
                return;
            }

            // 쿨타임 세팅
            info.StartCooldown(remainCoolTime);
        }

        void OnStartController()
        {
            if (clonePlayer.IsDie)
                return;

            UnitActor actor = clonePlayer.GetActor();
            if (actor == null)
                return;

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

            if (clonePlayer.IsDie)
                return;

            if (clonePlayer.battleCrowdControlInfo.GetCannotMove())
                return;

            UnitActor actor = clonePlayer.GetActor();
            if (actor == null)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);
        }

        void OnResetController()
        {
            if (clonePlayer.IsDie)
                return;

            UnitActor actor = clonePlayer.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();
        }

        void OnSelectSkill(int skillId)
        {
            RequestSelectSkill(skillId).WrapNetworkErrors();
        }

        void OnSelectItemSkill(int itemSkillId)
        {
            // 아이템 스킬 사용
            if (!UseItemSkill(itemSkillId))
                return;

            RequestUseItemSkill(itemSkillId).WrapNetworkErrors();
        }

        void OnChangedBossHp(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }

        void OnDeadBoss(UnitEntity unit, UnitEntity attacker)
        {
            uiBattleBossHp.Hide();
            RemoveBossEvent();
        }

        /// <summary>
        /// 아이템스킬 UI 활성화 여부
        /// </summary>
        private bool IsShowItemSkillUI()
        {
            return uiCentralLabItemSkill.IsVisible;
        }

        /// <summary>
        /// 스킬 선택 완료 여부
        /// </summary>
        private bool IsFinishedSelectSkill()
        {
            return isFinishedSelectSkill;
        }

        /// <summary>
        /// 최대 몬스터 세팅
        /// </summary>
        private void SetMonsterMaxCount(int value)
        {
            monsterMaxCount = value; // 최대 몬스터 세팅
            monsterRemainCount = value; // 몬스터 죽인 수 초기화

            ShowRemainMonsterCount(monsterRemainCount, monsterMaxCount);
        }

        /// <summary>
        /// Wave에 해당하는 몬스터 타입 반환
        /// </summary>
        private MonsterType GetMonsterType(int index)
        {
            if (index < 0 || index >= arrMonsterType.Length)
                return default;

            return arrMonsterType[index];
        }

        /// <summary>
        /// 남은 몬스터 수 보여주기
        /// </summary>
        private void ShowRemainMonsterCount(int remainCount, int maxCount)
        {
            uiBattleMonsterCount.UpdateMonsterCount(remainCount, maxCount);
        }

        /// <summary>
        /// 아이템 스킬 업데이트
        /// </summary>
        private void UpdateItemSkill()
        {
            uiBattleCentralLab.SetItemSkills(itemSkillList.ToArray());
        }

        /// <summary>
        /// 다음 웨이브 시작
        /// </summary>
        private void NextWaveStart()
        {
            ++waveIndex;
            Ready();
        }

        /// <summary>
        /// 게임 포기
        /// </summary>
        private void GiveUp()
        {
            clonePlayer.Die(null);
        }

        /// <summary>
        /// AI 종료
        /// </summary>
        private void EndCloneAI()
        {
            UnitActor actor = clonePlayer.GetActor();
            if (actor)
                actor.AI.EndAI();
        }

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        private async void GamePause()
        {
            // 이미 일시 정지 중
            if (BattleTime.IsPause)
                return;

            BattleTime.IsPause = true; // 일시정지

            try
            {
                UICentralLabPause.SelectResult result = await UI.Show<UICentralLabPause>().Show(skillList.ToArray());
                if (result == UICentralLabPause.SelectResult.Exit)
                    GiveUp();
            }
            catch (System.Exception exception)
            {
                if (exception is UIException uiException)
                {
                    uiException.Execute();
                }
                else
                {
                    string description = exception.Message;
                    UI.ConfirmPopup(description);
                }
            }

            BattleTime.IsPause = false; // 일시정지 해제
        }

        /// <summary>
        /// 아이템 스킬 사용
        /// </summary>
        private bool UseItemSkill(int itemSkillId)
        {
            // 캐릭터 죽어있음
            if (clonePlayer == null || clonePlayer.IsDie)
                return false;

            // Actor가 존재하지 않음
            UnitActor actor = clonePlayer.GetActor();
            if (actor == null)
                return false;

            // 선택한 아이디 존재하지 않음
            int index = GetItemSkillIndex(itemSkillId);
            if (index == -1)
                return false;

            SkillInfo info = itemSkillList[index];
            actor.UseItemSkill(info); // 아이템 사용
            UI.ShowToastPopup(info.SkillDescription); // 아이템 설명 표시

            itemSkillList.RemoveAt(index); // 사용한 아이템 제거
            UpdateItemSkill(); // 아이템 스킬 업데이트
            return true;
        }

        /// <summary>
        /// 아이템 스킬 사용
        /// </summary>
        private int GetItemSkillIndex(int itemSkillId)
        {
            for (int i = 0; i < itemSkillList.size; i++)
            {
                if (itemSkillList[i].SkillId == itemSkillId)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 보스 이벤트 연결
        /// </summary>
        private void AddBossEvent()
        {
            if (bossEntity == null)
                return;

            bossEntity.OnChangeHP += OnChangedBossHp;
            bossEntity.OnDie += OnDeadBoss;
        }

        /// <summary>
        /// 보스 이벤트 해제
        /// </summary>
        private void RemoveBossEvent()
        {
            if (bossEntity == null)
                return;

            bossEntity.OnChangeHP -= OnChangedBossHp;
            bossEntity.OnDie -= OnDeadBoss;
            bossEntity = null;
        }

        private void ShowResultClear(RewardData[] rewards)
        {
            AsyncShowResult(rewards, isClear: true).WrapUIErrors();
        }

        private void ShowResultFail(RewardData[] rewards)
        {
            AsyncShowResult(rewards, isClear: false).WrapUIErrors();
        }

        private async Task RequestSelectSkill(int skillId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", skillId);
            Response response = await Protocol.REQUEST_CLAB_SELECT_SKILL.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CentralLabSkillPacket[] arrSkillPacket = response.GetPacketArray<CentralLabSkillPacket>("1");

            // 배운 스킬 세팅
            skillList.Clear();
            foreach (var item in arrSkillPacket)
            {
                item.Initialize(skillDataRepo);
                skillList.Add(item);
            }

            isFinishedSelectSkill = true;
        }

        private async Task RequestUseItemSkill(int itemSkillId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", itemSkillId);
            Response response = await Protocol.REQUEST_CLAB_USEITEM.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            int[] arrItemSkillIds = response.GetIntArray("1"); // 남은 아이템 스킬 아이디 목록
        }

        private async Task AsyncShowResult(RewardData[] rewards, bool isClear)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(3f));
            string message;

            if (isClear)
            {
                message = LocalizeKey._48323.ToText() // LEVEL {VALUE} 클리어를 축하드립니다!
                    .Replace(ReplaceKey.VALUE, curDataIndex + 1);
            }
            else
            {
                message = LocalizeKey._48324.ToText(); // 다음에 다시 도전해보아요!
            }

            uiResultDungeon.Show(rewards, DungeonType.CentralLab, isRetry: false, isClear ? UIResultDungeon.TitleType.Clear : UIResultDungeon.TitleType.Failed, message);
        }

        /// <summary>
        /// 클론 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnClonePlayer()
        {
            clonePlayer.Character.Initialize(cloneSettings);
            clonePlayer.Status.Initialize(cloneSettings);
            clonePlayer.Inventory.Initialize(cloneSettings.ItemStatusValue, cloneSettings.WeaponItemId, cloneSettings.ArmorItemId, cloneSettings.WeaponChangedElement, cloneSettings.WeaponElementLevel, cloneSettings.ArmorChangedElement, cloneSettings.ArmorElementLevel, cloneSettings.GetEquippedItems);

            UnitActor cloneActor;
            if (impl.Add(clonePlayer, isEnemy: false))
            {
                clonePlayer.SetExtraOption(ExtraBattleOptionType.PlusMoveSpd, PLAYER_PLUS_MOVE_SPEED);

                cloneActor = clonePlayer.SpawnActor(); // 유닛 소환

                cameraController.SetPlayer(cloneActor.CachedTransform); // 카메라 타겟 세팅

                Vector3 playerPosition = map.GetPlayerPosition();
                cloneActor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster(CentralLabMonsterData[] arrMonsterData)
        {
            foreach (var item in arrMonsterData)
            {
                int monsterId = item.monster_id;
                if (monsterId == 0)
                    continue;

                int monsterLevel = item.monster_level;
                MonsterType monsterType = item.monster_type.ToEnum<MonsterType>();
                float scale = item.GetMonsterScale();
                MonsterEntity entity = MonsterEntity.Factory.CreateMonster(monsterType, monsterId, monsterLevel, scale);
                entity.SetCentralLabId(item.id);

                // 추가 실패 (이미 등록 되어있음)
                if (!impl.Add(entity, isEnemy: true))
                    continue;

                UnitActor monsterActor = entity.SpawnActor(); // 유닛 소환
                monsterActor.AI.SetHomePosition(item.position, isWarp: true); // 위치 세팅
                monsterActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)
                yield return Timing.WaitForOneFrame;

                if (monsterType == MonsterType.Boss)
                {
                    bossEntity = entity;
                    bossHud = monsterActor.EffectPlayer.SetHpHudTarget(entity.GetHudOffset());
                }
            }
        }

        private class CloneCharacterSettings : IMultiPlayerInput, BattleItemInfo.IValue
        {
            public bool IsExceptEquippedItems => true; // 장착아이템 옵션 제외 (더미)

            public int Cid { get; private set; }
            public string Name { get; private set; }
            public byte Job { get; private set; }
            public byte Gender { get; private set; }
            public int Level { get; private set; }
            public int LevelExp => 0;
            public int JobLevel { get; private set; }
            public long JobLevelExp => 0;
            public int RebirthCount => 0;
            public int RebirthAccrueCount => 0;
            public int NameChangeCount => 0;
            public string CidHex => string.Empty;
            public int ProfileId { get; private set; }

            public int Str { get; private set; }
            public int Agi { get; private set; }
            public int Vit { get; private set; }
            public int Int { get; private set; }
            public int Dex { get; private set; }
            public int Luk { get; private set; }
            public int StatPoint => 0;

            public int WeaponItemId { get; private set; }
            public BattleItemInfo.IValue ItemStatusValue => this;
            public int ArmorItemId => 0;
            public ElementType WeaponChangedElement => ElementType.None;
            public int WeaponElementLevel => 0;
            public ElementType ArmorChangedElement => ElementType.None;
            public int ArmorElementLevel => 0;
            public int TotalItemAtk => 0;
            public int TotalItemMatk => 0;
            public int TotalItemDef => 0;
            public int TotalItemMdef => 0;

            public SkillModel.ISkillValue[] Skills => null;
            public SkillModel.ISlotValue[] Slots => null;

            public CupetListModel.IInputValue[] Cupets => null;

            public IBattleOption[] BattleOptions => null;
            public IBattleOption[] GuildBattleOptions => null;

            public int GuildId => -1;
            public string GuildName => string.Empty;
            public int GuildEmblem => 0;
            public byte GuildPosition => 0;
            public int GuildCoin => 0;
            public int GuildQuestRewardCount => 0;
            public long GuildSkillBuyDateTime => 0L;
            public byte GuildSkillBuyCount => 0;
            public long GuildRejoinTime => 0L;

            public float PosX => 0f;
            public float PosY => 0f;
            public float PosZ => 0f;
            public string CostumeName => string.Empty;
            public byte State => 0;
            string TradeModel.IInputValue.PrivateStoreComment => string.Empty;
            PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => default;
            int IMultiPlayerInput.UID => 0;
            bool IMultiPlayerInput.HasMaxHp => false;
            int IMultiPlayerInput.MaxHp => 0;
            bool IMultiPlayerInput.HasCurHp => false;
            int IMultiPlayerInput.CurHp => 0;
            byte IMultiPlayerInput.TeamIndex => 0;
            public ItemInfo.IEquippedItemValue[] GetEquippedItems => null;
            public int[] EquipCostumeIds => null;

            public void Initialize(int cid, string name, Job job, Gender gender, int level, int profileId, CentralLabCharacterPacket packet)
            {
                Cid = cid;
                Name = name;
                Job = (byte)job;
                Gender = (byte)gender;
                Level = level;
                JobLevel = level;
                ProfileId = profileId;

                Str = packet.str;
                Agi = packet.agi;
                Vit = packet.vit;
                Int = packet.@int;
                Dex = packet.dex;
                Luk = packet.luk;
                WeaponItemId = packet.weaponId;
            }

            DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
            {
                return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, Cid, Level);
            }
        }
    }
}