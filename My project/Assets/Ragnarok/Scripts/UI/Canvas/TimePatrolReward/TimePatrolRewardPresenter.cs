namespace Ragnarok
{
    public class TimePatrolRewardPresenter : ViewPresenter
    {
        private readonly BetterList<RewardElement> normalRewards;
        private readonly BetterList<RewardElement> bossRewards;

        public TimePatrolRewardPresenter()
        {
            normalRewards = new BetterList<RewardElement>();
            bossRewards = new BetterList<RewardElement>();

            var normalRewardList = BasisType.TIME_PATROL_NORMAL_MONSTER_REWARD.GetKeyList();
            for (int i = 0; i < normalRewardList.Count; i++)
            {
                int itemId = BasisType.TIME_PATROL_NORMAL_MONSTER_REWARD.GetInt(normalRewardList[i]);
                normalRewards.Add(new RewardElement(itemId));
            }

            var bossRewardList = BasisType.TIME_PATROL_BOSS_MONSTER_REWARD.GetKeyList();
            for (int i = 0; i < bossRewardList.Count; i++)
            {
                int itemId = BasisType.TIME_PATROL_BOSS_MONSTER_REWARD.GetInt(bossRewardList[i]);
                bossRewards.Add(new RewardElement(itemId));
            }
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public UIRewardListElement.IInput[] GetNormalRewards()
        {
            return normalRewards.ToArray();
        }

        public UIRewardListElement.IInput[] GetBossRewards()
        {
            return bossRewards.ToArray();
        }

        private class RewardElement : UIRewardListElement.IInput
        {
            public RewardData Reward { get; }

            public int Rate => 0;

            public int TotalRate => 0;

            public RewardElement(int itemId)
            {
                Reward = new RewardData(RewardType.Item, itemId, 1);
            }

            public void SetTotalRate(int totalRate)
            {               
            }
        }
    }
}