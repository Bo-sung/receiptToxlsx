using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPackageShare : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRibbonTitle;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UILabelHelper labelRoPoint;
        [SerializeField] UILabelHelper labelSecretTicket;
        [SerializeField] UILabelHelper labelShareTicket;
        [SerializeField] UILabelHelper labelCatCoin;
        [SerializeField] UILabelHelper labelZeny;
        [SerializeField] UILabelHelper labelBonus;
        [SerializeField] UILabelHelper labelBonusDesc;
        [SerializeField] UILabelHelper labelAlways;
        [SerializeField] UILabelHelper labelBaseExp;
        [SerializeField] UILabelHelper labelJobExp;
        [SerializeField] UILabelHelper labelZenyValue;
        [SerializeField] UILabelHelper labelCoolingOff;
        [SerializeField] UIButtonhWithGrayScale btnPurchase;
        [SerializeField] UILabelHelper labelNotice;

        PackageSharePresenter presenter;

        protected override void OnInit()
        {
            presenter = new PackageSharePresenter();
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnClose()
        {
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
            labelTitle.LocalKey = LocalizeKey._3400; // [285190]공유 보상[-] [FFBF00]UP[-] [285190]팩[-]
            labelRibbonTitle.LocalKey = LocalizeKey._3401; // NEW
            labelRewardTitle.LocalKey = LocalizeKey._3402; // 구매 즉시 지급
            labelRoPoint.LocalKey = LocalizeKey._3403; // RO POINT\n20
            labelSecretTicket.LocalKey = LocalizeKey._3404; // 비밀 상점 갱신권\n2개
            labelShareTicket.LocalKey = LocalizeKey._3405; // 시계 16H\n1개
            labelCatCoin.LocalKey = LocalizeKey._3406; // 냥다래 200개
            labelZeny.LocalKey = LocalizeKey._3407; // 100,000 제니
            labelBonus.LocalKey = LocalizeKey._3409; // Bonus
            labelBonusDesc.LocalKey = LocalizeKey._3410; // 구매 시, 영구적으로 셰어 정산 보상
            labelAlways.LocalKey = LocalizeKey._3408; // 영구
            labelBaseExp.Text = presenter.GetShareUpRateText();
            labelJobExp.Text = presenter.GetShareUpRateText();
            labelZenyValue.Text = presenter.GetShareUpRateText();
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
            UI.Close<UIPackageShare>();
        }

        public void Set(int shopId)
        {
            presenter.SetShopId(shopId);

            Refresh();
        }

        void Refresh()
        {
            // 활성화 or 메일함에 보유중
            if (presenter.IsShareBuff() || presenter.HasMailShopItem())
            {
                btnPurchase.IsEnabled = false;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                btnPurchase.LocalKey = LocalizeKey._3414; // 구매 완료
            }
            else
            {
                btnPurchase.IsEnabled = true;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                btnPurchase.Text = presenter.GetPrice();
            }
        }
    }
}