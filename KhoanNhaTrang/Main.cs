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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using System.Drawing.Printing;
using Spire.Xls;

namespace KhoanNhaTrang
{

    public partial class Form1 : Form
    {
        int limitPercentScaleY = 95;
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
        Boolean debugMode = true;
        Config config = new Config();
        DateTime lastInsert;
        Boolean firstInsert;
        MaxY maxY = new MaxY();


        float widthBorderGraph = 2.0F;

        public void changeMaxY() {
            if(!txtMaxOfYFlowrate.Text.Equals(""))
                maxY.flow = int.Parse(txtMaxOfYFlowrate.Text);

            if (!txtMaxOfYTotalFlow.Text.Equals(""))
                maxY.fluid = int.Parse(txtMaxOfYTotalFlow.Text);

            if (!txtMaxOfYWC.Text.Equals(""))
                maxY.wc = int.Parse(txtMaxOfYWC.Text);

            if (!txtMaxOfYPressure.Text.Equals(""))
                maxY.pressure = int.Parse(txtMaxOfYPressure.Text);
        }

        public Form1()
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
                    data.fluid = double.Parse(txttotalflow.Text);
                    data.pressure = double.Parse(txtpressure.Text);
                    data.wc = double.Parse(txtWC.Text);
                    data.management_id = managementId;
                    data.insert_date = new DateTime();
                }
                else {
                    data.flow_rate = Math.Round(PLCDB1Read.Instance().flow_rate, 2);
                    data.fluid = Math.Round(PLCDB1Read.Instance().fluid, 2);
                    data.pressure = Math.Round(PLCDB1Read.Instance().pressure, 2);
                    //data.wc = PLCDB1Read.Instance().wc_1;
                    data.wc = double.Parse( (txtWC.Text).Replace(":1","") );
                    data.management_id = managementId;
                    data.insert_date = new DateTime();

                }
                string query = "";


                Debug.WriteLine("data.insert_date:" +data.insert_date.ToString());
                Debug.WriteLine("lastInsert:" + lastInsert.ToString());
                Debug.WriteLine((data.insert_date - lastInsert).TotalSeconds.ToString());

                if (!firstInsert)
                { 
                    int a = 4;
                }

