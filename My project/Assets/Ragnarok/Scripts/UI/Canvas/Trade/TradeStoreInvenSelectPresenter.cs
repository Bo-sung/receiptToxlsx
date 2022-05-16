using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UITradeStoreInvenSelect"/>
    /// </summary>
    public class TradeStoreInvenSelectPresenter : ViewPresenter
    {
        private readonly InventoryModel invenModel;

        public TradeStoreInvenSelectPresenter()
        {
            invenModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 아이템 목록 얻어오기
        /// </summary>
        /// <param name="itemInfoType"></param>
        /// <param name="isExceptPrivateExist">개인상점에 등록된 아이템종류는 제외시킬지.</param>
        /// <returns></returns>
        public List<ItemInfo> GetInventoryItemList(System.Type itemInfoType, bool isExceptPrivateExist)
        {
            if (itemInfoType.IsEquivalentTo(typeof(EquipmentItemInfo)))
            {
                return invenModel.itemList.FindAll(e => e is EquipmentItemInfo && !e.IsEquipped && !e.IsLock && (!isExceptPrivateExist || !isAlreadyRegisteredItem_Private(e.ItemNo)));
            }
            if (itemInfoType.IsEquivalentTo(typeof(PartsItemInfo)))
            {
                return invenModel.itemList.FindAll(e => e is PartsItemInfo && (!isExceptPrivateExist || !isAlreadyRegisteredItem_Private(e.ItemNo)));
            }
            if (itemInfoType.IsEquivalentTo(typeof(CardItemInfo)))
            {
                return invenModel.itemList.FindAll(e => e is CardItemInfo && !e.IsLock && !e.IsEquipped && (!isExceptPrivateExist || !isAlreadyRegisteredItem_Private(e.ItemNo)));
            }

            return null;
        }

        /// <summary>
        /// 개인상점에 이미 등록되어있는 아이템인지 체크
        /// </summary>
        /// <param name="itemNo"></param>
        bool isAlreadyRegisteredItem_Private(long itemNo)
        {
            return Entity.player.Trade.PrivateStoreItemList.Exists(e => e.item_No == itemNo);
        }

        public void SelectItem(ItemInfo info)
        {
            if (!info.CanTrade)
            {
                UI.ShowToastPopup(LocalizeKey._34024.ToText()); // 거래가 불가능한 아이템입니다.
                return;
            }

            // 카드가 박힌 장비는 등록 불가
            if (info.ItemGroupType == ItemGroupType.Equipment)
            {
                if (info.IsEquippedCard)
                {
                    UI.ShowToastPopup(LocalizeKey._90196.ToText()); // 카드가 장착된 장비는 등록할 수 없습니다.
                    return;
                }
            }

            if (info.ItemGroupType == ItemGroupType.Card)
            {
                if (info.IsShadow && info.CardLevel > 1)
                {
                    UI.ShowToastPopup(LocalizeKey._90287.ToText()); // 쉐도우 카드는 레벨1 카드만 거래할 수 있습니다.
                    return;
                }
            }

            CloseUI();

            int spendRoPoint = info.RoPoint;

            // 장비 초월의 경우 구매시 필요 ROPoint 추가
            // 서버에서 itemTranscend값을 카드에서 사용중
            if (info.ItemGroupType == ItemGroupType.Equipment && info.ItemTranscend > 0)
            {
                spendRoPoint += BasisType.TRANSCEND_ITEM_EXTRA_ROPOINT.GetInt(info.ItemTranscend);
            }

            UI.Show<UIPrivateStoreProductSetting>().Show(itemInfo: info,
                price: 0,
                isMyStore: true,
                lowestPriceLimit: info.Price,
                maxPriceLimit: info.MaxPrice,
                spendRoPoint,
                CID: Entity.player.Character.Cid);
        }

        public void CloseUI()
        {
            UI.Close<UITradeStoreInvenSelect>();
        }

    }
}