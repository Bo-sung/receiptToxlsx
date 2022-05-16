using UnityEngine;

namespace Ragnarok.View.League
{
    /// <summary>
    /// <see cref="LeagueModelView"/>
    /// </summary>
    public class UILeagueRankInfo : UIView, IAutoInspectorFinder
    {
        private const int TAB_SINGLE = 0; // 싱글 보상
        private const int TAB_AGNET = 1; // 협동 보상

        public interface IInput : UILeagueRankBar.IInput // <- 내 정보
        {
            int RankSize { get; }
            bool HasNextPage { get; }
            UILeagueRankBar.IInput[] RankArray();
        }

        [SerializeField] UITabHelper tabRank;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UILeagueRankBar element;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILeagueRankBar myRank;

        private SuperWrapContent<UILeagueRankBar, UILeagueRankBar.IInput> wrapContent;

        private IInput input;

        public event System.Action<int> OnSelectTab;
        public event System.Action OnShowNextPage;
        public event System.Action<(int uid, int cid)> OnSelectProfile;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UILeagueRankBar, UILeagueRankBar.IInput>(element);
            wrapContent.OnDragFinished += OnDragFinished;
            foreach (var item in wrapContent)
            {
                item.OnSelectProfile += InvokeOnSelectProfile;
            }

            tabRank.OnSelect += InvokeOnSelectTab;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            wrapContent.OnDragFinished -= OnDragFinished;
            foreach (var item in wrapContent)
            {
                item.OnSelectProfile -= InvokeOnSelectProfile;
            }
            tabRank.OnSelect -= InvokeOnSelectTab;
        }

        protected override void OnLocalize()
        {
            tabRank[TAB_SINGLE].LocalKey = LocalizeKey._47038; // 싱글 랭킹
            tabRank[TAB_AGNET].LocalKey = LocalizeKey._47039; // 협동 랭킹
        }

        void InvokeOnSelectTab(int index)
        {
            OnSelectTab?.Invoke(index);
        }

        void InvokeOnSelectProfile((int uid, int cid) info)
        {
            OnSelectProfile?.Invoke((info.uid, info.cid));
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            myRank.SetData(input); // 내 정보
            wrapContent.SetData(input.RankArray());

            if (input.RankSize == 0)
            {
                labelNoData.SetActive(true);
                labelNoData.Text = LocalizeKey._47009.ToText(); // 랭킹 정보가 없습니다.
            }
            else
            {
                labelNoData.SetActive(false);
            }
        }

        private void OnDragFinished()
        {
            if (input == null)
                return;

            if (!input.HasNextPage)
                return;

            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            if (constraint.sqrMagnitude > 0.1f && constraint.y < 0f)
                OnShowNextPage?.Invoke();
        }
    }
}