using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGateReward : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] GateRewardView gateRewardView;

        GateRewardPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GateRewardPresenter();

            gateRewardView.OnConfirm += OnBack;
            gateRewardView.OnExit += OnBack;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            gateRewardView.OnConfirm -= OnBack;
            gateRewardView.OnExit -= OnBack;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            UpdateMainTitleText();
            gateRewardView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }

        public void Show(int gateId)
        {
            presenter.SetGateId(gateId);

            gateRewardView.SetData(presenter.GetRewards());
            UpdateMainTitleText();
        }

        private void UpdateMainTitleText()
        {
            gateRewardView.MainTitle = LocalizeKey._6951.ToText() // {NAME} 보상
                .Replace(ReplaceKey.NAME, presenter.GetGateName());
        }
    }
}