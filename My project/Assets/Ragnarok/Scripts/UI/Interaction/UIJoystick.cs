using UnityEngine;
using System.Collections;

namespace Ragnarok
{
    public class UIJoystick : MonoBehaviour
    {
        public delegate void JoystickStartEvent();
        public delegate void JoystickDragEvent(Vector2 position);
        public delegate void JoystickResetEvent();
        public delegate void JoystickDoubleClickEvent();
        public delegate void JoystickLongPressEvent();
        public delegate void PinchZoomEvent(float zoomDistance);

        [SerializeField]
        [Tooltip("조이스틱: 드래그 되는 오브젝트")]
        Transform target;

        [SerializeField]
        [Tooltip("조이스틱 이동 범위")]
        float radius = 40f;

        [SerializeField]
        [Tooltip("조이스틱 회전: 회전이 되는 오브젝트")]
        Transform rotateTarget;

        [SerializeField]
        [Tooltip("조이스틱 이동 스케일")]
        Vector3 scale = Vector3.one;

        [SerializeField]
        [Tooltip("터치한 곳이 조이스틱 시작점이 되며, 모든 widgetsToCenter 가 터치한 곳으로 이동")]
        bool centerOnPress = true;

        [SerializeField]
        [Tooltip("centerOnPress값이 false라도 터치 시 Alpha값 초기화")]
        bool alphaOnPress = true;

        [SerializeField]
        [Tooltip("centerOnPress값으로 인해 이동된 widgetsToCenter를 처음 위치로 초기화")]
        bool isDefaultInitPos = true;

        [SerializeField]
        [Tooltip("정규화 유무")]
        bool normalize = false;

        [SerializeField]
        [Tooltip("이 값을 넘어야 조이스틱이 작동한다")]
        float deadZone = 2f;

        [SerializeField]
        [Tooltip("FadeOut 될 alpha 값")]
        float fadeOutAlpha = 0.2f;

        [SerializeField]
        [Tooltip("FadeOut 딜레이 시간(초)")]
        float fadeOutDelay = 1f;

        [SerializeField]
        [Tooltip("FadeOut 될 Widget 목록")]
        UIWidget[] widgetsToFade;

        [SerializeField]
        [Tooltip("Center로 움직일 Transform 목록")]
        Transform[] widgetsToCenter;

        [SerializeField]
        [Tooltip("더블클릭 제한 시간")]
        float doubleTapTimeWindow = 0.5f;

        [SerializeField]
        [Tooltip("더블클릭 시, 호출할 함수가 존재하는 Object")]
        GameObject doubleTapMessageTarget;

        [SerializeField]
        [Tooltip("더블클릭 시, 호출할 함수 이름")]
        public string doubleTabMethodeName;

        private float lastTapTime = 0f; // 터치 카운트를 위한 시간 값
        private float[] widgetsDefulatAlpha; // alpha 초기값
        private Vector3 userInitTouchPos; // 터치 시 초기화 되는 Center 위치
        private Vector3 defaultInitPos; // 되돌아갈 위치
        private int tapCount; // 터치 카운트
        private float savedDistance = 0f; // 핀치줌 거리

        /// <summary>
        /// 조이스틱 position
        /// </summary>
        [HideInInspector]
        public Vector2 position;

        /// <summary>
        /// 조이스틱 작동 이벤트
        /// </summary>
        public event JoystickStartEvent OnJoystickStart;

        /// <summary>
        /// 조이스틱 드래그 이벤트
        /// </summary>
        public event JoystickDragEvent OnJoystickDrag;

        /// <summary>
        /// 조이스틱 입력 종료 이벤트
        /// </summary>
        public event JoystickResetEvent OnJoystickReset;

        /// <summary>
        /// 조이스틱 더블클릭 이벤트
        /// </summary>
        public event JoystickDoubleClickEvent OnJoystickDoubleClick;

        /// <summary>
        /// 조이스틱 길게누르기 이벤트
        /// </summary>
        public event JoystickLongPressEvent OnJoystickLongPress;

        /// <summary>
        /// 핀치 줌 인/아웃
        /// </summary>
        public event PinchZoomEvent OnJoystickPinchZoom;

