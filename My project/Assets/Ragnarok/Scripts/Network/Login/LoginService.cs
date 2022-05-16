using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 로그인 공통 구현부
    /// <see cref="EditorLogin"/>
    /// </summary>
    public abstract class LoginService : ILoginImpl
    {
        protected bool isLogined;
        protected bool isAgreedTerm;

        public event System.Action OnLogoutSuccess;

        public abstract NCommon.LoginType GetLastLoginType();

        public abstract void ShowAgreeDialog();

        public virtual bool IsAgreeTerm()
        {
            return isAgreedTerm;
        }

        public abstract void Login(NCommon.LoginType type);

        public abstract void Logout();

        public virtual bool IsLogin()
        {
            return isLogined;
        }

        public abstract AuthLoginType GetAuthLoginType();

        public string GetUuid()
        {
            return SystemInfo.deviceUniqueIdentifier.Replace("-", "").Substring(0, 10).ToLower();
        }

        public virtual string GetAccountKey()
        {
            return GetUuid();
        }

        public virtual string GetAccountPassword()
        {
            return string.Empty;
        }

        protected void InvokeLogoutSuccess()
        {
            OnLogoutSuccess?.Invoke();
        }
    }
}