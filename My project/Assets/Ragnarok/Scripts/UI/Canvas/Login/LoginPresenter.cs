using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UILogin"/>
    /// </summary>
    public sealed class LoginPresenter : ViewPresenter
    {
        private readonly LoginManager loginManager;

        public event System.Action OnLogin;
        public event System.Action<System.Exception> OnLoginError;

        public LoginPresenter()
        {
            loginManager = LoginManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public async Task LoginAsync(NCommon.LoginType type)
        {
            try
            {
                await loginManager.AsyncLogin(type); // 로그인
                OnLogin?.Invoke();
            }
            catch (System.Exception ex)
            {
                OnLoginError?.Invoke(ex);
            }
        }
    }
}