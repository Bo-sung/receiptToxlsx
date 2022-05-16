namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIItemSource"/>
    /// </summary>
    public class ItemSourceCategoryData : IData
    {
        public ItemSourceCategoryType categoryType;
        public ItemInfo itemInfo;

        public ItemSourceCategoryData(ItemSourceCategoryType categoryType, ItemInfo itemInfo)
        {
            this.categoryType = categoryType;
            this.itemInfo = itemInfo;
        }
    }
}