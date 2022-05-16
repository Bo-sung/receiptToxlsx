using System;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class PackageCatCoinPresenter : ViewPresenter
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

        public event Action OnUpdateEveryDayGoods
        {
            add { shopModel.OnUpdateEveryDayGoods += value; }
            remove { shopModel.OnUpdateEveryDayGoods -= value; }
        }

        protected ShopInfo info;

        public PackageCatCoinPresenter()
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
            shopModel.RequestEveryDayGoodsInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 패키지 상태
        /// </summary>
        /// <returns></returns>
        public EveryDayPackageState GetEveryDayPackageState()
        {
            return shopModel.GetEveryDayPackageState(info.ID);
        }

        public int GetRemainCount()
        {
            RewardGroupData[] rewards = rewardGroupDataRepo.Gets(info.GetGroupId());
            int max = rewards.Max(x => x.condition_value);
            Debug.Log($"GetRemainDay={max}");
            Debug.Log($"GetRewardCount={shopModel.GetRewardCount(info.ID)}");

            // 마지막날 보상을 받을때 GetRewardCount가 -1로 들어온다
            if (shopModel.GetRewardCount(info.ID) == -1)
                return 0;

            return max - shopModel.GetRewardCount(info.ID);
        }

        /// <summary>
        /// 매일매일 보상 정보
        /// </summary>
        /// <returns></returns>
        public RewardGroupData[] GetEveryDayRewards()
        {
            return rewardGroupDataRepo.Gets(info.GetGroupId(), 1);
        }

        /// <summary>
        /// 상품 가격
        /// </summary>
        /// <returns></returns>
        public string GetPrice()
        {
            return info.CostText;
        }

        public void OnClickedBtnPurchase()
        {
            EveryDayPackageState state = GetEveryDayPackageState();

            switch (state)
            {
                case EveryDayPackageState.AvailablePurchase:
                    shopModel.RequestCashShopPurchase(info.ID, isShowNotiPopup: false).WrapNetworkErrors();
                    break;

                case EveryDayPackageState.StandByReward:
                    shopModel.RequestEveryDayGoodsReward(info.ID).WrapNetworkErrors();
                    break;
            }
        }
    }
}