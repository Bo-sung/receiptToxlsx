using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 레벨엡 패키지, 시나리오 패키지 사용
    /// </summary>
    public sealed class UIPackageAchieve : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRewardDesc;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonhWithGrayScale btnPurchase;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelCoolingOff;

        PackageAchievePresenter presenter;

        private PackageAchieveSlot.IInput[] arrData;

        protected override void OnInit()
        {
            presenter = new PackageAchievePresenter();

            presenter.OnRewardPackageAchieve += Refresh;
            presenter.OnPucharseSuccess += Refresh;

            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            presenter.OnRewardPackageAchieve -= Refresh;
            presenter.OnPucharseSuccess -= Refresh;

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
            UI.Close<UIPackageAchieve>();
        }

        public void Set(int shopId)
        {
            presenter.SetShopId(shopId);
            Refresh();
        }

        private void Refresh()
        {
            arrData = presenter.GetInputs();
            wrapper.Resize(arrData.Length);

            labelTitle.Text = presenter.GetTitle();
            labelRewardDesc.Text = presenter.GetDescription();
            labelDescription.Text = LocalizeKey._3912.ToText() // 구매즉시 냥다래 {VALUE} 지급
                .Replace(ReplaceKey.VALUE, presenter.GetImmediateRewardCount());

            // 활성화 or 메일함에 보유중
            if (presenter.IsActivation() || presenter.HasMailShopItem())
            {
                btnPurchase.IsEnabled = false;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                btnPurchase.LocalKey = LocalizeKey._3908; // 구매 완료
            }
            else
            {
                btnPurchase.IsEnabled = true;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                btnPurchase.Text = presenter.GetPrice();
            }
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            PackageAchieveSlot ui = go.GetComponent<PackageAchieveSlot>();
            ui.Set(index, arrData[index], presenter);
        }
    }
}