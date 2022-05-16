using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class LeagueEntry : BattleEntry
    {
        private const float GROUP_VIEW_TARGET_PLAYER_RADIUS = 4f;
        private const float POSITION_DISTANCE = 3f;

        /******************** Models ********************/
        private readonly LeagueModel leagueModel;
        private readonly InventoryModel inventoryModel;
        private readonly AgentModel agentModel;
        private readonly CharacterModel characterModel;
        private readonly StatusModel statusModel;

        /******************** Repositories ********************/
        private readonly JobDataManager jobDataRepo;
        private readonly AgentDataManager agentDataRepo;
        private readonly StatDataManager statDataRepo;
        private readonly SoundManager soundManager;

        /******************** Scene Compositions ********************/
        private readonly int leagueTicketId;
        private readonly long limitTime;
        private readonly int pvpHpRate;
        private readonly GhostBattlePlayers agentCharacterFactory, opponentAgentCharacterFactory;
        private readonly Buffer<AgentCharacterPacket> agentPacketBuffer, opponentAgentPacketBuffer;

        /******************** UIs ********************/
        UIBattleDuel uiBattleDuel;
        UITimer uiTimer;
        UIResultLeague uiResultLeague;
        UIBattleIntro uiBattleIntro;

        /******************** Temp Data ********************/
        private Map map; // 맵
        private CharacterEntity opponent; // 상대편
        private int battleCount; // 진행 전투 수
        private int maxBattleCount; // 최대 진행 전투 수
        private bool isRequestEndPve; // 리그 완료 (나중에 다른 플레이어가 죽었을 때에 대한 처리 막기 위해)
        private int savedOpponentPoint = 0; // 상대편 티어
        private bool isFinishedHideBattleIntro; // 인트로 화면 꺼지고 난 후에 호출
        private bool isGiveUp;
        private int serverRandomKey;
#if UNITY_EDITOR
        private DebugDamageTuple debugDamageTuple;
#endif

        public LeagueEntry() : base(BattleMode.League)
        {
            leagueModel = player.League;
            inventoryModel = player.Inventory;
            agentModel = player.Agent;
            characterModel = player.Character;
            statusModel = player.Status;

            jobDataRepo = JobDataManager.Instance;
            agentDataRepo = AgentDataManager.Instance;
            statDataRepo = StatDataManager.Instance;
            soundManager = SoundManager.Instance;

            agentCharacterFactory = new GhostBattlePlayers();
            agentPacketBuffer = new Buffer<AgentCharacterPacket>();
            opponentAgentCharacterFactory = new GhostBattlePlayers();
            opponentAgentPacketBuffer = new Buffer<AgentCharacterPacket>();

            leagueTicketId = BasisItem.LeagueTicket.GetID();
            limitTime = BasisType.PVE_BATTLE_TIME.GetInt();
            pvpHpRate = MathUtils.ToPermyriad(BasisType.PVP_HP_RATE.GetFloat());
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            battleCount = 0;
            maxBattleCount = 0;
            map = null;
            savedOpponentPoint = 0;

            if (player)
            {
                player.ResetExtraOption(ExtraBattleOptionType.PlusPvpHpRate);
            }

            if (opponent)
                opponent.ResetExtraOption(ExtraBattleOptionType.PlusPvpHpRate);

            // 상대편 폐기
            Dispose(opponent);
            opponent = null;
            DisposeAgentCharacters();
            DisposeOpponentAgentCharacters();

            agentCharacterFactory.Clear();
            agentPacketBuffer.Clear();
            opponentAgentCharacterFactory.Clear();
            opponentAgentPacketBuffer.Clear();
#if UNITY_EDITOR
            debugDamageTuple = null;
#endif

            SetNextSingleAction(ShowLeagueUI);
        }

        private void ShowLeagueUI()
        {
            UI.Show<UILeague>();
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnAttacked += OnPlayerAttack;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnAttacked -= OnPlayerAttack;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiBattleDuel = UI.Show<UIBattleDuel>();
            uiTimer = UI.Show<UITimer>();
            uiBattleIntro = UI.Show<UIBattleIntro>();
            uiResultLeague = UI.Show<UIResultLeague>();

            uiTimer.OnFinish += Timeout;
            uiBattleIntro.OnFinished += OnHideBattleIntro;
            uiBattleDuel.OnGiveUp += OnBattleGiveUp;

            uiTimer.Hide();
            uiBattleDuel.Hide();
            uiBattleIntro.Hide();
            uiResultLeague.Hide();

            cameraController.SetView(CameraController.View.GroupHigh); // GroupView로 전환
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiBattleDuel.ResetData();

            uiTimer.OnFinish -= Timeout;
            uiBattleIntro.OnFinished -= OnHideBattleIntro;
            uiBattleDuel.OnGiveUp -= OnBattleGiveUp;

            UI.Close<UIBattleDuel>();
            UI.Close<UITimer>();
            UI.Close<UIBattleIntro>();
            UI.Close<UIResultLeague>();

            uiBattleDuel = null;
            uiTimer = null;
            uiBattleIntro = null;
            uiResultLeague = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();
            isRequestEndPve = false;

            cameraController.ClearMember();
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            Debug.Log($"{nameof(battleCount)}={battleCount} {nameof(maxBattleCount)}={maxBattleCount}");
            yield return Timing.WaitUntilDone(YieldEnterBattle(Mathf.Max(maxBattleCount)));
        }

        /// <summary>
        /// 특정 수만큼 리그 시작
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int maxBattleCount)
        {
            this.maxBattleCount = maxBattleCount;

            // 입장권 없음
            if (leagueModel.LeagueFreeTicket == 0 && inventoryModel.GetItemCount(leagueTicketId) == 0)
            {
                IsSuccessEnter = false;
                UI.ShowToastPopup(LocalizeKey._481.ToText()); // 대전 입장권이 부족합니다.
                yield break;
            }

            Task<Response> task = leagueModel.RequestPveStart(); // 대전 시작
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            // ResultCode.IN_CLAC_PVP_SEASON_RANK // (82) 시즌이 종료되어 정산중
            // ResultCode.PVE_NOT_MATCH // (83) 매칭실패
            // ResultCode.PVE_SEASON_CHANGE // (84) 1:1대전 시즌 정보가 변경되었다

            if (!IsSuccessEnter)
                yield break;

            // 기존 동료 폐기
            DisposeAgentCharacters();
            DisposeOpponentAgentCharacters();

            SetAgentPlayer();

            BattleCharacterPacket opponent = response.GetPacket<BattleCharacterPacket>("1");
            savedOpponentPoint = response.GetInt("2");
            serverRandomKey = response.GetInt("3");

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("서버에서 받은 대전 RandomKey 값: " + serverRandomKey);
            }

            SetOpponentMultiPlayers(opponent);
            SetOpponentAgentPlayer(opponent);

            ++battleCount;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = "Scene_InsidePyramid_PVE"; // 씬 이름
            string bgmName = "Raid_01"; // 배경음 이름
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

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            cameraController.RemoveMask(Layer.PLAYER, Layer.CUPET, Layer.ALLIES, Layer.ENEMY, Layer.GHOST); // 카메라 레이어 설정

            uiTimer.Initialize(limitTime);

            uiBattleDuel.Hide();
            uiTimer.Hide();
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

            yield return Timing.WaitUntilDone(YieldSpawnPlayer(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnDuelCharacter(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnAgentCharacters(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnOpponentAgentCharacters(), TAG);
        }

        protected override IEnumerator<float> YieldComposeSceneAfter()
        {
            isFinishedHideBattleIntro = false;
            uiBattleIntro.Show(player.Character, opponent.Character);
            soundManager.PlayUISfx(Sfx.UI.DimensionIn);
            yield return Timing.WaitUntilTrue(IsFinishedHideBattleIntro);
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            uiBattleDuel.Show();
            uiBattleDuel.SetData(UIBattleDuel.UnitType.Player, leagueModel.SeasonScore, player, leagueModel.IsSingle);
            uiBattleDuel.SetAgents(UIBattleDuel.UnitType.Player, agentCharacterFactory.ToArray());
            uiBattleDuel.SetData(UIBattleDuel.UnitType.Enemy, savedOpponentPoint, opponent, leagueModel.IsSingle);
            uiBattleDuel.SetAgents(UIBattleDuel.UnitType.Enemy, opponentAgentCharacterFactory.ToArray());
            uiBattleDuel.SetActiveCount(maxBattleCount > 0); // 전투 진행 상황 보여준다
            uiBattleDuel.SetCount(battleCount, maxBattleCount);

            uiTimer.Show();
            uiTimer.RestartTimer();

            cameraController.AddMask(Layer.PLAYER, Layer.CUPET, Layer.ALLIES, Layer.ENEMY, Layer.GHOST); // 카메라 레이어 설정

            player.GetActor().EffectPlayer.ShowUnitCircle();
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            UnitActor unitActor;
            if (impl.Add(player, isEnemy: false))
            {
                player.SetExtraOption(ExtraBattleOptionType.PlusPvpHpRate, pvpHpRate);

                unitActor = player.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                unitActor = player.GetActor();
            }

            cameraController.AddMember(unitActor.CachedTransform, GROUP_VIEW_TARGET_PLAYER_RADIUS); // 유닛 추가
            Vector3 playerPosition = map.GetPlayerPosition();
            unitActor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
            unitActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)
        }

        /// <summary>
        /// 듀얼 상대 캐릭터 생성
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldSpawnDuelCharacter()
        {
            UnitActor unitActor;
            if (impl.Add(opponent, isEnemy: true))
            {
                opponent.SetExtraOption(ExtraBattleOptionType.PlusPvpHpRate, pvpHpRate);

                opponent.OnDespawnActor += OnCharacterDespawn;
                opponent.OnAttacked += OnOpponentAttack;

                unitActor = opponent.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);
                cameraController.AddMember(unitActor.CachedTransform, GROUP_VIEW_TARGET_PLAYER_RADIUS); // 유닛 추가
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                unitActor = opponent.GetActor();
            }

            Vector3 opponentPosition = map.GetOpponentPosition();
            unitActor.AI.SetHomePosition(opponentPosition, isWarp: true); // 위치 세팅
            unitActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)
        }

        private void OnCharacterDespawn(UnitActor actor)
        {
            actor.Entity.OnAttacked -= OnOpponentAttack;
            actor.Entity.OnDespawnActor -= OnCharacterDespawn;
        }

        private void OnHideBattleIntro()
        {
            isFinishedHideBattleIntro = true;
        }

        private bool IsFinishedHideBattleIntro()
        {
            return isFinishedHideBattleIntro;
        }

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (impl.IsAllEnemyDead()) // 모든 적이 죽었는지 확인
            {
                RequestEndPve(isWin: true, unitEntity.damagePacket);
            }
            else if (impl.IsAllAlliesDead()) // 모든 아군이 죽었는지 확인
            {
                RequestEndPve(isWin: false);
            }
        }

        /// <summary>
        /// 대전 종료 서버 호출
        /// </summary>
        private void RequestEndPve(bool isWin, DamagePacket damagePacket = null)
        {
            uiTimer.StopTimer();
            RequestPveEnd(isWin, damagePacket).WrapNetworkErrors();
        }

        /// <summary>
        /// 대전 결과
        /// </summary>
        private async Task RequestPveEnd(bool isWin, DamagePacket damagePacket)
        {
            // 중복 호출 방지
            if (isRequestEndPve)
                return;

            foreach (var unit in unitList)
            {
                unit.GetActor()?.AI.EndAI();
            }

            isRequestEndPve = true;

            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", isWin);
            sfs.PutUtfString("2", MathUtils.CidToHexCode(serverRandomKey));

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("서버에서 받은 대전 RandomKey 값: " + serverRandomKey);
            }

            if (isWin)
            {
                sfs.PutByteArray("99", damagePacket.ToByteArray()); // 대미지 패킷
#if UNITY_EDITOR
                debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            }

            Response response = await Protocol.REQUEST_PVE_END.SendAsync(sfs);

            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.IN_CLAC_PVP_SEASON_RANK) // (82) 시즌이 종료되어 정산중
                {
                    ExitEntry();
                    return;
                }
                else if (response.resultCode == ResultCode.IN_CLAC_PVP_SEASON_RANK) // (82) 시즌이 종료되어 정산중
                {
                    ExitEntry();
                    return;
                }

                ExitEntry();
                response.ShowResultCode();
                return;
            }

            int currentScore = leagueModel.IsSingle ? leagueModel.SingleScore : leagueModel.SeasonScore;
            int totalScore = response.GetInt("1");

