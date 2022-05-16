using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class MaterialInfo : DataInfo<ItemData>
    {
        ObscuredInt count; // 필요 수량
        ObscuredInt smelt; // [장비] 제련도

        public MaterialInfo(int count, int smelt)
        {
            this.count = count;
            this.smelt = smelt;
        }

        public int SlotIndex;

        public int ItemId => data.id;
        /// <summary>
        /// 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;
        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string ItemName => data.name_id.ToText();

        public ItemGroupType ItemGroupType => data.ItemGroupType;
        public ItemType ItemType => data.ItemType;
        public ItemData ItemData => data;
        /// <summary>
        /// 중첩 가능아이템 여부
        /// </summary>
        public bool IsStackable => data.IsStackable();
        /// <summary>
        /// 필요 수량
        /// </summary>
        public int Count => count;

        /// <summary>
        /// [장비] 필요 제련도
        /// </summary>
        public int Smelt => smelt;

        public RewardData GetRewardData()
        {
            return new RewardData(6, data.id, 1);
        }

        /// <summary>
        /// 속성석 레벨
        /// </summary>
        public int GetElementStoneLevel()
        {
            if (ItemData == null)
                return -1;

            if (ItemType != ItemType.ProductParts)
                return -1;

            return ItemData.GetElementStoneLevel();
        }
    }
}
