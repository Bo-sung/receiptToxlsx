using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIKafraDelivery : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] private UILabelHelper labelMainTitle;
        [SerializeField] private KafraSelectView kafraSelectView;
        [SerializeField] private KafraListView kafraListView;
        [SerializeField] private KafraInProgressView kafraInProgressView;

        private KafraDeliveryPresenter presenter;

        protected override void OnInit()
        {
            presenter = new KafraDeliveryPresenter();

            kafraSelectView.OnSelect += OnKafraListView;
            kafraListView.OnAccept += OnAccept;

            presenter.OnUpdateRewardCount += OnUpdateRewardCount;
            presenter.OnUpdateKafra += OnUpdateKafra;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            kafraSelectView.OnSelect -= OnKafraListView;
            kafraListView.OnAccept -= OnAccept;

            presenter.OnUpdateRewardCount -= OnUpdateRewardCount;
            presenter.OnUpdateKafra -= OnUpdateKafra;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._19100; // 카프라운송
        }

        protected override void OnBack()
        {
            if (kafraListView.IsShow)
            {
                kafraListView.Hide();
                kafraSelectView.Show();
                return;
            }
            base.OnBack();
        }

        void UpdateView()
        {
            KafraCompleteType kafraCompleteType = presenter.GetKafraCompleteType();
            if (kafraCompleteType == KafraCompleteType.None)
            {
                kafraSelectView.Show();
                kafraListView.Hide();
                kafraInProgressView.Hide();
            }
            else
            {
                kafraSelectView.Hide();
                kafraListView.Hide();
                kafraInProgressView.Set(presenter.GetKafraType(), presenter.GetArrayData());
                kafraInProgressView.Show();
            }
        }

        private void OnKafraListView(KafraType kafraType)
        {
            kafraSelectView.Hide();
            kafraInProgressView.Hide();
            kafraListView.Set(kafraType, presenter.GetArrayData(kafraType));
            kafraListView.Show();
        }

        private void OnUpdateRewardCount()
        {
            kafraListView.UpdateRewardCount(presenter.GetRewardCount());
        }

        private void OnAccept()
        {
            presenter.AsyncRequestKafraDelivery().WrapNetworkErrors();
        }

        private void OnUpdateKafra()
        {
            if (presenter.GetKafraCompleteType() == KafraCompleteType.InProgress)
                CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIKafraDelivery>();
        }
    }
}