#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif

            Timing.RunCoroutine(YieldShowResult(isWin, currentScore, totalScore), TAG);

            if (leagueModel.IsSingle)
            {
                leagueModel.SetCurrentSingleScore(totalScore);
            }
            else
            {
                leagueModel.SetCurrentScore(totalScore);
            }
        }

        private IEnumerator<float> YieldShowResult(bool isWin, int beforePoint, int afterPoint)
        {
            yield return Timing.WaitForSeconds(3f);

            uiTimer.Hide(); // 타이머 꺼줌

            bool hasNextBattle = battleCount < maxBattleCount; // 다음 전투가 남아있음
            bool isEndBattle = isGiveUp || !hasNextBattle || !uiBattleDuel.IsToggleOn; // 포기했거나, 다음 전투가 남아있지 않거나, 토글이 꺼져 있음
            uiBattleDuel.ShowSelectPopup(false);
            uiResultLeague.Show(isWin, beforePoint, afterPoint, isShowButton: isEndBattle);

            // 전투 종료
            if (isEndBattle)
            {
                battleCount = 0;
                maxBattleCount = 0;
            }
            else
            {
                Timing.RunCoroutine(YieldStartNextBattle(), TAG);
            }
        }

        private IEnumerator<float> YieldStartNextBattle()
        {
            yield return Timing.WaitForSeconds(3f);
            uiResultLeague.Hide(); // 결과 창 닫음

            impl.StartBattle(BattleMode.League);
        }

        /// <summary>
        /// 항복 버튼
        /// </summary>
        void OnBattleGiveUp()
        {
            isGiveUp = true;
            RequestEndPve(isWin: false);
        }

        private void SetOpponentMultiPlayers(IMultiPlayerInput input)
        {
            if (input == null)
                return;

            opponent = CharacterEntity.Factory.CreateMultiBattlePlayer();
            opponent.Character.Initialize(input);
            opponent.Status.Initialize(input);
            opponent.Status.Initialize(input.IsExceptEquippedItems, input.BattleOptions, input.GuildBattleOptions);
            opponent.Inventory.Initialize(input.ItemStatusValue, input.WeaponItemId, input.ArmorItemId, input.WeaponChangedElement, input.WeaponElementLevel, input.ArmorChangedElement, input.ArmorElementLevel, input.GetEquippedItems);
            opponent.Skill.Initialize(input.IsExceptEquippedItems, input.Skills);
            opponent.Skill.Initialize(input.Slots);
            opponent.CupetList.Initialize(input.Cupets);
            opponent.Guild.Initialize(input);
        }

        private void Timeout()
        {
            RequestEndPve(isWin: false);
        }

        void OnPlayerAttack(UnitEntity target, SkillType skillType, int skillId, ElementType elementType, bool hasDamage, bool isChainableSkill)
        {
            if (player == null)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            // 평타
            if (skillType == SkillType.BasicActiveSkill)
                return;

            PlusWeight(actor.CachedTransform);
        }

        void OnOpponentAttack(UnitEntity target, SkillType skillType, int skillId, ElementType elementType, bool hasDamage, bool isChainableSkill)
        {
            if (opponent == null)
                return;

            UnitActor actor = opponent.GetActor();
            if (actor == null)
                return;

            // 평타
            if (skillType == SkillType.BasicActiveSkill)
                return;

            PlusWeight(actor.CachedTransform);
        }

        private void PlusWeight(Transform member)
        {
            cameraController.PlusWeight(member);
            Timing.RunCoroutine(ResetCameraWeight(member), TAG);
        }

        IEnumerator<float> ResetCameraWeight(Transform member)
        {
            yield return Timing.WaitForSeconds(1f);
            cameraController.ResetWeight(member);
        }

        private void SetAgentPlayer()
        {
            if (leagueModel.IsSingle)
                return;

            foreach (var item in agentModel.GetEquipedCombatAgents())
            {
                if (item.ID == 0 || item.AgentData == null)
                    continue;

                agentPacketBuffer.Add(new AgentCharacterPacket(item.AgentData));
            }

            int jobLevel = characterModel.JobLevel;
            int totalStatPoint = statDataRepo.GetTotalPoint(jobLevel); // JobLevel 로 맞춘다
            int maxStatus = statusModel.MaxStatus();
            for (int i = 0; i < agentPacketBuffer.size; i++)
            {
                int jobId = agentPacketBuffer[i].job;
                JobData agentJobData = jobDataRepo.Get(jobId);
                if (agentJobData == null)
                {
                    Debug.LogError($"JobData가 존재하지 않습니다: {nameof(jobId)} = {jobId}");
                    continue;
                }
                JobData.StatValue basicStat = new JobData.StatValue(0);
                JobData.StatValue maxStat = new JobData.StatValue(maxStatus);
                short[] plusGuidStats = agentJobData.GetAutoStatGuidePoints(totalStatPoint, basicStat, maxStat);
                agentPacketBuffer[i].UpdateStatus(jobLevel, plusGuidStats[0], plusGuidStats[1], plusGuidStats[2], plusGuidStats[3], plusGuidStats[4], plusGuidStats[5]);
            }

            SetAgent(agentPacketBuffer.GetBuffer(isAutoRelease: true)); // 멀티 플레이어 세팅
        }

        private void SetOpponentAgentPlayer(IAgentMultiPlayerInfo agent)
        {
            if (leagueModel.IsSingle)
                return;

            foreach (var item in agent.AgentSlots)
            {
                if (!item.IsUsingSlot)
                    continue;

                AgentData agentData = agentDataRepo.Get(item.AgentID);

                if (agentData == null)
                    continue;

                opponentAgentPacketBuffer.Add(new AgentCharacterPacket(agentData));
            }

            int jobLevel = agent.JobLevel;
            int totalStatPoint = statDataRepo.GetTotalPoint(jobLevel); // JobLevel 로 맞춘다
            int maxStatus = statusModel.MaxStatus();
            for (int i = 0; i < opponentAgentPacketBuffer.size; i++)
            {
                int jobId = opponentAgentPacketBuffer[i].job;
                JobData agentJobData = jobDataRepo.Get(jobId);
                if (agentJobData == null)
                {
                    Debug.LogError($"JobData가 존재하지 않습니다: {nameof(jobId)} = {jobId}");
                    continue;
                }
                JobData.StatValue basicStat = new JobData.StatValue(0);
                JobData.StatValue maxStat = new JobData.StatValue(maxStatus);
                short[] plusGuidStats = agentJobData.GetAutoStatGuidePoints(totalStatPoint, basicStat, maxStat);
                opponentAgentPacketBuffer[i].UpdateStatus(jobLevel, plusGuidStats[0], plusGuidStats[1], plusGuidStats[2], plusGuidStats[3], plusGuidStats[4], plusGuidStats[5]);
            }

            SetOpponentAgent(opponentAgentPacketBuffer.GetBuffer(isAutoRelease: true)); // 멀티 플레이어 세팅
        }

        private void SetAgent(IMultiPlayerInput[] packets)
        {
            if (packets == null)
                return;

            foreach (var item in packets)
            {
                agentCharacterFactory.AddGhostPlayer(item);
            }
        }

        private void SetOpponentAgent(IMultiPlayerInput[] packets)
        {
            if (packets == null)
                return;

            foreach (var item in packets)
            {
                opponentAgentCharacterFactory.AddGhostPlayer(item);
            }
        }

        /// <summary>
        /// 동료 캐릭터 폐기
        /// </summary>
        private void DisposeAgentCharacters()
        {
            // 중요: GhostBattlePlayers 의 경우 Recycle 시 Remove 시키므로 for문을 거꾸로 처리한다
            for (int i = agentCharacterFactory.size - 1; i >= 0; i--)
            {
                agentCharacterFactory[i].ResetExtraOption(ExtraBattleOptionType.PlusPvpHpRate);

                Dispose(agentCharacterFactory[i]);
                agentCharacterFactory.Recycle(agentCharacterFactory[i]);
            }

            agentCharacterFactory.Clear();
        }

        /// <summary>
        /// 듀얼 동료 캐릭터 폐기
        /// </summary>
        private void DisposeOpponentAgentCharacters()
        {
            // 중요: GhostBattlePlayers 의 경우 Recycle 시 Remove 시키므로 for문을 거꾸로 처리한다
            for (int i = opponentAgentCharacterFactory.size - 1; i >= 0; i--)
            {
                opponentAgentCharacterFactory[i].ResetExtraOption(ExtraBattleOptionType.PlusPvpHpRate);

                Dispose(opponentAgentCharacterFactory[i]);
                opponentAgentCharacterFactory.Recycle(opponentAgentCharacterFactory[i]);
            }

            opponentAgentCharacterFactory.Clear();
        }

        private IEnumerator<float> YieldSpawnAgentCharacters()
        {
            Vector3 posCenter = player.GetActor().CachedTransform.position;
            int size = agentCharacterFactory.size;
            Vector3[] spawnPositions = map.GetAroundPosition(posCenter, POSITION_DISTANCE, size);

            for (int i = 0; i < size; i++)
            {
                if (impl.Add(agentCharacterFactory[i], isEnemy: false))
                {
                    agentCharacterFactory[i].SetExtraOption(ExtraBattleOptionType.PlusPvpHpRate, pvpHpRate);

                    UnitActor characterActor = agentCharacterFactory[i].SpawnActor(); // 유닛 소환
                    characterActor.AI.SetHomePosition(spawnPositions[i], isWarp: true); // 위치 세팅
                    characterActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)

                    cameraController.AddMember(characterActor.CachedTransform, GROUP_VIEW_TARGET_PLAYER_RADIUS); // 유닛 추가

                    yield return Timing.WaitForOneFrame;
                }
            }
        }

        private IEnumerator<float> YieldSpawnOpponentAgentCharacters()
        {
            Vector3 posCenter = opponent.GetActor().CachedTransform.position;
            int size = opponentAgentCharacterFactory.size;
            Vector3[] spawnPositions = map.GetAroundPosition(posCenter, POSITION_DISTANCE, size);

            for (int i = 0; i < size; i++)
            {
                if (impl.Add(opponentAgentCharacterFactory[i], isEnemy: true))
                {
                    opponentAgentCharacterFactory[i].SetExtraOption(ExtraBattleOptionType.PlusPvpHpRate, pvpHpRate);

                    UnitActor characterActor = opponentAgentCharacterFactory[i].SpawnActor(); // 유닛 소환
                    characterActor.AI.SetHomePosition(spawnPositions[i], isWarp: true); // 위치 세팅
                    characterActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)

                    cameraController.AddMember(characterActor.CachedTransform, GROUP_VIEW_TARGET_PLAYER_RADIUS); // 유닛 추가

                    yield return Timing.WaitForOneFrame;
                }
            }
        }
    }
}