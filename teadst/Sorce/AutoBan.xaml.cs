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
using MailMaker.Scripts.AutoBan;
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
        AutoBanSheetPresenter presenter;
        Position prevMousePosition;
        int index;
        System.Action InputAction;
        MouseKeyboardController mouseKeyboardController;

        public Page1()
        {
            InitializeComponent();
            mouseKeyboardController = new MouseKeyboardController();
            presenter = new AutoBanSheetPresenter(ProcessPanel);
            MouseKeyboardController.OnEscInput += InterrupProcess;
        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            if (presenter == null)
            {
                //시트 모델 로드
                presenter.ModelImport
                (
                    new SheetModel
                    (
                        GoogleConnectionMannager.GetInstance().ActivatedManager,
                        "1jLZ3UmfGTPzjSUSpCtJu1Y6jPLqHQtZc1cWOgVXf91U",
                        FilePathInputBox.Text
                    )
                );
            }
            (List<GeneralSheet.Layout> sheets, List<String> heads) = presenter.GeneralSheetLayoutList();
            ObservableCollection<GeneralSheet.Layout> dump = new ObservableCollection<GeneralSheet.Layout>(sheets);
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
            MouseDown -= GetMousePoint;
            if (MouseKeyboardController.GetCursorPos(out prevMousePosition))
                InputAction?.Invoke();
        }

        private void AddProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            presenter.AddProcess(ModeSelector.SelectedIndex, prevMousePosition);
        }
        private void ProcessStartBtn_Click(object sender, RoutedEventArgs e)
        {
            presenter.StartProcess();
        }
        private void ProcessRepeatBtn_Click(object sender, RoutedEventArgs e)
        {
            presenter.RepeatProcess();
            if (ProcessRepeattBtn.Content.Equals("반복"))
                ProcessRepeattBtn.Content = "한번만";
            else
                ProcessRepeattBtn.Content = "반복";
        }
        private void InterrupProcess()
        {
            presenter.InterruptProcess(1);
        }

        private void ResetAllBtn_Click(object sender, RoutedEventArgs e)
        {
            presenter.ResetAll();
        }
    }
}
