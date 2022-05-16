using UnityEngine;

namespace Ragnarok
{
    public class UICameraTouchManager : MonoBehaviour
    {
        private Camera mainCamera;
        private readonly float targetWidth = 720;
        private readonly float targetHeight = 1280;
        private float extraHeightRate;
        private float extraWidthRate;

        private float lastScreenWidth;
        private float lastScreenHeight;

        private void Update()
        {
            // Find Camera
            if (mainCamera == null)
                mainCamera = GetComponent<Camera>();

            if (!UICamera.isOverUI) // 마지막 레이캐스트가 UI에 있을 경우
                return;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                SpawnTouchEffect(Input.GetTouch(0).position);

            if (Input.GetMouseButtonDown(0))
                SpawnTouchEffect(Input.mousePosition);
        }

        /// <summary>
        /// 해상도 비율 갱신
        /// </summary>
        void CalcExtraScreenSizeRate()
        {
            if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
                return;

            float originRate = targetHeight / targetWidth;
            float screenRate = (float)Screen.height / Screen.width;
            extraHeightRate = screenRate / originRate;
            if (extraHeightRate < 1f)
            {
                extraWidthRate = 1f / extraHeightRate;
                extraHeightRate = 1f;
            }
            else
                extraWidthRate = 1f;

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        void SpawnTouchEffect(Vector3 touchPosition)
        {
            if (!AssetManager.IsAllAssetReady)
                return;

            CalcExtraScreenSizeRate();

            touchPosition = mainCamera.ScreenToViewportPoint(touchPosition);
            touchPosition.x = (touchPosition.x - 0.5f) * targetWidth * extraWidthRate;
            touchPosition.y = (touchPosition.y - 0.5f) * targetHeight * extraHeightRate;
            touchPosition.z = 0f;

            IHUDPool hudPool = HUDPoolManager.Instance;
            hudPool.SpawnTouchEffect(touchPosition);
        }

    }
}