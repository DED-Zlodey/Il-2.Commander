using Il_2.Commander.Commander;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Il_2.Commander
{
    public partial class Form1 : Form
    {
        public static bool busy;
        private HubMessenger messenger;
        private CommanderCL commander;
        private Process processGenerator;
        public Form1()
        {
            InitializeComponent();
            lvLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            SetApp.SetUp();
            btn_Stop.Enabled = false;
            busy = true;
            messenger = new HubMessenger();
            Action hubstart = () =>
            {
                messenger.Start();
            };
            Task taskhubstart = Task.Factory.StartNew(hubstart);
            commander = new CommanderCL();
            commander.GetLogStr += Commander_GetLogStr;
            commander.GetOfficerTime += Commander_GetOfficerTime;
            commander.GetLogArray += Commander_GetLogArray;
        }

        private void Commander_GetLogArray(string[] array)
        {
            var counter = lvLog.Items.Count;
            foreach (var item in array)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = item;
                if ((counter % 2) == 0)
                {
                    lvi.BackColor = Color.WhiteSmoke;
                }
                else
                {
                    lvi.BackColor = Color.LightGray;
                }
                BeginInvoke((MethodInvoker)(() => lvLog.Items.Add(lvi)));
                counter++;
            }
            BeginInvoke((MethodInvoker)(() => lvLog.Items[lvLog.Items.Count - 1].EnsureVisible()));
        }

        private void Commander_GetLogStr(string Message, Color color)
        {
            var counter = lvLog.Items.Count;
            ListViewItem lvi = new ListViewItem();
            lvi.Text = Message;
            lvi.ForeColor = color;
            if ((counter % 2) == 0)
            {
                lvi.BackColor = Color.WhiteSmoke;
            }
            else
            {
                lvi.BackColor = Color.LightGray;
            }
            BeginInvoke((MethodInvoker)(() => lvLog.Items.Add(lvi)));
            BeginInvoke((MethodInvoker)(() => lvLog.Items[lvLog.Items.Count - 1].EnsureVisible()));
        }

        /// <summary>
        /// Старт таймера для разработки плана наступления в следующей миссии
        /// </summary>
        /// <param name="Message"></param>
        private void Commander_GetOfficerTime(string Message, Color color)
        {
            BeginInvoke((MethodInvoker)(() => timerOfficer.Start()));
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
            timerRcon.Start();
            timerLog.Start();
            Action startgen = () =>
            {
                commander.Start();
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = true;
            btn_StartGen.Enabled = true;
            btn_StartPredGen.Enabled = true;
            Action startgen = () =>
            {
                commander.Stop();
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
        }

        private void btn_StartPredGen_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
            btn_StartPredGen.Enabled = false;
            StartGeneration("pregen");
        }

        private void timerOfficer_Tick(object sender, EventArgs e)
        {
            Action startgen = () =>
            {
                commander.StartGeneration();
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
            BeginInvoke((MethodInvoker)(() => timerOfficer.Stop()));
        }

        private void timerRcon_Tick(object sender, EventArgs e)
        {
            Action startgen = () =>
            {
                commander.SendRconCommand();
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
        }

        private void timerLog_Tick(object sender, EventArgs e)
        {
            if (busy)
            {
                if (CommanderCL.qLog.Count > 0)
                {
                    busy = false;
                    var str = CommanderCL.qLog.Dequeue();
                    Action startgen = () =>
                    {
                        commander.HandleLogFile(str);
                    };
                    Task taskstartgen = Task.Factory.StartNew(startgen);
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            lvLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        /// <summary>
        /// Запускает предварительную генерацию миссии.
        /// </summary>
        /// <param name="predGen">pregen</param>
        public void StartGeneration(string predGen)
        {
            var counter = lvLog.Items.Count;
            ListViewItem lvi = new ListViewItem();
            lvi.Text = "Start PredGen...";
            lvi.ForeColor = Color.DarkRed;
            if ((counter % 2) == 0)
            {
                lvi.BackColor = Color.WhiteSmoke;
            }
            else
            {
                lvi.BackColor = Color.LightGray;
            }
            BeginInvoke((MethodInvoker)(() => lvLog.Items.Add(lvi)));
            BeginInvoke((MethodInvoker)(() => lvLog.Items[lvLog.Items.Count - 1].EnsureVisible()));
            processGenerator = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(SetApp.Config.Generator, predGen);
            processStartInfo.WorkingDirectory = SetApp.Config.GeneratorWorkingDirectory;
            processStartInfo.RedirectStandardOutput = true; //Выводить в родительское окно
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true; // не создавать окно CMD
            processStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            processGenerator.StartInfo = processStartInfo;
            processGenerator.EnableRaisingEvents = true;
            processGenerator.Exited += new EventHandler(PredGen_complete);
            processGenerator.Start();
        }
        /// <summary>
        /// Вызывается после завершения предварительной генерации миссии.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PredGen_complete(object sender, EventArgs e)
        {
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            Commander_GetLogArray(content);
            BeginInvoke((MethodInvoker)(() => btn_Stop.Enabled = false));
            BeginInvoke((MethodInvoker)(() => btn_Start.Enabled = true));
            BeginInvoke((MethodInvoker)(() => btn_StartGen.Enabled = true));
            BeginInvoke((MethodInvoker)(() => btn_StartPredGen.Enabled = true));
        }
        /// <summary>
        /// Старт генератора. Запускает полную генерацию миссии
        /// </summary>
        public void StartGeneration()
        {
            var counter = lvLog.Items.Count;
            ListViewItem lvi = new ListViewItem();
            lvi.Text = "Start Generation...";
            lvi.ForeColor = Color.DarkRed;
            if ((counter % 2) == 0)
            {
                lvi.BackColor = Color.WhiteSmoke;
            }
            else
            {
                lvi.BackColor = Color.LightGray;
            }
            BeginInvoke((MethodInvoker)(() => lvLog.Items.Add(lvi)));
            BeginInvoke((MethodInvoker)(() => lvLog.Items[lvLog.Items.Count - 1].EnsureVisible()));
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
            bool Loading = false;
            bool PrepareLoc = false;
            bool SavingLoc = false;
            bool SavingList = false;
            bool SavingBin = false;
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            foreach (var item in content)
            {
                if (item.Contains("Loading ") && item.Contains(" DONE"))
                {
                    Loading = true;
                }
                if (item.Contains("Prepare localisation data DONE"))
                {
                    PrepareLoc = true;
                }
                if (item.Contains("Saving localisation data DONE"))
                {
                    SavingLoc = true;
                }
                if (item.Contains("Saving binary data DONE"))
                {
                    SavingBin = true;
                }
                if (item.Contains("Saving .list DONE"))
                {
                    SavingList = true;
                }
            }
            if (Loading && PrepareLoc && SavingBin && SavingList && SavingLoc)
            {
                Commander_GetLogArray(content);
                BeginInvoke((MethodInvoker)(() => btn_Stop.Enabled = false));
                BeginInvoke((MethodInvoker)(() => btn_Start.Enabled = true));
                BeginInvoke((MethodInvoker)(() => btn_StartGen.Enabled = true));
                BeginInvoke((MethodInvoker)(() => btn_StartPredGen.Enabled = true));
            }
            else
            {
                Commander_GetLogStr("Generation error. Restart generation. Please wait...", Color.Red);
                StartGeneration();
            }
        }
    }
}