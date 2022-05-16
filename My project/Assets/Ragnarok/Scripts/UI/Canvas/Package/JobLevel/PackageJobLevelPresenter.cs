using System;
using UnityEngine;

namespace Ragnarok
{
    public class PackageJobLevelPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly ShopModel shopModel;

        /******************** Repositories ********************/
        private readonly RewardGroupDataManager rewardGroupDataRepo;

        /******************** Event ********************/
        public event Action OnPucharseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        private ShopInfo info;

        public PackageJobLevelPresenter()
        {
            shopModel = Entity.player.ShopModel;
            rewardGroupDataRepo = RewardGroupDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public bool Set()
        {
            int shopId = shopModel.JobLevelPackageShopId;
            if (shopId == 0)
                return false;

            info = shopModel.GetInfo(shopId);
            if (info == null)
            {
                Debug.LogError($"shop Info is Null: {nameof(shopId)} = {shopId}");
                return false;
            }

            return true;
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
        /// 상품 아이콘 이름
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            return info.IconName;
        }

        /// <summary>
        /// 구매 가능 남은 시간
        /// </summary>
        /// <returns></returns>
        public float GetRemainTime()
        {
            return shopModel.JobLevelPackageRemainTime.ToRemainTime();
        }

        /// <summary>
        /// 구매 가능 여부
        /// </summary>
        /// <returns></returns>
        public bool CanBuy()
        {
            return GetRemainTime() > 0;
        }

        /// <summary>
        /// 보상 정보 목록
        /// </summary>
        /// <returns></returns>
        public RewardGroupData[] GetRewards()
        {
            return rewardGroupDataRepo.Gets(info.GetGroupId());
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

        public string GetPoringText()
        {
            return info.GetSubDescription();
        }
    }
}