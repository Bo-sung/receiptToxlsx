using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICustomerReward"/>
    /// </summary>
    public sealed class CustomerRewardPresenter : ViewPresenter
    {
        private const int NORMAL_SHOP_REWARD_GROUP = 5;
        private const int PREMIUM_SHOP_REWARD_GROUP = 6;

        // <!-- Models --!>
        private readonly ShopModel shopModel;
        private readonly QuestModel questModel;

        // <!-- Repositories --!>
        private readonly RewardGroupInfo normalInfo;
        private readonly RewardGroupInfo premiumInfo;
        private readonly IronSourceManager ironSourceManager;

        private CustomerRewardType customerRewardType;

        // <!-- Event --!>
        public event System.Action OnUpdateCustomerRewardInfo
        {
            add { shopModel.OnUpdateCustomerRewardInfo += value; }
            remove { shopModel.OnUpdateCustomerRewardInfo -= value; }
        }

        public CustomerRewardPresenter()
        {
            shopModel = Entity.player.ShopModel;
            questModel = Entity.player.Quest;
            ironSourceManager = IronSourceManager.Instance;

            RewardGroupDataManager rewardGroupDataManager = RewardGroupDataManager.Instance;
            int normalGroupId = BasisType.SHOP_REWARD_GROUP_ID.GetInt(NORMAL_SHOP_REWARD_GROUP);
            int premiumGroupId = BasisType.SHOP_REWARD_GROUP_ID.GetInt(PREMIUM_SHOP_REWARD_GROUP);

            normalInfo = new RewardGroupInfo(rewardGroupDataManager.Gets(normalGroupId));
            premiumInfo = new RewardGroupInfo(rewardGroupDataManager.Gets(premiumGroupId));
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RequestCustomerReward(CustomerRewardType type)
        {
            if (!questModel.IsTotalClearQuest())
            {
                UI.ConfirmPopup(LocalizeKey._8211.ToText()); // 일일 퀘스트를 모두 완료해야 보상을 받을 수 있습니다.
                return;
            }

            if (shopModel.GetCustomerRemainTime(type) > 0L)
            {
                UI.ShowToastPopup(LocalizeKey._8210.ToText()); // 일정 시간 이후에 보상 획득이 가능합니다.
                return;
            }

            customerRewardType = type;
            bool isFree = shopModel.HasSumPaymentType(type);
            ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.None, isFree, isBeginner: false, OnCompleteRewardVideo);
        }

        private void OnCompleteRewardVideo()
        {
            shopModel.RequestCustomerReward(customerRewardType).WrapNetworkErrors();
        }

        public CustomerRewardView.IInput GetInfo(CustomerRewardType type)
        {
            switch (type)
            {
                case CustomerRewardType.NORMAL:
                    normalInfo.SetMainStep(shopModel.GetCustomerRewardStep(type));
                    normalInfo.SetRemainTime(shopModel.GetCustomerRemainTime(type));
                    return normalInfo;

                case CustomerRewardType.PREMIUM:
                    premiumInfo.SetMainStep(shopModel.GetCustomerRewardStep(type));
                    premiumInfo.SetRemainTime(shopModel.GetCustomerRemainTime(type));
                    return premiumInfo;
            }

            return null;
        }

        private class RewardGroupInfo : CustomerRewardView.IInput
        {
            private readonly RewardInfo[] infos;
            public UICustomerRewardSlot.IInput[] Inputs => infos;
            public RemainTime RemainTime { get; private set; }
            public bool IsAllReceived { get; private set; }

            public RewardGroupInfo(RewardGroupData[] arrData)
            {
                int length = arrData == null ? 0 : arrData.Length;
                infos = new RewardInfo[length];

                for (int i = 0; i < length; i++)
                {
                    infos[i] = new RewardInfo(i, arrData[i].rewardData);
                }
            }

            public void SetRemainTime(RemainTime remainTime)
            {
                RemainTime = remainTime;
            }

            public void SetMainStep(int mainStep)
            {
                for (int i = 0; i < Inputs.Length; i++)
                {
                    infos[i].SetMainStep(mainStep);
                }

                IsAllReceived = infos.Length == mainStep;
            }

            private class RewardInfo : UICustomerRewardSlot.IInput
            {
                public int Step { get; }
                public RewardData Reward { get; }
                public int MainStep { get; private set; }

                public RewardInfo(int index, RewardData reward)
                {
                    Step = index;
                    Reward = reward;
                }

                public void SetMainStep(int mainStep)
                {
                    MainStep = mainStep;
                }
            }
        }
    }
}