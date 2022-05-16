//#define LOAD_LOCAL_DATA

using CodeStage.AntiCheat.ObscuredTypes;
using Sfs2X.Entities.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public class DataManager : Singleton<DataManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!(sceneName.Equals(SceneLoader.PRELOAD) || sceneName.Equals(SceneLoader.INTRO)))
                return;

            var types = ReflectionUtils.GetAllInterfaces<IDataManger>(); // IDataManager 를 상속받는 모든 Type 호출
            foreach (var item in types)
            {
                var propertyInfo = typeof(Singleton<>).MakeGenericType(item).GetProperty(nameof(Singleton<object>.Instance), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                IDataManger instance = (IDataManger)propertyInfo.GetValue(null, null); // Instance 호출
                Instance.dataManagers.Add(instance); // 추가
            }
        }

        private const int DETAIL_LOAD_RANDOM_NUM = 8;

        private readonly ConnectionManager connectionManager;

        /// <summary>
        /// 데이터 매니저
        /// </summary>
        private readonly List<IDataManger> dataManagers;

        /// <summary>
        /// 다운 받아야할 리소스 정보
        /// </summary>
        private readonly List<ResourceVersion> resources;

        public event Action<float> OnDownloadTotalProgress;
        public event Action<float> OnDownloadDetailProgress;
        public event Action<int, int> OnDownloadCountProgress;
        public event Action<float> OnLoadTotalProgress;
        public event Action<float> OnLoadDetailProgress;
        public event Action<int, int> OnLoadCountProgress;
        public event Action<long> OnDownloadSpeed;

        private ObscuredString key, iv;
        private long downloadSize;

        private const string UNUSED_DB_NAME = "DungeonDataDB";

        public DataManager()
        {
            connectionManager = ConnectionManager.Instance;
            dataManagers = new List<IDataManger>();
            resources = new List<ResourceVersion>();
        }

        protected override void OnTitle()
        {
        }

#if LOAD_LOCAL_DATA
        /// <summary>
        /// 로컬 데이터 로드
        /// </summary>
        public async Task LoadLocalData()
        {
            const string LOCAL_HASH_KEY = "93796681d483212252ba01a7d797e8b4";
            AesEncrypter encrypter = new AesEncrypter(LOCAL_HASH_KEY);

            string dataFolderPath = $"{Application.persistentDataPath}/Data";

            if (!Directory.Exists(dataFolderPath))
                Directory.CreateDirectory(dataFolderPath);

            int max = dataManagers.Count;
            bool isAsync;
            for (int i = 0; i < dataManagers.Count; i++)
            {
                isAsync = MathUtils.IsCheckPermyriad(1250); // 1.25%

                string filaPath = $"{dataFolderPath}/{dataManagers[i].DataType}";

                // 이미 저장되어 있는 값이 존재
                if (File.Exists(filaPath))
                    continue;

                OnLoadCountProgress?.Invoke(i, max);
                OnLoadTotalProgress?.Invoke(MathUtils.GetProgress(i, max));

                string path = string.Concat("Data/", dataManagers[i].DataType.ToString());
                if (DebugUtils.IsLogResoureceData)
                    Debug.Log($"리소스 파일 Path: Resource = {path}");

                TextAsset asset = Resources.Load<TextAsset>(path);
                if (asset == null)
                {
                    Debug.Log($"리소스 파일이 존재하지 않습니다: Resource = {dataManagers[i].DataType}");
                    continue;
                }

                byte[] bytes = encrypter.decrypt(asset.bytes);
                if (bytes.Length == 0)
                {
                    Debug.LogError($"리소스 파일 복호화 실패 Resource = {dataManagers[i].DataType}");
                    continue;
                }

                // 로컬 데이터(복호화 하기 전) 저장
                using (FileStream stream = File.Create(filaPath))
                {
                    if (DebugUtils.IsLogResoureceData)
                        Debug.LogError($"데이터 파일 생성 = {filaPath}, 크기={asset.bytes.Length} 바이트");

                    await stream.WriteAsync(asset.bytes, 0, asset.bytes.Length);
                }

                OnLoadDetailProgress?.Invoke(0f);

                if (isAsync)
                {
                    await Awaiters.NextFrame;
                }

                dataManagers[i].LoadData(bytes);

                OnLoadDetailProgress?.Invoke(1f);

                if (isAsync)
                {
                    await Awaiters.NextFrame;
                }
            }

            OnLoadDetailProgress?.Invoke(1f);
            OnLoadTotalProgress?.Invoke(1f);
        }
