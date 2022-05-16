using UnityEngine;

namespace Ragnarok
{
    [System.Serializable]
    public struct AssetBundleSettings
    {
        public enum Mode
        {
            AssetBundle = 1,
            StreamingAssets = 2,

#if UNITY_EDITOR
            Editor = 3,
            RawEditor = 4,
#endif
        }

        [SerializeField]
        [Tooltip("어셋번들 다운로드 할 기본 주소")]
        string baseURL;

        [SerializeField]
        [Tooltip("어셋번들 로드 방식")]
        Mode mode;

        public AssetLoader GetAssetLoader(int assetVersion)
        {
            switch (mode)
            {
                case Mode.AssetBundle:
                    return new WebAssetLoader(baseURL, assetVersion, true);

                case Mode.StreamingAssets:
#if UNITY_ANDROID
    #if UNITY_EDITOR
                    return new WebAssetLoader(string.Concat("file:///", Application.streamingAssetsPath), assetVersion, false);
#else
                    return new WebAssetLoader(Application.streamingAssetsPath, assetVersion, false);
#endif
#else
                    return new WebAssetLoader(string.Concat("file:///", Application.streamingAssetsPath), assetVersion, false);
#endif

#if UNITY_EDITOR
                case Mode.Editor:
                    return new EditorAssetLoader(string.Concat("file:///", Application.dataPath));

                case Mode.RawEditor:
                    return new EditorAssetLoader();
#endif
            }

            throw new System.Exception($"AssetLoader를 가져올 수 없습니다: {nameof(mode)} = {mode}");
        }

        public Mode GetMode()
        {
            return mode;
        }

        public void SetMode(Mode mode)
        {
            this.mode = mode;
        }
    }
}