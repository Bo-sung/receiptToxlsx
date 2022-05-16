using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIPrivateStoreProductSetting"/>
    /// </summary>
    public class PrivateStoreProductSettingPresenter : ViewPresenter
    {
        public interface IView
        {
        }

        private readonly TradeModel tradeModel;
        private readonly InventoryModel invenModel;
        private readonly GoodsModel goodsModel;

        public bool IsPrivateStoreSelling => (tradeModel.SellingState == PrivateStoreSellingState.SELLING);

        /// <summary>
        /// 거래소 물품 최대 등록 수
        /// </summary>
        public readonly int maxRegisterCount;

        public PrivateStoreProductSettingPresenter(IView view)
        {
            tradeModel = Entity.player.Trade;
            invenModel = Entity.player.Inventory;
            goodsModel = Entity.player.Goods;
            maxRegisterCount = BasisType.MARKET_MAX_REGIST_COUNT.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetItemHaveCount(int itemID)
        {
            return invenModel.GetItemCount(itemID);
        }

        /// <summary>
        /// 인벤토리에서 아이템 차감 또는 제거
        /// </summary>
        public void RemoveFromInven(long itemNo, int itemID, int count)
        {
            invenModel.RemoveFromItemList(itemNo, itemID, count);
        }

        /// <summary>
        /// 개인상점 인벤에 아이템 추가
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="isVirtual"></param>
        public void PrivateInvenRegister(ItemInfo itemInfo, int count, int price, bool isVirtual)
        {
            PrivateStoreItemData data = new PrivateStoreItemData(itemInfo);
            data.item_count = (short)count;
            data.item_price = price;
            tradeModel.PrivateStoreItemList.Register(data, isVirtual);
        }

        /// <summary>
        /// UIPersonalStore Refresh 이벤트
        /// </summary>
        public void RefreshPersonalStore()
        {
            // 내 개인상점 변동 이벤트
            UI.GetUI<UIPrivateStore>()?.Refresh();
        }

        public async Task<bool> RequestPrivateItemBuy(int CID, long itemNo, int itemID, int itemCount, byte index)
        {
            return await tradeModel.RequestPrivateItemBuy(CID, itemNo, itemID, itemCount, index);
        }

        /// <summary>
        /// 판매중일 때 아이템 추가 등록 프로토콜 송신.
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        /// <returns>추가된 아이템 목록? 전체 목록?</returns>
        public async Task<bool> RequestPrivateItemRegister(PrivateStoreItemData item)
        {
            List<PrivateStoreItemData> newItem = new List<PrivateStoreItemData>();
            newItem.Add(item);
            return await tradeModel.RequestPrivateItemRegister(newItem, tradeModel.StallName);
        }

        /// <summary>
        /// 가진 재화로 구매할 수 있는 최대치
        /// </summary>
        public int GetMaxCount(long priceEA, int roPointEA, int maxCount)
        {
            long haveZeny = goodsModel.Zeny;
            long haveRoPoint = goodsModel.RoPoint;

            int maxCount_Zeny = priceEA == 0 ? int.MaxValue : Mathf.FloorToInt(haveZeny / priceEA); // 제니로 살 수 있는 최대 개수
            int maxCount_RoPoint = haveRoPoint < roPointEA ? 0 : int.MaxValue; // RoPoint로 살 수 있는 최대 개수

            int maxCount_total = Mathf.Min(maxCount_Zeny, maxCount_RoPoint); // 가진 제니, RoPoint 내에서 살 수 있는 최대 개수
            int availableCount = Mathf.Min(maxCount_total, maxCount); // 상점에 올려진 아이템 개수를 적용한 현실적인 최대 개수

            return Mathf.Max(1, availableCount); // 아무리 재화가 부족해도 최소 1을 반환
        }
    }
}