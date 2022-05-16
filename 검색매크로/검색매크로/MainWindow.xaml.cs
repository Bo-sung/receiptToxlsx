using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.IO;

namespace 검색매크로
{
    
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private Microsoft.Office.Interop.Excel.Sheets sheets = null;
            private Microsoft.Office.Interop.Excel.Worksheet worksheet = null;

        public MainWindow()
        {
            InitializeComponent();
            var filepath = new FileInfo(FilePathInputBox.Text);

            using (var package = new (filepath))
            {

            }
        }
    }
}
