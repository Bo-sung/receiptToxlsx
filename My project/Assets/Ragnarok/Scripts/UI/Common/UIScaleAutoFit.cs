using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// UIRoot 사이즈에 따라 LocalScale을 조절 (ParticleSystem 등 Anchor를 붙일 수 없는 곳에 사용)
    /// </summary>
    [ExecuteInEditMode]
    public class UIScaleAutoFit : MonoBehaviour
    {
        Transform cachedTransform;
        Transform tfUIRoot;
        float lastUIRootSize;
        Vector3 originSize;

        void Awake()
        {
            lastUIRootSize = 0f;
            cachedTransform = transform;
            originSize = cachedTransform.localScale;
            tfUIRoot = NGUITools.GetRoot(gameObject).transform;
        }

        /// <summary>
        /// NGUI사이즈 변경 시 호출. (NGUI.UIRoot의 Broadcast)
        /// </summary>
        void UpdateAnchors()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (tfUIRoot is null || tfUIRoot.localScale.x == lastUIRootSize)
                return;

            float newRate = tfUIRoot.localScale.x / Constants.Screen.UI_CAMERA_SCALE;
            cachedTransform.localScale = originSize * newRate;

            lastUIRootSize = tfUIRoot.localScale.x;
        }


    }
}