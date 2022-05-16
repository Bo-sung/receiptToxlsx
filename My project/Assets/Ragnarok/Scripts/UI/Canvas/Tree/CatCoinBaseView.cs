using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CatCoinBaseView : UISubCanvas<DailyCheckPresenter>, IAutoInspectorFinder, IInspectorFinder
    {
        [SerializeField] UILabelValue labelCumulativeTime;
        [SerializeField] UIButtonHelper btnGetRwerad;
        [SerializeField] CatCoinRewardSlot[] catCoinRewardSlots;
        [SerializeField] GameObject treePackImage;
        [SerializeField] GameObject goRoPointEvent;
        [SerializeField] UILabelHelper labelEventTitle;

        CatCoinRewardInfo[] arrayInfo;

        protected override void OnInit()
        {
            EventDelegate.Add(btnGetRwerad.OnClick, OnClickedBtnGetReward);
        }

        protected override void OnClose()
        {
            EventDelegate.Add(btnGetRwerad.OnClick, OnClickedBtnGetReward);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnLocalize()
        {
            labelCumulativeTime.TitleKey = LocalizeKey._9006; // 온라인 접속 시간: 
            btnGetRwerad.LocalKey = LocalizeKey._9009; // 보상 획득
        }

        protected override void OnHide() { }

        public void Refresh()
        {
            arrayInfo = presenter.GetCatCoinRewardInfos();
            RewardType rewardType = presenter.GetCatCoinTreeEventRewardType();
            int[] rewardCount = presenter.GetCatCoinTreeAddReward();
            bool isEvent = false;
            for (int i = catCoinRewardSlots.Length - 1; i >= 0; i--)
            {
                catCoinRewardSlots[i].SetData(presenter, arrayInfo[i]);
                catCoinRewardSlots[i].SetReward(rewardType, rewardCount[i]);
                if (rewardCount[i] > 0)
                    isEvent = true;
            }
            goRoPointEvent.SetActive(isEvent);
            Timing.RunCoroutineSingleton(YieldCumulativeTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);

            labelEventTitle.LocalKey = BasisType.CAT_COIN_TREE_BOTTOM_TEXT_SCRIPT.GetInt();
        }

        private IEnumerator<float> YieldCumulativeTime()
        {
            while (true)
            {
                labelCumulativeTime.Value = LocalizeKey._9007.ToText()
                    .Replace("{MINUTE}", presenter.DayConnectionTime.ToCumulativeTime().ToTotalMinute()); // {MINUTE}분

                btnGetRwerad.IsEnabled = presenter.IsCatCoinReward;

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = presenter.GetTreePackRemainTime();
                if (time <= 0)
                    break;

                treePackImage.SetActive(true);
                yield return Timing.WaitForSeconds(0.5f);
            }
            treePackImage.SetActive(false);
        }

        private void OnClickedBtnGetReward()
        {
            presenter.RequestGetConnectReward();
        }

        bool IInspectorFinder.Find()
        {
            CatCoinRewardSlot[] slots = GetComponentsInChildren<CatCoinRewardSlot>();
            catCoinRewardSlots = new CatCoinRewardSlot[slots.Length];
            for (int i = 0, index = slots.Length - 1; i < catCoinRewardSlots.Length; i++, index--)
            {
                catCoinRewardSlots[i] = slots[index];
            }
            return true;
        }
    }
}