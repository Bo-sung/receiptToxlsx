namespace Ragnarok
{
    public class DuplicateLoginDetectedException : NetworkException
    {
        /// <summary>
        /// 로그인 중에 중복 로그인 감지 => 재접속 필요
        /// </summary>
        public override void Execute()
        {
        }
    }
}