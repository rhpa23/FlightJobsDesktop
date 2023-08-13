using FlightJobsDesktop.Common;
using FlightJobsDesktop.ViewModels;
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
        private LogbookUserJobViewModel _logbookUserJob;

        public JobDetailModal(LogbookUserJobViewModel logbookUserJob)
        {
            _logbookUserJob = logbookUserJob;

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
                WindowsChartArea.Visibility = Visibility.Visible;
                RouteMapPanel.Visibility = Visibility.Collapsed;

                FlightRecorderUtil.UpdateChartVerticalProfile(ChartFlightRecorder);
                DataContext = new FlightRecorderAnaliseViewModel()
                {
                    AverageFuelConsumption = FlightRecorderUtil.GetAverageFuelConsumption(_logbookUserJob.Dist),
                    AveragePlaneSpeed = FlightRecorderUtil.GetAverageSpeed()
                };
            }
            else if (radioButton.Name == RadioSpeed.Name)
            {
                WindowsChartArea.Visibility = Visibility.Visible;
                RouteMapPanel.Visibility = Visibility.Collapsed;

                FlightRecorderUtil.UpdateChartSpeed(ChartFlightRecorder);
            }
            else if (radioButton.Name == RadioFuel.Name)
            {
                WindowsChartArea.Visibility = Visibility.Visible;
                RouteMapPanel.Visibility = Visibility.Collapsed;

                FlightRecorderUtil.UpdateChartFuel(ChartFlightRecorder);
            }
            else if (radioButton.Name == RadioRoteMap.Name)
            {
                WindowsChartArea.Visibility = Visibility.Collapsed;
                RouteMapPanel.Visibility = Visibility.Visible;

                var htmlText = FlightRecorderUtil.GetRouteMapHtmlText();

                RouteMapWebView.NavigateToString(htmlText);
            }
            else if (radioButton.Name == RadioFps.Name)
            {
                WindowsChartArea.Visibility = Visibility.Visible;
                RouteMapPanel.Visibility = Visibility.Collapsed;

                FlightRecorderUtil.UpdateChartFPS(ChartFlightRecorder);
            }
        }
    }
}
