using System;
using System.Collections.Generic;
using MailMaker.Scripts.AutoBan;
using MailMaker;
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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Gma.System.MouseKeyHook;

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


        //VersionSheets

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
        private (List<GeneralSheet.Layout>, List<string>) _generalSheet;
        public List<ProcessBlock> ProcessBlockList { get; set; }
        public List<IProcess> processList;
        int _pid;
        bool _isModelEnabled;
        int _bitFlag_interrupt = -1;
        bool isPlaying;
        bool isRepeatMode;
        private StackPanel _processPanel;

        public void AddEvent()
        {
            throw new NotImplementedException();
        }

        public void RemoveEvent()
        {
            throw new NotImplementedException();
        }
        public AutoBanSheetPresenter(StackPanel processPanel)
        {
            _processPanel = processPanel;
        }

        public void ModelImport(SheetModel sheetModel)
        {
            _sheetmodel = sheetModel;
            _isModelEnabled = true;
        }

        public AutoBanSheetPresenter(SheetModel sheetModel, StackPanel processPanel)
        {
            _sheetmodel = sheetModel;
            _isModelEnabled = true;
            _processPanel = processPanel;
        }

        public List<VersionSheet.Layout> GetSheetLayoutList()
        {
            if (_isModelEnabled)
                return _sheetmodel.GetVerSheetList();

            return null;
        }

        public (List<GeneralSheet.Layout>, List<string>) GeneralSheetLayoutList()
        {
            if (_isModelEnabled)
            {
                _generalSheet = _sheetmodel.GetGeneralSheet();
                return _generalSheet;
            }
            else
                return (null, null);
        }

        public void AddProcess(int typeindex, Position PrevMousePos)
        {
            if (processList == null)
            {
                processList = new List<IProcess>();
                ProcessBlockList = new List<ProcessBlock>();
                _pid = 0;
            }
            ProcessBlock process = MakeProcess((ProcessType)typeindex, PrevMousePos);
            process.GetProcess().ProcessDone += DoneProcess;

            processList.Add(process.GetProcess());
            ProcessBlockList.Add(process);
            _processPanel.Children.Add(process.GetBorder());
        }

        public ProcessBlock MakeProcess(ProcessType typeindex, Position prevMousePos)
        {
            IProcess process;
            switch (typeindex)
            {
                case ProcessType.Pause:
                    process = new PauseProcess(_pid++, "", prevMousePos, ProcessType.Pause);
                    break;
                case ProcessType.Click:
                    process = new ClickProcess(_pid++, "", prevMousePos, ProcessType.Click);
                    break;
                case ProcessType.Copy:
                    process = new CopyProcess(_pid++, "", prevMousePos, ProcessType.Copy);
                    break;
                case ProcessType.Paste:
                    process = new PasteProcess(_pid++, "", prevMousePos, ProcessType.Paste);
                    break;
                case ProcessType.InputValue:
                    process = new InputProcess(_pid++, "", prevMousePos, ProcessType.InputValue);
                    break;
                case ProcessType.InputFromChart:
                    process = new InputProcess(_pid++, "", prevMousePos, ProcessType.InputFromChart);
                    break;
                case ProcessType.InputFromChartOne:
                    process = new InputProcess(_pid++, "", prevMousePos, ProcessType.InputFromChartOne);
                    break;
                case ProcessType.SelectAll:
                    process = new SelectProcess(_pid++, "", prevMousePos, ProcessType.SelectAll);
                    break;
                case ProcessType.Delete:
                    process = new DeleteProcess(_pid++, "", prevMousePos, ProcessType.Delete);
                    break;
                default:
                    {
                        return null;
                    }
            }
            //
            //if(isPlaying)
            //    process.NextProcess = new LastProcess(_pid, "", new Position(), ProcessType.Last);

            return new ProcessBlock(process, this);
        }

        public void StartProcess()
        {
            if (isPlaying)
            {
                System.Windows.MessageBox.Show("Already Playing!!");
            }

            if (processList.Count == 0)
            {
                System.Windows.MessageBox.Show("Nothing to do!!");
            }
            isPlaying = true;
            ProcessBlockList[0].ChangeColor(ProcessBlock.State.Processing);
            processList[0].Start();
        }

        public void RepeatProcess()
        {
            isRepeatMode = !isRepeatMode;
        }


        public void InterruptProcess(int Flag)
        {
            _bitFlag_interrupt = Flag;

        }
        public void DoneProcess()
        {

            if (processList.Count == 0)
            {
                isPlaying = false;
                return;
            }
            //리스트 삭제 전 중단
            if (_bitFlag_interrupt == 0)
            {
                _bitFlag_interrupt = -1;
                return;
            }

            if (!isRepeatMode)
            {
                processList.RemoveAt(0);
                ProcessBlockList.RemoveAt(0);
                _processPanel.Children.RemoveAt(0);
            }
            else
            {
                processList.Add(processList[0]);
                ProcessBlockList.Add(ProcessBlockList[0]);
                Border temp = (Border)_processPanel.Children[0];
                ProcessBlockList[0].ChangeColor(ProcessBlock.State.Waiting);
                processList.RemoveAt(0);
                ProcessBlockList.RemoveAt(0);
                _processPanel.Children.RemoveAt(0);
                _processPanel.Children.Add(temp);
            }

            //리스트 삭제 후 다음 프로세스 시작 전 중단
            if (_bitFlag_interrupt == 1)
            {
                _bitFlag_interrupt = -1;
                return;
            }

            if (processList.Count != 0)
                processList[0].Start();

            //프로세스 리스트는 남아있더라도 프로세스 블록 리스트는 없을수 있음(LastProcess)
            if (ProcessBlockList.Count != 0)
                ProcessBlockList[0].ChangeColor(ProcessBlock.State.Processing);
        }


        public void RemoveFromPanel(string pid)
        {

        }

        public void ResetAll()
        {
            processList.RemoveAt(0);
            ProcessBlockList.RemoveAt(0);
            _processPanel.Children.RemoveAt(0);
        }

        public void ResetFirst()
        {
            processList.RemoveAt(0);
            ProcessBlockList.RemoveAt(0);
            _processPanel.Children.RemoveAt(0);
        }

        public void ResetLast()
        {
            processList.RemoveAt(processList.Count - 1);
            ProcessBlockList.RemoveAt(ProcessBlockList.Count - 1);
            _processPanel.Children.RemoveAt(_processPanel.Children.Count - 1);
        }
    }

}
