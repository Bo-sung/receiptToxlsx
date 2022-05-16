#if UNITY_EDITOR
namespace Ragnarok
{
    public class EditorLogin : LoginService
    {
        private AuthLoginType authLoginType;
        private string accountKey;
        private string accountPassword;

        private bool UseInput => LoginManager.IsUseInputLogin || LoginManager.HasLastLogin;

        public override NCommon.LoginType GetLastLoginType()
        {
            // Input 로그인을 이용할 경우 => 마지막으로 로그인 한 타입을 대충 Guest 로 보여줌
            if (UseInput)
                return NCommon.LoginType.GUEST;

            return NCommon.LoginType.NONE;
        }

        public override void ShowAgreeDialog()
        {
            isAgreedTerm = true;
        }

        public override void Login(NCommon.LoginType type)
        {
            // Input 로그인 사용
            if (UseInput)
            {
                authLoginType = AuthLoginType.INPUT; // 인증타입: Input

                if (LoginManager.IsUseInputLogin && LoginManager.InputAccountInfo != null)
                {
                    EditorInputAccount.Tuple tuple = LoginManager.InputAccountInfo.GetSelectedAccountInfo();
                    accountKey = tuple.id;
                    accountPassword = tuple.pw;
                }
                else
                {
                    accountKey = LoginManager.LastAccountKey;
                    accountPassword = LoginManager.LastAccountPassword;
                }
            }
            else
            {
                authLoginType = AuthLoginType.GUEST_LOGIN; // 인증타입: Guest

                accountKey = GetUuid();
                accountPassword = string.Empty;
            }

            isLogined = true;
        }

        public override void Logout()
        {
            LoginManager.DeleteAccountInfo();
            isLogined = false;

            InvokeLogoutSuccess(); // 로그아웃 완료
        }

        public override AuthLoginType GetAuthLoginType()
        {
            return authLoginType;
        }

        public override string GetAccountKey()
        {
            return accountKey;
        }

        public override string GetAccountPassword()
        {
            return accountPassword;
        }
    }
}
#endif