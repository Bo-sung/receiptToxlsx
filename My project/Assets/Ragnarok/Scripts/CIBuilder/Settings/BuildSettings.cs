using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BuildSettings : GameObjectSingleton<BuildSettings>
    {
        [SerializeField]
        [Tooltip("빌드 버전")]
        string buildVersion;

        [SerializeField]
        [Tooltip("앱버전")]
        int appVersion;

        [SerializeField]
        [Tooltip("에셋 버전")]
        int assetVersion;

        [SerializeField]
        [Tooltip("개발자모드")]
        bool isDevelopment;

        [SerializeField]
        [Tooltip("스토어 타입")]
        StoreType storeType;

        [SerializeField]
        [Tooltip("스토어 url 주소")]
        string storeUrl;

        [SerializeField]
        [Tooltip("서버 세팅 정보")]
        AuthServerConfig serverConfig;

        [SerializeField]
        [Tooltip("어셋번들 세팅 정보")]
        AssetBundleSettings assetBundleSettings;

        [SerializeField]
        [Tooltip("로고 이름")]
        string logoName;

        [SerializeField]
        [Tooltip("페이스북 사용")]
        bool isFaceBookFriend;

        public string BuildVersion
        {
            get { return buildVersion; }
            set { buildVersion = value; }
        }

        public int AppVersion
        {
            get { return appVersion; }
            set { appVersion = value; }
        }

        public int AssetVersion
        {
            get { return assetVersion; }
            set { assetVersion = value; }
        }

        public StoreType StoreType
        {
            get { return storeType; }
        }

        public AuthServerConfig ServerConfig
        {
            get { return serverConfig; }
        }

        public AssetLoader GetAssetLoader()
        {
            return assetBundleSettings.GetAssetLoader(assetVersion);
        }

        public AssetBundleSettings GetAssetBundleSettings()
        {
            return assetBundleSettings;
        }

        public void SetAssetBundleSettingsMode(AssetBundleSettings.Mode mode)
        {
            assetBundleSettings.SetMode(mode);
        }

        public bool IsFaceBookFriend => isFaceBookFriend;

        public bool IsDevelopment => isDevelopment;
        public bool IsStreamingAssets => assetBundleSettings.GetMode() == AssetBundleSettings.Mode.StreamingAssets;

        public string GetLogoName()
        {
            return logoName;
        }

        /// <summary>
        /// 리뷰남기기
        /// </summary>
        public void ShowReviewPopup()
        {
            AsyncGoToStore().WrapUIErrors();
        }

        private async Task AsyncGoToStore()
        {
            // 이미 리뷰쓰기에 동의했을 경우
            if (LocalValue.IsAgreedReview)
                return;

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            {
                if (string.IsNullOrEmpty(storeUrl))
                    return;

                string message = GetStoreMessage();
                string confirmText = LocalizeKey._804.ToText(); // 리뷰 남기기
                string cancelText = LocalizeKey._805.ToText(); // 나중에
                if (!await UI.SelectPopup(message, confirmText, cancelText))
                    return;

                LocalValue.IsAgreedReview = true; // 리뷰쓰기 동의
                Application.OpenURL(storeUrl);
            }
        }

        private string GetStoreMessage()
        {
#if UNITY_ANDROID
            return LocalizeKey._806.ToText(); // Google Play에 소중한 리뷰를 남겨주세요.
#else
            return LocalizeKey._803.ToText(); // 스토어에 소중한 리뷰를 남겨주세요.
#endif
        }

        protected override void OnTitle()
        {
        }
    }
}