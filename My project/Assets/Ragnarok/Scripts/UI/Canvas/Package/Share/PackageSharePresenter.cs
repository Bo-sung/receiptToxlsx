namespace Ragnarok
{
    public class PackageSharePresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly ShopModel shopModel;

        /******************** Repositories ********************/

        /******************** Event ********************/

        int shareUpPointRate;

        private int shopId;

        public PackageSharePresenter()
        {
            shopModel = Entity.player.ShopModel;
            shareUpPointRate = BasisType.SHARE_UP_POINT_RATE.GetInt();
        }

        public override void AddEvent()
        {
            shopModel.OnPurchaseSuccess += OnPurchaseSuccess;
        }

        public override void RemoveEvent()
        {
            shopModel.OnPurchaseSuccess -= OnPurchaseSuccess;
        }

        public void SetShopId(int shopId)
        {
            this.shopId = shopId;
        }

        public string GetPrice()
        {
            return shopModel.GetInfo(shopId).CostText;
        }

        /// <summary>
        /// 우편함에 보유중 여부
        /// </summary>
        /// <returns></returns>
        public bool HasMailShopItem()
        {
            return shopModel.HasMailShopItem(shopId);
        }

        /// <summary>
        /// 활성화 여부
        /// </summary>
        /// <returns></returns>
        public bool IsShareBuff()
        {
            return shopModel.IsShareBuff;
        }

        public string GetShareUpRateText()
        {
            return $"+{MathUtils.ToPermyriadText(shareUpPointRate)}";
        }

        public void OnClickedBtnPurchase()
        {
            shopModel.RequestCashShopPurchase(shopId, isShowNotiPopup: false).WrapNetworkErrors();
        }

        private void OnPurchaseSuccess()
        {
            UI.Close<UIPackageShare>();
        }
    }
}