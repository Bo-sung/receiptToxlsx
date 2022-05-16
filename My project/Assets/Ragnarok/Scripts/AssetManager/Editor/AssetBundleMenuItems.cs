using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class AssetBundleMenuItems
    {
        const string TOGGLE_SYNCHRONIZE_ASSET_BUNDLE_MENU = "라그나로크/에셋번들/에셋 변경 시 자동 동기화";
        const string StreamingAssetsBundlePath = "Assets/StreamingAssets/AssetBundles";

        /// <summary>
        /// 에셋 변경 시 자동 동기화 여부
        /// </summary>
        public static bool IsAutoAssetBundleBuild
        {
            get { return EditorPrefs.GetBool(nameof(IsAutoAssetBundleBuild), defaultValue: false); }
            set { EditorPrefs.SetBool(nameof(IsAutoAssetBundleBuild), value); }
        }

        public static bool Build(bool isEditor)
        {
            Debug.Log("번들 빌드");

            string outputPath = GetAssetBundlePath(isEditor);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);

            if (manifest != null)
            {
                string[] bundleNames = manifest.GetAllAssetBundles();
                string basePath = System.Environment.CurrentDirectory.Replace("\\", "/");

                List<AssetData> assetList = new List<AssetData>();

                foreach (var bundle in bundleNames)
                {
                    // 로컬번들 체크
                    if (bundle.Equals("local"))
                        continue;

                    AssetData asset = new AssetData()
                    {
                        name = bundle,
                        hash = manifest.GetAssetBundleHash(bundle),
                        size = new FileInfo($"{basePath}/{outputPath}/{bundle}").Length
                    };
                    assetList.Add(asset);
                }

                assetList.Sort((x, y) => x.size.CompareTo(y.size));

                var filePath = $"{basePath}/{outputPath}/{AssetLoader.PATCH_LIST_FILE}";

                var fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var bf = new BinaryFormatter();
                bf.Serialize(fs, assetList);
                fs.Close();
            }

            if (manifest == null)
            {
                EditorUtility.DisplayDialog("알림", "에셋번들 빌드에 실패하였습니다.", "확인");
                return false;
            }

            return true;
        }

        [MenuItem("라그나로크/에셋번들/번들빌드")]
        public static bool RealBuild()
        {
            if (Application.isEditor && !Application.isBatchMode)
            {
                if (!EditorUtility.DisplayDialog("에셋번들", "에셋번들을 빌드하시겠습니까?", "빌드", "취소"))
                    return false;
            }

            if (!Build(false))
                return false;

            // 로컬 에셋번들 복사
            string source = GetAssetBundlePath(false);
            string[] localFileNames = { "local", "local.manifest" };
            for (int i = 0; i < localFileNames.Length; i++)
            {
                string localSource = $"{source}/{localFileNames[i]}";
                string localDest = $"Assets/StreamingAssets/{localFileNames[i]}";

                FileUtil.DeleteFileOrDirectory(localDest); // 기존 로컬에셋번들 제거
                FileUtil.MoveFileOrDirectory(localSource, localDest); // 이동
            }

            // 새로고침
            AssetDatabase.Refresh();
            return true;
        }

        [MenuItem("라그나로크/에셋번들/에디터 에셋번들 동기화")]
        public static void SynchronizeAssetBundle()
        {
            Debug.Log("에디터 에셋번들 동기화 시작");
            bool isEditor = true;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // 에셋번들 빌드
            if (!Build(isEditor))
                return;

            sw.Stop();
            Debug.Log($"에셋번들 빌드시간 : {sw.ElapsedMilliseconds}ms");

            // 에셋번들 복사 (주의: Move를 사용하면 캐시 정보가 날아감)
            string source = GetAssetBundlePath(isEditor);
            string dest = $"Assets/Ragnarok/AssetBundles";
            string platformDest = $"{dest}/{Config.PLATFORTM_NAME}";

            // 폴더 생성           
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            if (!Directory.Exists("Assets/StreamingAssets"))
                Directory.CreateDirectory("Assets/StreamingAssets");

            FileUtil.DeleteFileOrDirectory(platformDest); // 기존 어셋번들 제거
            FileUtil.CopyFileOrDirectory(source, platformDest); // 복사

            // 로컬 에셋번들 복사
            string[] localFileNames = { "local", "local.manifest" };
            for (int i = 0; i < localFileNames.Length; i++)
            {
                string localSource = $"{source}/{localFileNames[i]}";
                string localDest = $"Assets/StreamingAssets/{localFileNames[i]}";

                FileUtil.DeleteFileOrDirectory(localDest); // 기존 로컬에셋번들 제거
                FileUtil.CopyFileOrDirectory(localSource, localDest); // 복사
            }

            // 새로고침
            AssetDatabase.Refresh();
        }

        [MenuItem("라그나로크/에셋번들/스트리밍에셋에 복사")]
        public static void CopyStreamingAssetAssetBundle()
        {
            int assetVersion = BuildSettings.Instance.AssetVersion;

            string source = GetAssetBundlePath(false);
            string dest = $"{StreamingAssetsBundlePath}/{assetVersion}";
            string platformDest = $"{dest}/{Config.PLATFORTM_NAME}";

            FileUtil.DeleteFileOrDirectory(StreamingAssetsBundlePath); // 기존 어셋번들 제거

            // 폴더 생성
            DirectoryInfo directoryInfo = new DirectoryInfo(dest);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            FileUtil.CopyFileOrDirectory(source, $"{dest}/{Config.PLATFORTM_NAME}"); // 복사

            // 스트리밍에셋 번들폴더에서 로컬번들 제거
            string[] localFileNames = { "local", "local.manifest" };
            for (int i = 0; i < localFileNames.Length; i++)
            {
                string localDest = $"{platformDest}/{localFileNames[i]}";
                FileUtil.DeleteFileOrDirectory(localDest);
            }

            // 새로고침
            AssetDatabase.Refresh();
        }

        [MenuItem(TOGGLE_SYNCHRONIZE_ASSET_BUNDLE_MENU)]
        private static void ToggleAutoAssetBundleBuild()
        {
            IsAutoAssetBundleBuild = !IsAutoAssetBundleBuild;
        }

        [MenuItem(TOGGLE_SYNCHRONIZE_ASSET_BUNDLE_MENU, validate = true)]
        private static bool ToggleAutoAssetBundleBuildValidate()
        {
            Menu.SetChecked(TOGGLE_SYNCHRONIZE_ASSET_BUNDLE_MENU, IsAutoAssetBundleBuild);
            return true;
        }

        [MenuItem("라그나로크/에셋번들/에셋번들 캐시 삭제")]
        private static void ClearAssetBundleCache()
        {
            Caching.ClearCache();
        }

        private static string GetAssetBundlePath(bool isEditor)
        {
            if (isEditor)
                return $"AssetBundles/Editor/{Config.PLATFORTM_NAME}";

            if (Application.isBatchMode)
            {
                // 커맨드라인에 버전 정보가 있으면 번들 주소 변경
                if (int.TryParse(CommandLineReader.GetCustomArgument("assetVersion"), out int result))
                {
                    return $"AssetBundles/{result}/{Config.PLATFORTM_NAME}";
                }
            }

            int assetVersion = BuildSettings.Instance.AssetVersion;
            return $"AssetBundles/{assetVersion}/{Config.PLATFORTM_NAME}";
        }

        public static void DeleteStreamingAssets()
        {
            FileUtil.DeleteFileOrDirectory(StreamingAssetsBundlePath); // 기존 어셋번들 제거
            AssetDatabase.Refresh();
        }
    }
}