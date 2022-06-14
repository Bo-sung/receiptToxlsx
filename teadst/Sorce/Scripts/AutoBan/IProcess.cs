using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailMaker.Scripts.AutoBan
{
    public enum ProcessType
    {
        Pause = 0,
        Click = 1,
        Copy = 2,
        Paste = 3,
        InputValue = 4,
        InputFromChart = 5,
        InputFromChartOne = 6,
        SelectAll = 7,
        Delete = 8,
        Last = 999,
    }
    public interface IProcess
    {
        int PID { get; }
        IProcess NextProcess { get; set; }
        string Value { get; set; }
        Position MousePosition { get; set; }
        ProcessType CurProcessType { get; set; }

        System.Action ProcessStart { get; set; }
        System.Action ProcessProgress { get; set; }
        System.Action ProcessDone { get; set; }

        /// <summary>
        /// 프로세스 시작
        /// </summary>
        void Start();
        /// <summary>
        /// 프로세스. 0.1초당 한번씩 호출됨
        /// </summary>
        void Process();
        /// <summary>
        /// 프로세스 종료
        /// </summary>
        void Done();
    }
}
