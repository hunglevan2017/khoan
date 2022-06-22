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
        private DateTime startDateDate;
        private String startDate;
        private String startHour;
        private String endDate;
        private String endHour;



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
            // init combobox
            cbOrder.Items.Add("I Order");
            cbOrder.SelectedIndex = 0;

            cbRange.Items.Add("Upper");
            cbRange.SelectedIndex = 0;
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
            startDateDate = DateTime.Now;
            startDate = DateTime.Now.ToString("yyyy-MM-dd").ToString();
            startHour = DateTime.Now.ToString("hh:mm:ss tt").ToString();

            lbBeginTimeDate.Text = startDate;
            lbBeginTimeHour.Text = startHour;

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
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lbGroutedTime.Text = (DateTime.Now - startDateDate).Milliseconds.ToString();
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
            endDate = DateTime.Now.ToString("yyyy-MM-dd").ToString();
            endHour = DateTime.Now.ToString("hh:mm:ss tt").ToString();
        }

        private void btnSaveToAs_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnSaveToAs.Enabled = false;
            btnPrint.Enabled = true;

            SaveFileDialog svg = new SaveFileDialog();
            if (svg.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(svg.FileName + ".pdf", FileMode.Create))
                {
                    createPDF(stream, System.IO.Path.GetDirectoryName(svg.FileName));
                }
            }
        }

        private void createPDF(FileStream fs, String path)
        {
            Document doc = new Document(PageSize.A6, 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            doc.Open();

            // Draw Header
            var parHeader = new Paragraph("Grouting Record Report");
            parHeader.Alignment = Element.ALIGN_CENTER;
            doc.Add(parHeader);
            // Draw Paragraph 1
            var parEqui = new Paragraph();
            parEqui.Add(new Chunk("Equipment: " + "Value?"));
            parEqui.Add(Chunk.TABBING);
            parEqui.Add(Chunk.TABBING);
            parEqui.Add(new Chunk(startDate));
            doc.Add(parEqui);
            doc.Add(new Paragraph("Project Name: " + txtProjectName.Text));
            doc.Add(new Paragraph("Grouting Holes Parameters: " +"Value?"));
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
            doc.Add(new Paragraph("Supervisor: " + "Value?"));

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
            PdfPCell cellHeader1 = createHeaderCell("Time", "Min");
            tableHeader.AddCell(cellHeader1);
            PdfPCell cellHeader2 = createHeaderCell("Flowrate", "L/Min");
            tableHeader.AddCell(cellHeader2);
            PdfPCell cellHeader3 = createHeaderCell("TFluid", "L");
            tableHeader.AddCell(cellHeader3);
            PdfPCell cellHeader4 = createHeaderCell("W/C", "W:C");  
            tableHeader.AddCell(cellHeader4);
            PdfPCell cellHeader5 = createHeaderCell("Pressure", "MPa");
            tableHeader.AddCell(cellHeader5);
            doc.Add(tableHeader);
            doc.Add(lineSeparator);

            PdfPTable tableCell = new PdfPTable(5);
            tableCell.WidthPercentage = 108;
            float totalfluid = 0;
            float maxFlowRate = 0;
            float maxfluid = 0;
            foreach (Data data in listData)
            {
                totalfluid = totalfluid + data.fluid;
                if (maxFlowRate < data.flow_rate)
                {
                    maxFlowRate = data.flow_rate;
                }
                if (maxfluid < data.fluid)
                {
                    maxfluid = data.fluid;
                }
                PdfPCell cell1 = createCell("value?");
                tableCell.AddCell(cell1);
                PdfPCell cell2 = createCell(data.flow_rate.ToString());
                tableCell.AddCell(cell2);
                PdfPCell cell3 = createCell(data.fluid.ToString());
                tableCell.AddCell(cell3);
                PdfPCell cell4 = createCell("value?");
                tableCell.AddCell(cell4);
                PdfPCell cell5 = createCell("value?");
                tableCell.AddCell(cell5);
            }
            doc.Add(tableCell);
            doc.Add(lineSeparator);

            // Draw Paragraph 2
            var parEnd = new Paragraph();
            parEnd.Add(new Chunk("End Time: " + endDate));
            parEnd.Add(Chunk.TABBING);
            parEnd.Add(Chunk.TABBING);
            parEnd.Add(new Chunk(endHour));
            doc.Add(parEnd);
            var parTotalFluid = new Paragraph();
            parTotalFluid.Add(new Chunk("Total Fluid: "));
            parTotalFluid.Add(Chunk.TABBING);
            parTotalFluid.Add(new Chunk(totalfluid.ToString() + " L"));
            doc.Add(parTotalFluid);
            var parAshUsed = new Paragraph();
            parAshUsed.Add(new Chunk("Ash used: "));
            parAshUsed.Add(Chunk.TABBING);
            parAshUsed.Add(new Chunk("value?" + " Kg"));
            doc.Add(parAshUsed);
            var parAshDiscarded = new Paragraph();
            parAshDiscarded.Add(new Chunk("Ash discarded: "));
            parAshDiscarded.Add(Chunk.TABBING);
            parAshDiscarded.Add(new Chunk("value?" + " L"));
            doc.Add(parAshDiscarded);
            var parCementDiscarded = new Paragraph();
            parCementDiscarded.Add(new Chunk("Cememt discarded: "));
            parCementDiscarded.Add(Chunk.TABBING);
            parCementDiscarded.Add(new Chunk("value?" + " Kg"));
            doc.Add(parCementDiscarded);
            doc.Add(lineSeparator);

            // Draw Table 2
            PdfPTable tableHeader2 = new PdfPTable(5);
            tableHeader2.WidthPercentage = 108;
            PdfPCell cellTable2Header1 = createCellChart(" ", " ", "Max of Y", "Min of Y");
            tableHeader2.AddCell(cellTable2Header1);
            PdfPCell cellTable2Header2 = createCellChart("1", "Flowrate", maxFlowRate.ToString(), "0");
            tableHeader2.AddCell(cellTable2Header2);
            PdfPCell cellTable2Header3 = createCellChart("2", "TFluid", maxfluid.ToString(), "0");
            tableHeader2.AddCell(cellTable2Header3);
            PdfPCell cellTable2Header4 = createCellChart("3", "W/C", "value?", "0");
            tableHeader2.AddCell(cellTable2Header4);
            PdfPCell cellTable2Header5 = createCellChart("4", "Pressure", "value?", "0");
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
        }
        private PdfPCell createHeaderCell(String value1, String value2)
        {
            PdfPCell cellHeader = new PdfPCell();
            cellHeader.BorderWidthBottom = 0;
            cellHeader.BorderWidthLeft = 0;
            cellHeader.BorderWidthTop = 0;
            cellHeader.BorderWidthRight = 0;
            cellHeader.AddElement(new Paragraph(value1));
            cellHeader.AddElement(new Paragraph(value2));

            return cellHeader;
        }
        private PdfPCell createCell(String value1)
        {
            PdfPCell cell = new PdfPCell();
            cell.BorderWidthBottom = 0;
            cell.BorderWidthLeft = 0;
            cell.BorderWidthTop = 0;
            cell.BorderWidthRight = 0;
            cell.AddElement(new Paragraph(value1));

            return cell;
        }
        private PdfPCell createCellChart(String value1, String value2, String value3, String value4)
        {
            PdfPCell cellHeader = new PdfPCell();
            cellHeader.BorderWidthBottom = 0;
            cellHeader.BorderWidthLeft = 0;
            cellHeader.BorderWidthTop = 0;
            cellHeader.BorderWidthRight = 0;
            cellHeader.AddElement(new Paragraph(value1));
            cellHeader.AddElement(new Paragraph(value2));
            cellHeader.AddElement(new Paragraph(value3));
            cellHeader.AddElement(new Paragraph(value4));

            return cellHeader;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

        }
    }
}
