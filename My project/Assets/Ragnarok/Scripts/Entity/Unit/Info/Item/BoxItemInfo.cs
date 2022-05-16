using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class BoxItemInfo : ConsumableItemInfo
    {
        private readonly BoxDataManager boxDataRepo;

        private ObscuredInt boxType;
        private ObscuredInt rewardTotalWeight;
        public override bool IsWeightCheckForBoxOpen => rewardTotalWeight > 0;    
        
        public override BoxType BoxType => boxType.ToEnum<BoxType>();

        public BoxItemInfo()
        {
            boxDataRepo = BoxDataManager.Instance;
        }

        public override void SetData(ItemData data)
        {
            base.SetData(data);
            BoxData boxData = boxDataRepo.Get(data.event_id);
            boxType = boxData.box_type;          
            rewardTotalWeight = GetRewardTotalWeight(boxData.rewards);
        }

        public override void ResetData()
        {
            base.ResetData();
            boxType = 0;
            rewardTotalWeight = 0;
        }

        private int GetRewardTotalWeight(RewardData[] rewards)
        {
            int totalWeight = 0;

            foreach (var item in rewards)
            {
                totalWeight += item.TotalWeight();
            }

            return totalWeight;
        }
    }
}