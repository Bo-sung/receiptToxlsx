using CodeStage.AntiCheat.ObscuredTypes;
using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CommonRankInfo"/>
    /// <see cref="GuildRankInfo"/>
    /// </summary>
    public abstract class RankInfo : IInfo, UIRankElement.IInput, AdventureRankingElement.IInput, UIGuildBattleRankElement.IInput, UIGuildBattleMyRankElement.IInput
    {
        bool IInfo.IsInvalidData => false; // 무조건 유효한 데이터
        event System.Action IInfo.OnUpdateEvent { add { } remove { } } // 데이터가 변경 될 일이 음슴 

        public RankType RankType { get; private set; }

        ObscuredLong rank;
        protected ObscuredDouble score;
        bool isScore;

        /// <summary>
        /// [공통] 랭킹 정보
        /// </summary>
        public long Rank => rank;

        /// <summary>
        /// [공통] 랭킹 점수
        /// </summary>
        public virtual double Score => score;

        /// <summary>
        /// 스코어 점수 있는지 여부
        /// </summary>
        public bool IsScore => isScore;

        /// <summary>
        /// 유저 ID
        /// </summary>
        public virtual int UID => default;

        /// <summary>
        /// 캐릭터 이름
        /// </summary>
        public virtual string CharName => default;

        /// <summary>
        /// 직업레벨
        /// </summary>
        public virtual int JobLevel => default;

        /// <summary>
        /// 성별
        /// </summary>
        public virtual Gender Gender => default;

        /// <summary>
        /// 직업
        /// </summary>
        public virtual Job Job => default;

        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        public virtual int CID => default;

        /// <summary>
        /// 캐릭터 ID HexCode
        /// </summary>
        public virtual string CIDHex => default;

        /// <summary>
        /// 전투력
        /// </summary>
        public virtual int BattleScore => default;

        /// <summary>
        /// [길드] 길드 ID
        /// </summary>
        public virtual int GuildId => default;

        /// <summary>
        /// [길드] 길드 최대 인원수
        /// </summary>
        public virtual int MaxMemberCount => default;

        /// <summary>
        /// [길드] 길드 현재 인원수
        /// </summary>
        public virtual int CurMemberCount => default;

        /// <summary>
        /// [길드] 길드엠블럼 배경
        /// </summary>
        public virtual int EmblemBg => default;

        /// <summary>
        /// [길드] 길드엠블럼 프레임
        /// </summary>
        public virtual int EmblemFrame => default;

        /// <summary>
        /// [길드] 길드엠블럼 아이콘
        /// </summary>
        public virtual int EmblemIcon => default;

        /// <summary>
        /// [길드] 길드이름
        /// </summary>
        public virtual string GuildName => default;

        /// <summary>
        /// [길드] 길드장 이름
        /// </summary>
        public virtual string GuildMasterName => default;

        /// <summary>
        /// [길드] 길드 레벨
        /// </summary>
        public virtual int GuildLevel => default;

        public string Description => GetDescription();

        public string ProfileName => GetProfileName();

        public void SetRankType(RankType rankType)
        {
            this.RankType = rankType;
        }

        /// <summary>
        /// [공통] 랭크 정보 세팅
        /// </summary>
        /// <param name="rank"></param>
        public void SetRank(long rank)
        {
            this.rank = rank;
        }

        /// <summary>
        /// [공통] 랭킹 점수
        /// </summary>
        /// <param name="score"></param>
        public virtual void SetScore(double? score)
        {
            isScore = score.HasValue;
            if (score.HasValue)
                this.score = score.Value;
        }

        /// <summary>
        /// 유저 Id
        /// </summary>
        /// <param name="uId"></param>
        public virtual void SetUId(int uId)
        { }

        /// <summary>
        /// 캐릭터 이름
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetCharName(string name)
        { }

        /// <summary>
        /// 직업 레벨
        /// </summary>
        /// <param name="level"></param>
        public virtual void SetJobLevel(int level)
        { }

        /// <summary>
        /// 성별
        /// </summary>
        /// <param name="gender"></param>
        public virtual void SetGender(Gender gender)
        { }

        /// <summary>
        /// 직업
        /// </summary>
        /// <param name="job"></param>
        public virtual void SetJob(Job job)
        { }

        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        /// <param name="cid"></param>
        public virtual void SetCId(int cid)
        { }

        /// <summary>
        /// 캐릭터 HexCode
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetCIdHex(string name)
        { }

        /// <summary>
        /// 전투력
        /// </summary>
        /// <param name="battleScore"></param>
        public virtual void SetBattleScore(int battleScore)
        { }

        /// <summary>
        /// 프로필 Id
        /// </summary>
        public virtual void SetProfileId(int profileId)
        { }

        /// <summary>
        /// [길드] 길드Id 세팅
        /// </summary>
        /// <param name="guildId"></param>
        public virtual void SetGuildId(int guildId)
        { }

        /// <summary>
        /// [길드] 길드 최대 인원수
        /// </summary>
        /// <param name="Count"></param>
        public virtual void SetMaxMemberCount(int count)
        { }

        /// <summary>
        /// [길드] 길드 현재 인원수
        /// </summary>
        /// <param name="count"></param>
        public virtual void SetCurMemberCount(int count)
        { }

        /// <summary>
        /// [길드] 엠블렘 ID
        /// </summary>
        /// <param name="id"></param>
        public virtual void SetEmblem(int id)
        { }

        /// <summary>
        /// [길드] 길드이름
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetGuildName(string name)
        { }

        /// <summary>
        /// [길드] 길드장 이름
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetGuildMasterName(string name)
        { }

        /// <summary>
        /// [길드] 길드레벨 세팅
        /// </summary>
        public virtual void SetGuildLevel(int level)
        { }

        protected abstract string GetDescription();

        protected abstract string GetProfileName();
    }
}