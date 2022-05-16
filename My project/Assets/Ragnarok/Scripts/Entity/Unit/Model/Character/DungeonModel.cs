using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 던전 정보
    /// </summary>
    public class DungeonModel : CharacterEntityModel
    {
        private const string TAG = nameof(DungeonModel);

        public delegate void MonsterItemDropEvent(UnitEntity unit, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards);
        public delegate void MvpMonsterItemDropEvent(UnitEntity unit, RewardData[] rewards, RewardData[] wasted, int duelAlphabetIndex, bool isGainedArenaPoint);
        public delegate void BossMonsterItemDropEvent(UnitEntity unit, RewardData[] rewards);
        public delegate void TimePatrolBossMonsterItemDropEvent(UnitEntity unit, RewardData[] rewards, RewardData[] wasted, long coolTime, bool isClear);
        public delegate void MultiJoinRewardEvent(int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards);
        public delegate void MvpMonsterAppearEvent(int id, long remainTime);
        public delegate void BossMonsterAppearEvent(int remainTime);
        public delegate void TimePatrolBossMonsterAppearEvent(int id, int remainTime);
        public delegate void CentralLabMonsterKillEvent(RewardData[] rewards, int itemSkillPoint, int itemSkillId, CentralLabSkillPacket[] selectSkills);
        public delegate void CentralLabExitEvent(RewardData[] rewards);
        public delegate void EndlessTowerExitEvent(int floor, RewardData[] rewards);
        public delegate void ForestMazeExitEvent(RewardData[] rewards);
        public delegate void ClickerDungeonEndEvent(RewardData[] rewards);
        public delegate void EventDarkMazeRewardEvent(int cid, int droppedZeny, int droppedLevelExp, int droppedJobExp, RewardData[] rewards);

        public static int StageChapter { get; private set; }
        public static event System.Action OnUpdateStageChapter;

        private readonly BattleManager battleManager;
        private readonly MonsterDataManager monsterDataRepo;
        private readonly AdventureDataManager adventureDataRepo;
        private readonly ScenarioMazeDataManager scenarioMazeDataRepo;
        private readonly StageDataManager stageDataRepo;
        private readonly TimePatrolStageDataManager timePatrolStageDataRepo;
        private readonly ClickerDungeonDataManager clickerDungeonDataRepo;
        private readonly WorldBossDataManager worldBossDataRepo;
        private readonly DefenceDungeonDataManager defenceDungeonDataRepo;
        private readonly CentralLabDataManager centralLabDataRepo;
        private readonly TamingDataManager tamingDataRepo;
        private readonly EndlessTowerDataManager endlessTowerDataRepo;
        private readonly ForestBaseDataManager forestBaseDataRepo;
        private readonly Buffer<RewardData> rewardBuffer; // 던전 드랍 아이템 이벤트를 호출하기 위해 필요
        private ObscuredInt finalStageId;
        private ObscuredInt lastEnterStageId;
        private ObscuredInt dungeonFreeTicket, dungeonEntryCount;
        private ObscuredInt centralLabFreeTicket, centralLabTryCount;
        private ObscuredInt worldBossFreeTicket, worldBossEntryCount;
        private ObscuredInt defenceDungeonFreeTicket, defenceDungeonEntryCount;
        private ObscuredInt selectWorldBossId;
        private ObscuredInt selectWorldBossMaxHp;
        private ObscuredInt dungeonFreeReward;
        private readonly Dictionary<int, RemainTime> worldBossRemainTime; // 월드보스 오픈까지 남은시간    
        private readonly List<int> worldbossAlarm;
        public readonly List<int> worldbossOpenIds;
        private RemainTime worldBossTicketRemainTime;
        private ObscuredByte gameStartMapType; // 게임 시작위치
        private ObscuredInt mazeHp;
        private ObscuredInt mazeMp;
        private readonly Dictionary<int, ObscuredInt> mazeCupetHpDic;
        private readonly HashSet<int> cleardScenarioMazeIds; // 클리어한 시나리오 미로 ID 목록
        private readonly List<ContentType> openedContentsByScenario;
        private int lastClearedScenarioID;

        private ObscuredInt dayMultiMazeTicket; // 멀티미로 클리어시 소모되는 티켓
        private ObscuredInt dayMultiMazeCount; // 멀티미로 클리어 횟수
        private ObscuredInt summonMvpTicket; // MVP 몬스터 소환 티켓
        private ObscuredInt dayZenyDungeonTicket; // 제니 던전 티켓
        private ObscuredInt dayZenyDungeonCount; // 제니 던전 클리어 횟수
        private ObscuredInt dayExpDungeonTicket; // 경험치 던전 티켓
        private ObscuredInt dayExpDungeonCount; // 경험치 던전 클리어 횟수
        private ObscuredInt eventMultiMazeFreeTicket; // 이벤트미로 클리어시 소모되는 티켓
        private ObscuredInt eventMultiMazeEntryCount; // 이벤트미로 입장 횟수
        private ObscuredInt lastEnterMultiMazeLobbyId; // 마지막에 입장한 멀티미로 로비 ID
        private ObscuredInt lastEnterLobbyChannel = 1; // 마지막에 입장한 로비 채널
        private ObscuredInt lastEnterMultiMazeId; // 시나리오 미로 입장시 저장 멀티미로 ID
        private ObscuredInt endlessTowerFreeTicket; // 엔들리스 타워 티켓
        private ObscuredInt endlessTowerClearedFloor; // 엔들리스 타워 클리어한 층
        private ObscuredInt forestMazeFreeTicket; // 미궁숲 무료티켓
        public bool IsEnteredEventDarkMaze { get; private set; } // 이벤트미궁 입장 여부

        private ObscuredInt clearedZenyDungeonId; // 클리어한 제니 던전 id
        private ObscuredInt clearedExpDungeonId; // 클리어한 경험치 던전 id
        private ObscuredInt clearedDefenceDungeonId; // 클리어한 디팬스 던전 id
        private ObscuredInt clearedCentralLabId; // 클리어한 중앙실험실 던전 id
        private int serverMonsterRandomKey; // 서버에서 받은 몬스터 랜덤 키
        private int serverMvpMonsterRandomKey; // 서버에서 받은 MVP몬스터 랜덤 키
        private int serverBossMonsterRandomKey; // 서버에서 받은 보스몬스터 랜덤 키
        private int serverDefenceMonsterRandomKey; // 서버에서 받은 디팬스몬스터 랜덤 키

        private ObscuredString maxStageCoupon; // 최종 스테이지 쿠폰

        private readonly Dictionary<int, int> eventStageLevel; // 이벤트(이벤트,챌린지) 클리어 횟수 <스테이지ID, 레벨>
        private readonly Dictionary<int, int> challengClearCount; // 첼린지모드 클리어 횟수 <스테이지ID, 클리어횟수>
        private RemainTime eventStageRemainTime; // 이벤트모드 종료까지 남은시간
        private int eventStageSeq; // 이벤트모드 시퀀스

        public int LastClearedScenarioID => lastClearedScenarioID;

        /// <summary>
        /// 게임 시작 맵 위치
        /// </summary>
        public GameStartMapType GameStartMapType => gameStartMapType.ToEnum<GameStartMapType>();

        /// <summary>
        /// 몬스터 아이템 드랍 이벤트
        /// </summary>
        public event MonsterItemDropEvent OnMonsterItemDrop;

        /// <summary>
        /// Mvp몬스터 아이템 드랍 이벤트
        /// </summary>
        public event MvpMonsterItemDropEvent OnMvpMonsterItemDrop;

        /// <summary>
        /// 보스몬스터 아이템 드랍 이벤트
        /// </summary>
        public event BossMonsterItemDropEvent OnBossMonsterItemDrop;

        /// <summary>
        /// 멀티미로 참여보상 이벤트
        /// </summary>
        public event MultiJoinRewardEvent OnMultiJoinRewardEvent;

        /// <summary>
        /// 중앙실험실 몬스터 처치 이벤트
        /// </summary>
        public event CentralLabMonsterKillEvent OnCentralLabMonsterKill;

        /// <summary>
        /// 중앙실험실 종료 시 이벤트
        /// </summary>
        public event CentralLabExitEvent OnCentralLabExit;

        /// <summary>
        /// 엔들리스 타워 종료 시 이벤트
        /// </summary>
        public event EndlessTowerExitEvent OnEndlessTowerExit;

        /// <summary>
        /// 미궁숲 종료 시 이벤트
        /// </summary>
        public event ForestMazeExitEvent OnForestMazeExit;

        /// <summary>
        /// 클리커 던전 종료 시 이벤트
        /// </summary>
        public event ClickerDungeonEndEvent OnClickerDungeonEnd;

        /// <summary>
        /// 이벤트미궁 보상 이벤트
        /// </summary>
        public event EventDarkMazeRewardEvent OnEventDarkMazeRewardEvent;

        /// <summary>
        /// 도달한 스테이지
        /// </summary>
        public int FinalStageId => finalStageId;

        /// <summary>
        /// 마지막에 입장한 스테이지 ID
        /// </summary>
        public int LastEnterStageId => lastEnterStageId;

        /// <summary>
        /// 선택한 월드보스 ID
        /// </summary>
        public int SelectWorldBossId => selectWorldBossId;

        /// <summary>
        /// 선택한 월드보스 MAX HP
        /// </summary>
        public int WorldBossMaxHp => selectWorldBossMaxHp;

        /// <summary>
        /// 월드보스 무료티켓 충전까지 남은시간
        /// </summary>
        public float WorldBossFreeTicketCoolTime => worldBossTicketRemainTime.ToRemainTime();

        /// <summary>
        /// 월드보스 무료티켓 최대수량인지 여부
        /// </summary>
        public bool IsMaxFreeTicket => worldBossFreeTicket == BasisType.WORLD_BOSS_FREE_JOIN_CNT.GetInt();

        /// <summary>
        /// 미로 전투 Hp
        /// </summary>
        public int MazeHp => mazeHp;

        /// <summary>
        /// 미로 전투 Mp
        /// </summary>
        public int MazeMp => mazeMp;

        /// <summary>
        /// 마지막에 입장한 멀티미로 로비 ID
        /// </summary>
        public int LastEnterMultiMazeLobbyId => lastEnterMultiMazeLobbyId;

        /// <summary>
        /// 시나리오 미로 입장시 저장 멀티미로 ID
        /// </summary>
        public int LastEnterMultiMazeId => lastEnterMultiMazeId;

        /// <summary>
        /// 마지막에 입장한 로비 채널
        /// </summary>
        public int LastEnterLobbyChannel => lastEnterLobbyChannel;

        /// <summary>
        /// mvp 소환 티켓 수
        /// </summary>
        public int SummonMvpTicketCount => summonMvpTicket;

        /// <summary>
        /// 이벤트 스테이지 남은 시간
        /// </summary>
        public RemainTime EventStageRemainTime => eventStageRemainTime;

        /// <summary>
        /// 입장 요청한 스테이지 모드
        /// </summary>
        public StageMode RequestStageMode { get; private set; }

        /// <summary>
        /// 진행중인 스테이지 모드
        /// </summary>
        public StageMode StageMode { get; private set; }

        /// <summary>
        /// 엔들리스 타워 클리어한 층
        /// </summary>
        public int EndlessTowerClearedFloor => endlessTowerClearedFloor;

        /// <summary>
        /// 엔들리스 타워 무료티켓 충전까지 남은시간
        /// </summary>
        public RemainTime EndlessTowerFreeTicketCoolTime { get; private set; }

        /// <summary>
        /// 마지막으로 입장한 타임패트롤 ID
        /// </summary>
        public int LastEnterTimePatrolId { get; private set; }

        /// <summary>
        /// 도달한 타임패트롤 레벨
        /// </summary>
        public int FinalTimePatrolLevel { get; private set; }

        /// <summary>
        /// 티켓 수량 변경 이벤트
        /// </summary>
        public System.Action OnUpdateTicket;

        /// <summary>
        /// 월드보스 오픈 이벤트
        /// </summary>
        public System.Action OnUpdateWorldBossOpen;

        /// <summary>
        /// 미로 HP 변경 이벤트
        /// </summary>
        public System.Action OnChangeMazeHp;

        /// <summary>
        /// 미로 MP 변경 이벤트
        /// </summary>
        public System.Action OnChangeMazeMp;

        /// <summary>
        /// 큐브 드랍 이벤트
        /// </summary>
        public System.Action<UnitEntity> OnDropCube;

        /// <summary>
        /// MVP 몬스터 등장 이벤트
        /// </summary>
        public event MvpMonsterAppearEvent OnAppearMvpMonster;

        /// <summary>
        /// 보스 몬스터 등장 이벤트
        /// </summary>
        public event BossMonsterAppearEvent OnAppearBossMonster;

        /// <summary>
        /// [타임패트롤] 보스 몬스터 등장 이벤트
        /// </summary>
        public event TimePatrolBossMonsterAppearEvent OnAppearTimePatrolBossMonster;

        /// <summary>
        /// [타임패트롤] 보스 몬스터 드랍 이벤트
        /// </summary>
        public event TimePatrolBossMonsterItemDropEvent OnTimePatrolBossMonsterItemDrop;

        /// <summary>
        /// 멀티 미로 티켓 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateMultiMazeTicket;

        /// <summary>
        /// MVP 몬스터 소환 티켓 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateSummonMvpTicket;

        /// <summary>
        /// 제니 던전 티켓 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateZenyDungeonTicket;

        /// <summary>
        /// 경험치 던전 티켓 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateExpDungeonTicket;

        /// <summary>
        /// 중앙실험실 티켓 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateCentralLabTicket;

        /// <summary>
        /// 이벤트 멀티 미로 티켓 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateEventMultiMazeTicket;

        /// <summary>
        /// 던전 무료 보상 이벤트
        /// </summary>
        public event System.Action OnUpdateView;

        /// <summary>
        /// 스테이지 클리어
        /// </summary>
        public event System.Action OnUpdateClearedStage;

        /// <summary>
        /// 이벤트 스테이지 정보 업데이트
        /// </summary>
        public event System.Action OnUpdateEventStageInfo;

        /// <summary>
        /// 이벤트 스테이지 클리어횟수 업데이트
        /// </summary>
        public event System.Action OnUpdateEventStageCount;

        /// <summary>
        /// 엔들리스 타워 무료입장권 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateEndlessTowerFreeTicket;

        /// <summary>
        /// 던전 소탕 이벤트
        /// </summary>
        public event System.Action OnFastClear;

        public DungeonModel()
        {
            battleManager = BattleManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
            adventureDataRepo = AdventureDataManager.Instance;
            scenarioMazeDataRepo = ScenarioMazeDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;
            timePatrolStageDataRepo = TimePatrolStageDataManager.Instance;
            clickerDungeonDataRepo = ClickerDungeonDataManager.Instance;
            worldBossDataRepo = WorldBossDataManager.Instance;
            defenceDungeonDataRepo = DefenceDungeonDataManager.Instance;
            centralLabDataRepo = CentralLabDataManager.Instance;
            tamingDataRepo = TamingDataManager.Instance;
            endlessTowerDataRepo = EndlessTowerDataManager.Instance;
            forestBaseDataRepo = ForestBaseDataManager.Instance;
            rewardBuffer = new Buffer<RewardData>();
            worldBossRemainTime = new Dictionary<int, RemainTime>(IntEqualityComparer.Default);
            worldbossAlarm = new List<int>();
            worldbossOpenIds = new List<int>();
            mazeCupetHpDic = new Dictionary<int, ObscuredInt>(IntEqualityComparer.Default);
            cleardScenarioMazeIds = new HashSet<int>(IntEqualityComparer.Default);
            openedContentsByScenario = new List<ContentType>();
            lastClearedScenarioID = 0;
            RequestStageMode = StageMode.Normal;
            StageMode = StageMode.Normal;
            eventStageLevel = new Dictionary<int, int>(IntEqualityComparer.Default);
            challengClearCount = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RECEIVE_WORLD_BOSS_ALARM.AddEvent(OnResponseWorldBossAlarm);
                Protocol.REQUEST_AUTO_STAGE_MON_DROP.AddEvent(OnStageNormalMonsterDrop);
                Protocol.REQUEST_DEF_DUNGEON_ITEM_DROP.AddEvent(OnDefenceDungeonMonsterDrop);
                Protocol.RECEIVE_MULMAZE_BASEREWARD.AddEvent(OnReceiveMultiMazeBaseReward);
                Protocol.REQUEST_MULMAZE_BOSSBATTLE_END.AddEvent(OnResponseMultiMazeBossBattleEnd);
                Protocol.RECEIVE_MULMAZE_REWARD.AddEvent(OnReceiveEventMultiMazeReward);
                Protocol.ENDLESS_DUNGEON_END.AddEvent(OnReceiveEndlessTowerEnd);
                Protocol.REQUEST_TP_MON_KILL.AddEvent(OnTimePatrolNormalMonsterDrop);
                Protocol.RECIEVE_FOREST_ROOM_EXIT.AddEvent(OnForestMazeUserExit);
                Protocol.RECEIVE_DARKMAZE_REWARD.AddEvent(OnReceiveDarkMazeReward);
                Protocol.REQUEST_GATE_MAKEROOM.AddEvent(OnRequestGateMakeRoom);
                Protocol.REQUEST_GATE_JOINROOM.AddEvent(OnRequestGateJoinRoom);
                Protocol.RECIEVE_GATE_ROOM_EXIT.AddEvent(OnRecieveGateRoomExit);

                BattleManager.OnStart += OnStartBattle;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            Timing.KillCoroutines(TAG);

            if (type == UnitEntityType.Player)
            {
                Timing.KillCoroutines(nameof(YieldCheckFinishEventStage));
                Timing.KillCoroutines(nameof(YieldCheckFinishEndlessTower));

                Protocol.RECEIVE_WORLD_BOSS_ALARM.RemoveEvent(OnResponseWorldBossAlarm);
                Protocol.REQUEST_AUTO_STAGE_MON_DROP.RemoveEvent(OnStageNormalMonsterDrop);
                Protocol.REQUEST_DEF_DUNGEON_ITEM_DROP.RemoveEvent(OnDefenceDungeonMonsterDrop);
                Protocol.RECEIVE_MULMAZE_BASEREWARD.RemoveEvent(OnReceiveMultiMazeBaseReward);
                Protocol.REQUEST_MULMAZE_BOSSBATTLE_END.RemoveEvent(OnResponseMultiMazeBossBattleEnd);
                Protocol.RECEIVE_MULMAZE_REWARD.RemoveEvent(OnReceiveEventMultiMazeReward);
                Protocol.ENDLESS_DUNGEON_END.RemoveEvent(OnReceiveEndlessTowerEnd);
                Protocol.REQUEST_TP_MON_KILL.RemoveEvent(OnTimePatrolNormalMonsterDrop);
                Protocol.RECIEVE_FOREST_ROOM_EXIT.RemoveEvent(OnForestMazeUserExit);
                Protocol.RECEIVE_DARKMAZE_REWARD.RemoveEvent(OnReceiveDarkMazeReward);
                Protocol.REQUEST_GATE_MAKEROOM.RemoveEvent(OnRequestGateMakeRoom);
                Protocol.REQUEST_GATE_JOINROOM.RemoveEvent(OnRequestGateJoinRoom);
                Protocol.RECIEVE_GATE_ROOM_EXIT.RemoveEvent(OnRecieveGateRoomExit);

                BattleManager.OnStart -= OnStartBattle;
            }
        }

        public override void ResetData()
        {
            base.ResetData();

            finalStageId = 1; // 스테이지 기본값 1
            lastEnterStageId = 1;
            cleardScenarioMazeIds.Clear();
            openedContentsByScenario.Clear();
            lastClearedScenarioID = 0;
            clearedZenyDungeonId = 0;
            clearedExpDungeonId = 0;
            clearedDefenceDungeonId = 0;
            clearedCentralLabId = 0;
            maxStageCoupon = null;
            serverMonsterRandomKey = 0;
            serverMvpMonsterRandomKey = 0;
            serverBossMonsterRandomKey = 0;
            serverDefenceMonsterRandomKey = 0;
            selectWorldBossId = 0;
            RequestStageMode = StageMode.Normal;
            StageMode = StageMode.Normal;
            eventStageLevel.Clear();
            ResetChallengClearCount();
            eventStageRemainTime = 0L;
            eventStageSeq = 0;
            EndlessTowerFreeTicketCoolTime = 0L;
            endlessTowerClearedFloor = 0;
            FinalTimePatrolLevel = 1;
            LastEnterTimePatrolId = 1;

            if (Entity.type == UnitEntityType.Player)
            {
                StageChapter = 0;
            }
        }

        void OnStartBattle(BattleMode mode)
        {
            if (mode != BattleMode.Stage)
            {
                SetChapter(0);
                return;
            }

            StageData currentStageData = stageDataRepo.Get(LastEnterStageId);
            if (currentStageData == null)
            {
                SetChapter(0);
                return;
            }

            if (StageMode != StageMode.Normal)
            {
                SetChapter(0);
                return;
            }

            SetChapter(currentStageData.chapter);
        }

        internal void ResetChallengClearCount()
        {
            challengClearCount.Clear();
            OnUpdateEventStageCount?.Invoke();
        }

        internal void Initialize(CharacterPacket characterPacket)
        {
            dungeonFreeTicket = characterPacket.dungeonFreeTicket;
            dungeonEntryCount = characterPacket.dungeonCount;
            centralLabFreeTicket = characterPacket.centralLabFreeTicket;
            centralLabTryCount = characterPacket.centralLabTryCount;
            worldBossFreeTicket = characterPacket.dayWorldBossTicket;
            worldBossEntryCount = characterPacket.dayWorldBossCount;
            defenceDungeonFreeTicket = characterPacket.dayDefDungeonTicket;
            defenceDungeonEntryCount = characterPacket.dayDefDungeonCount;
            worldBossTicketRemainTime = characterPacket.worldBossTicketRemainTime;
            dungeonFreeReward = characterPacket.dungeonFreeReward;

            SetFinalStageId(characterPacket.finalStageId);
            SetLastEnterStageId(characterPacket.autoStageId);

            // 알람 등록된 월드보스 ID 세팅
            if (!string.IsNullOrEmpty(characterPacket.worldBossAlarm))
            {
                worldbossAlarm.Clear();
                foreach (var item in characterPacket.worldBossAlarm.Split(',').OrEmptyIfNull())
                {
                    if (int.TryParse(item, out int worldBossId))
                        worldbossAlarm.Add(worldBossId);
                }
            }

            gameStartMapType = characterPacket.gameStartMapType;

            dayMultiMazeTicket = characterPacket.dayMultiMazeTicket;
            dayMultiMazeCount = characterPacket.dayMultiMazeCount;
            summonMvpTicket = characterPacket.summonMvpTicket;
            dayZenyDungeonTicket = characterPacket.dayZenyDungeonTicket;
            dayZenyDungeonCount = characterPacket.dayZenyDungeonCount;
            dayExpDungeonTicket = characterPacket.dayExpDungeonTicket;
            dayExpDungeonCount = characterPacket.dayExpDungeonCount;
            eventMultiMazeFreeTicket = characterPacket.eventMultiMazeFreeTicket;
            eventMultiMazeEntryCount = characterPacket.eventMultiMazeEntryCount;

            SetCleardScenarioMazeIds(characterPacket.clearScenarioMazeIds);
            SetClearedDungeonGroupId(characterPacket.clearDungeonGroupIdText);
            SetEndlessTowerClearedFloor(characterPacket.endlessTowerClearedFloor);
            UpdateEndlessTowerTicket(characterPacket.endlessTowerFreeTicket, characterPacket.endlessTowerCooldownTime);
            UpdateForestMazeTicket(characterPacket.forestMazeFreeTicket, characterPacket.forestMazeEntryCount);
            UpdateEventDarkMazeEntryFlag(characterPacket.isEnteredEventDarkMaze);

            RunCorutine();
        }

        private void RunCorutine()
        {
            Timing.KillCoroutines(TAG);
            Timing.RunCoroutine(UpdateWorldBossTime(), TAG);
            Timing.RunCoroutine(UpdateWorldBossFreeTicketCoolTime(), TAG);
        }

        internal void Initialize(WorldBossAlarmPacket[] arrPacket)
        {
            if (arrPacket != null)
            {
                worldBossRemainTime.Clear();
                foreach (var item in arrPacket)
                {
#if UNITY_EDITOR
                    Debug.Log($"=== 월드보스 오픈까지 남은시간 === {item.worldBossId}, {item.remainTime}");
#endif
                    worldBossRemainTime.Add(item.worldBossId, item.remainTime);
                }
            }
        }

        internal void Initialize(EventChallengePacket packet)
        {
            eventStageLevel.Clear();
            ResetChallengClearCount();

            SetEventStageInfo(packet.eventStagePacket); // Level 및 Count 세팅 전 호출

            for (int i = 0; i < packet.stageEventModes.Length; i++)
            {
                eventStageLevel.Add(packet.stageEventModes[i].stageId, packet.stageEventModes[i].level);
                Debug.Log($"[이벤트 모드] stageId={packet.stageEventModes[i].stageId}, level={packet.stageEventModes[i].level}");
            }

            for (int i = 0; i < packet.stageChallengeModes.Length; i++)
            {
                if (!eventStageLevel.ContainsKey(packet.stageChallengeModes[i].stageId))
                {
                    eventStageLevel.Add(packet.stageChallengeModes[i].stageId, packet.stageChallengeModes[i].level);
                    challengClearCount.Add(packet.stageChallengeModes[i].stageId, packet.stageChallengeModes[i].clearCount);
                    Debug.Log($"[챌린지 모드] stageId={packet.stageChallengeModes[i].stageId}, level={packet.stageChallengeModes[i].level}, clearCount={packet.stageChallengeModes[i].clearCount}");
                }
                else
                {
                    Debug.LogError($"챌린지 모드 키중복 stageId={packet.stageChallengeModes[i].stageId}");
                }
            }
        }

        internal void InitializeFinalTimePatrolLevel(int level)
        {
            SetFinalTimePatrolLevel(level);
            Debug.Log($"[타임패트롤] FinalTimePatrolLevel={level}");
            Debug.Log($"[타임패트롤] 최대 레벨={BasisType.TP_MAX_LEVEL.GetInt()}");
        }

        internal void InitializeLastTimePatrolId(int id)
        {
            SetLastEnterTimePatrolId(id);
            Debug.Log($"[타임패트롤] LastEnterTimePatrolId={id}");
            Debug.Log($"[타임패트롤] 오픈 조건 (도달한스테이지 ID 기준)={BasisType.TP_OPEN_STAGE_ID.GetInt()}");
        }

        /// <summary>
        /// 스테이지 모드 초기화
        /// </summary>
        public void ResetStageMode()
        {
            RequestStageMode = StageMode.Normal;
        }

        /// <summary>
        /// 이벤트스테이지 정보 세팅
        /// </summary>
        private void SetEventStageInfo(EventStagePacket packet)
        {
            // 새로운 이벤트스테이지 시즌 진행
            if (eventStageSeq != packet.seq)
            {
                eventStageSeq = packet.seq;

                eventStageLevel.Clear(); // 기존 Level Reset
                ResetChallengClearCount(); // 기존 진행도 Reset
            }

            eventStageRemainTime = packet.remainTime; // 종료까지 남은 시간 세팅

            Timing.KillCoroutines(nameof(YieldCheckFinishEventStage));
            if (packet.remainTime > 0L)
            {
                Timing.RunCoroutine(YieldCheckFinishEventStage(), nameof(YieldCheckFinishEventStage));
            }

            OnUpdateEventStageInfo?.Invoke();
        }

        /// <summary>
        /// 무료 티켓 세팅
        /// </summary>
        internal void UpdateFreeTicket(int? dungeonFreeTicket, int? dungeonEntryCount
            , int? worldBossFreeTicket, int? worldBossEntryCount
            , int? defenceDungeonFreeTicket, int? defenceDungeonEntryCount
            , int? stageBossTicket)
        {
            bool isDirty = false;

            if (this.dungeonFreeTicket.Replace(dungeonFreeTicket))
                isDirty = true;

            if (this.dungeonEntryCount.Replace(dungeonEntryCount))
                isDirty = true;

            if (this.worldBossFreeTicket.Replace(worldBossFreeTicket))
                isDirty = true;

            if (this.worldBossEntryCount.Replace(worldBossEntryCount))
                isDirty = true;

            if (this.defenceDungeonFreeTicket.Replace(defenceDungeonFreeTicket))
                isDirty = true;

            if (this.defenceDungeonEntryCount.Replace(defenceDungeonEntryCount))
                isDirty = true;

            if (isDirty)
                OnUpdateTicket?.Invoke();
        }

        /// <summary>
        /// 멀티미로 티켓, 클리어 횟수 변경
        /// </summary>
        internal void UpdateDayMultiMazeTicket(int? ticket, int? count)
        {
            bool isDirty = false;

            if (dayMultiMazeTicket.Replace(ticket))
                isDirty = true;

            if (dayMultiMazeCount.Replace(count))
                isDirty = true;

            if (isDirty)
                OnUpdateMultiMazeTicket?.Invoke();
        }

        /// <summary>
        /// MVP 몬스터 소환 티켓 변경
        /// </summary>
        /// <param name="ticket"></param>
        internal void UpdateSummonMvpTicket(int? ticket)
        {
            if (summonMvpTicket.Replace(ticket))
                OnUpdateSummonMvpTicket?.Invoke();
        }

        /// <summary>
        /// 제니던전 티켓, 클리어 횟수 변경
        /// </summary>
        internal void UpdateDayZenyDungeonTicket(int? ticket, int? count)
        {
            bool isDirty = false;

            if (dayZenyDungeonTicket.Replace(ticket))
                isDirty = true;

            if (dayZenyDungeonCount.Replace(count))
                isDirty = true;

            if (isDirty)
                OnUpdateZenyDungeonTicket?.Invoke();
        }

        /// <summary>
        /// 경험치던전 티켓, 클리어 횟수 변경
        /// </summary>
        internal void UpdateDayExpDungeonTicket(int? ticket, int? count)
        {
            bool isDirty = false;

            if (dayExpDungeonTicket.Replace(ticket))
                isDirty = true;

            if (dayExpDungeonCount.Replace(count))
                isDirty = true;

            if (isDirty)
                OnUpdateExpDungeonTicket?.Invoke();
        }

        /// <summary>
        /// 중앙실험실 티켓, 클리어 횟수 변경
        /// </summary>
        internal void UpdateCentralLabTicket(int? ticket, int? count)
        {
            bool isDirty = false;

            if (centralLabFreeTicket.Replace(ticket))
                isDirty = true;

            if (centralLabTryCount.Replace(count))
                isDirty = true;

            if (isDirty)
                OnUpdateCentralLabTicket?.Invoke();
        }

        /// <summary>
        /// 이벤트 멀티미로 티켓, 클리어 횟수 변경
        /// </summary>
        internal void UpdateEventMultiMazeTicket(int? ticket, int? count)
        {
            bool isDirty = false;

            if (eventMultiMazeFreeTicket.Replace(ticket))
                isDirty = true;

            if (eventMultiMazeEntryCount.Replace(count))
                isDirty = true;

            if (isDirty)
                OnUpdateEventMultiMazeTicket?.Invoke();
        }

        internal void UpdateDungeonFreeReward(int dungeonFreeReward)
        {
            this.dungeonFreeReward = dungeonFreeReward;
            OnUpdateView?.Invoke(); // view 갱신
        }

        /// <summary>
        /// 월드보스 입장티켓 충전까지 남은 시간
        /// </summary>
        internal void UpdateWorldBossRemainTime(int? time)
        {
            if (time.HasValue)
            {
                worldBossTicketRemainTime = time.Value;
            }
            RunCorutine();
        }

        internal void SetSubChannelId(int subChannelId)
        {
            // GameStartMapType 1이 아닐때 0 : 로비 채널 ID, 2 : 멀티대기방 ID
            switch (GameStartMapType)
            {
                case GameStartMapType.Lobby:
                    SetLastEnterLobbyChannel(subChannelId);
                    break;

                case GameStartMapType.MultiMazeLobby:
                    SetLastEnterMultiMazeLobbyId(subChannelId);
                    break;
            }
        }

        internal void SetMaxStageCoupon(string maxStageCoupon)
        {
            this.maxStageCoupon = maxStageCoupon;
        }

        /// <summary>
        /// 엔들리스 타워 무료입장 및 시간 변경
        /// </summary>
        internal void UpdateEndlessTowerTicket(int? ticket, long? remainTime)
        {
            bool isDirty = false;

            if (endlessTowerFreeTicket.Replace(ticket))
            {
                isDirty = true;
            }

            if (endlessTowerFreeTicket > 0)
            {
                // 무료 티켓수가 존재할 경우, 특정 시간 후 초기화 해주는 코루틴 중지
                Timing.KillCoroutines(nameof(YieldCheckFinishEndlessTower));
            }
            else if (endlessTowerFreeTicket == 0 && remainTime.HasValue && remainTime.Value > 0L)
            {
                // 무료입장 수가 없고 남은시간이 존재할 경우만 세팅
                EndlessTowerFreeTicketCoolTime = remainTime.Value;
                isDirty = true;

                Timing.KillCoroutines(nameof(YieldCheckFinishEndlessTower));
                Timing.RunCoroutine(YieldCheckFinishEndlessTower(), nameof(YieldCheckFinishEndlessTower));
            }

            if (isDirty)
                OnUpdateEndlessTowerFreeTicket?.Invoke();
        }

        /// <summary>
        /// 엔들리스 타워 클리어한 층
        /// </summary>
        internal void SetEndlessTowerClearedFloor(int floor)
        {
            int maxFloor = BasisType.ENDLESS_TOWER_MAX_FLOOR.GetInt();
            endlessTowerClearedFloor = Mathf.Clamp(Mathf.Max(endlessTowerClearedFloor, floor), 1, maxFloor);
        }

        /// <summary>
        /// 미궁숲 무료입장 변경
        /// </summary>
        internal void UpdateForestMazeTicket(int? ticket, int? count)
        {
            forestMazeFreeTicket.Replace(ticket);
        }

        /// <summary>
        /// 무료티켓 초기화
        /// </summary>
        internal void ResetFreeTicketCount()
        {
            UpdateForestMazeTicket(GetFreeEntryMaxCount(DungeonType.ForestMaze), 0);
        }

        /// <summary>
        /// 이벤트미궁:암흑 입장 상태
        /// </summary>
        internal void UpdateEventDarkMazeEntryFlag(bool isEnteredEventDarkMaze)
        {
            IsEnteredEventDarkMaze = isEnteredEventDarkMaze;
        }

        /// <summary>
        /// 이벤트미궁:암흑 입장 플래그 초기화
        /// </summary>
        internal void ResetEventDarkMazeEntryFlag()
        {
            UpdateEventDarkMazeEntryFlag(isEnteredEventDarkMaze: false);
        }

        /// <summary>
        /// 특정 스테이지 입장 가능 여부
        /// </summary>
        public bool IsStageOpend(int stageId)
        {
            if (RequestStageMode == StageMode.Challenge)
            {
                StageData data = stageDataRepo.Get(stageId);
                if (data == null)
                    return false;

                stageId = data.challenge_return_stage;
            }

            return stageId <= FinalStageId;
        }

        /// <summary>
        /// 입장 가능 여부
        /// </summary>
        public bool IsOpened(DungeonType dungeonType, bool isShowPopup)
        {
            return IsOpend(GetFirstOpenConditional(dungeonType), isShowPopup);
        }

        /// <summary>
        /// 던전 오픈 여부 (오픈만 되고 진입 불가능할 수도 있다.)
        /// </summary>
        public bool IsOpened(DungeonType dungeonType, int id, bool isShowPopup)
        {
            IOpenConditional openConditional;
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                case DungeonType.ExpDungeon:
                    openConditional = clickerDungeonDataRepo.Get(id);
                    break;

                case DungeonType.Defence:
                    openConditional = defenceDungeonDataRepo.Get(id);
                    break;

                case DungeonType.CentralLab:
                    openConditional = centralLabDataRepo.Get(id);
                    break;

                case DungeonType.WorldBoss:
                    openConditional = worldBossDataRepo.Get(id);
                    break;

                case DungeonType.EnlessTower:
                    openConditional = endlessTowerDataRepo.DungeonGroupInfo;
                    break;

                case DungeonType.ForestMaze:
                    ForestBaseData[] arrForestData = forestBaseDataRepo.Get(id);
                    openConditional = (arrForestData == null || arrForestData.Length == 0) ? null : arrForestData[0];
                    break;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }

            if (openConditional == null)
                return false;

            return IsOpend(openConditional, isShowPopup);
        }

        /// <summary>
        /// 입장 가능 여부
        /// </summary>
        public bool IsOpend(IOpenConditional openConditional, bool isShowPopup)
        {
            if (openConditional == null)
                return false;

            switch (openConditional.ConditionType)
            {
                case DungeonOpenConditionType.JobLevel:
                    int needJobLevel = openConditional.ConditionValue;
                    if (Entity.Character.JobLevel < needJobLevel)
                    {
                        if (isShowPopup)
                            UI.ShowToastPopup(GetOpenConditionalDetailText(openConditional));

                        return false;
                    }

                    return true;

                case DungeonOpenConditionType.MainQuest:
                    QuestInfo currentGuideQuest = Entity.Quest.GetMaintQuest();

                    // 모든 가이드 퀘스트 완료 상태
                    if (currentGuideQuest.IsInvalidData)
                        return true;

                    int dailyGroupId = openConditional.ConditionValue;
                    if (currentGuideQuest.Group < dailyGroupId)
                    {
                        if (isShowPopup)
                        {
                            int seq;
                            string name;
                            QuestInfo needQuest = Entity.Quest.GetMaintQuest(dailyGroupId);
                            if (needQuest.IsInvalidData)
                            {
                                seq = dailyGroupId;
                                name = string.Empty;
                            }
                            else
                            {
                                seq = needQuest.GetMainQuestGroup();
                                name = needQuest.Name;
                            }

                            string description = LocalizeKey._90083.ToText() // 메인 퀘스트 [{NUMBER}.{NAME}] 클리어 해야합니다.
                                .Replace("{NUMBER}", seq.ToString())
                                .Replace("{NAME}", name.ToString());

                            UI.ShowToastPopup(description);
                        }

                        return false;
                    }

                    return true;

                case DungeonOpenConditionType.ScenarioMaze:
                    int scenarioMazeId = openConditional.ConditionValue;
                    if (!IsCleardScenarioMazeId(scenarioMazeId))
                    {
                        if (isShowPopup)
                        {
                            string name;
                            ScenarioMazeData needScenarioMaze = scenarioMazeDataRepo.Get(scenarioMazeId);
                            if (needScenarioMaze == null)
                            {
                                name = string.Empty;
                            }
                            else
                            {
                                name = needScenarioMaze.name_id.ToText();
                            }

                            string description = LocalizeKey._54305.ToText() // 시나리오 미궁 [{NAME}]를 클리어해야 합니다.
                                .Replace(ReplaceKey.NAME, name);

                            UI.ShowToastPopup(description);
                        }

                        return false;
                    }

                    return true;

                case DungeonOpenConditionType.UpdateLater:
                    if (isShowPopup)
                    {
                        UI.ShowToastPopup(GetOpenConditionalDetailText(openConditional));
                    }

                    return false;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(openConditional.ConditionType)} = {openConditional.ConditionType}");
            }
        }

        /// <summary>
        /// 첫번째 던전 아이디
        /// </summary>
        public IOpenConditional GetFirstOpenConditional(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return clickerDungeonDataRepo.GetByIndex(DungeonType.ZenyDungeon, 0);

                case DungeonType.ExpDungeon:
                    return clickerDungeonDataRepo.GetByIndex(DungeonType.ExpDungeon, 0);

                case DungeonType.WorldBoss:
                    return worldBossDataRepo.GetList()[0];

                case DungeonType.Defence:
                    return defenceDungeonDataRepo.GetList()[0];

                case DungeonType.CentralLab:
                    return centralLabDataRepo.GetByIndex(0);

                case DungeonType.EnlessTower:
                    return endlessTowerDataRepo.DungeonGroupInfo;

                case DungeonType.ForestMaze:
                    return forestBaseDataRepo.GetFirstData();

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 던전 오픈 조건 텍스트 반환
        /// </summary>
        public string GetOpenConditionalSimpleText(IOpenConditional openConditional)
        {
            switch (openConditional.ConditionType)
            {
                case DungeonOpenConditionType.JobLevel:
                    return LocalizeKey._7028.ToText() // Job Lv.{LEVEL} 오픈
                        .Replace(ReplaceKey.LEVEL, openConditional.ConditionValue);

                case DungeonOpenConditionType.UpdateLater:
                    return LocalizeKey._3500.ToText(); // 업데이트 예정

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(openConditional.ConditionType)} = {openConditional.ConditionType}");
            }
        }

        /// <summary>
        /// 던전 오픈 조건 텍스트 반환
        /// </summary>
        public string GetOpenConditionalDetailText(IOpenConditional openConditional)
        {
            switch (openConditional.ConditionType)
            {
                case DungeonOpenConditionType.JobLevel:
                    return LocalizeKey._90087.ToText() // 직업레벨 {LEVEL} 이후에 오픈됩니다.
                        .Replace(ReplaceKey.LEVEL, openConditional.ConditionValue);

                case DungeonOpenConditionType.UpdateLater:
                    return LocalizeKey._90045.ToText(); // 업데이트 예정

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(openConditional.ConditionType)} = {openConditional.ConditionType}");
            }
        }

        /// <summary>
        /// 마지막 클리어한 던전 id
        /// </summary>
        public int GetClearedId(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return clearedZenyDungeonId;

                case DungeonType.ExpDungeon:
                    return clearedExpDungeonId;

                case DungeonType.Defence:
                    return clearedDefenceDungeonId;

                case DungeonType.CentralLab:
                    return clearedCentralLabId;

                case DungeonType.WorldBoss:
                    return 0; // 월드보스는 클리어한 던전 개념이 음슴

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 클리어한 던전 난이도
        /// </summary>
        public int GetClearedDifficulty(DungeonType dungeonType)
        {
            int clearedId = GetClearedId(dungeonType);
            return GetDifficulty(dungeonType, clearedId);
        }

        /// <summary>
        /// 해당 시나리오미로 타입 클리어 여부
        /// </summary>
        public bool IsCleared(ScenarioMazeMode mode)
        {
            ScenarioMazeData finded = scenarioMazeDataRepo.Get(mode);
            if (finded == null)
                return false;

            return IsCleardScenarioMazeId(finded.id);
        }

        /// <summary>
        /// 특정 던전 난이도 반환 (없으면 -1, 0부터 시작)
        /// </summary>
        private int GetDifficulty(DungeonType dungeonType, int id)
        {
            if (id == 0)
                return 0;

            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                case DungeonType.ExpDungeon:
                    ClickerDungeonData clickerDungeonData = clickerDungeonDataRepo.Get(id);
                    if (clickerDungeonData == null)
                        return 0;

                    return clickerDungeonData.Difficulty;

                case DungeonType.Defence:
                    DefenceDungeonData defenceDungeonData = defenceDungeonDataRepo.Get(id);
                    if (defenceDungeonData == null)
                        return 0;

                    return defenceDungeonData.difficulty;

                case DungeonType.CentralLab:
                    CentralLabData centralLabData = centralLabDataRepo.Get(id);
                    if (centralLabData == null)
                        return 0;

                    return id; // id가 곧 난이도

                case DungeonType.WorldBoss:
                    return 0; // 월드보스는 난이도 개념이 음슴

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 던전 클리어 여부
        /// </summary>
        public bool IsCleared(DungeonType dungeonType, int id)
        {
            return GetDifficulty(dungeonType, id) <= GetClearedDifficulty(dungeonType); // 현재 난이도 <= 클리어한 난이도
        }

        /// <summary>
        /// 던전 진입 가능 여부
        /// </summary>
        public bool CanEnter(DungeonType dungeonType, int id, bool isShowPopup)
        {
            if (!IsOpened(dungeonType, id, isShowPopup))
                return false;

            int nextDifficulty = GetClearedDifficulty(dungeonType) + 1;
            int difficulty = GetDifficulty(dungeonType, id);
            if (nextDifficulty < difficulty)
            {
                if (isShowPopup)
                {
                    string description = LocalizeKey._90221.ToText(); // 이전 난이도를 클리어해야 합니다.
                    UI.ShowToastPopup(description);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 던전에 입장한 횟수 반환
        /// </summary>
        public int GetEntryCount(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return dayZenyDungeonCount;

                case DungeonType.ExpDungeon:
                    return dayExpDungeonCount;

                case DungeonType.WorldBoss:
                    return worldBossEntryCount;

                case DungeonType.Defence:
                    return defenceDungeonEntryCount;

                case DungeonType.MultiMaze:
                case DungeonType.Gate:
                    return dayMultiMazeCount;

                case DungeonType.CentralLab:
                    return centralLabTryCount;

                case DungeonType.EventMultiMaze:
                    return eventMultiMazeEntryCount;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 무료입장 남은 횟수
        /// </summary>
        public int GetFreeEntryCount(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return dayZenyDungeonTicket;

                case DungeonType.ExpDungeon:
                    return dayExpDungeonTicket;

                case DungeonType.WorldBoss:
                    return worldBossFreeTicket;

                case DungeonType.Defence:
                    return defenceDungeonFreeTicket;

                case DungeonType.MultiMaze:
                case DungeonType.Gate:
                    return dayMultiMazeTicket;

                case DungeonType.CentralLab:
                    return centralLabFreeTicket;

                case DungeonType.EventMultiMaze:
                    return eventMultiMazeFreeTicket;

                case DungeonType.EnlessTower:
                    return endlessTowerFreeTicket;

                case DungeonType.ForestMaze:
                    return forestMazeFreeTicket;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 던전 무료입장 최대 횟수
        /// </summary>
        public int GetFreeEntryMaxCount(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return BasisType.ZENY_DUNGEON_FREE_JOIN_COUNT.GetInt();

                case DungeonType.ExpDungeon:
                    return BasisType.EXP_DUNGEON_FREE_JOIN_COUNT.GetInt();

                case DungeonType.WorldBoss:
                    return BasisType.WORLD_BOSS_FREE_JOIN_CNT.GetInt();

                case DungeonType.Defence:
                    return BasisType.DEF_DUNGEON_FREE_JOIN_CNT.GetInt();

                case DungeonType.MultiMaze:
                case DungeonType.Gate:
                    return BasisType.MULTI_MAZE_FREE_JOIN_COUNT.GetInt();

                case DungeonType.CentralLab:
                    return BasisType.CENTRAL_LAB_FREE_JOIN_CNT.GetInt();

                case DungeonType.EventMultiMaze:
                    return BasisType.EVENT_MULTI_MAZE_FREE_JOIN_COUNT.GetInt();

                case DungeonType.EnlessTower:
                    return BasisType.ENDLESS_TOWER_FREE_JOIN_COUNT.GetInt();

                case DungeonType.ForestMaze:
                    return 1; // 미궁숲 최대무료입장은 1

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 유료 입장 비용(냥다래)
        /// </summary>
        private int GetBuyTicketCatCoin(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                case DungeonType.ExpDungeon:
                case DungeonType.Defence:
                case DungeonType.CentralLab:
                    return BasisType.BUY_DUNGEON_TICKET_CAT_COIN.GetInt();

                case DungeonType.WorldBoss:
                    return BasisType.WORLD_BOSS_TICKET_CAT_COIN.GetInt();

                case DungeonType.MultiMaze:
                case DungeonType.Gate:
                    return BasisType.MULTI_MAZE_CAT_COIN_JOIN.GetInt();

                case DungeonType.EventMultiMaze:
                    return BasisType.EVENT_MULTI_MAZE_CAT_COIN_JOIN.GetInt();

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 입장 비용 가중치(냥다래 열매)
        /// </summary>
        private int GetBuyTicketIncCatCoin(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                case DungeonType.ExpDungeon:
                case DungeonType.Defence:
                case DungeonType.CentralLab:
                    return BasisType.BUY_DUNGEON_TICKET_INC_CAT_COIN.GetInt();

                case DungeonType.WorldBoss:
                    return BasisType.WORLD_BOSS_INC_CAT_COIN.GetInt();

                case DungeonType.MultiMaze:
                case DungeonType.Gate:
                    return BasisType.MULTI_MAZE_CAT_COIN_INC.GetInt();

                case DungeonType.EventMultiMaze:
                    return BasisType.EVENT_MULTI_MAZE_CAT_COIN_INC.GetInt();

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 던전 무료보상 수령가능
        /// </summary>
        public bool PossibleFreeReward(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return !dungeonFreeReward.ToEnum<DungeonFreeRewardType>().HasFlag(DungeonFreeRewardType.FreeRewardZeny);

                case DungeonType.ExpDungeon:
                    return !dungeonFreeReward.ToEnum<DungeonFreeRewardType>().HasFlag(DungeonFreeRewardType.FreeRewardExp);

                case DungeonType.Defence:
                    return !dungeonFreeReward.ToEnum<DungeonFreeRewardType>().HasFlag(DungeonFreeRewardType.FreeRewardDefence);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 던전 무료 입장 가능 여부
        /// </summary>
        public bool IsFreeEntry(DungeonType dungeonType)
        {
            return GetFreeEntryCount(dungeonType) > 0;
        }

        /// <summary>
        /// 이벤트 스테이지 오픈 여부 
        /// </summary>
        public bool IsOpendEventStage()
        {
            return eventStageRemainTime.ToRemainTime() > 0f;
        }

        /// <summary>
        /// 엔들리스 타워 무료오픈 쿨타임 여부
        /// </summary>
        public bool HasCooltimeRemainEndlessTower()
        {
            return EndlessTowerFreeTicketCoolTime.ToRemainTime() > 0f;
        }

        /// <summary>
        /// 던전 입장 가능 체크 
        /// </summary>
        public async Task<bool> IsEnterDungeon(DungeonType dungeonType)
        {
            if (IsFreeEntry(dungeonType))
                return true;

            int entryCount = GetEntryCount(dungeonType); // 실제 입장 횟수
            int maxFreeCount = GetFreeEntryMaxCount(dungeonType); // 무료 입장 횟수
            int overCount = entryCount - maxFreeCount; // 추가 입장 하려는 횟수

            int maxExtraCount = BasisType.DUNGEON_ADD_ENTER_LIMIT.GetInt((int)dungeonType); // 최대 추가 입장 횟수
            int remainExtraCount = Mathf.Clamp(maxExtraCount - overCount, 0, maxExtraCount); // 남은 추가 입장 횟수

            if (remainExtraCount == 0)
            {
                UI.ShowToastPopup(LocalizeKey._90224.ToText()); // 오늘 남은 횟수를 모두 사용하였습니다.
                return false;
            }

            int needCoin = GetBuyTicketCatCoin(dungeonType) + (GetBuyTicketIncCatCoin(dungeonType) * overCount); // 필요 냥다래
            UIDungeonExtraEntry.SelectResult result = await UI.Show<UIDungeonExtraEntry>().Show(remainExtraCount, maxExtraCount, needCoin);

            // 사용자가 취소함
            if (result == UIDungeonExtraEntry.SelectResult.Cancel)
                return false;

            // 필요 재화 체크
            return CoinType.CatCoin.Check(needCoin);
        }

        #region 스테이지

        /// <summary>
        /// [스테이지] 시작
        /// </summary>
        public async Task<Response> RequestStageStart(int stageId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", stageId); // 스테이지 아이디
            sfs.PutByte("2", RequestStageMode.ToByteValue()); // 스테이지 모드

            IMultiPlayerInput[] multiPlayers = null;
            var response = await Protocol.REQUEST_AUTO_STAGE_ENTER.SendAsync(sfs);

            if (response.isSuccess)
            {
                StageMode = RequestStageMode;
                SetLastEnterStageId(stageId); // 스테이지 시작점 세팅

                if (response.ContainsKey("1"))
                    multiPlayers = response.GetPacketArray<BattleCharacterPacket>("1");

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// [스테이지] 일반 몬스터 아이템 드랍 (스테이지 전용)
        /// </summary>
        public async Task RequestStageNormalMonsterDrop(int monsterId, int clientMonsterId, int stageId, DamagePacket damagePacket)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", monsterId);
            sfs.PutInt("2", clientMonsterId);

            if (serverMonsterRandomKey > 0)
            {
                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("일반몬스터 RandomKey 처리: key = " + serverMonsterRandomKey + ", hex = " + MathUtils.CidToHexCode(serverMonsterRandomKey));
                }

                sfs.PutUtfString("6", MathUtils.CidToHexCode(serverMonsterRandomKey));
                serverMonsterRandomKey = 0;
            }

            sfs.PutInt("7", stageId);
#if UNITY_EDITOR
            sfs.PutByteArray("99", damagePacket.ToByteArray()); // 대미지 패킷
#endif

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            Response response = await Protocol.REQUEST_AUTO_STAGE_MON_DROP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.STAGE_MON_NOT_EXISTS)
                {
#if UNITY_EDITOR
                    Debug.LogError("일치하지 않은 몬스터 정보");
#endif
                    return;
                }

                response.ShowResultCode();
                return;
            }

#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
        }

        /// <summary>
        /// 던전 무료보상
        /// </summary>
        public async Task RequestFreeReward(int dungeonType)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", dungeonType);

            Response response = await Protocol.REQUEST_DUNGEON_DAILY_FREE_REWARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var cud = response.GetPacket<CharUpdateData>("cud");
            Notify(cud);
            UI.RewardInfo(cud.rewards);

            UpdateDungeonFreeReward(response.GetInt("1"));
        }

        void OnStageNormalMonsterDrop(Response response)
        {
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.STAGE_MON_NOT_EXISTS)
                {
#if UNITY_EDITOR
                    Debug.LogError("일치하지 않은 몬스터 정보");
#endif
                    return;
                }

                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            int receivedClientMonsterId = response.ContainsKey("3") ? response.GetInt("3") : 0;

            // 알람 타입(New 표시)
            if (response.ContainsKey("2"))
                NotifyAlarm(response.GetInt("2"));

            // eb. 이벤트 버프 정보
            if (response.ContainsKey("eb"))
            {
                EventBuffPacket eventBuffPacket = response.GetPacket<EventBuffPacket>("eb");
                Notify(eventBuffPacket);
            }

            // 1. MVP 정보
            if (response.ContainsKey("1"))
            {
                MvpMonsterPacket mvpPacket = response.GetPacket<MvpMonsterPacket>("1");
                serverMvpMonsterRandomKey = mvpPacket.randomKey;

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 MVP몬스터 RandomKey 값: " + serverMvpMonsterRandomKey);
                }

                OnAppearMvpMonster?.Invoke(mvpPacket.mvpTableId, mvpPacket.remainTime);
            }

            if (response.ContainsKey("4"))
            {
                NotifyDuelPoint(response.GetInt("4"));
                // 획득 연출
                UnitEntity unitEntity = (receivedClientMonsterId > 0 && UnitEntity.entityDic.ContainsKey(receivedClientMonsterId)) ? UnitEntity.entityDic[receivedClientMonsterId] : null;
                OnDropCube?.Invoke(unitEntity);
            }

            // 몬스터 도감 획득
            if (response.ContainsKey("5"))
            {
                int bookIndex = response.GetShort("5");
                NotyfyBookRecord(BookTabType.Monster, bookIndex);
            }

            // 서버에서 받은 몬스터 랜덤 키
            if (response.ContainsKey("6"))
            {
                serverMonsterRandomKey = response.GetInt("6");

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 일반몬스터 RandomKey 값: " + serverMonsterRandomKey);
                }
            }

            ProcessDroppedItem(charUpdateData, receivedClientMonsterId);
        }

        /// <summary>
        /// [스테이지] 보스 소환
        /// </summary>
        public async Task RequestSummonStageBoss()
        {
            Response response = await Protocol.REQUEST_SUMMON_STAGE_BOSS.SendAsync();
            if (response.isSuccess)
            {
                int remainTime = response.GetInt("1");
                serverBossMonsterRandomKey = response.GetInt("2");

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 보스몬스터 RandomKey 값: " + serverBossMonsterRandomKey);
                }

                OnAppearBossMonster?.Invoke(remainTime);
                return;
            }

            if (response.resultCode == ResultCode.MAX_CLEAR_STAGE)
            {
                if (response.ContainsKey("C"))
                {
                    maxStageCoupon = response.GetUtfString("C");
                    ShowStageCoupon();
                }
                else
                {
                    UI.ShowToastPopup(LocalizeKey._90045.ToText());
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 쿠폰 번호 보여주기
        /// </summary>
        public bool ShowStageCoupon()
        {
            if (string.IsNullOrEmpty(maxStageCoupon))
                return false;

            UI.ConfirmPopup(LocalizeKey._90197.ToText().Replace(ReplaceKey.VALUE, maxStageCoupon));
            return true;
        }

        /// <summary>
        /// [스테이지] MVP 몬스터 아이템 드랍 (스테이지 전용)
        /// </summary>
        public async Task RequestStageMvpMonsterDrop(bool isClear, int clientMonsterId, DamagePacket damagePacket)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", clientMonsterId);
            sfs.PutByteArray("2", damagePacket.ToByteArray()); // 대미지 패킷
            sfs.PutBool("3", isClear);
            sfs.PutUtfString("4", MathUtils.CidToHexCode(serverMvpMonsterRandomKey));

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("MVP몬스터 RandomKey 처리: key = " + serverMvpMonsterRandomKey + ", hex = " + MathUtils.CidToHexCode(serverMvpMonsterRandomKey));
            }

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            Response response = await Protocol.REQUEST_AUTO_STAGE_MVP_DROP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            int receivedClientMonsterId = response.ContainsKey("1") ? response.GetInt("1") : 0;
            RewardPacket[] wasted = response.ContainsKey("2") ? response.GetPacketArray<RewardPacket>("2") : null; // 2. 꽉차서 받지 못한 아이템
#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
            UnitEntity unitEntity = (clientMonsterId > 0 && UnitEntity.entityDic.ContainsKey(clientMonsterId)) ? UnitEntity.entityDic[clientMonsterId] : null;

            if (charUpdateData != null)
                Notify(charUpdateData);

            if (isClear)
            {
                bool newDuelAlphabet = response.ContainsKey("3");
                int newAlphabet = 0;
                int duelRewardedCount = 0;
                int duelAlphabetIndex = -1;

                if (newDuelAlphabet)
                {
                    newAlphabet = response.GetInt("3");
                    duelRewardedCount = response.GetShort("4");
                    int chapter = stageDataRepo.Get(lastEnterStageId).chapter;
                    Entity.Duel.UpdateDuelState(chapter, newAlphabet);

                    duelAlphabetIndex = 0;

                    while (newAlphabet > 1)
                    {
                        newAlphabet = newAlphabet >> 1;
                        ++duelAlphabetIndex;
                    }
                }

                // 몬스터 도감 획득
                if (response.ContainsKey("5"))
                {
                    int bookIndex = response.GetShort("5");
                    NotyfyBookRecord(BookTabType.Monster, bookIndex);
                }

                // 듀얼 아레나 무료 포인트 획득 여부
                bool isGainedArenaPoint = response.GetBool("6");

                // 데이터 세팅하기 전 이벤트를 호출할 것!!
                OnMvpMonsterItemDrop?.Invoke(unitEntity, UI.ConvertRewardData(charUpdateData?.rewards), UI.ConvertRewardData(wasted), duelAlphabetIndex, isGainedArenaPoint);

                // 몬스터로 인한
                if (unitEntity)
                {
                    // 퀘스트 체크
                    UnitSizeType unitSizeType = unitEntity.battleUnitInfo.UnitSizeType;
                    ElementType elementType = unitEntity.battleUnitInfo.UnitElementType;
                    int monsterGroup = MonsterDataManager.Instance.Get(unitEntity.battleUnitInfo.Id).monster_group;

                    Quest.QuestProgress(QuestType.MONSTER_KILL); // 몬스터 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_TARGET, unitEntity.battleUnitInfo.Id); // 특정 몬스터 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_SIZE, (int)unitSizeType); // 몬스터 크기별 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_ELEMENT, (int)elementType); // 몬스터 속성별 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_GROUP_KILL_COUNT, conditionValue: monsterGroup); // 특정 몬스터 그룹 처치 수
                    Quest.QuestProgress(QuestType.MVP_CLEAR_COUNT); // MVP 처치 횟수

                    // 획득 보상의 경우에는
                    if (charUpdateData != null)
                    {
                        UI.RewardToast(charUpdateData.rewards); // 획득 보상 보여줌 (토스트팝업)
                    }
                }
            }
        }

        /// <summary>
        /// [스테이지] 보스 클리어 (스테이지 전용)
        /// </summary>
        public async Task RequestStageBossClear(bool isClear, int stageId, int clientMonsterId, DamagePacket damagePacket)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", clientMonsterId);
            sfs.PutByteArray("2", damagePacket.ToByteArray()); // 대미지 패킷
            sfs.PutBool("3", isClear);
            sfs.PutUtfString("4", MathUtils.CidToHexCode(serverBossMonsterRandomKey));

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("보스몬스터 RandomKey 처리: key = " + serverBossMonsterRandomKey + ", hex = " + MathUtils.CidToHexCode(serverBossMonsterRandomKey));
            }

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            Response response = await Protocol.REQUEST_STAGE_BOSS_CLEAR.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (isClear && StageMode == StageMode.Normal)
            {
                SetFinalStageId(stageId + 1); // 다음 스테이지 진입 가능
                Entity.Agent.AddNewTradeCountInfo(stageId + 1);
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            int receivedClientMonsterId = response.ContainsKey("1") ? response.GetInt("1") : 0;
            int bossCoolTime = response.GetInt("2");

#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
            UnitEntity unitEntity = (clientMonsterId > 0 && UnitEntity.entityDic.ContainsKey(clientMonsterId)) ? UnitEntity.entityDic[clientMonsterId] : null;

            if (charUpdateData != null)
                Notify(charUpdateData);

            // 데이터 세팅하기 전 이벤트를 호출할 것!!
            if (isClear)
            {
                OnBossMonsterItemDrop?.Invoke(unitEntity, UI.ConvertRewardData(charUpdateData?.rewards));

                // 몬스터로 인한
                if (unitEntity)
                {
                    // 퀘스트 체크
                    UnitSizeType unitSizeType = unitEntity.battleUnitInfo.UnitSizeType;
                    ElementType elementType = unitEntity.battleUnitInfo.UnitElementType;
                    int monsterGroup = MonsterDataManager.Instance.Get(unitEntity.battleUnitInfo.Id).monster_group;

                    Quest.QuestProgress(QuestType.MONSTER_KILL); // 몬스터 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_TARGET, unitEntity.battleUnitInfo.Id); // 특정 몬스터 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_SIZE, (int)unitSizeType); // 몬스터 크기별 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_ELEMENT, (int)elementType); // 몬스터 속성별 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_GROUP_KILL_COUNT, conditionValue: monsterGroup); // 특정 몬스터 그룹 처치 수

                    // 획득 보상의 경우에는
                    if (charUpdateData != null)
                    {
                        UI.RewardToast(charUpdateData.rewards); // 획득 보상 보여줌 (토스트팝업)
                    }

                    int level = GetEventStageLevel(stageId);

                    if (StageMode == StageMode.Event || StageMode == StageMode.Challenge)
                    {
                        Quest.QuestProgress(QuestType.EVENT_STAGE_LEVEL_CLEAR_COUNT, level); // 이벤트 모드 레벨 별 클리어 횟수

                        StageData stageData = stageDataRepo.Get(stageId);
                        if (stageData != null)
                            Quest.QuestProgress(QuestType.EVENT_STAGE_CHAPTER_CLEAR_COUNT, stageData.chapter); // 이벤트 모드 챕터 별 클리어 횟수
                    }

                    switch (StageMode)
                    {
                        case StageMode.Event:
                            SetEventStageLevel(stageId, level + 1);
                            break;

                        case StageMode.Challenge:
                            SetEventStageLevel(stageId, level + 1);
                            int clearCount = GetChallengeClearCount(stageId);
                            SetChallengeClearCount(stageId, clearCount + 1);
                            break;
                    }
                }

                // 몬스터 도감 획득
                if (response.ContainsKey("5"))
                {
                    int bookIndex = response.GetShort("5");
                    NotyfyBookRecord(BookTabType.Monster, bookIndex);
                }
            }
        }

        /// <summary>
        /// [스테이지] 이벤트스테이지 정보 호출
        /// </summary>
        public async Task RequestEventStageInfo()
        {
            Response response = await Protocol.REQUEST_EVENT_STAGE_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            EventStagePacket eventStagePacket;
            if (response.ContainsKey("1"))
            {
                eventStagePacket = response.GetPacket<EventStagePacket>("1");
            }
            else
            {
                eventStagePacket = EventStagePacket.EMPTY;
            }

            SetEventStageInfo(eventStagePacket);
        }

        #endregion

        #region 디펜스 던전

        /// <summary>
        /// [디펜스] 시작
        /// </summary>
        public async Task<bool> RequestDefenceDungeonStart(int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 디펜스 던전 아이디

            var response = await Protocol.REQUEST_DEF_DUNGEON_START.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// [디펜스] 종료
        /// </summary>
        public async Task<bool> RequestDefenceDungeonEnd(int id, bool isClear)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 디펜스 던전 아이디            
            sfs.PutBool("2", isClear); // 디펜스 던전 클리어 여부

            var response = await Protocol.REQUEST_DEF_DUNGEON_END.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
                ProcessDroppedItem(charUpdateData);

                if (isClear)
                    SetClearedDungeonId(DungeonType.Defence, id);
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// [디펜스] 몬스터 드랍
        /// </summary>
        public async Task RequestDefenceDungeonMonsterDrop(int defDungeonId, int defDungeonWave, int monsterId, int clientMonsterId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", defDungeonId); // 디펜스 던전 아이디
            sfs.PutInt("2", defDungeonWave); // 웨이브 넘버
            sfs.PutInt("3", monsterId); // 몬스터 아이디
            sfs.PutInt("4", clientMonsterId); // 클라이언트 고유 몬스터 아이디

            if (serverDefenceMonsterRandomKey > 0)
            {
                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("디팬스 몬스터 RandomKey 처리: key = " + serverDefenceMonsterRandomKey + ", hex = " + MathUtils.CidToHexCode(serverDefenceMonsterRandomKey));
                }

                sfs.PutUtfString("6", MathUtils.CidToHexCode(serverDefenceMonsterRandomKey));
                serverDefenceMonsterRandomKey = 0;
            }

            await Protocol.REQUEST_DEF_DUNGEON_ITEM_DROP.SendAsync(sfs);
        }

        void OnDefenceDungeonMonsterDrop(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            int receivedClientMonsterId = response.ContainsKey("2") ? response.GetInt("2") : 0;
            // 서버에서 받은 몬스터 랜덤 키
            if (response.ContainsKey("3"))
            {
                serverDefenceMonsterRandomKey = response.GetInt("3");

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 디팬스몬스터 RandomKey 값: " + serverDefenceMonsterRandomKey);
                }
            }

            ProcessDroppedItem(charUpdateData, receivedClientMonsterId);
        }

        void OnReceiveMultiMazeBaseReward(Response response)
        {
            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");

                int droppedZeny = 0;
                int droppedLevelExp = 0;
                int droppedJobExp = 0;

                if (charUpdateData != null)
                {
                    if (charUpdateData.zeny.HasValue)
                        droppedZeny = Mathf.Max(0, (int)(charUpdateData.zeny.Value - Entity.Goods.Zeny));

                    if (charUpdateData.levelExp.HasValue)
                        droppedLevelExp = Mathf.Max(0, charUpdateData.levelExp.Value - Entity.Character.LevelExp);

                    if (charUpdateData.jobExp.HasValue)
                        droppedJobExp = Mathf.Max(0, (int)(charUpdateData.jobExp.Value - Entity.Character.JobLevelExp));

                    if (charUpdateData.rewards != null)
                    {
                        const byte ZENY_TYPE = (byte)RewardType.Zeny;
                        const byte LEVEL_EXP_TYPE = (byte)RewardType.LevelExp;
                        const byte JOB_EXP_TYPE = (byte)RewardType.JobExp;

                        foreach (var item in charUpdateData.rewards)
                        {
                            /** 중복을 막기 위함
                             * 스테이지 데이터 참조한 값은 여기로 오지않고 droppedZeny, droppedLevelExp, droppedJobExp 로 계산하여 사용
                             * 그 외의 몬스터 보상 값이 여기로 오는데, 거기에 하필 zeny, levelExp, jobExp 가 포함될 경우 중복처럼 보이게 된다.
                             * zeny, levelExp, jobExp 는 순수하게 계산한 값으로만 사용한다
                             */
                            if (item.rewardType == ZENY_TYPE || item.rewardType == LEVEL_EXP_TYPE || item.rewardType == JOB_EXP_TYPE)
                                continue;

                            rewardBuffer.Add(new RewardData(item.rewardType, item.rewardValue, item.rewardCount, item.rewardOption));
                        }
                    }
                }

                Notify(charUpdateData);

                // 데이터 세팅하기 전 이벤트를 호출할 것!!
                OnMultiJoinRewardEvent?.Invoke(droppedZeny, droppedLevelExp, droppedJobExp, rewardBuffer.GetBuffer(isAutoRelease: true));
            }
        }

        void OnReceiveEventMultiMazeReward(Response response)
        {
            int snowball = response.GetInt("1");
            int rudolph = response.GetInt("2");

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");

                int droppedZeny = 0;
                int droppedLevelExp = 0;
                int droppedJobExp = 0;

                if (charUpdateData != null)
                {
                    if (charUpdateData.zeny.HasValue)
                        droppedZeny = Mathf.Max(0, (int)(charUpdateData.zeny.Value - Entity.Goods.Zeny));

                    if (charUpdateData.levelExp.HasValue)
                        droppedLevelExp = Mathf.Max(0, charUpdateData.levelExp.Value - Entity.Character.LevelExp);

                    if (charUpdateData.jobExp.HasValue)
                        droppedJobExp = Mathf.Max(0, (int)(charUpdateData.jobExp.Value - Entity.Character.JobLevelExp));

                    if (charUpdateData.rewards != null)
                    {
                        const byte ZENY_TYPE = (byte)RewardType.Zeny;
                        const byte LEVEL_EXP_TYPE = (byte)RewardType.LevelExp;
                        const byte JOB_EXP_TYPE = (byte)RewardType.JobExp;

                        foreach (var item in charUpdateData.rewards)
                        {
                            /** 중복을 막기 위함
                             * 스테이지 데이터 참조한 값은 여기로 오지않고 droppedZeny, droppedLevelExp, droppedJobExp 로 계산하여 사용
                             * 그 외의 몬스터 보상 값이 여기로 오는데, 거기에 하필 zeny, levelExp, jobExp 가 포함될 경우 중복처럼 보이게 된다.
                             * zeny, levelExp, jobExp 는 순수하게 계산한 값으로만 사용한다
                             */
                            if (item.rewardType == ZENY_TYPE || item.rewardType == LEVEL_EXP_TYPE || item.rewardType == JOB_EXP_TYPE)
                                continue;

                            rewardBuffer.Add(new RewardData(item.rewardType, item.rewardValue, item.rewardCount, item.rewardOption));
                        }
                    }
                }

                Notify(charUpdateData);

                // 데이터 세팅하기 전 이벤트를 호출할 것!!
                OnMultiJoinRewardEvent?.Invoke(droppedZeny, droppedLevelExp, droppedJobExp, rewardBuffer.GetBuffer(isAutoRelease: true));
            }
        }

        void OnReceiveDarkMazeReward(Response response)
        {
            int cid = response.GetInt("1"); // 보스 붙잡은 유저
            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;

            if (cid > 0)
            {
                UpdateEventDarkMazeEntryFlag(true);
                Quest.QuestProgress(QuestType.DUNGEON_TYPE_CLEAR_COUNT, DungeonType.EventMultiMaze.ToIntValue()); // 특정던전 입장 횟수
            }

            int droppedZeny = 0;
            int droppedLevelExp = 0;
            int droppedJobExp = 0;

            if (charUpdateData != null)
            {
                if (charUpdateData.zeny.HasValue)
                    droppedZeny = Mathf.Max(0, (int)(charUpdateData.zeny.Value - Entity.Goods.Zeny));

                if (charUpdateData.levelExp.HasValue)
                    droppedLevelExp = Mathf.Max(0, charUpdateData.levelExp.Value - Entity.Character.LevelExp);

                if (charUpdateData.jobExp.HasValue)
                    droppedJobExp = Mathf.Max(0, (int)(charUpdateData.jobExp.Value - Entity.Character.JobLevelExp));

                if (charUpdateData.rewards != null)
                {
                    const byte ZENY_TYPE = (byte)RewardType.Zeny;
                    const byte LEVEL_EXP_TYPE = (byte)RewardType.LevelExp;
                    const byte JOB_EXP_TYPE = (byte)RewardType.JobExp;

                    foreach (var item in charUpdateData.rewards)
                    {
                        /** 중복을 막기 위함
                         * 스테이지 데이터 참조한 값은 여기로 오지않고 droppedZeny, droppedLevelExp, droppedJobExp 로 계산하여 사용
                         * 그 외의 몬스터 보상 값이 여기로 오는데, 거기에 하필 zeny, levelExp, jobExp 가 포함될 경우 중복처럼 보이게 된다.
                         * zeny, levelExp, jobExp 는 순수하게 계산한 값으로만 사용한다
                         */
                        if (item.rewardType == ZENY_TYPE || item.rewardType == LEVEL_EXP_TYPE || item.rewardType == JOB_EXP_TYPE)
                            continue;

                        rewardBuffer.Add(new RewardData(item.rewardType, item.rewardValue, item.rewardCount, item.rewardOption));
                    }
                }
            }

            Notify(charUpdateData);

            // 데이터 세팅하기 전 이벤트를 호출할 것!!
            OnEventDarkMazeRewardEvent?.Invoke(cid, droppedZeny, droppedLevelExp, droppedJobExp, rewardBuffer.GetBuffer(isAutoRelease: true));
        }

        #endregion

        #region 게이트

        void OnRequestGateMakeRoom(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }
        }

        void OnRequestGateJoinRoom(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }
        }

        void OnRecieveGateRoomExit(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }
        }

        #endregion

        #region 중앙실험실

        public async Task<Response> RequsetCentralLabStart(int labId, Job cloneJob)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", labId);
            sfs.PutByte("2", (byte)cloneJob);
            Response response = await Protocol.REQUEST_CLAB_START.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                // 퀘스트 처리
                Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.CentralLab.ToIntValue()); // 특정던전 입장 횟수
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        public async Task RequestCentralLabMonsterKill(int labId, int centralLabMonId, DamagePacket damagePacket)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", centralLabMonId);
            sfs.PutByteArray("99", damagePacket.ToByteArrayForCentralLab());

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif

            Response response = await Protocol.REQUEST_CLAB_MONKILL.SendAsync(sfs);
            if (response.isSuccess)
            {
                RewardData[] rewards = null;
                CentralLabSkillPacket[] selectSkills = null;

                // Clear
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");

                    if (charUpdateData != null)
                        Notify(charUpdateData);

                    rewards = UI.ConvertRewardData(charUpdateData.rewards);
                    SetClearedDungeonId(DungeonType.CentralLab, labId);
                }

                int itemSkillPoint = response.GetInt("1");
                int itemSkillId = response.GetInt("2");

                if (response.ContainsKey("3"))
                    selectSkills = response.GetPacketArray<CentralLabSkillPacket>("3");

