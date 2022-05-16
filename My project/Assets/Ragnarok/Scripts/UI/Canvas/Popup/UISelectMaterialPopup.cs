using UnityEngine;

namespace Ragnarok
{
    public sealed class UISelectMaterialPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;        

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnAllow;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelNeed;
        [SerializeField] UILabelHelper labelNeedValue;
        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;

        private event System.Action onAllow;

        protected override void OnInit()
        {
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnCancel.OnClick, CloseUI);
            EventDelegate.Add(btnAllow.OnClick, OnClickedBtnAllow);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnCancel.OnClick, CloseUI);
            EventDelegate.Remove(btnAllow.OnClick, OnClickedBtnAllow);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._2900; // 알림
            btnCancel.LocalKey = LocalizeKey._2901; // 취소
            btnConfirm.LocalKey = LocalizeKey._2902; // 확인
            labelNeed.LocalKey = LocalizeKey._2903; // 필요 갯수
            labelOwned.LocalKey = LocalizeKey._2904; // 보유 갯수
        }

        private void CloseUI()
        {
            UI.Close<UISelectMaterialPopup>();
        }

        public void Set(RewardData rewardData, int myCount, int needCount, string text, string btnAllowText, bool isAllow, System.Action OnSelectAllow)
        {
            rewardHelper.SetData(rewardData);
            btnAllow.Text = btnAllowText;

            bool hasMaterial = myCount >= needCount;

            labelNeedValue.Text = needCount.ToString("N0");
            if (hasMaterial)
            {
                labelOwnedValue.Text = $"[4C4A4D]{myCount:N0}[-]";
            }
            else
            {
                labelOwnedValue.Text = $"[D76251]{myCount:N0}[-]";
            }

            btnCancel.SetActive(isAllow);
            btnAllow.SetActive(isAllow);
            btnConfirm.SetActive(!isAllow);

            labelDescription.Text = text;
            onAllow = OnSelectAllow;
        }

        void OnClickedBtnAllow()
        {
            CloseUI();
            onAllow?.Invoke();
        }
    }
}