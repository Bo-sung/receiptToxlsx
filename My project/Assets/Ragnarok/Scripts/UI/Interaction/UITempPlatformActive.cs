using UnityEngine;

namespace Ragnarok
{
    public sealed class UITempPlatformActive : MonoBehaviour
    {
        private enum Platform
        {
            Android = 1,
            iOS = 2,
        }

        [SerializeField] Platform platform;
        [SerializeField] bool isActive;

        void Awake()
        {
            switch (platform)
            {
#if UNITY_ANDROID
                case Platform.Android:
                    SetActive(isActive);
                    break;
#endif
#if UNITY_IOS || UNITY_IPHONE
                case Platform.iOS:
                    SetActive(isActive);
                    break;
#endif
            }
        }

        private void SetActive(bool isActive)
        {
#if UNITY_EDITOR
            Debug.Log($"[{GetType()}] 에 의해 변경: {nameof(isActive)} = {isActive}", this);
#endif
            gameObject.SetActive(isActive);
        }
    }
}