namespace Ragnarok
{
    /// <see cref="AuctionItemData"/> // 거래소 아이템 데이터
    /// <summary>
    /// 개인상점 아이템 데이터
    /// </summary>
    public class PrivateStoreItemData : TradeItemData
    {
        // 개인상점 등록할 때를 위한 변수...
        public bool isVirtualRegister;

        public PrivateStoreItemData() : base() { }
        public PrivateStoreItemData(int itemID) : base(itemID) { }
        public PrivateStoreItemData(ItemInfo itemInfo) : base(itemInfo) { }
        public PrivateStoreItemData(int itemID, short item_count, int item_price) : base(itemID)
        {
            SetInfo(-1, item_count, item_price);
        }
    }
}