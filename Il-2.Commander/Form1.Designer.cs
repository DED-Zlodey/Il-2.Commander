namespace Il_2.Commander
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.TabBG = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label_status = new System.Windows.Forms.Label();
            this.lvLog = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.btn_StartPredGen = new System.Windows.Forms.Button();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.btn_Start = new System.Windows.Forms.Button();
            this.btn_StartGen = new System.Windows.Forms.Button();
            this.timerOfficer = new System.Windows.Forms.Timer(this.components);
            this.timerRcon = new System.Windows.Forms.Timer(this.components);
            this.TabBG.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabBG
            // 
            this.TabBG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabBG.Controls.Add(this.tabPage1);
            this.TabBG.Location = new System.Drawing.Point(12, 12);
            this.TabBG.Name = "TabBG";
            this.TabBG.SelectedIndex = 0;
            this.TabBG.Size = new System.Drawing.Size(776, 639);
            this.TabBG.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label_status);
            this.tabPage1.Controls.Add(this.lvLog);
            this.tabPage1.Controls.Add(this.btn_StartPredGen);
            this.tabPage1.Controls.Add(this.btn_Stop);
            this.tabPage1.Controls.Add(this.btn_Start);
            this.tabPage1.Controls.Add(this.btn_StartGen);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(768, 611);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Управление";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label_status
            // 
            this.label_status.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(4, 567);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(64, 15);
            this.label_status.TabIndex = 2;
            this.label_status.Text = "Status True";
            // 
            // lvLog
            // 
            this.lvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvLog.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lvLog.FullRowSelect = true;
            this.lvLog.HideSelection = false;
            this.lvLog.Location = new System.Drawing.Point(3, 3);
            this.lvLog.Name = "lvLog";
            this.lvLog.Size = new System.Drawing.Size(762, 557);
            this.lvLog.TabIndex = 1;
            this.lvLog.UseCompatibleStateImageBehavior = false;
            this.lvLog.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Log";
            this.columnHeader1.Width = 700;
            // 
            // btn_StartPredGen
            // 
            this.btn_StartPredGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_StartPredGen.Location = new System.Drawing.Point(291, 566);
            this.btn_StartPredGen.Name = "btn_StartPredGen";
            this.btn_StartPredGen.Size = new System.Drawing.Size(114, 31);
            this.btn_StartPredGen.TabIndex = 0;
            this.btn_StartPredGen.Text = "Start PredGen";
            this.btn_StartPredGen.UseVisualStyleBackColor = true;
            this.btn_StartPredGen.Click += new System.EventHandler(this.btn_StartPredGen_Click);
            // 
            // btn_Stop
            // 
            this.btn_Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Stop.Location = new System.Drawing.Point(651, 566);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(114, 31);
            this.btn_Stop.TabIndex = 0;
            this.btn_Stop.Text = "Stop";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // btn_Start
            // 
            this.btn_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Start.Location = new System.Drawing.Point(531, 566);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(114, 31);
            this.btn_Start.TabIndex = 0;
            this.btn_Start.Text = "Start";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // btn_StartGen
            // 
            this.btn_StartGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_StartGen.Location = new System.Drawing.Point(411, 566);
            this.btn_StartGen.Name = "btn_StartGen";
            this.btn_StartGen.Size = new System.Drawing.Size(114, 31);
            this.btn_StartGen.TabIndex = 0;
            this.btn_StartGen.Text = "Start Generator";
            this.btn_StartGen.UseVisualStyleBackColor = true;
            this.btn_StartGen.Click += new System.EventHandler(this.btn_StartGen_Click);
            // 
            // timerOfficer
            // 
            this.timerOfficer.Interval = 180000;
            this.timerOfficer.Tick += new System.EventHandler(this.timerOfficer_Tick);
            // 
            // timerRcon
            // 
            this.timerRcon.Interval = 700;
            this.timerRcon.Tick += new System.EventHandler(this.timerRcon_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 663);
            this.Controls.Add(this.TabBG);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Commander Expert";
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.TabBG.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl TabBG;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Button btn_StartGen;
        private System.Windows.Forms.Button btn_StartPredGen;
        private System.Windows.Forms.Timer timerOfficer;
        private System.Windows.Forms.ListView lvLog;
        private System.Windows.Forms.Timer timerRcon;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label_status;
    }
}

