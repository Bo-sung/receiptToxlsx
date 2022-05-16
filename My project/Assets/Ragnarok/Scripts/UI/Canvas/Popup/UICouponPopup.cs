using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// CBT쿠폰 입력 기간에만 사용하고 없어질 예정임..
    /// </summary>
    public sealed class UICouponPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;
        public override int layer => Layer.UI_Popup;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescNormal;
        [SerializeField] UIInput inputNormal;
        [SerializeField] UILabelHelper labelInputDefaultNormal;
        [SerializeField] UIButtonHelper btnSubmitNormal;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelNotice01, labelNotice02;

        CouponPopupPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CouponPopupPresenter();

            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnSubmitNormal.OnClick, OnClickedNormalBtnSubmit);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnSubmitNormal.OnClick, OnClickedNormalBtnSubmit);
        }

        protected override void OnShow(IUIData data = null)
        {
            inputNormal.keyboardType = UIInput.KeyboardType.Default;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._3700; // 쿠폰
            labelDescNormal.LocalKey = LocalizeKey._90204; // 쿠폰코드를 입력해주세요.
            labelInputDefaultNormal.Text = default; // null
            btnSubmitNormal.LocalKey = LocalizeKey._1; // 확인
            labelNotice01.LocalKey = LocalizeKey._3705; // 모든 쿠폰의 대소문자를 구분하여 입력해주세요.
            labelNotice02.LocalKey = LocalizeKey._3706; // 모든 쿠폰은 계정 당 1회만 사용 가능합니다.
        }

        void OnClickedNormalBtnSubmit()
        {
            string code = inputNormal.value;

            if (string.IsNullOrEmpty(code))
                return;

            // 쿠폰보상
            presenter.RewardNormalCoupon(code);
            CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UICouponPopup>();
        }
    }
}