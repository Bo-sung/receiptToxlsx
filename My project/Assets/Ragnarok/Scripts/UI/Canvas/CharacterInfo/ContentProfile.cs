using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterInfo"/> 내 정보 보기
    /// <see cref="UIOthersCharacterInfo"/> 다른 유저 정보 보기 
    /// </summary>
    public sealed class ContentProfile : UISubCanvas, ContentProfilePresenter.IView, UserInfoPresenter.IView, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UITextureHelper jobUserIcon;
        [SerializeField] UILabelHelper labNickName;
        [SerializeField] UILabelHelper labID;
        [SerializeField] UILabelHelper labelServerName;
        [SerializeField] UILabelHelper labBaseLevel;
        [SerializeField] UILabelHelper labJobLevel;
        [SerializeField] UILabelHelper labJobName;
        [SerializeField] UILabelHelper labGuildName;
        [SerializeField] UIButtonHelper btnNameChange;
        [SerializeField] UIButtonHelper btnChangeChar;
        [SerializeField] NameChangeView nameChangeInfoView;
        [SerializeField] UIButtonHelper btnChangeServer;
        [SerializeField] UIButton btnProfile;

        ContentProfilePresenter presenter;
        ContentProfilePresenter.IView view;
        UserInfoPresenter userInfoPresenter;

        /// <summary>
        /// 수정 가능 여부 (스탯포인트 분배 등)
        /// </summary>
        private bool isEditable;

        protected override void OnInit()
        {
            presenter = new ContentProfilePresenter(this);
            userInfoPresenter = new UserInfoPresenter(this);
            if (isEditable)
            {
                nameChangeInfoView.Initialize(userInfoPresenter);
                nameChangeInfoView.Init();
            }
            view = this;
            presenter.AddEvent();

            if (isEditable)
            {
                EventDelegate.Add(btnNameChange.OnClick, presenter.ShowNameChangeView);
                EventDelegate.Add(btnChangeChar.OnClick, presenter.GoToTitle);

                if (btnChangeServer)
                {
                    EventDelegate.Add(btnChangeServer.OnClick, OnClickedBtnChangeServer);
                }

                if (btnProfile)
                {
                    EventDelegate.Add(btnProfile.onClick, OnClickedBtnProfile);
                }

                nameChangeInfoView.Hide();
            }
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            if (isEditable)
            {
                EventDelegate.Remove(btnNameChange.OnClick, presenter.ShowNameChangeView);
                EventDelegate.Remove(btnChangeChar.OnClick, presenter.GoToTitle);

                if (btnChangeServer)
                {
                    EventDelegate.Remove(btnChangeServer.OnClick, OnClickedBtnChangeServer);
                }

                if (btnProfile)
                {
                    EventDelegate.Remove(btnProfile.onClick, OnClickedBtnProfile);
                }
            }
        }

        protected override void OnShow()
        {
            Refresh();
        }

        /// <summary>
        /// 플레이어 정보 입력 (OnInit 이후에 실행되어야 한다.)
        /// </summary>
        public void SetPlayer(CharacterEntity charaEntity)
        {
            presenter.SetPlayer(charaEntity);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            if (isEditable)
            {
                btnNameChange.LocalKey = LocalizeKey._14016; // 이름 변경
                btnChangeChar.LocalKey = LocalizeKey._31015; // 캐릭터 선택
                if (btnChangeServer)
                    btnChangeServer.LocalKey = LocalizeKey._31016; // 서버 변경
                nameChangeInfoView.Localize();
            }
        }

        /// <summary>
        /// 편집 가능 여부 설정 (OnInit 전에 실행되어야 한다.)
        /// </summary>
        public void Initialize(bool isEditable)
        {
            this.isEditable = isEditable;
        }

        public void UpdateJobLevel(int level)
        {
            Refresh();
        }

        public void UpdateBaseLevel(int level)
        {
            Refresh();
        }

        public void Refresh()
        {
            Job job = presenter.Job;
            Gender gender = presenter.Gender;
            jobIcon.Set(job.GetJobIcon());
            jobUserIcon.Set(presenter.ProfileName);

            labNickName.Text = LocalizeKey._31010.ToText().Replace(ReplaceKey.NAME, presenter.CharName); // 캐릭터 이름 : {NAME}
            labID.Text = LocalizeKey._31011.ToText().Replace(ReplaceKey.NAME, presenter.HexName); // 캐릭터 ID : {NAME}
            labBaseLevel.Text = LocalizeKey._31012.ToText().Replace(ReplaceKey.LEVEL, presenter.BaseLevel.ToString()); // Base Lv. {LEVEL}
            labJobLevel.Text = LocalizeKey._31013.ToText().Replace(ReplaceKey.LEVEL, presenter.JobLevel.ToString()); // Job Lv. {LEVEL}
            labJobName.Text = LocalizeKey._31001.ToText().Replace(ReplaceKey.NAME, job.GetJobName()); // 직업 : {NAME}     
            labGuildName.Text = presenter.GetGuildText();

            if (presenter.IsMyPlayer())
            {
                labelServerName.Text = LocalizeKey._31017.ToText().Replace(ReplaceKey.NAME, presenter.GetServerName()); // 서버 : {NAME}
            }
            else
            {
                labelServerName.Text = string.Empty;
            }
        }

        public void ShowUserInfoView()
        {
            if (isEditable)
            {
                nameChangeInfoView.Hide();
            }
        }

        public void ShowNameChangeView()
        {
            if (isEditable)
            {
                nameChangeInfoView.Show();
            }
        }

        public void HideNameChangeView()
        {
            nameChangeInfoView.Hide();
        }

        public bool IsActiveNameChangeView()
        {
            return nameChangeInfoView.isActiveAndEnabled;
        }

        public void CloseUI()
        {
        }

        void OnClickedBtnChangeServer()
        {
            UI.Show<UIServerChange>();
        }

        void OnClickedBtnProfile()
        {
            UI.Show<UIProfileSelect>();
        }
    }
}