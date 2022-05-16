namespace Ragnarok
{
    public sealed class RankGuildBattleRewardData : GuildBattleRewardData
    {
        private readonly int start;

        public RankGuildBattleRewardData(int start, int end, RewardData reward1, RewardData reward2, RewardData reward3)
            : base(end, reward1, reward2, reward3)
        {
            this.start = start;
        }

        public override string GetName()
        {
            if (IsEntryReward())
                return LocalizeKey._33930.ToText(); // 참여보상

            // 1위
            if (start == end)
                return LocalizeKey._33931.ToText().Replace(ReplaceKey.RANK, start); // {RANK}위

            // 4위 ~ 10위
            return StringBuilderPool.Get()
                .Append(LocalizeKey._33931.ToText().Replace(ReplaceKey.RANK, start)) // {RANK}위
                .Append(" ~ ")
                .Append(LocalizeKey._33931.ToText().Replace(ReplaceKey.RANK, end)) // {RANK}위
                .Release();
        }

        public override int CompareTo(GuildBattleRewardData other)
        {
            // 참여보상
            if (other.IsEntryReward())
                return -1;

            return end.CompareTo(other.end); // 랭킹 작은 순으로 정렬
        }
    }
}