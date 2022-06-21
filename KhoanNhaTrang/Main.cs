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

        private void sampleConnectDB()
        {
            try
            {
                db.Open();
                var data = new Data();
                data.flow_rate = 30f;
                data.fluid = 25f;
                string query = @"insert into data(flow_rate, fluid) values(@flow_rate, @fluid);
                            SELECT LAST_INSERT_ID()";
                long id = db.Query<int>(query, data).Single();
                data.Id = id;
                MessageBox.Show("ID:" + data.Id);

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

        private void Form1_Load(object sender, EventArgs e)
        {
            // khi khởi động sẽ được chạy
            sampleConnectDB();
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
            btnSaveToAs.Enabled = true;
            btnPrint.Enabled = false;

            timer1.Stop();
            timer1.Enabled = false;
        }

        private void btnSaveToAs_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnSaveToAs.Enabled = false;
            btnPrint.Enabled = true;

            chartTimeCurves.MasterPane.GetImage().Save(@"D:\Download\test.bmp");

            System.IO.FileStream fs = new FileStream(@"D:\Download\First PDF document.pdf", FileMode.Create);
            Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            doc.Open();
            doc.Add(iTextSharp.text.Image.GetInstance(@"D:\Download\test.bmp"));

            PdfPTable table1 = new PdfPTable(2);
            table1.DefaultCell.Border = 0;
            table1.WidthPercentage = 80;

            //var titleFont = new Font(Font.FontFamily, 24);
            //var subTitleFont = new Font(Font.FontFamily, 16);

            PdfPCell cell11 = new PdfPCell();
            cell11.Colspan = 1;
            //cell11.AddElement(new Paragraph("ABC Traders Receipt", titleFont));

           // cell11.AddElement(new Paragraph("Thankyou for shoping at ABC traders,your order details are below", subTitleFont));


            cell11.VerticalAlignment = Element.ALIGN_LEFT;

            PdfPCell cell12 = new PdfPCell();


            cell12.VerticalAlignment = Element.ALIGN_CENTER;


            table1.AddCell(cell11);

            table1.AddCell(cell12);


            PdfPTable table2 = new PdfPTable(3);

            //One row added

            PdfPCell cell21 = new PdfPCell();

            cell21.AddElement(new Paragraph("Photo Type"));

            PdfPCell cell22 = new PdfPCell();

            cell22.AddElement(new Paragraph("No. of Copies"));

            PdfPCell cell23 = new PdfPCell();

            cell23.AddElement(new Paragraph("Amount"));


            table2.AddCell(cell21);

            table2.AddCell(cell22);

            table2.AddCell(cell23);


            //New Row Added

            PdfPCell cell31 = new PdfPCell();

            cell31.AddElement(new Paragraph("Safe"));

            cell31.FixedHeight = 300.0f;

            PdfPCell cell32 = new PdfPCell();

            cell32.AddElement(new Paragraph("2"));

            cell32.FixedHeight = 300.0f;

            PdfPCell cell33 = new PdfPCell();

            cell33.AddElement(new Paragraph("20.00 * " + "2" + " = " + (20 * Convert.ToInt32("2")) + ".00"));

            cell33.FixedHeight = 300.0f;



            table2.AddCell(cell31);

            table2.AddCell(cell32);

            table2.AddCell(cell33);


            PdfPCell cell2A = new PdfPCell(table2);

            cell2A.Colspan = 2;

            table1.AddCell(cell2A);

            PdfPCell cell41 = new PdfPCell();

            cell41.AddElement(new Paragraph("Name : " + "ABC"));

            cell41.AddElement(new Paragraph("Advance : " + "advance"));

            cell41.VerticalAlignment = Element.ALIGN_LEFT;

            PdfPCell cell42 = new PdfPCell();

            cell42.AddElement(new Paragraph("Customer ID : " + "011"));

            cell42.AddElement(new Paragraph("Balance : " + "3993"));

            cell42.VerticalAlignment = Element.ALIGN_RIGHT;


            table1.AddCell(cell41);

            table1.AddCell(cell42);


            doc.Add(table1);

            doc.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

        }
    }
}
