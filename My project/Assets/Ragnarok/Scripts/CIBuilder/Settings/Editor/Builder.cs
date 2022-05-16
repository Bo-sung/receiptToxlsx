using LunarConsoleEditorInternal;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ragnarok
{
    public static class Builder
    {
        public const string BaseMenu = "라그나로크/";
        public const string BuildMenu = "빌드/";

        private const string LocalServer242 = "로컬(242)서버/";
        private const string LocalServer249 = "로컬(249)서버/";
        private const string TestServer = "TEST서버/";
        private const string RealServer = "Real서버/";
        private const string StageServer = "Stage서버/";

        private const string ReleaseBuild = "Build";
        private const string DebugBuild = "Build(Debug)";
        private const string ReleaseAppGuardBuild = "Build(AppGuard)";

        private const string LocalServer242Release = BaseMenu + BuildMenu + LocalServer242 + ReleaseBuild;
        private const string LocalServer242Debug = BaseMenu + BuildMenu + LocalServer242 + DebugBuild;
        private const string LocalServer242ReleaseAppGuard = BaseMenu + BuildMenu + LocalServer242 + ReleaseAppGuardBuild;

        private const string LocalServer249Release = BaseMenu + BuildMenu + LocalServer249 + ReleaseBuild;
        private const string LocalServer249Debug = BaseMenu + BuildMenu + LocalServer249 + DebugBuild;
        private const string LocalServer249ReleaseAppGuard = BaseMenu + BuildMenu + LocalServer249 + ReleaseAppGuardBuild;

        private const string TestServerRelease = BaseMenu + BuildMenu + TestServer + ReleaseBuild;
        private const string TestServerDebug = BaseMenu + BuildMenu + TestServer + DebugBuild;
        private const string TestServerReleaseAppGuard = BaseMenu + BuildMenu + TestServer + ReleaseAppGuardBuild;

        private const string RealServerReleaseAppGuard = BaseMenu + BuildMenu + RealServer + ReleaseAppGuardBuild;

        private const string StageServerReleaseAppGuard = BaseMenu + BuildMenu + StageServer + ReleaseAppGuardBuild;

        private const string OpenScene = BaseMenu + "첫씬 열기";

        private const string Scene_PreLoad = "Assets/Ragnarok/Scenes/PreLoad.unity";
        private const string KeyStorePath = "/Keystore/RagCube.keystore";
        private const string keyaliasName = "ragcube";
        private const string KeyStorePass = "funigloo!@#123";
        private const string BASE_APK_NAME = "Labyrinth";

        private const string PresetLocalServer242 = "LocalServer242";
        private const string PresetLocalServer242Debug = "LocalServer242Debug";
        private const string PresetLocalServer249 = "LocalServer249";
        private const string PresetLocalServer249Debug = "LocalServer249Debug";
        private const string PresetTestServer = "TestServer";
        private const string PresetTestServerDebug = "TestServerDebug";
        private const string PresetRealServer = "RealServer";
        private const string PresetStageServer = "StageServer";

        private const string Preset_iOS = "iOS";

        private const BuildOptions Development = BuildOptions.CompressWithLz4HC | BuildOptions.Development;
        private const BuildOptions Release = BuildOptions.CompressWithLz4HC | BuildOptions.None;

        private static string buildBasePath;
        private static string serverName;

        [MenuItem(OpenScene)]
        public static void OpenScenePreLoad()
        {
            EditorSceneManager.OpenScene(Scene_PreLoad);
        }

        public static void BuildAndroid(string presetName, bool isAppGuard = false)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return;

            BuildSettings settings = null;

            if (Application.isBatchMode)
            {
                // 프로젝트 변경
                string production = CommandLineReader.GetCustomArgument("production");
                if (!ProjectSwitch.Switch(production))
                    return;

                // 페이스북 앱가드 미적용
                if (production.Contains("Facebook"))
                    isAppGuard = false;

                // 서버 프리셋 세팅
                if (!string.IsNullOrEmpty(production))
                {
                    serverName = $"{presetName}_{production}";
                    settings = SetBuildSetting(serverName);
                }
            }

            if (settings == null)
            {
                serverName = presetName;
                settings = SetBuildSetting(presetName);
            }

            if (settings == null)
                return;

            Debug.Log("[Builder] Log 설정");
            if (settings.IsDevelopment)
            {
                SetLog(StackTraceLogType.ScriptOnly);
            }
            else
            {
                SetLog(StackTraceLogType.None);
            }

            Debug.Log("[Builder] 프로젝트 세팅");
            SetProjectSetting();

            Debug.Log("[Builder] Keystore 설정");
            SetKeyStore();

            if (!BundleBuild(settings.IsStreamingAssets))
                return;

            Debug.Log("[Builder] 빌드 옵션 설정");
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = FindEnabledEditorScenes();
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = settings.IsDevelopment ? Development : Release;
            buildPlayerOptions.locationPathName = tempApk;

            Debug.Log("[Builder] 빌드 시작");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            SetLog(StackTraceLogType.ScriptOnly);

            if (!isAppGuard)
            {
                if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    System.Diagnostics.Process.Start(buildBasePath); // 폴더 열기     
                }
                else
                {
                    Debug.LogError($"[Builder] 빌드 실패 = {report.summary.result}");
                }
            }
            else
            {
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                    throw new System.Exception($"[Builder] 빌드 실패 = {report.summary.result}");

                // 앱가드 적용
                EditorUtility.DisplayProgressBar("Protect Apk", "Wait few seconds...", (float)0.7);
                ProtectAppGuard(tempApk);
                //DeleteTempFile();
                EditorUtility.ClearProgressBar();
                System.Diagnostics.Process.Start(buildBasePath); // 폴더 열기     
            }
        }

        public static void BuildIOS(string presetName)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                return;

            BuildSettings settings = null;

            if (Application.isBatchMode)
            {
                // 프로젝트 변경
                string production = CommandLineReader.GetCustomArgument("production");
                if (!ProjectSwitch.Switch(production))
                    return;

                // 서버 프리셋 세팅
                if (!string.IsNullOrEmpty(production))
                {
                    serverName = $"{presetName}_{production}";
                    settings = SetBuildSetting(serverName);
                }
            }

            if (settings == null)
            {
                serverName = presetName;
                settings = SetBuildSetting(presetName);
            }

            if (settings == null)
                return;

            Debug.Log("[Builder] Log 설정");
            if (settings.IsDevelopment)
            {
                SetLog(StackTraceLogType.ScriptOnly);
                // LunarConsole Enable
                Installer.EnablePlugin();
                // LunarConsole Prefab Install
                Installer.Install(silent: true);
            }
            else
            {
                SetLog(StackTraceLogType.None);
                // LunarConsole Disable
                Installer.DisablePlugin();
            }
            SetProjectSetting();

            if (Application.isBatchMode)
            {
                // 프로젝트 변경
                string production = CommandLineReader.GetCustomArgument("production");
                bool isSimulator = CommandLineReader.GetCustomArgument("IsSimulator").Equals("true");

                PlayerSettings.iOS.sdkVersion = isSimulator ? iOSSdkVersion.SimulatorSDK : iOSSdkVersion.DeviceSDK;

                if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.DeviceSDK)
                {
                    // 글로벌 서버 프리셋 세팅
                    if (production.Contains("Global"))
                    {
                        buildBasePath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/XCode_Global";
                    }
                    else
                    {
                        buildBasePath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/XCode";
                    }
                }
                else
                {
                    buildBasePath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/XCode_Simulator";
                }
            }
            else
            {
                buildBasePath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/XCode";
            }

            Debug.Log("[Builder] 빌드 옵션 설정");
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = FindEnabledEditorScenes();
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = settings.IsDevelopment ? Development : Release;
            buildPlayerOptions.locationPathName = buildBasePath;

            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            if (!BundleBuild(settings.IsStreamingAssets))
                return;

            Debug.Log("[Builder] 빌드 시작");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            SetLog(StackTraceLogType.ScriptOnly);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                System.Diagnostics.Process.Start(buildBasePath); // 폴더 열기     
            }
            else
            {
                Debug.LogError($"[Builder] 빌드 실패 = {report.summary.result}");
            }
        }

        [MenuItem(LocalServer242Release)]
        public static void BuildLocalServer()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetLocalServer242);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetLocalServer242}_{Preset_iOS}");
            }
        }

        [MenuItem(LocalServer242Debug)]
        public static void BuildLocalServerDebug()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetLocalServer242Debug);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetLocalServer242Debug}_{Preset_iOS}");
            }
        }

        [MenuItem(LocalServer242ReleaseAppGuard)]
        public static void BuildLocalServerAppGuard()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetLocalServer242, isAppGuard: true);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetLocalServer242}_{Preset_iOS}");
            }
        }

        [MenuItem(LocalServer249Release)]
        public static void BuildDevServer2()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetLocalServer249);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetLocalServer249}_{Preset_iOS}");
            }
        }

        [MenuItem(LocalServer249Debug)]
        public static void BuildDevServer2Debug()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetLocalServer249Debug);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetLocalServer249Debug}_{Preset_iOS}");
            }
        }

        [MenuItem(LocalServer249ReleaseAppGuard)]
        public static void BuildDevServer2AppGuard()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetLocalServer249, isAppGuard: true);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetLocalServer249}_{Preset_iOS}");
            }
        }

        [MenuItem(TestServerRelease)]
        public static void BuildTestServer()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetTestServer);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetTestServer}_{Preset_iOS}");
            }
        }

        [MenuItem(TestServerDebug)]
        public static void BuildTestServerDebug()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetTestServerDebug);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetTestServerDebug}_{Preset_iOS}");
            }
        }

        [MenuItem(TestServerReleaseAppGuard)]
        public static void BuildTestServerAppGuard()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetTestServer, isAppGuard: true);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetTestServer}_{Preset_iOS}");
            }
        }

        [MenuItem(RealServerReleaseAppGuard)]
        public static void BuildRealServerAppGuard()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetRealServer, isAppGuard: true);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetRealServer}_{Preset_iOS}");
            }
        }

        [MenuItem(StageServerReleaseAppGuard)]
        public static void BuildStageServerAppGuard()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                BuildAndroid(PresetStageServer, isAppGuard: true);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                BuildIOS($"{PresetStageServer}_{Preset_iOS}");
            }
        }

        private static string[] FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }

            return EditorScenes.ToArray();
        }

        /// <summary>
        /// PreLoad씬 오픈 && 빌드세팅 프리셋 적용
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        private static BuildSettings SetBuildSetting(string presetName)
        {
            var scene = EditorSceneManager.OpenScene(Scene_PreLoad);

            // 씬 오브젝트 가져오기
            var objects = scene.GetRootGameObjects();

            // 빌드 세팅 프리셋 적용
            BuildSettings buildSettings = null;
            GameObject tools = null;
            foreach (var item in objects)
            {
                if (item.name.Equals("Tools"))
                {
                    tools = item;
                }
                if (item.GetComponent<BuildSettings>())
                {
                    buildSettings = item.GetComponent<BuildSettings>();
                }
            }

            if (tools != null)
            {
                GameObject.DestroyImmediate(tools);
            }

            if (buildSettings == null)
                return null;

            if (!PresetApplyer.Apply(buildSettings, presetName))
            {
                Debug.LogError($"[Builder] 프리셋={presetName} 적용 실패");
                return null;
            }

            buildSettings.BuildVersion = $"Build {System.DateTime.Now.ToString("yy.M.d.H.m")}";

            if (Application.isBatchMode)
            {
                // 커맨드라인에 버전 정보로 세팅
                if (int.TryParse(CommandLineReader.GetCustomArgument("appVersion"), out int appVersion))
                {
                    buildSettings.AppVersion = appVersion;
                }

                if (int.TryParse(CommandLineReader.GetCustomArgument("assetVersion"), out int assetVersion))
                {
                    buildSettings.AssetVersion = assetVersion;
                }

                string isStreamingAssets = CommandLineReader.GetCustomArgument("IsStreamingAssets");

                if (isStreamingAssets.Equals("true"))
                {
                    buildSettings.SetAssetBundleSettingsMode(AssetBundleSettings.Mode.StreamingAssets);
                }
                else
                {
                    buildSettings.SetAssetBundleSettingsMode(AssetBundleSettings.Mode.AssetBundle);
                }

                string isBuildAppBundle = CommandLineReader.GetCustomArgument("IsBuildAppBundle");

                if (isBuildAppBundle.Equals("true"))
                {
                    EditorUserBuildSettings.buildAppBundle = true;
                }
                else
                {
                    EditorUserBuildSettings.buildAppBundle = false;
                }
            }

            return buildSettings;
        }

        /// <summary>
        /// 로그 세팅
        /// </summary>
        /// <param name="logType"></param>
        private static void SetLog(StackTraceLogType logType)
        {
            PlayerSettings.SetStackTraceLogType(LogType.Error, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Assert, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Log, logType);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, logType);
        }

        /// <summary>
        /// 키스토어 세팅
        /// </summary>
        private static void SetKeyStore()
        {
            PlayerSettings.Android.keystoreName = Directory.GetCurrentDirectory().Replace("\\", "/") + KeyStorePath;
            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.keystorePass = KeyStorePass;
            PlayerSettings.keyaliasPass = KeyStorePass;
            InitPath();
        }

        /// <summary>
        /// 기타 세팅
        /// </summary>
        private static void SetProjectSetting()
        {
            if (Application.isBatchMode)
            {
                // 커맨드라인에 버전 정보로 세팅
                string bundleVersion = CommandLineReader.GetCustomArgument("bundleVersion");
                if (!string.IsNullOrEmpty(bundleVersion))
                {
                    PlayerSettings.bundleVersion = bundleVersion;
                }

                string bundleVersionCode = CommandLineReader.GetCustomArgument("bundleVersionCode");

                if (int.TryParse(bundleVersionCode, out int result))
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    {
                        PlayerSettings.Android.bundleVersionCode = result;
                    }
                }

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    PlayerSettings.iOS.buildNumber = bundleVersionCode;
                }

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
                }
            }
        }

        private static bool BundleBuild(bool isStreamingAssets)
        {
            if (isStreamingAssets)
            {
                if (!AssetBundleMenuItems.RealBuild())
                    return false;

                Debug.Log("[Builder] 에셋번들 스트리밍 폴더에 복사");
                AssetBundleMenuItems.CopyStreamingAssetAssetBundle();
            }
            else
            {
                Debug.Log("[Builder] 에셋번들 빌드");
                if (!AssetBundleMenuItems.RealBuild())
                    return false;

                Debug.Log("[Builder] 에셋번들 스트리밍 폴더 제거");
                AssetBundleMenuItems.DeleteStreamingAssets();
            }
            return true;
        }

        private static void SetTools()
        {
            Debug.Log("[Builder] SetTools");
            string[] searchInFolders = { "Assets/Ragnarok/Prefabs" };
            string[] GUIDs = AssetDatabase.FindAssets("Tools", searchInFolders);
            if (GUIDs.Length == 0)
                return;
            string assetPath = AssetDatabase.GUIDToAssetPath(GUIDs[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            PrefabUtility.InstantiatePrefab(prefab);
        }

        #region 앱가드

        private static string appGuardCli, projectRootPath, level;
        private static string buildOutputFilePath, tempApk;
        private static string appGuardAppKey, keystore, keystorePassword, keystoreAlias;

        public static bool IsNull(string input)
        {
            if ((input == null) || (input == ""))
                return true;
            else
                return false;
        }

        private static void InitPath()
        {
            Debug.Log("[Builder] 빌드 경로 설정");
            buildBasePath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/Build";

            if (!Directory.Exists(buildBasePath))
                Directory.CreateDirectory(buildBasePath);

            string fileNameExtension = EditorUserBuildSettings.buildAppBundle ? "aab" : "apk"; // 파일 확장자 이름
            string bundleVersion = PlayerSettings.bundleVersion;
            int bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            tempApk = $"{buildBasePath}/{BASE_APK_NAME}_{serverName}_{System.DateTime.Now.ToString("MMdd_HHmm")}_{bundleVersion}_{bundleVersionCode}.{fileNameExtension}";
            buildOutputFilePath = $"{buildBasePath}/{BASE_APK_NAME}_{serverName}_{System.DateTime.Now.ToString("MMdd_HHmm")}_{bundleVersion}_{bundleVersionCode}_AppGuard.{fileNameExtension}";

            keystore = UnityEditor.PlayerSettings.Android.keystoreName;
            keystorePassword = UnityEditor.PlayerSettings.Android.keystorePass;
            keystoreAlias = UnityEditor.PlayerSettings.Android.keyaliasName;
            projectRootPath = Path.GetFullPath(Application.dataPath + "/..");
#if UNITY_EDITOR_WIN
            appGuardCli = Application.dataPath + "/AppGuard/windows/AppGuard.exe";
            // 키스토어가 상대경로로 주어지는 경우가 있음
            if (IsNull(keystore) == false)
            {
                if (keystore[1] != ':')
                {
                    keystore = Path.Combine(projectRootPath, keystore);
                }
            }
#elif UNITY_EDITOR_OSX
            appGuardCli = Application.dataPath + "/AppGuard/mac/AppGuard";
            if (IsNull(keystore) == false)
		    {
			        if (keystore[0] != '/')
			        {
				        keystore = Path.Combine(projectRootPath, keystore);
			        }
		    }
#endif
            level = "--level3";
            appGuardAppKey = "zwjXddSHynKpQCVv";
        }

        private static void ProtectAppGuard(string unprotectedApk)
        {
            try
            {
                string args = " -k \"" + keystore + "\" -a \"" + keystoreAlias + "\" -p \"" + keystorePassword + "\" -n \"" + unprotectedApk + "\" -o \"" + buildOutputFilePath + "\" -v " + appGuardAppKey + " " + level + " --latestVersion";
                UnityEngine.Debug.Log(appGuardCli + " " + args);
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = appGuardCli;
                p.StartInfo.Arguments = args;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();
                p.Close();
                if (output != "")
                    UnityEngine.Debug.Log(output);
                if (error != "")
                    UnityEngine.Debug.LogError(error);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Builder] {e} Exception caught.");
            }
        }

        private static void DeleteTempFile()
        {
            if (File.Exists(tempApk))
            {
                try
                {
                    UnityEngine.Debug.Log("[Builder] AppGuard Protection Complete.");
                    File.Delete(tempApk);
                }
                catch (IOException e)
                {
                    UnityEngine.Debug.Log(e.Message);
                    return;
                }
            }
            else
            {
                UnityEngine.Debug.LogError("[Builder] Build Fail");
            }
        }

        #endregion

        [MenuItem(BaseMenu + BuildMenu + "Missing 변수를 포함하는 프리팹 Reimport", priority = 10001)]
        private static void ReimportPrefabsContainsMissingField()
        {
            const string TITLE = "Missing 변수를 포함하는 프리팹 Reimport";

            string[] paths = GetPrefabPaths();
            int pathCount = paths.Length;
            if (pathCount == 0)
            {
                EditorUtility.DisplayDialog(TITLE, "프리팹 에셋번들 음슴", "확인");
                return;
            }

            List<string> list = new List<string>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Text.StringBuilder sbLog = new System.Text.StringBuilder();
            for (int i = 0; i < pathCount; i++)
            {
                string path = paths[i];
                EditorUtility.DisplayProgressBar(TITLE, path, MathUtils.GetProgress(i, pathCount)); // Show Progress

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                FindMissingField(sb, prefab.transform);
                if (sb.Length == 0)
                    continue;

                list.Add(path);

                if (sbLog.Length > 0)
                    sbLog.AppendLine();

                sbLog.Append("[").Append(path).Append("]")
                    .AppendLine()
                    .Append(sb.ToString());

                sb.Length = 0;
            }

            EditorUtility.ClearProgressBar(); // Clear Progress

            int missingCount = list.Count;
            if (missingCount == 0)
            {
                EditorUtility.DisplayDialog(TITLE, "Missing 변수를 포함하는 프리팹 음슴", "확인");
                return;
            }

            string log = sbLog.ToString();
            sb.Append("Missing 변수를 포함하는 프리팹을 Import 하시겠습니까?");
            sb.AppendLine();
            sb.AppendLine(log);

            if (EditorUtility.DisplayDialog(TITLE, sb.ToString(), "확인", "취소"))
            {
                for (int i = 0; i < missingCount; i++)
                {
                    EditorUtility.DisplayProgressBar(TITLE, list[i], MathUtils.GetProgress(i, missingCount)); // Show Progress
                    AssetDatabase.ImportAsset(list[i]);
                    //AssetDatabase.ImportAsset(list[i], ImportAssetOptions.ImportRecursive);
                }

                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar(); // Clear Progress

                EditorUtility.DisplayDialog(TITLE, "Missing 변수를 포함하는 프리팹 Import 완료", "확인");
            }

            Debug.LogError(log);
        }

        [MenuItem(BaseMenu + BuildMenu + "Missing 스크립트를 포함하는 프리팹 찾기", priority = 10002)]
        private static void FindMissingAssetBundles()
        {
            const string TITLE = "Missing 스크립트를 포함하는 프리팹 찾기";

            string[] paths = GetPrefabPaths();
            int pathCount = paths.Length;
            if (pathCount == 0)
            {
                EditorUtility.DisplayDialog(TITLE, "프리팹 에셋번들 음슴", "확인");
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Text.StringBuilder sbLog = new System.Text.StringBuilder();
            for (int i = 0; i < pathCount; i++)
            {
                string path = paths[i];
                EditorUtility.DisplayProgressBar(TITLE, path, MathUtils.GetProgress(i, pathCount)); // Show Progress

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                FindMissingScript(sb, prefab.transform);
                if (sb.Length == 0)
                    continue;

                if (sbLog.Length > 0)
                    sbLog.AppendLine();

                sbLog.Append("[").Append(path).Append("]")
                    .AppendLine()
                    .Append(sb.ToString());

                sb.Length = 0;
            }

            EditorUtility.ClearProgressBar(); // Clear Progress

            if (sbLog.Length == 0)
            {
                EditorUtility.DisplayDialog(TITLE, "Missing 스크립트를 포함하는 프리팹 음슴", "확인");
                return;
            }

            string log = sbLog.ToString();

            EditorUtility.DisplayDialog(TITLE, "Missing 스크립트를 포함하는 프리팹 클립보드에 저장", "확인");
            GUIUtility.systemCopyBuffer = log;

            Debug.LogError(log);
        }

        private static string[] GetPrefabPaths()
        {
            Buffer<string> bufer = new Buffer<string>();

            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (var item in assetBundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(item);
                foreach (var path in assetPaths)
                {
                    if (!string.Equals(Path.GetExtension(path), ".prefab"))
                        continue;

                    bufer.Add(path);
                }
            }

            return bufer.GetBuffer(isAutoRelease: true);
        }

        private static void FindMissingField(System.Text.StringBuilder sb, Transform tf)
        {
            const System.Reflection.BindingFlags serializeFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            Component[] components = tf.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                    continue;

                SerializedObject so = new SerializedObject(components[i]);
                foreach (System.Reflection.FieldInfo field in components[i].GetType().GetFields(serializeFlags))
                {
                    // Missing 변수를 포함하는 변수 유무
                    IsMissingProperty(sb, so.FindProperty(field.Name));
                }
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                FindMissingField(sb, tf.GetChild(i));
            }
        }

        private static void IsMissingProperty(System.Text.StringBuilder sb, SerializedProperty sp)
        {
            // Serialized 변수가 아님
            if (sp == null)
                return;

            if (sp.isArray)
            {
                for (int i = 0; i < sp.arraySize; i++)
                {
                    IsMissingProperty(sb, sp.GetArrayElementAtIndex(i));
                }

                return;
            }

            // 참조 타입이 아님
            if (sp.propertyType != SerializedPropertyType.ObjectReference)
                return;

            // 아예 할당이 안 된 값
            if (sp.objectReferenceInstanceIDValue == 0)
                return;

            // Missing 된 참조 값
            if (sp.objectReferenceValue != null)
                return;

            if (sb.Length > 0)
                sb.AppendLine();

            sb.Append(sp.serializedObject.targetObject.name).Append(" (").Append(sp.name).Append(")");
        }

        private static void FindMissingScript(System.Text.StringBuilder sb, Transform tf)
        {
            Component[] components = tf.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.Append(NGUITools.GetHierarchy(tf.gameObject));
                }
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                FindMissingScript(sb, tf.GetChild(i));
            }
        }

        [MenuItem(BaseMenu + BuildMenu + "Legacy 애니메이션을 포함하는 프리팹 찾기", priority = 10003)]
        private static void FindMissingAnimations()
        {
            const string TITLE = "Legacy 애니메이션을 포함하는 프리팹 찾기";

            Buffer<string> pathBuffer = new Buffer<string>();
            pathBuffer.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle("prefab_character"));
            pathBuffer.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle("prefab_monster"));
            pathBuffer.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle("prefab_npc"));

            int pathCount = pathBuffer.size;
            if (pathCount == 0)
            {
                EditorUtility.DisplayDialog(TITLE, "프리팹 에셋번들 음슴", "확인");
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < pathCount; i++)
            {
                string path = pathBuffer[i];
                EditorUtility.DisplayProgressBar(TITLE, path, MathUtils.GetProgress(i, pathCount)); // Show Progress

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                Animation[] animations = prefab.GetComponentsInChildren<Animation>(includeInactive: true);
                foreach (Animation ani in animations)
                {
                    foreach (AnimationState aniState in ani)
                    {
                        if (aniState.clip.legacy)
                            continue;

                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append("[").Append(path).Append("]");
                    }
                }
            }

            EditorUtility.ClearProgressBar(); // Clear Progress

            if (sb.Length == 0)
            {
                EditorUtility.DisplayDialog(TITLE, "Legacy 애니메이션을 포함하는 프리팹 음슴", "확인");
                return;
            }

            string log = sb.ToString();

            EditorUtility.DisplayDialog(TITLE, "Legacy 애니메이션을 포함하는 프리팹 클립보드에 저장", "확인");

            Debug.LogError(log);
        }

        [MenuItem(BaseMenu + BuildMenu + ConvertJenkinsDetailLogWizard.TITLE, priority = 10004)]
        private static void ConvertJenkinsDetailLog()
        {
            ConvertJenkinsDetailLogWizard wizard = ScriptableWizard.DisplayWizard<ConvertJenkinsDetailLogWizard>(ConvertJenkinsDetailLogWizard.TITLE);
            wizard.minSize = wizard.maxSize = new Vector2(480f, 200f);
            wizard.Focus();
            wizard.Repaint();
            wizard.Show();
        }
    }
}