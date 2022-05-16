using System.Collections.Generic;

namespace Ragnarok
{
    public class PackageFirstPaymentPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly ShopModel shopModel;

        /******************** Repositories ********************/
        RewardGroupDataManager rewardGroupDataRepo;

        /******************** Event ********************/

        private ShopInfo info;

        public PackageFirstPaymentPresenter()
        {
            shopModel = Entity.player.ShopModel;
            rewardGroupDataRepo = RewardGroupDataManager.Instance;
            info = shopModel.GetInfo(BasisShop.FirtstPayment.GetID());           
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
        /// 상품 아이콘 이름
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            return info.IconName;
        }

        /// <summary>
        /// 보상 정보 목록
        /// </summary>
        /// <returns></returns>
        public RewardData[] GetRewards()
        {
            int rewardGroupId = info.GetAddGoodsReward().RewardValue;
            RewardGroupData[] data = rewardGroupDataRepo.Gets(rewardGroupId);

            List<RewardData> rewardList = new List<RewardData>();
            foreach (var item in data.OrEmptyIfNull())
            {
                rewardList.Add(item.GetReward());
            }
            rewardList.Add(new RewardData(RewardType.InvenWeight, 1, 0)); // 첫결제 보상에 가방 증가 보상 추가

            return rewardList.ToArray();
        }
    }
}