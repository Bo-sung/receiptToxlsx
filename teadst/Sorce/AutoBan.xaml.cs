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
        System.Action InputAction;
        System.Action<int, int> InputPositionActon;
        MouseKeyboardController mouseKeyboardController;

        public Page1()
        {
            InitializeComponent();
            mouseKeyboardController = new MouseKeyboardController();
            presenter = new AutoBanSheetPresenter(ProcessPanel);
            MouseKeyboardController.OnEscInput += InterrupProcess;
            presenter.OnUpdateIndexNumber += UpdateIndexNumber;
            MouseKeyboardController.OnMouseMove += UpdateMousePosition;

        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
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
            (List<GeneralSheet.Layout> sheets, List<String> heads) = presenter.GeneralSheetLayoutList();
            ExcelDataGrid.DataContext = sheets;
            ExcelDataGrid.SelectedCellsChanged += GotFocusedCellPoint;
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
            System.Windows.MessageBox.Show($"녹화 완료! (X = {prevMousePosition.X},Y = {prevMousePosition.Y})");

            SavedMousePosX.Text = prevMousePosition.X.ToString();
            SavedMousePosY.Text = prevMousePosition.Y.ToString();
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
            if (ProcessStartBtn.Content.Equals("시작"))
                ProcessRepeattBtn.Content = "중지";
            else
                ProcessRepeattBtn.Content = "시작";
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

        private void SetBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GotFocusedCellPoint(object sender, SelectedCellsChangedEventArgs e)
        {
            //데이터 그리드상의 값의 위치를 바로 찾는 방법을 모르겠음. 우회적으로 유추하는 방법 사용
            Point selectedPos = new Point(0, 0);
            //List<Dictionary<GeneralSheet.LayoutTypes, string>> list =  SheetDataDrawer.Instance.GetGeneralSheetListWithDic();
            List<GeneralSheet.Layout> list = SheetDataDrawer.Instance.GetGeneralSheetLayouts();
            //현재 선택된 데이터 그리드 내 텍스트 블록
            DataGridColumn dataSelectedColumns = ExcelDataGrid.CurrentCell.Column;
            //DataGridRow dataSelectedRows = (DataGridRow)ExcelDataGrid.CurrentItem;
            TextBlock selectedData = (TextBlock)dataSelectedColumns.GetCellContent(ExcelDataGrid.CurrentItem);
            int ColumnIndex = dataSelectedColumns.DisplayIndex;
            int RowIndex = ExcelDataGrid.SelectedIndex;
            string test = selectedData.Text;
            presenter.SelectedCell(ColumnIndex, RowIndex, test, selectedPos);
            RepeatPositionInput.Text = (ColumnIndex, RowIndex, test).ToString();
        }

        public void UpdateMousePosition()
        {
            MouseKeyboardController.GetCursorPos(out Position pos);
            CurrentMousePosX.Text = pos.IntX.ToString();
            CurrentMousePosY.Text = pos.intY.ToString();
        }
        public void UpdateIndexNumber(int index)
        {
            CurrentIndex.Text = index.ToString();
        }

        public void ReciveSetPoint(int x, int y)
        {
            InputPositionActon -= ReciveSetPoint;
            prevMousePosition.X = (uint)x;
            prevMousePosition.Y = (uint)y;

            SavedMousePosX.Text = prevMousePosition.X.ToString();
            SavedMousePosY.Text = prevMousePosition.Y.ToString();
        }

        private void SetPointManualBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InputPositionActon == null)
                InputPositionActon += ReciveSetPoint;
            PositionInputPopup positionInputPopup = new PositionInputPopup(InputPositionActon);
            positionInputPopup.ShowDialog();
        }
    }
}
