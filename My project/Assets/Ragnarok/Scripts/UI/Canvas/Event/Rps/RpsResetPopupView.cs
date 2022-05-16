using UnityEngine;

namespace Ragnarok.View
{
    public sealed class RpsResetPopupView : UIView
    {
        [SerializeField] UILabelHelper labelResetTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelHelper labelCurrentReward;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelCost;
        [SerializeField] UITextureHelper iconCost;
        [SerializeField] UILabelHelper labelResetWarning;
        [SerializeField] UIButtonHelper btnConfirmReset;

        public event System.Action OnReset;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnConfirmReset.OnClick, OnClickedBtnConfirmReset);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnConfirmReset.OnClick, OnClickedBtnConfirmReset);
        }

        protected override void OnLocalize()
        {
            labelResetTitle.LocalKey = LocalizeKey._11204; // 초기화 안내
            labelCurrentReward.LocalKey = LocalizeKey._11205; // 현재 보상
            labelResetWarning.LocalKey = LocalizeKey._11206; // [c][2664EF]ROUND 1[-][/c]로 되돌아갑니다. 정말 초기화 하시겠습니까?
            btnConfirmReset.LocalKey = LocalizeKey._11207; // 초기화
        }

        void OnClickedBtnConfirmReset()
        {
            OnReset?.Invoke();
            Hide();
        }

        public void Show(RewardData reward, string iconName, int itemCount)
        {
            Show();

            rewardHelper.SetData(reward);
            labelCost.Text = itemCount.ToString("N0");
            iconCost.SetItem(iconName);
        }
    }
}