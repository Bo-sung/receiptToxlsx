namespace Ragnarok
{
    public static class Config
    {
        #region PlayerPrefsName

        /// <summary>
        /// 선택한 서버정보 (선택한서버Index)
        /// </summary>
        public const string SELECT_SERVER = "Select_Server";

        /// <summary>
        /// 약관동의 여부..
        /// </summary>
        public const string CONFIRM_AGREE = "Confirm_Agree";

#if UNITY_EDITOR
        public const string CONNECT_IP = "Connect_Ip";
#endif

        public const string ACCOUNT_KEY = "AccountKey";
        public const string PASSWORD = "Password";

        public const string LANGUAGE_TYPE = "LanguageType";

        /// <summary>
        /// 최근 접속한 캐릭터 ID
        /// </summary>
        public const string CHARACTER_ID = "characterId";

        #endregion

        public const string AuthZone = "ragm_auth";
        public const string GameZone = "ragm";

#if UNITY_ANDROID
        public const string PLATFORTM_NAME = "Android";
#elif UNITY_IOS
        public const string PLATFORTM_NAME = "iOS";
#else
        public const string PLATFORTM_NAME = "unknown";
#endif
    }
}