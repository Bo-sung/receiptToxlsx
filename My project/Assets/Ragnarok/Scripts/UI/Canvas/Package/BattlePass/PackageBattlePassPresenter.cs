using System;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIPackageBattlePass"/>
    /// </summary>
    public class PackageBattlePassPresenter : ViewPresenter
    {
        private readonly PassType passType;
        private readonly ShopInfo info;

        // <!-- Models --!>
        private readonly ShopModel shopModel;
        private readonly CharacterModel charactorModel;

        // <!-- Event --!>
        public event Action OnPucharseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        public PackageBattlePassPresenter(PassType passType)
        {
            this.passType = passType;

            shopModel = Entity.player.ShopModel;
            charactorModel = Entity.player.Character;
            BasisShop basisShop = shopModel.GetBasisShop(passType);
            info = shopModel.GetInfo(basisShop.GetID());
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 상품 이름
        /// </summary>
        public string GetTitle()
        {
            return info.Name;
        }

        /// <summary>
        /// 상품 설명
        /// </summary>
        public string GetDescription()
        {
            return info.Description;
        }

        /// <summary>
        /// 상품 가격
        /// </summary>
        public string GetPrice()
        {
            return info.CostText;
        }

        /// <summary>
        /// 구매 가능 여부
        /// </summary>
        //public bool CanBuy()
        //{
        //    // 직업레벨 체크
        //    if (charactorModel.JobLevel < info.JobLevel)
        //        return false;
        //
        //    // 패스 활성화중
        //    if (shopModel.IsActivePass(passType))
        //        return false;
        //
        //    // 우편함에 있음
        //    if (shopModel.HasMailShopItem(info.ID))
        //        return false;
        //
        //    // 시즌 기간 아님
        //    if (!shopModel.IsBattlePass(passType))
        //        return false;
        //
        //    return true;
        //}
        /// <summary>
        /// 구매 가능 여부
        /// </summary>
        public PassState CanBuy()
        {
            // 직업레벨 체크
            if (charactorModel.JobLevel < info.JobLevel)
                return PassState.LevelLimit;

            // 패스 활성화중
            if (shopModel.IsActivePass(passType))
                return PassState.OnActivate;

            // 우편함에 있음
            if (shopModel.HasMailShopItem(info.ID))
                return PassState.Purchased;

            // 시즌 기간 아님
            if (!shopModel.IsBattlePass(passType))
                return PassState.NotTime;

            return PassState.Avalable;
        }

        public RemainTime GetRemainTime()
        {
            return shopModel.GetSeasonRemainTime(passType);
        }

        public void OnClickedBtnPurchase()
        {
            shopModel.RequestCashShopPurchase(info.ID, isShowNotiPopup: false).WrapNetworkErrors();
        }

        public ShopProductType GetProductValueType()
        {
            return info.GetProductValueType();
        }

        public int GetProductValue()
        {
            return info.GetProductValue();
        }

        /// <summary>
        /// 패키지의 레벨 제한
        /// </summary>
        /// <returns></returns>
        public int GetJobLevelLimitValue()
        {
            return info.JobLevel;
        }
        /// <summary>
        /// 플레이어의 레벨 제한
        /// </summary>
        /// <returns></returns>
        public int GetPlayerJobLevelValue()
        {
            return charactorModel.JobLevel;
        }
    }
}