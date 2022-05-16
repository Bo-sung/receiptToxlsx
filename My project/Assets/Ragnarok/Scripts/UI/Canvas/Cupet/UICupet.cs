using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UICupet : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] UILabelHelper labelCupetCount;
        [SerializeField] CupetListView cupetListView;
        [SerializeField] UIToggleHelper toggleSort;
        [SerializeField] UIButtonHelper btnUnowned, btnRankAscending, btnRankDescending, btnAll;

        CupetViewSortType cupetListSortType;

        CupetPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CupetPresenter();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            cupetListView.OnSelect += presenter.OnSelectCupet;

            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnUpdateCupetList += Refresh;
            presenter.AddEvent();

            EventDelegate.Add(btnUnowned.OnClick, OnClickedBtnUnowned);
            EventDelegate.Add(btnRankAscending.OnClick, OnClickedBtnRankAscending);
            EventDelegate.Add(btnRankDescending.OnClick, OnClickedBtnRankDescending);
            EventDelegate.Add(btnAll.OnClick, OnClickedBtnAll);
        }

        protected override void OnClose()
        {
            cupetListView.OnSelect -= presenter.OnSelectCupet;

            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnUpdateCupetList -= Refresh;
            presenter.RemoveEvent();

            EventDelegate.Remove(btnUnowned.OnClick, OnClickedBtnUnowned);
            EventDelegate.Remove(btnRankAscending.OnClick, OnClickedBtnRankAscending);
            EventDelegate.Remove(btnRankDescending.OnClick, OnClickedBtnRankDescending);
            EventDelegate.Remove(btnAll.OnClick, OnClickedBtnAll);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._5000.ToText()); // 큐펫
            btnUnowned.LocalKey = LocalizeKey._5026; // 미보유
            btnRankAscending.LocalKey = LocalizeKey._5027; // 랭크 낮은 순
            btnRankDescending.LocalKey = LocalizeKey._5028; // 랭크 높은 순
            btnAll.LocalKey = LocalizeKey._5029; // 전체
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RequestCupetList(); // 큐펫 정보 목록 요청
            presenter.RemoveNewOpenContent_Cupet(); // 신규 컨텐츠 플래그 제거
            cupetListSortType = CupetViewSortType.All;
        }

        public void Refresh()
        {
            // 상단
            labelCupetCount.Text = LocalizeKey._5023.ToText() // 보유 큐펫 : {COUNT}/{MAX}
                .Replace(ReplaceKey.COUNT, presenter.GetHaveCupetCount())
                .Replace(ReplaceKey.MAX, presenter.GetAllCupetCount());

            UpdateCupetList();
        }

        private void UpdateCupetList()
        {
            toggleSort.Text = cupetListSortType.ToText();
            cupetListView.SetData(presenter.GetCupetArray(cupetListSortType));
        }

        /// <summary>
        /// 제니 업데이트
        /// </summary>
        private void UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        /// <summary>
        /// 캣코인 업데이트
        /// </summary>
        private void UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        void OnClickedBtnUnowned()
        {
            SetSortCategory(CupetViewSortType.Unowned);
        }

        void OnClickedBtnRankAscending()
        {
            SetSortCategory(CupetViewSortType.RankAscending);
        }

        void OnClickedBtnRankDescending()
        {
            SetSortCategory(CupetViewSortType.RankDescending);
        }

        void OnClickedBtnAll()
        {
            SetSortCategory(CupetViewSortType.All);
        }

        private void SetSortCategory(CupetViewSortType sortType)
        {
            toggleSort.Value = false;
            if (cupetListSortType == sortType)
                return;

            cupetListSortType = sortType;
            UpdateCupetList();
        }
    }
}