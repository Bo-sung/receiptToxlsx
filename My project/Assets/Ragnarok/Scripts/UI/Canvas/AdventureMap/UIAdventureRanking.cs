using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAdventureRanking : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int POINT_RANKING = 0; // 점수랭킹
        private const int KILL_RANKING = 1; // 처치랭킹

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UITabHelper tab;
        [SerializeField] AdventureRankingView adventureRankingView;
        [SerializeField] UIButtonHelper btnConfirm;

        AdventureRankingPresenter presenter;

        private RankType rankType;

        protected override void OnInit()
        {
            presenter = new AdventureRankingPresenter();

            EventDelegate.Add(btnExit.onClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            tab.OnSelect += OnSelectTab;

            adventureRankingView.OnDragFinish += RequestNextPage;
            adventureRankingView.OnSelect += presenter.RequestOtherCharacterInfo;
            presenter.OnUpdateRankList += Refresh;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            adventureRankingView.OnDragFinish -= RequestNextPage;
            adventureRankingView.OnSelect -= presenter.RequestOtherCharacterInfo;
            presenter.OnUpdateRankList -= Refresh;

            EventDelegate.Remove(btnExit.onClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            tab.OnSelect -= OnSelectTab;
        }

        protected override void OnShow(IUIData data = null)
        {
            tab[POINT_RANKING].Set(false, false);
            tab.Value = POINT_RANKING;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._48215; // 순위 안내
            tab[POINT_RANKING].LocalKey = LocalizeKey._48217; // 점수 순위
            tab[KILL_RANKING].LocalKey = LocalizeKey._48218; // 처치 순위
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        void OnSelectTab(int index)
        {
            switch (index)
            {
                case POINT_RANKING:
                    rankType = RankType.EventStagePoint;
                    break;

                case KILL_RANKING:
                    rankType = RankType.EventStageKillCount;
                    break;
            }

            Refresh((rankType, 1));
            presenter.RequestRankList(rankType);
        }

        private void Refresh((RankType rankType, int page) info)
        {
            adventureRankingView.SetMyRank(presenter.GetMyInfo(info.rankType));
            adventureRankingView.SetData(presenter.GetArrayInfo(info.rankType));
        }

        private void CloseUI()
        {
            UI.Close<UIAdventureRanking>();
        }

        private void RequestNextPage()
        {
            presenter.RequestNextPage(rankType);
        }
    }
}