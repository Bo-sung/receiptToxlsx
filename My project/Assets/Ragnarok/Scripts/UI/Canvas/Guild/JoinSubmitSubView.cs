using UnityEngine;

namespace Ragnarok
{
    public class JoinSubmitSubView : UISubCanvas<GuildJoinPresenter>
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelSortName;
        [SerializeField] UILabelHelper labelSortJoin;
        [SerializeField] UILabelHelper labelSortMember;
        [SerializeField] UILabelHelper labelNoGuildJoin;

        GuildSimpleInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }
        protected override void OnClose()
        {

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
            labelSortName.LocalKey = LocalizeKey._33021; // 길드명
            labelSortJoin.LocalKey = LocalizeKey._33022; // 가입조건
            labelSortMember.LocalKey = LocalizeKey._33023; // 길드원
            labelNoGuildJoin.LocalKey = LocalizeKey._33132; // 가입신청한 길드가 없습니다.
        }

        private void Refresh()
        {
            arrayInfo = presenter.GetGuildRequests();
            wrapper.Resize(arrayInfo.Length);

            // 가입신청한 길드가 없을때 메시지 표시
            labelNoGuildJoin.SetActive(arrayInfo.Length == 0);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIGuildJoinSubmitInfo ui = go.GetComponent<UIGuildJoinSubmitInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }
    }
}