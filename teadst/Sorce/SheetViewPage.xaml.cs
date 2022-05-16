using System;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;


namespace SheetViewer
{
    using ClipboardHelper;
    using MyGoogleServices;



    /// <summary>
    /// SheetViewPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SheetViewPage : Page
    {
        SheetPresenter presenter;
        String printedText;
        string ContentsInputBox_DefalutText;// = "추가로 입력할 내용은 여기에 기입.   줄 시작시 스페이스바로 내어쓰기 사용 가능.  !버전 입력시 현재 버전 자동 기입 ex)현재 버전 42.2422.2일떄 !버전 -&gt; 42.2422. !버전1 -&gt; 42.2422.1";

        public SheetViewPage()
        {
            //시트 모델 로드
            presenter = new SheetPresenter
                (
                new SheetModel
                (
                    GoogleConnectionMannager.GetInstance().ActivatedManager,
                    "1jLZ3UmfGTPzjSUSpCtJu1Y6jPLqHQtZc1cWOgVXf91U",
                    "버전!A2:IZ"
                    )
                );
            InitializeComponent();
            ContentsInputBox_DefalutText = ContentsInputBox.Text;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            String toPrint = presenter.PrintToHtml(EditorNameBox.Text);
            if (toPrint == "")
            {
                LatestVerPrintTextBox.Text = "ERROR Failed To PrintToHTML";
                return;
            }
            else
            {
                PrintHtmlAtScreen(toPrint);
                printedText = toPrint;
                if (presenter.TryGetLayoutLine(out String str))
                {
                    LatestVerPrintTextBox.Text = str;
                }
            }
        }

        /// <summary>
        /// webBrowser 컨트롤에 편집된 데이터 출력.
        /// </summary>
        /// <param name="_htmlstring"></param>
        public void PrintHtmlAtScreen(String _htmlstring)
        {
            //WebBrowser 컨트롤에 HTML 바로 출력.
            HtmlPrint.NavigateToString(_htmlstring);
        }
        private void URLMakeBtn_Click(object sender, RoutedEventArgs e)
        {
            presenter.CreateDownloadLink(AppNameBox.Text.Trim());
        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            if (presenter.TryGetLayoutLine(out String str))
            {
                LatestVerPrintTextBox.Text = str;
            }
        }
        private void AddContentsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (printedText == null)
            {
                MessageBox.Show("printedText is Null", "Error");
                return;
            }
            String[] forAdd = new String[ContentsInputBox.LineCount];
            for (int i = 0; i < ContentsInputBox.LineCount; ++i)
            {
                forAdd[i] = ContentsInputBox.GetLineText(i);
                forAdd[i] = forAdd[i].Replace("\n", " ").Replace("\r", " ");
            }
            printedText = presenter.GetConvertedContents(printedText, forAdd, ContentsInputBox.LineCount);
            PrintHtmlAtScreen(printedText);
            ClipboardHelper.CopyToClipboard(printedText, printedText);    //클립보드에 작성한 데이터 출력. 바로 붙혀넣기 하면 가능.
        }

        private void GetPlatformBoxText(object sender, TextChangedEventArgs e)
        {
            presenter.GetPlatformBoxText(PlatformInputBox.Text);
        }
        private void GetAppNameBoxText(object sender, TextChangedEventArgs e)
        {
            presenter.GetAppNameBoxText(AppNameBox.Text);
        }
        private void GetVersionBoxText(object sender, TextChangedEventArgs e)
        {
            presenter.GetVersionBoxText(SheetVersionInputBox.Text);
        }

        private void FileSerchBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog.ShowDialog().ToString() == "OK")
            {
                AppNameBox.Text = openFileDialog.FileName;
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {

        }

        private void OnDragOver(object sender, DragEventArgs e)
        {

        }

        private void OnDropEnter(object sender, DragEventArgs e)
        {
            //aab파일이면 복사
            string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
            AppNameBox.Text = Path.GetFileName(data[0]);
            if (data[0].Contains(".aab"))
                AabConvertor.GetInstance().Initiallize(data[0]);
        }
        
        private void ContentsInputBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (ContentsInputBox.Text.Equals(ContentsInputBox_DefalutText))
            {
                ContentsInputBox.Text = "";
            }
        }

        private void ContentsInputBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (ContentsInputBox.Text.Equals(""))
            {
                ContentsInputBox.Text = ContentsInputBox_DefalutText;
            }
        }
    }

}
