using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIPowerSaving"/>
    /// </summary>
    public sealed class PowerSavingPresenter : ViewPresenter
    {
        // <!-- Managers --!>
        private readonly ConnectionManager connectionManager;

        private bool isInitialize;
        private int savedCameraCullingMask;
        private int savedUiCameraCullingMask;

        public PowerSavingPresenter()
        {
            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void Initialize()
        {
            if (isInitialize)
                return;

            isInitialize = true;

            savedCameraCullingMask = Camera.main.cullingMask;
            savedUiCameraCullingMask = UI.CurrentCamera.cullingMask;

            Camera.main.cullingMask = ~-1; // Layer Nothing
            UI.CurrentCamera.cullingMask = 1 << Layer.UI_Empty;
        }

        public void Dispose()
        {
            if (!isInitialize)
                return;

            isInitialize = false;

            Camera.main.cullingMask = savedCameraCullingMask;
            UI.CurrentCamera.cullingMask = savedUiCameraCullingMask;
        }

        /// <summary>
        /// 로고 반환
        /// </summary>
        public string GetLogo()
        {
            return connectionManager.GetLogoName();
        }

        /// <summary>
        /// 현재 시간 반환
        /// </summary>
        public string GetNowTime()
        {
            return System.DateTime.Now.ToString("H:mm");
        }

        /// <summary>
        /// 현재 배터리 반환
        /// </summary>
        public float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }
    }
}