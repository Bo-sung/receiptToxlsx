using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildRank : UICanvas, GuildRankPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        public enum GuildRankType
        {
            /// <summary>
            /// 길드랭킹 (내길드 존재o)
            /// </summary>
            HasGuild,
            /// <summary>
            /// 길드랭킹 (내길드 존재x)
            /// </summary>
            NoGuild,
            /// <summary>
            /// // 길드전랭킹
            /// </summary>
            GuildBattle,
        }

        [SerializeField] UIButton background;
        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] UILabelHelper labelSortJob, labelSortName, labelSortContribution, labelSortGrade;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIGuildRankInfo myRank;
        [SerializeField] GameObject goMyRankBase;
        [SerializeField] UIWidget listView;
        [SerializeField] int smallHeight, fullHeight;

        GuildRankPresenter presenter;
        RankInfo[] arrayInfo;
        bool hasNextPage;

        protected override void OnInit()
        {
            presenter = new GuildRankPresenter(this);

            simplePopupView.OnExit += OnBack;

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.ScrollView.onDragFinished = OnDragFinished;
            wrapper.SpawnNewList(prefab, 0, 0);

            presenter.AddEvent();
            EventDelegate.Add(background.onClick, OnBack);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            simplePopupView.OnExit -= OnBack;

            EventDelegate.Remove(background.onClick, OnBack);

            if (presenter != null)
                presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelSortJob.LocalKey = LocalizeKey._33095; // 순위
            labelSortName.LocalKey = LocalizeKey._33021; // 길드명
            labelSortGrade.LocalKey = LocalizeKey._33031; // 길드원

            RefreshText();
        }

        public void Set(GuildRankType rankType)
        {
            presenter.SelectGuildType(rankType);

            bool hasMyGuild = presenter.HasMyGuildRank();
            goMyRankBase.SetActive(hasMyGuild);
            listView.height = hasMyGuild ? smallHeight : fullHeight;
            RefreshText();

            presenter.RequestGuildRank().WrapNetworkErrors();
        }

        public void Refresh()
        {
            arrayInfo = presenter.GetRankInfos();
            hasNextPage = presenter.HasNextPage();
            wrapper.Resize(arrayInfo.Length);

            if (presenter.HasMyGuildRank())
                myRank.SetData(presenter.GetMyRankInfo()); // 나의 길드 랭킹
        }

        void OnItemRefresh(GameObject go, int dataIndex)
        {
            UIGuildRankInfo ui = go.GetComponent<UIGuildRankInfo>();
            ui.SetData(arrayInfo[dataIndex]);
        }

        private void OnDragFinished()
        {
            if (!hasNextPage)
                return;

            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            if (constraint.sqrMagnitude > 0.1f && constraint.y < 0f)
                presenter.RequestNextPage();
        }

        private void RefreshText()
        {
            simplePopupView.MainTitleLocalKey = presenter.GetMainTitleLocalKey();
            labelSortContribution.LocalKey = presenter.GetContentLocalKey();
        }
    }
}