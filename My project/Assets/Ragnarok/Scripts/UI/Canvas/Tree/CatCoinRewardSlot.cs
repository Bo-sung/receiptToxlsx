using UnityEngine;

namespace Ragnarok
{
    public sealed class CatCoinRewardSlot : UIInfo<DailyCheckPresenter, CatCoinRewardInfo>, IAutoInspectorFinder
    {
        [SerializeField] CatCoinRewardSubSlot listBase;
        [SerializeField] CatCoinRewardSubSlot currentBase;

        [SerializeField] GameObject completeBase;
        [SerializeField] GameObject remainBase;
        [SerializeField] GameObject fx;
        [SerializeField] GameObject goRoPointBase;
        [SerializeField] UILabel labelCount;
        [SerializeField] UITextureHelper icon;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            CatCoinRewardInfo.CatCoinCompleteType completeType = info.CompleteType;

            listBase.SetData(info);
            currentBase.SetData(info);

            switch (completeType)
            {
                case CatCoinRewardInfo.CatCoinCompleteType.StandByReward:
                    listBase.SetActive(false);
                    currentBase.SetActive(true);
                    completeBase.SetActive(false);
                    fx.SetActive(true);
                    break;
                case CatCoinRewardInfo.CatCoinCompleteType.InProgress:
                    listBase.SetActive(true);
                    currentBase.SetActive(false);
                    completeBase.SetActive(false);
                    break;
                case CatCoinRewardInfo.CatCoinCompleteType.ReceivedReward:
                    listBase.SetActive(false);
                    currentBase.SetActive(true);
                    completeBase.SetActive(true);
                    fx.SetActive(false);
                    break;
            }
        }

        public void SetReward(RewardType rewardType, int roPoint)
        {
            if (roPoint > 0)
            {
                icon.SetItem(rewardType.IconName());
                goRoPointBase.SetActive(true);
                labelCount.text = $"x{roPoint}";
            }
            else
            {
                goRoPointBase.SetActive(false);
            }
        }
    }
}
