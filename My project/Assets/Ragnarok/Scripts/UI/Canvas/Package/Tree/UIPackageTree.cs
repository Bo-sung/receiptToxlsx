using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPackageTree : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRewardDesc;
        [SerializeField] UILabelHelper labelRewardRate;
        [SerializeField] UILabelHelper labelCoolingOff;
        [SerializeField] UIButtonhWithGrayScale btnPurchase;
        [SerializeField] UILabelHelper labelNotice;

        PackageTreePresenter presenter;

        protected override void OnInit()
        {
            presenter = new PackageTreePresenter();

            presenter.OnPucharseSuccess += CloseUI;

            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnClose()
        {
            presenter.OnPucharseSuccess -= CloseUI;

            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelRewardRate.LocalKey = LocalizeKey._5101; // 2배!
            labelNotice.LocalKey = LocalizeKey._90295; // 해당 상품은 우편함에서 수령 시 바로 사용됩니다.

            if (GameServerConfig.IsKoreaLanguage())
            {
                const int LOCAL_KEY = LocalizeKey._90302; // 구매하는 상품의 수량 및 금액을 반드시 확인하시기 바랍니다.\n유료재화로 구매된 상품은 구매 후 7일 이내에는 청약철회가 가능합니다.\n상품을 사용하였거나 즉시 적용되는 상품의 경우, 추가 혜택이 사용된 경우 청약 철회 대상에서 제외됩니다.\n(유료 재화부터 우선 소모 됩니다.)
                labelCoolingOff.Text = BasisUrl.KoreanPurchaseTerms.AppendText(LOCAL_KEY.ToText());
            }
            else
            {
                labelCoolingOff.LocalKey = LocalizeKey._90220; // 구매한 아이템은 우편함으로 지급 됩니다.\n우편함에서 해당 상품의 구성품을 수령 할 경우, 청약 철회가 불가능합니다.
            }
        }

        void CloseUI()
        {
            UI.Close<UIPackageTree>();
        }

        public void Set(int shopId)
        {
            presenter.SetShopId(shopId);
            Refresh();
        }

        private void Refresh()
        {
            labelTitle.Text = presenter.GetTitle();
            labelRewardDesc.Text = presenter.GetDescription();

            // 구매 가능 여부
            if (presenter.CanBuy())
            {
                btnPurchase.IsEnabled = true;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                btnPurchase.Text = presenter.GetPrice();
            }
            else
            {
                btnPurchase.IsEnabled = false;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                btnPurchase.LocalKey = LocalizeKey._5100; // 구매완료
            }
        }

    }
}