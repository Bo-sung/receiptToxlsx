namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildBattleReward"/>
    /// </summary>
    public sealed class GuildBattleRewardPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly GuildBattleRewardDataManager guildBattleRewardDataRepo;

        public GuildBattleRewardPresenter()
        {
            guildBattleRewardDataRepo = GuildBattleRewardDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public UIRankRewardElement.IInput[] GetAttackRewards()
        {
            return guildBattleRewardDataRepo.GetAttackRewards();
        }

        public UIRankRewardElement.IInput[] GetDefenseRewards()
        {
            return guildBattleRewardDataRepo.GetDefenseRewards();
        }

        public UIRankRewardElement.IInput[] GetRankRewards()
        {
            return guildBattleRewardDataRepo.GetRankRewards();
        }
    }
}