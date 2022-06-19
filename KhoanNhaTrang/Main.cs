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
using System.Diagnostics;

namespace KhoanNhaTrang
{

    public partial class Form1 : Form
    {
        private List<Data> listData = new List<Data>();
        private int index = 0;
        int tickStart = 0;

        public Form1()
        {
            InitializeComponent();
        }
        //Dapper 
        //https://hanhtranglaptrinh.net/dapper-c-la-gi-micro-orm-trong-net/
        static string connStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
        static IDbConnection db = new MySqlConnection(connStr);

        private void Form1_Load(object sender, EventArgs e)
        {
            // khi khởi động sẽ được chạy

            // init button
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnSaveToAs.Enabled = false;
            btnPrint.Enabled = false;
            btnAddInfo.Enabled = false;
            btnClose.Enabled = false;

            // Khai báo sử dụng Graph loại GraphPane;
            GraphPane myPane = chartTimeCurves.GraphPane;
            myPane.Title.Text = "EM Grouting Curves";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Percent";

            //Định nghĩa list để vẽ đồ thị.
            RollingPointPairList list1 = new RollingPointPairList(60000); // Sử dụng list với 60000 điểm
            LineItem curve1 = myPane.AddCurve("Flowrate", list1, Color.Red, SymbolType.None); // SymbolType là kiểu biểu thị đồ thị : điểm, đường tròn, tam giác....
            RollingPointPairList list2 = new RollingPointPairList(60000);
            LineItem curve2 = myPane.AddCurve("Total flow", list2, Color.Blue, SymbolType.None);

            // Định hiện thị cho trục thời gian (Trục X)
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            // Định hiện thị cho trục thời gian(Trục Y)
            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.Max = 100;
            myPane.YAxis.Scale.MinorStep = 1;
            myPane.YAxis.Scale.MajorStep = 10;

            // Gọi hàm xác định cỡ trục
            myPane.AxisChange();

            // Khởi động timer về vị trí ban đầu
            tickStart = Environment.TickCount;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnEnd.Enabled = true;
            timer1.Enabled = true;
            timer1.Start();

            try
            {
                db.Open();
                string query = @"SELECT flow_rate, fluid FROM grouting.data";
                listData = db.Query<Data>(query).ToList();
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

                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Draw(listData[index].flow_rate, listData[index].fluid);
                index++;
                txtMaxOfYFlowrate.Text = listData[index].flow_rate.ToString();
                txtMaxOfYTotalFlow.Text = listData[index].fluid.ToString();
                //Debug.WriteLine("flow_rate : " + listData[index].flow_rate + " - fluid : "  + listData[index].fluid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Draw(float flowRate, float fluid)
        {

            if (chartTimeCurves.GraphPane.CurveList.Count <= 0)
                return;

            // Đưa về điểm xuất phát
            LineItem curve1 = chartTimeCurves.GraphPane.CurveList[0] as LineItem;
            LineItem curve2 = chartTimeCurves.GraphPane.CurveList[1] as LineItem;

            if (curve1 == null)
                return;

            if (curve2 == null)
                return;

            IPointListEdit list1 = curve1.Points as IPointListEdit;
            IPointListEdit list2 = curve2.Points as IPointListEdit;

            if (list1 == null)
                return;

            if (list1 == null)
                return;

            // Time được tính bằng ms
            double time = (Environment.TickCount - tickStart) / 1000.0;

            // Thêm điểm trên đồ thị
            list1.Add(time, flowRate);
            list2.Add(time, fluid);

            // Đoạn chương trình thực hiện vẽ đồ thị
            Scale xScale = chartTimeCurves.GraphPane.XAxis.Scale;
            Scale yScale = chartTimeCurves.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (time > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = time + xScale.MajorStep;
                xScale.Min = 0;
            }

            //// Tự động Scale theo trục y
            //if (value > yScale.Max - yScale.MajorStep)
            //{
            //    yScale.Max = value + yScale.MajorStep;
            //}
            //else if (value < yScale.Min + yScale.MajorStep)
            //{
            //    yScale.Min = value - yScale.MajorStep;
            //}

            // Vẽ đồ thị
            chartTimeCurves.AxisChange();
            // Force a redraw
            chartTimeCurves.Invalidate();
            chartTimeCurves.Refresh();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = true;
            timer1.Stop();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            timer1.Stop();
            timer1.Enabled = false;
        }
    }
}
