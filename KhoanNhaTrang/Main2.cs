using Dapper;
using KhoanNhaTrang.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Drawing.Printing;
using Spire.Xls;

namespace KhoanNhaTrang
{

    public partial class Main2 : Form
    {
        int limitPercentScaleY = 100;
        private List<Data> listData = new List<Data>();
        private int index = 0;
        int tickStart = 0;
        long managementId = 0;
        private String startDate;
        private String startHour;
        private String endDate;
        private String endHour;
        private GraphPane myPane;
        Boolean isInsertData = false;
        Boolean debugMode = false;
        Config config = new Config();
        DateTime lastInsert;
        Boolean firstInsert;
        MaxY maxY = new MaxY();
        static Boolean isRunning = false;


        float widthBorderGraph = 2.0F;

        public void changeMaxY() {
            if(!txtMaxOfYFlowrate.Text.Equals(""))
                maxY.flow = int.Parse(txtMaxOfYFlowrate.Text);

            if (!txtMaxOfYPressure.Text.Equals(""))
                maxY.pressure = int.Parse(txtMaxOfYPressure.Text);
        }

        public Main2()
        {
            InitializeComponent();
            changeMaxY();
        }
        //Dapper 
        //https://hanhtranglaptrinh.net/dapper-c-la-gi-micro-orm-trong-net/
        static string connStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
        public static IDbConnection db = new MySqlConnection(connStr);

        private Data insertDB()
        {
            var data = new Data();
            try
            {

                db.Open();
                if (debugMode)
                {
                    data.flow_rate = double.Parse(txtflowrate.Text);
                    data.pressure = double.Parse(txtpressure.Text);
                    data.management_id = managementId;
                    data.insert_date = new DateTime();
                }
                else {
                    data.flow_rate = Math.Round(PLCDB1Read.Instance().flow_rate, 2);
                    data.pressure = Math.Round(PLCDB1Read.Instance().pressure, 2);
                    data.management_id = managementId;
                    data.insert_date = new DateTime();

                }
                string query = "";

                if (!firstInsert)
                { 
                    int a = 4;
                }

                if (tickStart % config.time_store_db == 0 || firstInsert)
                {
                    if (firstInsert)
                    {
                        data.flow_rate = 0;
                        data.pressure = 0;
                    }
                    query = @"insert into data(flow_rate,pressure,management_id) values(@flow_rate,@pressure,@management_id);
                            select * from data order by id desc limit 1";
                    data = db.Query<Data>(query, data).Single();
                    data.flow_rate = Math.Round(data.flow_rate, 2);
                    data.pressure = Math.Round(data.pressure, 2);
                    data.management_id = managementId;
                    lastInsert = data.insert_date;
                    firstInsert = false;
                }
                else
                {
                    data.management_id = 0;
                }

               
                //Update Cement total
                var param = new DynamicParameters();
                param.Add("Id", managementId);
                param.Add("cement_total", Math.Round(PLCDB1Read.Instance().cement_total, 2));
                query = "update management set cement_total = @cement_total where Id = @Id";
                db.Execute(query, param);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                try
                {
                    db.Close();
                }
                catch (Exception ex)
                {

                }
            }


            return data;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Start();
            try
            {
                if (PLC.Instance().Open())
                {
                    lbAlarmPLC.Text = "PLC Connected";
                    lbAlarmPLC.BackColor = Color.Lime;
                }
                else
                {
                    lbAlarmPLC.Text = "PLC not connected";
                    lbAlarmPLC.BackColor = Color.Red;
                }
            }
            catch (Exception EX) { }
            // khi khởi động sẽ được chạy
            //sampleConnectDB();
            // init combobox
            cbOrder.Items.Add("I Order");
            cbOrder.SelectedIndex = 0;
            cbRange.Items.Add("Upper");
            cbRange.SelectedIndex = 0;

            // init button
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnSaveToAs.Enabled = false;
            btnPrint.Enabled = false;
            btnAddInfo.Enabled = false;
            btnClose.Enabled = true;

            initChart();
        }

