using UnityEngine;

namespace Ragnarok
{
    public class UIGuildCreate : UICanvas, GuildCreatePresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton background;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UITextureHelper emblemBackground;
        [SerializeField] UITextureHelper emblemFrame;
        [SerializeField] UITextureHelper emblemIcon;
        [SerializeField] UIButtonHelper btnChange;
        [SerializeField] UIInput inputGuildName;
        [SerializeField] UILabelHelper labelIntroTitle;
        [SerializeField] UIInput inputIntro;
        [SerializeField] UIButtonHelper btnEdit;
        [SerializeField] UILabelValue notiLabel;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnCreate;
        [SerializeField] UILabelHelper labelNameNotice;
        [SerializeField] UILabelHelper labelNotice;

        GuildCreatePresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildCreatePresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(btnCancel.OnClick, OnBack);
            EventDelegate.Add(btnEdit.OnClick, OnClickedBtnEdit);
            EventDelegate.Add(btnCreate.OnClick, OnClickedBtnCreate);
            EventDelegate.Add(btnChange.OnClick, OnClickedBtnChangeEmblem);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(btnCancel.OnClick, OnBack);
            EventDelegate.Remove(btnEdit.OnClick, OnClickedBtnEdit);
            EventDelegate.Remove(btnCreate.OnClick, OnClickedBtnCreate);
            EventDelegate.Remove(btnChange.OnClick, OnClickedBtnChangeEmblem);
        }

        protected override void OnShow(IUIData data = null)
        {
            emblemBackground.SetGuildEmblem($"background_{presenter.EmblemBg}");
            emblemFrame.SetGuildEmblem($"frame_{presenter.EmblemFrame}");
            emblemIcon.SetGuildEmblem($"icon_{presenter.EmblemIcon}");
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33001; // 길드 생성
            btnChange.LocalKey = LocalizeKey._33005; // 엠블렘 변경
            inputGuildName.defaultText = LocalizeKey._33006.ToText(); // 길드명을 입력하세요
            labelIntroTitle.LocalKey = LocalizeKey._33007; // 길드 소개
            inputIntro.defaultText = LocalizeKey._33008.ToText(); // 길드 소개글을 입력해주세요.
            notiLabel.TitleKey = LocalizeKey._33009; // 길드 생성 시
            notiLabel.Value = LocalizeKey._33010.ToText() // {COST}이 필요합니다.
                .Replace("{COST}", BasisType.GUILD_CREATE_ZENY.GetInt().ToString("N0"));
            btnCancel.LocalKey = LocalizeKey._33011; // 취소
            btnCreate.LocalKey = LocalizeKey._33012; // 생성
            labelNameNotice.LocalKey = LocalizeKey._33072; // 길드명은 최소 2글자에서 최대 8글자 입니다.
            labelNotice.LocalKey = LocalizeKey._33125; // 1. 길드 엠블렘을 선택하세요.\n2. 생성할 길드 이름을 입력하세요.\n3. 길드 소개글을 입력하세요.
        }

        /// <summary>
        /// 소개글 편집버튼 클릭
        /// </summary>
        void OnClickedBtnEdit()
        {
            inputIntro.isSelected = true;
        }

        /// <summary>
        /// 길드이름, 소개글 변경 
        /// </summary>
        void OnChangeInput()
        {

        }

        bool CanGuildName()
        {
            string guildName = inputGuildName.value;
            int nameLength = guildName.Length;
            if (nameLength < 2 || nameLength > 8)
                return false;

            return true;
        }

        /// <summary>
        /// 엠블렘 변경 버튼 클릭
        /// </summary>
        void OnClickedBtnChangeEmblem()
        {
            UI.Show<UIGuildEmblem>();
        }

        /// <summary>
        /// 길드 생성 버튼 클릭
        /// </summary>
        void OnClickedBtnCreate()
        {
            string guildName = inputGuildName.value;
            if (string.IsNullOrWhiteSpace(guildName))
            {
                string message = LocalizeKey._33006.ToText(); // 길드명을 입력하세요
                UI.ShowToastPopup(message);
                return;
            }

            int nameLength = guildName.Length;
            if (nameLength < 2 || nameLength > 8)
            {
                string message = LocalizeKey._33072.ToText(); // 길드명은 최소 2글자에서 최대 8글자 입니다.
                UI.ShowToastPopup(message);
                return;
            }

            string errorMessage = FilterUtils.CheckCharacterName(guildName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                UI.ShowToastPopup(errorMessage);
                return;
            }

            string guildIntro = inputIntro.value;
            if (string.IsNullOrWhiteSpace(guildIntro))
            {
                string message = LocalizeKey._33008.ToText(); // 길드 소개글을 입력해주세요.
                UI.ShowToastPopup(message);
                return;
            }

            int introLength = guildIntro.Length;
            if (introLength > 64)
            {
                string message = LocalizeKey._33126.ToText(); // 길드 소개글은 최대 64글자 입니다.
                UI.ShowToastPopup(message);
                return;
            }

            presenter.RequestGuildCreate(guildName, guildIntro);
        }
    }
}