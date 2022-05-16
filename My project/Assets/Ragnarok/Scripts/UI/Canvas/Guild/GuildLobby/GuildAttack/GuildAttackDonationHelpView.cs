using UnityEngine;

namespace Ragnarok.View
{
    public class GuildAttackDonationHelpView : UIView
    {
        [SerializeField] PopupView donationPopupView;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelDescription;

        protected override void Awake()
        {
            base.Awake();
            donationPopupView.OnConfirm += Hide;
            donationPopupView.OnExit += Hide;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            donationPopupView.OnConfirm -= Hide;
            donationPopupView.OnExit -= Hide;
        }

        protected override void OnLocalize()
        {
            donationPopupView.MainTitleLocalKey = LocalizeKey._38417; // 길드 기부
            donationPopupView.ConfirmLocalKey = LocalizeKey._38419; // 확인
            labelDescription.LocalKey = LocalizeKey._38418; // 엠펠리움 결정을 기부하면 보상을 드려요!
        }

        public void SetData(RewardData reward)
        {
            rewardHelper.SetData(reward);
        }
    }
}