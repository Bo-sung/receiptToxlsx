using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using MyGoogleServices;
using SheetViewer;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Gma.System.MouseKeyHook;

namespace MailMaker
{
    public struct Position
    {
        public uint X { get; set; }
        public uint Y { get; set; }

        public int IntX { get => (int)X; set => value = (int)X; }
        public int intY { get => (int)Y; set => value = (int)Y; }

        public static implicit operator Point(Position point)
        {
            return new Point(point.X, point.Y);
        }
    }
    public class MouseKeyboardController : ApplicationContext
    {
        private IKeyboardMouseEvents globalHook;
        public static System.Action OnMouseDown;
        public static System.Action OnMouseUp;
        public static System.Action OnMouseMove;
        public static System.Action OnMouseClick;
        public static System.Action OnEscInput;
        public MouseKeyboardController()
        {
            globalHook = Hook.GlobalEvents();
            globalHook.MouseDown += GlobalHook_MouseDown;
            globalHook.MouseUp += GlobalHook_MouseUP;
            globalHook.MouseMove += GlobalHook_MouseMove;
            globalHook.MouseClick += GlobalHook_MouseMove;
            globalHook.KeyPress += GlobalHook_KeyPress;
        }
        ~MouseKeyboardController()
        {
            globalHook.MouseDown -= GlobalHook_MouseDown;
            globalHook.MouseUp -= GlobalHook_MouseUP;
            globalHook.MouseMove -= GlobalHook_MouseMove;
            globalHook.MouseClick -= GlobalHook_MouseMove;

        }
        private void GlobalHook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseDown?.Invoke();
        }
        private void GlobalHook_MouseUP(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseUp?.Invoke();
        }
        private void GlobalHook_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseClick?.Invoke();
        }
        private void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseMove?.Invoke();
        }

        private void GlobalHook_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)        
        {
            if (e.KeyChar == 27)
            {
                OnEscInput?.Invoke();
            }
        }

        //[DllImport("User32.dll")]
        //public static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, UIntPtr dwExtraInfo);


        [DllImport("User32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        protected Point mousePosition;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000; //	The dx and dy parameters contain normalized absolute coordinates.If not set, those parameters contain relative data: the change in position since the last reported position.This flag can be set, or not set, regardless of what kind of mouse or mouse-like device, if any, is connected to the system. For further information about relative mouse motion, see the following Remarks section.
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002; //    The left button is down.
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;//    The left button is up.
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;//    The middle button is down.
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;//    The middle button is up.
        public const uint MOUSEEVENTF_MOVE = 0x0001;//    Movement occurred.
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;//    The right button is down.
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;//    The right button is up.
        public const uint MOUSEEVENTF_WHEEL = 0x0800;//    The wheel has been moved, if the mouse has a wheel.The amount of movement is specified in dwData or The wheel button is rotated
        public const uint MOUSEEVENTF_XDOWN = 0x0080;//    An X button was pressed.
        public const uint MOUSEEVENTF_XUP = 0x0100;//    An X button was released.
        public const uint MOUSEEVENTF_HWHEEL = 0x01000; //The wheel button is tilted. 

        [DllImport("User32.dll")]
        public static extern bool GetCursorPos(out Position position);
        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(Position position);

        public static void MoveAndLeftClick(Position MousePosition)
        {
            MouseKeyboardController.SetCursorPos(MousePosition);
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void LeftClick()
        {
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public static void RightClick()
        {
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        public static void WheelClick(Position MousePosition)
        {
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
            MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }
        public static void RightPressDown() { MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0); }
        public static void RightPressUp() { MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0); }
        public static void LeftPressDown() { MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); }
        public static void LeftPressUP() { MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); }
        public static void WheelPressDown() { MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0); }
        public static void WheelPressUP() { MouseKeyboardController.mouse_event(MouseKeyboardController.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0); }
        public static void Move(Position MousePosition) { MouseKeyboardController.SetCursorPos(MousePosition); }
    }
}
