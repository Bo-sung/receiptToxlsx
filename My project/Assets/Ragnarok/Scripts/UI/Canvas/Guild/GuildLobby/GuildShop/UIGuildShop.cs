using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildShop : UICanvas, ShopPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] GuildShopView guildShopView;

        ShopPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ShopPresenter(this);

            guildShopView.Initialize(presenter);

            titleView.Initialize(TitleView.FirstCoinType.GuildCoin, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
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
            titleView.ShowTitle(LocalizeKey._8061.ToText()); // 길드 상점
        }

        public void Refresh()
        {
            guildShopView.Show();
        }

        public void ShowShopDefault()
        {
        }

        public void ShowShopSecret()
        {
        }

        public void UpdateZeny(long zeny)
        {
        }

        public void UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        public void UpdateGuildCoin(long guildCoin)
        {
            titleView.ShowGuildCoin(guildCoin);
        }

        public void SetLockSecretShop(bool isActive)
        {
        }

        public void UpdatePackageTabNotice()
        {
        }

        public void UpdateDefaultTabNotice()
        {
        }

        public void UpdateMileageNotice()
        {
        }
    }
}