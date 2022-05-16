using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UITextureHelper))]
    public class AnimatedTextureLoad : MonoBehaviour
    {
        [SerializeField] UITextureHelper.BundleType bundleType;
        [SerializeField] string namePrefix;
        [SerializeField] int zeroPadCount = 4;
        [SerializeField] bool isAsync = false;

        public int frameIndex;

        UITextureHelper textureHelper;
        private int savedFramIndex;

        void Awake()
        {
            textureHelper = GetComponent<UITextureHelper>();
        }

        void OnEnable()
        {
            SetTexture();
        }

        void LateUpdate()
        {
            SetTexture();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (textureHelper == null)
                return;

            SetTexture();
        }
#endif

        private void SetTexture()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (savedFramIndex == frameIndex)
                return;

            savedFramIndex = frameIndex;

            string indexName = savedFramIndex.ToString();

            if (zeroPadCount > 0)
                indexName = indexName.PadLeft(zeroPadCount, '0');

            textureHelper.Load(bundleType, string.Concat(namePrefix, indexName), isAsync);
            //NGUITools.MarkParentAsChanged(textureHelper.cachedGameObject);
        }
    }
}