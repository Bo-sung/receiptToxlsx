using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Ragnarok
{
    public abstract class AssetLoader
    {
        protected const string LOCAL_ASSET_BUNDLE_NAME = "local";
        public const string PATCH_LIST_FILE = "PatchList.bin";

        public readonly bool isShowDownload;
        private readonly string baseURL;
        private readonly Dictionary<string, AssetBundle> assetBundleDic;
        private readonly List<AssetData> downloadAssetList;
        private readonly List<AssetData> cachingAssetList;
        private bool isAllReady;
        private long assetDownloadSize;

        public event System.Action<float> OnDownloadTotalProgress;
        public event System.Action<float> OnDownloadDetailProgress;
        public event System.Action<int, int> OnDownloadCountProgress;
        public event System.Action<float> OnLoadTotalProgress;
        public event System.Action<float> OnLoadDetailProgress;
        public event System.Action<int, int> OnLoadCountProgress;
        public event System.Action OnAllReady;
        public event System.Action<long> OnDownloadSpeed;

        public AssetLoader(string baseURL, bool isShowDownload)
        {
            this.baseURL = baseURL;
            this.isShowDownload = isShowDownload;

            assetBundleDic = new Dictionary<string, AssetBundle>(System.StringComparer.Ordinal);
            cachingAssetList = new List<AssetData>();
            downloadAssetList = new List<AssetData>();
        }

        /// <summary>
        /// 로컬 에셋번들 로드
        /// </summary>
        public void LoadLocalAssetBundle()
        {
            AssetBundle localAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "local"));
            if (localAssetBundle == null)
            {
                Debug.LogError($"찾을수 없는 에셋번들: name = local");
                return;
            }
            Add(LOCAL_ASSET_BUNDLE_NAME, localAssetBundle);
        }

        /// <summary>
        /// 패치 리스트 다운로드
        /// </summary>
        public async virtual Task<long> DownloadPatchList()
        {
            if (isAllReady)
                return 0L;

            string url = GetPatchURL(); // PatchList Url

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                    throw new WWWErrorException(request, "Patch List Download Error");

                using (MemoryStream stream = new MemoryStream(request.downloadHandler.data))
                {
                    IList<AssetData> dataList = (List<AssetData>)new BinaryFormatter().Deserialize(stream);
                    return CheckBundle(dataList);
                }
            }
        }

        /// <summary>
        /// 어셋번들 다운로드
        /// </summary>
        public async virtual Task DownloadAssetBundles()
        {
            if (isAllReady)
            {
                OnDownloadDetailProgress?.Invoke(1f);
                OnDownloadTotalProgress?.Invoke(1f);
                return;
            }

            // 에셋번들 다운로드
            float totalProgress = 0;
            int count = downloadAssetList.Count;
            float time = 0;
            float preProgress = 0;

            for (int i = 0; i < count; i++)
            {
                AssetData data = downloadAssetList[i];
                //Debug.LogError($"에셋번들 이름={data.name} 용량={data.size.FormatBytes()}");
                using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(string.Concat(baseURL, data.name), data.hash, 0))
                {
                    OnDownloadCountProgress?.Invoke(i, count);

                    request.useHttpContinue = false;
                    AsyncOperation async = request.SendWebRequest();

                    while (!async.isDone)
                    {
                        float tempProgress = async.progress;
                        OnDownloadDetailProgress?.Invoke(tempProgress);

                        float progress = (totalProgress + tempProgress) / count;
                        OnDownloadTotalProgress?.Invoke(progress);

                        time += Time.deltaTime;
                        if (time >= 1f)
                        {
                            time = 0;
                            float interval = progress - preProgress;
                            preProgress = progress;
                            OnDownloadSpeed?.Invoke((long)(assetDownloadSize * interval));
                            Debug.Log($"[에셋번들] 진행도={progress}, 전체용량={assetDownloadSize.FormatBytes()}, 다운로드 용량={((long)(assetDownloadSize * progress)).FormatBytes()}, 다운로드 속도={((long)(assetDownloadSize * interval)).FormatBytes()}/s");
                        }
                        await Awaiters.NextFrame;
                    }

                    OnDownloadDetailProgress?.Invoke(1f);
                    totalProgress += 1f;
                    await Awaiters.NextFrame;

                    if (request.isNetworkError || request.isHttpError)
                        throw new WWWErrorException(request, "AssetBundle Download Error");

                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                    Add(data.name, bundle);
                    Caching.ClearOtherCachedVersions(data.name, data.hash);
                }
            }

            OnDownloadTotalProgress?.Invoke(1f);
            downloadAssetList.Clear();
        }

        public async virtual Task LoadCachedAssetBundles()
        {
            if (isAllReady)
            {
                OnLoadDetailProgress?.Invoke(1f);
                OnLoadTotalProgress?.Invoke(1f);
                OnAllReady?.Invoke();
                return;
            }

            Debug.Log("캐시된 어셋번들 로드");

            while (!Caching.ready)
            {
                await Awaiters.NextFrame;
            }

            // 저장되어있는 에셋번들 로드
            float totalProgress = 0;
            int max = cachingAssetList.Count;
            bool isAsync;
            for (int i = 0; i < max; i++)
            {
                isAsync = MathUtils.IsCheckPermyriad(1250); // 1.25%

                AssetData data = cachingAssetList[i];
                using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(string.Concat(baseURL, data.name), data.hash, 0))
                {
                    OnLoadCountProgress?.Invoke(i, max);
                    OnLoadTotalProgress?.Invoke(MathUtils.GetProgress(i, max));

                    request.useHttpContinue = false;
                    AsyncOperation async = request.SendWebRequest();
                    while (!async.isDone)
                    {
                        float tempProgress = async.progress;
                        OnLoadDetailProgress?.Invoke(tempProgress);

                        if (isAsync)
                        {
                            await Awaiters.NextFrame;
                        }
                    }

                    OnLoadDetailProgress?.Invoke(1f);
                    totalProgress += 1f;

                    if (isAsync)
                    {
                        await Awaiters.NextFrame;
                    }

                    if (request.isNetworkError || request.isHttpError)
                        throw new WWWErrorException(request, "Cached AssetBundle Load Error");

                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                    Add(data.name, bundle);
                }
            }

            OnLoadDetailProgress?.Invoke(1f);
            OnLoadTotalProgress?.Invoke(1f);
            cachingAssetList.Clear();

            AllReady();
        }

        protected void AllReady()
        {
            isAllReady = true;
            OnAllReady?.Invoke();
        }

        public virtual bool Contains(string assetBundleName)
        {
            return assetBundleDic.ContainsKey(assetBundleName);
        }

        public virtual bool Contains(string assetBundleName, string assetName)
        {
            if (!assetBundleDic.ContainsKey(assetBundleName))
                return false;

            return assetBundleDic[assetBundleName].Contains(assetName);
        }

        public virtual T GetResource<T>(string assetBundleName, string assetName) where T : Object
        {
            if (!Contains(assetBundleName))
                throw new System.ArgumentException($"Path Error: assetBundleName: {assetBundleName}, assetName: {assetName}");

            return assetBundleDic[assetBundleName].LoadAsset<T>(assetName);
        }

        public virtual AssetBundleRequest GetResourceAsync<T>(string assetBundleName, string assetName) where T : Object
        {
            if (!Contains(assetBundleName))
                throw new System.ArgumentException($"Path Error: assetBundleName: {assetBundleName}, assetName: {assetName}");

            return assetBundleDic[assetBundleName].LoadAssetAsync<T>(assetName);
        }

        public virtual T[] GetResourceAll<T>(string assetBundleName) where T : Object
        {
            if (!assetBundleDic.ContainsKey(assetBundleName))
                throw new System.ArgumentException($"Path Error: assetBundleName: {assetBundleName}");

            var assetBundle = assetBundleDic[assetBundleName];
            T[] assets = assetBundle.LoadAllAssets<T>();
            return assets;
        }

        public virtual void UnloadAssetBundle()
        {
            if (assetBundleDic.Count == 0)
                return;

            foreach (var item in assetBundleDic.Values)
            {
                item.Unload(false);
                Object.Destroy(item);
            }

            AssetBundle.UnloadAllAssetBundles(unloadAllObjects: true);
            assetBundleDic.Clear();
        }

        /// <summary>
        /// 패치리스트 URL
        /// </summary>
        protected string GetPatchURL()
        {
            return string.Concat(baseURL, PATCH_LIST_FILE);
        }

        /// <summary>
        /// 다운로드 해야할 에셋번들과 저장되어있는 에셋번들 체크
        /// </summary>
        private long CheckBundle(IList<AssetData> dataList)
        {
            assetDownloadSize = 0L; // 다운로드 해야할 에셋번들 용량

            foreach (AssetData data in dataList)
            {
                // 캐싱 여부 확인
                if (Caching.IsVersionCached(string.Concat(baseURL, data.name), data.hash))
                {
                    cachingAssetList.Add(data);
                    continue;
                }

                // 다운받아야 할 어셋번들 정보
                assetDownloadSize += data.size;
                downloadAssetList.Add(data);
            }

            return assetDownloadSize;
        }

        /// <summary>
        /// 어셋번들 파일 추가
        /// </summary>
        private void Add(string name, AssetBundle assetBundle)
        {
            // 씬 에셋번들의 경우 리턴
            if (assetBundle.isStreamedSceneAssetBundle)
                return;

            if (assetBundleDic.ContainsKey(name))
            {
                Debug.LogError($"이미 있는 어셋번들: name = {name}");
                return;
            }

            assetBundleDic.Add(name, assetBundle);
        }
    }
}