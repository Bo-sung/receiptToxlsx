using UnityEngine;

namespace Ragnarok
{
    public class ObjectPicking : MonoBehaviour
    {
        private const float LONG_PRESS_TIME = 1f;
        private const float TOUCH_DRAG_THRESHOLD = 40f;
#if UNITY_EDITOR
        private const float MOUSE_DRAG_THRESHOLD = 4f;
#endif

        private enum TouchState
        {
            None,
            ScreenTouch,
#if UNITY_EDITOR
            MouseClick,
#endif
        }

        private enum PressType
        {
            None,
            Press,
            LongPress,
            Drag,
        }

        private Camera mainCamera;
        [SerializeField]
        private LayerMask layer;

        public System.Action onTouch, onClick;
        public System.Action<RaycastHit> onTouchHit, onClickHit;

        private RaycastHit rayHit;
        private Vector3 pressPosition, currentPosition;
        private float pressTime;
        private TouchState savedState;
        private PressType pressType;

        void Update()
        {
            // Find Camera
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (UICamera.isOverUI) // 마지막 레이캐스트가 UI에 있을 경우
                return;

            TouchState state = GetCurrentTouchState();

            if (state == TouchState.None)
            {
                UpdateTouchState(state);
                return;
            }

            currentPosition = GetCurrentPosition(state);

            UpdatePressType();

            if (UICamera.Raycast(currentPosition)) // UI 위에 래이캐스트 되었을 경우
                return;

            Ray ray = mainCamera.ScreenPointToRay(currentPosition);

            // 레이를 쏴서 제일 가까운 곳에 있는 오브젝트 가져옴
            if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layer))
                UpdateTouchState(state);
        }

        void OnDrawGizmos()
        {
            if (!enabled)
                return;

			if (mainCamera == null)
				return;

            Ray ray = mainCamera.ScreenPointToRay(currentPosition);
            Gizmos.DrawRay(ray);
            Gizmos.DrawLine(ray.origin, ray.direction);
        }

        public void SetTargetLayer(params int[] layers)
        {
            layer = 0;

            for (int i = 0; i < layers.Length; i++)
            {
                layer |= 1 << layers[i];
            }
        }

        private TouchState GetCurrentTouchState()
        {
            if (savedState == TouchState.ScreenTouch)
            {
                if (Input.touchCount == 0)
                    return TouchState.None;
            }
#if UNITY_EDITOR
            else if (savedState == TouchState.MouseClick)
            {
                if (Input.GetMouseButtonUp(0))
                    return TouchState.None;
            }
#endif
            else
            {
                if (Input.touchCount > 0)
                    return TouchState.ScreenTouch;

#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                    return TouchState.MouseClick;
#endif
            }

            return savedState;
        }

        private Vector3 GetCurrentPosition(TouchState state)
        {
            switch (state)
            {
                case TouchState.ScreenTouch:
                    return Input.touches[0].position;

#if UNITY_EDITOR
                case TouchState.MouseClick:
                    return Input.mousePosition; // 현재 포지션 저장
#endif
            }

            return Vector3.zero;
        }

        private void UpdateTouchState(TouchState state)
        {
            if (savedState == state)
                return;

            savedState = state;

            if (state == TouchState.None)
            {
                if (pressType == PressType.Press)
                {
                    onClick?.Invoke();
                    onClickHit?.Invoke(rayHit);
                }
                else
                {
                    // LongPress or Drag
                }

                pressType = PressType.None;
            }
            else
            {
                pressType = PressType.Press;
                pressPosition = currentPosition;
                pressTime = RealTime.time;

                onTouch?.Invoke();
                onTouchHit?.Invoke(rayHit);
                Debug.Log($"{rayHit}");
            }
        }

        private void UpdatePressType()
        {
            if (pressType == PressType.Press)
            {
                if (IsDrag())
                {
                    pressType = PressType.Drag;
                }
                else if (IsLongPress())
                {
                    pressType = PressType.LongPress;
                }
            }
        }

        private bool IsDrag()
        {
            float deletaPosition = (currentPosition - pressPosition).sqrMagnitude;

            switch (savedState)
            {
                case TouchState.ScreenTouch:
                    return deletaPosition > TOUCH_DRAG_THRESHOLD;

#if UNITY_EDITOR
                case TouchState.MouseClick:
                    return deletaPosition > MOUSE_DRAG_THRESHOLD;
#endif
            }

            return false;
        }

        private bool IsLongPress()
        {
            float deltaTime = RealTime.time - pressTime;
            return deltaTime > LONG_PRESS_TIME;
        }
    }
}