                if (tickStart%config.time_store_db==0 || firstInsert)
                {
                    query = @"insert into data(flow_rate, fluid,pressure,wc,management_id) values(@flow_rate, @fluid,@pressure,@wc,@management_id);
                            select * from data order by id desc limit 1";
                    data = db.Query<Data>(query, data).Single();
                    data.flow_rate = Math.Round(data.flow_rate, 2);
                    data.fluid = Math.Round(data.fluid, 2);
                    data.pressure = Math.Round(data.pressure, 2);
                    data.wc = Math.Round(data.wc, 2);
                    lastInsert = data.insert_date;
                    firstInsert = false;
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
            myPane.Title.Text = "EM Grouting Curves";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Percent";

            //Định nghĩa list để vẽ đồ thị.
            RollingPointPairList listFlowRate = new RollingPointPairList(60000); // Sử dụng list với 60000 điểm
            LineItem curveFlowRate = myPane.AddCurve("Flowrate", listFlowRate, Color.Red, SymbolType.None); // SymbolType là kiểu biểu thị đồ thị : điểm, đường tròn, tam giác....
            RollingPointPairList listFluid = new RollingPointPairList(60000);
            LineItem curveFluid = myPane.AddCurve("Total flow", listFluid, Color.Blue, SymbolType.None);
            RollingPointPairList listWC = new RollingPointPairList(60000);
            LineItem curveWC = myPane.AddCurve("W/C", listWC, Color.Brown, SymbolType.None);
            RollingPointPairList listPressure = new RollingPointPairList(60000);
            LineItem curvePressure = myPane.AddCurve("Pressure", listPressure, Color.Green, SymbolType.None);
            //RollingPointPairList listFlowRate = new RollingPointPairList(60000); // Sử dụng list với 60000 điểm
            //LineItem curveFlowRate = myPane.AddCurve("Flowrate", listFlowRate, Color.Black, SymbolType.None); // SymbolType là kiểu biểu thị đồ thị : điểm, đường tròn, tam giác....
            //RollingPointPairList listFluid = new RollingPointPairList(60000);
            //LineItem curveFluid = myPane.AddCurve("Total flow", listFluid, Color.Black, SymbolType.None);
            //RollingPointPairList listWC = new RollingPointPairList(60000);
            //LineItem curveWC = myPane.AddCurve("W/C", listWC, Color.Black, SymbolType.None);
            //RollingPointPairList listPressure = new RollingPointPairList(60000);
            //LineItem curvePressure = myPane.AddCurve("Pressure", listPressure, Color.Black, SymbolType.None);

            // Định hiện thị cho trục thời gian (Trục X)
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 600;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 10;
            myPane.XAxis.MajorGrid.IsVisible = true;

            // Set Scale to default X
            myPane.XAxis.Scale.MinAuto = true;
            myPane.XAxis.Scale.MaxAuto = false;
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
            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.Max = 100;
            myPane.YAxis.Scale.MinorStep = 1;
            myPane.YAxis.Scale.MajorStep = 10;
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
            //txtMaxOfYTotalFlow.Text = (Convert.ToDouble(txtMaxOfYTotalFlow.Text) * 2).ToString();
            //valueFluid = ((data.fluid * 100) / Convert.ToDouble(txtMaxOfYTotalFlow.Text));
            //changeMaxY();
            //reDraw();
            PLC.Instance().ResetBit("DB2.DBX48.0");
            if (txtMaxOfYFlowrate.Text != null && !"".Equals(txtMaxOfYFlowrate.Text.Trim())
                && txtMaxOfYTotalFlow.Text != null && !"".Equals(txtMaxOfYTotalFlow.Text.Trim())
                && txtMaxOfYWC.Text != null && !"".Equals(txtMaxOfYWC.Text.Trim())
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
                        //query = @"SELECT flow_rate, fluid, wc, pressure,insert_date FROM grouting.data";
                        //listData = db.Query<Data>(query).ToList();
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
                txtMaxOfYTotalFlow.ReadOnly = false;
                txtMaxOfYWC.ReadOnly = false;
                txtMaxOfYPressure.ReadOnly = false;

                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnEnd.Enabled = true;
                timer1.Enabled = true;


                isInsertData = true;

            }
            else
            {
                MessageBox.Show("Các trường Max of Y của Flowrate, Total flow, W/C, Pressure không được rỗng.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().WC_start);
            changeMaxY();
            lastInsert = new DateTime();
            firstInsert = true;
            string query = @"select * from config order by id desc limit 1";
            config = db.Query<Config>(query, config).Single();
            timer1.Stop();
            timer1.Interval = config.time_update_ui * 100;
            btnEnd.Enabled = true;
            timer1.Start();
        }

        private void show_Data_Real_lb_wc(TextBox lb, double value)
        {
            lb.Text = value.ToString("0.0") +":1";
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
            txtflowrate.Text = _random.Next(min, max).ToString();
            txttotalflow.Text = _random.Next(50, 100).ToString();
            txtWC.Text = _random.Next(1, 10).ToString();
            txtpressure.Text = _random.Next(30, 40).ToString();
        }

        public void reDraw()
        {
            tickStart = 0;
            initChart();
            for (int i = 0; i < listData.Count;i++)
            {
                double valueFlowRate = ((listData[i].flow_rate * 100) / Convert.ToDouble(maxY.flow));
                double valueFluid = ((listData[i].fluid * 100) / Convert.ToDouble(maxY.fluid));
                double valueWC = ((listData[i].wc * 100) / Convert.ToDouble(maxY.wc));
                double valuePressure = ((listData[i].pressure * 100) / Convert.ToDouble(maxY.pressure));
                Draw(valueFlowRate, valueFluid, valueWC, valuePressure);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {


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
                    show_Data_Real_lb(txttotalflow, Math.Round(PLCDB1Read.Instance().fluid, 2));
                    show_Data_Real_lb(txtdensi, PLCDB1Read.Instance().wc);
                    show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().wc_1);
                    show_Data_Real_lb(txtpressure, Math.Round(PLCDB1Read.Instance().pressure, 2));
                }

                if ( (debugMode && isInsertData) || isInsertData && PLC.Instance().Open())
                { 
                    Data data = insertDB();
                        
                    show_Data_Real_lb(txtflowrate, data.flow_rate);
                    show_Data_Real_lb(txttotalflow, data.fluid);
                    //show_Data_Real_lb_wc(txtWC, data.wc);
                    show_Data_Real_lb(txtpressure, data.pressure);

                    double valueFlowRate = ((data.flow_rate * 100) / Convert.ToDouble(maxY.flow));
                    double valueFluid = ((data.fluid * 100) / Convert.ToDouble(maxY.fluid));
                    double valueWC = ((data.wc * 100) / Convert.ToDouble(maxY.wc));
                    double valuePressure = ((data.pressure * 100) / Convert.ToDouble(maxY.pressure));

                    listData.Add(data);
                    Boolean isRedraw = false;

                    while (valueFlowRate > limitPercentScaleY)
                    {
                        txtMaxOfYFlowrate.Text = (Convert.ToDouble(txtMaxOfYFlowrate.Text) * 2).ToString();
                        valueFlowRate = ((data.flow_rate * 100) / Convert.ToDouble(txtMaxOfYFlowrate.Text));
                        changeMaxY();
                        reDraw();
                        isRedraw = true;
                    }

                    while (valueFluid > limitPercentScaleY)
                    {
                        txtMaxOfYTotalFlow.Text = (Convert.ToDouble(txtMaxOfYTotalFlow.Text) * 2).ToString();
                        valueFluid = ((data.fluid * 100) / Convert.ToDouble(txtMaxOfYTotalFlow.Text));
                        changeMaxY();
                        reDraw();
                        isRedraw = true;
                    }

                    while (valueWC > limitPercentScaleY)
                    {
                        txtMaxOfYWC.Text = (Convert.ToDouble(txtMaxOfYWC.Text) * 2).ToString();
                        valueWC = ((data.wc * 100) / Convert.ToDouble(txtMaxOfYWC.Text));
                        changeMaxY();
                        reDraw();
                        isRedraw = true;
                    }

                    while (valuePressure > limitPercentScaleY)
                    {
                        txtMaxOfYPressure.Text = (Convert.ToDouble(txtMaxOfYPressure.Text) * 2).ToString();
                        valuePressure = ((data.wc * 100) / Convert.ToDouble(txtMaxOfYPressure.Text));
                        changeMaxY();
                        reDraw();
                        isRedraw = true;
                    }

                    if(!isRedraw)
                        Draw(valueFlowRate, valueFluid, valueWC, valuePressure);

                }

                /**/

                /** /
                double valueFlowRate = ((listData[index].flow_rate * 100) / Convert.ToDouble(txtMaxOfYFlowrate.Text));
                double valueFluid = ((listData[index].fluid * 100) / Convert.ToDouble(txtMaxOfYTotalFlow.Text));
                double valueWC = ((listData[index].wc * 100) / Convert.ToDouble(txtMaxOfYWC.Text));
                double valuePressure = ((listData[index].pressure * 100) / Convert.ToDouble(txtMaxOfYPressure.Text));
                Draw(valueFlowRate, valueFluid, valueWC, valuePressure);
                index++;
                /**/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Draw(double flowRate, double fluid, double wc, double pressure)
        {

            if (chartTimeCurves.GraphPane.CurveList.Count <= 0)
                return;

            // Đưa về điểm xuất phát
            LineItem curveFlowRate = chartTimeCurves.GraphPane.CurveList[0] as LineItem;
            curveFlowRate.Line.Width = widthBorderGraph;
            LineItem curveFluid = chartTimeCurves.GraphPane.CurveList[1] as LineItem;
            curveFluid.Line.Width = widthBorderGraph;
            LineItem curveWC = chartTimeCurves.GraphPane.CurveList[2] as LineItem;
            curveWC.Line.Width = widthBorderGraph;
            LineItem curvePressure = chartTimeCurves.GraphPane.CurveList[3] as LineItem;
            curvePressure.Line.Width = widthBorderGraph;

            if (curveFlowRate == null)
                return;

            if (curveFluid == null)
                return;

            if (curveWC == null)
                return;

            if (curvePressure == null)
                return;


            IPointListEdit listFlowRate = curveFlowRate.Points as IPointListEdit;
            IPointListEdit listFluid = curveFluid.Points as IPointListEdit;
            IPointListEdit listWC = curveWC.Points as IPointListEdit;
            IPointListEdit listPressure = curvePressure.Points as IPointListEdit;


            if (listFlowRate == null)
                return;

            if (listFluid == null)
                return;

            if (listWC == null)
                return;

            if (listPressure == null)
                return;

            // Thêm điểm trên đồ thị
            listFlowRate.Add(tickStart, flowRate);
            listFluid.Add(tickStart, fluid);
            listWC.Add(tickStart, wc);
            listPressure.Add(tickStart, pressure);

            // Đoạn chương trình thực hiện vẽ đồ thị
            Scale xScale = chartTimeCurves.GraphPane.XAxis.Scale;
            Scale yScale = chartTimeCurves.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (tickStart > xScale.Max - xScale.MajorStep)
            {
                //xScale.Max = tickStart + xScale.MajorStep;
                xScale.Max = tickStart + xScale.Max;
                xScale.Min = 0;
            }
            int seconds = tickStart;

            TimeSpan timeSpan = TimeSpan.FromSeconds(tickStart);
            string groutedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            lbGroutedTime.Text = groutedTime;
            tickStart = tickStart + config.time_update_ui;

            /** /
            // Tự động Scale theo trục y
            if (flowRate > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = flowRate + yScale.MajorStep;
                yScale.Min = 0;
            }

            if (fluid > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = fluid + yScale.MajorStep;
                yScale.Min = 0;
            }

            if (wc > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = wc + yScale.MajorStep;
                yScale.Min = 0;
            }

            if (pressure > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = pressure + yScale.MajorStep;
                yScale.Min = 0;
            }
            /**/


            // Vẽ đồ thị
            chartTimeCurves.AxisChange();
            // Force a redraw
            chartTimeCurves.Invalidate();
            chartTimeCurves.Refresh();
        }
        private void btnPause_MouseDown(object sender, MouseEventArgs e)
        {
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
            txtMaxOfYTotalFlow.ReadOnly = false;
            txtMaxOfYWC.ReadOnly = false;
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
               
               // var param = new DynamicParameters();
                //param.Add("Id", managementId);
                //param.Add("cement_total", Math.Round(PLCDB1Read.Instance().cement_total, 2));
                //String query = "update management set cement_total = @cement_total where Id = @Id";
                //db.Execute(query, param);
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
                    //createPDF(stream, System.IO.Path.GetDirectoryName(svg.FileName));
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
            if (listDataReport != null && listDataReport.Any())
            {
                var ds = new DataSet();
               
                // Paragrap1
                var par1 = new DataTable();
                par1.Columns.Add("equipment");
                par1.Columns.Add("projectname");
                par1.Columns.Add("grouting"); 
                par1.Columns.Add("holeno");
                par1.Columns.Add("order");
                par1.Columns.Add("the");
                par1.Columns.Add("length");
                par1.Columns.Add("num");
                par1.Columns.Add("desofhole");
                par1.Columns.Add("distbwejectandhole");
                par1.Columns.Add("holehigh");
                par1.Columns.Add("begintime");
                par1.Columns.Add("recorder");
                par1.Columns.Add("leader");
                par1.Columns.Add("quality");
                object[] values = new object[15];
                values[0] = "Equipment: " + lbEquipment.Text + "  " + startDate;
                values[1] = "Project Name: " + txtProjectName.Text;
                values[2] = "Grouting Holes Parameters: ";
                values[3] = "Hole No.: " + txtHoleNo.Text;
                values[4] = "Order: " + cbOrder.Text  + " Range: " + cbRange.Text;
                values[5] = "The: " + txtThe.Text + " Sect From: " + txtSectFrom.Text + " To: " + txtTo.Text + "m";
                values[6] = "Length(m): " + txtLength.Text;
                values[7] = "Num.: " + txtNum.Text;
                values[8] = "Des of Hole(mm): " + txtDesOfHole.Text;
                values[9] = "Dist bw eject and hole(cm): " + txtDistBwEjectAndHole.Text;
                values[10] = "Hole High(m): " + txtHoleHigh.Text;
                values[11] = "Begin Time: " + startHour;
                values[12] = "Recorder: " + txtRecorder.Text;
                values[13] = "Leader: " + txtLeader.Text;
                values[14] = "Quality: " + txtQuality.Text;
                par1.Rows.Add(values);
                par1.TableName = "par1";
                ds.Tables.Add(par1);

                // Table 1
                var tab1 = new DataTable();
                tab1.Columns.Add("time", typeof(string));
                tab1.Columns.Add("flowrate", typeof(string));
                tab1.Columns.Add("tfluid", typeof(string));
                tab1.Columns.Add("wc", typeof(string));
                tab1.Columns.Add("pressure", typeof(string));
                String timeMin = "00:00:01";
                int seconds = config.time_store_db;
                double totalfluid = 0;
                foreach (Data data in listDataReport)
                {
                    totalfluid = data.fluid;
                    tab1.Rows.Add(timeMin, Math.Round(data.flow_rate, 2).ToString("0.00"), Math.Round(data.fluid, 1).ToString("0.0"), data.wc.ToString() + ":1", Math.Round(data.pressure, 2).ToString("0.00"));
                    TimeSpan timeMinSpan = TimeSpan.FromSeconds(seconds);
                    timeMin = string.Format("{0:D2}:{1:D2}:{2:D2}", timeMinSpan.Hours, timeMinSpan.Minutes, timeMinSpan.Seconds);
                    seconds = seconds + config.time_store_db;
                }
                tab1.TableName = "tab1";
                ds.Tables.Add(tab1);

                // Paragrap 2
                var par2 = new DataTable();
                par2.Columns.Add("endtime");
                par2.Columns.Add("totalfluid");
                par2.Columns.Add("ashused");
                object[] values2 = new object[3];
                values2[0] = "End Time: " + endDate;
                values2[1] = "Total Fluid: " + Math.Round(totalfluid, 2) + " L";
                values2[2] = "Ash used: " + Math.Round(PLCDB1Read.Instance().cement_total, 2) + " Kg";
                par2.Rows.Add(values2);
                par2.TableName = "par2";
                ds.Tables.Add(par2);

                // Paragrap 3
                var par3 = new DataTable();
                par3.Columns.Add("maxofflowrate");
                par3.Columns.Add("maxoftfluid");
                par3.Columns.Add("maxofwc");
                par3.Columns.Add("maxofpressure");
                par3.Columns.Add("minofflowrate");
                par3.Columns.Add("minoftfluid");
                par3.Columns.Add("minofwc");
                par3.Columns.Add("minofpressure");
                object[] values3 = new object[8];
                values3[0] = txtMaxOfYFlowrate.Text;
                values3[1] = txtMaxOfYTotalFlow.Text;
                values3[2] = txtMaxOfYWC.Text;
                values3[3] = txtMaxOfYPressure.Text;
                values3[4] = "0";
                values3[5] = "0";
                values3[6] = "0";
                values3[7] = "0";
                par3.Rows.Add(values3);
                par3.TableName = "par3";
                ds.Tables.Add(par3);

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
                LineItem curveFluid = tmp.GraphPane.CurveList[1] as LineItem;
                curveFluid.Line.Width = 3F;
                LineItem curveWC = tmp.GraphPane.CurveList[2] as LineItem;
                curveWC.Line.Width = 3F;
                LineItem curvePressure = tmp.GraphPane.CurveList[3] as LineItem;
                curvePressure.Line.Width = 3F;

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

                graphPane.YAxis.Scale.Min = 0;
                graphPane.YAxis.Scale.Max = 100;
                graphPane.YAxis.Scale.MinorStep = 1;
                graphPane.YAxis.Scale.MajorStep = 10;
                graphPane.YAxis.MajorGrid.IsVisible = true;

                Bitmap bitmap = graphPane.GetImage();
                ToGrayScale(bitmap);
                bitmap.Save(path + imageChartName);

                ExportExcelToTemplateEpplus.TemplateExcel.FillReport(fs, "Template.xlsx", ds, listDataReport.Count -1, path + imageChartName,  new string[] { "{", "}" });
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

        private void createPDF(FileStream fs, String path)
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
                foreach (Data temp in listDataReport) {

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
            if (listDataReport != null && listDataReport.Any())
            {
                iTextSharp.text.Rectangle pagesize = new iTextSharp.text.Rectangle(320, 1500);
                Document doc = new Document(pagesize);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // Draw Header
                iTextSharp.text.Font myFont = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                var parHeader = new Paragraph("Grouting Record Report", myFont);
                parHeader.Alignment = Element.ALIGN_CENTER;
                doc.Add(parHeader);
                // Draw Paragraph 1
                var parEqui = new Paragraph();
                parEqui.Add(new Chunk("Equipment: " + lbEquipment.Text));
                parEqui.Add(Chunk.TABBING);
                parEqui.Add(Chunk.TABBING);
                parEqui.Add(Chunk.TABBING);
                parEqui.Add(new Chunk(startDate));
                doc.Add(parEqui);
                doc.Add(new Paragraph("Project Name: " + txtProjectName.Text));
                doc.Add(new Paragraph("Grouting Holes Parameters: "));
                doc.Add(new Paragraph("Hole No.: " + txtHoleNo.Text));
                var parOrder = new Paragraph();
                parOrder.Add(new Chunk("Order: " + cbOrder.Text));
                parOrder.Add(Chunk.TABBING);
                parOrder.Add(new Chunk(" Range: " + cbRange.Text));
                doc.Add(parOrder);
                var parThe = new Paragraph();
                parThe.Add(new Chunk("The: " + txtThe.Text));
                parThe.Add(Chunk.TABBING);
                parThe.Add(new Chunk(" Sect From: " + txtSectFrom.Text));
                parThe.Add(Chunk.TABBING);
                parThe.Add(new Chunk(" To: " + txtTo.Text + " m"));
                doc.Add(parThe);
                doc.Add(new Paragraph("Length(m): " + txtLength.Text));
                doc.Add(new Paragraph("Num.: " + txtNum.Text));
                doc.Add(new Paragraph("Des of Hole(mm): " + txtDesOfHole.Text));
                doc.Add(new Paragraph("Dist bw eject and hole(cm): " + txtDistBwEjectAndHole.Text));
                doc.Add(new Paragraph("Hole High(m): " + txtHoleHigh.Text));
                doc.Add(new Paragraph("Begin Time: " + startHour));
                doc.Add(new Paragraph("Recorder: " + txtRecorder.Text));
                doc.Add(new Paragraph("Leader: " + txtLeader.Text));
                doc.Add(new Paragraph("Quality: " + txtQuality.Text));
                doc.Add(new Paragraph("Supervisor: "));

                // Draw Separator
                Paragraph par = new Paragraph(" ");
                par.SetLeading(0.7F, 0.7F);
                Paragraph lineSeparator = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 105.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                lineSeparator.SetLeading(0.5F, 0.5F);
                doc.Add(lineSeparator);
                doc.Add(par);
                doc.Add(lineSeparator);

                // Draw Table 1
                PdfPTable tableHeader = new PdfPTable(5);
                tableHeader.WidthPercentage = 108;
                PdfPCell cellHeader1 = createHeaderCell("Time ", "Min", true);
                tableHeader.AddCell(cellHeader1);
                PdfPCell cellHeader2 = createHeaderCell("Flowrate", "L/Min", false);
                tableHeader.AddCell(cellHeader2);
                PdfPCell cellHeader3 = createHeaderCell("TFluid", "L", false);
                tableHeader.AddCell(cellHeader3);
                PdfPCell cellHeader4 = createHeaderCell("W/C", "W:C", false);
                tableHeader.AddCell(cellHeader4);
                PdfPCell cellHeader5 = createHeaderCell("Pressure", "MPa", false);
                tableHeader.AddCell(cellHeader5);
                doc.Add(tableHeader);
                doc.Add(lineSeparator);

                PdfPTable tableCell = new PdfPTable(5);
                tableCell.WidthPercentage = 108;
                double totalfluid = 0;


                String timeMin = "00:00:01";
                int seconds = config.time_store_db;
                foreach (Data data in listDataReport)
                {
                    totalfluid =  data.fluid;
                    PdfPCell cell1 = createCell(timeMin, false);
                    tableCell.AddCell(cell1);
                    PdfPCell cell2 = createCell(data.flow_rate.ToString(), false);
                    tableCell.AddCell(cell2);
                    PdfPCell cell3 = createCell(data.fluid.ToString(), false);
                    tableCell.AddCell(cell3);
                    PdfPCell cell4 = createCell(data.wc.ToString() + ":1", false);
                    tableCell.AddCell(cell4);
                    PdfPCell cell5 = createCell(data.pressure.ToString(), false);
                    tableCell.AddCell(cell5);

                    TimeSpan timeMinSpan = TimeSpan.FromSeconds(seconds);
                    timeMin = string.Format("{0:D2}:{1:D2}:{2:D2}", timeMinSpan.Hours, timeMinSpan.Minutes, timeMinSpan.Seconds);
                    seconds = seconds + config.time_store_db;
                }
                doc.Add(tableCell);
                doc.Add(lineSeparator);

                // Draw Paragraph 2
                var parEnd = new Paragraph();
                parEnd.Add(new Chunk("End Time: " + endDate));
                parEnd.Add(Chunk.TABBING);
                parEnd.Add(new Chunk(endHour));
                doc.Add(parEnd);
                var parTotalFluid = new Paragraph();
                parTotalFluid.Add(new Chunk("Total Fluid: "));
                parTotalFluid.Add(Chunk.TABBING);
                parTotalFluid.Add(new Chunk(Math.Round(totalfluid, 2) + " L"));
                doc.Add(parTotalFluid);
                var parAshUsed = new Paragraph();
                parAshUsed.Add(new Chunk("Ash used: "));
                parAshUsed.Add(Chunk.TABBING);
                parAshUsed.Add(new Chunk("     " + " Kg"));
                doc.Add(parAshUsed);
                var parAshDiscarded = new Paragraph();
                parAshDiscarded.Add(new Chunk("Ash discarded: "));
                parAshDiscarded.Add(Chunk.TABBING);
                parAshDiscarded.Add(    new Chunk(Math.Round(PLCDB1Read.Instance().cement_total, 2) + " L"));
                doc.Add(parAshDiscarded);
                var parCementDiscarded = new Paragraph();
                parCementDiscarded.Add(new Chunk("Cememt discarded: "));
                parCementDiscarded.Add(Chunk.TABBING);
                parCementDiscarded.Add(new Chunk("     " + " Kg"));
                doc.Add(parCementDiscarded);
                doc.Add(lineSeparator);

                // Draw Table 2
                PdfPTable tableHeader2 = new PdfPTable(5);
                tableHeader2.WidthPercentage = 108;
                PdfPCell cellTable2Header1 = createCellChart(" ", " ", "Max of Y", "Min of Y");
                tableHeader2.AddCell(cellTable2Header1);
                PdfPCell cellTable2Header2 = createCellChart("1", "Flowrate", txtMaxOfYFlowrate.Text, "0");
                tableHeader2.AddCell(cellTable2Header2);
                PdfPCell cellTable2Header3 = createCellChart("2", "TFluid", txtMaxOfYTotalFlow.Text, "0");
                tableHeader2.AddCell(cellTable2Header3);
                PdfPCell cellTable2Header4 = createCellChart("3", "W/C", txtMaxOfYWC.Text, "0");
                tableHeader2.AddCell(cellTable2Header4);
                PdfPCell cellTable2Header5 = createCellChart("4", "Pressure", txtMaxOfYPressure.Text, "0");
                tableHeader2.AddCell(cellTable2Header5);
                doc.Add(tableHeader2);

                // Draw chart
                String imageChartName = @"\Chart" + DateTime.Now.ToString("yyyyMMddhhmmss").ToString() + ".bmp";
                chartTimeCurves.MasterPane.GetImage().Save(path + imageChartName);
                PdfPTable tableChart = new PdfPTable(1);
                tableChart.WidthPercentage = 108;
                PdfPCell cellChart = new PdfPCell();
                cellChart.BorderWidthBottom = 0;
                cellChart.BorderWidthLeft = 0;
                cellChart.BorderWidthTop = 0;
                cellChart.BorderWidthRight = 0;
                cellChart.AddElement(iTextSharp.text.Image.GetInstance(path + imageChartName));
                tableChart.AddCell(cellChart);
                doc.Add(tableChart);

                doc.Close();
            } else
            {
                MessageBox.Show("Có lỗi xảy ra vui lòng thử lại.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private PdfPCell createHeaderCell(String value1, String value2, Boolean flgPadding)
        {
            PdfPCell cellHeader = new PdfPCell();
            if (flgPadding)
            {
                cellHeader = new PdfPCell { PaddingLeft = 10};
            }
            cellHeader.BorderWidthBottom = 0;
            cellHeader.BorderWidthLeft = 0;
            cellHeader.BorderWidthTop = 0;
            cellHeader.BorderWidthRight = 0;


            var par1 = new Paragraph(value1);
            par1.Alignment = Element.ALIGN_CENTER;
            var par2 = new Paragraph(value2);
            par2.Alignment = Element.ALIGN_CENTER;

            cellHeader.AddElement(par1);
            cellHeader.AddElement(par2);

            return cellHeader;
        }
        private PdfPCell createCell(String value1, Boolean flgPadding)
        {
            PdfPCell cell = new PdfPCell();
            if (flgPadding)
            {
                cell = new PdfPCell { PaddingLeft = 10 };
            }
            cell.BorderWidthBottom = 0;
            cell.BorderWidthLeft = 0;
            cell.BorderWidthTop = 0;
            cell.BorderWidthRight = 0;

            var par = new Paragraph(value1);
            par.Alignment = Element.ALIGN_CENTER;
            cell.AddElement(par);

            return cell;
        }
        private PdfPCell createCellChart(String value1, String value2, String value3, String value4)
        {
            PdfPCell cellHeader = new PdfPCell();
            cellHeader.BorderWidthBottom = 0;
            cellHeader.BorderWidthLeft = 0;
            cellHeader.BorderWidthTop = 0;
            cellHeader.BorderWidthRight = 0;
            var parHeader = new Paragraph(value1);
            parHeader.Alignment = Element.ALIGN_CENTER;
            cellHeader.AddElement(parHeader);
            parHeader = new Paragraph(value2);
            parHeader.Alignment = Element.ALIGN_CENTER;
            cellHeader.AddElement(parHeader);
            parHeader = new Paragraph(value3);
            parHeader.Alignment = Element.ALIGN_CENTER;
            cellHeader.AddElement(parHeader);
            parHeader = new Paragraph(value4);
            parHeader.Alignment = Element.ALIGN_CENTER;
            cellHeader.AddElement(parHeader);

            return cellHeader;
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

        private void txtMaxOfYTotalFlow_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtMaxOfYWC_KeyPress(object sender, KeyPressEventArgs e)
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
            //show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().wc_1);
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
     
            if(!debugMode)
            { 
                try
                {
                    if (PLC.Instance().Open())
                    {
                        show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().WC_start);
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
                    show_Data_Real_lb(txtflowrate, Math.Round(PLCDB1Read.Instance().flow_rate, 2));
                    show_Data_Real_lb(txttotalflow, Math.Round(PLCDB1Read.Instance().fluid, 2));
                    show_Data_Real_lb(txtdensi, PLCDB1Read.Instance().wc);
                    show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().wc_1);
                    //show_Data_Real_lb_wc(txtWC, PLCDB1Read.Instance().wc_1);
                    show_Data_Real_lb(txtpressure, Math.Round(PLCDB1Read.Instance().pressure, 2));
                }
                catch (Exception ex)
                {
                
                }
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

        private void timeUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frTimeStore fr = new frTimeStore();
            fr.Show();
        }

        private void form2ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Main2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main2 frMain2 = new Main2();
            frMain2.Show();
        }
    }
}
