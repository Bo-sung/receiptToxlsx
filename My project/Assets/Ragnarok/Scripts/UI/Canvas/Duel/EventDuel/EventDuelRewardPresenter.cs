namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEventDuelReward"/>
    /// </summary>
    public class EventDuelRewardPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly EventDuelRewardDataManager eventDuelRewardDataRepo;
        private readonly EventDualBuffDataManager eventDualBuffDataRepo;

        public EventDuelRewardPresenter()
        {
            eventDuelRewardDataRepo = EventDuelRewardDataManager.Instance;
            eventDualBuffDataRepo = EventDualBuffDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public UIDuelBuffReward.IInput GetPerfectBuffReward()
        {
            return eventDualBuffDataRepo.GetPerfectReward();
        }

        public UIDuelBuffReward.IInput[] GetBuffRewards()
        {
            return eventDualBuffDataRepo.GetNormalRewards();
        }

        public UIDuelReward.IInput[] GetWorldServerRewards()
        {
            return eventDuelRewardDataRepo.GetWorldServerRewards();
        }

        public UIDuelReward.IInput[] GetServerRewards()
        {
            return eventDuelRewardDataRepo.GetServerRewards();
        }
    }
}