        void Awake()
        {
            userInitTouchPos = Vector3.zero;

            if (target)
                defaultInitPos = target.position;

            widgetsDefulatAlpha = new float[widgetsToFade.Length];

            for (int i = 0; i < widgetsDefulatAlpha.Length; i++)
            {
                widgetsDefulatAlpha[i] = widgetsToFade[i].alpha;
            }
        }

        void Start()
        {
            if (centerOnPress)
            {
                StartCoroutine(FadeOutJoystick());
            }
        }

        void LateUpdate()
        {
            if (tapCount == 0)
                return;

            if (tapCount == 1)
            {
                OnJoystickDrag?.Invoke(position);
            }

#if UNITY_EDITOR
            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
            {
                float currentDistance = Vector2.Distance(Input.mousePosition, new Vector2(Screen.width / 2, Screen.height / 2)) * 2f;
#else
            if (Input.touchCount == 2)
                {
                float currentDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
#endif

                if (savedDistance != 0f)
                    OnJoystickPinchZoom?.Invoke(savedDistance - currentDistance);

                savedDistance = currentDistance;
                return;
            }

#if !UNITY_EDITOR
            if (Input.touchCount != 2)
            {
                savedDistance = 0f;
            }
#endif
        }

        public void ResetJoystick()
        {
            // Release the finger control and set the joystick back to the default position
            tapCount = 0;
            position = Vector2.zero;

            if (isDefaultInitPos)
            {
                foreach (Transform item in widgetsToCenter)
                {
                    item.position = defaultInitPos;
                }
            }

            if (target)
                target.position = isDefaultInitPos ? defaultInitPos : userInitTouchPos;

            if (centerOnPress || alphaOnPress)
            {
                StartCoroutine(FadeOutJoystick());
            }

            OnJoystickReset?.Invoke();
        }

        /// <summary>
        /// Create a plane on which we will be performing the dragging.
        /// </summary>
        void OnPress(bool pressed)
        {
            if (target == null)
                return;

            if (pressed)
            {
                StopAllCoroutines();

                if (Time.time < lastTapTime + doubleTapTimeWindow)
                {
                    ++tapCount;

                    if (doubleTapMessageTarget == null || string.IsNullOrEmpty(doubleTabMethodeName))
                    {
                        // Double Tab on Joystick but no Reciever or Methodename available
                    }
                    else
                    {
                        doubleTapMessageTarget.SendMessage(doubleTabMethodeName, SendMessageOptions.DontRequireReceiver);
                    }
                }
                else
                {
                    tapCount = 1;
                }

                if (tapCount == 1)
                    OnJoystickStart?.Invoke();

#if UNITY_EDITOR
                if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl))
                {
                    savedDistance = Vector2.Distance(Input.mousePosition, new Vector2(Screen.width / 2, Screen.height / 2)) * 2f;
                }
#else
                if (Input.touchCount == 2)
                {
                    savedDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                }
#endif


                lastTapTime = Time.time;

                //set Joystick to fingertouchposition
                Ray ray = UICamera.currentRay;
                float dist = 0f;

                Vector3 currentPos = ray.GetPoint(dist);
                currentPos.z = 0;
                if (centerOnPress)
                {
                    userInitTouchPos = currentPos;

                    foreach (Transform widgetTF in widgetsToCenter)
                    {
                        widgetTF.position = userInitTouchPos;
                    }
                }
                else
                {
                    userInitTouchPos = target.position;
                }

                if (centerOnPress || alphaOnPress)
                {
                    for (int i = 0; i < widgetsToFade.Length; i++)
                    {
                        TweenAlpha.Begin(widgetsToFade[i].cachedGameObject, 0.1f, widgetsDefulatAlpha[i]).method = UITweener.Method.EaseIn;
                    }
                }
            }
            else
            {
                ResetJoystick();
            }

#if UNITY_EDITOR
            if (!Input.GetMouseButton(0) || !Input.GetKey(KeyCode.LeftControl))
            {
                savedDistance = 0f;
            }
#endif
        }

