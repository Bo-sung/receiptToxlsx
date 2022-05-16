using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleGuildWar"/>
    /// </summary>
    public sealed class BattleGuildWarPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly GuildBattleRewardDataManager guildBattleRewardDataRepo;
        private readonly int maxEmperiumHp;

        public BattleGuildWarPresenter()
        {
            guildBattleRewardDataRepo = GuildBattleRewardDataManager.Instance;
            maxEmperiumHp = Mathf.Max(1, BasisGuildWarInfo.EmperiumMaxHp.GetInt());
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 현재까지 보상
        /// </summary>
        public RewardData[] GetCurrentRewards(int damage)
        {
            return guildBattleRewardDataRepo.GetAttackRewards(damage);
        }

        /// <summary>
        /// 현재 Damage 진행도
        /// </summary>
        public int GetDamagePercentProgress(long damage)
        {
            return (int)((damage * 100) / maxEmperiumHp);
        }

        /// <summary>
        /// 다음 보상까지 남은 Damage 진행도
        /// </summary>
        public int GetNextRemainDamageProgress(int damage)
        {
            return GetDamagePercentProgress(guildBattleRewardDataRepo.GetNextRemainDamage(damage));
        }
    }
}