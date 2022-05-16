using UnityEngine;

namespace Ragnarok
{
    public static class CameraUtils
    {
        private static PlayerTrackingSettings playerTrackingSetting;
        private static PlayerTrackingSettings PlayerTrackingSetting
        {
            get
            {
                if (playerTrackingSetting is null)
                    playerTrackingSetting = PlayerTrackingSettings.Instance;
                return playerTrackingSetting;
            }
        }

        public static event System.Action<CameraZoomType> OnZoomEvent;

        public static void Zoom(CameraZoomType type)
        {
            OnZoomEvent?.Invoke(type);
        }

        /// <summary>
        /// 플레이어 이동 시 카메라 효과 호출
        /// </summary>
        public static void InvokePlayerTrackingEffect()
        {
            switch (PlayerTrackingSetting.TrackingType)
            {
                case PlayerTrackingType.ZoomIn:
                    OnZoomEvent?.Invoke(CameraZoomType.ZoomIn);
                    break;

                case PlayerTrackingType.None:
                    // PASS
                    break;

                case PlayerTrackingType.ZoomOut:
                    OnZoomEvent?.Invoke(CameraZoomType.ZoomOut);
                    break;

                default:
                    Debug.Log($"[올바르지 않은 {nameof(PlayerTrackingSetting.TrackingType)}] {nameof(PlayerTrackingSetting.TrackingType)} = {PlayerTrackingSetting.TrackingType}");
                    break;
            }
        }
    }
}