        private void initChart()
        {
            chartTimeCurves.GraphPane.CurveList.Clear();
            chartTimeCurves.GraphPane.GraphObjList.Clear();

            // Khai báo sử dụng Graph loại GraphPane;
            myPane = chartTimeCurves.GraphPane;
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Flowrate (L/min)";
            myPane.YAxis.Title.Text = "Pressure (MPa)";

            //Định nghĩa list để vẽ đồ thị.
            RollingPointPairList listFlowRate = new RollingPointPairList(60000); 
            LineItem curveLine = myPane.AddCurve("", listFlowRate, Color.Red, SymbolType.None); // SymbolType là kiểu biểu thị đồ thị : điểm, đường tròn, tam giác....

            // Định hiện thị cho trục thời gian (Trục X)
            myPane.XAxis.Scale.Min = 0;
            //myPane.XAxis.Scale.Max = 100;
            //myPane.XAxis.Scale.MinorStep = 15;
            //myPane.XAxis.Scale.MajorStep = 5;
            myPane.XAxis.MajorGrid.IsVisible = true;

            // Set Scale to default X
            myPane.XAxis.Scale.MinAuto = true;
            myPane.XAxis.Scale.MaxAuto = true;
            myPane.XAxis.Scale.MajorStepAuto = true;
            myPane.XAxis.Scale.MinorStepAuto = true;
            myPane.XAxis.CrossAuto = true;
            myPane.XAxis.Scale.MagAuto = true;
            myPane.XAxis.Scale.FormatAuto = true;

            // Set Scale to default Y
            myPane.YAxis.Scale.MinAuto = true;
            myPane.YAxis.Scale.MaxAuto = true;
            myPane.YAxis.Scale.MajorStepAuto = true;
            myPane.YAxis.Scale.MinorStepAuto = true;
            myPane.YAxis.CrossAuto = true;
            myPane.YAxis.Scale.MagAuto = true;
            myPane.YAxis.Scale.FormatAuto = true;


            // Định hiện thị cho trục thời gian(Trục Y)
           // myPane.YAxis.Scale.Min = 0;
           // myPane.YAxis.Scale.Max = 100;
           // myPane.YAxis.Scale.MinorStep = 1;
           // myPane.YAxis.Scale.MajorStep = 10;
            myPane.YAxis.MajorGrid.IsVisible = true;

            // Gọi hàm xác định cỡ trục
            myPane.AxisChange();

        }
        private void btnStart_MouseDown(object sender, MouseEventArgs e)
        {
            PLC.Instance().SetBit("DB2.DBX48.0");
        }

