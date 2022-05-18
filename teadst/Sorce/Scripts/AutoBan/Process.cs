using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SheetViewer;

namespace MailMaker.Scripts.AutoBan
{
    public class ClickProcess : IProcess
    {
        public int PID { get; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public Action ProcessStart { get; set; }
        public Action ProcessProgress { get; set; }
        public Action ProcessDone { get; set; }

        public ClickProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }

        public void Process()
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            Done();
        }
        public void Done()
        {
            ProcessDone?.Invoke();
            NextProcess.Start();
        }
    }
    public class CopyProcess : IProcess
    {
        public int PID { get; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public System.Action ProcessStart { get; set; }
        public System.Action ProcessProgress { get; set; }
        public System.Action ProcessDone { get; set; }

        public CopyProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }

        public void Process()
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            SendKeys.SendWait("^c");
            ProcessDone?.Invoke();
        }

        public void Done()
        {
            NextProcess.Start();
        }
    }
    public class PasteProcess : IProcess
    {
        public int PID { get; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public System.Action ProcessStart { get; set; }
        public System.Action ProcessProgress { get; set; }
        public System.Action ProcessDone { get; set; }
        public PasteProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }

        public void Process()
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            SendKeys.SendWait("^v");
            ProcessDone?.Invoke();
        }

        public void Done()
        {
            NextProcess.Start();
        }
    }
    public class InputProcess : IProcess
    {
        public int PID { get; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public System.Action ProcessStart { get; set; }
        public System.Action ProcessProgress { get; set; }
        public System.Action ProcessDone { get; set; }

        public InputProcess(int pid, string value, Position mousePosition, ProcessType processType, AutoBanSheetPresenter presenter)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }

        public void Process()
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            SendKeys.SendWait(Value);
            ProcessDone?.Invoke();
        }

        public void Done()
        {
            NextProcess.Start();
        }

        private string GetValue(ProcessType type)
        {
            string str = Value;

            switch (type)
            {
                case ProcessType.InputValue: return str;
                case ProcessType.InputFromChart:
                    {

                    }
                    break;
                case ProcessType.InputFromChartOne:
                    {

                    }break;
            }


            return str;
        }



    }
    public class PauseProcess : IProcess
    {
        public int PID { get; }
        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public System.Action ProcessStart { get; set; }
        public System.Action ProcessProgress { get; set; }
        public System.Action ProcessDone { get; set; }

        public PauseProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }

        public async void Process()
        {
            if (int.TryParse(Value, out int result))
                await Task.Delay(result);
            else
                System.Windows.MessageBox.Show("INVAILED INPUT!!!!!!");

            ProcessDone?.Invoke();
        }

        public void Done()
        {
            ProcessDone?.Invoke();
        }
    }
    public class SelectProcess : IProcess
    {
        public int PID { get; set; }

        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public Action ProcessStart { get; set; }
        public Action ProcessProgress { get; set; }
        public Action ProcessDone { get; set; }

        public SelectProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }
        public void Process()
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            MouseKeyboardController.LeftClick();
            SendKeys.SendWait("^a");
            ProcessDone?.Invoke();
        }

        public void Done()
        {
            ProcessDone?.Invoke();
        }
    }
    public class DeleteProcess : IProcess
    {
        public int PID { get; set; }

        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public Action ProcessStart { get; set; }
        public Action ProcessProgress { get; set; }
        public Action ProcessDone { get; set; }

        public DeleteProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }
        public void Process()
        {
            MouseKeyboardController.MoveAndLeftClick(MousePosition);
            MouseKeyboardController.LeftClick();
            SendKeys.SendWait("^a");
            SendKeys.SendWait("{DELETE}");
            ProcessDone?.Invoke();
        }

        public void Done()
        {
            ProcessDone?.Invoke();
        }
    }


    public class LastProcess : IProcess
    {
        public int PID { get; set; }

        public IProcess NextProcess { get; set; }
        public string Value { get; set; }
        public Position MousePosition { get; set; }
        public ProcessType CurProcessType { get; set; }
        public Action ProcessStart { get; set; }
        public System.Action ProcessProgress { get; set; }
        public Action ProcessDone { get; set; }
        public LastProcess(int pid, string value, Position mousePosition, ProcessType processType)
        {
            PID = pid;
            this.Value = value;
            CurProcessType = processType;
            MousePosition = mousePosition;
        }
        public void Start()
        {
            ProcessStart?.Invoke();
            Process();
        }
        public void Process()
        {
            ProcessDone?.Invoke();
        }
        public void Done()
        {

        }
    }
}
