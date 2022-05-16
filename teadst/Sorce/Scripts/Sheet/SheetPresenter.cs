using System;

namespace SheetViewer
{
    public class aabConverterPresenter : ViewPresenter
    {
        AabConvertor _aabConvertor;
        public override void AddEvent()
        {

        }
        public override void RemoveEvent()
        {

        }
        public aabConverterPresenter()
        {
            _aabConvertor = AabConvertor.GetInstance();
        }

        public System.Diagnostics.DataReceivedEventHandler OnPrintConsolOutput;
        public EventHandler OnConvertFinished;
        public void Convert(String Droped_path)
        {
            _aabConvertor.Initiallize("BundleTools");
            _aabConvertor.SetConvertFile(Droped_path);
            _aabConvertor.ConvertProcessStart(
                new System.Diagnostics.DataReceivedEventHandler(PrintConsolOutput) + OnPrintConsolOutput,
                new EventHandler(ConverterFinished) + OnConvertFinished
                );
        }
        public void PrintConsolOutput(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {

        }
        public void ConverterFinished(object sender, EventArgs e)
        {

        }
    }

    public class SheetPresenter : ViewPresenter
    {
        private SheetModel _sheetmodel;
        private MailConvertModel mailconvertmodel;
        public override void AddEvent()
        {

        }
        public override void RemoveEvent()
        {

        }
        public SheetPresenter(SheetModel sheetModel)
        {
            _sheetmodel = sheetModel;
            mailconvertmodel = new MailConvertModel();
        }
        public bool IsSheetAccessible()
        {
            return _sheetmodel.IsSheetAccessible();
        }
        /// <summary>
        /// 입력된 버전
        /// </summary>
        String _cur_version;
        /// <summary>
        /// 입력된 플랫폼
        /// </summary>
        String _cur_Platform;
        /// <summary>
        /// 입력된 앱 이름
        /// </summary>
        String _cur_AppName;
        public void GetPlatformBoxText(String data)
        {
            _cur_Platform = data;
        }
        public void GetAppNameBoxText(String data)
        {
            _cur_AppName = data;
        }
        public void GetVersionBoxText(String data)
        {
            _cur_version = data;
        }
        public string GetConvertedContents(String Origin, String[] data, int count)
        {
            if (!Origin.Contains("<@Contexts>"))
                return Origin;
            return Origin.Replace("<@Contexts>", mailconvertmodel.GetConntentAddBoxText(data, count));
        }
        public bool TryGetLayoutLine(out String str)
        {
            if (_sheetmodel.TryGetLayoutLine(_cur_version, _cur_Platform, out VersionSheet.Layout output))
            {
                str = output.MakeString();
                return true;
            }
            else
            {
                str = "";
                return false;
            }
        }
        public String PrintToHtml(String Editor)
        {
            if (_sheetmodel.TryGetLayoutLine(_cur_version, _cur_Platform, out VersionSheet.Layout layout))
                return mailconvertmodel.InsertAtFormat(Editor, layout);
            return "";
        }
        public String CreateDownloadLink(String appName)
        {
            return mailconvertmodel.CreateDownloadLink(appName);
        }

    }
}