#endif
        /// <summary>
        /// 리소스 버전 요청
        /// </summary>
        public async Task<long> GetResoureceVersion()
        {
            var sfs = SFSObject.NewInstance();
            sfs.PutShort("1", 0);

            var response = await Protocol.RESOURCE_VERSION.SendAsync(sfs);
            downloadSize = 0L;

            if (response.isSuccess)
            {
                key = connectionManager.GetDecrypt(response.GetByteArray("1"));
                iv = connectionManager.GetDecrypt(response.GetByteArray("2"));

                MD5 md5 = MD5.Create();
                resources.Clear();

                var sfsArray = response.GetSFSArray("4");
                for (int i = 0; i < sfsArray.Count; i++)
                {
                    var obj = sfsArray.GetSFSObject(i);
                    // 클라용 데이터일때
                    if (obj.GetByte("3") == 0)
                    {
                        var item = new ResourceVersion(obj);

                        // 현재는 사용하지 않는 데이터
                        if (item.typeName.Equals(UNUSED_DB_NAME))
                            continue;

                        if (!item.hasType)
                        {
                            Debug.LogError($"========== 테이블 타입={item.typeName} 작업필요함 ==========");
                            continue;
                        }

                        if (!item.VerifyMd5Hash(md5))
                        {
                            resources.Add(item);
                            downloadSize += item.size;
                        }
                    }
                }

                md5.Dispose();
            }
            else
            {
                response.ShowResultCode();
            }

            return downloadSize;
        }

        /// <summary>
        /// 리소스 다운로드
        /// </summary>
        public async Task Download()
        {
            float totalProgress = 0;
            float time = 0;
            float preProgress = 0;

            string dataFolderPath = $"{Application.persistentDataPath}/Data";

            if (!Directory.Exists(dataFolderPath))
                Directory.CreateDirectory(dataFolderPath);

            int max = resources.Count;
            // 데이터 테이블 다운로드
            for (int i = 0; i < max; i++)
            {
                string url = connectionManager.GetResourceUrl(resources[i].typeName, resources[i].version);

                if (DebugUtils.IsLogResoureceData)
                    Debug.Log($"다운로드 데이터 URL = {url}");

                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    OnDownloadCountProgress?.Invoke(i, max);

                    AsyncOperation async = request.SendWebRequest();
                    while (!async.isDone)
                    {
                        float tempProgress = async.progress;
                        OnDownloadDetailProgress?.Invoke(tempProgress);

                        float progress = (totalProgress + tempProgress) / max;
                        OnDownloadTotalProgress?.Invoke(progress);

                        time += Time.deltaTime;
                        if (time >= 1f)
                        {
                            time = 0;
                            float interval = progress - preProgress;
                            preProgress = progress;
                            OnDownloadSpeed?.Invoke((long)(downloadSize * interval));
                            Debug.Log($"[테이블] 진행도={progress}, 전체용량={downloadSize.FormatBytes()}, 다운로드 용량={((long)(downloadSize * progress)).FormatBytes()}, 다운로드 속도={((long)(downloadSize * interval)).FormatBytes()}/s");
                        }
                        await Awaiters.NextFrame;
                    }

                    OnDownloadDetailProgress?.Invoke(1f);
                    totalProgress += 1f;
                    await Awaiters.NextFrame;

                    if (request.isNetworkError || request.isHttpError)
                        throw new WWWErrorException(request, "Resource Download Error");

                    using (FileStream stream = File.Create(resources[i].path))
                    {
                        if (DebugUtils.IsLogResoureceData)
                            Debug.Log($"데이터 파일 생성 = {resources[i].path}, 크기={request.downloadHandler.data.Length} 바이트");
                        await stream.WriteAsync(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
                    }
                }
            }

            OnDownloadTotalProgress?.Invoke(1f);
        }

        /// <summary>
        /// 모든 리소스 테이블 읽기
        /// </summary>
        public async Task Load()
        {
            AesEncrypter encrypter = new AesEncrypter(iv);
            int max = dataManagers.Count;
            bool isAsync;
            for (int i = 0; i < dataManagers.Count; i++)
            {
                isAsync = MathUtils.IsCheckPermyriad(1250); // 1.25%

                // 기존 로드한 LocalData 가 존재할 수 있음
                dataManagers[i].ClearData();

                OnLoadCountProgress?.Invoke(i, max);
                OnLoadTotalProgress?.Invoke(MathUtils.GetProgress(i, max));

                string path = string.Concat(Application.persistentDataPath, "/Data/", dataManagers[i].DataType.ToString());

                if (DebugUtils.IsLogResoureceData)
                    Debug.Log($"리소스 파일 Path: Resource = {path}");

                if (!File.Exists(path))
                {
                    Debug.Log($"리소스 파일이 존재하지 않습니다: Resource = {dataManagers[i].DataType}");
                    continue;
                }

                byte[] bytes = encrypter.decrypt(File.ReadAllBytes(path));
                if (bytes.Length == 0)
                {
                    Debug.LogError($"리소스 파일 복호화 실패 Resource = {dataManagers[i].DataType}");
                    continue;
                }

                OnLoadDetailProgress?.Invoke(0f);

                if (isAsync)
                {
                    await Awaiters.NextFrame;
                }

                dataManagers[i].LoadData(bytes);

                OnLoadDetailProgress?.Invoke(1f);

                if (isAsync)
                {
                    await Awaiters.NextFrame;
                }
            }

            OnLoadDetailProgress?.Invoke(1f);
            OnLoadTotalProgress?.Invoke(1f);
            // VerifyData();
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void InitializeData()
        {
            foreach (var item in dataManagers)
            {
                item.Initialize();
            }
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        private void VerifyData()
        {
#if UNITY_EDITOR
            if (DebugUtils.IsLogVerifyData)
            {
                foreach (var item in dataManagers)
                {
                    item.VerifyData();
                }
            }
#endif
        }
    }
}