using UnityEngine;

namespace Ragnarok
{
    public sealed class UILogin : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        private static readonly Color GoogleTextColor = new Color32(0, 0, 0, 138);
        private static readonly Color AppleTextColor = new Color32(0, 0, 0, 255);

        [SerializeField] UIButtonHelper btnGoogleLogin;
        [SerializeField] UIButtonHelper btnFaceBookLogin;
        [SerializeField] UIButtonHelper btnGuestLogin;
        [SerializeField] UIButtonHelper btnApplLogin;
        [SerializeField] UILabel[] loginLabels;

        LoginPresenter presenter;
        bool isLogin;

        private System.Action<bool> onLogin;

        protected override void OnInit()
        {
            presenter = new LoginPresenter();

            EventDelegate.Add(btnGoogleLogin.OnClick, OnClickedBtnGoogleLogin);
            EventDelegate.Add(btnFaceBookLogin.OnClick, OnClickedBtnFaceBookLogin);
            EventDelegate.Add(btnGuestLogin.OnClick, OnClikedBtnGuestLogin);
            EventDelegate.Add(btnApplLogin.OnClick, OnClickedBtnAppleLogin);

            presenter.OnLogin += OnLogin;
            presenter.OnLoginError += OnLoginError;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnLogin -= OnLogin;
            presenter.OnLoginError -= OnLoginError;

            EventDelegate.Remove(btnGoogleLogin.OnClick, OnClickedBtnGoogleLogin);
            EventDelegate.Remove(btnFaceBookLogin.OnClick, OnClickedBtnFaceBookLogin);
            EventDelegate.Remove(btnGuestLogin.OnClick, OnClikedBtnGuestLogin);
            EventDelegate.Remove(btnApplLogin.OnClick, OnClickedBtnAppleLogin);

            onLogin = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            isLogin = false;

            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                btnApplLogin.SetActive(false);
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
        }

        protected override void OnHide()
        {
            onLogin?.Invoke(isLogin);
            onLogin = null;
        }

        protected override void OnLocalize()
        {
            string text = LocalizeKey._250.ToText(); // Sign in with {TYPE}
            btnGoogleLogin.Text = text.Replace(ReplaceKey.TYPE, GetLoginTypeString(NCommon.LoginType.GOOGLE));
            btnFaceBookLogin.Text = text.Replace(ReplaceKey.TYPE, GetLoginTypeString(NCommon.LoginType.FACEBOOK));
            btnGuestLogin.Text = GetLoginTypeString(NCommon.LoginType.GUEST);
            //btnApplLogin.Text = text.Replace(ReplaceKey.TYPE, GetLoginTypeString(NCommon.LoginType.APPLE));
        }

        public void Set(System.Action<bool> onLogin)
        {
            this.onLogin = onLogin;
        }

        void OnClickedBtnGoogleLogin()
        {
            if (!Application.isEditor)
                presenter.LoginAsync(NCommon.LoginType.GOOGLE).WrapNetworkErrors();
        }

        void OnClickedBtnFaceBookLogin()
        {
            if (!Application.isEditor)
                presenter.LoginAsync(NCommon.LoginType.FACEBOOK).WrapNetworkErrors();
        }

        async void OnClikedBtnGuestLogin()
        {
            string title = LocalizeKey._6.ToText(); // 게스트 로그인
            string message = LocalizeKey._12.ToText(); // 게스트 계정은 앱 삭제, 디바이스 변경 시\n게임 정보가 삭제되며 복구할 수 없습니다.\n안전한 계정 보호를 위해 계정 연동 후 이용하시길 바랍니다.\n\n게스트 계정으로 계속 진행하시겠습니까?
            string cancelText = LocalizeKey._2.ToText(); // 취소
            string confirmText = LocalizeKey._1.ToText(); // 확인
            if (await UI.Show<UIGuestNoticePopup>().Show(title, message, cancelText, confirmText) == UIGuestNoticePopup.SelectResult.Cancel)
                return;

            presenter.LoginAsync(NCommon.LoginType.GUEST).WrapNetworkErrors();
        }

        void OnClickedBtnAppleLogin()
        {
            if (!Application.isEditor)
                presenter.LoginAsync(NCommon.LoginType.APPLE).WrapNetworkErrors();
        }

        /// <summary>
        /// 로그인 성공
        /// </summary>
        private void OnLogin()
        {
            isLogin = true;
            CloseUI();
        }

        /// <summary>
        /// 로그인 실패
        /// </summary>
        private void OnLoginError(System.Exception exception)
        {
            Debug.LogError("OnLoginError");
            return;

            isLogin = false;
            CloseUI();

            UI.ConfirmPopup(exception.Message);
        }

        private void CloseUI()
        {
            UI.Close<UILogin>();
        }

        private string GetLoginTypeString(NCommon.LoginType type)
        {
            switch (type)
            {
                case NCommon.LoginType.GOOGLE:
                    return LocalizeKey._201.ToText();

                case NCommon.LoginType.FACEBOOK:
                    return LocalizeKey._200.ToText();

                case NCommon.LoginType.GUEST:
                    return LocalizeKey._202.ToText();

                case NCommon.LoginType.APPLE:
                    return LocalizeKey._203.ToText();
            }

            return string.Empty;
        }

        public override bool Find()
        {
            base.Find();
            loginLabels = GetComponentsInChildren<UILabel>();
            return true;
        }
    }
}