using UnityEngine;

namespace Ragnarok
{
    public class UIOptionAccount : UISubCanvas<OptionPresenter>, OptionPresenter.IViewAccount, IInspectorFinder
    {
        private static readonly Color GoogleTextColor = new Color32(0, 0, 0, 138);
        private static readonly Color AppleTextColor = new Color32(0, 0, 0, 255);

        [SerializeField] UITable table;

        /******************** Account ********************/
        [SerializeField] UILabelHelper labelMemberTitle;
        [SerializeField] UILabelHelper labelMemberId;
        [SerializeField] UIButtonHelper btnCopy;

        /******************** Account ********************/
        [SerializeField] UILabelHelper labelAccountTitle;
        [SerializeField] UILabelValue characterID, characterName;
        [SerializeField] UIButtonHelper btnChangeName;
        [SerializeField] UILabelHelper labelInfoOpen;
        [SerializeField] UIToggleHelper toggleInfoOpen;
        [SerializeField] UIButtonHelper btnHexCopy;
        [SerializeField] UILabelHelper labelAdAppCustom;
        [SerializeField] UIToggleHelper toggleAdAppCustom;

        /******************** Connect ********************/
        [SerializeField] UILabelHelper labelConnectTitle;
        [SerializeField] UIButtonHelper btnGoogle, btnFaceBook, btnApple;
        [SerializeField] UISprite connectBase;
        [SerializeField] UIGrid gridConnect;
        [SerializeField] UILabel[] loginLabels;

        /******************** OnBuff ********************/
        [SerializeField] GameObject OnBuffBase;
        [SerializeField] UILabelHelper labelOnBuffTitle;
        [SerializeField] UIButtonHelper btnOnBuffLink;
        [SerializeField] UIButtonHelper btnOnBuffPoint;

        /******************** Etc ********************/
        [SerializeField] UILabelHelper labelEtcTitle;
        [SerializeField] UIButtonHelper btnCenter, btnLanguage, btnPrivacy, btnTerms, btnLogout, btnNotice, btnCoupon, btnPolicy, btnDeleteMember;
        [SerializeField] UIGrid gridEtc;
        [SerializeField] int padding;

        public System.Action onClickedBtnChangeName;
        public System.Action onClickedBtnLanguage;
        public System.Action onClickedBtnDeleteMember;
        public System.Action onClickedBtnOnBuffLink;
        public System.Action onClickedBtnOnBuffPoint;

        public override void Initialize(OptionPresenter presenter)
        {
            base.Initialize(presenter);

            toggleInfoOpen.Set(Entity.player.User.IsOpenInfo);
        }

        protected override void OnInit()
        {
            EventDelegate.Add(btnChangeName.OnClick, OnClickedBtnChangeName);
            EventDelegate.Add(btnGoogle.OnClick, OnClickedBtnGoogle);
            EventDelegate.Add(btnFaceBook.OnClick, OnClickedBtnFaceBook);
            EventDelegate.Add(btnApple.OnClick, OnClickedBtnApple);
            EventDelegate.Add(btnCenter.OnClick, OnClickedBtnCenter);
            EventDelegate.Add(btnLanguage.OnClick, OnClickedBtnLanguage);
            EventDelegate.Add(btnPrivacy.OnClick, OnClickedBtnPrivacy);
            EventDelegate.Add(btnTerms.OnClick, OnClickedBtnTerms);
            EventDelegate.Add(btnLogout.OnClick, OnClickedBtnLogout);
            EventDelegate.Add(btnNotice.OnClick, OnClickedBtnNotice);
            EventDelegate.Add(btnCoupon.OnClick, OnClickedBtnCoupon);
            EventDelegate.Add(btnPolicy.OnClick, OnClickedBtnPolicy);
            EventDelegate.Add(toggleInfoOpen.OnChange, OnChangedToggleInfoOpen);
            EventDelegate.Add(btnCopy.OnClick, OnClickedBtnCopy);
            EventDelegate.Add(btnHexCopy.OnClick, OnClickedBtnHexCopy);
            EventDelegate.Add(toggleAdAppCustom.OnChange, OnChangedToggleAdAppCustom);
            EventDelegate.Add(btnDeleteMember.OnClick, OnClickedBtnDeleteMember);
            EventDelegate.Add(btnOnBuffLink.OnClick, OnClickedBtnOnBuffLink);
            EventDelegate.Add(btnOnBuffPoint.OnClick, OnClickedBtnOnBuffPoint);

            // 검수서버 접속시 쿠폰버튼 제거
            if (presenter.IsStageServer())
            {
                btnCoupon.SetActive(false);
                gridEtc.Reposition();
            }

            // 안드로이드에서 애플 계정 연동 버튼 제거
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                btnApple.SetActive(false);
                var bounds = NGUIMath.CalculateRelativeWidgetBounds(gridConnect.transform);
                int size = (int)Mathf.Abs(bounds.min.y - bounds.max.y);
                connectBase.SetDimensions(connectBase.width, size + padding);
#endif
            }

