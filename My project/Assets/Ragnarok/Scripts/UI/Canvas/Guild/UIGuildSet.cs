using UnityEngine;

namespace Ragnarok
{
    public class UIGuildSet : UICanvas<GuildSetPresenter>, GuildSetPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnPrevious;
        [SerializeField] UIButtonHelper btnNext;
        [SerializeField] UILabelHelper labelJoin;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            presenter = new GuildSetPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnPrevious.OnClick, presenter.OnClickedPrevious);
            EventDelegate.Add(btnNext.OnClick, presenter.OnClickedNext);
            EventDelegate.Add(btnConfirm.OnClick, presenter.OnClickedConfirm);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnPrevious.OnClick, presenter.OnClickedPrevious);
            EventDelegate.Remove(btnNext.OnClick, presenter.OnClickedNext);
            EventDelegate.Remove(btnConfirm.OnClick, presenter.OnClickedConfirm);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.SetView();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._33090; // 길드 관리
            labelTitle.LocalKey = LocalizeKey._33091; // 가입 설정
            btnConfirm.LocalKey = LocalizeKey._33094; // 확인
        }

        void GuildSetPresenter.IView.CloseUI()
        {
            UI.Close<UIGuildSet>();
        }

        void GuildSetPresenter.IView.SetJoin(string name)
        {
            labelJoin.Text = name;
        }
    }
}
