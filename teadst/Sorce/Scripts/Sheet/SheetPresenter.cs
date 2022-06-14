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
        private (List<GeneralSheet.Layout>, List<string>) _generalSheetData;
        GeneralSheet generalSheet;
        public List<ProcessBlock> ProcessBlockList { get; set; }
        public List<IProcess> processList;
        int _pid;
        bool _isModelEnabled;
        int _bitFlag_interrupt = -1;
        bool isPlaying;
        bool isRepeatMode;
        private StackPanel _processPanel;

        private int nextIndex = -1;

        //호출시 마다 1씩 증가됨
        public int GetNextIndex()
        {
            return ++nextIndex;
        }

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
            SheetDataDrawer.Instance.Initialize(sheetModel);
        }

        public AutoBanSheetPresenter(SheetModel sheetModel, StackPanel processPanel)
        {
            _sheetmodel = sheetModel;
            _isModelEnabled = true;
            _processPanel = processPanel;
            SheetDataDrawer.Instance.Initialize(sheetModel);
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
                if (generalSheet == null)
                    generalSheet = _sheetmodel.GetGeneralSheet();

                if(_generalSheetData != (null,null))
                    return _generalSheetData;

                _generalSheetData = (generalSheet.Layouts, generalSheet.Heads);
                return _generalSheetData;
            }
            else
                return (null, null);
        }

        public VersionSheet GetVersheetLayout()
        {
            return _sheetmodel.GetVerSheet();
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
                    process = new InputProcess(_pid++, "", prevMousePos, ProcessType.InputValue, this);
                    break;
                case ProcessType.InputFromChart:
                    process = new InputProcess(_pid++, "", prevMousePos, ProcessType.InputFromChart, this);
                    break;
                case ProcessType.InputFromChartOne:
                    process = new InputProcess(_pid++, "", prevMousePos, ProcessType.InputFromChartOne, this);
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
                System.Windows.MessageBox.Show("Stop Playing!!");

                isPlaying = false;
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
            {
                ProcessBlockList[0].ChangeColor(ProcessBlock.State.Processing);
                processList[0].Start();
            }
        }

        public void SetPositionToRepeat(Point positon)
        {
            
        }

        public void RemoveFromPanel(string pid)
        {

        }

        public void ResetAll()
        {
            processList.Clear();
            ProcessBlockList.Clear();
            _processPanel.Children.Clear();

            isPlaying = false;
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

        public string GetSheetData(int row, int col)
        {
            //시트 데이터 받기
            return _generalSheetData.Item1[col].GetRowData(row);
        }
        public string GetSheetData(string head, int index, bool IsRow = false)
        {
            if (IsRow)
                return generalSheet.GetRow(head)[index];

            //시트 데이터 받기
            return generalSheet.GetCol(head)[index];
        }

        public List<string> GetSheetDatas(string head)
        {
            int index = generalSheet.Heads.IndexOf(head);
            if (index != -1)
            {
                return generalSheet.GetRow(head).ToList<string>();
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 시트 데이터 저장 및 불러오기용 인스턴스. 싱글톤
    /// </summary>
    public class SheetDataDrawer
    {
        private static SheetDataDrawer instance;
        public static SheetDataDrawer Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new SheetDataDrawer();
                }
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        SheetModel sheetmodel;
        GeneralSheet generalSheet;
        VersionSheet versionSheet;

        List<GeneralSheet.Layout> generalsheetData;
        List<VersionSheet.Layout> versionsheetData;
        public bool isInit = false;
        private int _seq_of_ChartProcess = -1;
        public Point RepeatPoint { get; set; }
        protected SheetDataDrawer()
        {

        }
        
        public void Initialize(SheetModel sheetModel)
        {
            this.sheetmodel = sheetModel;
            generalSheet = sheetModel.GetGeneralSheet();
            generalsheetData = generalSheet.Layouts;
            versionSheet = sheetModel.GetVerSheet();
            versionsheetData = versionSheet.Data;
            isInit = false;
        }
    
        public List<GeneralSheet.Layout> GetGeneralSheetLayouts()
        {
            if (generalsheetData == null)
                return null;

            return generalsheetData;
        }

        public Dictionary<GeneralSheet.LayoutTypes, Dictionary<string, List<GeneralSheet.Layout>>> GetGeneralSheetDictionary()
        {
            return generalSheet.LayoutDic;
        }

        public List<Dictionary<GeneralSheet.LayoutTypes,string>> GetGeneralSheetListWithDic()
        {
            return generalSheet.LayoutListWithDic;
        }
        public List<VersionSheet.Layout> GetVersionSheetLayouts()
        {
            if (versionsheetData == null)
                return null;

            return versionsheetData;
        }

        public GeneralSheet.Layout GetGeneralSheetLayout(int index)
        {
            if (index > generalsheetData.Count() || index < 0)
                return new GeneralSheet.Layout();

            return generalsheetData[index];
        }

        public VersionSheet.Layout GetVersionSheetLayout(int index)
        {
            if (index > versionsheetData.Count() || index < 0)
                return new VersionSheet.Layout();

            return versionsheetData[index];
        }

        public string GetGeneralSheetData()
        {
            if (RepeatPoint == null)
                RepeatPoint = new Point(0,0);
            return GetGeneralSheetData((int)RepeatPoint.Y, (int)RepeatPoint.X,true);
        }

        public string GetGeneralSheetData(int row, int column, bool isSeq = false)
        {
            string result = "";
            if (isSeq)
            {
                if (_seq_of_ChartProcess == -1)
                    _seq_of_ChartProcess = 0;

                if (_seq_of_ChartProcess >= generalSheet.MaxColumn)
                {
                    _seq_of_ChartProcess = -1;
                    return "Error SheetData Is Overloaded";
                }

                result = generalSheet.LayoutListWithDic[column + _seq_of_ChartProcess][(GeneralSheet.LayoutTypes)column];
                _seq_of_ChartProcess++;
            }
            else
                result = generalSheet.LayoutListWithDic[column][(GeneralSheet.LayoutTypes)column];

            if(result == null)
                return "Error SheetData Is Empty";

            return result;
        }
    }

}
