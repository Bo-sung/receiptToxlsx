using Cinemachine;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(CinemachineBlendListCamera))]
    public class BlendCameraReceiver : MonoBehaviour
    {
        private bool isLive;
        private bool isNotify;

        /// <summary>
        /// 모든 Blend 가 완료 되었을 경우 호출
        /// </summary>
        public event System.Action OnFinished;

        CinemachineBlendListCamera blendListCamera;

        void Awake()
        {
            blendListCamera = GetComponent<CinemachineBlendListCamera>();
        }
        
        void Update()
        {
            bool isLive = CinemachineCore.Instance.IsLive(blendListCamera);
            if (this.isLive != isLive)
            {
                this.isLive = isLive;
                isNotify = isLive;
            }

            if (!isNotify)
                return;

            // Blend 중일 경우
            if (blendListCamera.IsBlending)
                return;

            // 마지막 카메라가 아닐 경우
            ICinemachineCamera lastCamera = blendListCamera.ChildCameras[blendListCamera.ChildCameras.Length - 1];
            if (blendListCamera.LiveChild != lastCamera)
                return;

            isNotify = false;
            OnFinished?.Invoke();
        }
    }
}