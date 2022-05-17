using System;
using System.Collections.Generic;

namespace SheetViewer
{
    public class SheetToMailPresenter : ViewPresenter
    {
        private SheetModel _sheetmodel;
        private MailConvertModel mailconvertmodel;
        public override void AddEvent()
        {

        }
        public override void RemoveEvent()
        {

        }
        public SheetToMailPresenter(SheetModel sheetModel)
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
    public class AutoBanSheetPresenter : IViewPresenter
    {
        private SheetModel _sheetmodel;
        public void AddEvent()
        {
            throw new NotImplementedException();
        }

        public void RemoveEvent()
        {
            throw new NotImplementedException();
        }

        public AutoBanSheetPresenter(SheetModel sheetModel)
        {
            _sheetmodel = sheetModel;
        }

        public List<VersionSheet.Layout> GetSheetLayoutList()
        {
            return _sheetmodel.GetVerSheetList();
        }

        public (List<TestSheet.Layout>,List<string>) GetTestSheetLayoutList()
        {
            return _sheetmodel.GetTestSheets();
        }
        
    }

}
