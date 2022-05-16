namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildBattleRewardEvent"/>
    /// </summary>
    public class GuildBattleRewardEventPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly GuildBattleRewardDataManager guildBattleRewardDataRepo;

        public GuildBattleRewardEventPresenter()
        {
            guildBattleRewardDataRepo = GuildBattleRewardDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public UIRankRewardElement.IInput[] GetEventRewards()
        {
            return guildBattleRewardDataRepo.GetEventRewards();
        }
    }
}