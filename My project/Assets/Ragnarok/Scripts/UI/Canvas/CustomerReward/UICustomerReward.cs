using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICustomerReward : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] CustomerRewardView normalRewardView;
        [SerializeField] CustomerRewardView premiumRewardView;
        [SerializeField] UILabelHelper labelNotice;

        CustomerRewardPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CustomerRewardPresenter();

            normalRewardView.Initialize(LocalizeKey._8202); // 감사리워드
            premiumRewardView.Initialize(LocalizeKey._8203); // 프리미엄

            simplePopupView.OnExit += CloseUI;
            normalRewardView.OnSelect += OnSelectNormalReward;
            premiumRewardView.OnSelect += OnSelectPremiumReward;
            presenter.OnUpdateCustomerRewardInfo += Refresh;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            simplePopupView.OnExit -= CloseUI;
            normalRewardView.OnSelect -= OnSelectNormalReward;
            premiumRewardView.OnSelect -= OnSelectPremiumReward;
            presenter.OnUpdateCustomerRewardInfo -= Refresh;

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
            simplePopupView.MainTitleLocalKey = LocalizeKey._8200; // 고객 보상
            labelNotice.LocalKey = LocalizeKey._8201; // 일일 퀘스트를 모두 완료하시면 광고를 통해 특별한 선물을 드립니다.\n(자정(GMT+8)에 초기화)
        }

        void OnSelectNormalReward()
        {
            presenter.RequestCustomerReward(CustomerRewardType.NORMAL);
        }

        void OnSelectPremiumReward()
        {
            presenter.RequestCustomerReward(CustomerRewardType.PREMIUM);
        }

        private void Refresh()
        {
            normalRewardView.SetData(presenter.GetInfo(CustomerRewardType.NORMAL));
            premiumRewardView.SetData(presenter.GetInfo(CustomerRewardType.PREMIUM));
        }

        private void CloseUI()
        {
            UI.Close<UICustomerReward>();
        }
    }
}