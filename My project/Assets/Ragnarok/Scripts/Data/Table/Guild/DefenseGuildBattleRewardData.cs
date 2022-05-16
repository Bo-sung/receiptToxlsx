using UnityEngine;

namespace Ragnarok
{
    public sealed class DefenseGuildBattleRewardData : GuildBattleRewardData
    {
        private int percentageValue;

        public DefenseGuildBattleRewardData(int end, RewardData reward1, RewardData reward2, RewardData reward3)
            : base(end, reward1, reward2, reward3)
        {
        }

        /// <summary>
        /// 엠펠리움 최대 Hp 세팅
        /// </summary>
        public override void SetMaxEmperiumHp(int maxHp)
        {
            percentageValue = Mathf.RoundToInt(MathUtils.GetRate(end, maxHp) * 100);
        }

        public override string GetName()
        {
            if (IsEntryReward())
                return LocalizeKey._33930.ToText(); // 참여보상

            // 120,000,000 (100%)
            return StringBuilderPool.Get()
                .Append(end.ToString("N0"))
                .Append(" (").Append(percentageValue).Append("%)")
                .Release();
        }

        public override int CompareTo(GuildBattleRewardData other)
        {
            // 참여보상
            if (other.IsEntryReward())
                return -1;

            return other.end.CompareTo(end); // 남은 엠펠리움 Hp 높은 순으로 정렬
        }
    }
}