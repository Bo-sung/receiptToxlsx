using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDuelArenaRank : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int TAB_RANK = 0; // 랭킹
        private const int TAB_REWARD = 1; // 보상

        [SerializeField] UITabHelper tab;
        [SerializeField] PopupView popupView;
        [SerializeField] DuelArenaRankView duelArenaRankView;
        [SerializeField] DuelArenaRewardView duelArenaRewardView;
        [SerializeField] UILabelHelper labelNotice;

        DuelArenaRankPresenter presenter;

        protected override void OnInit()
        {
            presenter = new DuelArenaRankPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
            tab.OnSelect += OnSelectTab;
            duelArenaRankView.OnSelectUserInfo += presenter.ShowOtherUserInfo;
            duelArenaRankView.OnDragFinish += presenter.RequestNextPage;

            presenter.OnUpdateRank += RefreshRankView;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateRank -= RefreshRankView;

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
            tab.OnSelect -= OnSelectTab;
            duelArenaRankView.OnSelectUserInfo -= presenter.ShowOtherUserInfo;
            duelArenaRankView.OnDragFinish -= presenter.RequestNextPage;
        }

        protected override void OnShow(IUIData data = null)
        {
            SelectTab(TAB_RANK);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._47928; // 순위
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인

            tab[TAB_RANK].LocalKey = LocalizeKey._47928; // 순위
            tab[TAB_REWARD].LocalKey = LocalizeKey._47929; // 순위 보상

            labelNotice.Text = LocalizeKey._47930.ToText() // 순위는 {NAME}에 속한 유저만 집계됩니다.
                .Replace(ReplaceKey.NAME, presenter.GetLastArenaNameId().ToText());
        }

        void OnSelectTab(int index)
        {
            duelArenaRankView.SetActive(index == TAB_RANK);
            duelArenaRewardView.SetActive(index == TAB_REWARD);

            switch (index)
            {
                case TAB_RANK:
                    RefreshRankView();
                    presenter.RequestRankList();
                    break;

                case TAB_REWARD:
                    RefreshRewardView();
                    break;
            }
        }

        private void SelectTab(int index)
        {
            tab[index].Set(false, false);
            tab.Value = index;
        }

        private void RefreshRankView()
        {
            duelArenaRankView.SetData(presenter.GetRanks(), presenter.GetMyRank());
        }

        private void RefreshRewardView()
        {
            duelArenaRewardView.SetData(presenter.GetRewards());
        }
    }
}