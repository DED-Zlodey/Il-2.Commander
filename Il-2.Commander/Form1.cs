using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            btn_Stop.Enabled = false;
            busy = true;          
        }

        private void btn_StartGen_Click(object sender, EventArgs e)
        {

        }

        private void btn_Start_Click(object sender, EventArgs e)
        {

        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {

        }
    }
}
