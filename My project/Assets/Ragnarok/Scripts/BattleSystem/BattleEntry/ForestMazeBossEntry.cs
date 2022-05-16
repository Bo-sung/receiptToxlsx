using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ForestMazeBossEntry : BattleEntry
    {
        private const float UNIT_CAMERA_RADIUS = 5f;
        private const float BOSS_CAMERA_RADIUS = 5f;
        private const float SKILL_CAMERA_RADIUS = 3f;

        private const float SKILL_CAMERA_DURATION = 1.5f;
        private const float SKILL_CAMERA_FADE_OVER_DURATION = 0.25f;
        private const float SKILL_CAMERA_FADE_OUT_DURATION = 1.5f;

        private const string TAG_SKILL_CAMERA = nameof(ForestMazeBossEntry) + "_TAG_SKILL_CAMERA";

        private const CameraController.View defaultView = CameraController.View.GroupViewFront;
        private const CameraController.View skillView = CameraController.View.GroupViewSkill;

        // <!-- Models --!>
        private readonly SharingModel sharingModel;
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;

        // <!-- Managers --!>
        private readonly ForestMazeManager forestMazeManager;

        // <!-- Scene Compositions --!>
        private readonly GhostBattlePlayers multiBattlePlayers;
        private readonly List<UnitActor> meleeUnitList;
        private readonly List<UnitActor> rangedUnitList;
        private readonly Buffer<CharacterEntity> agentBuffer;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIResultClear uiResultClear;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattleMazeAgentList uiBattleMazeAgentList;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIMvpTimer uiMvpTimer;
        private UIBattleGuildAttackSkillList uiBattleGuildAttackSkillList;
        private UIBattleToastPopup uiBattleToastPopup;
        private UIBattlePotion uiBattlePotion;
        private UIForestMazeSkill uiForestMazeSkill;

        // <!-- Temp Data --!>
        private Map map;
        private int monsterIndex; // 보스 인덱스
        private UnitEntity bossEntity; // 보스
        private bool isRequestBattleEnd;
        private bool isBoss; // 보스여부 (false 시 중간보스)
        private bool isPlayBattleToastPopup;
        private SkillInfo autoGuardSkill;

        public ForestMazeBossEntry() : base(BattleMode.ForestMazeBoss)
        {
            sharingModel = player.Sharing;
            dungeonModel = player.Dungeon;
            skillDataRepo = SkillDataManager.Instance;
            forestMazeManager = ForestMazeManager.Instance;

            multiBattlePlayers = new GhostBattlePlayers();
            meleeUnitList = new List<UnitActor>();
            rangedUnitList = new List<UnitActor>();
            agentBuffer = new Buffer<CharacterEntity>();
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

        /// <summary>
        /// 코루틴 중지
        /// </summary>
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

            forestMazeManager.OnUpdateState += OnUpdateState;

            forestMazeManager.OnSelectReward += OnSelectReward;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnUseSkill -= OnPlayerUseSkill;
            player.OnDespawnActor -= OnPlayerDespawnActor;

            forestMazeManager.OnUpdateState -= OnUpdateState;

            forestMazeManager.OnSelectReward -= OnSelectReward;
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
            uiBattleMazeAgentList = UI.Show<UIBattleMazeAgentList>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBattleGuildAttackSkillList = UI.Show<UIBattleGuildAttackSkillList>();
            uiBattleToastPopup = UI.Show<UIBattleToastPopup>();
            uiBattlePotion = UI.Show<UIBattlePotion>();
            uiForestMazeSkill = UI.Show<UIForestMazeSkill>();

            uiBattleMenu.OnExit += GiveUp;
            uiResultClear.OnFinishDungeon += ExitEntry;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;
            uiBattleGuildAttackSkillList.OnSelect += OnSelectSkill;
            uiBattleToastPopup.OnFinished += OnFinishedBattleToast;
            uiBattlePotion.OnSelect += OnSelectPotion;
            uiForestMazeSkill.OnSelect += OnSelectReward;

            uiBattleBossHp.Hide();
            cameraController.SetView(defaultView);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiResultClear.Hide();
            uiBattlePotion.SetMode();
            uiForestMazeSkill.Hide();
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
            uiBattleToastPopup.OnFinished -= OnFinishedBattleToast;
            uiBattlePotion.OnSelect -= OnSelectPotion;
            uiForestMazeSkill.OnSelect -= OnSelectReward;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattleMazeAgentList>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBattleGuildAttackSkillList>();
            UI.Close<UIBattleToastPopup>();
            UI.Close<UIBattlePotion>();
            UI.Close<UIForestMazeSkill>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiResultClear = null;
            uiBattleBossHp = null;
            uiBattleMazeAgentList = null;
            uiBattlePlayerStatus = null;
            uiMvpTimer = null;
            uiBattleGuildAttackSkillList = null;
            uiBattleToastPopup = null;
            uiBattlePotion = null;
            uiForestMazeSkill = null;
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
            forestMazeManager.Dispose();
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
            if (!forestMazeManager.IsJoined())
            {
                IsSuccessEnter = false;
                yield break;
            }

            // 전투중인 몬스터가 보스몬스터의 경우
            if (input is MonsterBotEntity entity)
            {
                monsterIndex = entity.BotServerIndex;
                isBoss = monsterIndex == forestMazeManager.BossMonsterIndex;
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

            ForestBaseData data = forestMazeManager.CurrentData;
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
            long bossPlayTime;
            if (isBoss) // 최종 보스
            {
                bossPlayTime = BasisForestMazeInfo.BossPlayTime.GetInt();
                uiMvpTimer.Initialize(bossPlayTime);
            }
            else // 중간 보스
            {
                System.TimeSpan endTimeSpan = forestMazeManager.EndTime - ServerTime.Now;
                float milliseconds = Mathf.Max(0f, (float)endTimeSpan.TotalMilliseconds);
                uiMvpTimer.Initialize((long)milliseconds);
            }

            uiMvpTimer.RestartTimer();
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            cameraController.AddMask(Layer.GHOST);
            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnAgentCharacter(), TAG);
            SetHomePosition();
            yield return Timing.WaitUntilDone(YieldSpawnBossMonster(), TAG);
            SetDestination();

            // 스킬 연출
            if (isBoss)
            {
                if (ShowBattleToastPopup(ForestMazeSkill.SKILL_1))
                {
                    ApplySkill1();
                }

                if (ShowBattleToastPopup(ForestMazeSkill.SKILL_2))
                {
                    ApplySkill2();
                }

                if (ShowBattleToastPopup(ForestMazeSkill.SKILL_3))
                {
                    ApplySkill3();
                }

                if (ShowBattleToastPopup(ForestMazeSkill.SKILL_4))
                {
                    // Do Nothing
                }
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            player.SetExtraOption(ExtraBattleOptionType.MaxHp, forestMazeManager.PlayerMaxHp);
            player.SetCurrentHp(forestMazeManager.PlayerHp); // MaxHp 가 더 커질 경우에는 값이 이상해 보일 수 있음

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

        /// <summary>
        /// 전투 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnAgentCharacter()
        {
            int index = 0;
            int multiPlayerSize = 0;
            foreach (IMultiPlayerInput item in sharingModel.GetSharingCharacters())
            {
                if (item == null)
                    continue;

                multiBattlePlayers.AddGhostPlayer(item);
                ++multiPlayerSize;
            }

            for (; index < multiPlayerSize; index++)
            {
                if (impl.Add(multiBattlePlayers[index], isEnemy: false))
                {
                    UnitActor agentActor = multiBattlePlayers[index].SpawnActor(); // 유닛 소환
                    multiBattlePlayers[index].ReadyToBattle(); // HP 최대 (죽은 것 처럼 보이는 것 방지)
                    agentBuffer.Add(multiBattlePlayers[index]);

                    if (multiBattlePlayers[index].battleSkillInfo.basicActiveSkill.AttackType == AttackType.MeleeAttack)
                    {
                        meleeUnitList.Add(agentActor);
                    }
                    else
                    {
                        rangedUnitList.Add(agentActor);
                    }

                    cameraController.AddMember(agentActor.CachedTransform, UNIT_CAMERA_RADIUS); // 카메라 - 유닛 추가
                    yield return Timing.WaitForOneFrame;
                }
            }

            uiBattleMazeAgentList.SetShareCharacters(agentBuffer.GetBuffer(isAutoRelease: true));

            IBattleSharingCharacter[] cloneCharacters = sharingModel.GetCloneCharacters();
            foreach (IMultiPlayerInput item in cloneCharacters)
            {
                if (item == null)
                    continue;

                multiBattlePlayers.AddGhostPlayer(item);
                ++multiPlayerSize;
            }

            for (; index < multiPlayerSize; index++)
            {
                if (impl.Add(multiBattlePlayers[index], isEnemy: false))
                {
                    UnitActor agentActor = multiBattlePlayers[index].SpawnActor(); // 유닛 소환
                    multiBattlePlayers[index].ReadyToBattle(); // HP 최대 (죽은 것 처럼 보이는 것 방지)
                    agentBuffer.Add(multiBattlePlayers[index]);

                    if (multiBattlePlayers[index].battleSkillInfo.basicActiveSkill.AttackType == AttackType.MeleeAttack)
                    {
                        meleeUnitList.Add(agentActor);
                    }
                    else
                    {
                        rangedUnitList.Add(agentActor);
                    }

                    cameraController.AddMember(agentActor.CachedTransform, UNIT_CAMERA_RADIUS); // 카메라 - 유닛 추가
                    yield return Timing.WaitForOneFrame;
                }
            }

            uiBattleMazeAgentList.SetCloneCharacters(agentBuffer.GetBuffer(isAutoRelease: true));
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

        private bool ShowBattleToastPopup(ForestMazeSkill skill)
        {
            if (!forestMazeManager.HasSkill(skill))
                return false;

            isPlayBattleToastPopup = true;
            uiBattleToastPopup.Play(skill.DungeonSkillIcon, skill.MessageId.ToText());
            return true;
        }

        /// <summary>
        /// 보스 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnBossMonster()
        {
            MonsterBotEntity find = forestMazeManager.FindMonster(monsterIndex); // MonsterBot
            if (find == null)
            {
                Debug.LogError($"보스 음슴: {nameof(monsterIndex)} = {monsterIndex}");
                yield break;
            }

            int bossMonsterId = find.Monster.MonsterID;
            int bossLevel = find.Monster.MonsterLevel;
            float bossScale = find.Scale;

            if (isBoss)
            {
                // 소지한 엠펠리움의 개수만큼 Level 감소
                int levelDecreaseValue = BasisForestMazeInfo.LevelDecreaseValue.GetInt();
                int decreaseLevelValue = (forestMazeManager.EmperiumCount - forestMazeManager.NeedBossBattleEmperiumCount) * levelDecreaseValue;
                bossLevel = Mathf.Max(1, bossLevel - decreaseLevelValue);

                // 악마의 뿔 보유
                if (forestMazeManager.HasSkill(ForestMazeSkill.SKILL_4))
                {
                    bossMonsterId = BasisForestMazeInfo.ChangeBossMonsterId.GetInt();
                }
            }

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
            // 전투팝업이 진행중임
            if (isPlayBattleToastPopup)
                return false;

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

            player.SetCurrentHp(forestMazeManager.PlayerHp);

            UnitActor playerActor = player.GetActor();
            UnitActor bossActor = bossEntity.GetActor();

            if (playerActor == null)
            {
                Debug.LogError("PlayerActor is Null");
                return;
            }

            if (bossActor == null)
            {
                Debug.LogError("BossActor is Null");
                return;
            }

            playerActor.EffectPlayer.ShowUnitCircle();

            // HpRegen 은 사용하지 말 것
            playerActor.AI.StopHpRegen();
            bossActor.AI.StopHpRegen();

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit);

            // 특수스킬 UI 표시
            if (autoGuardSkill != null)
                uiBattlePotion.SetMode(UIBattlePotion.MenuContent.AutoGuard);

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
            switch (forestMazeManager.State)
            {
                case ForestMazeState.None:
                    uiForestMazeSkill.Hide(); // 혹시라도 켜져있을 보상선택 UI Hide

                    UIResultClear.ResultType resultType = forestMazeManager.BattleResult == ForestMazeBattleResult.Succees ? UIResultClear.ResultType.Clear : UIResultClear.ResultType.Result;
                    uiResultClear.Show(forestMazeManager.TotalRewards, DungeonType.ForestMaze, false, resultType);
                    uiResultClear.SetDescription(string.Empty);
                    break;

                case ForestMazeState.Maze:
                    impl.StartBattle(BattleMode.ForestMaze); // 미궁숲 미로로 되돌아가기
                    break;
            }
        }

        void OnSelectReward(int[] rewardIds)
        {
            uiForestMazeSkill.Show(rewardIds, forestMazeManager.GetSelectedRewardIds());
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
            AsyncTimeOver().WrapErrors();
        }

        void OnFinishedBattleToast()
        {
            isPlayBattleToastPopup = false;
        }

        void OnSelectPotion(UIBattlePotion.MenuContent content)
        {
            switch (content)
            {
                case UIBattlePotion.MenuContent.AutoGuard:
                    if (autoGuardSkill == null)
                        return;

                    if (player.IsDie)
                        return;

                    UnitActor actor = player.GetActor();
                    if (actor == null)
                        return;

                    actor.UseItemSkill(autoGuardSkill);
                    break;
            }
        }

        void OnSelectReward(int id)
        {
            // 이미 전투가 종료되었을 수 있다.
            if (forestMazeManager.State == ForestMazeState.None)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id);
            Protocol.REQUEST_FOREST_ITEMSELECT.SendAsync(sfs).WrapNetworkErrors();
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

            RequestBattleEnd(isWin: false);
        }

        private void RequestBattleEnd(bool isWin)
        {
            // 이미 전투가 종료되었을 수 있다.
            if (forestMazeManager.State == ForestMazeState.None)
                return;

            if (isRequestBattleEnd)
                return;

            isRequestBattleEnd = true;

            const byte FLAG_WIN = 1;
            const byte FLAG_LOSE = 2;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", isWin ? FLAG_WIN : FLAG_LOSE);

            if (isBoss)
            {
                Protocol.REQUEST_FOREST_BOSSBATTLE_END.SendAsync(sfs).WrapNetworkErrors();
            }
            else
            {
                int remainHp = Mathf.Max(1, player.CurHP);
                sfs.PutInt("2", remainHp);
                forestMazeManager.SetPlayerHp(remainHp); // 플레이어 hp 세팅
                Protocol.REQUEST_FOREST_MINIBOSSBATTLE_END.SendAsync(sfs).WrapNetworkErrors();
            }
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

        private void ApplySkill1()
        {
            int value = BasisForestMazeInfo.DmgRateResist.GetInt(); // 댐저항
            player.SetExtraOption(ExtraBattleOptionType.DmgRateResist, value);
        }

        private void ApplySkill2()
        {
            int value = BasisForestMazeInfo.DmgRate.GetInt(); // 댐증가
            player.SetExtraOption(ExtraBattleOptionType.DmgRate, value);
        }

        private void ApplySkill3()
        {
            int skillId = BasisForestMazeInfo.UseSkillId.GetInt();
            SkillData data = skillDataRepo.Get(skillId, level: 1);
            if (data == null)
            {
                Debug.LogError($"특수스킬이 존재하지 않습니다: {nameof(skillId)} = {skillId}");
                return;
            }

            autoGuardSkill = new ActiveSkill();
            autoGuardSkill.SetData(data);
        }

        private async Task AsyncTimeOver()
        {
            await Task.Delay(System.TimeSpan.FromSeconds(1f));

            RequestBattleEnd(isWin: false);
        }
    }
}