using Facebook.Unity.Settings;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Settings", menuName = "ScriptableObjects/ProductSettings")]
    public class ProductSettings : ScriptableObject
    {
        [Header("PlayerSettings")]
        [SerializeField] string productName;
        [SerializeField] Texture2D defaultIcon;
        [SerializeField] Texture2D[] adaptiveBackgroundIcons;
        [SerializeField] Texture2D[] adaptiveForegroundIcons;
        [SerializeField] Texture2D[] legacyIcons;

        [Header("AppName")]
        [SerializeField, RelativePath("xml")] string valuesKoStringsXmlPath; // android 앱이름 세팅(디바이스 한국어)
        [SerializeField, RelativePath("xml")] string valueszhTWStringsXmlPath; // android 앱이름 세팅(디바이스 중국어-번체)
        [SerializeField, RelativePath("Strings")] string infoPlistEnStringsPath; // android 앱이름 세팅
        [SerializeField, RelativePath("Strings")] string infoPlistKoStringsPath; // android 앱이름 세팅(디바이스 한국어)
        [SerializeField, RelativePath("Strings")] string infoPlistzhTWStringsPath; // android 앱이름 세팅(디바이스 중국어-번체)

        [Header("Firebase")]
        [SerializeField, RelativePath("json")] string googleServicesDesktopPath;
        [SerializeField, RelativePath("json")] string googleServicesPath;
        [SerializeField, RelativePath("xml")] string googleServiceXmlPath;
        [SerializeField, RelativePath("plist")] string googleServiceInfoPath;

        [Header("GamePot")]
        [SerializeField, RelativePath("gradle")] string launcherTemplatePath;
        [SerializeField, RelativePath("plist")] string gamePotConfigInfoPath;

        [Header("IronSource(AdMob)")]
        [SerializeField, RelativePath("xml")] string androidManifestPath;
        [SerializeField] string gADApplicationIdentifier;

        [Header("FaceBook")]
        [SerializeField] List<string> appLabels;
        [SerializeField] List<string> appIds;
        [SerializeField] bool isFaceBookFriend;

        [Header("PostProcessBuildValues")]
        [SerializeField] string developmentTeam;
        [SerializeField] string provisioningProfileSpecifier;

        [Header("Presets")]
        [SerializeField] Preset[] presets;

        private Scene scenePreload;

        public void Switch()
        {
            OpenPreloadScene(); // 씬 오픈
            {
                // 프리셋 적용
                foreach (Preset item in presets)
                {
                    ApplyPreset(item);
                }

                // PlayerSettings 세팅
                PlayerSettings.productName = productName;
                SetPackageName();
                SetIcons();

                // 앱 이름 로컬라이징
                ReplaceFile("Assets/Plugins/Android/res/values-ko/strings.xml", valuesKoStringsXmlPath);
                ReplaceFile("Assets/Plugins/Android/res/values-zh-rTW/strings.xml", valueszhTWStringsXmlPath);
                ReplaceFile("Assets/Plugins/iOS/DisplayName/en.lproj/InfoPlist.strings", infoPlistEnStringsPath);
                ReplaceFile("Assets/Plugins/iOS/DisplayName/ko.lproj/InfoPlist.strings", infoPlistKoStringsPath);
                ReplaceFile("Assets/Plugins/iOS/DisplayName/zh-Hant.lproj/InfoPlist.strings", infoPlistzhTWStringsPath);

                // Firebase 세팅
                ReplaceFile("Assets/StreamingAssets/google-services-desktop.json", googleServicesDesktopPath);
                ReplaceFile("Assets/Plugins/Android/google-services.json", googleServicesPath);
                ReplaceFile("Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml", googleServiceXmlPath);
                ReplaceFile("Assets/Plugins/iOS/GoogleService-Info.plist", googleServiceInfoPath);

                // GamePot 세팅
                ReplaceFile("Assets/Plugins/Android/launcherTemplate.gradle", launcherTemplatePath);
                ReplaceFile("Assets/Plugins/iOS/GamePotConfig-Info.plist", gamePotConfigInfoPath);

                // IronSource 세팅
                ReplaceFile("Assets/Plugins/Android/AndroidManifest.xml", androidManifestPath); // Android AdMob 앱ID 변경
                PostProcessBuildSettings.gADApplicationIdentifier = gADApplicationIdentifier; // iOS AdMob 앱ID 변경

                // FaceBook 세팅
                SetFaceBookSettings();

                // PostProcessBuild 세팅
                PostProcessBuildSettings.developmentTeam = developmentTeam;
                PostProcessBuildSettings.provisioningProfileSpecifier = provisioningProfileSpecifier;
            }
            SavePreloadScene(); // 씬 저장

            AssetDatabase.SaveAssets();
        }

        private void SetPackageName()
        {
            if (string.IsNullOrEmpty(googleServicesPath))
                return;

            string path = PathUtils.GetPath(googleServicesPath, PathUtils.PathType.Relative);
            string json = System.IO.File.ReadAllText(path);
            GoogleServicesJson googleServices = JsonUtility.FromJson<GoogleServicesJson>(json);
            if (googleServices == null)
                return;

            string androidPackageName = googleServices.GetAndroidPackageName();
            string iosBundleId = googleServices.GetIosBundleId();
            string iosAppStoreId = googleServices.GetIosAppStoreId();
            string storeUrl;
#if UNITY_ANDROID
            storeUrl = $"market://details?id={androidPackageName}";
#elif UNITY_IOS || UNITY_IPHONE
            storeUrl = $"itms-apps://itunes.apple.com/app/appcrafthd/id{iosAppStoreId}";
#else
            storeUrl = string.Empty;
#endif

            // Set Package Name
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, androidPackageName);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, iosBundleId);

            // Set Store Url
            BuildSettings buildSettings = GetComponent<BuildSettings>();
            SerializedObject so = new SerializedObject(buildSettings);
            SerializedProperty sp = so.FindProperty("storeUrl");
            sp.stringValue = storeUrl;
            so.ApplyModifiedProperties();

            Debug.Log($"androidPackageName: {androidPackageName}");
            Debug.Log($"iosBundleId: {iosBundleId}");
            Debug.Log($"iosAppStoreId: {iosAppStoreId}");
            Debug.Log($"StoreUrl: {storeUrl}");
        }

        private void SetIcons()
        {
            // Default
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { defaultIcon });

            // Android - Adaptive
            PlatformIcon[] platformAdaptiveIcons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive);
            for (int i = 0; i < platformAdaptiveIcons.Length; i++)
            {
                platformAdaptiveIcons[i].SetTexture(i < adaptiveBackgroundIcons.Length ? adaptiveBackgroundIcons[i] : null, layer: 0);
                platformAdaptiveIcons[i].SetTexture(i < adaptiveForegroundIcons.Length ? adaptiveForegroundIcons[i] : null, layer: 1);
            }
            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive, platformAdaptiveIcons);

            // Android - Legacy
            PlatformIcon[] platformLegacyIcons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Legacy);
            for (int i = 0; i < platformLegacyIcons.Length; i++)
            {
                platformLegacyIcons[i].SetTexture(i < legacyIcons.Length ? legacyIcons[i] : null);
            }
            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Legacy, platformLegacyIcons);
        }

        private void ReplaceFile(string target, string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                Debug.Log($"파일 제거: {target}");
                FileUtil.DeleteFileOrDirectory(target);
            }
            else
            {
                string source = PathUtils.GetPath(result, PathUtils.PathType.Relative);
                Debug.Log($"파일 교체: {source} => {target}");
                FileUtil.ReplaceFile(source, target);
            }

            Refresh(target);
        }

        private void ReplaceDirectory(string target, string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                Debug.Log($"폴더 제거: {target}");
                FileUtil.DeleteFileOrDirectory(target);
            }
            else
            {
                string source = PathUtils.GetPath(result, PathUtils.PathType.Relative);
                Debug.Log($"폴더 교체: {source} => {target}");
                FileUtil.ReplaceDirectory(result, target);
            }

            Refresh(target);
        }

        private void Refresh(string path)
        {
            AssetDatabase.Refresh();

            Object obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null)
                return;

            EditorUtility.SetDirty(obj);
        }

        private void OpenPreloadScene()
        {
            // 이미 Load 되어있는 씬
            if (scenePreload.isLoaded)
                return;

            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
                return;

            string path = EditorBuildSettings.scenes[0].path;
            scenePreload = EditorSceneManager.OpenScene(path);
        }

        private void SavePreloadScene()
        {
            if (!scenePreload.isLoaded)
                return;

            EditorSceneManager.SaveScene(scenePreload);
        }

        private T GetComponent<T>()
            where T : Component
        {
            if (!scenePreload.isLoaded)
                return null;

            GameObject[] rootGameObjects = scenePreload.GetRootGameObjects();
            if (rootGameObjects == null)
                return null;

            foreach (GameObject item in rootGameObjects)
            {
                T t = item.GetComponentInChildren<T>();
                if (t != null)
                    return t;
            }

            return null;
        }

        private void ApplyPreset(Preset preset)
        {
            if (preset == null)
                return;

            if (!scenePreload.isLoaded)
                return;

            GameObject[] rootGameObjects = scenePreload.GetRootGameObjects();
            if (rootGameObjects == null)
                return;

            foreach (GameObject item in rootGameObjects)
            {
                ApplyPresetRecursively(item.transform, preset);
            }
        }

        /// <summary>
        /// 재귀함수로 프리셋 적용 가능한 모든 Object를 찾아서 적용시킵니다.
        /// </summary> 
        private void ApplyPresetRecursively(Transform tf, Preset preset)
        {
            Component find = tf.GetComponent(preset.GetTargetTypeName());
            if (find != null && preset.ApplyTo(find))
            {
                Debug.Log($"프리셋 적용: {NGUITools.GetHierarchy(find.gameObject)} ({preset.GetTargetTypeName()})");
            }

            // 재귀함수를 통하여 모든 Transform 의 name 을 찾음
            for (int i = 0; i < tf.childCount; ++i)
            {
                ApplyPresetRecursively(tf.GetChild(i), preset);
            }
        }

        /// <summary>
        /// 페이스북 세팅
        /// </summary>
        private void SetFaceBookSettings()
        {
            FacebookSettings.AppLabels = appLabels;
            FacebookSettings.AppIds = appIds;

            BuildSettings buildSettings = GetComponent<BuildSettings>();
            SerializedObject so = new SerializedObject(buildSettings);
            SerializedProperty sp = so.FindProperty("isFaceBookFriend");
            sp.boolValue = isFaceBookFriend;
            so.ApplyModifiedProperties();
        }
       
        [System.Serializable]
        private class GoogleServicesJson
        {
            private const int APPLE_PLATFORM = 2;

            [System.Serializable]
            public class Client
            {
                [System.Serializable]
                public class ClientInfo
                {
                    [System.Serializable]
                    public class AndroidClientInfo
                    {
                        public string package_name;
                    }

                    public AndroidClientInfo android_client_info;
                }

                [System.Serializable]
                public class Services
                {
                    [System.Serializable]
                    public class AppinviteService
                    {
                        [System.Serializable]
                        public class OtherPlatformOauthClient
                        {
                            [System.Serializable]
                            public class IosInfo
                            {
                                public string bundle_id;
                                public string app_store_id;
                            }

                            public int client_type;
                            public IosInfo ios_info;
                        }

                        public List<OtherPlatformOauthClient> other_platform_oauth_client;
                    }

                    public AppinviteService appinvite_service;
                }

                public ClientInfo client_info;
                public Services services;
            }

            public List<Client> client;

            public string GetAndroidPackageName()
            {
                for (int i = 0; i < client.Count; i++)
                {
                    return client[i].client_info.android_client_info.package_name;
                }

                return string.Empty;
            }

            public string GetIosBundleId()
            {
                for (int i = 0; i < client.Count; i++)
                {
                    for (int j = 0; j < client[i].services.appinvite_service.other_platform_oauth_client.Count; j++)
                    {
                        if (client[i].services.appinvite_service.other_platform_oauth_client[j].client_type != APPLE_PLATFORM)
                            continue;

                        return client[i].services.appinvite_service.other_platform_oauth_client[j].ios_info.bundle_id;
                    }
                }

                return string.Empty;
            }

            public string GetIosAppStoreId()
            {
                for (int i = 0; i < client.Count; i++)
                {
                    for (int j = 0; j < client[i].services.appinvite_service.other_platform_oauth_client.Count; j++)
                    {
                        if (client[i].services.appinvite_service.other_platform_oauth_client[j].client_type != APPLE_PLATFORM)
                            continue;

                        return client[i].services.appinvite_service.other_platform_oauth_client[j].ios_info.app_store_id;
                    }
                }

                return string.Empty;
            }
        }
    }
}