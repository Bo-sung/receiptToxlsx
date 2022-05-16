using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public enum AuthLoginType : byte
    {
        None = 0,
        INPUT = 1,
        LINE = 2,
        FACEBOOK = 3,
        GOOGLE = 4,
        GAME_CENTER = 5,
        EMAIL = 6,
        GAME_POT = 7,

        GUEST_LOGIN = 99,
    }

    public enum StoreType : byte
    {
        GOOGLE = 0,
        APPLE = 1,
        TSTORE = 2,
    }

    public class LoginManager : Singleton<LoginManager>
    {
#if UNITY_EDITOR
        public static bool IsUseInputLogin
        {
            get => UnityEditor.EditorPrefs.GetBool("AccountUse", defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool("AccountUse", value);
        }

        public static int ServerGroup
        {
            get => UnityEditor.EditorPrefs.GetInt("EditorServerGroup", defaultValue: 1);
            set => UnityEditor.EditorPrefs.SetInt("EditorServerGroup", value);
        }

        public static EditorInputAccount InputAccountInfo
        {
            get
            {
                string json = UnityEditor.EditorPrefs.GetString("AccountData", string.Empty);
                return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<EditorInputAccount>(json);
            }
            set
            {
                UnityEditor.EditorPrefs.SetString("AccountData", JsonUtility.ToJson(value));
            }
        }
#endif

        private readonly ILoginImpl loginImpl;

        public event System.Action OnLogout;

        public LoginManager()
        {
#if UNITY_EDITOR
            loginImpl = new EditorLogin();
#else
            loginImpl = GamePotSystem.Instance;
#endif

            loginImpl.OnLogoutSuccess += OnLogoutSuccess;
        }

        ~LoginManager()
        {
            loginImpl.OnLogoutSuccess -= OnLogoutSuccess;
        }

        protected override void OnTitle()
        {
        }

        /// <summary>
        /// 마지막 로그인 타입
        /// </summary>
        public NCommon.LoginType GetLastLoginType()
        {
            return loginImpl.GetLastLoginType();
        }

        /// <summary>
        /// 마지막 계정 로그인 시도
        /// </summary>
        public async Task LastAutoLogin()
        {
            NCommon.LoginType lastLoginType = GetLastLoginType();
            await AsyncLogin(lastLoginType);
        }

        /// <summary>
        /// 계정 로그인
        /// </summary>
        public async Task AsyncLogin(NCommon.LoginType type)
        {
            loginImpl.Login(type);
            await new WaitUntil(loginImpl.IsLogin);
        }

        /// <summary>
        /// 계정 로그아웃
        /// </summary>
        public async Task AsyncLogOut()
        {
            // 이미 로그아웃 되어있을 경우
            if (!loginImpl.IsLogin())
                return;

            loginImpl.Logout();
            await new WaitWhile(loginImpl.IsLogin);
        }

        /// <summary>
        /// 인증 AccountKey
        /// </summary>
        public string GetAccountKey()
        {
            return loginImpl.GetAccountKey();
        }

        /// <summary>
        /// 인증 Password
        /// </summary>
        public string GetAccountPassword()
        {
            return loginImpl.GetAccountPassword();
        }

        public string GetUuid()
        {
            return loginImpl.GetUuid();
        }

        public AuthLoginType GetAuthLoginType()
        {
            return loginImpl.GetAuthLoginType();
        }

        void OnLogoutSuccess()
        {
            OnLogout?.Invoke();
        }

#if UNITY_EDITOR
        public static string LastAccountKey => ObscuredPrefs.GetString(Config.ACCOUNT_KEY);

        public static string LastAccountPassword => ObscuredPrefs.GetString(Config.PASSWORD);

        public static bool HasLastLogin => ObscuredPrefs.HasKey(Config.ACCOUNT_KEY) && ObscuredPrefs.HasKey(Config.PASSWORD);

        public static void SaveAccountInfo(string accountKey, string password)
        {
            ObscuredPrefs.SetString(Config.ACCOUNT_KEY, accountKey);
            ObscuredPrefs.SetString(Config.PASSWORD, password);
            ObscuredPrefs.Save();
        }

        public static void DeleteAccountInfo()
        {
            ObscuredPrefs.DeleteKey(Config.ACCOUNT_KEY);
            ObscuredPrefs.DeleteKey(Config.PASSWORD);
            ObscuredPrefs.Save();
        }
#endif
    }
}