using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIShop : UICanvas, ShopPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        public enum ViewType
        {
            Default = 0,
            Secret = 1,
        }

        [SerializeField] TitleView titleView;
        [SerializeField] ShopDefaultView shopDefault;
        [SerializeField] ShopSecretView shopSecret;
        [SerializeField] GameObject lockSecretShop;

        [SerializeField] UIGrid gridSub;
        [SerializeField] UIButtonHelper btnCustomerReward;
        [SerializeField] UIButtonHelper btnFirstPayment;
        [SerializeField] UIButtonHelper btnMileage;

        [SerializeField] UIGrid gridPass;
        [SerializeField] UIButtonWithIconHelper btnOnBuffPass;
        [SerializeField] UIButtonWithIconHelper btnPass;

        ShopPresenter presenter;
        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new ShopPresenter(this);

            shopDefault.Initialize(presenter);
            shopSecret.Initialize(presenter);

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.OnStandByReward += UpdateOnBuffPassNotice;
            presenter.OnStandByReward += UpdatePassNotice;
            presenter.AddUpdatePassExpEvent(PassType.OnBuff, UpdateOnBuffPassNotice);
            presenter.AddUpdatePassExpEvent(PassType.Labyrinth, UpdatePassNotice);
            presenter.AddUpdatePassRewardEvent(PassType.OnBuff, UpdateOnBuffPassNotice);
            presenter.AddUpdatePassRewardEvent(PassType.Labyrinth, UpdatePassNotice);

            presenter.AddEvent();

            EventDelegate.Add(btnMileage.OnClick, OnClickedBtnMileage);
            EventDelegate.Add(btnFirstPayment.OnClick, OnClickedBtnFirstPayment);
            EventDelegate.Add(btnCustomerReward.OnClick, OnClickedBtnCustomerReward);
            EventDelegate.Add(btnOnBuffPass.OnClick, OnClickedBtnOnBuffPass);
            EventDelegate.Add(btnPass.OnClick, OnClickedBtnPass);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnStandByReward -= UpdateOnBuffPassNotice;
            presenter.OnStandByReward -= UpdatePassNotice;
            presenter.RemoveUpdatePassExpEvent(PassType.OnBuff, UpdateOnBuffPassNotice);
            presenter.RemoveUpdatePassExpEvent(PassType.Labyrinth, UpdatePassNotice);
            presenter.RemoveUpdatePassRewardEvent(PassType.OnBuff, UpdateOnBuffPassNotice);
            presenter.RemoveUpdatePassRewardEvent(PassType.Labyrinth, UpdatePassNotice);

            EventDelegate.Remove(btnMileage.OnClick, OnClickedBtnMileage);
            EventDelegate.Remove(btnFirstPayment.OnClick, OnClickedBtnFirstPayment);
            EventDelegate.Remove(btnCustomerReward.OnClick, OnClickedBtnCustomerReward);
            EventDelegate.Remove(btnOnBuffPass.OnClick, OnClickedBtnOnBuffPass);
            EventDelegate.Remove(btnPass.OnClick, OnClickedBtnPass);
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateMileageNotice();
            UpdateOnBuffPassNotice();
            UpdatePassNotice();

            RefreshPass();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._12006.ToText()); // 상점
            btnMileage.LocalKey = LocalizeKey._8048; // 누적충전
            btnFirstPayment.LocalKey = LocalizeKey._8049; // 첫 결제
            btnCustomerReward.LocalKey = LocalizeKey._8064; // 고객보상
            btnOnBuffPass.LocalKey = LocalizeKey._8069; // OnBuff 패스
            btnPass.LocalKey = LocalizeKey._8068; // 라비린스 패스
        }

        public void Set(ViewType viewType, ShopTabType tabType = ShopTabType.Package)
        {
            switch (viewType)
            {
                case ViewType.Default:
                    ShowShopDefault();
                    shopDefault.Set(tabType);
                    break;

                case ViewType.Secret:
                    ShowShopSecret();
                    break;
            }
        }

        public void Refresh()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();

            btnFirstPayment.SetActive(!presenter.HasPaymentHistory());
            btnCustomerReward.SetActive(presenter.HasPaymentHistory());
            gridSub.Reposition();
        }

        private void RefreshPass()
        {
            btnOnBuffPass.SetActive(presenter.IsDisplayPass(PassType.OnBuff));
            btnPass.SetActive(presenter.IsDisplayPass(PassType.Labyrinth));
            gridPass.Reposition();
        }

        public void UpdatePackageTabNotice()
        {
            shopDefault.UpdatePackageTabNotice();
        }

        public void UpdateDefaultTabNotice()
        {
            shopDefault.UpdateDefaultTabNotice(); // Refresh Tab Notice
        }

        public void UpdateMileageNotice()
        {
            bool hasNotice = presenter.GetHasMileageNotice();
            btnMileage.SetNotice(hasNotice);
        }

        void UpdateOnBuffPassNotice()
        {
            btnOnBuffPass.SetNotice(presenter.IsPassNotice(PassType.OnBuff));
        }

        void UpdatePassNotice()
        {
            btnPass.SetNotice(presenter.IsPassNotice(PassType.Labyrinth));
        }

        private void ShowSubCanvas(UISubCanvas subCanvas, bool force = false)
        {
            if (currentSubCanvas == subCanvas && !force)
                return;
            currentSubCanvas = subCanvas;
            HideAllSubCanvas();
            Refresh();
        }

        public async void ShowShopDefault()
        {
            // 호출 전 미리 세팅
            ShowSubCanvas(shopDefault);

            if (!await presenter.RequestItemShopLimitList())
                return;

            ShowSubCanvas(shopDefault);
            presenter.SetLockSecretShop();

            UpdatePackageTabNotice();
            UpdateDefaultTabNotice();
        }

        public void ShowShopSecret()
        {
            ShowSubCanvas(shopSecret);
        }

        void ShopPresenter.IView.UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        void ShopPresenter.IView.UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        void ShopPresenter.IView.UpdateGuildCoin(long guildCoin)
        {
        }

        void ShopPresenter.IView.SetLockSecretShop(bool isActive)
        {
            lockSecretShop.SetActive(isActive);
        }

        void OnClickedBtnMileage()
        {
            UI.Show<UIMileage>();
        }

        void OnClickedBtnFirstPayment()
        {
            UI.Show<UIPackageFirstPayment>();
        }

        void OnClickedBtnCustomerReward()
        {
            UI.Show<UICustomerReward>();
        }

        void OnClickedBtnOnBuffPass()
        {
            if (presenter.IsBattlePass(PassType.OnBuff))
            {
                UI.Show<UIOnBuffPass>();
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._90323.ToText()); // ONBUFF 패스 시즌 기간이 아닙니다.
            }
        }

        void OnClickedBtnPass()
        {
            if (presenter.IsBattlePass(PassType.Labyrinth))
            {
                UI.Show<UIPass>();
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._90317.ToText()); // 라비린스 패스 시즌 기간이 아닙니다.
            }
        }

        protected override void OnBack()
        {
            UI.Close<UIShop>();
        }
    }
}