#if UNITY_EDITOR
                if (response.ContainsKey("99"))
                {
                    debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                    DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
                }
#endif

                OnCentralLabMonsterKill?.Invoke(rewards, itemSkillPoint, itemSkillId, selectSkills);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public async Task RequestCentralLabExit()
        {
            Response response = await Protocol.REQUEST_CLAB_EXIT.SendAsync();
            if (response.isSuccess)
            {
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;

                if (charUpdateData != null)
                    Notify(charUpdateData);

                OnCentralLabExit?.Invoke(UI.ConvertRewardData(charUpdateData?.rewards));
            }
            else
            {
                response.ShowResultCode();
            }
        }

        #endregion

        #region 엔들리스 타워

        /// <summary>
        /// 엔들리스 타워 시작
        /// </summary>
        public async Task<Response> RequestEndlessTowerStart(int skipItemCount)
        {
            const int NON_FREE_ENTER = 0; // 유료 입장
            const int FREE_ENTER = 1; // 무료 입장

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", skipItemCount);
            sfs.PutInt("2", GetFreeEntryCount(DungeonType.EnlessTower) > 0 ? FREE_ENTER : NON_FREE_ENTER);
            Response response = await Protocol.ENDLESS_DUNGEON_START.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                Entity.ResetSkillCooldown(); // 쿨타임 초기화

                // 퀘스트 체크
                Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.EnlessTower.ToIntValue()); // 특정던전 입장 횟수
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// 앤들리스 타워 종료 시 호출
        /// </summary>
        void OnReceiveEndlessTowerEnd(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;

            if (charUpdateData != null)
                Notify(charUpdateData);

            int floor = response.GetInt("1");
            SetEndlessTowerClearedFloor(floor); // 클리어한 층

            // 마지막층 클리어
            if (floor == BasisType.ENDLESS_TOWER_MAX_FLOOR.GetInt())
            {
                Quest.QuestProgress(QuestType.ENDLESS_TOWER_FLOOR_CLEAR_COUNT, floor); // 엔들리스 타워 특정 특 클리어 횟수
            }

            OnEndlessTowerExit?.Invoke(floor, UI.ConvertRewardData(charUpdateData?.rewards));
        }

        #endregion

        #region 미궁숲

        /// <summary>
        /// [미궁숲] 입장
        /// </summary>
        public async Task<Response> RequestForestMazeRoomJoin(int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 탐험할 맵 아이디
            Response response = await Protocol.REQUEST_FOREST_ROOM_JOIN.SendAsync(sfs);

            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
                Notify(charUpdateData);

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }

            return response;
        }

        /// <summary>
        /// 미궁숲 나갔을 때에 대한 처리
        /// </summary>
        void OnForestMazeUserExit(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }
        }

        #endregion

        /// <summary>
        /// 최종 클리어한 스테이지 세팅
        /// </summary>
        private void SetFinalStageId(int fanalStageId)
        {
            this.finalStageId = Mathf.Max(fanalStageId, this.finalStageId); // 기존의 스테이지와 비교했을 때, 큰 값을 사용
            OnUpdateClearedStage?.Invoke();
        }

        /// <summary>
        /// 마지막에 입장한 스테이지 ID
        /// </summary>
        private void SetLastEnterStageId(int lastEnterStageId)
        {
            // 존재하지 않는 스테이지 아이디의 경우(마지막 스테이지) 이전 스테이지로 진입
            if (lastEnterStageId > 1 && !stageDataRepo.IsExists(lastEnterStageId))
                --lastEnterStageId;

            this.lastEnterStageId = Mathf.Max(1, lastEnterStageId);
        }

        public void SetClearedDungeonId(DungeonType dungeonType, int id)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    clearedZenyDungeonId = Mathf.Max(clearedZenyDungeonId, id);
                    break;

                case DungeonType.ExpDungeon:
                    clearedExpDungeonId = Mathf.Max(clearedExpDungeonId, id);
                    break;

                case DungeonType.Defence:
                    clearedDefenceDungeonId = Mathf.Max(clearedDefenceDungeonId, id);
                    break;

                case DungeonType.CentralLab:
                    clearedCentralLabId = Mathf.Max(clearedCentralLabId, id);
                    break;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }

            Quest.QuestProgress(QuestType.DUNGEON_TYPE_CLEAR_COUNT, dungeonType.ToIntValue()); // 특정 타입 던전 클리어 횟수
        }

        /// <summary>
        /// 드랍 보상 처리
        /// </summary>
        private void ProcessDroppedItem(CharUpdateData charUpdateData, int clientMonsterId = 0)
        {
            UnitEntity unitEntity = (clientMonsterId > 0 && UnitEntity.entityDic.ContainsKey(clientMonsterId)) ? UnitEntity.entityDic[clientMonsterId] : null;

            int droppedZeny = 0;
            int droppedLevelExp = 0;
            int droppedJobExp = 0;

            if (charUpdateData != null)
            {
                if (charUpdateData.zeny.HasValue)
                    droppedZeny = Mathf.Max(0, (int)(charUpdateData.zeny.Value - Entity.Goods.Zeny));

                if (charUpdateData.levelExp.HasValue)
                    droppedLevelExp = Mathf.Max(0, charUpdateData.levelExp.Value - Entity.Character.LevelExp);

                if (charUpdateData.jobExp.HasValue)
                    droppedJobExp = Mathf.Max(0, (int)(charUpdateData.jobExp.Value - Entity.Character.JobLevelExp));

                if (charUpdateData.rewards != null)
                {
                    const byte ZENY_TYPE = (byte)RewardType.Zeny;
                    const byte LEVEL_EXP_TYPE = (byte)RewardType.LevelExp;
                    const byte JOB_EXP_TYPE = (byte)RewardType.JobExp;

                    foreach (var item in charUpdateData.rewards)
                    {
                        /** 중복을 막기 위함
                         * 스테이지 데이터 참조한 값은 여기로 오지않고 droppedZeny, droppedLevelExp, droppedJobExp 로 계산하여 사용
                         * 그 외의 몬스터 보상 값이 여기로 오는데, 거기에 하필 zeny, levelExp, jobExp 가 포함될 경우 중복처럼 보이게 된다.
                         * zeny, levelExp, jobExp 는 순수하게 계산한 값으로만 사용한다
                         */
                        if (item.rewardType == ZENY_TYPE || item.rewardType == LEVEL_EXP_TYPE || item.rewardType == JOB_EXP_TYPE)
                            continue;

                        rewardBuffer.Add(new RewardData(item.rewardType, item.rewardValue, item.rewardCount, item.rewardOption));
                    }
                }
            }

            if (charUpdateData != null)
            {
                Notify(charUpdateData);
            }

            // 데이터 세팅하기 전 이벤트를 호출할 것!!
            OnMonsterItemDrop?.Invoke(unitEntity, droppedZeny, droppedLevelExp, droppedJobExp, rewardBuffer.GetBuffer(isAutoRelease: true));

            // 몬스터로 인한
            if (unitEntity)
            {
                // 퀘스트 체크
                UnitSizeType unitSizeType = unitEntity.battleUnitInfo.UnitSizeType;
                ElementType elementType = unitEntity.battleUnitInfo.UnitElementType;
                int monsterGroup = MonsterDataManager.Instance.Get(unitEntity.battleUnitInfo.Id).monster_group;

                Quest.QuestProgress(QuestType.MONSTER_KILL); // 몬스터 사냥 수
                Quest.QuestProgress(QuestType.MONSTER_KILL_TARGET, unitEntity.battleUnitInfo.Id); // 특정 몬스터 사냥 수
                Quest.QuestProgress(QuestType.MONSTER_KILL_SIZE, (int)unitSizeType); // 몬스터 크기별 사냥 수
                Quest.QuestProgress(QuestType.MONSTER_KILL_ELEMENT, (int)elementType); // 몬스터 속성별 사냥 수
                Quest.QuestProgress(QuestType.MONSTER_GROUP_KILL_COUNT, conditionValue: monsterGroup); // 특정 몬스터 그룹 처치 수

                // 획득 보상의 경우에는
                if (charUpdateData != null)
                {
                    UI.RewardToast(charUpdateData.rewards); // 획득 보상 보여줌 (토스트팝업)
                }
            }
        }

        /// <summary>
        /// 월드보스 ID 세팅
        /// </summary>
        /// <param name="id"></param>
        public void SetSelectWorldBoss(int id)
        {
            selectWorldBossId = id;
        }

        /// <summary>
        /// 월드보스 ID, MaxHP 세팅
        /// </summary>
        /// <param name="id"></param>
        public void SetSelectWorldBoss(int id, int maxHp)
        {
            selectWorldBossId = id;
            selectWorldBossMaxHp = maxHp;
        }

        /// <summary>
        /// [월드보스] 월드보스 정보 목록
        /// </summary>
        public async Task<(bool isSuccess, WorldBossInfoPacket[] worldBossInfos)> RequestWorldBossList(bool isAllList)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", isAllList);
            var response = await Protocol.REQUEST_WORLD_BOSS_LIST.SendAsync(sfs);

            WorldBossInfoPacket[] worldBossInfoPackets = null;
            if (response.isSuccess)
            {
                worldBossInfoPackets = response.GetPacketArray<WorldBossInfoPacket>("1");
            }
            else
            {
                response.ShowResultCode();
            }

            return (response.isSuccess, worldBossInfoPackets);
        }

        public void SetWorldBossRemainTime(int id, long time)
        {
            if (worldBossRemainTime.ContainsKey(id))
            {
                worldBossRemainTime[id] = time;
            }
            else
            {
                worldBossRemainTime.Add(id, time);
            }
        }

        /// <summary>
        /// [월드보스] 던전 시작
        /// </summary>
        public async Task<Response> RequestWorldBossStart(int worldBossId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", worldBossId); // 던전 아이디          

            var response = await Protocol.REQUEST_WORLD_BOSS_ROOM_JOIN.SendAsync(sfs);

            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                Entity.ResetSkillCooldown(); // 쿨타임 초기화

                // 퀘스트 처리
                Quest.QuestProgress(QuestType.WORLD_BOSS_TYPE_COUNT, worldBossId); // 특정 월드 보스 처치(전투 참여)
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// [월드보스] 던전 나가기
        /// </summary>
        public async Task<Response> RequestWorldBossExit(bool isBossDie)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", isBossDie);
            var response = await Protocol.REQUEST_WORLD_BOSS_EXIT.SendAsync(sfs);

            if (!response.isSuccess)
                response.ShowResultCode();

            return response;
        }

        /// <summary>
        /// [월드보스] 알람 세팅
        /// </summary>
        public async void SetWorldBossAlarm(int worldBossId, bool isAlarm)
        {
            bool isSuccess = await RequestWolrdBossAlarm(worldBossId);

            if (!isSuccess)
                return;

            if (worldbossAlarm.Contains(worldBossId))
            {
                worldbossAlarm.Remove(worldBossId);
            }
            else
            {
                worldbossAlarm.Add(worldBossId);
            }
        }

        /// <summary>
        /// 알람 설정된 월드보스인지 여부
        /// </summary>
        /// <param name="worldBossId"></param>
        /// <returns></returns>
        public bool IsAlarmWorldBoss(int worldBossId)
        {
            return worldbossAlarm.Contains(worldBossId);
        }

        /// <summary>
        /// 월드보스 오픈 알림 체크
        /// </summary>
        private IEnumerator<float> UpdateWorldBossTime()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);

                if (worldBossRemainTime.Count == 0)
                    continue;

                var openWorldBoss = worldBossRemainTime.Where(x => x.Value <= 0).ToList();
                if (openWorldBoss.Count == 0)
                    continue;

                for (int i = 0; i < openWorldBoss.Count; i++)
                {
                    if (!worldbossOpenIds.Contains(openWorldBoss[i].Key) && worldbossAlarm.Contains(openWorldBoss[i].Key))
                        worldbossOpenIds.Add(openWorldBoss[i].Key);

                    worldBossRemainTime.Remove(openWorldBoss[i].Key);
                }

                OnUpdateWorldBossOpen?.Invoke();
            }
        }

        public void RemoveOpenWorldBossId(int id)
        {
            if (worldbossOpenIds.Contains(id))
                worldbossOpenIds.Remove(id);
        }

        private IEnumerator<float> UpdateWorldBossFreeTicketCoolTime()
        {
            while (!IsMaxFreeTicket)
            {
                if (WorldBossFreeTicketCoolTime <= 0)
                {

                    worldBossTicketRemainTime = BasisType.WORLD_BOSS_TICKET_TIME_COOL.GetInt();
                    worldBossFreeTicket = Mathf.Min(worldBossFreeTicket + 1, BasisType.WORLD_BOSS_FREE_JOIN_CNT.GetInt());
                    OnUpdateTicket?.Invoke();
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// 월드보스 알람 설정/해제
        /// </summary>
        private async Task<bool> RequestWolrdBossAlarm(int worldBossId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", worldBossId);

            Response response = await Protocol.REQUEST_WORLD_BOSS_ALARM.SendAsync(sfs);
            if (response.isSuccess)
            {

            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// 알람 설정된 월드보스 오픈까지 남은시간
        /// </summary>
        /// <param name="response"></param>
        private void OnResponseWorldBossAlarm(Response response)
        {
            if (response.isSuccess)
            {
#if UNITY_EDITOR
                Debug.Log($"=== 월드보스 오픈까지 남은시간 === {response.GetInt("1")}, {response.GetLong("2")}");
#endif
                SetWorldBossRemainTime(response.GetInt("1"), response.GetLong("2"));
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// [클리커 던전] 경험치/제니 던전 입장
        /// </summary>
        public async Task<Response> RequestClickerDungeonEnter(int id)
        {
            var param = Protocol.NewInstance();
            param.PutInt("1", id);
            var response = await Protocol.REQUEST_CLICKER_DUNGEON_START.SendAsync(param);
            if (response.isSuccess)
            {
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
            }
            return response;
        }

        /// <summary>
        /// [클리커 던전] 종료
        /// </summary>
        public async Task RequestClickerDungeonEnd(int[] clearCubeArray)
        {
            var sfs = Protocol.NewInstance();
            if (clearCubeArray != null)
                sfs.PutIntArray("1", clearCubeArray);

            var response = await Protocol.REQUEST_CLICKER_DUNGEON_END.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;

            if (charUpdateData != null)
                Notify(charUpdateData);

            OnClickerDungeonEnd?.Invoke(UI.ConvertRewardData(charUpdateData?.rewards));
        }

        private int curBossBattleEndChapter;
        private int curClientMonsterID;

        /// <summary>
        /// [멀티 미로] 보스 전투 종료
        /// </summary>
        public void RequestMultiMazeBossBattleEnd(bool isWin, int remainBossHp, int clientMonsterId, int multiMazeId, int chapter)
        {
            curBossBattleEndChapter = chapter;
            curClientMonsterID = clientMonsterId;

            const byte WIN = 1;
            const byte LOSE = 2;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", isWin ? WIN : LOSE);
            sfs.PutInt("2", remainBossHp);
            Protocol.REQUEST_MULMAZE_BOSSBATTLE_END.SendAsync(sfs).WrapNetworkErrors();
        }

        private void OnResponseMultiMazeBossBattleEnd(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.MULTI_MAZE_CHAPTER_CLEAR_COUNT, curBossBattleEndChapter); // 특정 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_10); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_15); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_20); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_25); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_30); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_35); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_40); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_45); // 멀티 미로 클리어 횟수
            Quest.QuestProgress(QuestType.MULTI_MAZE_CLEAR_COUNT_50); // 멀티 미로 클리어 횟수

            // cud. 캐릭터 업데이트 데이터
            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
            ProcessDroppedItem(charUpdateData, curClientMonsterID);
        }

        /// <summary>
        /// [시나리오 미로] 보스 전투 종료
        /// </summary>
        public async Task<Response> RequestScenarioMazeBossBattleEnd(int clientMonsterId, int randomKey)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", MathUtils.CidToHexCode(randomKey));

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("MVP몬스터 시나리오미로보스 처리: key = " + randomKey + ", hex = " + MathUtils.CidToHexCode(randomKey));
            }

            Response response = await Protocol.REQUEST_MAZE_BOSS_CLEAR.SendAsync(sfs);

            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
                ProcessDroppedItem(charUpdateData, clientMonsterId);
            }
            return response;
        }

        /// <summary>
        /// [길드전] 종료
        /// </summary>
        public async Task RequestGuildBattleEnd()
        {
            Response response = await Protocol.REQUEST_GUILD_BATTLE_END.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
            ProcessDroppedItem(charUpdateData);
        }

        public int GetCupetHp(int cupetId)
        {
            if (mazeCupetHpDic.ContainsKey(cupetId))
                return mazeCupetHpDic[cupetId];

            return 0;
        }

        private void SetCurrentMazeHp(int hp)
        {
            mazeHp = hp;
            OnChangeMazeHp?.Invoke();
        }

        public void SetCurrentMazeMp(int mp)
        {
            mazeMp = mp;
            OnChangeMazeMp?.Invoke();
        }

        /// <summary>
        /// MVP 몬스터 소환
        /// </summary>
        public async Task SummonMvpMonster()
        {
            Response response = await Protocol.REQUEST_AUTO_STAGE_SUMMON_MVP.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // MVP 몬스터 소환 정보
            if (response.ContainsKey("1"))
            {
                MvpMonsterPacket mvpPacket = response.GetPacket<MvpMonsterPacket>("1");

                serverMvpMonsterRandomKey = mvpPacket.randomKey;

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 MVP몬스터 RandomKey 값: " + serverMvpMonsterRandomKey);
                }

                OnAppearMvpMonster?.Invoke(mvpPacket.mvpTableId, mvpPacket.remainTime);
            }
        }

        /// <summary>
        /// 플레이어 소환
        /// </summary>
        public async Task RequestAutoStageSummonPlayer(int stageId)
        {
            Response response = await Protocol.REQUEST_AUTO_STAGE_SUMMON_PLAYER.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // 퀘스트 처리
            Quest.QuestProgress(QuestType.FIELD_ID_ASSEMBLE_COUNT, stageId); // 특정 필드에서 셰어+동료 집결 횟수
        }

        /// <summary>
        /// [멀티미로] 입장
        /// </summary>
        public async Task<Response> RequestMultiMazeRoomJoin(int channelId, int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", channelId); // 채널 id
            sfs.PutInt("2", 0); // 사용하지 않음
            sfs.PutInt("3", id);
            Response response = await Protocol.REQUEST_MULMAZE_ROOM_JOIN.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
                Notify(charUpdateData);

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }
            return response;
        }

        /// <summary>
        /// [멀티매칭미로] 입장
        /// </summary>
        public async Task<Response> RequestMultimazeMatchStart(int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 가고자 하는 id
            Response response = await Protocol.REQUEST_MULMAZE_MATCH_START.SendAsync(sfs);

            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
                Notify(charUpdateData);

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }

            return response;
        }

        /// <summary>
        /// [이벤트멀티매칭미로] 입장
        /// </summary>
        public async Task<Response> RequestEventMultimazeMatchStart(int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 가고자 하는 id
            Response response = await Protocol.REQUEST_EVENTMULMAZE_MATCH_START.SendAsync(sfs);

            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
                Notify(charUpdateData);

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }

            return response;
        }

        /// <summary>
        /// [이벤트미궁:암흑] 입장
        /// </summary>
        public async Task<Response> RequestDarkmazeMatchStart(int id)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 가고자 하는 id
            Response response = await Protocol.REQUEST_DARKMAZE_MATCH_START.SendAsync(sfs);

            if (response.isSuccess)
            {
                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }

            return response;
        }

        /// <summary>
        /// 던전 소탕
        /// </summary>
        public async Task RequestFastClearDungeon(DungeonType dungeonType, int id, bool isFree = false)
        {
            // 무료 소탕을 이용하여 던전을 소탕하시겠습니까?\n던전 입장 횟수가 1회 차감됩니다. : 던전 소탕권을 사용하여 해당 던전을 소탕하시겠습니까?
            string description = isFree ? LocalizeKey._90310.ToText() : LocalizeKey._90223.ToText();
            if (!await UI.SelectPopup(description))
                return;

            const byte TICKET = 0;
            const byte FREE = 1;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)dungeonType);
            sfs.PutInt("2", id);
            sfs.PutByte("3", isFree ? FREE : TICKET); // 무료 소탕 여부 

            Response response = await Protocol.REQUEST_DUNGEON_FAST_CLEAR.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
            Notify(charUpdateData);
            UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)

            Quest.QuestProgress(QuestType.DUNGEON_TYPE_CLEAR_COUNT, dungeonType.ToIntValue()); // 특정 타입 던전 클리어 횟수

            OnFastClear?.Invoke();
        }

        /// <summary>
        /// 클리어한 시나리오 ID 목록 세팅
        /// </summary>
        /// <param name="scenarioMazeIds"></param>
        public void SetCleardScenarioMazeIds(string scenarioMazeIds)
        {
            if (!string.IsNullOrEmpty(scenarioMazeIds))
            {
                cleardScenarioMazeIds.Clear();
                openedContentsByScenario.Clear();
                lastClearedScenarioID = 0;

                foreach (var item in scenarioMazeIds.Split(',').OrEmptyIfNull())
                {
                    if (int.TryParse(item, out int scenarioMazeId))
                    {
                        AddCleardScenarioMazeId(scenarioMazeId);
                    }
                }
            }
        }

        public bool IsOpenContent(ContentType contentType)
        {
            return openedContentsByScenario.Contains(contentType);
        }

        /// <summary>
        /// 클리어한 시나리오 ID 추가
        /// </summary>
        public void AddCleardScenarioMazeId(int scenarioMazeId)
        {
            if (!cleardScenarioMazeIds.Contains(scenarioMazeId))
            {
                cleardScenarioMazeIds.Add(scenarioMazeId);
                lastClearedScenarioID = Mathf.Max(lastClearedScenarioID, scenarioMazeId);
                ScenarioMazeData data = scenarioMazeDataRepo.Get(scenarioMazeId);

                if (data.OpenContent.HasValue)
                {
                    openedContentsByScenario.Add(data.OpenContent.Value);
                }
            }
        }

        /// <summary>
        /// 시나리오 클리어 여부
        /// </summary>
        public bool IsCleardScenarioMazeId(int scenarioMazeId)
        {
            return cleardScenarioMazeIds.Contains(scenarioMazeId);
        }

        /// <summary>
        /// 마지막에 입장한 멀티미로 로비 ID
        /// </summary>
        public void SetLastEnterMultiMazeLobbyId(int multiMazeLobbyId)
        {
            lastEnterMultiMazeLobbyId = multiMazeLobbyId;
        }

        /// <summary>
        /// 마지막에 입장한 멀티미로 ID (시나리오 입장 시도시 저장)
        /// </summary>
        public void SetLastEnterMultiMazeId(int multiMazeId)
        {
            lastEnterMultiMazeId = multiMazeId;
        }

        /// <summary>
        /// 마지막에 입장한 로비 채널
        /// </summary>
        public void SetLastEnterLobbyChannel(int lobbyChannel)
        {
            lastEnterLobbyChannel = Mathf.Max(1, lobbyChannel);
        }

        public void StartBattle()
        {
            switch (GameStartMapType)
            {
                case GameStartMapType.Lobby:
                    battleManager.StartBattle(BattleMode.Lobby);
                    break;

                case GameStartMapType.Stage:
                    battleManager.StartBattle(BattleMode.Stage);
                    break;

                case GameStartMapType.MultiMazeLobby:
                    battleManager.StartBattle(BattleMode.MultiMazeLobby);
                    break;

                case GameStartMapType.FreeFight:
                    AsyncExitFreeFight().WrapNetworkErrors(); // 난전 나가기 처리
                    break;

                case GameStartMapType.TimePatrol:
                    battleManager.StartBattle(BattleMode.TimePatrol);
                    break;
            }
        }

        /// <summary>
        /// 난전 나가기 처리
        /// </summary>
        private async Task AsyncExitFreeFight()
        {
            battleManager.StartBattle(BattleMode.Stage); // 일단 Stage 시작 (재접속이 일어날지 안 일어날지 모르기 때문에)

            // 난전 나가기 처리
            Response response = await Protocol.REQUEST_FF_ROOM_EXIT.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }
        }

        private ScenarioMazeMode ToContentMode(ContentType contentType)
        {
            foreach (ScenarioMazeMode item in System.Enum.GetValues(typeof(ScenarioMazeMode)))
            {
                if (item.GetOpenContentType() == contentType)
                    return item;
            }

            return default;
        }

        /// <summary>
        /// [테이밍] 테이밍 시작 요청 (반환: 대성공 여부)
        /// </summary>
        public async Task<Response> RequestTamingStart()
        {
            var response = await Protocol.REQUEST_TAMING_MONSTER.SendAsync();
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
                int drop_item_id = response.GetInt("1");
                int count = response.GetInt("2");
                byte result = response.GetByte("3"); // 0:실패, 1:성공, 2:대성공
            }
            else
            {
                response.ShowResultCode();
            }
            return response;
        }

        private void SetClearedDungeonGroupId(string text)
        {
            clearedZenyDungeonId = 0;
            clearedExpDungeonId = 0;
            clearedDefenceDungeonId = 0;
            clearedCentralLabId = 0;

            if (string.IsNullOrEmpty(text))
                return;

            // 던전 별 클리어 Id 목록
            string[] results = text.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in results)
            {
                string[] values = item.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 2)
                {
                    Debug.LogError($"클리어 던전 형식 오류: {item}");
                    continue;
                }

                if (int.TryParse(values[0], out int dungeonTypeValue) && int.TryParse(values[1], out int clearedId))
                {
                    DungeonType dungeonType = dungeonTypeValue.ToEnum<DungeonType>();
                    switch (dungeonType)
                    {
                        case DungeonType.ZenyDungeon:
                            clearedZenyDungeonId = clearedId;
                            Debug.Log($"clearedZenyDungeonId={clearedZenyDungeonId}");
                            break;

                        case DungeonType.ExpDungeon:
                            clearedExpDungeonId = clearedId;
                            Debug.Log($"clearedExpDungeonId={clearedExpDungeonId}");
                            break;

                        case DungeonType.Defence:
                            clearedDefenceDungeonId = clearedId;
                            Debug.Log($"clearedDefenceDungeonId={clearedDefenceDungeonId}");
                            break;

                        case DungeonType.WorldBoss:
                        case DungeonType.MultiMaze:
                        case DungeonType.EventMultiMaze:
                        case DungeonType.EnlessTower:
                        case DungeonType.ForestMaze:
                        case DungeonType.Gate:
                            // Do Nothing
                            break;

                        case DungeonType.CentralLab:
                            clearedCentralLabId = clearedId;
                            Debug.Log($"clearedCentralLabId={clearedCentralLabId}");
                            break;

                        default:
                            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
                    }
                }
                else
                {
                    Debug.LogError($"클리어 던전 형식 오류: {item}");
                }
            }
        }

        /// <summary>
        /// 이벤트 & 챌린지 레벨
        /// </summary>
        public int GetEventStageLevel(int stageId)
        {
            if (eventStageLevel.ContainsKey(stageId))
                return eventStageLevel[stageId];

            return 1; // 기본 레벨은 1
        }

        /// <summary>
        /// 이벤트 & 챌린지 모드 레벨 세팅
        /// </summary>
        private void SetEventStageLevel(int stageId, int level)
        {
            if (eventStageLevel.ContainsKey(stageId))
            {
                eventStageLevel[stageId] = level;
            }
            else
            {
                eventStageLevel.Add(stageId, level);
            }
        }

        /// <summary>
        /// 이벤트 & 챌린지 보스 도전 쿨타임
        /// </summary>
        public int GetEventStageBossCoolTime(int level)
        {
            int fixedCoolTime = BasisType.STAGE_HARD_CHELLENGE_COOL_TIME.GetInt(1); // 고정 쿨타임
            int increaseValue = BasisType.STAGE_HARD_CHELLENGE_COOL_TIME.GetInt(2); // 증가치
            return fixedCoolTime + (increaseValue * level);
        }

        /// <summary>
        /// 챌린지 모드 클리어 횟수
        /// </summary>
        public int GetChallengeClearCount(int stageId)
        {
            if (challengClearCount.ContainsKey(stageId))
                return challengClearCount[stageId];

            return 0;
        }

        /// <summary>
        /// 무료 챌린지 가능여부
        /// </summary>
        public bool HasFreeChallengeTicket()
        {
            // 마지막 입장 스테이지
            StageData stageData = stageDataRepo.Get(FinalStageId);
            if (stageData == null)
                return false;

            int eventStageFreeEnterCount = BasisType.CHELLENGE_FREE_ENTER_COUNT.GetInt(); // 하루 입장 무료 가능 횟수
            int eventStageMaxLevel = BasisType.HARD_CHELLENGE_MAX_LEVEL.GetInt();
            for (int chapter = 1; chapter <= stageData.chapter; chapter++)
            {
                StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Challenge, chapter);
                if (find == null)
                    continue;

                // 최대 레벨 도달 체크
                int stageLevel = GetEventStageLevel(find.id);
                if (stageLevel > eventStageMaxLevel)
                    continue;

                int clearCount = GetChallengeClearCount(find.id);
                if (clearCount < eventStageFreeEnterCount)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 챌린지 모드 클리어 횟수 세팅
        /// </summary>
        private void SetChallengeClearCount(int stageId, int clearCount)
        {
            if (challengClearCount.ContainsKey(stageId))
            {
                challengClearCount[stageId] = clearCount;
            }
            else
            {
                challengClearCount.Add(stageId, clearCount);
            }

            OnUpdateEventStageCount?.Invoke();
        }

        /// <summary>
        /// 스테이지 입장
        /// </summary>
        public void StartBattleStageMode(StageMode stageMode, int stageId)
        {
            RequestStageMode = stageMode;
            battleManager.StartBattle(BattleMode.Stage, stageId);
        }

        /// <summary>
        /// 이벤트스테이지 종료시점 체크
        /// </summary>
        IEnumerator<float> YieldCheckFinishEventStage()
        {
            yield return Timing.WaitUntilTrue(IsOpendEventStage);
            OnUpdateEventStageInfo?.Invoke();
        }

        /// <summary>
        /// 엔들리스 타워 종료시점 체크
        /// </summary>
        IEnumerator<float> YieldCheckFinishEndlessTower()
        {
            yield return Timing.WaitUntilFalse(HasCooltimeRemainEndlessTower);
            UpdateEndlessTowerTicket(BasisType.ENDLESS_TOWER_FREE_JOIN_COUNT.GetInt(), null);
        }

        /// <summary>
        /// 최종 도달한 타임패트롤 레벨
        /// </summary>
        private void SetFinalTimePatrolLevel(int level)
        {
            FinalTimePatrolLevel = Mathf.Max(level, FinalTimePatrolLevel); // 기존의 레벨 비교했을 때, 큰 값을 사용
        }

        /// <summary>
        /// 마지막에 입장한 타임패트롤 ID
        /// </summary>
        private void SetLastEnterTimePatrolId(int id)
        {
            // 존재하지 않는 아이디의 경우(마지막) 이전으로 진입
            if (!timePatrolStageDataRepo.IsExists(id))
                --id;

            LastEnterTimePatrolId = Mathf.Max(1, id);
        }

        /// <summary>
        /// [타임패트롤] 시작
        /// </summary>
        public async Task<Response> RequestTimePatrolStart(int timePatrolId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", timePatrolId);

            var response = await Protocol.REQUEST_TP_STAGE_ENTER.SendAsync(sfs);

            if (response.isSuccess)
            {
                StageMode = StageMode.TimePatrol;
                SetLastEnterTimePatrolId(timePatrolId);

                TimePatrolStageData data = timePatrolStageDataRepo.Get(timePatrolId);
                if (data != null)
                    SetFinalTimePatrolLevel(data.level);

                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                Entity.ResetSkillCooldown(); // 쿨타임 초기화
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// [타임패트롤] 보스 소환
        /// </summary>
        public async Task RequestSummonTimePatrolBoss()
        {
            Response response = await Protocol.REQUEST_TP_SUMMON_BOSS.SendAsync();
            if (response.isSuccess)
            {
                int timePatrolBossDataId = response.GetInt("1"); // 타임패트롤 보스 테이블 ID
                int remainTime = response.GetInt("2");
                serverBossMonsterRandomKey = response.GetInt("3");

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 보스몬스터 RandomKey 값: " + serverBossMonsterRandomKey);
                }

                OnAppearTimePatrolBossMonster?.Invoke(timePatrolBossDataId, remainTime);
                return;
            }

            response.ShowResultCode();
        }

        /// <summary>
        /// [타임패트롤] 일반 몬스터 아이템 드랍 (타임패트롤 전용)
        /// </summary>
        public async Task RequestTimePatrolNormalMonsterDrop(int monsterId, int clientMonsterId, int stageId, DamagePacket damagePacket)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", monsterId);
            sfs.PutInt("2", clientMonsterId);

            if (serverMonsterRandomKey > 0)
            {
                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("일반몬스터 RandomKey 처리: key = " + serverMonsterRandomKey + ", hex = " + MathUtils.CidToHexCode(serverMonsterRandomKey));
                }

                sfs.PutUtfString("3", MathUtils.CidToHexCode(serverMonsterRandomKey));
                serverMonsterRandomKey = 0;
            }

            sfs.PutInt("4", stageId);
#if UNITY_EDITOR
            sfs.PutByteArray("99", damagePacket.ToByteArray()); // 대미지 패킷
#endif

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            Response response = await Protocol.REQUEST_TP_MON_KILL.SendAsync(sfs);
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.STAGE_MON_NOT_EXISTS)
                {
#if UNITY_EDITOR
                    Debug.LogError("일치하지 않은 몬스터 정보");
#endif
                    return;
                }

                response.ShowResultCode();
                return;
            }

