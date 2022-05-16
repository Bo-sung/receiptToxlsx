using UnityEngine;

namespace Ragnarok
{
    public class GuildMemberSubView : UISubCanvas<GuildMainPresenter>
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelSortJob;
        [SerializeField] UILabelHelper labelSortName;
        [SerializeField] UILabelHelper labelSortLevel;
        [SerializeField] UILabelHelper labelSortDonate;
        [SerializeField] UILabelHelper labelSortPosition;
        [SerializeField] UILabelHelper labelOnline;        

        GuildMemberInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {

        }

        protected async override void OnShow()
        {
            await presenter.RequestGuildMemberList();
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
            labelSortDonate.LocalKey = LocalizeKey._33055; // 기여도
            labelSortPosition.LocalKey = LocalizeKey._33056; // 직위
        }

        private void Refresh()
        {
            arrayInfo = presenter.GetGuildMemberInfos();
            wrapper.Resize(arrayInfo.Length);

            labelOnline.Text = $"{presenter.OnlineMemberCount}/{presenter.MemberCount}";            
        }

        private void OnItemRefresh(GameObject go, int dataIndex)
        {
            UIGuildMemberInfoSlot ui = go.GetComponent<UIGuildMemberInfoSlot>();
            ui.SetData(presenter, arrayInfo[dataIndex]);
        }
    }
}
