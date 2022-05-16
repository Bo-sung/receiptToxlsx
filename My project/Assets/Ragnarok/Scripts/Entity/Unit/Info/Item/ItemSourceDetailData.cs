namespace Ragnarok
{
    public class ItemSourceDetailData : IData
    {
        public ItemSourceCategoryType categoryType;

        // 스테이지의 경우 : stage_id, 
        // 던전의 경우 : dungeon_id, 
        // 월드보스의 경우 : worldboss_id ?, 
        public string text;
        public int value_1;

        // 상자, 제작의 경우 : item_id
        public ItemInfo itemInfo;



        public ItemSourceDetailData(ItemSourceCategoryType categoryType, string text = default, int value_1 = default, ItemInfo itemInfo = default)
        {
            this.categoryType = categoryType;
            this.text = text;
            this.value_1 = value_1;
            this.itemInfo = itemInfo;
        }
    }
}