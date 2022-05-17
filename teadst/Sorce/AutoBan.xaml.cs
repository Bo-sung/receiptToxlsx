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
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Page1 : Page
    {
        List<IProcess> processList;
        AutoBanSheetPresenter presenter;
        Position prevMousePosition;
        MouseKeyboardController mouseController;
        int index;
        System.Action InputAction;
        public Page1()
        {
            InitializeComponent();
        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            if (presenter == null)
            {
                //시트 모델 로드
                presenter = new AutoBanSheetPresenter
                    (
                    new SheetModel
                    (
                        GoogleConnectionMannager.GetInstance().ActivatedManager,
                        "1jLZ3UmfGTPzjSUSpCtJu1Y6jPLqHQtZc1cWOgVXf91U",
                        FilePathInputBox.Text
                        )
                    );
            }
            (List<TestSheet.Layout> sheets, List<String> heads) = presenter.GetTestSheetLayoutList();
            ObservableCollection<TestSheet.Layout> dump = new ObservableCollection<TestSheet.Layout>(sheets);
            ExcelDataGrid.DataContext = dump;
            for (int i = 0; i < heads.Count; i++)
            {
                ExcelDataGrid.Columns[i].Header = heads[i];
            }
        }

        private void SetPointBtn_Click(object sender, RoutedEventArgs e)
        {
            MouseDown += GetMousePoint;
            InputAction += CheckMouseClick;
            this.MouseLeave += MouseLeaved;
            this.MouseEnter += MouseComeback;
        }
        public void MouseComeback(object sender, EventArgs e)
        {
            MouseKeyboardController.OnMouseDown -= CheckMouseClick;
        }
        public void MouseLeaved(object sender, EventArgs e)
        {
            MouseKeyboardController.OnMouseDown += CheckMouseClick;
        }
        public void CheckMouseClick()
        {
            MouseKeyboardController.GetCursorPos(out prevMousePosition);
            MouseDown -= GetMousePoint;
            InputAction -= CheckMouseClick;
            this.MouseLeave -= MouseLeaved;
            this.MouseEnter -= MouseComeback;
            MouseKeyboardController.OnMouseDown -= CheckMouseClick;
            System.Windows.MessageBox.Show($"{prevMousePosition.X},{prevMousePosition.Y}");
            MouseKeyboardController.Move(prevMousePosition);
        }

        public void GetMousePoint(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            MouseDown -= GetMousePoint;
            if (MouseKeyboardController.GetCursorPos(out prevMousePosition))
                InputAction?.Invoke();
        }
        private void AddProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            if (processList == null)
            {
                processList = new List<IProcess>();
                index = 0;
            }

            IProcess processs;
            switch (ModeSelector.SelectedIndex)
            {
                case (int)ProcessType.Pause:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Pause);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Pause);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.Click:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Click);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Click);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.Copy:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Copy);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Copy);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.Paste:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Paste);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Paste);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.InputValue:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.InputValue);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.InputValue);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.InputFromChart:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.InputFromChart);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.InputFromChart);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.InputFromChartOne:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.InputFromChartOne);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.InputFromChartOne);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.SelectAll:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.SelectAll);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.SelectAll);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                case (int)ProcessType.Delete:
                    {
                        if (index == 0)
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Delete);
                        else
                        {
                            processs = new ClickProcess(index, "", prevMousePosition, ProcessType.Delete);
                            processs.NextProcess = processList[index - 1];
                        }
                        index++;
                    }
                    break;
                default:
                    {
                        return;
                    }

            }
            ProcessBlock process = new ProcessBlock(processs);
            processList.Add(process.GetProcess());
            ProcessPanel.Children.Add(process.GetBorder());
        }
    }

    public enum ProcessType
    {
        Pause = 0,
        Click = 1,
        Copy = 2,
        Paste = 3,
        InputValue = 4,
        InputFromChart = 5,
        InputFromChartOne = 6,
        SelectAll = 7,
        Delete = 8,
    }


    public interface IProcess
    {
        int PID { get; }
        IProcess NextProcess { get; set; }
        string Value { get; set; }
        Position MousePosition { get; set; }
        ProcessType CurProcessType { get; set; }
        /// <summary>
        /// 프로세스 시작
        /// </summary>
        void Start();
        /// <summary>
        /// 프로세스. 0.1초당 한번씩 호출됨
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Process(object sender, EventArgs e);
        /// <summary>
        /// 프로세스 종료
        /// </summary>
        void End();
    }

    public class MouseKeyboardController : ApplicationContext
    {
        private IKeyboardMouseEvents globalHook;
        public static System.Action OnMouseDown;
        public static System.Action OnMouseUp;
        public static System.Action OnMouseMove;
        public static System.Action OnMouseClick;
        public MouseKeyboardController()
        {
            globalHook = Hook.GlobalEvents();
            globalHook.MouseDown += GlobalHook_MouseDown;
            globalHook.MouseUp += GlobalHook_MouseUP;
            globalHook.MouseMove += GlobalHook_MouseMove;
            globalHook.MouseClick += GlobalHook_MouseMove;
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


    public struct Position
    {
        public uint X { get; set; }
        public uint Y { get; set; }

        public int intX { get => (int)X; set => value = (int)X; }
        public int intY { get => (int)Y; set => value = (int)Y; }

        public static implicit operator Point(Position point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public class ClickProcess : IProcess
    {
        public int PID { get; }
        private DispatcherTimer UpdateLoop { get; set; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }

        public ClickProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            Value = value;
            MousePosition = mousePosition;
            CurProcessType = processType;
        }

        public void Start()
        {
            UpdateLoop = new DispatcherTimer();
            UpdateLoop.Tick += Process;
            UpdateLoop.Start();
        }

        public void Process(object sender, EventArgs e)
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
        }
        public void End()
        {
            UpdateLoop.Tick -= Process;
            UpdateLoop?.Stop();
            if (NextProcess != null)
                NextProcess.Start();
        }
    }

    public class CopyProcess : IProcess
    {
        public int PID { get; }
        private DispatcherTimer UpdateLoop { get; set; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }

        public CopyProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }

        public void Start()
        {
            UpdateLoop = new DispatcherTimer();
            UpdateLoop.Tick += Process;
            UpdateLoop.Start();
        }

        public void Process(object sender, EventArgs e)
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            SendKeys.Send("^c");
        }

        public void End()
        {
            UpdateLoop.Tick -= Process;
            UpdateLoop?.Stop();
            if (NextProcess != null)
                NextProcess.Start();
        }
    }

    public class PasteProcess : IProcess
    {
        public int PID { get; }
        private DispatcherTimer UpdateLoop { get; set; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }

        public PasteProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            UpdateLoop = new DispatcherTimer();
            UpdateLoop.Tick += Process;
            UpdateLoop.Start();
        }

        public void Process(object sender, EventArgs e)
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            SendKeys.Send("^v");
        }

        public void End()
        {
            UpdateLoop.Tick -= Process;
            UpdateLoop?.Stop();
            if (NextProcess != null)
                NextProcess.Start();
        }
    }

    public class InputProcess : IProcess
    {
        public int PID { get; }
        private DispatcherTimer UpdateLoop { get; set; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }

        public void Start()
        {
            UpdateLoop = new DispatcherTimer();
            UpdateLoop.Tick += Process;
            UpdateLoop.Start();
        }

        public void Process(object sender, EventArgs e)
        {
            SendKeys.Send("^c");
        }

        public void End()
        {
            UpdateLoop.Tick -= Process;
            UpdateLoop?.Stop();
            if (NextProcess != null)
                NextProcess.Start();
        }
    }

    /// <summary>
    /// 프로세스 띄워줄 블록
    /// </summary>
    public class ProcessBlock
    {
        IProcess process;
        Border border;
        StackPanel panel;
        TextBlock processText;
        TextBlock positionText;
        System.Windows.Controls.TextBox valueInput;
        public ProcessBlock(IProcess process)
        {
            this.process = process;
            border = new Border();
            border.Margin = new Thickness(3, 3, 3, 3);
            
            panel = new StackPanel();

            processText = new TextBlock();
            processText.Text = GetProcessName();
            processText.VerticalAlignment = VerticalAlignment.Stretch;
            processText.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            positionText = new TextBlock();
            positionText.Text = $"({process.MousePosition.X},{process.MousePosition.Y})";
            positionText.VerticalAlignment = VerticalAlignment.Stretch;
            positionText.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            valueInput = new System.Windows.Controls.TextBox();
            valueInput.VerticalAlignment = VerticalAlignment.Stretch;
            valueInput.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            valueInput.TextChanged += ValueChanged;
            panel.Children.Add(processText);
            panel.Children.Add(positionText);
            panel.Children.Add(valueInput);
            border.Child = panel;
        }

        ~ProcessBlock()
        {
            valueInput.TextChanged -= ValueChanged;
        }

        public Border GetBorder()
        {
            return border;
        }

        public IProcess GetProcess()
        {
            return process;
        }


        public void ValueChanged(object sender, TextChangedEventArgs e)
        {
            process.Value = valueInput.Text;
        }


        private string GetProcessName()
        {
            switch (process.CurProcessType)
            {
                case ProcessType.Click:
                    return "클릭";
                case ProcessType.Copy:
                    return "복사";
                case ProcessType.Paste:
                    return "붙혀넣기";
                case ProcessType.InputValue:
                    return "입력{Value}";
                case ProcessType.InputFromChart:
                    return "입력 (차트에서 하나씩 아래로) {Range}";
                case ProcessType.InputFromChartOne:
                    return "입력 (차트의 특정 값) {Range}";
                case ProcessType.SelectAll:
                    return "전체 선택";
                case ProcessType.Delete:
                    return "삭제";
                default:
                    return "대기";
            }

        }
    }

}
