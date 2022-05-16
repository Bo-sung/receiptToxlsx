using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class GuildRankInfo : RankInfo
    {
        int guildId;
        int maxMemberCount;
        int curMemberCount;
        int emblem;
        string guildName;
        string guildMasterName;
        int guildLevel;

        public override int GuildId => guildId;
        public override int MaxMemberCount => maxMemberCount;
        public override int CurMemberCount => curMemberCount;
        public override int EmblemBg => MathUtils.GetBitFieldValue(emblem, 0);
        public override int EmblemFrame => MathUtils.GetBitFieldValue(emblem, 6);
        public override int EmblemIcon => MathUtils.GetBitFieldValue(emblem, 12);
        public override string GuildName => guildName;
        public override string GuildMasterName => guildMasterName;
        public override int GuildLevel => guildLevel;

        public override void SetGuildId(int id)
        {
            guildId = id;
        }

        public override void SetMaxMemberCount(int count)
        {
            maxMemberCount = count;
        }

        public override void SetCurMemberCount(int count)
        {
            curMemberCount = count;
        }

        public override void SetEmblem(int id)
        {
            emblem = id;
        }

        public override void SetGuildName(string name)
        {
            guildName = name;
        }

        public override void SetGuildMasterName(string name)
        {
            guildMasterName = name;
        }

        public override void SetGuildLevel(int level)
        {
            guildLevel = level;
        }

        protected override string GetDescription()
        {
            return string.Empty;
        }

        protected override string GetProfileName()
        {
            return string.Empty;
        }
    }
}