#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ragnarok
{
    public sealed class EditorAssetLoader : AssetLoader
    {
        /// <summary>
        /// 패치리스트 파일 존재
        /// </summary>
        bool isExistsPatchList;

        public EditorAssetLoader() : base(string.Empty, false)
        {
        }

        public EditorAssetLoader(string baseURL)
            : base($"{baseURL}/Ragnarok/AssetBundles/{Config.PLATFORTM_NAME}/", false)
        {
        }

        public async override Task<long> DownloadPatchList()
        {
            string path = GetPatchURL().Replace("file:///", string.Empty);
            isExistsPatchList = File.Exists(path);

            // 패치리스트 파일이 존재하지 않을 경우
            if (!isExistsPatchList)
                return 0L;

            return await base.DownloadPatchList();
        }

        public async override Task DownloadAssetBundles()
        {
            // 패치리스트 파일이 존재할 경우
            if (isExistsPatchList)
            {
                await base.DownloadAssetBundles();
            }
        }

        public async override Task LoadCachedAssetBundles()
        {
            // 패치리스트 파일이 존재할 경우
            if (isExistsPatchList)
            {
                await base.LoadCachedAssetBundles();
            }
            else
            {
                AllReady();
            }
        }

        public override bool Contains(string assetBundleName)
        {
            if (isExistsPatchList || LOCAL_ASSET_BUNDLE_NAME.Equals(assetBundleName))
                return base.Contains(assetBundleName);

            string[] assetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            return assetBundlePaths.Length > 0;
        }

        public override bool Contains(string assetBundleName, string assetName)
        {
            if (isExistsPatchList || LOCAL_ASSET_BUNDLE_NAME.Equals(assetBundleName))
                return base.Contains(assetBundleName, assetName);

            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
            return assetPaths.Length > 0;
        }

        public override T GetResource<T>(string assetBundleName, string assetName)
        {
            if (isExistsPatchList || LOCAL_ASSET_BUNDLE_NAME.Equals(assetBundleName))
                return base.GetResource<T>(assetBundleName, assetName);

            // Editor 용 어셋번들
            string assetPath = GetAssetPathInEditor(assetBundleName, assetName);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public override AssetBundleRequest GetResourceAsync<T>(string assetBundleName, string assetName)
        {
            if (isExistsPatchList || LOCAL_ASSET_BUNDLE_NAME.Equals(assetBundleName))
                return base.GetResourceAsync<T>(assetBundleName, assetName);

            // Editor 용 어셋번들
            string assetPath = GetAssetPathInEditor(assetBundleName, assetName);
            return AssetBundle.LoadFromFile(assetPath).LoadAssetAsync<T>(assetName);
        }

        public override T[] GetResourceAll<T>(string assetBundleName)
        {
            if (isExistsPatchList || LOCAL_ASSET_BUNDLE_NAME.Equals(assetBundleName))
                return base.GetResourceAll<T>(assetBundleName);

            // Editor 용 어셋번들
            string[] assetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

            List<T> list = new List<T>();
            foreach (string path in assetBundlePaths)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object asset in assets)
                {
                    if (asset is T)
                        list.Add(asset as T);
                }
            }

            return list.ToArray();
        }

        private string GetAssetPathInEditor(string assetBundleName, string assetName)
        {
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);

            if (assetPaths.Length > 1)
                Debug.LogWarning($"Duplication Error: assetBundleName: {assetBundleName}, assetName: {assetName}");

            if (assetPaths.Length == 0)
            {
                foreach (string assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName))
                {
                    string name = Path.GetFileNameWithoutExtension(assetPath);
                    if (name.Equals(assetName))
                        return assetPath;
                }

                if (assetPaths.Length == 0)
                    throw new System.ArgumentException($"Path Error: assetBundleName: {assetBundleName}, assetName: {assetName}");
            }

            return assetPaths[0];
        }
    }
}
#endif