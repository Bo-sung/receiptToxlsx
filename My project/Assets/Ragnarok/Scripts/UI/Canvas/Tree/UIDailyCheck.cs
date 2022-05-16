using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDailyCheck : UICanvas, DailyCheckPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum TabType
        {
            Event = 0,
            Tree = 1,
            DailyCheck = 2,
        }

        public static TabType tabType = TabType.Event;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UITabHelper tabTop;
        [SerializeField] UIEventBanner eventBannerView;
        [SerializeField] DailyCheckView dailyCheckView;
        [SerializeField] TreeBaseView treeBaseView;

        DailyCheckPresenter presenter;
        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new DailyCheckPresenter(this);

            dailyCheckView.Initialize(presenter);
            treeBaseView.Initialize(presenter);

            presenter.OnUpdateEventQuest += OnUpdateEventQuestTab;
            presenter.OnUpdateEventNotice += OnUpdateEventQuestTab;
            presenter.OnUpdateWordCollectionItemCount += OnUpdateEventQuestTab;

            presenter.AddEvent();
            EventDelegate.Add(btnClose.OnClick, CloseUI);
            EventDelegate.Add(tabTop[0].OnChange, ShowEventView);
            EventDelegate.Add(tabTop[1].OnChange, ShowTreeBaseView);
            EventDelegate.Add(tabTop[2].OnChange, ShowDailyCheckView);
        }

        protected override void OnClose()
        {
            presenter.OnUpdateEventQuest -= OnUpdateEventQuestTab;
            presenter.OnUpdateEventNotice -= OnUpdateEventQuestTab;
            presenter.OnUpdateWordCollectionItemCount -= OnUpdateEventQuestTab;

            presenter.RemoveEvent();
            EventDelegate.Remove(btnClose.OnClick, CloseUI);
            EventDelegate.Remove(tabTop[0].OnChange, ShowEventView);
            EventDelegate.Remove(tabTop[1].OnChange, ShowTreeBaseView);
            EventDelegate.Remove(tabTop[2].OnChange, ShowDailyCheckView);
        }

        protected override void OnShow(IUIData data = null)
        {
            for (int i = 0; i < tabTop.Count; ++i)
                tabTop[i].Value = false;
            tabTop[(int)tabType].Value = true;
            tabType = TabType.Event;
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            tabTop[0].LocalKey = LocalizeKey._11000; // 이벤트
            tabTop[1].LocalKey = LocalizeKey._9002; // 나무
            tabTop[2].LocalKey = LocalizeKey._9000; // 출석체크
        }

        void CloseUI()
        {
            UI.Close<UIDailyCheck>();
        }

        public void Refresh()
        {
            OnUpdateTreeTab();
            OnUpdateEventQuestTab();

            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();
        }

        public void OnUpdateTreeView()
        {
            OnUpdateTreeTab();

            if (currentSubCanvas == null)
                return;

            if (currentSubCanvas == treeBaseView)
                currentSubCanvas.Show();
        }

        private void ShowEventView()
        {
            if (!UIToggle.current.value)
                return;

            ShowSubCanvas(eventBannerView);
        }

        private void ShowDailyCheckView()
        {
            if (!UIToggle.current.value)
                return;

            ShowSubCanvas(dailyCheckView);
        }

        private void ShowTreeBaseView()
        {
            if (!UIToggle.current.value)
                return;

            ShowSubCanvas(treeBaseView);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            if (!UIToggle.current.value)
                return;

            currentSubCanvas = subCanvas;

            HideAllSubCanvas();
            Refresh();
        }

        private void OnUpdateTreeTab()
        {
            bool isNew = presenter.IsCatCoinReward || presenter.IsZenyTreeReward || presenter.IsMaterialTreeReward || presenter.IsDarkTreeReward;
            tabTop[1].SetNotice(isNew);
        }

        private void OnUpdateEventQuestTab()
        {
            tabTop[0].SetNotice(presenter.IsEventQuestStandByReward()
                                || presenter.IsBingoQuestStandByReward()
                                || presenter.IsSpecialRouletteNotice()
                                || presenter.IsAttendEventStandByReward()
                                || presenter.IsWordCollectionStandByReward());
        }
    }
}