            foreach (var label in loginLabels.OrEmptyIfNull())
            {
#if UNITY_ANDROID
                label.color = GoogleTextColor;
#elif UNITY_IOS
                label.color = AppleTextColor;
#endif
            }

            OnBuffBase.SetActive(GameServerConfig.IsOnBuff());
            table.Reposition();

            presenter.OnUpdateAdAppCustom += UpdateAdAppCustom;
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnChangeName.OnClick, OnClickedBtnChangeName);
            EventDelegate.Remove(btnGoogle.OnClick, OnClickedBtnGoogle);
            EventDelegate.Remove(btnFaceBook.OnClick, OnClickedBtnFaceBook);
            EventDelegate.Remove(btnApple.OnClick, OnClickedBtnApple);
            EventDelegate.Remove(btnCenter.OnClick, OnClickedBtnCenter);
            EventDelegate.Remove(btnLanguage.OnClick, OnClickedBtnLanguage);
            EventDelegate.Remove(btnPrivacy.OnClick, OnClickedBtnPrivacy);
            EventDelegate.Remove(btnTerms.OnClick, OnClickedBtnTerms);
            EventDelegate.Remove(btnLogout.OnClick, OnClickedBtnLogout);
            EventDelegate.Remove(btnNotice.OnClick, OnClickedBtnNotice);
            EventDelegate.Remove(btnCoupon.OnClick, OnClickedBtnCoupon);
            EventDelegate.Remove(btnPolicy.OnClick, OnClickedBtnPolicy);
            EventDelegate.Remove(toggleInfoOpen.OnChange, OnChangedToggleInfoOpen);
            EventDelegate.Remove(btnCopy.OnClick, OnClickedBtnCopy);
            EventDelegate.Remove(btnHexCopy.OnClick, OnClickedBtnHexCopy);
            EventDelegate.Remove(toggleAdAppCustom.OnChange, OnChangedToggleAdAppCustom);
            EventDelegate.Remove(btnDeleteMember.OnClick, OnClickedBtnDeleteMember);
            EventDelegate.Remove(btnOnBuffLink.OnClick, OnClickedBtnOnBuffLink);
            EventDelegate.Remove(btnOnBuffPoint.OnClick, OnClickedBtnOnBuffPoint);

