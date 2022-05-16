using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEventDuelRanking : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int WORLD_RANKING = 0; // 순위(전체 서버)
        private const int SERVER_RANKING = 1; // 순위(내 서버)

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UITabHelper tab;
        [SerializeField] EventDuelRankingView eventDuelRankingView;
        [SerializeField] UIButtonHelper btnConfirm;

        EventDuelRankingPresenter presenter;

        protected override void OnInit()
        {
            presenter = new EventDuelRankingPresenter();

            EventDelegate.Add(btnExit.onClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            tab.OnSelect += OnSelectTab;

            presenter.OnUpateData += Refresh;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpateData -= Refresh;

            EventDelegate.Remove(btnExit.onClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            tab.OnSelect -= OnSelectTab;
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.Initialize();

            tab[WORLD_RANKING].Set(false, false);
            tab.Value = WORLD_RANKING;
        }

        protected override void OnHide()
        {
            presenter.ResetData();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47919; // 순위 안내
            tab[WORLD_RANKING].LocalKey = LocalizeKey._47920; // 개인 순위\n(전체 서버)
            tab[SERVER_RANKING].LocalKey = LocalizeKey._47921; // 개인 순위\n(내 서버)
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        void OnSelectTab(int index)
        {
            switch (index)
            {
                case WORLD_RANKING:
                    presenter.SetServerFlag(EventDuelRankingPresenter.WORLD_SERVER_RANK_REQUEST_FLAG);
                    break;

                case SERVER_RANKING:
                    presenter.SetServerFlag(EventDuelRankingPresenter.MY_SERVER_RANK_REQUEST_FLAG);
                    break;
            }

            presenter.RequestDuelRank(); // 서버 정보 호출
            Refresh();
        }

        private void Refresh()
        {
            eventDuelRankingView.SetData(presenter.GetMyRank(), presenter.GetRanks());
        }

        private void CloseUI()
        {
            UI.Close<UIEventDuelRanking>();
        }
    }
}