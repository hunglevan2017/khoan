using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace KhoanNhaTrang
{
    public partial class Test : Form
    {
        private GraphPane graphPane;

        public Test()
        {
            InitializeComponent();

            double x = 27.234f / (5.9f*8.75f);
            MessageBox.Show(""+x);


            graphPane = zedGraphControl1.GraphPane;



            graphPane.YAxis.Scale.FontSpec.Size = 24;

            graphPane.XAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MajorGrid.IsVisible = true;


            graphPane.XAxis.MajorGrid.DashOn = 10.0F;
            graphPane.YAxis.MajorGrid.DashOn = 10.0F;


            graphPane.YAxis.Title.IsVisible = false;
            graphPane.XAxis.Title.IsVisible = false;
            graphPane.Title.IsVisible = false;

            graphPane.Border.IsVisible = false;
            graphPane.Legend.IsVisible = false;
            graphPane.Title.IsVisible = false;


            graphPane.YAxis.Scale.Min = 0;
            graphPane.YAxis.Scale.Max = 100;
            graphPane.YAxis.Scale.MinorStep = 1;
            graphPane.YAxis.Scale.MajorStep = 100;



            LineItem lineGraph = graphPane.CurveList[0] as LineItem;
            lineGraph.Line.Width = 3F;

            IPointListEdit pointListEdit = lineGraph.Points as IPointListEdit;
            pointListEdit.Add(0, 0);
            pointListEdit.Add(57, 4);

           // zedGraphControl1.AxisChange();
            // Force a redraw
           // zedGraphControl1.Invalidate();
            //zedGraphControl1.Refresh();




            //zedGraphControl1.ZoomPane(graphPane, 0.9, centrePoint, false);

        }
    }
}