        /// <summary>
        /// Drag the object along the plane.
        /// </summary>
        void OnDrag(Vector2 delta)
        {
            if (target == null)
                return;

            // Debug.Log("delta " +  delta + " delta.magnitude = " + delta.magnitude);
            Ray ray = UICamera.currentRay;
            float dist = 0f;

            Vector3 currentPos = ray.GetPoint(dist);
            Vector3 offset = currentPos - userInitTouchPos;

            if (offset.x != 0f || offset.y != 0f)
            {
                offset = target.InverseTransformDirection(offset);
                offset.Scale(scale);
                offset = target.TransformDirection(offset);
                offset.z = 0f;
            }

            target.position = userInitTouchPos + offset;

            Vector3 zeroZpos = target.position;
            zeroZpos.z = 0f;
            target.position = zeroZpos;

            // Calculate the length. This involves a squareroot operation,
            // so it's slightly expensive. We re-use this length for multiple
            // things below to avoid doing the square-root more than one.
            float length = target.localPosition.magnitude;

            if (length < deadZone)
            {
                // If the length of the vector is smaller than the deadZone radius,
                // set the position to the origin.
                position = Vector2.zero;
                target.localPosition = position;
            }
            else if (length > radius)
            {
                target.localPosition = Vector2.ClampMagnitude(target.localPosition, radius);
                position = target.localPosition;
            }
            else
            {
                position = target.localPosition;
            }

            if (rotateTarget)
                rotateTarget.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg);

            if (normalize)
            {
                // Normalize the vector and multiply it with the length adjusted
                // to compensate for the deadZone radius.
                // This prevents the position from snapping from zero to the deadZone radius.
                //position = position / radius * Mathf.InverseLerp(radius, deadZone, 1);
                position.Normalize();
            }
        }

        public float GetDragDistance()
        {
            if (target == null)
                return 0f;

            Ray ray = UICamera.currentRay;
            float dist = 0f;

            Vector3 currentPos = ray.GetPoint(dist);
            Vector3 offset = currentPos - userInitTouchPos;

            if (offset.x != 0f || offset.y != 0f)
            {
                offset = target.InverseTransformDirection(offset);
                offset.Scale(scale);
                offset = target.TransformDirection(offset);
                offset.z = 0f;
            }

            //offset = userInitTouchPos + offset;
            offset.z = 0f;

            return offset.magnitude;
        }

        void OnDoubleClick()
        {
            OnJoystickDoubleClick?.Invoke();
        }

        void OnLongPress()
        {
            OnJoystickLongPress?.Invoke();
        }

        private IEnumerator FadeOutJoystick()
        {
            if (fadeOutDelay > 0f)
                yield return new WaitForSeconds(fadeOutDelay);

            foreach (UIWidget widget in widgetsToFade)
            {
                TweenAlpha.Begin(widget.gameObject, 0.5f, fadeOutAlpha).method = UITweener.Method.EaseOut;
            }
        }

#if UNITY_EDITOR
        void OnPan(Vector2 delta)
        {
            Vector3 offset = delta;
            target.position = userInitTouchPos + offset;

            Vector3 zeroZpos = target.position;
            zeroZpos.z = 0f;
            target.position = zeroZpos;

            // Calculate the length. This involves a squareroot operation,
            // so it's slightly expensive. We re-use this length for multiple
            // things below to avoid doing the square-root more than one.
            float length = target.localPosition.magnitude;

            if (length < deadZone)
            {
                // If the length of the vector is smaller than the deadZone radius,
                // set the position to the origin.
                position = Vector2.zero;
                target.localPosition = position;
            }
            else if (length > radius)
            {
                target.localPosition = Vector2.ClampMagnitude(target.localPosition, radius);
                position = target.localPosition;
            }
            else
            {
                position = target.localPosition;
            }

            if (rotateTarget)
                rotateTarget.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg);

            if (normalize)
            {
                // Normalize the vector and multiply it with the length adjusted
                // to compensate for the deadZone radius.
                // This prevents the position from snapping from zero to the deadZone radius.
                //position = position / radius * Mathf.InverseLerp(radius, deadZone, 1);
                position.Normalize();
            }
        }
#endif
    }
}