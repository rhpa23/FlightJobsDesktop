using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DataVis = System.Windows.Forms.DataVisualization;

namespace FlightJobsDesktop.Views.POC
{
    /// <summary>
    /// Lógica interna para ChartsPoC.xaml
    /// </summary>
    public partial class ChartsPoC : Window
    {
        public ChartsPoC()
        {
            InitializeComponent();

            Chart1.Series[0].Points.Add(3.0).AxisLabel = "Sample data";
            Chart1.Series[0].BorderWidth = 4;
            Chart1.Series[0].LabelBackColor = System.Drawing.Color.White;
            Chart1.Series[0].Font = new System.Drawing.Font("Segoe UI", 12);
            Chart1.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Segoe UI", 12);
            Chart1.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Segoe UI", 12);

            Chart1.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 12);
            Chart1.ChartAreas[0].AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 12);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Chart1.Series[0].ChartType = DataVis.Charting.SeriesChartType.Column;
            //Chart1.Series[0].ChartType = DataVis.Charting.SeriesChartType.Bar;
            //Chart1.Series[0].ChartType = DataVis.Charting.SeriesChartType.Pie;
            Chart1.Series[0].ChartType = DataVis.Charting.SeriesChartType.Line;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //Mar/2023,9105
            //Apr/2023,8136
            //May/2023,10301

            Chart1.Series[0].Points.Clear();

            foreach (var point in TxbData.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                var info = point.Split(new[] { "," }, StringSplitOptions.None);
                var label = info[0];
                var value = double.Parse(info[1]);

                Chart1.Series[0].Points.Add(value).AxisLabel = label;
            }

            //Adding titles
            Chart1.Titles.Clear();
            Chart1.Titles.Add("Título do Chart");
            Chart1.Titles[0].Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold);
            Chart1.ChartAreas[0].AxisX.Title = "Título do eixo X";
            Chart1.ChartAreas[0].AxisY.Title = "Título do eixo Y";

            //Changing colour scheme
            //Chart1.Palette = DataVis.Charting.ChartColorPalette.Chocolate;
            Chart1.Palette = DataVis.Charting.ChartColorPalette.EarthTones;

            //Adding value labels
            Chart1.Series[0].IsValueShownAsLabel = true;

            
        }
    }
}
