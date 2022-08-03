using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using KhoanNhaTrang.Model;
using MySqlConnector;
//using System;
//using System.Collections.Generic;
using System.Configuration;
//using System.Data;
//using System.Linq;
//using System.Windows.Forms;
using ZedGraph;
//using System.Drawing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using System.Drawing.Printing;
using Spire.Xls;
namespace KhoanNhaTrang
{
    public partial class FormMain : Form
    {
        Boolean debugMode = false;
        public FormMain()
        {
            InitializeComponent();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            timermain.Start();
            //timermain.Enabled = true;
        }


        private void SettingtimeupdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frTimeStore fr = new frTimeStore();
            fr.Show();
        }

        private void GroutingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Home fr = new Form_Home();
            fr.Show();
        }

        private void waterPressureTestReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main2 fr = new Main2();
            fr.Show();
        }

        private void timermain_Tick(object sender, EventArgs e)
        {
            if (!debugMode)
            {
                try
                {
                    if (PLC.Instance().Open())
                    {
                        //show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().WC_start);
                        lbAlarmPLC.Text = "PLC Connected";
                        lbAlarmPLC.BackColor = Color.Lime;
                    }
                    else
                    {
                        lbAlarmPLC.Text = "PLC not connected";
                        lbAlarmPLC.BackColor = Color.Red;
                    }
                    PLC.Instance().ReadClass(PLCDB1Read.Instance(), 1);
                    PLC.Instance().ReadClass(PLCDB2Write.Instance(), 2);
                    PLC.Instance().ReadClass(PLCDB3READ.Instance(), 3);
                    //show_Data_Real_lb(txtflowrate, Math.Round(PLCDB1Read.Instance().flow_rate, 2));
                    //show_Data_Real_lb(txttotalflow, Math.Round(PLCDB1Read.Instance().fluid, 2));
                    //show_Data_Real_lb(txtdensi, PLCDB1Read.Instance().wc);
                    //show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().wc_1);
                    //show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().wc_1);
                    //show_Data_Real_lb(txtpressure, Math.Round(PLCDB1Read.Instance().pressure, 2));
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void CalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frParameter fr = new frParameter();
            fr.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
