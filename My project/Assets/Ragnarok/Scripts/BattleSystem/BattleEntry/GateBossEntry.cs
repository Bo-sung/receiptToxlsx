using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GateBossEntry : BattleEntry
    {
        private const float UNIT_CAMERA_RADIUS = 5f;
        private const float BOSS_CAMERA_RADIUS = 5f;
        private const float SKILL_CAMERA_RADIUS = 3f;

        private const float SKILL_CAMERA_DURATION = 1.5f;
        private const float SKILL_CAMERA_FADE_OVER_DURATION = 0.25f;
        private const float SKILL_CAMERA_FADE_OUT_DURATION = 1.5f;

        private const string TAG_SKILL_CAMERA = nameof(GateBossEntry) + "_TAG_SKILL_CAMERA";

        private const CameraController.View defaultView = CameraController.View.GroupViewFront;
        private const CameraController.View skillView = CameraController.View.GroupViewSkill;

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;

        // <!-- Managers --!>
        private readonly GateMazeManager gateMazeManager;

        // <!-- Scene Compositions --!>
        private readonly GhostBattlePlayers multiBattlePlayers;
        private readonly List<UnitActor> meleeUnitList;
        private readonly List<UnitActor> rangedUnitList;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIResultClear uiResultClear;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMvpTimer uiMvpTimer;
        private UIBattleGuildAttackSkillList uiBattleGuildAttackSkillList;

        // <!-- Temp Data --!>
        private Map map;
        private int monsterIndex; // 보스 인덱스
        private UnitEntity bossEntity; // 보스
        private bool isRequestBattleEnd;

        public GateBossEntry() : base(BattleMode.GateBoss)
        {
            skillDataRepo = SkillDataManager.Instance;
            gateMazeManager = GateMazeManager.Instance;

            multiBattlePlayers = new GhostBattlePlayers();
            meleeUnitList = new List<UnitActor>();
            rangedUnitList = new List<UnitActor>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            cameraController.ShowSkillCamera(false);

            map = null;
            multiBattlePlayers.Clear();
            meleeUnitList.Clear();
            rangedUnitList.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.MaxHp); // 강제 Hp 제거
        }

        protected override void StopAllCoroutine()
        {
            base.StopAllCoroutine();

            Timing.KillCoroutines(TAG_SKILL_CAMERA);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnUseSkill += OnPlayerUseSkill;
            player.OnDespawnActor += OnPlayerDespawnActor;

            gateMazeManager.OnUpdateState += OnUpdateState;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnUseSkill -= OnPlayerUseSkill;
            player.OnDespawnActor -= OnPlayerDespawnActor;

            gateMazeManager.OnUpdateState -= OnUpdateState;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiResultClear = UI.Show<UIResultClear>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBattleGuildAttackSkillList = UI.Show<UIBattleGuildAttackSkillList>();

            uiBattleMenu.OnExit += GiveUp;
            uiResultClear.OnFinishDungeon += ExitEntry;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;
            uiBattleGuildAttackSkillList.OnSelect += OnSelectSkill;

            uiBattleBossHp.Hide();
            cameraController.SetView(defaultView);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiResultClear.Hide();
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiBattleMenu.OnExit -= GiveUp;
            uiResultClear.OnFinishDungeon -= ExitEntry;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;
            uiBattleGuildAttackSkillList.OnSelect -= OnSelectSkill;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBattleGuildAttackSkillList>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiResultClear = null;
            uiBattleBossHp = null;
            uiBattlePlayerStatus = null;
            uiMvpTimer = null;
            uiBattleGuildAttackSkillList = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
        }

        public override void OnBack()
        {
            GiveUp();
        }

        /// <summary>
        /// 종료
        /// </summary>
        protected override void ExitEntry()
        {
            gateMazeManager.Dispose();
            impl.StartBattle(BattleMode.MultiMazeLobby); // 멀티로비로 되돌아감
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            if (bossEntity != null)
                bossEntity.OnChangeHP -= OnChangeBossMonsterHp;

            // 보스 폐기
            Dispose(bossEntity);

            // 멀티플레이어 폐기
            foreach (var item in multiBattlePlayers)
            {
                Dispose(item);
            }

            multiBattlePlayers.Recycle();

            // 강제 스탯 제거
            player.ResetExtraOption(ExtraBattleOptionType.DmgRate);
            player.ResetExtraOption(ExtraBattleOptionType.DmgRateResist);
        }

        public override IEnumerator<float> YieldEnterBattle(IBattleInput input)
        {
            // 이미 입장하지 않은 상태
            if (!gateMazeManager.IsJoined())
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 전투중인 몬스터가 보스몬스터의 경우
            if (input is MonsterBotEntity entity)
            {
                monsterIndex = entity.BotServerIndex;
                IsSuccessEnter = true; // 무조건 성공
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
            string sceneName = data.boss_battle_scene_name; // 씬 이름
            string bgmName = data.boss_bgm; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
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

            map.SetPortalActive(isActive: false); // 가로막 없음
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            uiBattlePlayerStatus.SetPlayer(player);

            uiBattleGuildAttackSkillList.SetCharacter(player); // 스킬 조작 UI 업데이트

            // 남은 시간 세팅
            System.TimeSpan endTimeSpan = gateMazeManager.EndTime - ServerTime.Now;
            float milliseconds = Mathf.Max(0f, (float)endTimeSpan.TotalMilliseconds);
            uiMvpTimer.Initialize((long)milliseconds);

            uiMvpTimer.RestartTimer();
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
#endif
                yield break;
            }

            cameraController.AddMask(Layer.GHOST);
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);
            SetHomePosition();
            yield return Timing.WaitUntilDone(YieldSpawnBossMonster(), TAG);
            SetDestination();
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            player.SetExtraOption(ExtraBattleOptionType.MaxHp, gateMazeManager.PlayerMaxHp);
            player.SetCurrentHp(gateMazeManager.PlayerHp); // MaxHp 가 더 커질 경우에는 값이 이상해 보일 수 있음

            if (impl.Add(player, isEnemy: false))
            {
                UnitActor characterActor = player.SpawnActor(); // 유닛 소환

                player.Skill.SetAntiSkillAuto(true); // 자동스킬 해제

                if (player.battleSkillInfo.basicActiveSkill.AttackType == AttackType.MeleeAttack)
                {
                    meleeUnitList.Add(characterActor);
                }
                else
                {
                    rangedUnitList.Add(characterActor);
                }

                cameraController.AddMember(characterActor.CachedTransform, UNIT_CAMERA_RADIUS); // 카메라 - 유닛 추가
                yield return Timing.WaitForOneFrame;
            }
        }

        public override void OnMonsterRequestSpawn(MonsterEntity bossMonster, int spawnID, int spawnCount)
        {
            IEnumerable<UnitEntity> monsters = factory.CreateSpawnMonster(bossMonster, spawnID, spawnCount);

            List<UnitEntity> buffer = new List<UnitEntity>();

            foreach (UnitEntity item in monsters)
            {
                // 추가 실패 (이미 등록 되어있음)
                if (!impl.Add(item, isEnemy: true))
                    continue;

                buffer.Add(item);

                // 현재 스테이지 Id 세팅
                if (item is MonsterEntity monsterEntity)
                    monsterEntity.SetIsBossSpawnMonster(true);
            }

            Vector3[] positions = null;

            for (int i = 0; i < buffer.Count; i++)
            {
                UnitActor monsterActor = buffer[i].SpawnActor(); // 유닛 소환

                if (positions == null)
                    positions = map.GetMonsterPositions(bossMonster.LastPosition, bossMonster.GetActor().Appearance.GetRadius() + 1 + monsterActor.Appearance.GetRadius(), buffer.Count); // 위치 반환

                monsterActor.AI.SetHomePosition(positions[i], isWarp: true); // 위치 세팅
                buffer[i].GetActor().AI.ReadyToBattle();
                buffer[i].ReadyToBattle();
                cameraController.AddMember(buffer[i].GetActor().CachedTransform, radius: BOSS_CAMERA_RADIUS); // 카메라 - 유닛 추가
            }
        }

        private void SetHomePosition()
        {
            Vector3 bossPosition = map.GetBossPosition();
            Vector3 playerPosition = map.GetPlayerPosition();

            Vector3 localY = (bossPosition - playerPosition).normalized;
            Vector3 localX = Vector3.Cross(Vector3.up, localY);
            Vector3 globalYOffset = localY * -6f;

            Vector3 supportLineOffset = localY * -6.5f;
            float intervalInLine = 3f;

            Vector3 basePos = localX * (meleeUnitList.Count - 1) * intervalInLine * -0.5f + playerPosition;

            for (int i = 0; i < meleeUnitList.Count; ++i)
            {
                Vector3 pos = basePos + i * intervalInLine * localX;
                pos += localX * Random.Range(-0.5f, 0.5f) + localY * Random.Range(-2.0f, 0.0f) + globalYOffset;

                meleeUnitList[i].AI.SetHomePosition(pos, isWarp: true); // 위치 세팅
            }

            basePos = localX * (rangedUnitList.Count - 1) * intervalInLine * -0.5f + playerPosition + supportLineOffset;

            for (int i = 0; i < rangedUnitList.Count; ++i)
            {
                Vector3 pos = basePos + i * intervalInLine * localX;
                pos += localX * Random.Range(-0.5f, 0.5f) + localY * Random.Range(-1.0f, 0.0f) + globalYOffset;

                rangedUnitList[i].AI.SetHomePosition(pos, isWarp: true); // 위치 세팅
            }
        }

        private void SetDestination()
        {
            Vector3 bossPosition = map.GetBossPosition();
            Vector3 playerPosition = map.GetPlayerPosition();

            Vector3 localZ = (bossPosition - playerPosition).normalized;
            Vector3 localX = Vector3.Cross(Vector3.up, localZ);

            float radius = 5.5f;
            float intervalInLine = 3f;

            float intervalAngle = Mathf.Atan2(intervalInLine * 0.5f, radius) * 2f * Mathf.Rad2Deg;
            float baseAngle = (meleeUnitList.Count - 1) * intervalAngle * 0.5f;

            for (int i = 0; i < meleeUnitList.Count; ++i)
            {
                Vector3 newLocalZ = Quaternion.Euler(0.0f, baseAngle - i * intervalAngle, 0.0f) * localZ;
                Vector3 relPos = newLocalZ * -radius;
                Vector3 pos = bossPosition + relPos;
                pos += newLocalZ * Random.Range(-1f, 1f);

                meleeUnitList[i].Movement.SetDestination(pos); // 위치 세팅
            }

            intervalAngle = Mathf.Atan2(intervalInLine * 0.5f, radius * 2) * 2f * Mathf.Rad2Deg;
            baseAngle = (rangedUnitList.Count - 1) * intervalAngle * 0.5f;

            for (int i = 0; i < rangedUnitList.Count; ++i)
            {
                Vector3 newLocalZ = Quaternion.Euler(0.0f, baseAngle - i * intervalAngle, 0.0f) * localZ;
                Vector3 relPos = newLocalZ * -(radius * 2);
                Vector3 pos = bossPosition + relPos;
                pos += newLocalZ * Random.Range(-1f, 1f);

                rangedUnitList[i].Movement.SetDestination(pos); // 위치 세팅
            }
        }

        /// <summary>
        /// 보스 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnBossMonster()
        {
            MonsterBotEntity find = gateMazeManager.FindMonster(monsterIndex); // MonsterBot
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"보스 음슴: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                yield break;
            }

            int bossMonsterId = find.Monster.MonsterID;
            int bossLevel = find.Monster.MonsterLevel;
            float bossScale = find.Scale;

            bossEntity = factory.CreateBossMonster(bossMonsterId, bossLevel, bossScale);
            bossEntity.OnChangeHP += OnChangeBossMonsterHp;

            if (impl.Add(bossEntity, isEnemy: true))
            {
                UnitActor bossActor = bossEntity.SpawnActor(); // 유닛 소환
                cameraController.AddMember(bossActor.CachedTransform, radius: BOSS_CAMERA_RADIUS); // 카메라 - 유닛 추가

                Vector3 bossPosition = map.GetBossPosition();
                bossActor.AI.SetHomePosition(bossPosition, isWarp: true); // 위치 세팅
                bossActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)

                yield return Timing.WaitForOneFrame;
            }
        }

        void OnChangeBossMonsterHp(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }

        /// <summary>
        /// 씬 추가 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeSceneAfter()
        {
            yield return Timing.WaitUntilTrue(IsCheckFinished);
        }

        private bool IsCheckFinished()
        {
            // 근거리 캐릭터가 도착하지 않았음
            for (int i = 0; i < meleeUnitList.Count; ++i)
                if (!meleeUnitList[i].Movement.IsStopped)
                    return false;

            // 원거리 캐릭터가 도착하지 않았음
            for (int i = 0; i < rangedUnitList.Count; ++i)
                if (!rangedUnitList[i].Movement.IsStopped)
                    return false;

            return true;
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 씬 변경되면서 미로 상태가 아닐 경우에 대한 처리
            if (gateMazeManager.State != GateMazeState.MiddleBossBattle)
            {
                OnUpdateState();
                return;
            }

            player.SetCurrentHp(gateMazeManager.PlayerHp);

            UnitActor playerActor = player.GetActor();
            UnitActor bossActor = bossEntity.GetActor();

            if (playerActor == null)
            {
#if UNITY_EDITOR
                Debug.LogError("PlayerActor is Null");
#endif
                return;
            }

            if (bossActor == null)
            {
#if UNITY_EDITOR
                Debug.LogError("BossActor is Null");
#endif
                return;
            }

            playerActor.EffectPlayer.ShowUnitCircle();

            // HpRegen 은 사용하지 말 것
            playerActor.AI.StopHpRegen();
            bossActor.AI.StopHpRegen();

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit);

            MonsterEntity bossMonsterEntity = bossEntity as MonsterEntity;
            MonsterData bossData = bossMonsterEntity.Monster.GetMonsterData();
            float hudOffset = bossData.GetHudOffset();
            PoolObject bossHud = bossActor.EffectPlayer.SetHpHudTarget(new Vector3(0, hudOffset, 0));

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.SetOffset(UIBattleBossHp.Offset.BossMonster);
            uiBattleBossHp.Show(
                bossMonsterEntity.Monster.MonsterLevel,
                bossData.name_id.ToText(),
                bossData.element_type.ToEnum<ElementType>(),
                UIBattleBossHp.MonsterType.NormalBoss,
                bossData.cost,
                bossHud.CachedGameObject);

            cameraController.ShowSkillCamera(true);
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity.type == UnitEntityType.PlayerCupet || unitEntity.type == UnitEntityType.NormalMonster)
            {
                UnitActor unitActor = unitEntity.GetActor();

                if (unitActor)
                    cameraController.RemoveMember(unitActor.CachedTransform);
            }

            if (impl.IsPlayerDead())
            {
                uiBattleGuildAttackSkillList.SetCharacter(player);
            }

            if (unitEntity.type == UnitEntityType.BossMonster) // 보스 몬스터 사망
            {
                impl.PauseAllUnit();
                RequestBattleEnd(isWin: true);
                uiBattleBossHp.Hide();
                return;
            }

            // 모든 아군이 죽었을 때
            if (impl.IsAllAlliesDead())
            {
                RequestBattleEnd(isWin: false);
                return;
            }
        }

        #region 플레이어 이벤트

        void OnPlayerDespawnActor(UnitActor obj)
        {
            player.Skill.SetAntiSkillAuto(false);
        }

        void OnPlayerUseSkill(UnitEntity target, SkillInfo skillInfo)
        {
            if (skillInfo.IsBasicActiveSkill)
                return;

            cameraController.ClearMemberSkillGroup();
            cameraController.AddMemberSkillGroup(player?.GetActor()?.CachedTransform, SKILL_CAMERA_RADIUS);
            cameraController.AddMemberSkillGroup(target?.GetActor()?.CachedTransform, SKILL_CAMERA_RADIUS);
            Timing.KillCoroutines(TAG_SKILL_CAMERA);
            Timing.RunCoroutine(YieldZoomSkillCamera(), TAG_SKILL_CAMERA);
        }

        private IEnumerator<float> YieldZoomSkillCamera()
        {
            cameraController.SetView(skillView);
            cameraController.SkillCameraFadeOver(SKILL_CAMERA_FADE_OVER_DURATION);

            yield return Timing.WaitForSeconds(SKILL_CAMERA_DURATION);

            cameraController.SetView(defaultView);
            cameraController.SkillCameraFadeOut(SKILL_CAMERA_FADE_OUT_DURATION);
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

                case GateMazeState.Maze:
                    AsyncStartBattle(BattleMode.GateMaze).WrapNetworkErrors(); // 미궁숲 미로로 되돌아가기
                    break;

                case GateMazeState.WorldBossBattle:
                    AsyncStartBattle(BattleMode.GateWorldBoss).WrapNetworkErrors(); // 월드보스 진행
                    break;
            }
        }

        #endregion

        #region UI 이벤트

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

        void OnFinishedRemainTimer()
        {
            EndUnitAI();
        }

        #endregion

        private void GiveUp()
        {
            AsyncShowGiveUpMessage().WrapNetworkErrors();
        }

        private async Task AsyncShowGiveUpMessage()
        {
            if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                return;

            for (int i = 0; i < unitList.size; i++)
            {
                UnitActor actor = unitList[i].GetActor();
                if (actor == null)
                    continue;

                impl.PauseAllUnit();
            }

            if (!await gateMazeManager.Exit()) // 퇴장
                return;
        }

        private void RequestBattleEnd(bool isWin)
        {
            // 이미 전투가 종료되었을 수 있다.
            if (gateMazeManager.State == GateMazeState.None)
                return;

            if (isRequestBattleEnd)
                return;

            isRequestBattleEnd = true;

            const byte FLAG_WIN = 1;
            const byte FLAG_LOSE = 2;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", isWin ? FLAG_WIN : FLAG_LOSE);
            int remainHp = isWin ? Mathf.Max(1, player.CurHP) : 0;
            sfs.PutInt("2", remainHp);
            gateMazeManager.SetPlayerHp(remainHp); // 플레이어 hp 세팅
            Protocol.REQUEST_GATE_MINIBOSSBATTLE_END.SendAsync(sfs).WrapNetworkErrors();
        }

        private void EndUnitAI()
        {
            foreach (UnitEntity unit in unitList)
            {
                if (unit.IsDie)
                    continue;

                UnitActor actor = unit.GetActor();
                if (actor == null)
                    continue;

                actor.AI.EndAI();
                actor.CancelSkill();
            }

            impl.PauseAllUnit();
        }

        private async Task AsyncStartBattle(BattleMode mode)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(1f));
            impl.StartBattle(mode);
        }
    }
}