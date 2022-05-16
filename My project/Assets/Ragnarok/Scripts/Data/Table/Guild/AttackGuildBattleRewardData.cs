namespace Ragnarok
{
    public sealed class AttackGuildBattleRewardData : GuildBattleRewardData
    {
        public AttackGuildBattleRewardData(int end, RewardData reward1, RewardData reward2, RewardData reward3)
            : base(end, reward1, reward2, reward3)
        {
        }

        public override string GetName()
        {
            if (IsEntryReward())
                return LocalizeKey._33930.ToText(); // 참여보상

            // 120,000,000
            return end.ToString("N0");
        }

        public override int CompareTo(GuildBattleRewardData other)
        {
            // 참여보상
            if (other.IsEntryReward())
                return -1;

            return other.end.CompareTo(end); // 입힌 피해량 높은 순으로 정렬
        }
    }
}