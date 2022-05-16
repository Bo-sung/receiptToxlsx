using UnityEngine;

namespace Ragnarok
{
    public class JoinSubView : UISubCanvas<GuildJoinPresenter>, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIInput input;
        [SerializeField] UIButtonHelper btnSearch;
        [SerializeField] UIButtonHelper btnRefresh;
        [SerializeField] UILabelHelper labelSortName;
        [SerializeField] UILabelHelper labelSortJoin;
        [SerializeField] UILabelHelper labelSortMember;
        [SerializeField] UILabelHelper labelNoGuild;

        GuildSimpleInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
            EventDelegate.Add(input.onChange, OnChangeInput);
            EventDelegate.Add(btnSearch.OnClick, OnClickedBtnSearch);
            EventDelegate.Add(btnRefresh.OnClick, OnClickedBtnRefresh);
        }
        protected override void OnClose()
        {
            EventDelegate.Remove(input.onChange, OnChangeInput);
            EventDelegate.Remove(btnSearch.OnClick, OnClickedBtnSearch);
            EventDelegate.Remove(btnRefresh.OnClick, OnClickedBtnRefresh);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            input.defaultText = LocalizeKey._33019.ToText(); // 길드명을 입력하세요
            btnSearch.LocalKey = LocalizeKey._33020; // 검색
            labelSortName.LocalKey = LocalizeKey._33021; // 길드명
            labelSortJoin.LocalKey = LocalizeKey._33022; // 가입조건
            labelSortMember.LocalKey = LocalizeKey._33023; // 길드원
            labelNoGuild.LocalKey = LocalizeKey._33131; // 가입 가능한 길드가 없습니다.
        }

        private void Refresh()
        {
            arrayInfo = presenter.GetRecommends();
            wrapper.Resize(arrayInfo.Length);
            OnChangeInput();

            // 가입 가능한 길드목록 없으면 메시지 표시
            labelNoGuild.SetActive(arrayInfo.Length == 0);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIGuildJoinInfo ui = go.GetComponent<UIGuildJoinInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        /// <summary>
        /// 길드 검색 버튼 비/활성화 처리
        /// </summary>
        void OnChangeInput()
        {
            btnSearch.IsEnabled = CanSearch();
        }

        bool CanSearch()
        {
            string searchName = input.value;
            int nameLength = searchName.Length;
            if (nameLength < 2 || nameLength > 8)
                return false;

            return true;
        }

        /// <summary>
        /// 길드 검색 버튼 클릭
        /// </summary>
        void OnClickedBtnSearch()
        {
            presenter.RequestGuildSearch(input.value);
        }

        /// <summary>
        /// 추천 길드 재갱신 버튼
        /// </summary>
        void OnClickedBtnRefresh()
        {
            presenter.RequestGuildRecommend();
        }
    }
}