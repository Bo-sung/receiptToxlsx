using UnityEngine;

namespace Ragnarok
{
    public sealed class UIIgnoreSafeArea : MonoBehaviour
    {
        Transform myTransform;
        UIManager uiManager;

        private Vector3 originalPosition;
        private Vector3 originalScale;

        void Awake()
        {
            myTransform = transform;
            uiManager = UIManager.Instance;

            originalPosition = myTransform.localPosition;
            originalScale = myTransform.localScale;

            uiManager.OnResizeSafeArea += ResizeSafeArea;
        }

        void OnDestroy()
        {
            uiManager.OnResizeSafeArea -= ResizeSafeArea;
        }

        void Start()
        {
            ResizeSafeArea();
        }

        private void ResizeSafeArea()
        {
            myTransform.localPosition = (originalPosition - uiManager.SafeAreaOffset) / uiManager.SafeAreaScale;
            myTransform.localScale = originalScale / uiManager.SafeAreaScale;
        }
    }
}