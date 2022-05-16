using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DuelEntry : BattleEntry
    {
        private const float GROUP_VIEW_TARGET_PLAYER_RADIUS = 4f;
        private const float POSITION_DISTANCE = 3f;

        /******************** Models ********************/
        private readonly DuelModel duelModel;
        private readonly AgentModel agentModel;
        private readonly CharacterModel characterModel;
        private readonly StatusModel statusModel;

        /******************** Repositories ********************/
        private readonly JobDataManager jobDataRepo;
        private readonly AgentDataManager agentDataRepo;
        private readonly StatDataManager statDataRepo;
        private readonly SoundManager soundManager;

        /******************** Scene Compositions ********************/
        private readonly long limitTime;
        private readonly GhostBattlePlayers agentCharacterFactory, opponentAgentCharacterFactory;
        private readonly Buffer<AgentCharacterPacket> agentPacketBuffer, opponentAgentPacketBuffer;
        private readonly int pvpHpRate;

        /******************** UIs ********************/
        UIBattleDuel uiBattleDuel;
        UITimer uiTimer;
        UIResultLeague uiResultLeague;
        UIBattleIntro uiBattleIntro;

        /******************** Temp Data ********************/
        private Map map; // 맵
        private CharacterEntity opponent; // 상대편
        private bool isRequestEndDuel; // 리그 완료 (나중에 다른 플레이어가 죽었을 때에 대한 처리 막기 위해)
        private bool isFinishedHideBattleIntro; // 인트로 화면 꺼지고 난 후에 호출
        private int enterDuelPoint; // 입장시 필요한 듀얼 포인트
        private UIDuel.State state;
        private int serverId; // 이벤트 전용
        private int serverRandomKey;

#if UNITY_EDITOR
        private DebugDamageTuple debugDamageTuple;
#endif

        public DuelEntry() : base(BattleMode.Duel)
        {
            duelModel = player.Duel;
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

            limitTime = BasisType.PVE_BATTLE_TIME.GetInt();
            enterDuelPoint = BasisType.ENTER_DUEL_POINT.GetInt();
            pvpHpRate = MathUtils.ToPermyriad(BasisType.PVP_HP_RATE.GetFloat());
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;

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

        public override void OnBack()
        {
            // 듀얼에서 백버튼은 무시합니다.
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();
            isRequestEndDuel = false;

            cameraController.ClearMember();
        }

        public override IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
            int curDuelPoint = duelModel.DuelPoint;
            if (curDuelPoint < enterDuelPoint)
            {
                Debug.LogError($"듀얼 포인트 부족 {nameof(curDuelPoint)}={curDuelPoint}, {nameof(enterDuelPoint)}={enterDuelPoint}");
                IsSuccessEnter = false;
                yield break;
            }

            var input = battleInput as BattleInputDuel;

            if (input is null)
            {
                Debug.LogError($"input NULL");
                IsSuccessEnter = false;
                yield break;
            }

            state = input.state;
            serverId = input.serverId;

            Task<Response> task;
            var sfs = Protocol.NewInstance();
            switch (state)
            {
                case UIDuel.State.Chapter:
                    sfs.PutInt("1", input.cid);
                    sfs.PutInt("2", input.uid);
                    sfs.PutInt("3", input.chapter); // chapter
                    sfs.PutInt("4", input.alphabetBit); // alphabetBit
                    task = Protocol.REQUEST_DUEL_START.SendAsync(sfs);
                    break;

                case UIDuel.State.Event:
                    sfs.PutInt("1", input.cid);
                    sfs.PutInt("2", input.uid);
                    task = Protocol.REQUEST_DUELWORLD_BATTLESTART.SendAsync(sfs);
                    break;

                case UIDuel.State.Arena:
                    sfs.PutInt("1", input.cid);
                    sfs.PutInt("2", input.uid);
                    task = Protocol.REQUEST_ARENA_BATTLE_START.SendAsync(sfs);
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(state)} = {state}");
            }

            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
            {
                // 해당 팝업은 띄우지 않
                if (state == UIDuel.State.Chapter && response.resultCode == ResultCode.PVE_BATTLE_NOT_END)
                    yield break;

                if (state == UIDuel.State.Arena)
                {
                    // 진행중이 아니다.
                    if (response.resultCode == ResultCode.NOT_IN_PROGRESS)
                    {
                        string message = LocalizeKey._47881.ToText(); // 아레나가 종료되었습니다.
                        UI.ConfirmPopup(message);
                        yield break;
                    }

                    // 최대 횟수를 초과하였습니다.
                    if (response.resultCode == ResultCode.NOT_ENOUGHT_COUNT)
                    {
                        string message = LocalizeKey._47882.ToText(); // 아레나 깃발이 부족합니다.
                        UI.ConfirmPopup(message);
                        yield break;
                    }

                }

                response.ShowResultCode();
                yield break;
            }

            // 기존 동료 폐기
            DisposeAgentCharacters();
            DisposeOpponentAgentCharacters();

            player.ResetSkillCooldown(); // 쿨타임 초기화

            // 월드 듀얼이 아닌 일반 듀얼일때만 퀘스트 카운트 체크
            if (state == UIDuel.State.Chapter)
                Quest.QuestProgress(QuestType.DUEL_COUNT); // 듀얼 도전 횟수

            SetAgentPlayer();

            BattleCharacterPacket opponent = response.GetPacket<BattleCharacterPacket>("1");
            SetOpponentMultiPlayers(opponent);
            SetOpponentAgentPlayer(opponent);

            // 듀얼 포인트 갱신
            int duelPoint = response.GetInt("2");
            duelModel.SetDualPoint(duelPoint);

            serverRandomKey = response.GetInt("3");

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("서버에서 받은 듀얼 RandomKey 값: " + serverRandomKey);
            }
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
            uiBattleDuel.SetData(UIBattleDuel.UnitType.Player, 0, player);
            uiBattleDuel.SetAgents(UIBattleDuel.UnitType.Player, agentCharacterFactory.ToArray());
            uiBattleDuel.SetData(UIBattleDuel.UnitType.Enemy, 0, opponent);
            uiBattleDuel.SetAgents(UIBattleDuel.UnitType.Enemy, opponentAgentCharacterFactory.ToArray());
            uiBattleDuel.SetActiveCount(false); // 전투 진행 상황 보여준다


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
            if (impl.Add(player, isEnemy: false))
            {
                player.SetExtraOption(ExtraBattleOptionType.PlusPvpHpRate, pvpHpRate);

                UnitActor unitActor = player.SpawnActor(); // 유닛 소환

                Vector3 playerPosition = map.GetPlayerPosition();
                unitActor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
                unitActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)

                cameraController.AddMember(unitActor.CachedTransform, GROUP_VIEW_TARGET_PLAYER_RADIUS); // 유닛 추가

                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 듀얼 상대 캐릭터 생성
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldSpawnDuelCharacter()
        {
            if (impl.Add(opponent, isEnemy: true))
            {
                opponent.SetExtraOption(ExtraBattleOptionType.PlusPvpHpRate, pvpHpRate);

                opponent.OnDespawnActor += OnCharacterDespawn;
                opponent.OnAttacked += OnOpponentAttack;

                UnitActor unitActor = opponent.SpawnActor(); // 유닛 소환

                Vector3 opponentPosition = map.GetOpponentPosition();
                unitActor.AI.SetHomePosition(opponentPosition, isWarp: true); // 위치 세팅
                unitActor.Animator.PlayIdle(); // 기본 자세 (YieldComposeSceneAfter 후에 AI 가 실행되기 때문에)

                cameraController.AddMember(unitActor.CachedTransform, GROUP_VIEW_TARGET_PLAYER_RADIUS); // 유닛 추가

                yield return Timing.WaitForOneFrame;
            }
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
            UnitActor actor = unitEntity.GetActor();
            if (actor)
                actor.AI.EndAI(); // 부활 방지

            if (impl.IsAllEnemyDead()) // 모든 적이 죽었는지 확인
            {
                RequestEndDuel(isWin: true, unitEntity.damagePacket).WrapNetworkErrors();
            }
            else if (impl.IsAllAlliesDead()) // 모든 아군이 죽었는지 확인
            {
                RequestEndDuel(isWin: false).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 듀얼 전투 종료
        /// </summary>
        private async Task RequestEndDuel(bool isWin, DamagePacket damagePacket = null)
        {
            uiTimer.StopTimer();

            foreach (var unit in unitList)
            {
                unit.GetActor()?.AI.EndAI();
            }

            // 중복 호출 방지
            if (isRequestEndDuel)
                return;

            isRequestEndDuel = true;

            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", isWin);
            sfs.PutUtfString("2", MathUtils.CidToHexCode(serverRandomKey));

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("듀얼 RandomKey 처리: key = " + serverRandomKey + ", hex = " + MathUtils.CidToHexCode(serverRandomKey));
            }

            Response response;
            switch (state)
            {
                case UIDuel.State.Chapter:
                    if (isWin)
                    {
                        sfs.PutByteArray("99", damagePacket.ToByteArray()); // 대미지 패킷
#if UNITY_EDITOR
                        debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
                    }
                    response = await Protocol.REQUEST_DUEL_END.SendAsync(sfs);
                    break;

                case UIDuel.State.Event:
                    response = await Protocol.REQUEST_DUELWORLD_BATTLEEND.SendAsync(sfs);
                    break;

                case UIDuel.State.Arena:
                    if (isWin)
                    {
                        sfs.PutByteArray("99", damagePacket.ToByteArray()); // 대미지 패킷
#if UNITY_EDITOR
                        debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
                    }
                    response = await Protocol.REQUEST_ARENA_BATTLE_END.SendAsync(sfs);
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(state)} = {state}");
            }

            if (!response.isSuccess)
            {
                if (state == UIDuel.State.Arena)
                {
                    // 진행 중이 아니다.
                    if (response.resultCode == ResultCode.NOT_IN_PROGRESS)
                    {
                        string message = LocalizeKey._47881.ToText(); // 아레나가 종료되었습니다.
                        UI.ConfirmPopup(message, ExitEntry);
                        return;
                    }
                }

                UI.ConfirmPopup(response.resultCode.GetDescription(), ExitEntry);
                return;
            }

            int result;
            switch (state)
            {
                case UIDuel.State.Chapter:
                    {
                        result = response.GetInt("1");
                        int duelPoint = response.GetInt("2");
                        duelModel.SetDualPoint(duelPoint); // 포인트 업데이트
                        duelModel.SetDuelResult(result == UIResultDuel.SUCCESS, state); // 듀얼 결과 업데이트
                    }
                    break;

                case UIDuel.State.Event:
                    {
                        result = isWin ? UIResultDuel.SUCCESS : UIResultDuel.FAIL;
                        duelModel.SetDuelResult(false, state);
                    }
                    break;

                case UIDuel.State.Arena:
                    {
                        result = response.GetInt("1");
                        int duelPoint = response.GetInt("2");
                        duelModel.SetDualPoint(duelPoint); // 포인트 업데이트
                        duelModel.SetDuelResult(false, state);

                        //  듀얼 아레나 승리 횟수
                        if (isWin)
                            Quest.QuestProgress(QuestType.DUEL_ARENA_CLEAR_COUNT);
                    }
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(state)} = {state}");
            }

            await Awaiters.Seconds(3f);

#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
            bool isClosed = false;
            uiBattleDuel.ShowSelectPopup(false);
            var resultUI = UI.Show<UIResultDuel>(new UIResultDuel.Input { state = state, result = result, serverId = serverId });
            resultUI.onClose = () => { isClosed = true; };

            await new WaitUntil(() => isClosed);
            ExitEntry();
        }

        /// <summary>
        /// 항복 버튼
        /// </summary>
        void OnBattleGiveUp()
        {
            RequestEndDuel(isWin: false).WrapNetworkErrors();
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
            RequestEndDuel(isWin: false).WrapNetworkErrors();
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