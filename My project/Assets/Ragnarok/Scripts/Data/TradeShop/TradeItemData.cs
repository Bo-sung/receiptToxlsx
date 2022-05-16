namespace Ragnarok
{
    /// <see cref="PrivateStoreItemData"/> // 개인상점 아이템 데이터
    /// <see cref="AuctionItemData"/> // 거래소 아이템 데이터
    /// <summary>
    /// 거래 아이템 데이터
    /// </summary>
    public class TradeItemData : IInfo
    {
        /// <summary>
        /// 노점 상품 고유 인덱스 구매시 유효성 체크
        /// </summary>
        public byte Index { get; private set; }

        public ItemData itemData;
        public long item_No;
        public int item_count;
        public int item_price;

        // 장비
        public int tier_per;
        public ItemEquipmentSlotType item_pos;
        public long card_id1;
        public long card_id2;
        public long card_id3;
        public long card_id4;

        /// <summary>
        /// [장비일 때] itemLevel(Smelt, 강화도)
        /// [카드일 때] cardLevel(CardLevel, 카드레벨)
        /// </summary>
        public byte item_level;
        public int itemTranscend;
        public int itemChangedElement;
        public int itemElementLevel;

        // ItemInfo로 변환할 때의 편의성을 위해 ..
        ItemInfo itemInfo;
        public ItemInfo ItemInfo => GetItemInfo();

        public bool IsInvalidData => itemData == null || itemInfo == null || itemInfo.ItemType != itemData.ItemType;

        public event System.Action OnUpdateEvent;

        public TradeItemData() { }
        public TradeItemData(int itemID) { SetData(itemID); }
        public TradeItemData(ItemInfo itemInfo)
        {
            SetData(itemInfo.ItemId);
            item_No = itemInfo.ItemNo;
            item_count = (short)itemInfo.ItemCount;
            item_price = 0;

            this.tier_per = itemInfo.Tier;
            this.item_level = (byte)itemInfo.CardLevel;
            this.item_pos = itemInfo.SlotType;
            itemTranscend = itemInfo.ItemTranscend;
            itemChangedElement = itemInfo.ItemChangedElement;
            itemElementLevel = itemInfo.ElementLevel;

            if (itemInfo.ItemGroupType == ItemGroupType.Card)
            {
                tier_per = itemInfo.CardLevel;
                this.card_id1 = itemInfo.GetCardOptionValue(0);
                this.card_id2 = itemInfo.GetCardOptionValue(1);
                this.card_id3 = itemInfo.GetCardOptionValue(2);
                this.card_id4 = itemInfo.GetCardOptionValue(3);
            }
            else if (itemInfo.ItemGroupType == ItemGroupType.Equipment)
            {
                this.item_level = itemInfo.Smelt.ToByteValue(); // [장비]
                this.card_id1 = itemInfo.GetCardItem(0) ? itemInfo.GetCardItem(0).ItemId : 0;
                this.card_id2 = itemInfo.GetCardItem(1) ? itemInfo.GetCardItem(1).ItemId : 0;
                this.card_id3 = itemInfo.GetCardItem(2) ? itemInfo.GetCardItem(2).ItemId : 0;
                this.card_id4 = itemInfo.GetCardItem(3) ? itemInfo.GetCardItem(3).ItemId : 0;
            }

            /// TODO: 임시 코드
            this.item_pos = 0;
        }


        public void SetData(int itemID)
        {
            itemData = ItemDataManager.Instance.Get(itemID);
        }

        public void SetInfo(long item_No, int item_count, int item_price = 0, int tier_per = 0, byte item_level = 0, ItemEquipmentSlotType item_pos = 0, long card_id1 = 0, long card_id2 = 0, long card_id3 = 0, long card_id4 = 0, int itemTranscend = 0, int itemElementChange = 0, int itemElementLevel = 0)
        {
            this.item_No = item_No;
            this.item_count = item_count;
            this.item_price = item_price;
            this.tier_per = tier_per;
            this.item_level = item_level;
            this.item_pos = item_pos;
            this.card_id1 = card_id1;
            this.card_id2 = card_id2;
            this.card_id3 = card_id3;
            this.card_id4 = card_id4;
            this.itemTranscend = itemTranscend;
            this.itemChangedElement = itemElementChange;
            this.itemElementLevel = itemElementLevel;

            /// TODO: 임시 코드
            this.item_pos = 0;
        }

        public void SetIndex(byte index)
        {
            Index = index;
        }

        public ItemInfo GetItemInfo()
        {
            if (IsInvalidData)
            {
                switch (itemData.ItemGroupType)
                {
                    case ItemGroupType.Equipment:
                        itemInfo = new EquipmentItemInfo(Entity.player.Inventory);
                        break;

                    case ItemGroupType.Card:
                        itemInfo = new CardItemInfo();
                        break;

                    case ItemGroupType.ProductParts:
                        itemInfo = new PartsItemInfo();
                        break;

                    case ItemGroupType.ConsumableItem:

                        if (itemData.ItemType == ItemType.Box)
                        {
                            itemInfo = new BoxItemInfo();
                        }
                        else
                        {
                            itemInfo = new ConsumableItemInfo();
                        }
                        break;
                    default:
                        return null;
                }
            }

            itemInfo.SetData(itemData);

            itemInfo.SetItemNo(item_No);
            itemInfo.SetItemCount(item_count);

            itemInfo.SetItemInfo(
                tier_per,
                item_level,
                item_pos.ToByteValue(),
                card_id1,
                card_id2,
                card_id3,
                card_id4,
                false,
                itemTranscend,
                itemChangedElement,
                itemElementLevel);

            itemInfo.SetRemainCoolDown(float.MaxValue);

            return itemInfo;
        }
    }
}