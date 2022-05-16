using Cinemachine;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(CinemachineBlendListCamera))]
    public class ViewTimeline : MonoBehaviour
    {
        protected const string TOOL_TIP = "0보다 작을 경우에는 호출하지 않음";

        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float duration = 5f;

        public event System.Action OnStart;
        public event System.Action OnFinish;

        CinemachineBlendListCamera blendListCamera;
        GameObject myGameObject;

        public CinemachineVirtualCameraBase[] ChildCameras => blendListCamera.ChildCameras;

        protected virtual void Awake()
        {
            blendListCamera = GetComponent<CinemachineBlendListCamera>();
            myGameObject = gameObject;
        }

        protected virtual void OnEnable()
        {
            blendListCamera.enabled = true;

            OnStart?.Invoke();

            Invoke(duration, OnFinish);
        }

        protected virtual void OnDisable()
        {
            Timing.KillCoroutines(myGameObject);

            blendListCamera.enabled = false;
        }

        protected void Invoke(float delay, System.Action action)
        {
            Timing.RunCoroutine(YieldInvoke(delay, action), myGameObject);
        }

        private IEnumerator<float> YieldInvoke(float delay, System.Action action)
        {
            if (delay < 0f)
                yield break;

            if (delay > 0f)
                yield return Timing.WaitForSeconds(delay);

            action?.Invoke();
        }
    }
}