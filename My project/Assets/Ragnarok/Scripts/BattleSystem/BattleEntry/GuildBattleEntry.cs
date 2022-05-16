using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GuildBattleEntry : BattleEntry
    {
#if UNITY_EDITOR
        public interface IEdiitorCupetInput
        {
            int Id { get; }
            int Rank { get; }
            int Level { get; }
        }

        public interface IEditorBuffSkillInput : ISkillDataKey
        {
        }

        public interface IEditorGuildBattleInput : IBattleInput
        {
            IMultiPlayerInput GetAgent();
            IEdiitorCupetInput[] GetCupets();
            IEditorBuffSkillInput[] GetBuffs();

            IEdiitorCupetInput[] GetEnemyLeftCupets();
            IEdiitorCupetInput[] GetEnemyRightCupets();
            IEditorBuffSkillInput[] GetEnemyBuffs();
        }
#endif

        public interface IGuildBattleInput : IBattleInput
        {
            int GuildId { get; }
            string GuildName { get; }

            int AgentCid { get; }
            int AgentUid { get; }
        }

        private const string SCENE_NAME = "Scene_GuildWar";
        private const string BGM_NAME = "Prison_01";
        private const CameraController.View DEFAULT_VIEW = CameraController.View.GuildWarView;
        private const bool IS_DEFAULT_TURRET_TARGETING = false;
        private const float NOTICE_DELAY_ATTACKED_MY_TURRET = 10f; // 아군 포링포탑 피격 시 알림 딜레이시간 (초)

        /// <summary>
        /// 7초 뒤 부활
        /// </summary>
        private const int DEATH_COOL_TIME = 7;

        // <!-- Models --!>
        private readonly GuildModel guildModel;
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly MonsterDataManager monsterDataRepo;
        private readonly ExpDataManager expDataRepo;
        private readonly int playTime; // 플레이타임 (밀리초)
        private readonly int maxDefenceCupetCount; // 최대 방어큐펫 소환 수
        private readonly int maxAttackCupetCount; // 최대 공격큐펫 소환 수
        private readonly float cupetSpawnDelay; // 큐펫 소환 딜레이
        private readonly int maxEmperiumHp;

        // <!-- Scene Compositions --!>
        private readonly CupetSpawnList cupetSpawnList;
        private readonly CupetSpawnList[] enemySpawnList;
        private readonly CupetSpawnList emperiumSpawnList;
        private readonly GhostCupetEntityPool cupetPool;
        private readonly GhostCupetEntityPool enemyCupetPool;
        private readonly BetterList<ISkillDataKey> cupetBuffList;
        private readonly BetterList<ISkillDataKey> turretBuffList;
        private readonly BetterList<ISkillDataKey> enemyCupetBuffList;
        private readonly BetterList<ISkillDataKey> enemyTurretBuffList;
        private readonly Buffer<CharacterEntity> agentBuffer;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIController uiController;
        private UIBattleInfo uiBattleInfo;
        private UIDanger uiDanger;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleGuildAttackSkillList uiBattleGuildAttackSkillList;
        private UIBattleBossHp uiBattleBossHP;
        private UIBattleFail uiBattleFail;
        private UIBattleCountdown uiBattleCountdown;
        private UIBattleToggleOption uiBattleToggleOption;
        private UIResultClear uiResultClear;
        private UIBattleGuildWar uiBattleGuildWar;
        private UIBattleMazeAgentList uiBattleMazeAgentList;

        // <!-- Temp Data --!>
        private Map map; // 맵
        private UnitEntity turret; // 타워 (아군)
        private readonly UnitEntity[] enemyTurrets; // 타워 (적군)
        private UnitEntity emperium; // 엠펠리움
        private GhostPlayerEntity guildAgent; // 길드원
        private bool isFinishedStartingPistol;
        private bool isTurretTargeting;
        private int initEmperiumHp;
        private int totalEmperiumDamage;
        private string targetGuildName;
        private bool isRequestFinishProtocol;
        private RelativeRemainTime noticeRemainTime; // 포링포탑 피격 알림 딜레이

#if UNITY_EDITOR
        private bool isEditorMode;
#endif

        public GuildBattleEntry() : base(BattleMode.GuildBattle)
        {
            guildModel = player.Guild;
            dungeonModel = player.Dungeon;

            monsterDataRepo = MonsterDataManager.Instance;
            expDataRepo = ExpDataManager.Instance;
            playTime = BasisGuildWarInfo.PlayTime.GetInt();
            maxDefenceCupetCount = BasisGuildWarInfo.MaxDefenceCupetCount.GetInt();
            maxAttackCupetCount = BasisGuildWarInfo.MaxAttackCupetCount.GetInt();
            cupetSpawnDelay = BasisGuildWarInfo.CupetSpawnDelay.GetInt() * 0.001f;
            maxEmperiumHp = guildModel.GetMaxEmperiumHp();

            cupetSpawnList = new NextCupetSpawnList();
            enemySpawnList = new CupetSpawnList[2];
            enemySpawnList[0] = new NextCupetSpawnList();
            enemySpawnList[1] = new NextCupetSpawnList();
            emperiumSpawnList = new RandomCupetSpawnList();
            cupetPool = new GhostCupetEntityPool();
            enemyCupetPool = new GhostCupetEntityPool();
            cupetBuffList = new BetterList<ISkillDataKey>();
            turretBuffList = new BetterList<ISkillDataKey>();
            enemyCupetBuffList = new BetterList<ISkillDataKey>();
            enemyTurretBuffList = new BetterList<ISkillDataKey>();
            agentBuffer = new Buffer<CharacterEntity>();
            enemyTurrets = new UnitEntity[2]; // 적군 타워 개수 2개 (Left, Right)
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;

            cupetSpawnList.Dispose();
            for (int i = 0; i < enemySpawnList.Length; i++)
            {
                enemySpawnList[i].Dispose();
            }
            emperiumSpawnList.Dispose();
            cupetPool.Dispose();
            enemyCupetPool.Dispose();

            if (turret)
            {
                UnitActor actor = turret.GetActor();
                if (actor)
                {
                    actor.AI.ResetLookFixed();
                }

                Dispose(turret);
                turret = null;
            }

            for (int i = 0; i < enemyTurrets.Length; i++)
            {
                UnitActor actor = enemyTurrets[i].GetActor();
                if (actor)
                {
                    actor.AI.ResetLookFixed();
                }

                Dispose(enemyTurrets[i]);
                enemyTurrets[i] = null;
            }

            if (emperium)
            {
                emperium.ResetExtraOption(ExtraBattleOptionType.MaxHp);

                UnitActor actor = emperium.GetActor();
                if (actor)
                {
                    actor.AI.ResetLookFixed();
                }

                Dispose(emperium);
                emperium = null;
            }

            cupetBuffList.Release();
            turretBuffList.Release();
            enemyCupetBuffList.Release();
            enemyTurretBuffList.Release();

            SetNextSingleAction(RequestGuildBattleSeasonInfo);
        }

        protected override void StopAllCoroutine()
        {
            base.StopAllCoroutine();

            Timing.KillCoroutines(nameof(YieldSpawnCupet));
            Timing.KillCoroutines(nameof(YieldSpawnEnemyCupet));
            Timing.KillCoroutines(nameof(YieldSpawnEmperiumCupet));
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnRebirth += OnRebirthPlayer;
            dungeonModel.OnMonsterItemDrop += OnMonsterItemDrop;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnRebirth -= OnRebirthPlayer;
            dungeonModel.OnMonsterItemDrop -= OnMonsterItemDrop;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            // <!-- UI 생성, 이벤트 추가, 초기화 --!>
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiController = UI.Show<UIController>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiDanger = UI.Show<UIDanger>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleGuildAttackSkillList = UI.Show<UIBattleGuildAttackSkillList>();
            uiBattleBossHP = UI.Show<UIBattleBossHp>();
            uiBattleFail = UI.Show<UIBattleFail>();
            uiBattleCountdown = UI.Show<UIBattleCountdown>();
            uiBattleToggleOption = UI.Show<UIBattleToggleOption>();
            uiResultClear = UI.Show<UIResultClear>();
            uiBattleGuildWar = UI.Show<UIBattleGuildWar>();
            uiBattleMazeAgentList = UI.Show<UIBattleMazeAgentList>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += Finish;
            uiBattleGuildAttackSkillList.OnSelect += OnSelectSkill;
            uiBattleFail.OnConfirm += RebirthPlayer;
            uiBattleCountdown.OnFinish += OnFinishedCountdown;
            uiBattleToggleOption.OnChange += OnChangeBattleToggleOption;
            uiResultClear.OnFinishDungeon += ExitEntry;
            uiBattleGuildWar.OnFinish += Finish;

            cameraController.SetView(DEFAULT_VIEW); // 카메라 뷰 세팅

            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattleGuildWar.Hide();
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattleBossHP.Hide();
            uiBattleFail.Hide();
            uiBattleCountdown.Hide();
            uiBattleToggleOption.SetMode((UIBattleToggleOption.MenuContent.TurretTargeting, IS_DEFAULT_TURRET_TARGETING));
            uiResultClear.Hide();
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            // <!-- UI 삭제, 이벤트 해제 --!>
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= Finish;
            uiBattleGuildAttackSkillList.OnSelect -= OnSelectSkill;
            uiBattleFail.OnConfirm -= RebirthPlayer;
            uiBattleCountdown.OnFinish -= OnFinishedCountdown;
            uiBattleToggleOption.OnChange -= OnChangeBattleToggleOption;
            uiResultClear.OnFinishDungeon -= ExitEntry;
            uiBattleGuildWar.OnFinish -= Finish;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIController>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIDanger>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleGuildAttackSkillList>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattleFail>();
            UI.Close<UIBattleCountdown>();
            UI.Close<UIBattleToggleOption>();
            UI.Close<UIResultClear>();
            UI.Close<UIBattleGuildWar>();
            UI.Close<UIBattleMazeAgentList>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiController = null;
            uiBattleInfo = null;
            uiDanger = null;
            uiBattlePlayerStatus = null;
            uiBattleGuildAttackSkillList = null;
            uiBattleBossHP = null;
            uiBattleFail = null;
            uiBattleCountdown = null;
            uiBattleToggleOption = null;
            uiResultClear = null;
            uiBattleGuildWar = null;
            uiBattleMazeAgentList = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            uiBattleInfo.Set(LocalizeKey._38008.ToText()); // 길드전
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            isFinishedStartingPistol = false;
        }

        public override IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
#if UNITY_EDITOR
            if (battleInput is IEditorGuildBattleInput editorGuildBattleInput)
            {
                // Agent
                IMultiPlayerInput editorAgent = editorGuildBattleInput.GetAgent();
                if (editorAgent != null)
                {
                    guildAgent = CharacterEntity.Factory.CreateGhostPlayer();
                    guildAgent.Initialize(editorAgent);
                    guildAgent.SetDamageUnitKey(editorAgent.GetDamageUnitKey());
                }

                // Turret
                IEdiitorCupetInput[] editorCupets = editorGuildBattleInput.GetCupets();
                if (editorCupets != null)
                {
                    foreach (var item in editorCupets)
                    {
                        cupetSpawnList.AddTask(item.Id, item.Rank, item.Level);
                    }
                }

                // myBuffs
                IEditorBuffSkillInput[] editorMyBuffs = editorGuildBattleInput.GetBuffs();
                if (editorMyBuffs != null)
                {
                    foreach (var item in editorMyBuffs)
                    {
                        if (IsCupetBuff(item.Id))
                        {
                            cupetBuffList.Add(item);
                            continue;
                        }

                        if (IsEmperiumBuff(item.Id))
                        {
                            turretBuffList.Add(item);
                            continue;
                        }

                        Debug.LogError($"잘못된 버프스킬: skillId = {item.Id}");
                    }
                }

                // Enemey Turret (L)
                IEdiitorCupetInput[] editorEnemyLeftCupets = editorGuildBattleInput.GetEnemyLeftCupets();
                foreach (var item in editorEnemyLeftCupets)
                {
                    enemySpawnList[0].AddTask(item.Id, item.Rank, item.Level); // Enemey Turret (L)
                    emperiumSpawnList.AddTask(item.Id, item.Rank, item.Level); // Emperium
                }

                // Enemey Turret (R)
                IEdiitorCupetInput[] editorEnemyRightCupets = editorGuildBattleInput.GetEnemyRightCupets();
                foreach (var item in editorEnemyRightCupets)
                {
                    enemySpawnList[1].AddTask(item.Id, item.Rank, item.Level); // Enemey Turret (R)
                    emperiumSpawnList.AddTask(item.Id, item.Rank, item.Level); // Emperium
                }

                // enemyBuffs
                IEditorBuffSkillInput[] editorEnemyBuffs = editorGuildBattleInput.GetEnemyBuffs();
                if (editorEnemyBuffs != null)
                {
                    foreach (var item in editorEnemyBuffs)
                    {
                        if (IsCupetBuff(item.Id))
                        {
                            enemyCupetBuffList.Add(item);
                            continue;
                        }

                        if (IsEmperiumBuff(item.Id))
                        {
                            enemyTurretBuffList.Add(item);
                            continue;
                        }

                        Debug.LogError($"잘못된 버프스킬: skillId = {item.Id}");
                    }
                }

                initEmperiumHp = maxEmperiumHp;
                totalEmperiumDamage = 0;
                targetGuildName = "더미길드";
                isEditorMode = true;
                IsSuccessEnter = true;
                yield break;
            }

            isEditorMode = false;
#endif
            IGuildBattleInput guildBattleInput = battleInput as IGuildBattleInput;
            if (guildBattleInput == null)
            {
                IsSuccessEnter = false;
                yield break;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", guildBattleInput.GuildId);
            sfs.PutInt("2", guildBattleInput.AgentCid);
            sfs.PutInt("3", guildBattleInput.AgentUid);
            Task<Response> task = Protocol.REQUEST_GUILD_BATTLE_START_CHECK.SendAsync(sfs);
            yield return Timing.WaitUntilTrue(task.IsComplete);

            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
            {
                response.ShowResultCode();
                yield break;
            }

            targetGuildName = guildBattleInput.GuildName;
            Response res = new Response(response.GetSFSObject("1"));
            BattleCharacterPacket agent = res.ContainsKey("1") ? res.GetPacket<BattleCharacterPacket>("1") : null;
            GuildCupetPacket[] myCupets = res.ContainsKey("2") ? res.GetPacketArray<GuildCupetPacket>("2") : System.Array.Empty<GuildCupetPacket>();
            GuildBattleBuffPacket[] myBuffs = res.ContainsKey("3") ? res.GetPacketArray<GuildBattleBuffPacket>("3") : System.Array.Empty<GuildBattleBuffPacket>();
            initEmperiumHp = res.GetInt("4");
            totalEmperiumDamage = 0;
            Response enemyCupetInfo = new Response(res.GetSFSObject("5"));
            GuildCupetPacket[] enemyCupetsLeft = enemyCupetInfo.GetPacketArray<GuildCupetPacket>("1");
            GuildCupetPacket[] enemyCupetsRight = enemyCupetInfo.GetPacketArray<GuildCupetPacket>("2");
            GuildBattleBuffPacket[] enemyBuffs = res.ContainsKey("6") ? res.GetPacketArray<GuildBattleBuffPacket>("6") : System.Array.Empty<GuildBattleBuffPacket>();

            // Agent
            if (agent != null)
            {
                guildAgent = CharacterEntity.Factory.CreateGhostPlayer();
                guildAgent.Initialize(agent);
                guildAgent.SetDamageUnitKey(new DamagePacket.UnitKey(DamagePacket.DamageUnitType.SharingCharacter, agent.Cid, agent.JobLevel)); // 셰어링 캐릭터로 세팅
            }

            // Turret
            foreach (var item in myCupets)
            {
                cupetSpawnList.AddTask(item.CupetId, item.CupetRank, GetCupetLevel(item.CupetRank, item.exp));
            }

            // myBuffs
            foreach (var item in myBuffs)
            {
                item.SetLevel(guildModel.GetGuildBattleBuffLevel(item.TotalExp)); // Level 세팅

                if (IsCupetBuff(item.Id))
                {
                    cupetBuffList.Add(item);
                    continue;
                }

                if (IsEmperiumBuff(item.Id))
                {
                    turretBuffList.Add(item);
                    continue;
                }

#if UNITY_EDITOR
                Debug.LogError($"잘못된 버프스킬: skillId = {item.Id}");
#endif
            }

            // Enemey Turret (L)
            foreach (var item in enemyCupetsLeft)
            {
                int cupetLevel = GetCupetLevel(item.CupetRank, item.exp);
                enemySpawnList[0].AddTask(item.CupetId, item.CupetRank, cupetLevel); // Enemey Turret (L)
                emperiumSpawnList.AddTask(item.CupetId, item.CupetRank, cupetLevel); // Emperium
            }

            // Enemey Turret (R)
            foreach (var item in enemyCupetsRight)
            {
                int cupetLevel = GetCupetLevel(item.CupetRank, item.exp);
                enemySpawnList[1].AddTask(item.CupetId, item.CupetRank, cupetLevel); // Enemey Turret (R)
                emperiumSpawnList.AddTask(item.CupetId, item.CupetRank, cupetLevel); // Emperium
            }

            // enemyBuffs
            foreach (var item in enemyBuffs)
            {
                item.SetLevel(guildModel.GetGuildBattleBuffLevel(item.TotalExp)); // Level 세팅

                if (IsCupetBuff(item.Id))
                {
                    enemyCupetBuffList.Add(item);
                    continue;
                }

                if (IsEmperiumBuff(item.Id))
                {
                    enemyTurretBuffList.Add(item);
                    continue;
                }

#if UNITY_EDITOR
                Debug.LogError($"잘못된 버프스킬: skillId = {item.Id}");
#endif
            }
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            string sceneName = SCENE_NAME; // 씬 이름
            string bgmName = BGM_NAME; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            // <!-- 맵 정보 가져오기 --!>
            foreach (GameObject item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }

            map.SetPortalActive(true); // 가로막

            //map.GetPlayerPosition(); // 플레이어
            //map.GetBossPosition(); // 엠펠리움
            //map.GetMonsterSpawnZone(0); // 적군타워1
            //map.GetMonsterSpawnZone(1); // 적군타워2
            //map.GetGuardianPosition(); // 아군타워
            //map.GetOpponentPosition(); // 동료
            //map.GetBoxZone(); // 엠펠리움소환0~3
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            uiBattlePlayerStatus.SetPlayer(player);
            uiBattleGuildAttackSkillList.SetCharacter(player); // 스킬 조작 UI 업데이트
            uiBattleGuildWar.SetDamage(totalEmperiumDamage); // 대미지 세팅

            if (guildAgent != null)
                agentBuffer.Add(guildAgent);

            uiBattleMazeAgentList.SetShareCharacters(agentBuffer.GetBuffer(isAutoRelease: true));
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

            if (guildAgent)
                yield return Timing.WaitUntilDone(SpawnGuildAgent(), TAG);

            yield return Timing.WaitUntilDone(YieldSpawnTurret(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnEnemyTurret(), TAG);
            yield return Timing.WaitUntilDone(YieldSpawnEmperium(), TAG);
        }

        /// <summary>
        /// 씬 세팅
        /// </summary>
        protected override IEnumerator<float> YieldComposeSceneAfter()
        {
#if UNITY_EDITOR
            if (isEditorMode)
            {
                long editorServerRemainTime = playTime + 5000; // 5초 정도 추가
                uiBattleGuildWar.Initialize(editorServerRemainTime);
                uiBattleCountdown.Show(editorServerRemainTime - playTime); // 남은시간 카운트다운
                yield return Timing.WaitUntilTrue(IsFinishedStartingPistol);
                yield break;
            }
#endif

            Task<Response> task = Protocol.REQUEST_GUILD_BATTLE_START_LOADING_COMP.SendAsync();
            yield return Timing.WaitUntilTrue(task.IsComplete);

            Response response = task.Result;
            if (!response.isSuccess)
            {
                UI.ConfirmPopup(response.resultCode.GetDescription(), ExitEntry);
                yield return Timing.WaitUntilTrue(IsFinishedStartingPistol);
            }

            Quest.QuestProgress(QuestType.GUILD_BATTLE_ENTER_COUNT); // 길드전 도전 횟수

            long serverRemainTime = response.GetLong("1");
            uiBattleGuildWar.Initialize(serverRemainTime);
            uiBattleCountdown.Show(serverRemainTime - playTime); // 남은시간 카운트다운
            yield return Timing.WaitUntilTrue(IsFinishedStartingPistol);
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            uiBattleGuildWar.Show();
            uiBattleGuildWar.RestartTimer();

            Timing.RunCoroutine(YieldSpawnCupet(), nameof(YieldSpawnCupet)); // 큐펫 소환 (아군)
            Timing.RunCoroutine(YieldSpawnEnemyCupet(), nameof(YieldSpawnEnemyCupet)); // 큐펫 소환 (적군)

            UnitActor actor = player.GetActor();
            if (actor)
            {
                actor.EffectPlayer.ShowUnitCircle();
            }

            emperium.SetCurrentMp(initEmperiumHp); // 엠펠리움 Hp 적용

            ChangeFixedTargeting();
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity.IsEnemy)
            {
                if (unitEntity.type == UnitEntityType.TurretBoss)
                {
                    // 보스 엠펠리움은 OnDie가 오지 않는다.
                }
                else
                {
                    RequestGuildBattleAttackDamage(unitEntity);
                }
            }

            if (unitEntity.type == UnitEntityType.Player) // 플레이어
            {
                uiBattleFail.Show(UIBattleFail.ConfirmType.Rebirth, DEATH_COOL_TIME, isShowButton: false, player);
            }
            else if (unitEntity.type == UnitEntityType.TurretBoss) // 보스포탑
            {
                Timing.KillCoroutines(nameof(YieldSpawnEmperiumCupet)); // 엠펠리움 큐펫 소환 중지
            }
            else if (unitEntity.type == UnitEntityType.Turret) // 포탑
            {
                if (unitEntity.IsEnemy)
                {
                    ChangeFixedTargeting(); // 고정타겟팅 변경

                    // 모든 포탑이 죽었을 경우에
                    if (IsEnemyTurretAllDie())
                    {
                        Timing.KillCoroutines(nameof(YieldSpawnEnemyCupet)); // 포탑 큐펫 소환 중지 (적군)
                        Timing.RunCoroutine(YieldSpawnEmperiumCupet(), nameof(YieldSpawnEmperiumCupet)); // 엠펠리움 큐펫 소환 시작
                        map.SetPortalActive(false); // 가로막 해제

                        // 가로막 해제 후 동료 캐릭터 ResetTarget
                        if (guildAgent != null && !guildAgent.IsDie)
                        {
                            UnitActor actor = guildAgent.GetActor();
                            if (actor)
                            {
                                actor.AI.ResetTarget();
                            }
                        }
                        
                        // UI 업데이트 - 보스 타겟팅으로 변경
                        uiBattleToggleOption.SetMode((UIBattleToggleOption.MenuContent.BossTurretTargeting, isTurretTargeting));
                    }
                }
                else
                {
                    Timing.KillCoroutines(nameof(YieldSpawnCupet)); // 포탑 큐펫 소환 중지 (아군)
                }
            }
        }

        /// <summary>
        /// 유닛 대미지 처리
        /// </summary>
        public override void OnDamageValue(UnitEntity unitEntity, UnitEntity attacker, int damage)
        {
            // 엠펠리움의 경우에는 맞을 때마다 서버로 호출
            if (unitEntity.type == UnitEntityType.TurretBoss)
            {
                RequestGuildBattleAttackDamage(unitEntity);
                return;
            }

            // 아군 포링포탑의 경우 메시지 띄우기
            if (unitEntity.type == UnitEntityType.Turret && !unitEntity.IsEnemy)
            {
                if (noticeRemainTime.GetRemainTime() > 0f)
                    return;

                noticeRemainTime = NOTICE_DELAY_ATTACKED_MY_TURRET;
                UI.ShowToastPopup(LocalizeKey._90314.ToText()); // 포링 포탑이 공격받고 있습니다.
            }
        }

        /// <summary>
        /// 포탑 부서짐 (아군)
        /// </summary>
        private bool IsTurretDie()
        {
            return turret && turret.IsDie;
        }

        /// <summary>
        /// 포탑 부서짐 (적군)
        /// </summary>
        private bool IsEnemyTurretAllDie()
        {
            for (int i = 0; i < enemyTurrets.Length; i++)
            {
                // 살아있는 포탑 존재
                if (enemyTurrets[i] && !enemyTurrets[i].IsDie)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 엠펠리움 부서짐
        /// </summary>
        private bool IsEmperiumDie()
        {
            return emperium && emperium.IsDie;
        }

        /// <summary>
        /// 플레이어 부활
        /// </summary>
        private void RebirthPlayer()
        {
            UnitActor actor = player.GetActor();
            if (actor && actor.AI.ChangeState(AI.Transition.Rebirth))
            {
                uiBattleGuildAttackSkillList.SetCharacter(player);
                player.SetCurrentMp(player.MaxMp);

                actor.EffectPlayer.ShowHealEffect(); // Heal 이펙트 추가

                PlaySfx("[SYSTEM] Gacha_Result_Window");
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayer()
        {
            if (impl.Add(player, isEnemy: false))
            {
                UnitActor actor = player.SpawnActor(); // 유닛 소환
                actor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);

                cameraController.SetPlayer(actor.CachedTransform); // 카메라 타겟 세팅
                actor.AI.SetHomePosition(map.GetPlayerPosition(), isWarp: false); // 위치 세팅
                actor.AI.WarpToHomePosition();
                actor.Movement.SetRotation(map.GetPlayerRotation());
                actor.AI.ReadyToBattle();
                actor.AI.EndAI();
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 길드원 생성
        /// </summary>
        private IEnumerator<float> SpawnGuildAgent()
        {
            if (impl.Add(guildAgent, isEnemy: false))
            {
                UnitActor actor = guildAgent.SpawnActor(); // 유닛 소환
                actor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);

                actor.AI.SetHomePosition(map.GetOpponentPosition(), isWarp: false); // 위치 세팅
                actor.AI.WarpToHomePosition();
                actor.Movement.SetRotation(map.GetOpponentRotation());
                actor.AI.ReadyToBattle();
                actor.AI.EndAI();
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 포탑 생성 (아군)
        /// </summary>
        private IEnumerator<float> YieldSpawnTurret()
        {
            int id = BasisGuildWarInfo.AttackTowerId.GetInt(); // 아군
            int level = BasisGuildWarInfo.TowerLevel.GetInt(); // 레벨
            turret = factory.CreateTurret(id, level);
            turret.SetPassiveBuffSkills(turretBuffList.ToArray()); // 패시브 스킬 추가

            if (!impl.Add(turret, isEnemy: false)) // 아군 세팅
                yield break;

            UnitActor actor = turret.SpawnActor(); // 유닛 소환
            actor.AI.SetLookFixed();
            actor.AI.SetHomePosition(map.GetGuardianPosition(), isWarp: true); // 위치 세팅
            actor.Movement.SetRotation(map.GetGuardianRotation());
            actor.AI.ReadyToBattle();
            actor.AI.EndAI();
            yield return Timing.WaitForOneFrame;
        }

        /// <summary>
        /// 포탑 생성 (적군)
        /// </summary>
        private IEnumerator<float> YieldSpawnEnemyTurret()
        {
            int id = BasisGuildWarInfo.DefenceTowerId.GetInt(); // 적군
            int level = BasisGuildWarInfo.TowerLevel.GetInt(); // 레벨
            for (int i = 0; i < enemyTurrets.Length; i++)
            {
                enemyTurrets[i] = factory.CreateTurret(id, level);
                enemyTurrets[i].SetPassiveBuffSkills(enemyTurretBuffList.ToArray()); // 패시브 스킬 추가

                if (!impl.Add(enemyTurrets[i], isEnemy: true)) // 적군 세팅
                    continue;

                UnitActor actor = enemyTurrets[i].SpawnActor(); // 유닛 소환
                actor.AI.SetLookFixed();
                SpawnZone zone = map.GetMonsterSpawnZone(i);
                actor.AI.SetHomePosition(zone.GetCenter(), isWarp: true); // 위치 세팅
                actor.Movement.SetRotation(zone.GetRotation());
                actor.AI.ReadyToBattle();
                actor.AI.EndAI();
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 엠펠리움 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnEmperium()
        {
            int id = BasisGuildWarInfo.EmperiumId.GetInt();
            int level = BasisGuildWarInfo.EmperiumLevel.GetInt();
            emperium = factory.CreateTurretBoss(id, level);

            if (!impl.Add(emperium, isEnemy: true)) // 적군 세팅
                yield break;

            emperium.SetExtraOption(ExtraBattleOptionType.MaxHp, maxEmperiumHp);
            emperium.SetCurrentHp(initEmperiumHp);

            UnitActor actor = emperium.SpawnActor(); // 유닛 소환
            actor.AI.SetLookFixed();
            actor.AI.SetHomePosition(map.GetBossPosition(), isWarp: true); // 위치 세팅
            actor.Movement.SetRotation(map.GetBossRotation());
            actor.AI.ReadyToBattle();
            actor.AI.EndAI();

            MonsterData data = monsterDataRepo.Get(id);
            float hudOffset = data.GetHudOffset();
            PoolObject bossHud = actor.EffectPlayer.SetHpHudTarget(new Vector3(0, hudOffset, 0));
            string bossName = data.name_id.ToText().Replace(ReplaceKey.NAME, targetGuildName);
            uiBattleBossHP.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHP.SetOffset(UIBattleBossHp.Offset.BossMonster);
            uiBattleBossHP.Show(
                null,
                bossName,
                data.element_type.ToEnum<ElementType>(),
                UIBattleBossHp.MonsterType.NormalBoss,
                data.cost,
                bossHud.CachedGameObject);
            uiBattleBossHP.SetHp(initEmperiumHp, emperium.MaxHP);

            yield return Timing.WaitForOneFrame;
        }

        /// <summary>
        /// 포탑 큐펫 소환 (아군)
        /// </summary>
        private IEnumerator<float> YieldSpawnCupet()
        {
            if (cupetSpawnList.IsEmpty())
                yield break;

            SpawnZone zone = map.GetGuardianZone();
            while (!IsTurretDie())
            {
                // 생성 수 확인
                if (cupetPool.Size() < maxAttackCupetCount)
                {
                    UnitEntity entity = cupetPool.Create(cupetSpawnList.GetNextInput());
                    entity.SetPassiveBuffSkills(cupetBuffList.ToArray()); // 패시브 스킬 추가
                    entity.OnDespawnActor += OnDespawnCupet;

                    UnitActor actor;
                    if (impl.Add(entity, isEnemy: false)) // 아군 세팅
                    {
                        actor = entity.SpawnActor(); // 유닛 소환
                    }
                    else
                    {
                        actor = entity.GetActor();
                    }

                    actor.AI.SetHomePosition(zone.GetRandomPosition(), isWarp: false);
                    actor.AI.WarpToHomePosition();
                    actor.AI.ReadyToBattle(); // 생성과 동시에 Ready
                }

                yield return Timing.WaitForSeconds(cupetSpawnDelay);
            }
        }

        /// <summary>
        /// 포탑 큐펫 소환 (적군)
        /// </summary>
        private IEnumerator<float> YieldSpawnEnemyCupet()
        {
            int index = -1;
            while (!IsEnemyTurretAllDie())
            {
                // 생성 수 확인
                if (enemyCupetPool.Size() < maxDefenceCupetCount)
                {
                    index = GetNextEnemyTurretZoneIndex(index); // 다음 큐펫 생성 포탑 위치 확인
                    if (index == -1)
                    {
#if UNITY_EDITOR
                        Debug.LogError("잘못된 정보: 큐펫 생성 위치 불확실");
#endif
                    }
                    else if (enemySpawnList[index].IsEmpty())
                    {
#if UNITY_EDITOR
                        Debug.LogError("잘못된 정보: 큐펫 생성 정보 음슴");
#endif
                    }
                    else
                    {
                        UnitEntity entity = enemyCupetPool.Create(enemySpawnList[index].GetNextInput());
                        entity.SetPassiveBuffSkills(enemyCupetBuffList.ToArray()); // 패시브 스킬 추가
                        entity.OnDespawnActor += OnDespawnEnemyCupet;

                        UnitActor actor;
                        if (impl.Add(entity, isEnemy: true)) // 적군 세팅
                        {
                            actor = entity.SpawnActor(); // 유닛 소환
                        }
                        else
                        {
                            actor = entity.GetActor();
                        }

                        SpawnZone zone = map.GetMonsterSpawnZone(index);
                        actor.AI.SetHomePosition(zone.GetRandomPosition(), isWarp: false);
                        actor.AI.WarpToHomePosition();
                        actor.AI.ReadyToBattle(); // 생성과 동시에 Ready
                    }
                }

                yield return Timing.WaitForSeconds(cupetSpawnDelay);
            }
        }

        /// <summary>
        /// 엠펠리움 큐펫 소환
        /// </summary>
        private IEnumerator<float> YieldSpawnEmperiumCupet()
        {
            SpawnZone[] spawnZones = map.GetBoxZone();
            int maxCount = spawnZones.Length;

            int index = -1;
            while (!IsEmperiumDie())
            {
                // 생성 수 확인
                if (enemyCupetPool.Size() < maxDefenceCupetCount)
                {
                    // 다음 큐펫 생성 포탑 위치 확인
                    if (++index >= maxCount)
                        index = 0;

                    if (emperiumSpawnList.IsEmpty())
                    {
#if UNITY_EDITOR
                        Debug.LogError("잘못된 정보: 큐펫 생성 정보 음슴");
#endif
                    }
                    else
                    {
                        GhostCupetEntity entity = enemyCupetPool.Create(emperiumSpawnList.GetNextInput());
                        entity.SetPassiveBuffSkills(enemyCupetBuffList.ToArray()); // 패시브 스킬 추가
                        entity.OnDespawnActor += OnDespawnEnemyCupet;

                        UnitActor actor;
                        if (impl.Add(entity, isEnemy: true)) // 적군 세팅
                        {
                            actor = entity.SpawnActor(); // 유닛 소환
                        }
                        else
                        {
                            actor = entity.GetActor();
                        }

                        SpawnZone zone = spawnZones[index];
                        actor.AI.SetHomePosition(zone.GetRandomPosition(), isWarp: false);
                        actor.AI.WarpToHomePosition();
                        actor.AI.ReadyToBattle(); // 생성과 동시에 Ready
                    }
                }

                yield return Timing.WaitForSeconds(cupetSpawnDelay);
            }
        }

        /// <summary>
        /// 큐펫 소환을 위한 다음 포탑 index 반환
        /// </summary>
        private int GetNextEnemyTurretZoneIndex(int index)
        {
            int maxCount = enemyTurrets.Length;
            int count = maxCount;
            while (--count >= 0)
            {
                // Next Index
                if (++index >= maxCount)
                    index = 0;

                // 포탑 살아있음
                if (enemyTurrets[index] && !enemyTurrets[index].IsDie)
                    return index;
            }

            return -1;
        }

        void OnStartController()
        {
            if (!IsAllReady)
                return;

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
            if (!IsAllReady)
                return;

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

            if (actor.IsPlayingActiveSkill()) // 액티브스킬 사용 중 이동 불가
                return;

            if (actor.AI.IsInputMove() == false) // OnStartController를 거치지 않고 Drag상태로 시작한 경우, 프로펠러 현상을 방지.
            {
                actor.AI.SetInputMove(isControl: true);
            }

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);
        }

        void OnResetController()
        {
            if (!IsAllReady)
                return;

            CameraUtils.Zoom(CameraZoomType.None);

            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        private void Finish()
        {
            if (!IsAllReady)
                return;

            // 이미 프로토콜을 보낸적이 있음
            if (isRequestFinishProtocol)
                return;

            isRequestFinishProtocol = true;

            // 작동중인 코루틴 종료 (큐펫 소환 중지)
            StopAllCoroutine();

            // 전투 종료
            foreach (var item in unitList)
            {
                UnitActor actor = item.GetActor();
                if (actor == null)
                    continue;

                actor.AI.EndAI();
            }

#if UNITY_EDITOR
            if (isEditorMode)
            {
                OnMonsterItemDrop(null, 0, 0, 0, null);
                return;
            }
#endif

            dungeonModel.RequestGuildBattleEnd().WrapNetworkErrors();
        }

        /// <summary>
        /// 스킬 선택 사용
        /// </summary>
        void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
        {
            if (!IsAllReady)
                return;

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

        void OnFinishedCountdown()
        {
            isFinishedStartingPistol = true;
        }

        void OnChangeBattleToggleOption(UIBattleToggleOption.MenuContent content, bool value)
        {
            switch (content)
            {
                case UIBattleToggleOption.MenuContent.TurretTargeting:
                case UIBattleToggleOption.MenuContent.BossTurretTargeting:
                    isTurretTargeting = value;
                    ChangeFixedTargeting();
                    break;
            }
        }

        void OnRebirthPlayer()
        {
            ChangeFixedTargeting();
        }

        void OnDespawnCupet(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnDespawnCupet;

            if (impl.Remove(actor.Entity)) // UnitList 에서 제거
            {
                cupetPool.Recycle(actor.Entity as GhostCupetEntity);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("존재하지 않는 Cupet 제거");
#endif
            }
        }

        void OnDespawnEnemyCupet(UnitActor actor)
        {
            actor.Entity.OnDespawnActor -= OnDespawnEnemyCupet;

            if (impl.Remove(actor.Entity)) // UnitList 에서 제거
            {
                enemyCupetPool.Recycle(actor.Entity as GhostCupetEntity);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("존재하지 않는 EnemyCupet 제거");
#endif
            }
        }

        void OnMonsterItemDrop(UnitEntity unit, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards)
        {
            // 기존 StageEntry 에서 넘어오는 OnMonsterItemDrop 이 호출될 수 있음
            if (!IsAllReady)
                return;

            if (droppedZeny > 0)
                rewardBuffer.Add(new RewardData(RewardType.Zeny, droppedZeny, rewardCount: 0, rewardOption: 0));

            if (droppedLevelExp > 0)
                rewardBuffer.Add(new RewardData(RewardType.LevelExp, droppedLevelExp, rewardCount: 0, rewardOption: 0));

            if (droppedJobExp > 0)
                rewardBuffer.Add(new RewardData(RewardType.JobExp, droppedJobExp, rewardCount: 0, rewardOption: 0));

            if (rewards != null)
                rewardBuffer.AddRange(rewards);

            uiResultClear.Show(rewardBuffer.GetBuffer(isAutoRelease: true), default, false, UIResultClear.ResultType.Result);
            uiResultClear.SetDescription(string.Empty);

            // GhostCupet 모두 Despawn
            for (int i = unitList.size - 1; i >= 0; i--)
            {
                if (unitList[i].type != UnitEntityType.GhostCupet)
                    return;

                unitList[i].DespawnActor();
            }
        }

        private bool IsFinishedStartingPistol()
        {
            return isFinishedStartingPistol;
        }

        private void ChangeFixedTargeting()
        {
            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.AI.SetFixedTarget(GetFixedTargetingActor(actor));
        }

        /// <summary>
        /// 가장 가까운 타겟팅 반환
        /// </summary>
        private UnitActor GetFixedTargetingActor(UnitActor actor)
        {
            if (!isTurretTargeting)
                return null;

            // 모든 포탑 부서짐
            if (IsEnemyTurretAllDie())
                return emperium.GetActor();

            return unitList.FindMinTarget(actor, TargetType.Enemy, UnitEntityType.Turret); // 가장 가까운 포탑
        }

        /// <summary>
        /// 길드 레벨 반환
        /// </summary>
        private int GetGuildLevel(int guildExp)
        {
            return guildModel.GetGuildLevel(guildExp);
        }

        /// <summary>
        /// 큐펫 레벨 반환
        /// </summary>
        private int GetCupetLevel(int rank, int cupetExp)
        {
            int maxLevel = BasisType.MAX_CUPET_LEVEL.GetInt(rank);
            return Mathf.Min(maxLevel, expDataRepo.Get(cupetExp, ExpDataManager.ExpType.Cupet).level);
        }

        /// <summary>
        /// 큐펫 버프 여부
        /// </summary>
        private bool IsCupetBuff(int skillId)
        {
            if (skillId == BasisGuildWarInfo.BuffSkill1.GetInt())
                return true;

            if (skillId == BasisGuildWarInfo.BuffSkill2.GetInt())
                return true;

            if (skillId == BasisGuildWarInfo.BuffSkill3.GetInt())
                return true;

            if (skillId == BasisGuildWarInfo.BuffSkill4.GetInt())
                return true;

            return false;
        }

        /// <summary>
        /// 엠펠리움 버프 여부
        /// </summary>
        private bool IsEmperiumBuff(int skillId)
        {
            if (skillId == BasisGuildWarInfo.BuffSkill5.GetInt())
                return true;

            return false;
        }

        /// <summary>
        /// 엠펠리움 대미지 추가
        /// </summary>
        private void PlusEmperiumDamage(int damage)
        {
            totalEmperiumDamage = Mathf.Min(totalEmperiumDamage + damage, initEmperiumHp);
            uiBattleGuildWar.SetDamage(totalEmperiumDamage);

            int emperiumHp = initEmperiumHp - totalEmperiumDamage;
            emperium.SetCurrentHp(emperiumHp);
            uiBattleBossHP.TweenHp(emperiumHp, maxEmperiumHp);

            // 종료
            if (emperiumHp == 0)
            {
                Finish();
            }
        }

        /// <summary>
        /// 대미지 패킷 보내기
        /// </summary>
        private void RequestGuildBattleAttackDamage(UnitEntity entity)
        {
            // 종료 시도 후 Damage 패킷 막기
            if (isRequestFinishProtocol)
                return;

#if UNITY_EDITOR
            if (isEditorMode)
            {
                if (entity.type == UnitEntityType.TurretBoss)
                {
                    IDamageTuple damageTuple = entity.damagePacket;
                    PlusEmperiumDamage(damageTuple.FinalDamage);
                }
                return;
            }
#endif
            AsyncRequestGuildBattleAttackDamage(entity.damagePacket).WrapNetworkErrors();
        }

        /// <summary>
        /// 길드 진입 정보 호출
        /// </summary>
        private void RequestGuildBattleSeasonInfo()
        {
#if UNITY_EDITOR
            if (isEditorMode)
                return;
#endif
            guildModel.RequestGuildBattleSeasonInfo().WrapNetworkErrors();
        }

        private async Task AsyncRequestGuildBattleAttackDamage(DamagePacket damagePacket)
        {
            IDamageTuple damageTuple = damagePacket;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", damageTuple.FinalDamage);
            sfs.PutByteArray("2", damagePacket.ToByteArray()); // 대미지 패킷

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            Response response = await Protocol.REQUEST_GUILD_BATTLE_ATTACK_DAMAGE.SendAsync(sfs);
            if (!response.isSuccess)
                return;

            if (response.ContainsKey("1"))
            {
                int damage = response.GetInt("1");
                PlusEmperiumDamage(damage);
            }

#if UNITY_EDITOR
            if (response.ContainsKey("3"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("3")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
        }

        private abstract class CupetSpawnList : System.IDisposable
        {
            protected readonly BetterList<IMultiCupetInput> spawnList;

            public CupetSpawnList()
            {
                spawnList = new BetterList<IMultiCupetInput>();
            }

            public virtual void Dispose()
            {
                spawnList.Release();
            }

            public bool IsEmpty()
            {
                return spawnList.size == 0;
            }

            public void AddTask(int id, int rank, int level)
            {
                spawnList.Add(new CupetSpawnInfo(id, rank, level));
            }

            public abstract IMultiCupetInput GetNextInput();

            protected IMultiCupetInput GetInput(int index)
            {
                return spawnList[index];
            }

            private class CupetSpawnInfo : IMultiCupetInput
            {
                public int Id { get; private set; }
                public int Rank { get; private set; }
                public int Level { get; private set; }

                public CupetSpawnInfo(int id, int rank, int level)
                {
                    Id = id;
                    Rank = rank;
                    Level = level;
                }
            }
        }

        private class NextCupetSpawnList : CupetSpawnList
        {
            private int createIndex = -1;

            public override void Dispose()
            {
                base.Dispose();

                createIndex = -1;
            }

            public override IMultiCupetInput GetNextInput()
            {
                if (IsEmpty())
                    return null;

                // Next
                if (++createIndex >= spawnList.size)
                    createIndex = 0;

                return GetInput(createIndex);
            }
        }

        private class RandomCupetSpawnList : CupetSpawnList
        {
            public override IMultiCupetInput GetNextInput()
            {
                if (IsEmpty())
                    return null;

                int randNum = Random.Range(0, spawnList.size);
                return GetInput(randNum);
            }
        }
    }
}