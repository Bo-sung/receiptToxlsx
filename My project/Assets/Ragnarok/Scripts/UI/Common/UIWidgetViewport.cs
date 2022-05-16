using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UIWidget), typeof(Camera))]
    public class UIWidgetViewport : MonoBehaviour
    {
        public Camera sourceCamera;
        public float fullSize = 1f;

        Camera myCamera;
        UIWidget myWidget;

        void Awake()
        {
            myCamera = GetComponent<Camera>();
            myWidget = GetComponent<UIWidget>();
        }

        void Start()
        {
            myCamera.nearClipPlane = -1;
            myCamera.farClipPlane = 1;

            if (sourceCamera == null)
                sourceCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        }

        void LateUpdate()
        {
            if (sourceCamera == null)
                return;

            var corners = myWidget.worldCorners;

            Vector3 tl = sourceCamera.WorldToScreenPoint(corners[1]);
            Vector3 br = sourceCamera.WorldToScreenPoint(corners[3]);

            Rect rect = new Rect(tl.x / Screen.width, br.y / Screen.height,
                    (br.x - tl.x) / Screen.width, (tl.y - br.y) / Screen.height);

            float size = fullSize * rect.height;

            if (rect != myCamera.rect) myCamera.rect = rect;
            if (myCamera.orthographicSize != size) myCamera.orthographicSize = size;
        }
    }
}