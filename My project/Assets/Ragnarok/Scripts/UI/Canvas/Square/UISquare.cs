using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISquare : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single | UIType.Reactivation;

        [SerializeField] TitleView titleView;
        [SerializeField] UIButtonWithIconValueHelper btnLobby;
        [SerializeField] UIButtonWithIconValueHelper btnKafraDelivery;
        [SerializeField] UIButtonWithIconValueHelper btnExchangeShop;
        [SerializeField] UIButtonWithIconValueHelper btnNabiho;

        SquarePresenter presenter;

        protected override void OnInit()
        {
            presenter = new SquarePresenter();

            presenter.OnUpdateZeny += titleView.ShowZeny;
            presenter.OnUpdateCatCoin += titleView.ShowCatCoin;
            presenter.OnUpdateKafra += UpdateKafraDeliveryNotice;
            presenter.OnUpateNabiho += UpateNabihoNotice;

            EventDelegate.Add(btnLobby.OnClick, presenter.OnClickedBtnLobby);
            EventDelegate.Add(btnKafraDelivery.OnClick, presenter.OnClickedBtnKafraDelivery);
            EventDelegate.Add(btnExchangeShop.OnClick, presenter.OnClickedBtnExchangeShop);
            EventDelegate.Add(btnNabiho.OnClick, presenter.OnClickedBtnNabiho);

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateZeny -= titleView.ShowZeny;
            presenter.OnUpdateCatCoin -= titleView.ShowCatCoin;
            presenter.OnUpdateKafra -= UpdateKafraDeliveryNotice;
            presenter.OnUpateNabiho -= UpateNabihoNotice;

            EventDelegate.Remove(btnLobby.OnClick, presenter.OnClickedBtnLobby);
            EventDelegate.Remove(btnKafraDelivery.OnClick, presenter.OnClickedBtnKafraDelivery);
            EventDelegate.Remove(btnExchangeShop.OnClick, presenter.OnClickedBtnExchangeShop);
            EventDelegate.Remove(btnNabiho.OnClick, presenter.OnClickedBtnNabiho);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent();
            btnNabiho.SetActive(presenter.IsOpenNabiho());

            UpdateKafraDeliveryNotice();
            UpateNabihoNotice();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._10800.ToText()); // 광장
            btnLobby.LocalKey = LocalizeKey._10801; // 노점
            btnLobby.SetValue(LocalizeKey._10802.ToText()); // 아이템을 사고 팔 수 있는 공간입니다.
            btnKafraDelivery.LocalKey = LocalizeKey._10803; // 카프라 운송
            btnKafraDelivery.SetValue(LocalizeKey._10804.ToText()); // 소린의 부탁을 들어주세요!\n퀘스트 보상으로\nRo Point와 제니를 획득할 수 있습니다.
            btnExchangeShop.LocalKey = LocalizeKey._10805; // 카프라 교환소
            btnExchangeShop.SetValue(LocalizeKey._10806.ToText()); // 테일링에게 카프라 아이템을\n가져가 보세요!\n다양한 아이템으로 교환할 수 있습니다.
            btnNabiho.LocalKey = LocalizeKey._10807; // 나비호
            btnNabiho.SetValue(LocalizeKey._10808.ToText()); // 원하는 아이템을 의뢰해보세요!\n랭크5 장비를 무료로\n획득할 수 있습니다.
        }

        private void UpateNabihoNotice()
        {
            btnNabiho.SetNotice(presenter.IsNoticeNabiho());
        }

        private void UpdateKafraDeliveryNotice()
        {
            btnKafraDelivery.SetNotice(presenter.IsNoticeKafraDelivery());
        }
    }
}