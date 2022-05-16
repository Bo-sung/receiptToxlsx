using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GateWorldBossEntry : BattleEntry
    {
        private const CameraController.View defaultView = CameraController.View.GuildWarView;

        // <!-- Managers --!>
        private readonly GateMazeManager gateMazeManager;

        // <!-- Scene Compositions --!>
        private readonly WorldBossPlayerBotEntityPool playerBotPool;
        private readonly BetterList<CharacterEntity> characterList;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIResultClear uiResultClear;
        private UIController uiController;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleMazeAgentList uiBattleMazeAgentList;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIBattleMenu uiBattleMenu;
        private UIBattleSkillList uiBattleSkillList;
        private UIMvpTimer uiMvpTimer;
        private UIBossComing uiBossComing;

        // <!-- Temp Data --!>
        private Map map;
        private UnitEntity bossEntity; // 보스
        private bool isFixedBoss;

        public GateWorldBossEntry() : base(BattleMode.GateWorldBoss)
        {
            player.SetState(UnitEntity.UnitState.Stage);

            gateMazeManager = GateMazeManager.Instance;

            playerBotPool = new WorldBossPlayerBotEntityPool();
            characterList = new BetterList<CharacterEntity>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            cameraController.ShowSkillCamera(false);

            map = null;

            // 플레이어 봇 폐기
            foreach (var item in playerBotPool)
            {
                Dispose(item);
            }

            foreach (var item in characterList)
            {
                item.SetIgnoreTarget(isIgnoreTarget: false);
            }
            characterList.Clear();

            player.ResetExtraOption(ExtraBattleOptionType.MaxHp); // 강제 Hp 제거
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnDespawnActor += OnPlayerDespawnActor;

            gateMazeManager.OnUpdateState += OnUpdateState;
            gateMazeManager.OnMultiPlayerExit += OnMultiPlayerExit;
            gateMazeManager.OnMultiPlayerState += OnMultiPlayerState;
            gateMazeManager.OnWorldBossHp += OnWorldBossHp;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnDespawnActor -= OnPlayerDespawnActor;

            gateMazeManager.OnUpdateState -= OnUpdateState;
            gateMazeManager.OnMultiPlayerExit -= OnMultiPlayerExit;
            gateMazeManager.OnMultiPlayerState -= OnMultiPlayerState;
            gateMazeManager.OnWorldBossHp -= OnWorldBossHp;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiResultClear = UI.Show<UIResultClear>();
            uiController = UI.Show<UIController>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleMazeAgentList = UI.Show<UIBattleMazeAgentList>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleSkillList = UI.Show<UIBattleSkillList>();
            uiMvpTimer = UI.Show<UIMvpTimer>();
            uiBossComing = UI.Show<UIBossComing>();

            uiResultClear.OnFinishDungeon += ExitEntry;
            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnToggleSkill;
            uiBattleMenu.OnExit += GiveUp;
            uiBattleSkillList.OnSelect += OnSelectSkill;
            uiBattleSkillList.OnToggleSkill += OnToggleSkill;
            uiMvpTimer.OnFinish += OnFinishedRemainTimer;

            uiResultClear.Hide();
            uiBattleBossHp.Hide();
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleMazeSkillList.Hide();

            cameraController.SetView(defaultView);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiResultClear.OnFinishDungeon -= ExitEntry;
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnToggleSkill;
            uiBattleMenu.OnExit -= GiveUp;
            uiBattleSkillList.OnSelect -= OnSelectSkill;
            uiBattleSkillList.OnToggleSkill -= OnToggleSkill;
            uiMvpTimer.OnFinish -= OnFinishedRemainTimer;

            UI.Close<UIMainTop>();
            UI.Close<UIResultClear>();
            UI.Close<UIController>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleMazeAgentList>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleSkillList>();
            UI.Close<UIMvpTimer>();
            UI.Close<UIBossComing>();

            uiMainTop = null;
            uiResultClear = null;
            uiController = null;
            uiBattlePlayerStatus = null;
            uiBattleMazeAgentList = null;
            uiBattleBossHp = null;
            uiBattleMazeSkillList = null;
            uiBattleMenu = null;
            uiBattleSkillList = null;
            uiMvpTimer = null;
            uiBossComing = null;
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

            // 보스 폐기
            Dispose(bossEntity);

            // 강제 스탯 제거
            player.ResetExtraOption(ExtraBattleOptionType.DmgRate);
            player.ResetExtraOption(ExtraBattleOptionType.DmgRateResist);
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            // 이미 입장하지 않은 상태
            if (!gateMazeManager.IsJoined())
            {
                IsSuccessEnter = false;
                yield break;
            }

            playerBotPool.EnqueueRange(gateMazeManager.GetMultiPlayers());
            IsSuccessEnter = true; // 무조건 성공
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
            isFixedBoss = data.IsFixedBoss();
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

            map.SetPortalActive(isActive: true); // 가로막
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
            yield return Timing.WaitUntilDone(YieldSpawnBossMonster(), TAG);

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            characterList.Add(player);

            player.SetExtraOption(ExtraBattleOptionType.MaxHp, gateMazeManager.PlayerMaxHp);
            player.SetCurrentHp(gateMazeManager.PlayerMaxHp); // 부활

            if (impl.Add(player, isEnemy: false))
            {
                UnitActor actor = player.SpawnActor(); // 유닛 소환

                Vector3 playerPosition = map.GetGuardianPosition();
                Vector3 position = FindClosestEdge(playerPosition);
                actor.AI.SetHomePosition(position, isWarp: true); // Home 세팅
                actor.Movement.SetRotation(map.GetGuardianRotation());

                player.Skill.SetAntiSkillAuto(true); // 자동스킬 해제

                cameraController.SetPlayer(actor.CachedTransform); // 카메라 - 유닛 추가
                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> YieldSpawnPlayerBots()
        {
            while (playerBotPool.HasQueue())
            {
                if (!SpawnPlayerBot(playerBotPool.Dequeue()))
                    continue;

                yield return Timing.WaitForOneFrame;
            }

            RefreshMazeCharacters();
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(IMultiPlayerInput input)
        {
            CharacterEntity entity = playerBotPool.Create(input);
            characterList.Add(entity);
            entity.SetIgnoreTarget(isIgnoreTarget: true);

            bool needSpawn = impl.Add(entity, isEnemy: false);

            // ExtraOption - MaxHp 의 경우에는 ReadyToBattle 전에 설정
            UnitActor unitActor;
            if (needSpawn)
            {
                unitActor = entity.SpawnActor(); // 유닛 소환

                unitActor.AI.StopHpRegen(); // HpRegen 은 사용하지 말 것
                unitActor.AI.StopMpRegen(); // MpRegen 은 사용하지 말 것
            }
            else
            {
                unitActor = entity.GetActor();
            }

            Vector3 randomSpawnPos = map.GetRandomSpawnPosition();
            Vector3 position = FindClosestEdge(randomSpawnPos);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프
            unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출

            return needSpawn;
        }

        /// <summary>
        /// 보스 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnBossMonster()
        {
            bossEntity = factory.CreateWorldBoss(gateMazeManager.CurrentData);

            UnitActor bossActor;
            bossEntity.OnDespawnActor += OnDespawnBoss;
            if (impl.Add(bossEntity, isEnemy: true))
            {
                bossActor = bossEntity.SpawnActor(); // 유닛 소환

                Vector3 bossPosition = map.GetOpponentPosition();
                bossActor.AI.SetHomePosition(bossPosition, isWarp: true); // 위치 세팅
                bossActor.Movement.SetRotation(map.GetOpponentRotation());
                bossActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)

                yield return Timing.WaitForOneFrame;
            }
            else
            {
                bossActor = bossEntity.GetActor();
            }

            if (isFixedBoss)
            {
                bossActor.AI.SetLookFixed(); // 고정
                bossActor.Movement.SetLookFixed();
            }

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

            uiBattleBossHp.SetHp(gateMazeManager.WorldBossHp, gateMazeManager.WorldBossMaxHp);
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 씬 변경되면서 미로 상태가 아닐 경우에 대한 처리
            if (gateMazeManager.State != GateMazeState.WorldBossBattle)
            {
                OnUpdateState();
                return;
            }

            // 컷씬
            uiBossComing.ShowBoss(gateMazeManager.CurrentData);

            player.SetCurrentHp(gateMazeManager.PlayerMaxHp);
            player.SetCurrentMp(player.MaxMp);

            UnitActor playerActor = player.GetActor();
            UnitActor bossActor = bossEntity.GetActor();

            if (playerActor == null)
                return;

            if (bossActor == null)
                return;

            playerActor.EffectPlayer.ShowUnitCircle();

            // HpRegen 은 사용하지 말 것
            playerActor.AI.StopHpRegen();
            bossActor.AI.StopHpRegen();

            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit);

            cameraController.ShowSkillCamera(true);
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity.type == UnitEntityType.Player)
            {
                Protocol.RECEIVE_GATE_USERDIE.SendAsync().WrapNetworkErrors();

                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
            }

            if (unitEntity.type == UnitEntityType.BossMonster) // 보스 몬스터 사망
            {
                impl.PauseAllUnit();
                uiBattleBossHp.Hide();
                return;
            }

            // 모든 아군이 죽었을 때
            if (impl.IsAllAlliesDead())
            {
                return;
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
            }
        }

        public override void OnDamageValue(UnitEntity unitEntity, UnitEntity attacker, int damage)
        {
            if (unitEntity.type != UnitEntityType.WorldBoss || attacker.type != UnitEntityType.Player)
                return;

            RequestGateBossBattleAttackAsync(unitEntity, damage).WrapNetworkErrors();
        }

        #region 플레이어 이벤트

        void OnPlayerDespawnActor(UnitActor obj)
        {
            player.Skill.SetAntiSkillAuto(false);
        }

        /// <summary>
        /// 상태 변화
        /// </summary>
        void OnUpdateState()
        {
            switch (gateMazeManager.State)
            {
                case GateMazeState.None:
                    map.SetPortalActive(isActive: false); // 가로막 없음

                    uiBattleBossHp.Hide();
                    uiMvpTimer.StopTimer();
                    uiMvpTimer.Hide();
                    EndUnitAI();

                    UIResultClear.ResultType resultType = gateMazeManager.BattleResult == GateMazeBattleResult.Succees ? UIResultClear.ResultType.Clear : UIResultClear.ResultType.Result;
                    uiResultClear.Show(gateMazeManager.TotalRewards, DungeonType.Gate, false, resultType);
                    uiResultClear.SetDescription(string.Empty);
                    break;
            }
        }

        /// <summary>
        /// 플레이어 봇 퇴장
        /// </summary>
        void OnMultiPlayerExit(PlayerBotEntity entity)
        {
            DisposeBot(entity.Character.Cid);

            UnitActor actor = entity.GetActor();
            if (actor)
                actor.EffectPlayer.SpawnUnitTeleport();

            entity.ResetExtraOption(ExtraBattleOptionType.MaxHp);
            entity.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            entity.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);

            // Refresh 및 카메라 다시 세팅
            int index = FindIndex(entity);
            if (index != -1)
            {
                characterList.RemoveAt(index);
                RefreshMazeCharacters();
            }

            Dispose(entity);
        }

        private void DisposeBot(int cid)
        {
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
        }

        /// <summary>
        /// 플레이어 봇 상태변화
        /// </summary>
        void OnMultiPlayerState(PlayerBotEntity entity)
        {
            GateMazeEntry.PlayerState state = entity.BotState.ToEnum<GateMazeEntry.PlayerState>();
            UpdatePlayerState(entity, state);
        }

        void OnWorldBossHp()
        {
            if (uiBattleBossHp == null)
                return;

            uiBattleBossHp.TweenHp(gateMazeManager.WorldBossHp, gateMazeManager.WorldBossMaxHp);

            if (gateMazeManager.WorldBossHp <= 0)
            {
                bossEntity.Die(null);
            }
        }

        void OnDespawnBoss(UnitActor unitActor)
        {
            if (isFixedBoss)
            {
                unitActor.AI.ResetLookFixed();
                unitActor.Movement.ResetLookFixed();
            }

            unitActor.Entity.OnDespawnActor -= OnDespawnBoss;
        }

        /// <summary>
        /// 플레이어 상태 변화
        /// </summary>
        private void UpdatePlayerState(PlayerBotEntity entity, GateMazeEntry.PlayerState state)
        {
            int index = FindIndex(entity);
            if (index == -1)
                return;

            if (state == GateMazeEntry.PlayerState.Dead)
                characterList[index].Die(null);
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

        void OnToggleSkill()
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
            EndUnitAI();
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

        private async Task RequestGateBossBattleAttackAsync(UnitEntity entity, int damage)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", damage);
            sfs.PutByteArray("2", entity.damagePacket.ToByteArray()); // 대미지 패킷

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(entity.damagePacket);
#endif

            Response response = await Protocol.REQUEST_GATE_BOSSBATTLE_ATTACK.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

#if UNITY_EDITOR
            if (response.ContainsKey("3"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("3")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
        }

        /// <summary>
        /// 캐릭터 인덱스 반환
        /// </summary>
        private int FindIndex(PlayerBotEntity entity)
        {
            for (int i = 0; i < characterList.size; i++)
            {
                if (characterList[i].Character.Cid == entity.Character.Cid)
                    return i;
            }

            return -1;
        }

        private void RefreshMazeCharacters()
        {
            uiBattleMazeAgentList.SetShareCharacters(characterList.ToArray());
        }
    }
}