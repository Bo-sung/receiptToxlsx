using System;
using System.Collections.Generic;
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
using SheetViewer;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace MailMaker.Scripts.AutoBan
{
    /// <summary>
    /// 프로세스 띄워줄 블록
    /// </summary>
    public partial class ProcessBlock : Border, IDisposable
    {
        int PID;
        IProcess process;
        public Border border;
        public StackPanel panel;
        public TextBlock processText;
        public TextBlock positionText;
        public System.Windows.Controls.TextBox valueInput1;
        public System.Windows.Controls.TextBox valueInput2;
        public TextBlock PIDBlock;
        public TextBlock ETCBlock;

        public enum BackgroundColor
        {
            Black = 0,
            White = 1,
            Gray = 2,
            Red = 3,
            Green = 3,
            Blue = 3,
        };

        public enum State
        {
            Processing,
            Waiting,
            Destroying
        };

        public State CurrentState { get; set; }

        AutoBanSheetPresenter presenter;

        private SolidColorBrush BLACK = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private SolidColorBrush WHITE = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private SolidColorBrush GRAY = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        private SolidColorBrush RED = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private SolidColorBrush GREEN = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        private SolidColorBrush BLUE = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public ProcessBlock(IProcess process, AutoBanSheetPresenter presenter) : base()
        {
            this.process = process;
            process.ProcessDone += DestroyThis;
            Init();
            this.presenter = presenter;
            PID = process.PID;
        }
        private void Init()
        {
            border = new Border
            {
                Margin = new Thickness(3, 3, 3, 3),
                Background = GRAY
            };

            panel = new StackPanel();

            processText = new TextBlock
            {
                Text = GetProcessName(),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };
            panel.Children.Add(processText);

            positionText = new TextBlock
            {
                Text = $"({process.MousePosition.X},{process.MousePosition.Y})",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            };
            panel.Children.Add(positionText);

            if (process.CurProcessType == ProcessType.InputFromChartOne)
            {
                Grid grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), });

                valueInput1 = new System.Windows.Controls.TextBox
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                };
                valueInput2 = new System.Windows.Controls.TextBox
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                };

                valueInput1.Text = "Column";
                valueInput2.Text = "Row";

                valueInput1.TextChanged += Value2Changed;
                valueInput2.TextChanged += Value2Changed;

                valueInput1.GotFocus += RemoveDescript;
                valueInput2.GotFocus += RemoveDescript;

                Grid.SetColumn(valueInput1, 0);
                Grid.SetColumn(valueInput2, 1);

                grid.Children.Add(valueInput1);
                grid.Children.Add(valueInput2);
                panel.Children.Add(grid);
            }
            else
            {
                valueInput1 = new System.Windows.Controls.TextBox
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                };
                valueInput1.Text = process.Value;
                valueInput1.TextChanged += Value1Changed;
                valueInput1.GotFocus += RemoveDescript;
                panel.Children.Add(valueInput1);
            }


            PIDBlock = new TextBlock
            {
                Text = process.PID.ToString(),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };
            panel.Children.Add(PIDBlock);

            ETCBlock = new TextBlock
            {
                Text = "",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };
            panel.Children.Add(ETCBlock);


            border.Child = panel;

            CurrentState = State.Waiting;
            process.ProcessDone += DestroyThis;
        }

        ~ProcessBlock()
        {
            valueInput1.TextChanged -= Value1Changed;
            process.ProcessDone -= DestroyThis;
        }

        void DestroyThis()
        {
            presenter.RemoveFromPanel(border.Uid);
            //Dispose();
        }

        public void ChangeColor(State state)
        {
            switch (state)
            {
                case State.Processing:
                    {
                        border.Background = RED;
                        if(process.CurProcessType == ProcessType.InputFromChart)
                            GetValueFromChart();
                    }
                    break;
                case State.Waiting:
                    {
                        border.Background = GRAY;
                    }
                    break;
                case State.Destroying:
                    {
                        border.Background = BLACK;
                    }
                    break;
            }
        }

        public Border GetBorder()
        {
            return border;
        }

        public IProcess GetProcess()
        {
            return process;
        }


        public void Value1Changed(object sender, TextChangedEventArgs e)
        {
            process.Value = valueInput1.Text;
        }

        public void Value2Changed(object sender, TextChangedEventArgs e)
        {
            if(process.CurProcessType == ProcessType.InputFromChartOne)
            {
                process.Value = valueInput1.Text + InputProcess.CHART_DATA_DEIVDE_FORMAT + valueInput2.Text;
            }
        }

        public void RemoveDescript(object sender, EventArgs e)
        {
            if (typeof(System.Windows.Controls.TextBox) == sender.GetType())
            {
                System.Windows.Controls.TextBox dump = (System.Windows.Controls.TextBox)sender;
                if (int.TryParse(dump.Text, out int data))
                    return;
                dump.Text = "";
            }
        }


        public void GetValueFromChart()
        {
            int nextIndex = presenter.GetNextIndex();
            ETCBlock.Text = $"Target Value = {process.Value}";

            string str = process.Value;

            string[] val = process.Value.Split(InputProcess.CHART_DATA_DEIVDE_FORMAT);
            if (!int.TryParse(val[0], out int column))
                str = "Error position Is Invald";
            if (!int.TryParse(val[1], out int row))
                str = "Error index Is Invald";

            switch (process.CurProcessType)
            {
                case ProcessType.InputFromChart:
                    {
                        str = SheetDataDrawer.Instance.GetGeneralSheetData(column, row, true);
                    }
                    break;
                case ProcessType.InputFromChartOne:
                    {
                        str = SheetDataDrawer.Instance.GetGeneralSheetData(column, row);
                    }
                    break;
                default:
                    return;
            }
            process.Value = str;
        }

        private string GetProcessName()
        {
            switch (process.CurProcessType)
            {
                case ProcessType.Click:
                    return "클릭";
                case ProcessType.Copy:
                    return "복사";
                case ProcessType.Paste:
                    return "붙혀넣기";
                case ProcessType.InputValue:
                    return "입력{Value}";
                case ProcessType.InputFromChart:
                    return "입력 (차트에서 하나씩 아래로) {Range}";
                case ProcessType.InputFromChartOne:
                    return "입력 (차트의 특정 값) {Range}";
                case ProcessType.SelectAll:
                    return "전체 선택";
                case ProcessType.Delete:
                    return "삭제";
                default:
                    return "대기";
            }

        }
        public void Dispose()
        {
            valueInput1.TextChanged -= Value1Changed;
            process.ProcessDone -= DestroyThis;
            GC.SuppressFinalize(this);
        }
    }
}
