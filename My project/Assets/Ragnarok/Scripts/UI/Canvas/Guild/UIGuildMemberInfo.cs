using UnityEngine;

namespace Ragnarok
{
    public class UIGuildMemberInfo : UICanvas<GuildMemberInfoPresenter>, GuildMemberInfoPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UITextureHelper userIcon;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelJob;
        [SerializeField] UIButtonHelper btnAppoint;
        [SerializeField] UIButtonHelper btnHandOver;
        [SerializeField] UIButtonHelper btnKick;

        protected override void OnInit()
        {
            presenter = new GuildMemberInfoPresenter(this);
            presenter.AddEvent();
            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnAppoint.OnClick, presenter.OnClickedBtnAppoint);
            EventDelegate.Add(btnHandOver.OnClick, presenter.OnClickedBtnHandOver);
            EventDelegate.Add(btnKick.OnClick, presenter.OnClickedBtnKick);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnAppoint.OnClick, presenter.OnClickedBtnAppoint);
            EventDelegate.Remove(btnHandOver.OnClick, presenter.OnClickedBtnHandOver);
            EventDelegate.Remove(btnKick.OnClick, presenter.OnClickedBtnKick);
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
            labelMainTitle.LocalKey = LocalizeKey._33079; // 상세정보
            btnHandOver.LocalKey = LocalizeKey._33084; // 길드 양도
            btnKick.LocalKey = LocalizeKey._33085; // 길드 추방
        }

        void GuildMemberInfoPresenter.IView.SetUserIcon(string name)
        {
            userIcon.Set(name);
        }

        void GuildMemberInfoPresenter.IView.SetJobIcon(string name)
        {
            jobIcon.Set(name);
        }

        void GuildMemberInfoPresenter.IView.SetUserName(string name)
        {
            labelName.Text = name;
        }

        void GuildMemberInfoPresenter.IView.SetJobName(string name)
        {
            labelJob.Text = name;
        }

        void GuildMemberInfoPresenter.IView.SetBtnAppointName(string name)
        {
            btnAppoint.Text = name;
        }

        void GuildMemberInfoPresenter.IView.SetEnabledBtnAppoint(bool isEnabled)
        {
            btnAppoint.IsEnabled = isEnabled;
        }

        void GuildMemberInfoPresenter.IView.SetEnabledBtnHandOver(bool isEnabled)
        {
            btnHandOver.IsEnabled = isEnabled;
        }

        void GuildMemberInfoPresenter.IView.SetEnabledBtnKick(bool isEnabled)
        {
            btnKick.IsEnabled = isEnabled;
        }

        void GuildMemberInfoPresenter.IView.CloseUI()
        {
            UI.Close<UIGuildMemberInfo>();
        }
    }
}