            presenter.OnUpdateAdAppCustom -= UpdateAdAppCustom;
        }

        protected override void OnShow()
        {
            labelMemberId.Text = presenter.GetMemberId();
            characterID.Value = presenter.GetCharacterHexId();
            characterName.Value = presenter.GetCharacterName();
            UpdateAdAppCustom();
            btnPolicy.SetActive(GameServerConfig.IsKorea()); // 한국일 경우에만 운영정책 버튼 표시
            gridEtc.Reposition();
        }

        protected override void OnHide()
        {
        }

        async void OnChangedToggleInfoOpen()
        {
            bool isOpen = toggleInfoOpen.Value;
            if (isOpen == Entity.player.User.IsOpenInfo)
                return;

            var param = Protocol.NewInstance();
            param.PutByte("1", (byte)(isOpen ? 1 : 0));
            var response = await Protocol.REQUEST_OPEN_INFO_SETTING.SendAsync(param);
            if (!response.isSuccess)
            {
                toggleInfoOpen.Set(!isOpen, notify: false);
                return;
            }

            Entity.player.User.IsOpenInfo = isOpen;
        }

        void OnClickedBtnChangeName()
        {
            onClickedBtnChangeName?.Invoke();
        }

        void OnClickedBtnGoogle()
        {
            presenter.LinkGoogle();
        }

        void OnClickedBtnFaceBook()
        {
            presenter.LinkFaceBook();
        }

        void OnClickedBtnApple()
        {
            presenter.LinkApple();
        }

        void OnClickedBtnCenter()
        {
            presenter.ShowCSWebView();
        }

        void OnClickedBtnLanguage()
        {
            onClickedBtnLanguage?.Invoke();
        }

        void OnClickedBtnPrivacy()
        {
            presenter.ShowPrivacy();
        }

        void OnClickedBtnTerms()
        {
            presenter.ShowTerms();
        }

        void OnClickedBtnLogout()
        {
            presenter?.Logout();
        }

        void OnClickedBtnNotice()
        {
            presenter.ShowNotice();
        }

        void OnClickedBtnCoupon()
        {
            UI.Show<UICouponPopup>();
        }

        void OnClickedBtnPolicy()
        {
            BasisUrl.KoreanPolicy.OpenUrl();
        }

        void OnChangedToggleAdAppCustom()
        {
            if (toggleAdAppCustom.Value == presenter.IsAdAppCustom())
                return;

            presenter.ToggleAdAppCustom();
        }

        /// <summary>
        /// 게임탈퇴 버튼 이벤트
        /// </summary>
        void OnClickedBtnDeleteMember()
        {
            onClickedBtnDeleteMember?.Invoke();
        }

        /// <summary>
        /// 온버프 연동/해제 버튼
        /// </summary>
        void OnClickedBtnOnBuffLink()
        {
            onClickedBtnOnBuffLink?.Invoke();
        }

        /// <summary>
        /// 온버프 포인트 동기화 버튼
        /// </summary>
        void OnClickedBtnOnBuffPoint()
        {
            onClickedBtnOnBuffPoint?.Invoke();
        }

        private void UpdateAdAppCustom()
        {
            toggleAdAppCustom.Set(presenter.IsAdAppCustom());
        }

        protected override void OnLocalize()
        {
            labelMemberTitle.LocalKey = LocalizeKey._14054; // 회원번호
            labelAccountTitle.LocalKey = LocalizeKey._14013; // 계정 정보
            characterID.TitleKey = LocalizeKey._14014; // 캐릭터 ID
            characterName.TitleKey = LocalizeKey._14015; // 캐릭터 이름
            btnChangeName.LocalKey = LocalizeKey._14016; // 이름 변경

            labelConnectTitle.LocalKey = LocalizeKey._14027; // 계정 연동

            labelOnBuffTitle.LocalKey = LocalizeKey._14069; // INNO 지갑 연동

            labelEtcTitle.LocalKey = LocalizeKey._14017; // 기타
            btnCenter.LocalKey = LocalizeKey._14033; // 고객 센터
            btnLanguage.LocalKey = LocalizeKey._14025; // 언어 선택
            btnPrivacy.LocalKey = LocalizeKey._14047; // 개인 정보 처리 방침
            btnTerms.LocalKey = LocalizeKey._14048; // 게임 이용 약관
            btnLogout.LocalKey = LocalizeKey._14018; // 로그아웃
            btnNotice.LocalKey = LocalizeKey._14052; // 공지사항
            btnCoupon.LocalKey = LocalizeKey._14034; // 쿠폰 등록
            btnPolicy.LocalKey = LocalizeKey._14055; // 운영 정책
            btnDeleteMember.LocalKey = LocalizeKey._14065; // 게임탈퇴

            labelInfoOpen.LocalKey = LocalizeKey._14046; // 캐릭터 정보 공개
            labelAdAppCustom.LocalKey = LocalizeKey._14400; // 광고 개인화 동의

            btnOnBuffPoint.LocalKey = LocalizeKey._14404; // 동기화

            presenter.SetView();
            SetOnBuffInfo(presenter.IsOnBuffAccountLink());
        }

        void OptionPresenter.IViewAccount.SetBtnGoogleView(bool isLinked)
        {
            btnGoogle.LocalKey = LocalizeKey._14028; // Google
            if (isLinked)
                btnGoogle.Text += LocalizeKey._14030.ToText(); // (연동됨)
        }

        void OptionPresenter.IViewAccount.SetBtnFacebookView(bool isLinked)
        {
            btnFaceBook.LocalKey = LocalizeKey._14029; // Facebook
            if (isLinked)
                btnFaceBook.Text += LocalizeKey._14030.ToText(); // (연동됨)
        }

        void OptionPresenter.IViewAccount.SetBtnAppleView(bool isLinked)
        {
            btnApple.LocalKey = LocalizeKey._14031; // Apple
            if (isLinked)
                btnApple.Text += LocalizeKey._14030.ToText(); // (연동됨)
        }

        void OnClickedBtnCopy()
        {
            GUIUtility.systemCopyBuffer = presenter.GetMemberId();
            UI.ShowToastPopup(LocalizeKey._90263.ToText()); // 클립보드에 복사했어요.
        }

        void OnClickedBtnHexCopy()
        {
            GUIUtility.systemCopyBuffer = presenter.GetCharacterHexId();
            UI.ShowToastPopup(LocalizeKey._90263.ToText()); // 클립보드에 복사했어요.
        }

        public void SetOnBuffInfo(bool isOnBuffAccountLink)
        {
            btnOnBuffLink.LocalKey = LocalizeKey._14064; // INNO
            if (isOnBuffAccountLink)
                btnOnBuffLink.Text += LocalizeKey._14030.ToText(); // (연동됨)

            btnOnBuffPoint.SetActive(isOnBuffAccountLink);
        }

        bool IInspectorFinder.Find()
        {
            loginLabels = gridConnect.GetComponentsInChildren<UILabel>();
            return true;
        }
    }
}