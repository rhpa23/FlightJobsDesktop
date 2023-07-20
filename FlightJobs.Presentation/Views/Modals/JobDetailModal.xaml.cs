using FlightJobsDesktop.Common;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para JobDetailModal.xam
    /// </summary>
    public partial class JobDetailModal : UserControl
    {
        public JobDetailModal()
        {
            InitializeComponent();

            var chartFont = new Font("Segoe UI", 10);
            ChartFlightRecorder.Series[0].Font = chartFont;

            ChartFlightRecorder.ChartAreas[0].AxisX.LabelStyle.Font = chartFont;
            ChartFlightRecorder.ChartAreas[0].AxisY.LabelStyle.Font = chartFont;

            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
            {
                ChartFlightRecorder.BackColor = ColorTranslator.FromHtml("#FF2B2B2B");
                ChartFlightRecorder.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                ChartFlightRecorder.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
                ChartFlightRecorder.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
                ChartFlightRecorder.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;
                ChartFlightRecorder.ChartAreas[0].BackColor = Color.Black;
                ChartFlightRecorder.ChartAreas[0].AxisX.TitleForeColor = Color.White;
                ChartFlightRecorder.ChartAreas[0].AxisY.TitleForeColor = Color.White;
            }
        }

        private void ChartTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            if (radioButton.Name == RadioAltitude.Name)
            {
                FlightRecorderUtil.UpdateChartVerticalProfile(ChartFlightRecorder);
            }
            else if (radioButton.Name == RadioSpeed.Name)
            {
                FlightRecorderUtil.UpdateChartSpeed(ChartFlightRecorder);
            }
            else if (radioButton.Name == RadioFuel.Name)
            {
                FlightRecorderUtil.UpdateChartFuel(ChartFlightRecorder);
            }
        }
    }
}
