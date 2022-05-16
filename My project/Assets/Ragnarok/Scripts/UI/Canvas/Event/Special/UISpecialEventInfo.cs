using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UISpecialEventInfo : UIView
    {
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelCondition;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelProgress;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UIButtonHelper btnComplete;
        [SerializeField] GameObject completeBase;
        [SerializeField] GameObject fxComplete;

        CatCoinGiftData catCoinGiftData;

        public event System.Action<int> OnClickedConfirm;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnComplete.OnClick, OnClickedBtnComplete);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnComplete.OnClick, OnClickedBtnComplete);
        }

        private void OnDisable()
        {
            NGUITools.SetActive(fxComplete, false);
        }

        protected override void OnLocalize()
        {
            labelCondition.LocalKey = LocalizeKey._5419; // 일일 퀘스트 모두 클리어
            btnComplete.LocalKey = LocalizeKey._5421; // 보상 획득
        }

        public void SetData(CatCoinGiftData data)
        {
            this.catCoinGiftData = data;

            OnClickedConfirm = catCoinGiftData.OnReward;

            Refresh();
        }

        private void Refresh()
        {
            if (catCoinGiftData == null)
            {
                return;
            }

            var id = catCoinGiftData.Id;
            var curVal = catCoinGiftData.CurrentCount;
            var maxVal = catCoinGiftData.CompleteCount;
            var rewardData = catCoinGiftData.RewardData;
            var canReward = catCoinGiftData.CanReward;

            var clearCount = canReward ? curVal + 1 : curVal;
            var isComplete = curVal >= maxVal;

            labelDesc.Text = LocalizeKey._5418.ToText() // {INDEX}. 일일 퀘스트 모두 {COUNT}회 클리어
                .Replace(ReplaceKey.INDEX, id)
                .Replace(ReplaceKey.COUNT, maxVal);

            reward.SetData(rewardData);
            btnComplete.IsEnabled = canReward && clearCount == maxVal;
            completeBase.SetActive(isComplete);

            if (isComplete)
            {
                progressBar.value = MathUtils.GetProgress(maxVal, maxVal);
                labelProgress.Text = $"({maxVal}/{maxVal})";
            }
            else
            {
                progressBar.value = MathUtils.GetProgress(clearCount, maxVal);
                labelProgress.Text = $"({clearCount}/{maxVal})";
            }
        }

        private void OnClickedBtnComplete()
        {
            OnClickedConfirm?.Invoke(catCoinGiftData.Id);

            NGUITools.SetActive(fxComplete, false);
            NGUITools.SetActive(fxComplete, true);
        }
    }
}