//#define SPAWN_AGENT_CHARACTER
using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MultiBossMazeEntry : BattleEntry
    {
        private const float UNIT_CAMERA_RADIUS = 5f;
        private const float BOSS_CAMERA_RADIUS = 5f;
        private const float SKILL_CAMERA_RADIUS = 3f;

        private const float SKILL_CAMERA_DURATION = 1.5f;
        private const float SKILL_CAMERA_FADE_OVER_DURATION = 0.25f;
        private const float SKILL_CAMERA_FADE_OUT_DURATION = 1.5f;

        private const string TAG_SKILL_CAMERA = nameof(MultiBossMazeEntry) + "_TAG_SKILL_CAMERA";

        private const CameraController.View defaultView = CameraController.View.GroupViewFront;
        private const CameraController.View skillView = CameraController.View.GroupViewSkill;

        /******************** Models ********************/
#if SPAWN_AGENT_CHARACTER
        private readonly CharacterModel characterModel;
        private readonly StatusModel statusModel;
        private readonly AgentModel agentModel;
#else
        private readonly SharingModel sharingModel;
#endif
        private readonly DungeonModel dungeonModel;
        private readonly SkillModel skillModel;

        /******************** Repositories ********************/
#if SPAWN_AGENT_CHARACTER
        private readonly AgentDataManager agentDataRepo;
        private readonly JobDataManager jobDataRepo;
#endif
        private readonly MultiMazeDataManager multiMazeDataRepo;

        /******************** Scene Compositions ********************/
        private readonly GhostBattlePlayers multiBattlePlayers;
        private readonly List<UnitActor> meleeUnitList;
        private readonly List<UnitActor> rangedUnitList;
        private readonly Buffer<CharacterEntity> agentBuffer;
        private readonly long remainTime; // 제한 시간(밀리초)

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIResultClear uiResultClear;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattleMazeAgentList uiBattleMazeAgentList;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleSkillList uiBattleSkillList;
        private UIMvpTimer uiMvpTimer;

        /******************** Temp Data ********************/
        private static int playerHp; // 플레이어 Hp
        private static int bossHp; // 보스 Hp

        private MultiMazeData currentData;
        private Map map;
        private UnitEntity bossEntity; // 보스
        private bool isRequestBattleEnd;
        private bool isShowResult;

        public MultiBossMazeEntry() : base(BattleMode.MultiBossMaze)
        {
#if SPAWN_AGENT_CHARACTER
            agentModel = player.Agent;
            characterModel = player.Character;
            statusModel = player.Status;
#else
            sharingModel = player.Sharing;
#endif
            dungeonModel = player.Dungeon;
            skillModel = player.Skill;

#if SPAWN_AGENT_CHARACTER
            agentDataRepo = AgentDataManager.Instance;
            jobDataRepo = JobDataManager.Instance;
#endif
            multiMazeDataRepo = MultiMazeDataManager.Instance;

            multiBattlePlayers = new GhostBattlePlayers();
            meleeUnitList = new List<UnitActor>();
            rangedUnitList = new List<UnitActor>();
            agentBuffer = new Buffer<CharacterEntity>();
            remainTime = BasisType.WORLD_BOSS_BATTLE_TIME.GetInt();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            cameraController.ShowSkillCamera(false);

            map = null;
            currentData = null;
            multiBattlePlayers.Clear();
            meleeUnitList.Clear();
            rangedUnitList.Clear();
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

            dungeonModel.OnMonsterItemDrop += OnMonsterItemDrop;
            dungeonModel.OnMultiJoinRewardEvent += OnMultiJoinRewardEvent;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            dungeonModel.OnMonsterItemDrop -= OnMonsterItemDrop;
            dungeonModel.OnMultiJoinRewardEvent -= OnMultiJoinRewardEvent;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiResultClear = UI.Show<UIResultClear>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiBattleMazeAgentList = UI.Show<UIBattleMazeAgentList>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleSkillList = UI.Show<UIBattleSkillList>();
            uiMvpTimer = UI.Show<UIMvpTimer>();

            uiBattleMenu.OnExit += GiveUp;
            uiResultClear.OnRetryDungeon += OnRetryDungeon;
            uiResultClear.OnFinishDungeon += OnFinishDungeon;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnToggleSkill;
            uiBattleSkillList.OnSelect += OnSelectSkill;
            uiBattleSkillList.OnToggleSkill += OnToggleSkill;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;

            uiBattleBossHp.Hide();

            cameraController.SetView(defaultView);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleMazeSkillList.Hide();
            uiMvpTimer.Hide();
            uiResultClear.Hide();
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiBattleMenu.OnExit -= GiveUp;
            uiResultClear.OnRetryDungeon -= OnRetryDungeon;
            uiResultClear.OnFinishDungeon -= OnFinishDungeon;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnToggleSkill;
            uiBattleSkillList.OnSelect -= OnSelectSkill;
            uiBattleSkillList.OnToggleSkill -= OnToggleSkill;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattleMazeAgentList>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleSkillList>();
            UI.Close<UIMvpTimer>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiResultClear = null;
            uiBattleMazeSkillList = null;
            uiBattleBossHp = null;
            uiBattleMazeAgentList = null;
            uiBattlePlayerStatus = null;
            uiBattleSkillList = null;
            uiMvpTimer = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
        }

        public override void OnBack()
        {
            if (uiResultClear.IsVisible)
            {
                uiResultClear.OnClickedBtnConfirm();
                return;
            }

            GiveUp();
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
        }

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            currentData = multiMazeDataRepo.Get(id);

            if (currentData == null)
            {
                IsSuccessEnter = false;
                yield break;
            }
            IsSuccessEnter = true;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = currentData.boss_battle_scene_name; // 씬 이름
            string bgmName = currentData.boss_battle_bgm; // 배경음 이름
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
            uiBattleMazeSkillList.SetCharacter(player);
            uiBattleSkillList.SetCharacter(player);

            player.SetCurrentMp(player.MaxMp);

            uiMvpTimer.Initialize(remainTime);
            uiMvpTimer.Show();
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
            yield return Timing.WaitUntilDone(YieldSpawnCharacter(player), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnAgentCharacter(), TAG);
            SetHomePosition();
            yield return Timing.WaitUntilDone(YieldSpawnBossMonster(), TAG);
            SetDestination();
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCharacter(CharacterEntity character)
        {
            if (impl.Add(character, isEnemy: false))
            {
                UnitActor characterActor = character.SpawnActor(); // 유닛 소환

                player.OnUseSkill += OnPlayerUseSkill;
                player.OnDespawnActor += OnPlayerDespawnActor;

                player.Skill.SetAntiSkillAuto(true); // 자동스킬 해제

                if (character.battleSkillInfo.basicActiveSkill.AttackType == AttackType.MeleeAttack)
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
#if SPAWN_AGENT_CHARACTER
            int jobLevel = characterModel.JobLevel;
            int totalStatPoint = statusModel.TotalStatusPoint;

            foreach (var agent in agentModel.GetEquipedCombatAgents())
            {
                if (agent.ID == 0)
                    continue;

                AgentData agentData = agentDataRepo.Get(agent.ID);
                if (agentData == null)
                {
                    Debug.LogError($"AgentData가 존재하지 않습니다: {nameof(agent.ID)} = {agent.ID}");
                    continue;
                }

                int jobId = agentData.job_id;
                JobData agentJobData = jobDataRepo.Get(jobId);

                if (agentJobData == null)
                {
                    Debug.LogError($"JobData가 존재하지 않습니다: {nameof(jobId)} = {jobId}");
                    continue;
                }

                AgentCharacterPacket packet = new AgentCharacterPacket(jobId, agentData.gender, agentData.name_id, agentData.weapon_id, agentData.skill_id_1, agentData.skill_id_2, agentData.skill_id_3, agentData.skill_id_4);
                short[] plusGuidStats = agentJobData.GetAutoStatGuidePoints(0, 0, 0, 0, 0, 0, totalStatPoint);
                packet.UpdateStatus(jobLevel, plusGuidStats[0], plusGuidStats[1], plusGuidStats[2], plusGuidStats[3], plusGuidStats[4], plusGuidStats[5]);

                multiBattlePlayers.AddGhostPlayer(packet, DamagePacket.DamageUnitType.Agent, agent.ID, jobLevel);
            }
#else
            int index = 0;
            int multiPlayerSize = 0;
            foreach (IMultiPlayerInput item in sharingModel.GetSharingCharacters())
            {
                if (item == null)
                    continue;

                multiBattlePlayers.AddGhostPlayer(item);
                ++multiPlayerSize;
            }
#endif
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

        /// <summary>
        /// 보스 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnBossMonster()
        {
            bossEntity = factory.CreateBossMonster(currentData);
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
            yield return Timing.WaitUntilTrue(CheckIfAllUnitArrived);
        }

        private bool CheckIfAllUnitArrived()
        {
            for (int i = 0; i < meleeUnitList.Count; ++i)
                if (!meleeUnitList[i].Movement.IsStopped)
                    return false;

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

            player.SetCurrentHp(playerHp); // 플레이어 hp 세팅
            bossEntity.SetCurrentHp(bossHp); // 보스 Hp 세팅

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

            MonsterData bossData = MonsterDataManager.Instance.Get(currentData.boss_monster_id);

            float hudOffset = bossData.GetHudOffset();
            PoolObject bossHud = bossActor.EffectPlayer.SetHpHudTarget(new Vector3(0, hudOffset, 0));

            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.SetOffset(UIBattleBossHp.Offset.BossMonster);
            uiBattleBossHp.Show(
                currentData.boss_monster_level,
                bossData.name_id.ToText(),
                bossData.element_type.ToEnum<ElementType>(),
                UIBattleBossHp.MonsterType.NormalBoss,
                bossData.cost,
                bossHud.CachedGameObject);
            uiBattleBossHp.SetHp(bossHp, bossActor.Entity.MaxHP);

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
                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
            }

            if (unitEntity.type == UnitEntityType.BossMonster) // 보스 몬스터 사망
            {
                impl.PauseAllUnit();
                RequestBattleEnd(isWin: true, unitEntity.clientUID);
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

        private void RequestBattleEnd(bool isWin, int clientMonsterId = 0)
        {
            if (isRequestBattleEnd)
                return;

            isRequestBattleEnd = true;
            dungeonModel.RequestMultiMazeBossBattleEnd(isWin, bossEntity.CurHP, clientMonsterId, currentData.id, currentData.chapter);
        }

        void OnMonsterItemDrop(UnitEntity unit, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            if (droppedZeny > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.Zeny), droppedZeny, rewardCount: 0, rewardOption: 0));

            if (droppedLevelExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.LevelExp), droppedLevelExp, rewardCount: 0, rewardOption: 0));

            if (droppedJobExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.JobExp), droppedJobExp, rewardCount: 0, rewardOption: 0));

            if (rewards != null)
                rewardBuffer.AddRange(rewards);

            ShowResultUI(UIResultClear.ResultType.Clear);
        }

        private void OnPlayerDespawnActor(UnitActor obj)
        {
            player.OnDespawnActor -= OnPlayerDespawnActor;
            player.OnUseSkill -= OnPlayerUseSkill;
            player.Skill.SetAntiSkillAuto(false);
        }

        private void OnPlayerUseSkill(UnitEntity target, SkillInfo skillInfo)
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

        void OnMultiJoinRewardEvent(int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            if (droppedZeny > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.Zeny), droppedZeny, rewardCount: 0, rewardOption: 0));

            if (droppedLevelExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.LevelExp), droppedLevelExp, rewardCount: 0, rewardOption: 0));

            if (droppedJobExp > 0)
                rewardBuffer.Add(new RewardData((int)(RewardType.JobExp), droppedJobExp, rewardCount: 0, rewardOption: 0));

            if (rewards != null)
                rewardBuffer.AddRange(rewards);

            ShowResultUI(UIResultClear.ResultType.Result);
        }

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

        void OnRetryDungeon()
        {
            ExitMultiMaze();
        }

        void OnFinishDungeon()
        {
            ExitMultiMaze();
        }

        /// <summary>
        /// 멀티 미로 퇴장
        /// </summary>
        private void ExitMultiMaze()
        {
            impl.StartBattle(BattleMode.MultiMazeLobby);
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

        /// <summary>
        /// 결과창 띄우기
        /// </summary>
        private void ShowResultUI(UIResultClear.ResultType resultType)
        {
            ShowResultUIAsync(resultType).WrapNetworkErrors();
        }

        private async Task ShowResultUIAsync(UIResultClear.ResultType resultType)
        {
            isShowResult = true;

            if (resultType == UIResultClear.ResultType.Clear)
            {
                uiMvpTimer.StopTimer();
                uiMvpTimer.Hide();
            }

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

            uiResultClear.Show(buffer, DungeonType.MultiMaze, false, resultType, showRewardFX: true);
            uiResultClear.SetDescription(string.Empty);
        }

        void OnFinishedRemainTimer()
        {
            EndUnitAI();
            AsyncTimeOver().WrapErrors();
        }

        private async Task AsyncTimeOver()
        {
            await Task.Delay(System.TimeSpan.FromSeconds(1f));

            // 서버로 GiveUp 보내기 전에 서버에서 전투 종료가 날아올 수 있음
            if (isShowResult)
                return;

            RequestBattleEnd(isWin: false);
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

        public static void SetPlayerHp(int playerHp)
        {
            MultiBossMazeEntry.playerHp = playerHp;
        }

        public static void SetBossHp(int bossHp)
        {
            MultiBossMazeEntry.bossHp = Mathf.Max(1, bossHp); // 서버 bossHp 0 일 때의 방어코드
        }
    }
}