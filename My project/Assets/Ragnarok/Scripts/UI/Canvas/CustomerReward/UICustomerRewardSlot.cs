using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UICustomerRewardSlot : UIElement<UICustomerRewardSlot.IInput>
    {
        public interface IInput
        {
            int Step { get; }
            RewardData Reward { get; }
            int MainStep { get; }
        }

        [SerializeField] UILabelHelper labelStep;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject goComplete;
        [SerializeField] UIButtonHelper btnConfirm;

        public event System.Action OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            labelStep.Text = LocalizeKey._8204.ToText() // {INDEX}단계
                .Replace(ReplaceKey.INDEX, info.Step + 1);

            rewardHelper.SetData(info.Reward);
            bool isComplete = info.Step < info.MainStep;
            NGUITools.SetActive(goComplete, isComplete);
            btnConfirm.IsEnabled = info.Step == info.MainStep;
            btnConfirm.LocalKey = isComplete ? LocalizeKey._8206 : LocalizeKey._8205; // 보상받기
        }

        void OnClickedBtnConfirm()
        {
            if (info == null)
                return;

            OnSelect?.Invoke();
        }
    }
}