#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
        }

        /// <summary>
        /// [타임패트롤] 보스 클리어 (타임패트롤 전용)
        /// </summary>
        public async Task RequestTimePatrolBossClear(bool isClear, int stageId, int clientMonsterId, DamagePacket damagePacket)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", clientMonsterId);
            sfs.PutByteArray("2", damagePacket.ToByteArray()); // 대미지 패킷
            sfs.PutBool("3", isClear);
            sfs.PutUtfString("4", MathUtils.CidToHexCode(serverBossMonsterRandomKey));

            if (DebugUtils.IsMonsterDropKey)
            {
                Debug.Log("보스몬스터 RandomKey 처리: key = " + serverBossMonsterRandomKey + ", hex = " + MathUtils.CidToHexCode(serverBossMonsterRandomKey));
            }

#if UNITY_EDITOR
            DebugDamageTuple debugDamageTuple = new DebugDamageTuple(damagePacket);
#endif
            Response response = await Protocol.REQUEST_TP_BOSS_KILL.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            int receivedClientMonsterId = response.ContainsKey("1") ? response.GetInt("1") : 0;
            long bossCoolTime = response.GetLong("2");
            RewardPacket[] wasted = response.ContainsKey("3") ? response.GetPacketArray<RewardPacket>("3") : null; // 3. 꽉차서 받지 못한 아이템

