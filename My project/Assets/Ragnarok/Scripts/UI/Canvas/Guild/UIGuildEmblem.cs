using UnityEngine;

namespace Ragnarok
{
    public class UIGuildEmblem : UICanvas, GuildEmblemPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UITextureHelper emblemBackground;
        [SerializeField] UITextureHelper emblemFrame;
        [SerializeField] UITextureHelper emblemIcon;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] EmblemBackgroundSubView emblemBackgroundSubView;
        [SerializeField] EmblemFrameSubView emblemFrameSubView;
        [SerializeField] EmblemIconSubView emblemIconSubView;

        GuildEmblemPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildEmblemPresenter(this);
            emblemBackgroundSubView.Initialize(presenter);
            emblemFrameSubView.Initialize(presenter);
            emblemIconSubView.Initialize(presenter);
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
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
            labelTitle.LocalKey = LocalizeKey._33013; // 길드 엠블렘 변경
        }

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        void OnClickedBtnConfirm()
        {
            presenter.OnClickedBtnConfirm();
        }

        void GuildEmblemPresenter.IView.SetEmblem(int background, int frame, int icon)
        {
            emblemBackground.SetGuildEmblem($"background_{background}");
            emblemFrame.SetGuildEmblem($"frame_{frame}");
            emblemIcon.SetGuildEmblem($"icon_{icon}");
        }

        void GuildEmblemPresenter.IView.SetConfrim(string name)
        {
            btnConfirm.Text = name;
        }
    }
}
