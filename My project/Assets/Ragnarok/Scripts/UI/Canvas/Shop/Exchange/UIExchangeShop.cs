using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIExchangeShop : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] ExchangeShopView exchangeShopView;

        ExchangeShopPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ExchangeShopPresenter();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.RoPoint);

            exchangeShopView.OnSelect += presenter.RequestExchange;

            presenter.OnUpdateZeny += titleView.ShowZeny;
            presenter.OnUpdateRoPoint += titleView.ShowRoPoint;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            exchangeShopView.OnSelect -= presenter.RequestExchange;

            presenter.OnUpdateZeny -= titleView.ShowZeny;
            presenter.OnUpdateRoPoint -= titleView.ShowRoPoint;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._8065.ToText()); // 카프라 교환소
        }

        private void Refresh()
        {
            exchangeShopView.SetData(presenter.GetArrayData());
        }
    }
}