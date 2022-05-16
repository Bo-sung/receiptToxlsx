using UnityEngine;

namespace Ragnarok
{
    public class UIGuildJoin : UICanvas, GuildJoinPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton background;
        [SerializeField] UITabHelper tab;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] JoinSubView joinView;
        [SerializeField] JoinSubmitSubView joinSubmitView;

        GuildJoinPresenter presenter;
        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new GuildJoinPresenter(this);
            joinView.Initialize(presenter);
            joinSubmitView.Initialize(presenter);
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(tab.OnChange[0], ShowJoinView);
            EventDelegate.Add(tab.OnChange[1], ShowJoinSubmitView);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(tab.OnChange[0], ShowJoinView);
            EventDelegate.Remove(tab.OnChange[1], ShowJoinSubmitView);
        }

        protected override void OnShow(IUIData data = null)
        {

        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33018; // 길드 가입
            tab[0].LocalKey = LocalizeKey._33018; // 길드 가입
        }

        public void Refresh()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();
            SetTabLocalize();
        }

        void SetTabLocalize()
        {
            tab[1].Text = $"{LocalizeKey._33025.ToText()}({presenter.GuildRequestCount}/{BasisType.GUILD_REQUEST_LIMIT.GetInt()})";
        }

        private void ShowJoinView()
        {
            if (!UIToggle.current.value)
                return;

            ShowSubCanvas(joinView);
        }

        private async void ShowJoinSubmitView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestGuildJoinSubmitList();
            ShowSubCanvas(joinSubmitView);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            currentSubCanvas = subCanvas;
            HideAllSubCanvas();
            Refresh();
        }
    }
}
