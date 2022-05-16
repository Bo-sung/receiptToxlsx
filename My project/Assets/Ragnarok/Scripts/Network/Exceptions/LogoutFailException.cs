namespace Ragnarok
{
    public class LogoutFailException : NetworkException
    {
        /// <summary>
        /// 로그아웃 실패
        /// </summary>
        public LogoutFailException(string message) : base(message)
        {
        }

        public override void Execute()
        {
            UI.ConfirmPopup(Message);
        }
    }
}