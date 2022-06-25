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
        long managementId = 0;
        private String startDate;
        private String startHour;
        private String endDate;
        private String endHour;
        private GraphPane myPane;

        public Form1()
        {
            InitializeComponent();
        }
        //Dapper 
        //https://hanhtranglaptrinh.net/dapper-c-la-gi-micro-orm-trong-net/
        static string connStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
        static IDbConnection db = new MySqlConnection(connStr);

        private Data insertDB()
        {
            var data = new Data();
            try
            {
         
                db.Open();
                data.flow_rate = Math.Round(PLCDB1Read.Instance().flow_rate,2) ;
                data.fluid = Math.Round(PLCDB1Read.Instance().fluid,2);
                data.pressure = Math.Round(PLCDB1Read.Instance().pressure,2);
                data.wc = Math.Round(PLCDB1Read.Instance().wc,2);
                data.management_id = managementId;
                data.insert_date = new DateTime();

                string query = @"insert into data(flow_rate, fluid,pressure,wc,management_id) values(@flow_rate, @fluid,@pressure,@wc,@management_id);
                            SELECT LAST_INSERT_ID()";
                long id = db.Query<int>(query, data).Single();
                data.Id = id;
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
            if (PLC.Instance().Open())
            {

            }
            else
            {
                MessageBox.Show(" PLC not connected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


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


            // Định hiện thị cho trục thời gian (Trục X)
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 60;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.XAxis.MajorGrid.IsVisible = true;
            // Định hiện thị cho trục thời gian(Trục Y)
            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.Max = 100;
            myPane.YAxis.Scale.MinorStep = 1;
            myPane.YAxis.Scale.MajorStep = 10;
            myPane.YAxis.MajorGrid.IsVisible = true;


            // Gọi hàm xác định cỡ trục
            myPane.AxisChange();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (txtMaxOfYFlowrate.Text != null && !"".Equals(txtMaxOfYFlowrate.Text.Trim())
                && txtMaxOfYTotalFlow.Text != null && !"".Equals(txtMaxOfYTotalFlow.Text.Trim())
                && txtMaxOfYWC.Text != null && !"".Equals(txtMaxOfYWC.Text.Trim())
                && txtMaxOfYPressure.Text != null && !"".Equals(txtMaxOfYPressure.Text.Trim()))
            {
                if (tickStart == 0)
                {
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
                
                txtMaxOfYFlowrate.ReadOnly = true;
                txtMaxOfYTotalFlow.ReadOnly = true;
                txtMaxOfYWC.ReadOnly = true;
                txtMaxOfYPressure.ReadOnly = true;

                btnStart.Enabled = false;
                btnPause.Enabled = true;
                btnEnd.Enabled = true;
                timer1.Enabled = true;
                timer1.Start();
            } else
            {
                MessageBox.Show("Các trường Max of Y của Flowrate, Total flow, W/C, Pressure không được rỗng.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void show_Data_Real_lb(TextBox lb, double value)
        {

            lb.Text = (value / 1).ToString("0.00");

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                /**/
                PLC.Instance().ReadClass(PLCDB1Read.Instance(), 1);
                Data data = insertDB();
                show_Data_Real_lb(txtFlowrate, data.flow_rate);
                show_Data_Real_lb(txtTotalFlow, data.fluid);
                show_Data_Real_lb(txtWC, data.wc);
                show_Data_Real_lb(txtPressure, data.pressure);

                double valueFlowRate = ((data.flow_rate * 100) / Convert.ToDouble(txtMaxOfYFlowrate.Text));
                double valueFluid = ((data.fluid * 100) / Convert.ToDouble(txtMaxOfYTotalFlow.Text));
                double valueWC = ((data.wc * 100) / Convert.ToDouble(txtMaxOfYWC.Text));
                double valuePressure = ((data.pressure * 100) / Convert.ToDouble(txtMaxOfYPressure.Text));
                
                Draw(valueFlowRate, valueFluid, valueWC, valuePressure);
                listData.Add(data);
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
            curveFlowRate.Line.Width = 3.0F;
            LineItem curveFluid = chartTimeCurves.GraphPane.CurveList[1] as LineItem;
            curveFluid.Line.Width = 3.0F;
            LineItem curveWC = chartTimeCurves.GraphPane.CurveList[2] as LineItem;
            curveWC.Line.Width = 3.0F;
            LineItem curvePressure = chartTimeCurves.GraphPane.CurveList[3] as LineItem;
            curvePressure.Line.Width = 3.0F;

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
                xScale.Max = tickStart + xScale.MajorStep;
                xScale.Min = 0;
            }
            int seconds = tickStart;

            TimeSpan timeSpan = TimeSpan.FromSeconds(tickStart);
            string groutedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            lbGroutedTime.Text = groutedTime;
            tickStart = tickStart + 5;

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
            btnSaveToAs.Enabled = true;
            btnPrint.Enabled = true;
            timer1.Stop();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
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
            btnSaveToAs.Enabled = true;
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

        private void btnPrint_Click(object sender, EventArgs e)
        {

        }

        private void createPDF(FileStream fs, String path)
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
            doc.Add(new Paragraph("Grouting Holes Parameters: " ));
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
            foreach (Data data in listData)
            {
                totalfluid = totalfluid + data.fluid;

                PdfPCell cell1 = createCell(data.insert_date.ToString("HH:mm:ss"), false);
                
                tableCell.AddCell(cell1);
                PdfPCell cell2 = createCell(data.flow_rate.ToString(), false);
                tableCell.AddCell(cell2);
                PdfPCell cell3 = createCell(data.fluid.ToString(), false);
                tableCell.AddCell(cell3);
                PdfPCell cell4 = createCell(data.wc.ToString(), false);
                tableCell.AddCell(cell4);
                PdfPCell cell5 = createCell(data.pressure.ToString(), false);
                tableCell.AddCell(cell5);
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
            parTotalFluid.Add(new Chunk(totalfluid.ToString() + " L"));
            doc.Add(parTotalFluid);
            var parAshUsed = new Paragraph();
            parAshUsed.Add(new Chunk("Ash used: "));
            parAshUsed.Add(Chunk.TABBING);
            parAshUsed.Add(new Chunk("     " + " Kg"));
            doc.Add(parAshUsed);
            var parAshDiscarded = new Paragraph();
            parAshDiscarded.Add(new Chunk("Ash discarded: "));
            parAshDiscarded.Add(Chunk.TABBING);
            parAshDiscarded.Add(new Chunk("     " + " L"));
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


    }
}
