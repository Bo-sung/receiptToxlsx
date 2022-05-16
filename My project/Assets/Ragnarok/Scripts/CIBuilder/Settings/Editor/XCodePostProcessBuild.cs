#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Ragnarok
{
    public static class XCodePostProcessBuild
    {
        [PostProcessBuildAttribute(100)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
            {
                Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
                return;
            }

            Debug.Log("[XCodePostProcessBuild] Starting to perform post build tasks for iOS platform.");

            string projectPath = PBXProject.GetPBXProjectPath(buildPath);// pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string targetGuid = project.GetUnityMainTargetGuid();
            string targetUnityFrameworkGuid = project.GetUnityFrameworkTargetGuid();

            // USYM_UPLOAD_AUTH_TOKEN 세팅
            var token = project.GetBuildPropertyForAnyConfig(targetGuid, "USYM_UPLOAD_AUTH_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                token = "5ed9f076d3f43c924c21d3e5e1e6387b22c158c95ca79efd032ca578c77adb49";
            }
            project.SetBuildProperty(targetGuid, "USYM_UPLOAD_AUTH_TOKEN", token);
            project.SetBuildProperty(targetUnityFrameworkGuid, "USYM_UPLOAD_AUTH_TOKEN", token);
            project.SetBuildProperty(project.ProjectGuid(), "USYM_UPLOAD_AUTH_TOKEN", token);

            // Framework 추가
            project.AddFrameworkToProject(targetGuid, "StoreKit.framework", false);

            project.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", PostProcessBuildSettings.developmentTeam);

            if (string.IsNullOrEmpty(PostProcessBuildSettings.provisioningProfileSpecifier))
            {
                Debug.Log("[XCodePostProcessBuild] provisioningProfileSpecifier IsNullOrEmpty");
            }
            else
            {
                project.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", PostProcessBuildSettings.provisioningProfileSpecifier);
            }

            // ipa 업로드 오류 관련 세팅 추가
            // https://forum.unity.com/threads/2019-3-validation-on-upload-to-store-gives-unityframework-framework-contains-disallowed-file.751112/
            project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            project.SetBuildProperty(targetUnityFrameworkGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

            // iOS 14.2 대응
            Debug.Log("[XCodePostProcessBuild] Applying iOS 14.2 workaround. Remove me once Unity has patched this.");
            project.AddFrameworkToProject(targetGuid, "UnityFramework.framework", false);

            // 애드저스트 iOS 14 지원
            project.AddFrameworkToProject(targetUnityFrameworkGuid, "StoreKit.framework", true);
            project.AddFrameworkToProject(targetUnityFrameworkGuid, "AppTrackingTransparency.framework", true);

            // Facebook 8.0.0 충돌관련
            project.AddFrameworkToProject(targetGuid, "Accelerate.framework", false);
            project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-lz");
            project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-lstdc++");
            project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-lc++");

            // 언어 세팅
            Debug.Log("[XCodePostProcessBuild] 언어세팅");
            CopyAndReplaceDirectory("Assets/Plugins/iOS/DisplayName/en.lproj", Path.Combine(buildPath, "en.lproj"));
            project.AddFileToBuild(targetGuid, project.AddFile("en.lproj", "en.lproj", PBXSourceTree.Source));

            CopyAndReplaceDirectory("Assets/Plugins/iOS/DisplayName/ko.lproj", Path.Combine(buildPath, "ko.lproj"));
            project.AddFileToBuild(targetGuid, project.AddFile("ko.lproj", "ko.lproj", PBXSourceTree.Source));

            project.WriteToFile(projectPath);

            // Capability 추가
            var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
            manager.AddInAppPurchase();
            manager.AddSignInWithApple();
            manager.AddPushNotifications(development: false);
            manager.WriteToFile();

            // Info.plist file Setting
            var plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();

            plist.ReadFromFile(plistPath);

            PlistElementDict dict = plist.root.AsDict();

            // 수출 규정 준수 세팅
            dict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // http 허용 세팅
            PlistElementDict NSAppTransportSecurity = dict.CreateDict("NSAppTransportSecurity");
            NSAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);
            if (dict.values.ContainsKey("NSAllowsArbitraryLoadsInWebContent"))
            {
                dict.values.Remove("NSAllowsArbitraryLoadsInWebContent");
            }

            // 아이언 소스 info 세팅
            PlistElementArray SKAdNetworkItems = dict.CreateArray("SKAdNetworkItems");

            PlistElementDict SKAdNetworkItem_ironSource = SKAdNetworkItems.AddDict();
            SKAdNetworkItem_ironSource.SetString("SKAdNetworkIdentifier", "SU67R6K2V3.skadnetwork");

            PlistElementDict SKAdNetworkItem_UnityAds = SKAdNetworkItems.AddDict();
            SKAdNetworkItem_UnityAds.SetString("SKAdNetworkIdentifier", "4DZT52R2T5.skadnetwork");

            // 아이언소스 페이스북 적용 보류
            PlistElementDict SKAdNetworkItem_FAN1 = SKAdNetworkItems.AddDict();
            SKAdNetworkItem_FAN1.SetString("SKAdNetworkIdentifier", "v9wttpbfk9.skadnetwork");
            PlistElementDict SKAdNetworkItem_FAN2 = SKAdNetworkItems.AddDict();
            SKAdNetworkItem_FAN2.SetString("SKAdNetworkIdentifier", "n38lu8286q.skadnetwork");

            PlistElementDict SKAdNetworkItem_AdMob = SKAdNetworkItems.AddDict();
            SKAdNetworkItem_AdMob.SetString("SKAdNetworkIdentifier", "cstr6suwn9.skadnetwork");
            // 아이언소스 미디에이션 용, 애드몹 앱 ID
            if (string.IsNullOrEmpty(PostProcessBuildSettings.gADApplicationIdentifier))
            {
                Debug.Log("[XCodePostProcessBuild] GADApplicationIdentifier IsNullOrEmpty");
            }
            else
            {
                dict.SetString("GADApplicationIdentifier", PostProcessBuildSettings.gADApplicationIdentifier);
            }

            // iOS 14 버전부터 IDFA 값 획득 시 사용자에게 권한
            dict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");

            // 카메라 권한(네이버 라운지에서 사용)
            dict.SetString("NSCameraUsageDescription", "");
            // 사진 권한(네이버 라운지에서 사용)
            dict.SetString("NSPhotoLibraryUsageDescription", "");

            // 네이버 라운지 URL Schemes 값 세팅
            PlistElementArray array = plist.root.values["CFBundleURLTypes"].AsArray();
            var urlDict = array.AddDict();
            urlDict.SetString("CFBundleURLName", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            urlDict.CreateArray("CFBundleURLSchemes").AddString("ROLabyrinth");

            // AppsFlyer SKAN 추가 https://support.appsflyer.com/hc/ko/articles/4402320969617
            dict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");

            // Apply editing settings to Info.plist
            plist.WriteToFile(plistPath);

            Debug.Log("[XCodePostProcessBuild] End");
        }

        static void CopyAndReplaceDirectory(string srcPath, string dstPath)
        {
            if (Directory.Exists(dstPath))
                Directory.Delete(dstPath);

            if (File.Exists(dstPath))
                File.Delete(dstPath);

            Directory.CreateDirectory(dstPath);

            string[] exclude = new string[] { "^.*.meta$", "^.*.mdown^", "^.*.pdf$" };
            string regexExclude = string.Format(@"{0}", string.Join("|", exclude));
            foreach (string file in Directory.GetFiles(srcPath))
            {

                if (Regex.IsMatch(file, regexExclude))
                {
                    continue;
                }

                File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
            }

            foreach (var dir in Directory.GetDirectories(srcPath))
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }
}
#endif