        private void btnStart_MouseUp(object sender, MouseEventArgs e)
        {
            PLC.Instance().ResetBit("DB2.DBX48.0");
            if (txtMaxOfYFlowrate.Text != null && !"".Equals(txtMaxOfYFlowrate.Text.Trim())
                && txtMaxOfYPressure.Text != null && !"".Equals(txtMaxOfYPressure.Text.Trim()))
            {
                if (tickStart == 0)
                {
                    PLC.Instance().SetBit("DB2.DBX48.4");
                    lbGroutedTime.Text = "00:00:00";
                    string queryCheckManagement = @"SELECT number_equipment FROM management WHERE DATE(insert_date) = DATE(now()) ORDER BY id DESC LIMIT 1;";
                    int numberEquipment = db.Query<int>(queryCheckManagement).FirstOrDefault();
                    numberEquipment++;

                    string query = @"insert into management(number_equipment) values(" + numberEquipment + ");";
                    db.Execute(query);
                    lbEquipment.Text = numberEquipment.ToString();


                    queryCheckManagement = @"SELECT id FROM management WHERE DATE(insert_date) = DATE(now()) ORDER BY id DESC LIMIT 1;";
                    managementId = db.Query<long>(queryCheckManagement).FirstOrDefault();

                    startDate = DateTime.Now.ToString("yyyy-MM-dd").ToString();
                    startHour = DateTime.Now.ToString("hh:mm:ss tt").ToString();

                    lbBeginTimeDate.Text = startDate;
                    lbBeginTimeHour.Text = startHour;

                    try
                    {
                        db.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        try
                        {
                            db.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    initChart();
                }

                txtMaxOfYFlowrate.ReadOnly = false;
                txtMaxOfYPressure.ReadOnly = false;

                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnEnd.Enabled = true;
                timer1.Enabled = true;


                isInsertData = true;

            }
            else
            {
                MessageBox.Show("Các trường Max of Y của Flowrate, Pressure không được rỗng.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            isRunning = true;


            changeMaxY();
            lastInsert = new DateTime();
            firstInsert = true;
            string query = @"select * from config order by id desc limit 1";
            config = db.Query<Config>(query, config).Single();
            //
            //if (data.management_id > 0)
            //{
            Data data = insertDB();
            listData.Add(data);
                Boolean isRedraw = false;

                if (!isRedraw && isInsertData)
                    Draw(data.flow_rate, data.pressure);
            //}
            //
            timer1.Stop();
            timer1.Interval = config.time_update_ui * 1000;
            btnEnd.Enabled = true;
            timer1.Start();
        }


        private void show_Data_Real_lb(TextBox lb, double value)
        {
            lb.Text = (value / 1).ToString("0.00");
        }
        private void show_Data_Int_lb(TextBox lb, short value)
        {

            lb.Text = value.ToString();

        }

   

        public void simulator() {
            int min = 50;
            int max = 200;
            Random _random = new Random();
            txtflowrate.Text = _random.Next(50, 60).ToString();
            txtpressure.Text = _random.Next(3, 5).ToString();
        }

        public void reDraw()
        {
            tickStart = 0;
            initChart();
            for (int i = 0; i < listData.Count;i++)
            {
                double valueFlowRate = listData[i].flow_rate ;
                double valuePressure = listData[i].pressure ;
                Draw(valueFlowRate, valuePressure);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(isRunning) { 
            show_Data_Real_lb(txtdensi, PLCDB1Read.Instance().wc);
            try
            {
                if (debugMode)
                    simulator();
                else
                {
                    //Read value from PLC
                    PLC.Instance().ReadClass(PLCDB1Read.Instance(), 1);
                    PLC.Instance().ReadClass(PLCDB2Write.Instance(), 2);
                    show_Data_Real_lb(txtflowrate, Math.Round(PLCDB1Read.Instance().flow_rate, 2));
                    show_Data_Real_lb(txtpressure, Math.Round(PLCDB1Read.Instance().pressure, 2));
                }

                if ((debugMode && isInsertData) ||  (isInsertData && PLC.Instance().Open()))
                { 
                    Data data = insertDB();
    
                    show_Data_Real_lb(txtflowrate, data.flow_rate);
                    show_Data_Real_lb(txtpressure, data.pressure);

                    if(data.management_id>0)
                    { 
                        listData.Add(data);
                        Boolean isRedraw = false;

                        if(!isRedraw && isInsertData)
                            Draw(data.flow_rate, data.pressure);
                    }
                    tickStart = tickStart + config.time_update_ui;
                }
            }
            catch (Exception ex)
            {
                

               // ex.StackTrace();
            }
            }
        }

        private void Draw(double flowRate, double pressure)
        {
            initChart();
            if (chartTimeCurves.GraphPane.CurveList.Count <= 0)
                return;

            // Đưa về điểm xuất phát
            LineItem curveLine = chartTimeCurves.GraphPane.CurveList[0] as LineItem;
            curveLine.Line.Width = widthBorderGraph;


            if (curveLine == null)
                return;
            IPointListEdit listFlowRate = curveLine.Points as IPointListEdit;

            if (listFlowRate == null)
                return;

            // Thêm điểm trên đồ thị
            listFlowRate.Add(0, 0);
            listFlowRate.Add(flowRate, pressure );

            // Đoạn chương trình thực hiện vẽ đồ thị
            Scale xScale = chartTimeCurves.GraphPane.XAxis.Scale;
            Scale yScale = chartTimeCurves.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (flowRate > xScale.Max - xScale.MajorStep)
            {
                //xScale.Max = tickStart + xScale.MajorStep;
                xScale.Max = flowRate + xScale.Max;
                xScale.Min = 0;
            }
            int seconds = tickStart;

            TimeSpan timeSpan = TimeSpan.FromSeconds(tickStart);
            string groutedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            lbGroutedTime.Text = groutedTime;
         

            // Vẽ đồ thị
            chartTimeCurves.AxisChange();
            chartTimeCurves.Invalidate();
            chartTimeCurves.Refresh();
        }
        private void btnPause_MouseDown(object sender, MouseEventArgs e)
        {
            isRunning = false;
            PLC.Instance().SetBit("DB2.DBX48.1");
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = true;
            btnSaveToAs.Enabled = true;
            btnPrint.Enabled = true;
            isInsertData = false;
        }
        private void btnPause_MouseUp(object sender, MouseEventArgs e)
        {
            PLC.Instance().ResetBit("DB2.DBX48.1");
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            //PLC.Instance().ResetBit("DB2.DBX48.1");
            //btnStart.Enabled = true;
            //btnPause.Enabled = false;
            //btnEnd.Enabled = true;
            //btnSaveToAs.Enabled = true;
            //btnPrint.Enabled = true;
            //isInsertData = false;
        }
        private void btnEnd_MouseDown(object sender, MouseEventArgs e)
        {
            //Data data = insertDB();
            PLC.Instance().SetBit("DB2.DBX48.2");
            index = 0;

            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnSaveToAs.Enabled = true;
            btnPrint.Enabled = true;

            txtMaxOfYFlowrate.ReadOnly = false;
            txtMaxOfYPressure.ReadOnly = false;

            tickStart = 0;

            endDate = DateTime.Now.ToString("yyyy-MM-dd     hh:mm:ss tt").ToString();
            endHour = DateTime.Now.ToString("hh:mm:ss tt").ToString();

            isInsertData = false;
        }

        private void btnEnd_MouseUp(object sender, MouseEventArgs e)
        {
            PLC.Instance().ResetBit("DB2.DBX48.2");
        }
        private void btnEnd_Click(object sender, EventArgs e)
        {
            try
            {
                db.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                try
                {
                    db.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void btnSaveToAs_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnSaveToAs.Enabled = true;
            btnPrint.Enabled = true;

            SaveFileDialog svg = new SaveFileDialog();
            svg.FileName = "untitled";
            svg.Filter = "Excel files|.xlsx";
            if (svg.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(svg.FileName, FileMode.Create))
                {
                    createExcel(stream, System.IO.Path.GetDirectoryName(svg.FileName));
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            string tempPathFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddhhmmss").ToString() + ".xlsx");
            using (FileStream stream = new FileStream(tempPathFile, FileMode.Create))
            {
                createExcel(stream, Path.GetTempPath());

                Workbook workbook = new Workbook();
                workbook.LoadFromFile(tempPathFile);
                PrintDialog dialog = new PrintDialog();
                dialog.AllowPrintToFile = true;
                dialog.AllowCurrentPage = true;
                dialog.AllowSomePages = true;
                dialog.AllowSelection = true;
                dialog.UseEXDialog = true;
                dialog.PrinterSettings.Duplex = Duplex.Simplex;
                dialog.PrinterSettings.FromPage = 0;
                dialog.PrinterSettings.ToPage = 8;
                dialog.PrinterSettings.PrintRange = PrintRange.SomePages;
                PrintDocument pd = workbook.PrintDocument;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }
            }
        }

        private void createExcel(FileStream fs, String path)
        {
            List<Data> listDataReport = new List<Data>();
            try
            {
                db.Open();
                var param = new DynamicParameters();
                param.Add("management_id", managementId);
                string query = @"
                                 SELECT flow_rate, pressure ,insert_date
                                 FROM grouting.data
                                 where management_id =@management_id
                                    order by id";
                listDataReport = db.Query<Data>(query, param).ToList();
                List<Data> tmpReport = new List<Data>();
                foreach (Data temp in listDataReport)
                {

                    temp.flow_rate = Math.Round(temp.flow_rate, 2);
                    temp.pressure = Math.Round(temp.pressure, 2);
                    tmpReport.Add(temp);
                }
                listDataReport = tmpReport;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    db.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            if (listDataReport != null && listDataReport.Any())
            {
                var ds = new DataSet();
               
                // Paragrap1
                var par1 = new DataTable();
                par1.Columns.Add("equipment");
                par1.Columns.Add("projectname");
                par1.Columns.Add("holeparameter"); 
                par1.Columns.Add("holeno");
                par1.Columns.Add("order");
                par1.Columns.Add("the");
                par1.Columns.Add("begintime");
                par1.Columns.Add("recorder");
                par1.Columns.Add("leader");
                par1.Columns.Add("quality");
                object[] values = new object[10];
                values[0] = "Equipment: " + lbEquipment.Text + " Single " + startDate;
                values[1] = "Project Name: " + txtProjectName.Text;
                values[2] = "Holes parameters: ";
                values[3] = "Hole No.: " + txtHoleNo.Text;
                values[4] = "Order: " + cbOrder.Text  + " Range: " + cbRange.Text;
                values[5] = "The: " + txtThe.Text + " Sect From: " + txtSectFrom.Text + " To: " + txtTo.Text + "m" + " Length " + txtLength.Text;
                values[6] = "Begin Time: " + startHour;
                values[7] = "Recorder: " + txtRecorder.Text;
                values[8] = "Leader: " + txtLeader.Text;
                values[9] = "Quality: " + txtQuality.Text;
                par1.Rows.Add(values);
                par1.TableName = "par1";
                ds.Tables.Add(par1);

                // Table 1
                var tab1 = new DataTable();
                tab1.Columns.Add("time", typeof(string));
                tab1.Columns.Add("flowrate", typeof(string));
                tab1.Columns.Add("tfluid", typeof(string));
                tab1.Columns.Add("pressure", typeof(string));
                tab1.Columns.Add("wc", typeof(string));
                String timeMin = "00:00:01";
                int seconds = config.time_store_db;
                double totalFlowrate = 0;
                double totalPressure = 0;
                int count = 0;
                foreach (Data data in listDataReport)
                {
                    totalFlowrate = totalFlowrate + Math.Round(data.flow_rate, 2);
                    totalPressure = totalPressure + Math.Round(data.pressure, 2);
                    count++;
                    tab1.Rows.Add(timeMin, Math.Round(data.flow_rate, 2).ToString("0.000"), "",  Math.Round(data.pressure, 2).ToString("0.000"), "");
                    TimeSpan timeMinSpan = TimeSpan.FromSeconds(seconds);
                    timeMin = string.Format("{0:D2}:{1:D2}:{2:D2}", timeMinSpan.Hours, timeMinSpan.Minutes, timeMinSpan.Seconds);
                    seconds = seconds + config.time_store_db;
                }
                tab1.TableName = "tab1";
                ds.Tables.Add(tab1);

                // Paragrap 2
                var par2 = new DataTable();
                par2.Columns.Add("endtime");
                par2.Columns.Add("stablepressure");
                par2.Columns.Add("stableflowrate");
                par2.Columns.Add("radioofpermeate");
                object[] values2 = new object[4];
                values2[0] = "End Time: " + endDate;
                double tbTotalPressure = totalPressure / count;
                double tbTotalFlowrate = totalFlowrate / count;
                values2[1] = "Stable Pressure: " + Math.Round(tbTotalPressure, 3).ToString("0.000")  + " MPa";
                values2[2] = "Stable Flowrate: " + Math.Round(tbTotalFlowrate, 3).ToString("0.000") + " L/Min";
                double q = Math.Round(tbTotalFlowrate, 3) / (double.Parse(txtLength.Text) * Math.Round(tbTotalPressure, 3));
                values2[3] = "Ratio of Permeate q = " + Math.Round(q, 3).ToString("0.000") + " (Lu)";
                par2.Rows.Add(values2);
                par2.TableName = "par2";
                ds.Tables.Add(par2);

                // chart 
                var chart = new DataTable();
                chart.Columns.Add("timecurves");
                chart.Rows.Add("");
                chart.TableName = "chart";
                ds.Tables.Add(chart);

                // Draw chart
                String imageChartName = @"\Chart" + DateTime.Now.ToString("yyyyMMddhhmmss").ToString() + ".bmp";
                ZedGraphControl tmp = new ZedGraphControl();
                tmp = chartTimeCurves;
                GraphPane graphPane = tmp.GraphPane.Clone();
                graphPane.XAxis.MajorGrid.IsVisible = true;


                LineItem curveFlowRate = tmp.GraphPane.CurveList[0] as LineItem;
                curveFlowRate.Line.Width = 3F;

                graphPane.XAxis.MajorGrid.DashOn = 10.0F;
                graphPane.YAxis.MajorGrid.DashOn = 10.0F;

                graphPane.XAxis.Scale.FontSpec.Size = 32;
                graphPane.YAxis.Scale.FontSpec.Size = 32;

                graphPane.YAxis.Title.IsVisible = false;
                graphPane.XAxis.Title.IsVisible = false;
                graphPane.Title.IsVisible = false;

                graphPane.Border.IsVisible = false;
                graphPane.Legend.IsVisible = false;
                graphPane.Title.IsVisible = false;

                //graphPane.YAxis.Scale.Min = 0;
                //graphPane.YAxis.Scale.Max = 100;
                //graphPane.YAxis.Scale.MinorStep = 1;
                //graphPane.YAxis.Scale.MajorStep = 10;
                graphPane.YAxis.MajorGrid.IsVisible = true;

                Bitmap bitmap = graphPane.GetImage();
                ToGrayScale(bitmap);
                bitmap.Save(path + imageChartName);

                ExportExcelToTemplateEpplus.TemplateExcel2.FillReport(fs, "TemplateMain2.xlsx", ds, listDataReport.Count -1, path + imageChartName,  new string[] { "{", "}" });
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra vui lòng thử lại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
            {
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtMaxOfYFlowrate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&(e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }


            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                changeMaxY();
                reDraw();
            }


        }


        private void txtMaxOfYPressure_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                changeMaxY();
                reDraw();
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void txtQuality_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            frParameter _frShow = new frParameter();
            _frShow.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            PLC.Instance().SetBit("DB2.DBX48.3");
        }

        private void ON_OFF_ALARM(bool output, TextBox lb)
        {
            if (output == true) // khong error
            {
                lb.Text = "RECORDER RUNNING";
                lb.BackColor = Color.Lime;
            }

            else //on
            {
                lb.Text = "RECORDER STOOPING";
                lb.BackColor = Color.Red;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!debugMode  ) {
                try
                {
                    if (PLC.Instance().Open())
                    {
                        lbAlarmPLC.Text = "PLC Connected";
                        lbAlarmPLC.BackColor = Color.Lime;
                    }
                    else
                    {
                        lbAlarmPLC.Text = "PLC not connected";
                        lbAlarmPLC.BackColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {

                }
                PLC.Instance().ReadClass(PLCDB1Read.Instance(), 1);
                PLC.Instance().ReadClass(PLCDB2Write.Instance(), 2);
                PLC.Instance().ReadClass(PLCDB3READ.Instance(), 3);
                show_Data_Real_lb(txtflowrate, Math.Round(PLCDB1Read.Instance().flow_rate, 2));
                show_Data_Real_lb(txtpressure, Math.Round(PLCDB1Read.Instance().pressure, 2));
                show_Data_Real_lb(txtdensi, PLCDB1Read.Instance().wc);
            }
        }

        private void txtMaxOfYFlowrate_TextChanged(object sender, EventArgs e)
        {
          
        }

        private void txtMaxOfYTotalFlow_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void txtMaxOfYWC_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtMaxOfYPressure_TextChanged(object sender, EventArgs e)
        {
  
        }

        private void chartTimeCurves_VisibleChanged(object sender, EventArgs e)
        {
            chartTimeCurves.RestoreScale(myPane);
        }
        private List<Data> DataForGrid()
        {
            List<Data> listDataReport = new List<Data>();
            try
            {
                db.Open();
                var param = new DynamicParameters();
                param.Add("management_id", managementId);
                string query = @"
                                 SELECT flow_rate, fluid, wc, pressure ,insert_date
                                 FROM grouting.data
                                 where management_id =@management_id
                                    order by id";
                listDataReport = db.Query<Data>(query, param).ToList();
                List<Data> tmpReport = new List<Data>();
                foreach (Data temp in listDataReport)
                {

                    temp.flow_rate = Math.Round(temp.flow_rate, 2);
                    temp.fluid = Math.Round(temp.fluid, 2);
                    temp.pressure = Math.Round(temp.pressure, 2);
                    temp.wc = Math.Round(temp.wc, 2);
                    tmpReport.Add(temp);
                }
                listDataReport = tmpReport;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    db.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return listDataReport;

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    try
                    {
                        dataGridView1.Rows.Clear();
                    }
                    catch (Exception ex)
                    {
                    }
                    String timeMin = "00:00:01";
                    int seconds = config.time_store_db;

                    var dataTable = new DataTable();
                    dataTable.Columns.Add("time", typeof(string));
                    dataTable.Columns.Add("flowrate", typeof(string));
                    dataTable.Columns.Add("pressure", typeof(string));
                    foreach (Data data in DataForGrid())
                    {

                        DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
                        row.Cells[0].Value = timeMin;
                        row.Cells[1].Value = Math.Round(data.flow_rate, 2).ToString("0.00");
                        row.Cells[2].Value = Math.Round(data.pressure, 2).ToString("0.00");
                        dataGridView1.Rows.Add(row);

                        TimeSpan timeMinSpan = TimeSpan.FromSeconds(seconds);
                        timeMin = string.Format("{0:D2}:{1:D2}:{2:D2}", timeMinSpan.Hours, timeMinSpan.Minutes, timeMinSpan.Seconds);
                        seconds = seconds + config.time_store_db;
                    }
                    break;
            }
        }
        public static bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        private void txtSectFrom_TextChanged(object sender, EventArgs e)
        {
            if (IsNumeric(txtSectFrom.Text) && IsNumeric(txtTo.Text))
            {
                txtLength.Text = (Double.Parse(txtTo.Text) - Double.Parse(txtSectFrom.Text)).ToString();
            }
        }

        private void txtTo_TextChanged(object sender, EventArgs e)
        {
            if (IsNumeric(txtSectFrom.Text) && IsNumeric(txtTo.Text))
            {
                txtLength.Text = (Double.Parse(txtTo.Text) - Double.Parse(txtSectFrom.Text)).ToString();
            }
        }
    }

 
}
