namespace Ragnarok
{
    public class LoginCancelException : NetworkException
    {
        /// <summary>
        /// 사용자 로그인 취소
        /// </summary>
        public LoginCancelException()
        {
        }

        public override void Execute()
        {
        }
    }
}