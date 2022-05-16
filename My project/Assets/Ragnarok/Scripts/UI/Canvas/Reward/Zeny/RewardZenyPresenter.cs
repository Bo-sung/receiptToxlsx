namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIRewardZeny"/>
    /// </summary>
    public sealed class RewardZenyPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly ItemDataManager itemDataRepo;
        private readonly BoxDataManager boxDataRepo;
        private readonly int zenyBoxId;

        public RewardZenyPresenter()
        {
            itemDataRepo = ItemDataManager.Instance;
            boxDataRepo = BoxDataManager.Instance;
            zenyBoxId = BasisItem.ZenyBox.GetID();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 제니 박스
        /// </summary>
        public RewardData GetZenyBox()
        {
            ItemData itemData = itemDataRepo.Get(zenyBoxId);
            if (itemData == null)
                return null;

            if (itemData.ItemType == ItemType.Box)
            {
                BoxData boxData = boxDataRepo.Get(itemData.event_id);
                if (boxData == null)
                    return null;

                if (boxData.rewards.Length > 0)
                    return boxData.rewards[0];
            }

            return new RewardData(RewardType.Item, zenyBoxId, 1);

        }
    }
}