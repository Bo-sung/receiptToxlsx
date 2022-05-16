using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class GameServerConfig
    {
        /// <summary>
        /// 현재 선택된 서버 그룹 id
        /// </summary>
        public ObscuredInt selectedGroupId;

        public ObscuredString connectIp;
        public ObscuredInt connectPort;
        public ObscuredInt updateKey;
        public ObscuredInt serverKey;
        public ObscuredInt zoneIndex;

        public ObscuredBool isToss;
        public ObscuredInt userSessionKey;
        public ObscuredString accountKey;
        public ObscuredString password;
        public ObscuredInt linkedLogin;
        public ObscuredString resourceUrl;
        public ObscuredString serverPosition;

        public static string CountryCode { get; private set; } // 접속한지역 국가코드
        public static LanguageType CountryDefaultLanguage { get; private set; } // 접속한지역 기본언어

#if UNITY_EDITOR
        public static bool IsEditorRealServer { get; private set; }
#endif

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            int savedServerGroupId = ObscuredPrefs.GetInt(Config.SELECT_SERVER, -1);
            SelectServer(savedServerGroupId); // 저장된 서버로 선택            
        }

        /// <summary>
        /// 선택된 서버 있는지 여부
        /// </summary>
        public bool HasSelectedServer()
        {
            return selectedGroupId != -1;
        }

        /// <summary>
        /// 서버 선택
        /// </summary>
        public void SelectServer(int groupId)
        {
            selectedGroupId = groupId;
        }

        /// <summary>
        /// 현재 선택한 게임서버 그룹 id 저장
        /// </summary>
        public void SaveServerGroupId()
        {
            ObscuredPrefs.SetInt(Config.SELECT_SERVER, selectedGroupId);
            ObscuredPrefs.Save();
        }

        /// <summary>
        /// 마지막으로 선택한 게임서버 그룹 id 제거
        /// </summary>
        public void DeleteServerGroupId()
        {
            ObscuredPrefs.DeleteKey(Config.SELECT_SERVER);
            ObscuredPrefs.Save();
            SelectServer(-1);
        }

        /// <summary>
        /// 게임서버 연결 정보 세팅
        /// </summary>
        public void SetConnectInfo(GameServerPacket packet, bool isToss, int userSessionKey, string accountKey, string password, int linkedLogin, string resourceUrl, string serverPosition)
        {
            connectIp = packet.connectIP;
            connectPort = packet.connectPort;
            updateKey = packet.updateKey;
            serverKey = packet.severKey;
            zoneIndex = packet.zoneIndex;

            this.isToss = isToss;
            this.userSessionKey = userSessionKey;
            this.accountKey = accountKey;
            this.password = password;
            this.linkedLogin = linkedLogin;
            this.resourceUrl = resourceUrl;
            this.serverPosition = serverPosition;

#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(serverPosition)) // 리얼서버에서 토스로 스테이지 서버로 토스될때 NULL
                IsEditorRealServer = serverPosition.Equals("Real", System.StringComparison.OrdinalIgnoreCase);
#endif
        }

        /// <summary>
        /// 게임서버 연결 정보 세팅
        /// </summary>
        public void SetConnectInfo(string connectIp, int connectPort, int updateKey, int serverKey)
        {
            this.connectIp = connectIp;
            this.connectPort = connectPort;
            this.updateKey = updateKey;
            this.serverKey = serverKey;
        }

        /// <summary>
        /// 지역 코드 정보 세팅
        /// </summary>
        public void SetCountry(string countryCode)
        {
            CountryCode = countryCode;
            CountryDefaultLanguage = GetCountryDefaultLanguage(countryCode);
        }

        /// <summary>
        /// 현재 나라에 해당하는 언어
        /// </summary>
        private LanguageType GetCountryDefaultLanguage(string countryCode)
        {
            foreach (LanguageType item in System.Enum.GetValues(typeof(LanguageType)))
            {
                if (item == LanguageType.None)
                    continue;

                LanguageConfig config = LanguageConfig.GetBytKey(item);
                if (config == null)
                    continue;

                if (string.Equals(countryCode, config.type))
                    return item;
            }

            return LanguageType.ENGLISH;
        }

        /// <summary>
        /// 글로벌 서비스 여부
        /// </summary>
        public static bool IsGlobal()
        {
            return Cheat.IS_GLOBAL;
        }

        /// <summary>
        /// 한국 서비스 전용
        /// </summary>
        public static bool IsKorea()
        {
            return IsGlobal() && string.Equals(CountryCode, LanguageConfig.KOREAN.type);
        }

        /// <summary>
        /// 한국 서비스 전용
        /// </summary>
        public static bool IsKoreaLanguage()
        {
            return IsKorea() && Language.Current == LanguageType.KOREAN;
        }

        /// <summary>
        /// 온버프 여부
        /// </summary>
        public static bool IsOnBuff()
        {
            return BasisOnBuffInfo.Display.GetInt() == 1;
        }
    }
}