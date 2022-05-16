using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildTaming : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] GuildTamingView guildTamingView;

        GuildTamingPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildTamingPresenter();

            simplePopupView.OnExit += CloseUI;
            guildTamingView.OnSelectHelp += presenter.ShowHelpPopup;
            guildTamingView.OnSelectDay += OnSelectDay;
            guildTamingView.OnSelectEnter += presenter.StartBattle;
            guildTamingView.OnTimeCheck += presenter.RequestTamingMazeInfo;

            presenter.OnUpdateTime += Refresh;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateTime -= Refresh;

            simplePopupView.OnExit -= CloseUI;
            guildTamingView.OnSelectHelp -= presenter.ShowHelpPopup;
            guildTamingView.OnSelectDay -= OnSelectDay;
            guildTamingView.OnSelectEnter -= presenter.StartBattle;
            guildTamingView.OnTimeCheck -= presenter.RequestTamingMazeInfo;
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.TurnOffNotice(); // 테이밍 미로 Notice 끄기

            Refresh();

            int todayIndex = presenter.GetTodayIndex();
            guildTamingView.SetSelectDay(todayIndex);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            simplePopupView.MainTitleLocalKey = LocalizeKey._33112; // 테이밍 미로
        }

        void OnSelectDay(int dayIndex)
        {
            GuildTamingView.IInput input = presenter.GetData(dayIndex);
            guildTamingView.SetData(input);
        }

        private void Refresh()
        {
            int todayIndex = presenter.GetTodayIndex();
            bool isInProgress = presenter.GetIsInProgress();
            RemainTime remainTime = presenter.GetRemainTime();
            guildTamingView.UpdateData(todayIndex, isInProgress, remainTime);
        }

        private void CloseUI()
        {
            UI.Close<UIGuildTaming>();
        }
    }
}