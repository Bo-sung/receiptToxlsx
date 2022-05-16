using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class PackageGeneralPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly ShopModel shopModel;

        /******************** Repositories ********************/
        RewardGroupDataManager rewardGroupDataRepo;

        /******************** Event ********************/
        public event Action OnPucharseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        private ShopInfo info;

        public PackageGeneralPresenter()
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
        /// 상품 아이콘 이름
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            return info.IconName;
        }

        /// <summary>
        /// 구매 가능 여부
        /// </summary>
        /// <returns></returns>
        public bool CanBuy()
        {
            return info.GetCanBuy();
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
    }
}