#if UNITY_EDITOR
            if (response.ContainsKey("99"))
            {
                debugDamageTuple.SetServerResult(new DamageCheckPacket(response.GetByteArray("99")));
                DamageCheck.Add(debugDamageTuple); // 대미지 패킷에 저장
            }
#endif
            UnitEntity unitEntity = (clientMonsterId > 0 && UnitEntity.entityDic.ContainsKey(clientMonsterId)) ? UnitEntity.entityDic[clientMonsterId] : null;

            if (charUpdateData != null)
                Notify(charUpdateData);

            OnTimePatrolBossMonsterItemDrop?.Invoke(unitEntity, UI.ConvertRewardData(charUpdateData?.rewards), UI.ConvertRewardData(wasted), bossCoolTime, isClear);

            // 데이터 세팅하기 전 이벤트를 호출할 것!!
            if (isClear)
            {
                // 몬스터로 인한
                if (unitEntity)
                {
                    // 퀘스트 체크
                    UnitSizeType unitSizeType = unitEntity.battleUnitInfo.UnitSizeType;
                    ElementType elementType = unitEntity.battleUnitInfo.UnitElementType;
                    int monsterGroup = MonsterDataManager.Instance.Get(unitEntity.battleUnitInfo.Id).monster_group;

                    Quest.QuestProgress(QuestType.MONSTER_KILL); // 몬스터 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_TARGET, unitEntity.battleUnitInfo.Id); // 특정 몬스터 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_SIZE, (int)unitSizeType); // 몬스터 크기별 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_KILL_ELEMENT, (int)elementType); // 몬스터 속성별 사냥 수
                    Quest.QuestProgress(QuestType.MONSTER_GROUP_KILL_COUNT, conditionValue: monsterGroup); // 특정 몬스터 그룹 처치 수

                    // 획득 보상의 경우에는
                    if (charUpdateData != null)
                    {
                        UI.RewardToast(charUpdateData.rewards); // 획득 보상 보여줌 (토스트팝업)
                    }
                }

                // 몬스터 도감 획득
                if (response.ContainsKey("5"))
                {
                    int bookIndex = response.GetShort("5");
                    NotyfyBookRecord(BookTabType.Monster, bookIndex);
                }
            }
        }

        void OnTimePatrolNormalMonsterDrop(Response response)
        {
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.STAGE_MON_NOT_EXISTS)
                {
#if UNITY_EDITOR
                    Debug.LogError("일치하지 않은 몬스터 정보");
#endif
                    return;
                }

                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            int receivedClientMonsterId = response.ContainsKey("2") ? response.GetInt("2") : 0;

            // 알람 타입(New 표시)
            if (response.ContainsKey("1"))
                NotifyAlarm(response.GetInt("1"));

            // eb. 이벤트 버프 정보
            if (response.ContainsKey("eb"))
            {
                EventBuffPacket eventBuffPacket = response.GetPacket<EventBuffPacket>("eb");
                Notify(eventBuffPacket);
            }

            // 몬스터 도감 획득
            if (response.ContainsKey("5"))
            {
                int bookIndex = response.GetShort("5");
                NotyfyBookRecord(BookTabType.Monster, bookIndex);
            }

            // 서버에서 받은 몬스터 랜덤 키
            if (response.ContainsKey("3"))
            {
                serverMonsterRandomKey = response.GetInt("3");

                if (DebugUtils.IsMonsterDropKey)
                {
                    Debug.Log("서버에서 받은 일반몬스터 RandomKey 값: " + serverMonsterRandomKey);
                }
            }

            ProcessDroppedItem(charUpdateData, receivedClientMonsterId);
        }

        /// <summary>
        /// 타임패트롤 오픈 여부
        /// </summary>
        public bool IsOpenTimePatrol(bool isShowPopup)
        {
            int stageId = BasisType.TP_OPEN_STAGE_ID.GetInt();
            if (FinalStageId >= stageId)
                return true;

            if (isShowPopup)
            {
                StageData data = stageDataRepo.Get(stageId - 1);
                if (data != null)
                {
                    string message = LocalizeKey._90131.ToText() // {NAME} 클리어시 오픈 됩니다.
                        .Replace(ReplaceKey.NAME, data.name_id.ToText());

                    UI.ShowToastPopup(message);
                }
            }

            return false;
        }

        /// <summary>
        /// 게이트 오픈 여부
        /// </summary>
        public bool IsOpenGate(bool isShowPopup)
        {
            int chapter = GateDataManager.Instance.First.chapter;
            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Normal, chapter);
            if (find == null)
                return false;

            if (FinalStageId >= find.id)
                return true;

            if (isShowPopup)
            {
                string message = LocalizeKey._90131.ToText() // {NAME} 클리어시 오픈 됩니다.
                        .Replace(ReplaceKey.NAME, find.name_id.ToText());

                UI.ShowToastPopup(message);
            }

            return false;
        }

        /// <summary>
        /// 타임 패트롤 진입 가능 여부
        /// </summary>
        public bool CanEnterTimePatrol(int id)
        {
            var data = timePatrolStageDataRepo.Get(id);
            if (data.level > FinalTimePatrolLevel)
            {
                // 입장 가능한 레벨을 넘음
                return false;
            }

            return true;
        }

        /// <summary>
        /// 챕터 별 보스 처치 실패 시 보스 도전 쿨타임(밀리초)
        /// </summary>
        public int GetStageBossFailCoolTime(int chapter)
        {
            return BasisType.CHAPTER_BOSS_FAILURE_COOLTIME.GetInt(chapter);
        }

        /// <summary>
        /// 챕터 별 보스 처치 성공 후 보스 도전 쿨타임(밀리초)
        /// </summary>
        public int GetStageBossClearCoolTime(int chapter)
        {
            return BasisType.CHAPTER_BOSS_CLEAR_COOLTIME.GetInt(chapter);
        }

        /// <summary>
        /// 현재 진행중인 챕터 세팅 (스테이지 축복 버프 적용하기 위하여 세팅)
        /// </summary>
        private void SetChapter(int chapter)
        {
            if (StageChapter == chapter)
                return;

            StageChapter = chapter;
            OnUpdateStageChapter?.Invoke();
        }
    }
}