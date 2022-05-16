using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class GuildJoinSubmitSubView : UISubCanvas<GuildMainPresenter>
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelSortJob;
        [SerializeField] UILabelHelper labelSortName;
        [SerializeField] UILabelHelper labelSortLevel;
        [SerializeField] UILabelHelper labelSortDate;
        [SerializeField] UILabelHelper labelNoMember;

        GuildJoinSubmitInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }
        protected override void OnClose()
        {

        }

        protected override async void OnShow()
        {
            await presenter.RequestJoinSubmitUserList();
            Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelSortJob.LocalKey = LocalizeKey._33048; // 직업
            labelSortName.LocalKey = LocalizeKey._33049; // 이름
            labelSortLevel.LocalKey = LocalizeKey._33050; // 레벨
            labelSortDate.LocalKey = LocalizeKey._33051; // 신청일
            labelNoMember.LocalKey = LocalizeKey._33124; // 가입 신청한 유저가 없습니다.
        }

        private void Refresh()
        {
            arrayInfo = presenter.GetGuildJoinSubmitInfos();
            wrapper.Resize(arrayInfo.Length);

            // 가입 신청한 유저가 없으면 메세지 표시
            labelNoMember.SetActive(arrayInfo.Length == 0);
        }

        private void OnItemRefresh(GameObject go, int dataIndex)
        {
            UIGuildJoinSubmitUserInfo ui = go.GetComponent<UIGuildJoinSubmitUserInfo>();
            ui.SetData(presenter, arrayInfo[dataIndex]);
        }
    }
}
