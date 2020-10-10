using Il_2.Commander.Commander;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Il_2.Commander
{
    public partial class Form1 : Form
    {
        public static bool busy;
        private Process processGenerator;
        private HubMessenger messenger;
        private CommanderCL commander;
        public Form1()
        {
            InitializeComponent();
            SetApp.SetUp();
            btn_Stop.Enabled = false;
            busy = true;
            //lbLog.DrawMode = DrawMode.OwnerDrawFixed;
            //lbLog.DrawItem += lbLog_DrawItem;
            messenger = new HubMessenger();
            Action hubstart = () =>
            {
                messenger.Start();
            };
            Task taskhubstart = Task.Factory.StartNew(hubstart);
            commander = new CommanderCL();
        }

        private void lbLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index > -1)
            {
                e.DrawBackground();
                Graphics g = e.Graphics;
                g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
                ListBox lb = (ListBox)sender;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    e.Graphics.FillRectangle(Brushes.DarkRed, e.Bounds);
                    e.Graphics.DrawString(lb.Items[e.Index].ToString(), e.Font, Brushes.White, e.Bounds);
                }
                else
                {
                    if (e.Index % 2 == 1)
                    {
                        e.Graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                    }
                    e.Graphics.DrawString(lb.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds);
                }
                e.DrawFocusRectangle();
            }
        }

        private void btn_StartGen_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
            btn_StartPredGen.Enabled = false;
            StartGeneration();
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = true;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
            btn_StartPredGen.Enabled = false;
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = true;
            btn_StartGen.Enabled = true;
            btn_StartPredGen.Enabled = true;
        }

        private void btn_StartPredGen_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
            btn_StartPredGen.Enabled = false;
        }
        /// <summary>
        /// Старт генератора миссии
        /// </summary>
        private void StartGeneration()
        {
            lbLog.Items.Add("Start Generation...");
            lbLog.SetSelected(lbLog.Items.Count - 1, true);
            processGenerator = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(SetApp.Config.Generator);
            processStartInfo.WorkingDirectory = SetApp.Config.GeneratorWorkingDirectory;
            processStartInfo.RedirectStandardOutput = true; //Выводить в родительское окно
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true; // не создавать окно CMD
            processStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            processGenerator.StartInfo = processStartInfo;
            processGenerator.EnableRaisingEvents = true;
            processGenerator.Exited += new EventHandler(Generation_complete);
            processGenerator.Start();
        }
        /// <summary>
        /// Вызывается после завершения генерации миссии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generation_complete(object sender, EventArgs e)
        {
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            foreach (var item in content)
            {
                BeginInvoke((MethodInvoker)(() => lbLog.Items.Add(item)));
                BeginInvoke((MethodInvoker)(() => lbLog.SetSelected(lbLog.Items.Count - 1, true)));
            }
            BeginInvoke((MethodInvoker)(() => btn_Start.Enabled = true));
            BeginInvoke((MethodInvoker)(() => btn_StartGen.Enabled = true));
            BeginInvoke((MethodInvoker)(() => btn_StartPredGen.Enabled = true));
        }
    }
}