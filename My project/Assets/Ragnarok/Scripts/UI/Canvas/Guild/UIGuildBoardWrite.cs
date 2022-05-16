using UnityEngine;

namespace Ragnarok
{
    public class UIGuildBoardWrite : UICanvas<GuildBoardWritePresenter>, GuildBoardWritePresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIInput input;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            presenter = new GuildBoardWritePresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnCancel.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnBtnConfirm);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnCancel.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnBtnConfirm);
        }

        protected override void OnShow(IUIData data = null)
        {
            input.defaultText = LocalizeKey._33087.ToText(); // 메모를 남기세요.\n최대 64자까지 입력이 가능합니다.
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._33086; // 글쓰기
            btnCancel.LocalKey = LocalizeKey._33088; // 취소
            btnConfirm.LocalKey = LocalizeKey._33089; // 작성 완료

        }

        void GuildBoardWritePresenter.IView.CloseUI()
        {
            UI.Close<UIGuildBoardWrite>();
        }

        void OnBtnConfirm()
        {
            presenter.WriteGuildBoard(input.value);
        }

    }
}
