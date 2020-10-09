using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Il_2.Commander
{
    public partial class CommEx : Form
    {
        public static bool busy;
        private Process processGenerator;
        public CommEx()
        {
            InitializeComponent();
            SetApp.SetUp();
            btn_Stop.Enabled = false;
            busy = true;
        }

        private void btn_StartGen_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = true;
            btn_Start.Enabled = false;
            btn_StartGen.Enabled = false;
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            btn_Stop.Enabled = false;
            btn_Start.Enabled = true;
            btn_StartGen.Enabled = true;
        }
    }
}
