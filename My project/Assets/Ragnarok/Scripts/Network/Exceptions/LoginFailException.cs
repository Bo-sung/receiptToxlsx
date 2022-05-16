namespace Ragnarok
{
    public class LoginFailException : NetworkException
    {
        /// <summary>
        /// 로그인 실패
        /// </summary>
        public LoginFailException(string message) : base(message)
        {
        }

        public override void Execute()
        {
            UI.ConfirmPopup(Message);
        }
    }
}