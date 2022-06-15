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
using System.Windows.Shapes;

namespace MailMaker
{
    /// <summary>
    /// PositionInputPopup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PositionInputPopup : Window
    {
        public System.Action<int, int> result;
        int _x;
        int _y;
        public PositionInputPopup()
        {
            InitializeComponent();
            PositionXInput.Text = 0.ToString();
            PositionYInput.Text = 0.ToString();
        }
        public PositionInputPopup(System.Action<int,int> result)
        {
            InitializeComponent();
            this.result = result;
            PositionXInput.Text = 0.ToString();
            PositionYInput.Text = 0.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            result?.Invoke(_x,_y);
            Close();
        }

        private void xChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(PositionXInput.Text, out _x);
        }
        private void yChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(PositionYInput.Text, out _y);
        }

        private void xGotFocus(object sender, RoutedEventArgs e)
        {
            PositionXInput.Text = "";
        }
        private void yGotFocus(object sender, RoutedEventArgs e)
        {
            PositionYInput.Text = "";
        }
    }
}
