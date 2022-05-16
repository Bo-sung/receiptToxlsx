using UnityEngine;

namespace Ragnarok
{
    public class UIController : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        private enum KeyCode
        {
            Left = 1,
            Right,
            Up,
            Down,
        }

        private readonly KeyCode[] konamiCommand = { KeyCode.Up, KeyCode.Up, KeyCode.Down, KeyCode.Down, KeyCode.Left, KeyCode.Right, KeyCode.Left, KeyCode.Right, };

        private readonly BetterList<KeyCode> commandList = new BetterList<KeyCode>();

        [SerializeField] UIJoystick joystick;

        /// <summary>
        /// 조이스틱 작동 이벤트
        /// </summary>
        public event UIJoystick.JoystickStartEvent OnStart;

        /// <summary>
        /// 조이스틱 드래그 이벤트
        /// </summary>
        public event UIJoystick.JoystickDragEvent OnDrag;

        /// <summary>
        /// 조이스틱 입력 종료 이벤트
        /// </summary>
        public event UIJoystick.JoystickResetEvent OnReset;

        /// <summary>
        /// 조이스틱 더블클릭 이벤트
        /// </summary>
        public event UIJoystick.JoystickDoubleClickEvent OnDoubleClick;

        /// <summary>
        /// 조이스틱 길게누르기 이벤트
        /// </summary>
        public event UIJoystick.JoystickLongPressEvent OnLongPress;

        /// <summary>
        /// 핀치 줌 인/아웃
        /// </summary>
        public event UIJoystick.PinchZoomEvent OnPinchZoom;

        /// <summary>
        /// 코나미 커맨드 입력 성공
        /// </summary>
        public event System.Action OnSuccessKonamiCommand;

        private KeyCode keyCode;

        protected override void OnInit()
        {
            joystick.OnJoystickStart += OnJoystickStart;
            joystick.OnJoystickDrag += OnJoystickDrag;
            joystick.OnJoystickReset += OnJoystickReset;
            joystick.OnJoystickDoubleClick += OnJoystickDoubleClick;
            joystick.OnJoystickLongPress += OnJoystickLongPress;
            joystick.OnJoystickPinchZoom += OnChangePinchZoom;
        }

        protected override void OnClose()
        {
            joystick.OnJoystickStart -= OnJoystickStart;
            joystick.OnJoystickDrag -= OnJoystickDrag;
            joystick.OnJoystickReset -= OnJoystickReset;
            joystick.OnJoystickDoubleClick -= OnJoystickDoubleClick;
            joystick.OnJoystickLongPress -= OnJoystickLongPress;
            joystick.OnJoystickPinchZoom -= OnChangePinchZoom;
        }

        protected override void OnShow(IUIData data = null)
        {
            joystick.ResetJoystick();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnJoystickStart()
        {
            OnStart?.Invoke();
        }

        void OnJoystickDrag(Vector2 position)
        {
            if (position == Vector2.zero)
                return;

            if (Issue.USE_KONAMI_COMMAND)
            {
                keyCode = GetKeyCode(position);
            }

            OnDrag?.Invoke(position);
        }

        void OnJoystickReset()
        {
            if (Issue.USE_KONAMI_COMMAND)
            {
                commandList.Add(keyCode);

                if (IsEqualKonamiCommand())
                {
                    commandList.Clear();
                    OnSuccessKonamiCommand?.Invoke(); // 코나미 커맨드 성공
                }
                else
                {
                    CleanCommandList();
                }
            }

            OnReset?.Invoke();
        }

        void OnJoystickDoubleClick()
        {
            OnDoubleClick?.Invoke();
        }

        void OnJoystickLongPress()
        {
            OnLongPress?.Invoke();
        }

        void OnChangePinchZoom(float zoomDistance)
        {
            OnPinchZoom?.Invoke(zoomDistance);
        }

        /// <summary>
        /// 액티브 제어
        /// </summary>
        public void SetActive(bool isActive)
        {
            if (gameObject.activeSelf == isActive)
                return;

            if (isActive)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public float GetDragDistance()
        {
            return joystick.GetDragDistance();
        }

        /// <summary>
        /// 코드 방향값 반환
        /// </summary>
        private KeyCode GetKeyCode(Vector2 position)
        {
            Vector2 normalized = position.normalized;

            if (Vector2.Distance(normalized, Vector2.left) < 0.5f)
                return KeyCode.Left;

            if (Vector2.Distance(normalized, Vector2.right) < 0.5f)
                return KeyCode.Right;

            if (Vector2.Distance(normalized, Vector2.up) < 0.5f)
                return KeyCode.Up;

            if (Vector2.Distance(normalized, Vector2.down) < 0.5f)
                return KeyCode.Down;

            return default;
        }

        /// <summary>
        /// 커맨드 확인
        /// </summary>
        private bool IsEqualKonamiCommand()
        {
            if (commandList.size != konamiCommand.Length)
                return false;

            for (int i = 0; i < commandList.size; i++)
            {
                if (commandList[i] != konamiCommand[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 커맨드 리스트 정리
        /// </summary>
        private bool CleanCommandList()
        {
            if (commandList.size == 0)
                return false;

            for (int i = 0; i < commandList.size; i++)
            {
                if (commandList.size <= konamiCommand.Length && commandList[i] == konamiCommand[i])
                    continue;

                commandList.RemoveAt(0); // 처음 하나를 지움
                return CleanCommandList();
            }

            return true;
        }
    }
}