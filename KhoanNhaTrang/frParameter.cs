using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KhoanNhaTrang
{
    public partial class frParameter : Form
    {
        public frParameter()
        {
            InitializeComponent();
        }

        private void btCancle1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            //Properties.Settings.Default.IPPLC = txtIPPLC.Text;
            //Properties.Settings.Default.SQL = txtSQL.Text;
            //Properties.Settings.Default.Excel = txtExcel.Text;
            //Properties.Settings.Default.Update = int.Parse(txtUpdate.Text);
            //MessageBox.Show("Save complete !, please restart Application", "information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Properties.Settings.Default.Save();
            //this.Close();
        }
        #region
        private void frParameter_Load(object sender, EventArgs e)
        {
            if (PLC.Instance().PLC_connected)
            {
                timerpara.Interval = 100;
                timerpara.Enabled = true;
                //AI1
                //txtscalemax1.Text = Write_parastatic.Scale_value_high_1.ToString("0.00");
                show_Data_Dint_txt(txtanalogmin1, PLCDB2Write.Instance().Raw_low_1);
                show_Data_Dint_txt(txtanalogmax1, PLCDB2Write.Instance().Raw_high_1);
                show_Data_Dint_txt(txtscalemin1, PLCDB2Write.Instance().Scale_value_low_1);
                show_Data_Dint_txt(txtscalemax1, PLCDB2Write.Instance().Scale_value_high_1);
                //AI2
                show_Data_Dint_txt(txtanalogmin2, PLCDB2Write.Instance().Raw_low_2);
                show_Data_Dint_txt(txtanalogmax2, PLCDB2Write.Instance().Raw_high_2);
                show_Data_Dint_txt(txtscalemin2, PLCDB2Write.Instance().Scale_value_low_2);
                show_Data_Dint_txt(txtscalemax2, PLCDB2Write.Instance().Scale_value_high_2);
                //AI3
                show_Data_Dint_txt(txtanalogmin3, PLCDB2Write.Instance().Raw_low_3);
                show_Data_Dint_txt(txtanalogmax3, PLCDB2Write.Instance().Raw_high_3);
                show_Data_Dint_txt(txtscalemin3, PLCDB2Write.Instance().Scale_value_low_3);
                show_Data_Dint_txt(txtscalemax3, PLCDB2Write.Instance().Scale_value_high_3);
            }
        }
        #endregion
        private void frParameter_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerpara.Enabled = false;
        }
      
 
   
        #region
        private void timerpara_Tick(object sender, EventArgs e)
        {
            //PLC.Instance().ReadClass(PLCDB1Read.Instance(), 1);
            //AI1
            show_Data_Int_lb(lbraw1, PLCDB1Read.Instance().RawAI1);
            show_Data_Real_lb(lbrealvalue1, PLCDB1Read.Instance().flow_rate);
            //AI2
            show_Data_Int_lb(lbraw2, PLCDB1Read.Instance().RawAI2);
            show_Data_Real_lb(lbrealvalue2, PLCDB1Read.Instance().pressure);
            show_Data_Int_lb(lbraw3, PLCDB1Read.Instance().RawAI3);
            show_Data_Real_lb(lbrealvalue3, PLCDB1Read.Instance().wc);


        }
        #endregion

     //   #region
        private void show_Data_Int_lb(Label lb, short value)
        {

            lb.Text = value.ToString();

        }
       // #endregion
        #region
        private void show_Data_Real_lb(Label lb, double value)
        {

            lb.Text = value.ToString("0.00");

        }
        #endregion
        #region
        private void show_Data_Real_txt(TextBox txt, double value)
        {

            txt.Text = value.ToString("0.00");

        }
        private void show_Data_Dint_txt(TextBox txt, int value)
        {

            txt.Text = (value/100).ToString("0.00");

        }
        #endregion
        #region
        private void Write_Data_real_txt(string Address, TextBox txt)
        {
            if (txt.Text != "")
            {
                PLC.Instance().WriteReal(Address, (double.Parse(txt.Text))*100);
            }
        }
        #endregion

        private void label84_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }
        // AI1
        private void txtanalogmin1_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD0", txtanalogmin1);
            }
           
        }

        private void txtanalogmax1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD4", txtanalogmax1);
            }
        }

        private void txtscalemin1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD8", txtscalemin1);
            }
        }

        private void txtscalemax1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //Write_parastatic.Scale_value_high_1 = double.Parse(txtscalemax1.Text);
                Write_Data_real_txt("DB2.DBD12", txtscalemax1);
            }
        }

        // AI2

        private void txtanalogmin2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD16", txtanalogmin2);
            }
        }

        private void txtanalogmax2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD20", txtanalogmax2);
            }
        }

        private void txtscalemin2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD24", txtscalemin2);
            }
        }

        private void txtscalemax2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD28", txtscalemax2);
            }
        }

        private void label23_Click(object sender, EventArgs e)
        {

        }
        //AI3

        private void txtanalogmin3_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD32", txtanalogmin3);
            }
        }

        private void txtanalogmax3_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD36", txtanalogmax3);
            }
        }

        private void txtscalemin3_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD40", txtscalemin3);
            }
        }

        private void txtscalemax3_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Write_Data_real_txt("DB2.DBD44", txtscalemax3);
            }
        }





        //AI10












    }
}
