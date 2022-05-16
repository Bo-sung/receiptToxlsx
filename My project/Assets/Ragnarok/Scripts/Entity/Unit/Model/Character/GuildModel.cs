using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 길드 정보
    /// </summary>
    public class GuildModel : CharacterEntityModel
    {
        public interface IInputValue
        {
            int GuildId { get; }
            string GuildName { get; }
            int GuildEmblem { get; }
            byte GuildPosition { get; }
            int GuildCoin { get; }
            int GuildQuestRewardCount { get; }
            long GuildSkillBuyDateTime { get; }
            byte GuildSkillBuyCount { get; }
            long GuildRejoinTime { get; }
        }

        public interface IGuildSkill
        {
            int GuldId { get; }
            int SkillId { get; }
            int Exp { get; }
            int Level { get; }
        }

        private readonly SkillDataManager skillRepo;
        private readonly ProfileDataManager profileDataRepo;
        private readonly ItemDataManager itemDataRepo;

        #region 데이터

        // 길드 정보
        private ObscuredInt guildId;
        private ObscuredString guildName;
        private ObscuredString guildIntroduction;
        private ObscuredInt emblemId;
        private ObscuredInt emblemBg;
        private ObscuredInt emblemframe;
        private ObscuredInt emblemIcon;
        private ObscuredString masterName;
        private ObscuredByte memberCount;
        private ObscuredByte maxMemberCount;
        private ObscuredByte isAutoJoin;
        private ObscuredInt masterUid, masterCid;

        private ObscuredInt expPoint;
        private ObscuredInt level;
        private ObscuredString notice;
        private ObscuredByte onlineMemberCount;

        // 길드 출석 정보
        private ObscuredInt yesterDayMemberCount;
        private ObscuredInt todayMemberCount;
        private ObscuredBool isTodayAttend;
        private ObscuredBool isGetAttendReward;

        // 길드에서 내정보
        private ObscuredByte guildPosition;
        private ObscuredInt guildQuestRewardCount;
        private ObscuredInt guildDonatePoint;
        private ObscuredBool isTodayJoin;
        private ObscuredBool canMasterDismissal;
        private ObscuredBool isGetMasterPosition;
        private RemainTime guildOutRemainTime;

        // 길드 스킬
        private RemainTime guildSkillBuyTime;
        private ObscuredByte guildSkillBuyCount;

        /// <summary>
        /// 추천 길드 목록 or 검색한 길드
        /// </summary>
        private List<GuildSimpleInfo> recommendGuildList;

        /// <summary>
        ///길드 가입 신청한 목록
        /// </summary>
        private List<GuildSimpleInfo> joinSubmitGuildList;

        /// <summary>
        /// 길드 가입 신청한 유저 목록
        /// </summary>
        private List<GuildJoinSubmitInfo> joinSubmitUserList;

        /// <summary>
        /// 길드원 목록
        /// </summary>
        private List<GuildMemberInfo> memberList;

        /// <summary>
        /// 길드 스킬 목록
        /// </summary>
        public readonly List<GuildSkill> guildSkillList;

        /// <summary>
        /// 길드 게시판 목록
        /// </summary>
        private SortedList<int, GuildBoardInfo> guildBoardList;

        /// <summary>
        /// 출석체크 인원별 보상 정보 목록
        /// </summary>
        public List<GuildAttendRewardInfo> guildAttendRewardInfos { get; private set; }

        /// <summary>
        /// 스킬 버퍼
        /// </summary>
        private readonly Buffer<SkillInfo> skillBuffer;

        /// <summary>
        /// [길드전] 엠펠리움 최대 Hp
        /// </summary>
        private int maxEmperiumHp;

        /// <summary>
        /// [길드전] 전투 진형 정보 세팅
        /// </summary>
        private bool isRequestGuildBattleAttackPosition;

        // 게시판
        private int curPage;
        private int? nextPage;
        private const int PAGE_COUNT = 5;

        public int CID => Entity.Character.Cid;
        public string Name => Entity.Character.Name;

        public int GuildId => guildId;
        /// <summary>
        /// 길드가입 여부
        /// </summary>
        public bool HaveGuild => guildId != 0;
        /// <summary>
        /// 길드 이름
        /// </summary>
        public string GuildName => guildName;

        /// <summary>
        /// 길드 레벨
        /// </summary>
        public int GuildLevel => level;
        /// <summary>
        /// 길드 마스터 이름
        /// </summary>
        public string MasterName => masterName;

        /// <summary>
        /// 길드원 수
        /// </summary>
        public byte MemberCount => memberCount;

        /// <summary>
        /// 길드원 최대 수
        /// </summary>
        public byte MaxMemberCount => maxMemberCount;

        /// <summary>
        /// 길드 경험치
        /// </summary>
        public int ExpPoint => expPoint;

        public int EmblemId => emblemId;
        public int EmblemBg => emblemBg;
        public int EmblemFrame => emblemframe;
        public int EmblemIcon => emblemIcon;
        public string Introduction => guildIntroduction;
        public string Notice => notice;
        public byte OnlineMemberCount => onlineMemberCount;
        public GuildPosition GuildPosition => guildPosition.ToEnum<GuildPosition>();
        public int GuildDonationPoint => guildDonatePoint;

        /// <summary>
        /// 길드 퀘스트 완료 횟수 (일일) 
        /// </summary>
        public int GuildQuestRewardCount => Entity.Quest.GetGuildQuestRewardCount(); //guildQuestRewardCount;

        public bool IsMaxLevel => BasisType.GUILD_LEVEL_UP_EXP.GetInt(level + 1) == 0;

        public int CurLevelExp
        {
            get
            {
                var preExp = BasisType.GUILD_LEVEL_UP_EXP.GetInt(level);
                return expPoint - preExp;
            }
        }

        public int CurNeedLevelExp
        {
            get
            {
                var needExp = BasisType.GUILD_LEVEL_UP_EXP.GetInt(level + 1);
                // 최대 레벨
                if (needExp == 0)
                    return 0;

                int preExp = BasisType.GUILD_LEVEL_UP_EXP.GetInt(level);
                return needExp - preExp;
            }
        }

        public byte IsAutoJoin => isAutoJoin;

        // 출석 관련
        public int YesterdayMemberCount => yesterDayMemberCount;
        public int TodayMemberCount => todayMemberCount;
        public bool IsTodayAttend => isTodayAttend;
        public bool IsGetAttendReward => isGetAttendReward;
        public RemainTime RejoinTime; // 길드 재가입까지 남은 시간
        public RemainTime GuildOutRemainTime => guildOutRemainTime; // 길드 탈퇴까지 남은 시간

        // 스킬
        public RemainTime GuildSkillBuyTime => guildSkillBuyTime;
        public byte GuildSkillBuyCount => guildSkillBuyCount;

        public bool HasNextPage => nextPage.HasValue;

        // 길드 엠블렘 변경에 사용
        public byte SelectEmblemBg;
        public byte SelectEmblemFrame;
        public byte SelectEmblemIcon;

        // 길드 테이밍 미로 관련
        private int taming_id;
        private bool isTamingMazeInProgress;
        private RemainTime tamingMazeRemainTime;
        private int tamingMazeSeason;
        private CoroutineHandle handleRefreshTamingMazeInfo;
        private bool hasTamingMazeNotice;
        private List<DateTime> tamingMazeTimeTable; // 테이밍 미로 오픈 시각(서버타임) 목록

        /// <summary>
        /// 테이밍미로 알림 유무 (Notice)
        /// </summary>
        public bool HasTamingMazeNotice
        {
            get => hasTamingMazeNotice;
            set
            {
                hasTamingMazeNotice = value;
                OnTamingMazeOpen?.Invoke(hasTamingMazeNotice);
            }
        }

        /// <summary>
        /// 오늘의 테이밍 미로 Id
        /// </summary>
        public int TamingId => taming_id;

        /// <summary>
        /// 테이밍 미로 진행중 여부
        /// </summary>
        public bool IsTamingMazeInProgress => isTamingMazeInProgress;

        /// <summary>
        /// 테이밍 미로 남은 시간 (진행중이 아닌 경우, 다음 시작까지 남은 시간)
        /// </summary>
        public RemainTime TamingMazeRemainTime => tamingMazeRemainTime;

        /// <summary>
        /// 현재 테이밍 미로 시즌
        /// </summary>
        public int TamingMazeSeason => tamingMazeSeason;

        /// <summary>
        /// 선택한 길드원 정보
        /// </summary>
        public GuildMemberInfo SelectGuildMemberInfo { get; private set; }

        /// <summary>
        /// 부길마일 때, 길드권한을 가지고 올 수 있는지 여부
        /// </summary>
        public bool CanMasterDismissal => canMasterDismissal;

        /// <summary>
        /// 길드명 무료 변경 남은 횟수
        /// </summary>
        public int FreeGuildNameChangeCount { get; private set; }

        /// <summary>
        /// [길드전] 길드전 시즌 타입
        /// </summary>
        public GuildBattleSeasonType GuildBattleSeasonType { get; private set; }

        /// <summary>
        /// [길드전] 길드전 시즌 타입에 따른 남은 시간
        /// </summary>
        public RemainTime GuildBattleSeasonRemainTime { get; private set; }

        /// <summary>
        /// [길드전] 죄측 포탑에 배치된 큐펫
        /// </summary>
        private readonly Buffer<int> leftCupetIds;

        /// <summary>
        /// [길드전] 우측 포탑에 배치된 큐펫
        /// </summary>
        private readonly Buffer<int> rightCupetIds;

        /// <summary>
        /// [길드전] 길드전 신청 여부
        /// </summary>
        public bool IsGuildBattleRequest { get; private set; }

        /// <summary>
        /// [길드전] 버프 정보
        /// </summary>
        public readonly Dictionary<int, GuildBattleBuffPacket> GuildBattleBuffDic;

        /// <summary>
        /// [길드전] 다른 길드 정보
        /// </summary>
        private readonly BetterList<GuildBattleOpponentPacket> guildBattleOpponents;

        /// <summary>
        /// [길드전] 내가 입힌 누적피해량
        /// </summary>
        public long GuildBattleAccrueDamage { get; private set; }

        /// <summary>
        /// [길드전] 길드전 남은 공격횟수
        /// </summary>
        public int GuildBattleEnterRemainCount { get; private set; }

        /// <summary>
        /// [길드전] 길드전에 사용한 길드원 Cid
        /// </summary>
        public readonly BetterList<int> UsedGuildBattleSupportAgents;

        /// <summary>
        /// [길드전] 길드전 방어 포탑에 사용한 큐펫 ID 목록
        /// </summary>
        public readonly BetterList<int> UsedGuildBattleDefenseTurretCupetIds;

        #endregion

        #region 이벤트

        /// <summary>
        /// 길드 상태 변경
        /// </summary>
        public event Action OnUpdateGuildState;

        /// <summary>
        /// 길드 스킬 변경시 호출
        /// </summary>
        public event Action OnUpdateGuildSkill;

        /// <summary>
        /// 길드 엠블럼 변경시 호출
        /// </summary>
        public Action OnUpdateGuildEmblem;

        /// <summary>
        /// 길드 직위 변경시 호출
        /// </summary>
        public Action OnUpdateGuildPoision;

        /// <summary>
        /// 길드원 인원수 변경 변경시 호출(가입/추방)
        /// </summary>
        public Action OnUpdateGuildMemberCount;

        /// <summary>
        /// 길드게시판 변경시 호출
        /// </summary>
        public Action OnUpdateGuildBoard;

        /// <summary>
        /// 길드 가입 여부
        /// </summary>
        public Action<bool> OnHaveGuild;

        /// <summary>
        /// 테이밍 미로 열림/닫힘 (true: 열림, false: 닫힘)
        /// </summary>
        public Action<bool> OnTamingMazeOpen;

        /// <summary>
        /// [길드습격] 포션 사용
        /// </summary>
        public event Action OnUseGuildAttackPotion;

        /// <summary>
        /// 길드명 변경 시 호출
        /// </summary>
        public event Action OnUpdateGuildName;

        /// <summary>
        /// 길드전 신청 후 호출
        /// </summary>
        public event Action OnUpdateGuildBattleRequest;

        /// <summary>
        /// 길드전 버프 정보 업데이트 이벤트
        /// </summary>
        public event Action OnUpdateGuildBattleBuff;

        /// <summary>
        /// 길드전 전투 길드목록 업데이트 이벤트
        /// </summary>
        public event Action OnUpdateGuildBattleList;

        /// <summary>
        /// 길드원 목록 갱신
        /// </summary>
        public event Action OnUpdateGuildMember;

        /// <summary>
        /// 길드 정보 업데이트 이벤트
        /// </summary>
        public event Action OnUpdateGuildInfo;

        #endregion

        public GuildModel()
        {
            recommendGuildList = new List<GuildSimpleInfo>();
            joinSubmitGuildList = new List<GuildSimpleInfo>();
            joinSubmitUserList = new List<GuildJoinSubmitInfo>();
            memberList = new List<GuildMemberInfo>();
            guildSkillList = new List<GuildSkill>();
            guildBoardList = new SortedList<int, GuildBoardInfo>(new IntRevereser());
            guildAttendRewardInfos = new List<GuildAttendRewardInfo>();
            tamingMazeTimeTable = new List<DateTime>();
            skillRepo = SkillDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            skillBuffer = new Buffer<SkillInfo>();
            leftCupetIds = new Buffer<int>();
            rightCupetIds = new Buffer<int>();
            GuildBattleBuffDic = new Dictionary<int, GuildBattleBuffPacket>(IntEqualityComparer.Default);
            guildBattleOpponents = new BetterList<GuildBattleOpponentPacket>();
            UsedGuildBattleSupportAgents = new BetterList<int>();
            UsedGuildBattleDefenseTurretCupetIds = new BetterList<int>();
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.AddEvent(OnReceiveCharDailyCalc);
                Protocol.REQUEST_GUILD_SKILL_LEVEL_CHANGE.AddEvent(OnReceiveGuildSkillLevel);
                Protocol.RESPONSE_GUILD_MESSAGE.AddEvent(OnReceiveGuildMessage);

                handleRefreshTamingMazeInfo = Timing.RunCoroutine(YieldRefreshTamingMazeInfo());
            }

            if (Entity.Quest != null)
            {
                OnHaveGuild += Entity.Quest.SetHasGuild;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.RemoveEvent(OnReceiveCharDailyCalc);
                Protocol.REQUEST_GUILD_SKILL_LEVEL_CHANGE.RemoveEvent(OnReceiveGuildSkillLevel);
                Protocol.RESPONSE_GUILD_MESSAGE.RemoveEvent(OnReceiveGuildMessage);

                Timing.KillCoroutines(handleRefreshTamingMazeInfo);
            }

            if (Entity.Quest != null)
            {
                OnHaveGuild -= Entity.Quest.SetHasGuild;
            }
        }

        /// <summary>
        /// 테이밍 미로 갱신 코루틴
        /// </summary>
        private IEnumerator<float> YieldRefreshTamingMazeInfo()
        {
            long tamingDungeonProgressTime = 0;

            var lastTime = ServerTime.Now;
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);

                if (taming_id == default) // 초기화 체크
                    continue;

                // 기초데이터 로딩을 기다린다.
                if (tamingDungeonProgressTime == 0)
                {
                    tamingDungeonProgressTime = BasisType.TAMING_DUNGEON_OPEN_DURATION.GetInt();
                    continue;
                }

                var now = ServerTime.Now;
                for (int i = 0; i < tamingMazeTimeTable.Count; ++i)
                {
                    var startTime = tamingMazeTimeTable[i];
                    var endTime = startTime.AddMilliseconds(tamingDungeonProgressTime);

                    if (now < startTime)
                        continue;

                    if (now < endTime) // 현재 던전이 열려있을 때
                    {
                        bool isDirty = lastTime < startTime;
                        if (isDirty) // 이번 프레임에 열렸다면 Notice처리
                        {
                            HasTamingMazeNotice = true;
                        }
                    }
                    else // 던전이 닫혔을 때
                    {
                        bool isDirty = lastTime < endTime;
                        if (isDirty) // 이번 프레임에 닫혔다면 Notice처리
                        {
                            HasTamingMazeNotice = false;
                        }
                        tamingMazeTimeTable[i] = startTime.AddDays(1); // 이미 종료된 던전은 다음 날의 시간으로 재설정
                    }
                }

                lastTime = now;
            }
        }

        public override void ResetData()
        {
            base.ResetData();

            Reset();
        }

        private void Reset()
        {
            guildId = 0;
            guildName = null;
            masterUid = 0;
            masterCid = 0;

            guildSkillList.Clear();
            guildAttendRewardInfos.Clear();
            skillBuffer.Release();

            canMasterDismissal = false;
            guildSkillList.Clear();
            leftCupetIds.Release();
            rightCupetIds.Release();
            GuildBattleBuffDic.Clear();
            guildBattleOpponents.Release();
            UsedGuildBattleSupportAgents.Release();
            UsedGuildBattleDefenseTurretCupetIds.Release();
            maxEmperiumHp = 0;
            isRequestGuildBattleAttackPosition = false;
        }

        internal void Initialize(IInputValue inputValue)
        {
            Reset();
            SetGuildSkill();
            SetGuildAttendReward();

            guildId = inputValue.GuildId;
            guildName = inputValue.GuildName;

            if (HaveGuild)
            {
                emblemId = inputValue.GuildEmblem;
                emblemBg = MathUtils.GetBitFieldValue(inputValue.GuildEmblem, 0);
                emblemframe = MathUtils.GetBitFieldValue(inputValue.GuildEmblem, 6);
                emblemIcon = MathUtils.GetBitFieldValue(inputValue.GuildEmblem, 12);
                guildPosition = inputValue.GuildPosition;
                guildQuestRewardCount = inputValue.GuildQuestRewardCount; // 쓰이지 않음. 완료된 퀘스트 수를 직접 세는 방식으로 변경.
                guildSkillBuyTime = inputValue.GuildSkillBuyDateTime;
                guildSkillBuyCount = inputValue.GuildSkillBuyCount;
            }

            RejoinTime = inputValue.GuildRejoinTime;

            OnHaveGuild?.Invoke(HaveGuild);
        }

        /// <summary>
        /// 길드 스킬 세팅
        /// </summary>
        internal void Initialize(IGuildSkill[] guildSkills)
        {
            // 더미 캐릭터의 경우에 InputValue 에 의한 Initialize 없이 호출한다.
            if (guildSkillList.Count == 0)
                SetGuildSkill();

            foreach (var item in guildSkills.OrEmptyIfNull())
            {
                var skill = guildSkillList.FirstOrDefault(x => x.SkillId == item.SkillId);
                if (skill == null)
                    continue;

                skill.SetExp(item.Exp);

                // 길드 인원수 증가스킬 
                if (item.SkillId == 0)
                {
                    skill.SetData(new SkillData(0, item.Level));

                    if (item.Level > 0)
                        skill.SetIsInPossession();

                    continue;
                }

                var data = skillRepo.Get(item.SkillId, item.Level);
                if (data != null)
                {
                    skill.SetData(data);

                    if (item.Level > 0)
                        skill.SetIsInPossession();
                }
                else
                {
                    // 엠펠리움 버프 스킬의 경우 0레벨로 변경시 처리
                    int emperiumBuffID = BasisType.GUILD_ATTACK_EMPERIUM_BUFF_SKILL_ID.GetInt();
                    if (item.SkillId == emperiumBuffID && item.Level == 0)
                    {
                        skill.ResetIsInPossession();
                    }
                }
            }

            OnUpdateGuildSkill?.Invoke();
        }

        /// <summary>
        /// 테이밍 던전 오픈 시간 초기화
        /// </summary>
        public void Initialize(TamingDungeonTimePacket timePacket)
        {
            tamingMazeTimeTable.Clear();

            foreach (var time in timePacket.today_open_time)
            {
                DateTime startTime = time.ToDateTime();
                tamingMazeTimeTable.Add(startTime);
            }
        }

        /// <summary>
        /// 길드 정보 업데이트
        /// </summary>
        public void UpdateGuildInfo(string masterName, int memberCount, int maxMemberCount, int expPoint)
        {
            this.masterName = masterName;
            this.memberCount = (byte)memberCount;
            this.maxMemberCount = (byte)maxMemberCount;
            this.expPoint = expPoint;
            level = GetGuildLevel(expPoint);
        }

        /// <summary>
        /// 길드전 리스트 세팅
        /// </summary>
        private void SetBattleGuildOpponents(GuildBattleOpponentPacket[] packets)
        {
            guildBattleOpponents.Clear();

            foreach (GuildBattleOpponentPacket item in packets)
            {
                // Initialize
                item.SetMaxHp(GetMaxEmperiumHp());
                item.SetGuildLevel(GetGuildLevel(item.exp));

                guildBattleOpponents.Add(item);
            }
        }

        /// <summary>
        /// 하루 날짜 변동
        /// </summary>
        private void OnReceiveCharDailyCalc(Response response)
        {
            if (response.isSuccess)
            {
                guildSkillBuyCount = 0;
                isTodayAttend = false;
                isGetAttendReward = false;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 스킬 레벨 변동
        /// </summary>
        private async void OnReceiveGuildSkillLevel(Response response)
        {
            if (response.isSuccess)
            {
                await RequestGuildSkillList(isLevelUp: true);
                OnUpdateGuildSkill?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 상태 변동
        /// </summary>
        private async void OnReceiveGuildMessage(Response response)
        {
            if (response.isSuccess)
            {
                GuildUpdateType guildUpdateType = response.GetByte("1").ToEnum<GuildUpdateType>();
                switch (guildUpdateType)
                {
                    case GuildUpdateType.Join:
                        UI.ShowToastPopup(LocalizeKey._90069.ToText()); // 길드에 가입되었습니다.

                        await RequestMyGuildInfo(isForce: true);
                        await RequestGuildSkillList(isLevelUp: true);
                        OnUpdateGuildSkill?.Invoke();
                        OnUpdateGuildState?.Invoke();
                        OnHaveGuild?.Invoke(HaveGuild);

                        break;
                    case GuildUpdateType.Kick:
                        Reset();

                        OnUpdateGuildSkill?.Invoke();
                        OnUpdateGuildState?.Invoke();
                        OnHaveGuild?.Invoke(HaveGuild);
                        break;

                    case GuildUpdateType.UpdatePosition:
                        await RequestMyGuildInfo(isForce: true);

                        OnUpdateGuildPoision?.Invoke();
                        break;
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        private void SetGuildSkill()
        {
            guildSkillList.Clear();
            var keyList = BasisType.GUILD_SKILL.GetKeyList();
            foreach (var item in keyList)
            {
                GuildSkill guildSkill = new GuildSkill();
                // 길드 인원수 스킬
                if (item == 1)
                {
                    guildSkill.SetData(new SkillData(skillId: 0, skillLevel: 0));
                }
                else
                {
                    guildSkill.SetData(skillRepo.Get(BasisType.GUILD_SKILL.GetInt(item), 1));
                }
                guildSkillList.Add(guildSkill);
            }

            // 엠펠리움 버프 추가
            GuildSkill emperiulBuff = new GuildSkill();
            emperiulBuff.SetData(skillRepo.Get(BasisType.GUILD_ATTACK_EMPERIUM_BUFF_SKILL_ID.GetInt(), 1));
            guildSkillList.Add(emperiulBuff);
        }

        private void SetGuildAttendReward()
        {
            guildAttendRewardInfos.Clear();
            int count = BasisType.GUILD_ATTEND_COUNT_REWARD.GetKeyList().Count;
            for (int i = 0; i < count; i++)
            {
                int key = BasisType.GUILD_ATTEND_COUNT_REWARD.GetKeyList()[i];
                var reward = new GuildAttendRewardInfo(key, BasisType.GUILD_ATTEND_COUNT_REWARD.GetInt(key));
                guildAttendRewardInfos.Add(reward);
            }
        }

        /// <summary>
        /// 추천 길드 목록 반환
        /// </summary>
        public GuildSimpleInfo[] GetGuildRecommends()
        {
            return recommendGuildList.ToArray();
        }

        /// <summary>
        /// 가입 신청한 길드 목록 반환
        /// </summary>
        public GuildSimpleInfo[] GetGuildRequests()
        {
            return joinSubmitGuildList.ToArray();
        }

        /// <summary>
        /// 가입 신청한 길드 수
        /// </summary>
        public int GuildJoinSubmitCount => joinSubmitGuildList.Count;

        /// <summary>
        /// 길드 가입 신청한 유저 목록 반환
        /// </summary>
        public GuildJoinSubmitInfo[] GetGuildJoinSubmitInfos()
        {
            return joinSubmitUserList.ToArray();
        }

        public int GuildJoinSubmitUserCount => joinSubmitUserList.Count;

        /// <summary>
        /// 길드원 목록 반환
        /// </summary>
        public GuildMemberInfo[] GetGuildMemberInfos()
        {
            return memberList.ToArray();
        }

        /// <summary>
        /// 길드 스킬 목록
        /// </summary>
        public GuildSkill[] GetGuildSkillInfos()
        {
            // 엠펠리움 버프 스킬 목록에서 제외
            int emperiumBuffID = BasisType.GUILD_ATTACK_EMPERIUM_BUFF_SKILL_ID.GetInt();
            return guildSkillList.Where(x => x.SkillId != emperiumBuffID).ToArray();
        }

        /// <summary>
        /// 길드 게시판 목록
        /// </summary>
        public GuildBoardInfo[] GetGuildBoardInfos()
        {
            return guildBoardList.Values.ToArray();
        }

        /// <summary>
        /// 장착한 모든 스킬 반환
        /// </summary>
        public SkillInfo[] GetValidSkills(EquipmentClassType weaponType)
        {
            for (int i = 0; i < guildSkillList.Count; i++)
            {
                if (guildSkillList[i].IsInPossession && guildSkillList[i].IsValid(weaponType))
                    skillBuffer.Add(guildSkillList[i]);
            }

            return skillBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// exp에 해당하는 길드 레벨 반환
        /// </summary>
        public int GetGuildLevel(int totalExp)
        {
            var arraykey = BasisType.GUILD_LEVEL_UP_EXP.GetKeyList();
            for (int i = arraykey.Count - 1; i >= 0; i--)
            {
                var exp = BasisType.GUILD_LEVEL_UP_EXP.GetInt(arraykey[i]);
                if (totalExp >= exp)
                    return arraykey[i];
            }

            return 1;
        }

        /// <summary>
        /// [길드전] 엠펠리움 최대 Hp 반환
        /// </summary>
        public int GetMaxEmperiumHp()
        {
            if (maxEmperiumHp == 0)
                maxEmperiumHp = BasisGuildWarInfo.EmperiumMaxHp.GetInt();

            return maxEmperiumHp;
        }

        /// <summary>
        /// [길드전] 포링포탑 왼쪽 장착된 큐펫 Ids
        /// </summary>
        public int[] GetLeftTurretCupets()
        {
            return leftCupetIds.ToArray();
        }

        /// <summary>
        /// [길드전] 포링포탑 오른쪽 장착된 큐펫 Ids
        /// </summary>
        public int[] GetRightTurretCupets()
        {
            return rightCupetIds.ToArray();
        }

        /// <summary>
        /// 길드전 목록 반환
        /// </summary>
        public GuildBattleOpponentPacket[] GetGuildBattleOpponents()
        {
            return guildBattleOpponents.ToArray();
        }

        // TODO: Initialize를 통해 설정할 수 있도록 수정
        public void SetGuildName(string guildName)
        {
            this.guildName = guildName;
        }

        /// <summary>
        /// 가입된 길드가 있는지 체크
        /// </summary>
        private bool CheckJoinGuild()
        {
            if (HaveGuild)
            {
                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90067.ToText(); // 이미 가입되어 있는 길드가 있습니다.
                UI.ConfirmPopup(title, description);
            }
            return HaveGuild;
        }

        /// <summary>
        /// 길드 추방 체크
        /// </summary>
        private bool CheckKickGuild()
        {
            if (!HaveGuild)
            {
                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90068.ToText(); // 가입되어 있는 길드가 없습니다.
                UI.ConfirmPopup(title, description);
            }
            return !HaveGuild;
        }

        /// <summary>
        /// 선택한 길드원 정보 세팅
        /// </summary>
        public void SetSelectGuildMeberInfo(GuildMemberInfo info)
        {
            SelectGuildMemberInfo = info;
        }

        public IEnumerable<DateTime> GetTamingOpenTimes()
        {
            return tamingMazeTimeTable;
        }

        /// <summary>
        /// [길드전] 버프 정보 업데이트
        /// </summary>
        private void UpdateGuildBattleBuffInfo(GuildBattleBuffPacket[] packetArray)
        {
            foreach (var packet in packetArray)
            {
                if (GuildBattleBuffDic.ContainsKey(packet.SkillId))
                {
                    GuildBattleBuffDic[packet.SkillId] = packet;
                }
                else
                {
                    GuildBattleBuffDic.Add(packet.SkillId, packet);
                }
            }
        }

        /// <summary>
        /// [길드전] 버프 총 경헙치
        /// </summary>
        public int GetGuildBuffExp(int skillId)
        {
            if (GuildBattleBuffDic.ContainsKey(skillId))
                return GuildBattleBuffDic[skillId].TotalExp;

            return 0;
        }

        /// <summary>
        /// [길드전] 버프 레벨 반환
        /// </summary>
        public int GetGuildBattleBuffLevel(int totalExp)
        {
            int maxLevel = BasisGuildWarInfo.BuffSkillMaxLevel.GetInt();
            int levelUpExp = BasisGuildWarInfo.BuffNeedLevelUpExp.GetInt();
            return Mathf.Min(totalExp / levelUpExp, maxLevel);
        }

        /// <summary>
        /// 길드 가입 여부
        /// </summary>
        public bool HasGuild(bool isShowPopup)
        {
            if (HaveGuild)
                return true;

            if (isShowPopup)
            {
                string description = LocalizeKey._90068.ToText(); // 가입되어 있는 길드가 없습니다.
                UI.ShowToastPopup(description);
            }

            return false;
        }

        #region 프로토콜

        /// <summary>
        /// 길드 생성
        /// </summary>
        public async Task RequestGuildCreate(string guildName, string guildIntroduction)
        {
            if (CheckJoinGuild())
                return;

            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90055.ToText() // 길드 [{GUILD_NAME}] 생성 하시겠습니까?
                .Replace("{GUILD_NAME}", guildName);

            if (!await UI.CostPopup(CoinType.Zeny, BasisType.GUILD_CREATE_ZENY.GetInt(), title, description))
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", guildName);
            sfs.PutByte("2", SelectEmblemBg);
            sfs.PutByte("3", SelectEmblemFrame);
            sfs.PutByte("4", SelectEmblemIcon);
            sfs.PutUtfString("5", guildIntroduction);

            var response = await Protocol.GUILD_CREATE.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                guildId = response.GetInt("1");
                this.guildName = guildName;
                this.guildIntroduction = guildIntroduction;
                emblemBg = SelectEmblemBg;
                emblemframe = SelectEmblemFrame;
                emblemIcon = SelectEmblemIcon;

                emblemId = MathUtils.GetValueFromBitField(6, 31, emblemBg, emblemframe, emblemIcon);

                UI.ShortCut<UIGuildMain>();

                //vivoxRepo.LogIn(CID.ToString(), Name, GuildId);
                OnUpdateGuildState?.Invoke();
                OnHaveGuild?.Invoke(HaveGuild);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 추천 목록
        /// </summary>
        public async Task RequestGuildRandom()
        {
            var response = await Protocol.REQUEST_GUILD_RANDOM.SendAsync();
            if (response.isSuccess)
            {
                if (response.ContainsKey("1"))
                {
                    recommendGuildList.Clear();

                    var array = response.GetPacketArray<GuildSimplePacket>("1");
                    foreach (var item in array)
                    {
                        var info = new GuildSimpleInfo();
                        info.Initialize(item);
                        info.SetSubmitJoin(joinSubmitGuildList.FirstOrDefault(x => x.GuildId == item.guild_id) != null);
                        recommendGuildList.Add(info);
                    }
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 가입 신청한 목록 요청
        /// </summary>
        public async Task RequestJoinSubmitGuildList()
        {
            // 신청 목록 초기화
            joinSubmitGuildList.Clear();

            if (HaveGuild)
                return;

            var response = await Protocol.REQUEST_GUILD_JOIN_LIST.SendAsync();
            if (response.isSuccess)
            {
                if (response.ContainsKey("1"))
                {
                    joinSubmitGuildList.Clear();
                    var array = response.GetPacketArray<GuildRequestSimplePacket>("1");

                    foreach (var item in array.OrderBy(x => x.seq))
                    {
                        var info = new GuildSimpleInfo();
                        info.Initialize(item);
                        joinSubmitGuildList.Add(info);
                    }
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 가입 신청
        /// </summary>
        public async Task<bool> RequestGuildJoin(GuildSimpleInfo info)
        {
            if (CheckJoinGuild())
                return false;

            string title = LocalizeKey._5.ToText(); // 알람
            string description;

            // 가입신청 최대치
            if (!info.IsAutoJoin && GuildJoinSubmitCount == BasisType.GUILD_REQUEST_LIMIT.GetInt())
            {
                description = LocalizeKey._90079.ToText(); // 길드 신청 목록이 초과하였습니다.
                UI.ConfirmPopup(title, description);
                return false;
            }

            // 이미 가입신청한 길드
            if (info.IsSubmitJoin)
                return false;

            // 길드원 최대치
            if (info.IsMaxMember)
            {
                description = LocalizeKey._90080.ToText(); // 길드의 정원이 가득 찼습니다.
                UI.ConfirmPopup(title, description);
                return false;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.GuildId);

            var response = await Protocol.REQUEST_GUILD_JOIN.SendAsync(sfs);
            if (response.isSuccess)
            {
                if (info.IsAutoJoin)
                {
                    joinSubmitGuildList.Clear();
                    return true;
                }

                info.SetSubmitJoin(true);
                joinSubmitGuildList.Add(info);
                return true;
            }
            else if (response.resultCode == ResultCode.ALREADY_EXISTS)
            {
                UI.ConfirmPopup(LocalizeKey._80000.ToText()); // 이미 가입 신청한 길드입니다.
            }
            else if (response.resultCode == ResultCode.NOT_ENOUGHT_COUNT)
            {
                UI.ConfirmPopup(LocalizeKey._80001.ToText()); // 더 이상 가입 신청을 할 수 없습니다.
            }
            else if (response.resultCode == ResultCode.NOT_EXISTS)
            {
                UI.ConfirmPopup(LocalizeKey._80002.ToText()); // 없는 길드입니다.
            }
            else if (response.resultCode == ResultCode.LEVEL_CUT)
            {
                UI.ConfirmPopup(LocalizeKey._80004.ToText()); // 길드가입 신청 레벨이 부족합니다.
            }
            else if (response.resultCode == ResultCode.ALREADY_GUILD_MEMBER)
            {
                UI.ConfirmPopup(LocalizeKey._80005.ToText()); // 이미 소속된 길드가 있습니다.
            }
            else if (response.resultCode == ResultCode.GUILD_MEMBER_IS_FULL)
            {
                UI.ConfirmPopup(LocalizeKey._80006.ToText()); // 길드의 정원이 가득 찼습니다.
            }
            else if (response.resultCode == ResultCode.GUILD_JOIN_COOL)
            {
                UI.ConfirmPopup(LocalizeKey._80003.ToText()); // 길드 재가입까지 남은 시간이 있습니다.
            }
            else
            {
                response.ShowResultCode();
            }
            return false;
        }

        /// <summary>
        /// 길드 검색
        /// </summary>
        public async Task RequestGuildSearch(string guildName)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", guildName);

            var response = await Protocol.REQUEST_GUILD_SEARCH.SendAsync(sfs);
            if (response.isSuccess)
            {
                recommendGuildList.Clear();

                if (response.ContainsKey("1"))
                {
                    var packet = response.GetPacket<GuildSimplePacket>("1");
                    var info = new GuildSimpleInfo();
                    info.Initialize(packet);
                    info.SetSubmitJoin(joinSubmitGuildList.FirstOrDefault(x => x.GuildId == packet.guild_id) != null);
                    recommendGuildList.Add(info);
                }
            }
            else if (response.resultCode == ResultCode.SHORT_STRING)
            {
                UI.ConfirmPopup(LocalizeKey._80010.ToText()); // 길드이름은 최소2자 이상입니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 가입 신청 취소
        /// </summary>
        public async Task RequestGuildJoinCancel(GuildSimpleInfo info)
        {
            if (CheckJoinGuild())
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.Seq);
            sfs.PutInt("2", info.GuildId);

            var response = await Protocol.REQUEST_CANCEL_GUILD_JOIN.SendAsync(sfs);
            if (response.isSuccess)
            {
                var recommend = recommendGuildList.FirstOrDefault(x => x.GuildId == info.GuildId);
                if (recommend != null)
                    recommend.SetSubmitJoin(false);

                joinSubmitGuildList.Remove(info);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 내 길드 정보 요청
        /// </summary>
        public async Task RequestMyGuildInfo(bool isForce = false)
        {
            if (!HaveGuild && !isForce)
                return;

            var response = await Protocol.GUILD_MY.SendAsync();
            if (response.isSuccess)
            {
                var packet = response.GetPacket<GuildInfoPacket>("1");
                guildId = packet.guild_id;
                guildName = packet.name;
                emblemBg = MathUtils.GetBitFieldValue(packet.emblem, 0); // 32를 넘어가면 안된다
                emblemframe = MathUtils.GetBitFieldValue(packet.emblem, 6); // 32를 넘어가면 안된다
                emblemIcon = MathUtils.GetBitFieldValue(packet.emblem, 12); // 32를 넘어가면 안된다
                masterName = packet.master_name;
                memberCount = packet.member_count;
                maxMemberCount = packet.max_member_count;
                level = packet.level;
                notice = packet.guild_notice;
                expPoint = packet.exppoint;
                guildIntroduction = packet.introduction;
                isAutoJoin = packet.is_auto_join;
                masterUid = packet.master_uid;
                masterCid = packet.master_cid;
                FreeGuildNameChangeCount = packet.freeGuildNameChangeCount;

                guildPosition = response.GetByte("2");
                guildDonatePoint = response.GetInt("4");
                isTodayJoin = response.GetByte("11") == 1;
                canMasterDismissal = response.GetBool("12");
                guildOutRemainTime = response.GetInt("13");

                OnUpdateGuildInfo?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 출석 현황
        /// </summary>
        public async Task RequestAttendInfo()
        {
            if (!HaveGuild)
                return;

            var response = await Protocol.REQUEST_GUILD_ATTEND_INFO.SendAsync();
            if (response.isSuccess)
            {
                yesterDayMemberCount = response.GetInt("1");
                todayMemberCount = response.GetInt("2");
                isTodayAttend = response.GetBool("3");
                isGetAttendReward = response.GetBool("4");
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 가입 신청한 유저 목록 요청
        /// 최대 30건
        /// </summary>
        public async Task RequestJoinSubmitUserList()
        {
            if (!HaveGuild)
                return;

            if (GuildPosition == GuildPosition.Member)
                return;

            var response = await Protocol.REQUEST_GUILD_LIST.SendAsync();
            if (response.isSuccess)
            {
                joinSubmitUserList.Clear();
                if (response.ContainsKey("1"))
                {
                    var array = response.GetPacketArray<GuildJoinSubmitPacket>("1");
                    foreach (var item in array.OrderBy(x => x.seq))
                    {
                        var info = new GuildJoinSubmitInfo();
                        info.Initialize(item);
                        joinSubmitUserList.Add(info);
                    }
                }
            }
            else if (response.resultCode == ResultCode.PERMISSION_FAIL)
            {
                UI.ConfirmPopup(LocalizeKey._80013.ToText()); // 권한이 없습니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드원 목록 요청
        /// </summary>
        /// <param name="page">0일경우 모든 길드원 리스트 (페이지당 6개)</param>
        /// <param name="isMemberCount">전체 맴버수,접속중 맴버수 요청</param>
        /// <param name="isConnectUse">접속중인 길드원 리스트만 받을경우</param>
        /// <returns></returns>
        public async Task RequestGuildMemberList(int page = 0, bool isMemberCount = true, bool isConnectUse = false)
        {
            if (!HaveGuild)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", page);
            sfs.PutBool("2", isMemberCount);
            sfs.PutBool("3", isConnectUse);

            var response = await Protocol.REQUEST_GUILD_MEMBER.SendAsync(sfs);
            if (response.isSuccess)
            {
                memberList.Clear();

                if (response.ContainsKey("1"))
                {
                    var array = response.GetPacketArray<GuildMemberPacket>("1");
                    foreach (var item in array)
                    {
                        var info = new GuildMemberInfo();
                        info.Initialize(item);
                        info.Initialize(profileDataRepo);
                        memberList.Add(info);
                    }
                }

                if (response.ContainsKey("3"))
                    memberCount = response.GetByte("3");

                if (response.ContainsKey("4"))
                    onlineMemberCount = response.GetByte("4");

                OnUpdateGuildMember?.Invoke();
            }
            else if (response.resultCode == ResultCode.CHAR_GUILD_NOT_FOUND)
            {
                UI.ConfirmPopup(LocalizeKey._80012.ToText()); // 가입되어 있는 길드가 없습니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 테이밍 미로 정보 요청
        /// </summary>
        public async Task<bool> RequestTamingMazeInfo()
        {
            var response = await Protocol.REQUEST_TAMING_SEASON_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            var sfsObject = response.GetSFSObject("1");
            taming_id = sfsObject.GetInt("1");
            isTamingMazeInProgress = sfsObject.GetBool("2");
            tamingMazeRemainTime = sfsObject.GetLong("3");

            var tamingData = TamingDataManager.Instance.Get(taming_id);
            if (tamingData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"TamingData가 없다. {nameof(taming_id)} = {taming_id}");
#endif
                return false;
            }

            tamingMazeSeason = tamingData.rotation_value;

            return true;
        }

        /// <summary>
        /// 길드 테이밍 미로 정보 초기화
        /// </summary>
        public void InitializeTamingMazeInfo(TamingDungeonInfoPacket packet)
        {
            taming_id = packet.tamingId;
            isTamingMazeInProgress = packet.isOpen;
            tamingMazeRemainTime = packet.remainTime;
            var tamingData = TamingDataManager.Instance.Get(taming_id);
            if (tamingData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"TamingData가 없다. {nameof(taming_id)} = {taming_id}");
#endif
                return;
            }

            tamingMazeSeason = tamingData.rotation_value;
            HasTamingMazeNotice = isTamingMazeInProgress;
        }

        /// <summary>
        /// 길드 가입 신청 처리
        /// </summary>
        public async Task RequestGuildJoinSumitUserProc(GuildJoinSubmitInfo info, byte isAccept)
        {
            if (!HaveGuild)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.Seq);
            sfs.PutByte("2", isAccept);

            var response = await Protocol.REQUEST_GUILD_PROC.SendAsync(sfs);
            if (response.isSuccess)
            {
                joinSubmitUserList.Remove(info);
            }
            else if (response.resultCode == ResultCode.NOT_EXISTS)
            {
                UI.ConfirmPopup(LocalizeKey._80014.ToText()); // 길드 가입요청이 존재하지 않습니다.
            }
            else if (response.resultCode == ResultCode.NOT_EXISTS_GUILD_JOIN_REQUEST)
            {
                UI.ConfirmPopup(LocalizeKey._80014.ToText()); // 길드 가입요청이 존재하지 않습니다.
            }
            else if (response.resultCode == ResultCode.ALREADY_GUILD_MEMBER)
            {
                UI.ConfirmPopup(LocalizeKey._80005.ToText()); // 이미 가입되어 있는 길드가 있습니다.
            }
            else if (response.resultCode == ResultCode.GUILD_MEMBER_IS_FULL)
            {
                UI.ConfirmPopup(LocalizeKey._80006.ToText()); // 길드의 정원이 가득 찼습니다.
            }
            else if (response.resultCode == ResultCode.PERMISSION_FAIL)
            {
                UI.ConfirmPopup(LocalizeKey._80013.ToText()); // 권한이 없습니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 소개글 변경
        /// </summary>
        public async Task RequestChangeGuildIntroduction(string introduction)
        {
            if (!HaveGuild)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", introduction);

            var response = await Protocol.REQUEST_GUILD_CAHNGE_INTRO.SendAsync(sfs);
            if (response.isSuccess)
            {
                guildIntroduction = introduction;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 출석 체크
        /// </summary>
        public async Task RequestCheckAttend()
        {
            if (CheckKickGuild())
                return;

            var response = await Protocol.REQUEST_GUILD_USER_ATTEND.SendAsync();
            if (response.isSuccess)
            {
                todayMemberCount += 1;
                isTodayAttend = true;
                expPoint += BasisType.GUILD_ATTEND_GUILD_POINT.GetInt(1);
                level = GetGuildLevel(expPoint);

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }
            }
            else if (response.resultCode == ResultCode.CHAR_GUILD_NOT_FOUND)
            {
                UI.ConfirmPopup(LocalizeKey._80012.ToText()); // 가입되어 있는 길드가 없습니다.
            }
            else if (response.resultCode == ResultCode.LEFT_COOLTIME)
            {
                UI.ConfirmPopup(LocalizeKey._80017.ToText()); // 오늘은 이미 길드 출석을 했습니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 출석 보상 받기
        /// </summary>
        public async Task RequestAttendReward()
        {
            if (CheckKickGuild())
                return;

            if (isTodayJoin)
            {
                string message = LocalizeKey._90241.ToText(); // 길드를 가입한 날에는 출석 보상을 받을 수 없습니다.
                UI.ShowToastPopup(message);
                return;
            }

            var response = await Protocol.REQUEST_GUILD_ATTEND_REWARD.SendAsync();
            if (response.isSuccess)
            {
                isGetAttendReward = true;

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }
            }
            else if (response.resultCode == ResultCode.LEFT_COOLTIME)
            {
                string message = LocalizeKey._90241.ToText(); // 길드를 가입한 날에는 출석 보상을 받을 수 없습니다.
                UI.ShowToastPopup(message);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 탈퇴
        /// </summary>
        public async Task RequestGuildOut()
        {
            if (CheckKickGuild())
                return;

            string title = LocalizeKey._5.ToText(); // 알람
            string description = string.Empty;
            if (GuildPosition == GuildPosition.Master && MemberCount > 1)
            {
                // 길드 마스터는 권한 위임 후 탈퇴 할 수 있습니다
                description = LocalizeKey._90061.ToText(); // 길드장은 권한 위임 후 탈퇴할 수 있습니다.
                UI.ConfirmPopup(title, description);
                return;
            }

            TimeSpan span = TimeSpan.FromMilliseconds(BasisType.GUILD_REJOIN_TIME.GetInt());
            description = LocalizeKey._90062.ToText(); // 길드탈퇴시 {TIME}시간 동안 새로운 길드가입 및 생성이 불가능합니다.\n정말로 탈퇴하시겠습니까?
            if (!await UI.SelectPopup(title, description.Replace(ReplaceKey.TIME, span.TotalHours.ToString("N0"))))
                return;

            var response = await Protocol.REQUEST_GUILD_OUT.SendAsync();
            if (response.isSuccess)
            {
                Reset();

                RejoinTime = BasisType.GUILD_REJOIN_TIME.GetInt();
                UI.Close<UIGuildMain>();

                OnUpdateGuildSkill?.Invoke();
                OnUpdateGuildState?.Invoke();
                OnHaveGuild?.Invoke(HaveGuild);
            }
            else if (response.resultCode == ResultCode.CHAR_GUILD_NOT_FOUND)
            {
                UI.ConfirmPopup(LocalizeKey._80012.ToText()); // 가입되어 있는 길드가 없습니다.
            }
            else if (response.resultCode == ResultCode.PERMISSION_FAIL)
            {
                UI.ConfirmPopup(LocalizeKey._80013.ToText()); // 권한이 없습니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 스킬 경험치 구입
        /// </summary>
        /// <param name="info">구입하려는 길드스킬</param>
        /// <param name="costType">재화타입</param>
        public async Task RequestGuildSkillBuyExp(GuildSkill info, byte costType)
        {
            if (CheckKickGuild())
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", costType);
            sfs.PutInt("2", info.SkillId);

            var response = await Protocol.REQUEST_GUILD_BUY_EXP.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    UI.RewardInfo(charUpdateData.rewards); // 획득 보상 보여줌 (UI)
                }

                info.PlusExp(costType);
                if (costType == 1)
                {
                    guildSkillBuyTime = BasisType.GUILD_SKILL_BUY_EXP_COIN_COOLTIME.GetInt();
                }
                else if (costType == 2)
                {
                    guildSkillBuyCount += 1;
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드스킬 레벨업
        /// </summary>
        public async Task RequestSkillLevelUp(GuildSkill info)
        {
            if (CheckKickGuild())
                return;

            string title = LocalizeKey._5.ToText(); // 알람
            string description = string.Empty;
            // 길드 스킬 레벨업은 길드장, 부길드장만 가능
            if (GuildPosition != GuildPosition.Master)
            {
                description = LocalizeKey._90065.ToText(); // 스킬레벨업은 길드장만 가능합니다. 
                UI.ConfirmPopup(title, description);
                return;
            }

            // 길드 스킬레벨을 올리기 위해 필요한 길드레벨
            int needGuildLevel = info.NeedGuildLevel;
            if (GuildLevel < needGuildLevel)
            {
                // 길드 레벨이 부족합니다.
                description = LocalizeKey._90066.ToText() // 길드레벨 {LEVEL}부터 스킬레벨업이 가능합니다.
                    .Replace("{LEVEL}", needGuildLevel.ToString());
                UI.ConfirmPopup(title, description);
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.SkillId);

            var response = await Protocol.REQUEST_GUILD_LEVELUP.SendAsync(sfs);
            if (response.isSuccess)
            {
                description = LocalizeKey._90060.ToText() // 길드스킬 [{NAME}]이 Lv.{LEVEL}로 변경 되었습니다.
                    .Replace("{NAME}", info.SkillName)
                    .Replace("{LEVEL}", info.NextSkillLevel().ToString());
                UI.ShowToastPopup(description);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 스킬 리스트
        /// </summary>
        public async Task RequestGuildSkillList(bool isLevelUp)
        {
            if (!HaveGuild)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", isLevelUp);

            var response = await Protocol.REQUEST_GUILD_SKILL_LIST.SendAsync(sfs);
            if (response.isSuccess)
            {
                if (response.ContainsKey("1"))
                {
                    var array = response.GetPacketArray<GuildSkillPacket>("1");
                    Initialize(array);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 공지사항 변경
        /// </summary>
        public async Task RequestChangeGuildNotice(string notice)
        {
            if (!HaveGuild)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", notice);

            var response = await Protocol.REQUEST_GUILD_CHANGE_NOTICE.SendAsync(sfs);
            if (response.isSuccess)
            {
                this.notice = notice;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 게시판 목록
        /// </summary>
        public async Task RequestGuildBoardList(int page = 1)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", page);

            var response = await Protocol.REQUEST_GUILD_BOARD_LIST.SendAsync(sfs);
            if (response.isSuccess)
            {
                curPage = page;
                if (curPage == 1)
                {
                    guildBoardList.Clear();
                }

                if (response.ContainsKey("1"))
                {
                    var array = response.GetPacketArray<GuildBoardPacket>("1");
                    foreach (var item in array)
                    {
                        GuildBoardInfo info = new GuildBoardInfo();
                        info.Initialize(item);
                        guildBoardList.Add(info.Seq, info);
                    }
                }

                if (response.ContainsKey("2"))
                {
                    nextPage = page + 1;
                }
                else
                {
                    nextPage = null;
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 게시판 다음 페이지
        /// </summary>
        public async Task RequestNextBoardList()
        {
            if (!nextPage.HasValue)
                return;

            await RequestGuildBoardList(nextPage.Value);
        }

        /// <summary>
        /// 길드 엠블렘 변경
        /// </summary>
        public async Task RequestChangeEmblem()
        {
            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90074.ToText(); // 길드 엠블렘을 변경하시겠습니까?

            if (!await UI.CostPopup(CoinType.CatCoin, BasisType.GUILD_MARK_CHANGE_CAT_COIN.GetInt(), title, description))
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", SelectEmblemBg);
            sfs.PutByte("2", SelectEmblemFrame);
            sfs.PutByte("3", SelectEmblemIcon);

            var response = await Protocol.REQUEST_GUILD_CHANGE_EMBLEM.SendAsync(sfs);
            if (response.isSuccess)
            {
                UI.Close<UIGuildEmblem>();

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                emblemBg = SelectEmblemBg;
                emblemframe = SelectEmblemFrame;
                emblemIcon = SelectEmblemIcon;
                emblemId = MathUtils.GetValueFromBitField(6, 31, emblemBg, emblemframe, emblemIcon);

                OnUpdateGuildEmblem?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 부길드장 임명/해임
        /// </summary>
        public async Task RequestGuildGrantPartMaster(GuildMemberInfo info)
        {
            var sfs = Protocol.NewInstance();
            if (info.GuildPosition == GuildPosition.Member)
            {
                sfs.PutByte("1", 1);
            }
            else
            {
                sfs.PutByte("1", 0);
            }
            sfs.PutInt("2", info.UID);
            sfs.PutInt("3", info.CID);

            var response = await Protocol.REQUEST_GUILD_GRANT_PART_MASTER.SendAsync(sfs);
            if (response.isSuccess)
            {
                if (info.GuildPosition == GuildPosition.Member)
                {
                    info.SetGuildPosition(2); // 부길드장
                    string message = LocalizeKey._90056.ToText() // {NAME}님이 부길드장 직위로 변경 되었습니다.
                        .Replace(ReplaceKey.NAME, info.Name);
                    UI.ShowToastPopup(message);
                }
                else
                {
                    info.SetGuildPosition(1); // 일반길드원
                    string message = LocalizeKey._90229.ToText() // {NAME}님이 일반길드원 직위로 변경 되었습니다.
                        .Replace(ReplaceKey.NAME, info.Name);
                    UI.ShowToastPopup(message);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 마스터 위임
        /// </summary>
        public async Task RequestGuildMasterChange(GuildMemberInfo info)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.UID);
            sfs.PutInt("2", info.CID);

            var response = await Protocol.REQUEST_GUILD_MASTER_CHANGE.SendAsync(sfs);
            if (response.isSuccess)
            {
                info.SetGuildPosition(3);

                string message = LocalizeKey._90057.ToText() // {NAME}님이 길드장 직위로 변경 되었습니다.
                    .Replace(ReplaceKey.NAME, info.Name);
                UI.ShowToastPopup(message);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드원 추방
        /// </summary>
        public async Task RequestGuildKick(GuildMemberInfo info)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.UID);
            sfs.PutInt("2", info.CID);

            var response = await Protocol.REQUEST_GUILD_MEMBER_KICK.SendAsync(sfs);
            if (response.isSuccess)
            {
                OnUpdateGuildMemberCount?.Invoke();
            }
            else if (response.resultCode == ResultCode.NOT_EXISTS)
            {
                UI.ConfirmPopup(LocalizeKey._80015.ToText()); // 소속된 길드원이 아닙니다.
            }
            else if (response.resultCode == ResultCode.PERMISSION_FAIL)
            {
                UI.ConfirmPopup(LocalizeKey._80013.ToText()); // 권한이 없습니다.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 게시판 글쓰기
        /// </summary>
        public async Task<bool> RequestWriteGuildBoard(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            if (message.Length > 64)
                return false;

            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", message);

            var response = await Protocol.REQUEST_GUILD_BOARD_WRITE.SendAsync(sfs);
            if (response.isSuccess)
            {
                OnUpdateGuildBoard?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// 게시판 게시글 삭제
        /// </summary>
        public async Task RequestDeleteGuildBoard(int seq)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", seq);

            var response = await Protocol.REQUEST_GUILD_BOARD_DELETE.SendAsync(sfs);
            if (response.isSuccess)
            {

            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 관리 
        /// 0 : 신청가입, 1: 즉시가입
        /// </summary>
        public async Task RequestGuildAutoJoinUpdate(byte isAutoJoin)
        {
            if (this.isAutoJoin == isAutoJoin)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", isAutoJoin);

            var response = await Protocol.REQUEST_GUILD_AUTO_JOIN_UPDATE.SendAsync(sfs);
            if (response.isSuccess)
            {
                this.isAutoJoin = isAutoJoin;
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 길드 마스터권한 가져오기
        /// </summary>
        public async Task<bool> RequestGuildMasterGet()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", masterUid);
            sfs.PutInt("2", masterCid);
            var response = await Protocol.REQUEST_GUILD_MASTER_GET.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            string message = LocalizeKey._33123.ToText(); // 길드장으로 변경되었습니다.
            UI.ShowToastPopup(message);
            return true;
        }

        /// <summary>
        /// 길드습격 부활
        /// </summary>
        public async Task RequestRebirthGuildAttack()
        {
            Response response = await Protocol.REQUEST_GA_USER_REVIVE.SendAsync();
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

            // 성공 시 RECEIVE_GA_PLAYER_APPEAR 프로토콜
        }

        /// <summary>
        /// 길드습격 포션 사용
        /// </summary>
        public async Task RequestUseGuildAttackPotion()
        {
            Response response = await Protocol.REQUEST_GA_USE_PORTION.SendAsync();
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

            OnUseGuildAttackPotion?.Invoke();
        }

        /// <summary>
        /// 길드습격 시간 변경 (길드 마스터만 사용가능)
        /// </summary>
        public async Task RequestChangeGuildAttackTime()
        {
            Response response = await Protocol.REQUEST_GA_CHANGE_TIME.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }
        }

        /// <summary>
        /// 길드습격 기부
        /// </summary>
        public async Task RequestDonationGuildAttack()
        {
            Response response = await Protocol.REQUEST_GA_EMPERIUM_CONTRIBUTION.SendAsync();
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

            Quest.QuestProgress(QuestType.GUILD_ATTACK_DONATION_COUNT); // 길드 습격 기부 횟수
        }

        /// <summary>
        /// 길드습격 엠펠리움 생성
        /// </summary>
        public async Task RequestCreateEmperium()
        {
            Response response = await Protocol.REQUEST_GA_MAKEEMPAL.SendAsync();
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

        /// <summary>
        /// 길드명 변경 요청
        /// </summary>
        public async Task RequestEditGuildName(string guildName)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", guildName); // 변경할 길드명

            Response response = await Protocol.REQUEST_GUILD_NAME_CHANGE.SendAsync(sfs);
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

            FreeGuildNameChangeCount = Mathf.Max(0, FreeGuildNameChangeCount - 1);
            this.guildName = guildName;
            OnUpdateGuildName?.Invoke();
        }

        /// <summary>
        /// 길드전 시즌 정보
        /// </summary>
        public async Task RequestGuildBattleSeasonInfo()
        {
            if (!HaveGuild)
            {
                UI.ShowToastPopup(LocalizeKey._90068.ToText()); // 가입되어 있는 길드가 없습니다.
                return;
            }

            Response response = await Protocol.REQUEST_GUILD_BATTLE_SEASON_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            GuildBattleSeasonType = response.GetByte("1").ToEnum<GuildBattleSeasonType>();
            GuildBattleSeasonRemainTime = response.GetLong("2");

            // 길드전 참가 신청 시즌일때 정보
            if (response.ContainsKey("3"))
            {
                var sfs = response.GetSFSObject("3");
                IsGuildBattleRequest = sfs.GetByte("1") == 1; // 참가신청을 했는지 flag -- 0:아직 신청전, 1:신청완료

                // 좌측 포탑 큐펫 목록
                leftCupetIds.Clear();
                if (sfs.ContainsKey("2"))
                {
                    var leftCupetArray = sfs.GetSFSArray("2");
                    for (int i = 0; i < leftCupetArray.Count; i++)
                    {
                        var cupet = leftCupetArray.GetSFSObject(i);
                        leftCupetIds.Add(cupet.GetInt("2"));
                    }
                }

                // 우측 포탑 큐펫 목록
                rightCupetIds.Clear();
                if (sfs.ContainsKey("3"))
                {
                    var rightCupetArray = sfs.GetSFSArray("3");
                    for (int i = 0; i < rightCupetArray.Count; i++)
                    {
                        var cupet = rightCupetArray.GetSFSObject(i);
                        rightCupetIds.Add(cupet.GetInt("2"));
                    }
                }
            }

            // 길드전 진행중일때 정보
            if (response.ContainsKey("4"))
            {
                Response sfs = new Response(response.GetSFSObject("4"));

                GuildBattleOpponentPacket[] guildBattlePackets = sfs.ContainsKey("1") ? sfs.GetPacketArray<GuildBattleOpponentPacket>("1") : Array.Empty<GuildBattleOpponentPacket>();
                SetBattleGuildOpponents(guildBattlePackets);

                GuildBattleAccrueDamage = sfs.GetLong("2");
                GuildBattleEnterRemainCount = sfs.GetInt("3");

                UsedGuildBattleSupportAgents.Clear();
                var agentArray = sfs.GetIntArray("4");
                foreach (var item in agentArray)
                {
                    UsedGuildBattleSupportAgents.Add(item);
                }

                UsedGuildBattleDefenseTurretCupetIds.Clear();
                var cupetArray = sfs.GetIntArray("5");
                foreach (var item in cupetArray)
                {
                    UsedGuildBattleDefenseTurretCupetIds.Add(item);
                }
            }

            if (GuildBattleSeasonType == GuildBattleSeasonType.Ready)
            {
                UI.Show<UIGuildBattleReady>();
            }
            else if (GuildBattleSeasonType == GuildBattleSeasonType.InProgress)
            {
                UI.Show<UIGuildBattleEnter>();
            }
            else
            {
                var timeSpan = GuildBattleSeasonRemainTime.ToRemainTime().ToTimeSpan();
                string message = LocalizeKey._33820.ToText() // 길드전이 종료되었습니다.\n({MINUTES}분 {SECONDS}초 후에 정산이 완료됩니다.)
                    .Replace(ReplaceKey.MINUTES, timeSpan.TotalMinutes.ToString("00"))
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds.ToString("00"));
                UI.ConfirmPopup(message);
            }
        }

        /// <summary>
        /// [길드전] 신청
        /// </summary>
        public async Task RequestGuildBattleEntry(int[] leftCupetIds, int[] rightCupetIds)
        {
            int leftCupetLength = leftCupetIds == null ? 0 : leftCupetIds.Length;
            int rightCupetLength = rightCupetIds == null ? 0 : rightCupetIds.Length;

            if (GuildPosition == GuildPosition.Member || GuildPosition == GuildPosition.None)
            {
                UI.ShowToastPopup(LocalizeKey._90312.ToText()); // 길드전 신청은 길드장, 부길드장만 가능합니다.
                return;
            }

            if (leftCupetLength == 0 || rightCupetLength == 0)
            {
                UI.ShowToastPopup(LocalizeKey._90311.ToText()); // 각각의 포탑은 1개 이상의 큐펫이 필요합니다.
                return;
            }

            // 방어 포탑 기존 세팅과 같은지 체크
            bool isDuplicate = this.leftCupetIds.size == leftCupetLength && this.rightCupetIds.size == rightCupetLength;

            if (isDuplicate)
            {
                for (int i = 0; i < leftCupetLength; i++)
                {
                    if (this.leftCupetIds[i] != leftCupetIds[i])
                    {
                        isDuplicate = false;
                        break;
                    }
                }

                if (isDuplicate)
                {
                    for (int i = 0; i < rightCupetLength; i++)
                    {
                        if (this.rightCupetIds[i] != rightCupetIds[i])
                        {
                            isDuplicate = false;
                            break;
                        }
                    }
                }
            }

            // 기존 데이터와 동일
            if (isDuplicate)
            {
                Debug.Log($"방어포탑 기존 세팅과 동일하다.");
                return;
            }

            var sfs = Protocol.NewInstance();
            if (leftCupetLength > 0)
                sfs.PutIntArray("1", leftCupetIds);

            if (rightCupetLength > 0)
                sfs.PutIntArray("2", rightCupetIds);

            Response response = await Protocol.REQUEST_GUILD_BATTLE_ENTRY.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            this.leftCupetIds.Clear();
            if (leftCupetLength > 0)
            {
                this.leftCupetIds.AddRange(leftCupetIds);
            }

            this.rightCupetIds.Clear();
            if (rightCupetLength > 0)
            {
                this.rightCupetIds.AddRange(rightCupetIds);
            }

            IsGuildBattleRequest = true;

            OnUpdateGuildBattleRequest?.Invoke();

            UI.ShowToastPopup(LocalizeKey._90313.ToText()); // 길드전이 신청되었습니다.
        }

        /// <summary>
        /// [길드전] 버프 정보
        /// </summary>
        /// <returns></returns>
        public async Task RequestGuildBattleBuffInfo()
        {
            Response response = await Protocol.REQUEST_GUILD_BATTLE_BUFF_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("1"))
            {
                UpdateGuildBattleBuffInfo(response.GetPacketArray<GuildBattleBuffPacket>("1"));
            }

            OnUpdateGuildBattleBuff?.Invoke();
        }

        /// <summary>
        /// [길드전] 길드전 버프 경험치 업
        /// </summary>
        public async Task RequestGuildBattleBuffExpUp(int skillId, (int id, int count)[] items)
        {
            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();
            sfs.PutInt("1", skillId);
            foreach (var item in items)
            {
                var element = Protocol.NewInstance();
                element.PutInt("1", item.id);
                element.PutInt("2", item.count);
                sfsArray.AddSFSObject(element);

                ItemData itemData = itemDataRepo.Get(item.id);
                if (itemData == null)
                    continue;
            }
            sfs.PutSFSArray("2", sfsArray);

            var response = await Protocol.REQUEST_GUILD_BATTLE_BUFF_EXP_UP.SendAsync(sfs);
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

            // 길드전 버프 경험치
            if (response.ContainsKey("1"))
            {
                UpdateGuildBattleBuffInfo(response.GetPacketArray<GuildBattleBuffPacket>("1"));
            }

            OnUpdateGuildBattleBuff?.Invoke();
        }

        /// <summary>
        /// [길드전] 길드전 전투 길드 목록 요청
        /// </summary>
        public async Task RequestGuildBattleList()
        {
            var response = await Protocol.REQUEST_GUILD_BATTLE_LIST.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            GuildBattleOpponentPacket[] guildBattlePackets = response.ContainsKey("1") ? response.GetPacketArray<GuildBattleOpponentPacket>("1") : Array.Empty<GuildBattleOpponentPacket>();
            SetBattleGuildOpponents(guildBattlePackets);

            OnUpdateGuildBattleList?.Invoke();
        }

        #endregion
    }
}