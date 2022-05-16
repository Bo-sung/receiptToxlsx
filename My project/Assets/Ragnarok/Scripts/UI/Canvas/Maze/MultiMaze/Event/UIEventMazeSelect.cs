using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEventMazeSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] ModeSelectView modeSelectView;
        [SerializeField] EventMazeHelpView eventMazeHelpView;
        [SerializeField] TicketBuyView ticketBuyView;

        EventMazeSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new EventMazeSelectPresenter();

            modeSelectView.OnSelectExit += CloseUI;
            modeSelectView.OnSelectMaze += OnSelectEventMaze;
            modeSelectView.OnSelectFreeFight += presenter.StartEventFreeFight;
            modeSelectView.OnSelectHelp += eventMazeHelpView.Show;
            modeSelectView.OnRequestFreeFightInfo += presenter.RequestFreeFightInfo;
            ticketBuyView.OnSelectEnter += presenter.StartEventMaze;

            presenter.OnUpdateEventMultiMazeTicket += RefreshEventMultiMaze;
            presenter.OnUpdateEventFreeFightTime += RefreshFreeFight;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateEventMultiMazeTicket -= RefreshEventMultiMaze;
            presenter.OnUpdateEventFreeFightTime -= RefreshFreeFight;

            modeSelectView.OnSelectExit -= CloseUI;
            modeSelectView.OnSelectMaze -= OnSelectEventMaze;
            modeSelectView.OnSelectFreeFight -= presenter.StartEventFreeFight;
            modeSelectView.OnSelectHelp -= eventMazeHelpView.Show;
            modeSelectView.OnRequestFreeFightInfo -= presenter.RequestFreeFightInfo;
            ticketBuyView.OnSelectEnter -= presenter.StartEventMaze;
        }

        protected override void OnShow(IUIData data = null)
        {
            eventMazeHelpView.InitializeTime(presenter.eventFreeFightOpenTimes);
            eventMazeHelpView.InitializeSkill(presenter.eventFreeFightSkills);
            eventMazeHelpView.InitializeReward(presenter.eventFreeFightRewards);

            modeSelectView.Show();
            eventMazeHelpView.Hide();
            ticketBuyView.Hide();

            RefreshEventMultiMaze();
            RefreshFreeFight();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            const int MAIN_TITLE_LOCAL_KEY = LocalizeKey._49507; // 이벤트 설명
            const int TITLE_LOCAL_KEY = LocalizeKey._49510; // 눈싸움은 다함께!
            const int DESC_LOCAL_KEY = LocalizeKey._49511; // 난전 입장 시간
            eventMazeHelpView.SetText(MAIN_TITLE_LOCAL_KEY.ToText(), TITLE_LOCAL_KEY.ToText(), DESC_LOCAL_KEY.ToText());

            const int DETAIL_TITLE_LOCAL_KEY = LocalizeKey._49508; // 빼앗기기 전에 도망가요!
            const int DETAIL_DESC_LOCAL_KEY = LocalizeKey._49509; // 누구보다 먼저 [c][76C5FE]하얀 눈덩이[-][/c]를 획득해야 합니다. 빼앗기기 전에요!\n하얀 눈덩이를 획득 못해도 슬퍼하지 말아요.\n[c][76C5FE]강탈자 물약[-][/c]을 얻으면 다른 사람의 눈덩이를 뺏을 수 있어요!
            eventMazeHelpView.SetDetailText(DETAIL_TITLE_LOCAL_KEY.ToText(), DETAIL_DESC_LOCAL_KEY.ToText());
        }

        public void Show(int mazeId)
        {
            presenter.SetMazeId(mazeId);
        }

        void OnSelectEventMaze()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (presenter.GetTicketCount() <= 0)
            {
                ticketBuyView.Set(RewardType.EventMultiMazeTicket, presenter.GetNeedCatCoint());
                ticketBuyView.Show();
                return;
            }

            presenter.StartEventMaze();
        }

        private void CloseUI()
        {
            UI.Close<UIEventMazeSelect>();
        }

        private void RefreshEventMultiMaze()
        {
            modeSelectView.SetMultiMazeData(presenter.GetTicketCount(), presenter.GetTicketMaxCount());
        }

        private void RefreshFreeFight()
        {
            modeSelectView.SetFreeFightData(presenter.GetStartTime(), presenter.GetEndTime());
        }
    }
}