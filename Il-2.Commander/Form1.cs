using Il_2.Commander.Commander;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Il_2.Commander
{
    public partial class Form1 : Form
    {
        public static bool busy;
        private HubMessenger messenger;
        private CommanderCL commander;
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
            timerOfficer.Start();
        }
        private void btn_StartGen_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
            btn_StartPredGen.Enabled = false;
            Action startgen = () =>
            {
                commander.StartGeneration();
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = true;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
            btn_StartPredGen.Enabled = false;
            timerRcon.Start();
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
            Action startgen = () =>
            {
                commander.StartGeneration("pregen");
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
        }
        private void timerOfficer_Tick(object sender, EventArgs e)
        {
            timerOfficer.Stop();
            Action startgen = () =>
            {
                commander.StartGeneration();
            };
            Task taskstartgen = Task.Factory.StartNew(startgen);
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
            if(busy)
            {
                if(CommanderCL.qLog.Count > 0)
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
    }
}