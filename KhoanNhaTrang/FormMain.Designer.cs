
namespace KhoanNhaTrang
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.CalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingtimeupdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GroutingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.waterPressureTestReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timermain = new System.Windows.Forms.Timer(this.components);
            this.lbAlarmPLC = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem3,
            this.toolToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 29);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Checked = true;
            this.toolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.toolStripMenuItem1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(46, 25);
            this.toolStripMenuItem1.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CalibrationToolStripMenuItem,
            this.SettingtimeupdateToolStripMenuItem});
            this.toolStripMenuItem3.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(71, 25);
            this.toolStripMenuItem3.Text = "Setting";
            // 
            // CalibrationToolStripMenuItem
            // 
            this.CalibrationToolStripMenuItem.Name = "CalibrationToolStripMenuItem";
            this.CalibrationToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.CalibrationToolStripMenuItem.Text = "Calibration";
            this.CalibrationToolStripMenuItem.Click += new System.EventHandler(this.CalibrationToolStripMenuItem_Click);
            // 
            // SettingtimeupdateToolStripMenuItem
            // 
            this.SettingtimeupdateToolStripMenuItem.Name = "SettingtimeupdateToolStripMenuItem";
            this.SettingtimeupdateToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.SettingtimeupdateToolStripMenuItem.Text = "Setting time update";
            this.SettingtimeupdateToolStripMenuItem.Click += new System.EventHandler(this.SettingtimeupdateToolStripMenuItem_Click);
            // 
            // toolToolStripMenuItem
            // 
            this.toolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GroutingToolStripMenuItem,
            this.waterPressureTestReportToolStripMenuItem});
            this.toolToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            this.toolToolStripMenuItem.Size = new System.Drawing.Size(116, 25);
            this.toolToolStripMenuItem.Text = "Pump control";
            // 
            // GroutingToolStripMenuItem
            // 
            this.GroutingToolStripMenuItem.Name = "GroutingToolStripMenuItem";
            this.GroutingToolStripMenuItem.Size = new System.Drawing.Size(247, 26);
            this.GroutingToolStripMenuItem.Text = "EM Grouting Equipment";
            this.GroutingToolStripMenuItem.Click += new System.EventHandler(this.GroutingToolStripMenuItem_Click);
            // 
            // waterPressureTestReportToolStripMenuItem
            // 
            this.waterPressureTestReportToolStripMenuItem.Name = "waterPressureTestReportToolStripMenuItem";
            this.waterPressureTestReportToolStripMenuItem.Size = new System.Drawing.Size(247, 26);
            this.waterPressureTestReportToolStripMenuItem.Text = "Water Pressure Test";
            this.waterPressureTestReportToolStripMenuItem.Click += new System.EventHandler(this.waterPressureTestReportToolStripMenuItem_Click);
            // 
            // timermain
            // 
            this.timermain.Tick += new System.EventHandler(this.timermain_Tick);
            // 
            // lbAlarmPLC
            // 
            this.lbAlarmPLC.BackColor = System.Drawing.Color.Red;
            this.lbAlarmPLC.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAlarmPLC.ForeColor = System.Drawing.Color.White;
            this.lbAlarmPLC.Location = new System.Drawing.Point(289, 0);
            this.lbAlarmPLC.Name = "lbAlarmPLC";
            this.lbAlarmPLC.Size = new System.Drawing.Size(270, 29);
            this.lbAlarmPLC.TabIndex = 6;
            this.lbAlarmPLC.Text = "PLC stopping";
            this.lbAlarmPLC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lbAlarmPLC);
            this.Controls.Add(this.menuStrip1);
            this.Name = "FormMain";
            this.Text = "FormMain";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GroutingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CalibrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingtimeupdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem waterPressureTestReportToolStripMenuItem;
        private System.Windows.Forms.Timer timermain;
        private System.Windows.Forms.Label lbAlarmPLC;
    }
}