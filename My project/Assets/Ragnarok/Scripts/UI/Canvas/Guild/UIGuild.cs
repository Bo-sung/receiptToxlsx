using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuild : UICanvas, GuildPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelValue labelCreate;
        [SerializeField] UILabelValue labelJoin;
        [SerializeField] UIButtonHelper btnCreate;
        [SerializeField] UIButtonHelper btnJoin;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UIButtonHelper btnRank;

        GuildPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnCreate.OnClick, presenter.OnClickedBtnCreate);
            EventDelegate.Add(btnJoin.OnClick, presenter.OnClickedBtnJoin);
            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnCreate.OnClick, presenter.OnClickedBtnCreate);
            EventDelegate.Remove(btnJoin.OnClick, presenter.OnClickedBtnJoin);
            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);
        }

        protected override void OnShow(IUIData data = null)
        {

        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33000; // 길드
            labelCreate.TitleKey = LocalizeKey._33001; // 길드 생성
            labelCreate.ValueKey = LocalizeKey._33003; // 길드를 생성하고\n새로운 친구를 만들어 보세요.
            labelJoin.TitleKey = LocalizeKey._33002; // 길드 가입
            labelJoin.ValueKey = LocalizeKey._33004; // 길드에 가입하고\n새로운 친구를 만들어 보세요.
            btnRank.LocalKey = LocalizeKey._33136; // 랭킹
        }

        /// <summary>
        /// 길드 랭킹 보기
        /// </summary>
        void OnClickedBtnRank()
        {
            UI.Show<UIGuildRank>().Set(UIGuildRank.GuildRankType.NoGuild);
        }
    }
}