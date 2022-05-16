namespace Ragnarok
{
    /// <summary>
    /// 기본 로그인 구조
    /// <see cref="LoginService"/>
    /// <see cref="GamePotSystem"/>
    /// </summary>
    public interface ILoginImpl
    {
        event System.Action OnLogoutSuccess;

        /// <summary>
        /// 마지막 로그인 타입
        /// </summary>
        NCommon.LoginType GetLastLoginType();

        /// <summary>
        /// 인증 타입
        /// </summary>
        AuthLoginType GetAuthLoginType();

        /// <summary>
        /// 로그인
        /// </summary>
        void Login(NCommon.LoginType type);

        /// <summary>
        /// 로그아웃
        /// </summary>
        void Logout();

        /// <summary>
        /// 로그인 여부
        /// </summary>
        bool IsLogin();

        /// <summary>
        /// 디바이스 고유 Id 반환
        /// </summary>
        string GetUuid();

        /// <summary>
        /// 인증 AccountKey 반환
        /// </summary>
        string GetAccountKey();

        /// <summary>
        /// 인증 Password 반환
        /// </summary>
        string GetAccountPassword();
    }
}