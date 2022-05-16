namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGateReward"/>
    /// </summary>
    public sealed class GateRewardPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly GateDataManager gateDataRepo;
        private readonly BoxDataManager boxDataRepo;

        private GateData currentData;

        public GateRewardPresenter()
        {
            gateDataRepo = GateDataManager.Instance;
            boxDataRepo = BoxDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetGateId(int gateId)
        {
            currentData = gateDataRepo.Get(gateId);
        }

        public string GetGateName()
        {
            if (currentData == null)
                return string.Empty;

            return currentData.name_id.ToText();
        }

        public RewardData[][] GetRewards()
        {
            if (currentData == null)
                return null;

            const int REWARD_COUNT = 5; // Reward 형태 총 5개
            RewardData[][] result = new RewardData[REWARD_COUNT][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = boxDataRepo.ToBoxRewards(currentData.GetReward(i));
            }

            return result;
        }
    }
}