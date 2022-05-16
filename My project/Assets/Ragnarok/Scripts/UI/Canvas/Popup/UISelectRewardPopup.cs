using UnityEngine;

namespace Ragnarok
{
    public sealed class UISelectRewardPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnConfirm;

        private event System.Action onConfirm;

        protected override void OnInit()
        {
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnCancel.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnCancel.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._5800; // 알림
            btnCancel.LocalKey = LocalizeKey._5801; // 취소
            btnConfirm.LocalKey = LocalizeKey._5802; // 확인
        }

        private void CloseUI()
        {
            UI.Close<UISelectRewardPopup>();
        }

        public void Set(RewardData rewardData, string text, System.Action OnSelectConfirm)
        {
            rewardHelper.SetData(rewardData);
            labelDescription.Text = text;
            onConfirm = OnSelectConfirm;
        }

        void OnClickedBtnConfirm()
        {
            CloseUI();
            onConfirm?.Invoke();
            onConfirm = null;
        }
    }
}