using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOverStatus : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;

        [SerializeField] UIGrid overStatsGrid;
        [SerializeField] OverStatusElement[] overStats;
        [SerializeField] UIApplyStatBase applyStatBase;

        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UILabelHelper labelSuccessRate;

        [SerializeField] UILabelValue haveCoin;
        [SerializeField] UILabelValue spendCoin;

        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnOverStatus;

        OverStatusPresenter presenter;

        System.Action OnCloseUI;

        protected override void OnInit()
        {
            presenter = new OverStatusPresenter();

            presenter.OnOverStatusResult += OnOverStatusResult;

            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnCancel.OnClick, CloseUI);
            EventDelegate.Add(btnOverStatus.OnClick, OnClickedBtnOverStatus);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.OnOverStatusResult -= OnOverStatusResult;

            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnCancel.OnClick, CloseUI);
            EventDelegate.Remove(btnOverStatus.OnClick, OnClickedBtnOverStatus);

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
            labelMainTitle.LocalKey = LocalizeKey._6300; // 오버 스테이터스
            labelNotice.LocalKey = LocalizeKey._6301; // 오버 스탯을 성공하면 스테이터스 1포인트가 증가 됩니다.
            haveCoin.TitleKey = LocalizeKey._18501; // 보유 제니
            spendCoin.TitleKey = LocalizeKey._18502; // 소모 제니

            btnCancel.LocalKey = LocalizeKey._18503; // 나가기
            btnOverStatus.LocalKey = LocalizeKey._6303; // 스탯 강화

            overStats[0].Get().TitleKey = LocalizeKey._4004;
            overStats[1].Get().TitleKey = LocalizeKey._4005;
            overStats[2].Get().TitleKey = LocalizeKey._4006;
            overStats[3].Get().TitleKey = LocalizeKey._4007;
            overStats[4].Get().TitleKey = LocalizeKey._4008;
            overStats[5].Get().TitleKey = LocalizeKey._4009;
        }

        public void Set(BasicStatusType type)
        {
            presenter.Set(type);
            UpdateView();
        }

        public void Set(BasicStatusType type, bool isMeleeAttack, BattleStatusInfo previewStatus, BattleStatusInfo noBuffStatus, BattleStatusInfo previewWithNoBuffStatus, System.Action onAction)
        {
            OnCloseUI = onAction;

            presenter.Set(type);
            UpdateView();

            overStats[0].Get().SetStatValue(previewStatus.Str, noBuffStatus.Str, previewWithNoBuffStatus.Str - noBuffStatus.Str);
            overStats[1].Get().SetStatValue(previewStatus.Agi, noBuffStatus.Agi, previewWithNoBuffStatus.Agi - noBuffStatus.Agi);
            overStats[2].Get().SetStatValue(previewStatus.Vit, noBuffStatus.Vit, previewWithNoBuffStatus.Vit - noBuffStatus.Vit);
            overStats[3].Get().SetStatValue(previewStatus.Int, noBuffStatus.Int, previewWithNoBuffStatus.Int - noBuffStatus.Int);
            overStats[4].Get().SetStatValue(previewStatus.Dex, noBuffStatus.Dex, previewWithNoBuffStatus.Dex - noBuffStatus.Dex);
            overStats[5].Get().SetStatValue(previewStatus.Luk, noBuffStatus.Luk, previewWithNoBuffStatus.Luk - noBuffStatus.Luk);

            overStats[0].SetEffect(type == BasicStatusType.Str);
            overStats[1].SetEffect(type == BasicStatusType.Agi);
            overStats[2].SetEffect(type == BasicStatusType.Vit);
            overStats[3].SetEffect(type == BasicStatusType.Int);
            overStats[4].SetEffect(type == BasicStatusType.Dex);
            overStats[5].SetEffect(type == BasicStatusType.Luk);

            applyStatBase.SetStatView(isMeleeAttack, previewStatus, noBuffStatus, previewWithNoBuffStatus);
        }

        private void UpdateView()
        {
            SetSuccessRate();
            SetNeedZeny();
        }

        private void SetSuccessRate()
        {
            labelSuccessRate.Text = presenter.GetOverStateSuccessRate();
        }

        private void SetNeedZeny()
        {
            haveCoin.Value = presenter.GetHaveZeny().ToString("N0");
            spendCoin.Value = presenter.GetNeedZeny().ToString("N0");
        }

        private void OnClickedBtnOverStatus()
        {
            presenter.RequestOverStatus();
        }

        private void OnOverStatusResult(bool isSuccess)
        {
            Debug.Log($"오버스탯 결과={isSuccess}");

            if (isSuccess)
            {
                CloseUI();
            }
            else
            {
                UpdateView();
            }
        }

        void CloseUI()
        {
            OnCloseUI?.Invoke();
            UI.Close<UIOverStatus>();
        }

        protected override void OnBack()
        {
            OnCloseUI?.Invoke();

            base.OnBack();
        }

        bool IInspectorFinder.Find()
        {
            if (overStatsGrid != null)
            {
                overStats = overStatsGrid.GetComponentsInChildren<OverStatusElement>();
            }
            return true;
        }
    }
}