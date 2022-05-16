using System;

namespace Ragnarok
{
    public class PackageTreePresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly ShopModel shopModel;

        /******************** Repositories ********************/

        /******************** Event ********************/
        public event Action OnPucharseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        private ShopInfo info;

        public PackageTreePresenter()
        {
            shopModel = Entity.player.ShopModel;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetShopId(int shopId)
        {
            info = shopModel.GetInfo(shopId);
        }

        /// <summary>
        /// 상품 이름
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return info.Name;
        }

        /// <summary>
        /// 상품 설명
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return info.Description;
        }

        /// <summary>
        /// 상품 가격
        /// </summary>
        /// <returns></returns>
        public string GetPrice()
        {
            return info.CostText;
        }

        /// <summary>
        /// 구매 가능 여부
        /// </summary>
        /// <returns></returns>
        public bool CanBuy()
        {
            return !shopModel.HasMailShopItem(info.ID) && shopModel.TreePackRemainTime.ToRemainTime() <= 0;
        }

        public void OnClickedBtnPurchase()
        {
            shopModel.RequestCashShopPurchase(info.ID, isShowNotiPopup: false).WrapNetworkErrors();
        }
    }
}