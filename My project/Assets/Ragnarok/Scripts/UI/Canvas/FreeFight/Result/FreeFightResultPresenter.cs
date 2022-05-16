namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIFreeFightResult"/>
    /// </summary>
    public class FreeFightResultPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly FreeFightRewardDataManager freeFightRewardDataRepo;
        public readonly int totalRound;

        public FreeFightResultPresenter()
        {
            freeFightRewardDataRepo = FreeFightRewardDataManager.Instance;
            totalRound = BasisType.FF_ROUNT_COUNT.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public RewardData[] GetCurrentRewards(FreeFightEventType eventType, int killCount)
        {
            return freeFightRewardDataRepo.GetRewards(eventType, killCount);
        }
    }
}