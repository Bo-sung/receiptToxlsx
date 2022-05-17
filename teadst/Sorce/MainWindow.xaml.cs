using MyGoogleServices;
using SheetViewer;
using System;
using System.Windows;
namespace SheetViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        GoogleConnectionMannager manager;
        public MainWindow()
        {
            InitializeComponent();

            manager = new GoogleConnectionMannager("credentials.json");
            manager.InjectService(new GoogleSheetManager(manager.credential));

            //Sheet System Start
            
            Uri url = new Uri("AutoBan.xaml", UriKind.Relative);
            frame.Navigate(url);
        }
        private void SheetModBtn_Click(object sender, RoutedEventArgs e)
        {
            //Sheet System Start
            new SheetModel(manager.ActivatedManager, "1jLZ3UmfGTPzjSUSpCtJu1Y6jPLqHQtZc1cWOgVXf91U", "버전!A2:IZ");
            Uri url = new Uri("SheetViewPage.xaml", UriKind.Relative);
            frame.Navigate(url);
        }
    }
}
