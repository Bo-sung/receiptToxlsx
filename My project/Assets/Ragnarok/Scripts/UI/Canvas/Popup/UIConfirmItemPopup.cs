using UnityEngine;

namespace Ragnarok
{
    public sealed class UIConfirmItemPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void SetData(RewardData rewardData, string description)
        {
            rewardHelper.SetData(rewardData);
            labelDescription.Text = description;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._5; // 알림
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        private void CloseUI()
        {
            UI.Close<UIConfirmItemPopup>();
        }

        protected override void OnBack()
        {
            CloseUI();
        }
    }
}