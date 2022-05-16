using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// SafeArea 에 맞추어 자동으로 Camera 세팅을 수정
    /// <see cref="UIViewport"/>
    /// <seealso cref="NGUITools.GetSides(Camera, Transform)"/>
    /// </summary>
    //[ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class UISafeAreaResizeViewport : MonoBehaviour
    {
        public delegate void ResizeEvent(Vector3 offset, float scale);

        Transform myTransform, parent;
        Camera mCam;

        public float fullSize = 1f;

#if UNITY_EDITOR
        [HideInInspector, SerializeField] bool useCustom;
        [HideInInspector, SerializeField] Rect customArea;
#endif

        public event ResizeEvent OnResizeScreen;

        void Start()
        {
            myTransform = transform;
            parent = myTransform.parent;

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            mCam = camera;
#else
            mCam = GetComponent<Camera>();
#endif

#if UNITY_EDITOR
            if (customArea == Rect.zero)
                customArea = Screen.safeArea;
#endif
        }

        void LateUpdate()
        {
#if UNITY_EDITOR
            Rect area = useCustom ? customArea : Screen.safeArea;
#else
            Rect area = Screen.safeArea;
#endif

            Vector3 tl = new Vector2(area.xMin, area.yMax);
            Vector3 br = new Vector2(area.xMax, area.yMin);

            Rect rect = new Rect(tl.x / Screen.width, br.y / Screen.height, (br.x - tl.x) / Screen.width, (tl.y - br.y) / Screen.height);
            float size = fullSize * rect.height;

            bool isDirty = false;

            if (mCam.rect != rect)
            {
                mCam.rect = rect;
                isDirty = true;
            }

            if (mCam.orthographicSize != size)
            {
                mCam.orthographicSize = size;
                isDirty = true;
            }

            // Offset 설정
            if (isDirty)
            {
                Vector2 safeAreaSize = area.size; // 안전영역 사이즈
                Vector2 screenOffset = -(NGUITools.screenSize - safeAreaSize) * 0.5f + area.position; // offset (줄여진사이즈 * 0.5 + 안전영역offset)
                Vector3 screenSize = GetScreenSize(mCam, parent); // 실제 보이는 화면 사이즈
                float rateX = screenSize.x / safeAreaSize.x; // x 비율
                float rateY = screenSize.y / safeAreaSize.y; // y 비율
                Vector3 offset = new Vector3(screenOffset.x * rateX, screenOffset.y * rateY); // offset 적용

                myTransform.localPosition = offset;
                OnResizeScreen?.Invoke(offset, size);
            }
        }

        /// <summary>
        /// 실제 보이는 화면 사이즈
        /// </summary>
        private Vector3 GetScreenSize(Camera cam, Transform relativeTo)
        {
            if (!mCam.orthographic)
                return Vector3.zero;

            float depth = Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, 0.5f);
            float os = mCam.orthographicSize;
            float x = os;
            float y = os;

            Rect rect = mCam.rect;
            Vector2 size = NGUITools.screenSize;

            float aspect = size.x / size.y;
            aspect *= rect.width / rect.height;
            x *= aspect;

            Vector3 vector = new Vector3(x, y, depth);
            return relativeTo.InverseTransformPoint(vector) * 2f;
        }
    }
}