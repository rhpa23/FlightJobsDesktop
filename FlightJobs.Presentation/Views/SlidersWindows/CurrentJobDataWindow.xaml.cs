using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Connect.MSFS.SDK.Model;
using FlightJobs.Infrastructure;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Common;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using log4net;
using ModernWpf.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Color = System.Drawing.Color;

namespace FlightJobsDesktop.Views.SlidersWindows
{
    /// <summary>
    /// Lógica interna para CurrentJobDataWindow.xaml
    /// </summary>
    public partial class CurrentJobDataWindow : Window
    {
        #region Avoid getting the focus
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, -20,
                GetWindowLong(helper.Handle, -20) | 0x08000000);
        }
        #endregion

        DispatcherTimer _hideTimer = new DispatcherTimer();
        private bool _isResultsOpen = false;
        private const double TARGET_WIDTH = 190;
        private const double TARGET_RESULTS_WIDTH = 820;
        private const int SECONDS_TO_CLOSE = 180;
        private CurrentJobViewModel _currentJobViewModel;
        private static readonly ILog _log = LogManager.GetLogger(typeof(CurrentJobDataWindow));

        public CurrentJobDataWindow(CurrentJobViewModel currentJobView)
        {
            InitializeComponent();
            _hideTimer.Tick += HideTimer_Tick;
            _hideTimer.Interval = new TimeSpan(0, 0, SECONDS_TO_CLOSE);
            _currentJobViewModel = CurrentJobViewModel.Copy(currentJobView);

            var chartFont = new Font("Segoe UI", 10);
            //ChartFlightRecorder.Series[0].Font = chartFont;

            ChartFlightRecorder.ChartAreas[0].AxisX.LabelStyle.Font = chartFont;
            ChartFlightRecorder.ChartAreas[0].AxisY.LabelStyle.Font = chartFont;
            ChartFlightRecorder.BackColor = ColorTranslator.FromHtml("#FF2B2B2B");
            ChartFlightRecorder.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            ChartFlightRecorder.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
            ChartFlightRecorder.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
            ChartFlightRecorder.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;
            ChartFlightRecorder.ChartAreas[0].BackColor = Color.Black;
            ChartFlightRecorder.ChartAreas[0].AxisX.TitleForeColor = Color.White;
            ChartFlightRecorder.ChartAreas[0].AxisY.TitleForeColor = Color.White;
        }

        internal void ToggleSlider(bool toShow, int secondsToClose = SECONDS_TO_CLOSE)
        {
            var widthValue = toShow ? TARGET_WIDTH : 2;
            DoubleAnimation sliderAnimation = new DoubleAnimation(widthValue, new Duration(TimeSpan.FromSeconds(0.3)));
            BeginAnimation(WidthProperty, sliderAnimation);
            if (toShow)
            {
                _hideTimer.Interval = new TimeSpan(0, 0, secondsToClose);
                _hideTimer.Start();
            }
        }

        internal void ToggleResultsSlider(bool toShow)
        {
            var widthValue = toShow ? TARGET_RESULTS_WIDTH : TARGET_WIDTH;
            DoubleAnimation sliderAnimation = new DoubleAnimation(widthValue, new Duration(TimeSpan.FromSeconds(0.3)));
            BeginAnimation(WidthProperty, sliderAnimation);
            _isResultsOpen = toShow;
            ResultAreaBorder.Visibility = toShow ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            ToggleSlider(false);
            _hideTimer.Stop();
            _isResultsOpen = false;
        }

        private void HideIco_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isResultsOpen)
            {
                ToggleSlider(this.Width < 10);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
            if (currentJob != null)
            {
                _currentJobViewModel.PlaneSimData = FlightJobsConnectSim.PlaneSimData;
                _currentJobViewModel.SimData = FlightJobsConnectSim.CommonSimData;
                DataContext = _currentJobViewModel;
            }

#if DEBUG
            FlightRecorderUtil.FlightRecorderList =
                FlightRecorderUtil.LoadFlightRecorderFile(new CurrentJobViewModel() { Id = 71606, DepartureICAO = "LIMJ", ArrivalICAO = "LIRF" });
#endif
        }

        private void BtnShowFlightResults_Click(object sender, RoutedEventArgs e)
        {
            _hideTimer.Interval = new TimeSpan(0, 5, 0);
            FlightRecorderUtil.FlightRecorderList = FlightRecorderUtil.LoadFlightRecorderFile(_currentJobViewModel);
            ToggleResultsSlider(this.Width <= TARGET_WIDTH);
        }
        private void BtnCloseResults_Click(object sender, RoutedEventArgs e)
        {
            ToggleResultsSlider(false);
        }

        private void TabControlResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RadioAltitude.IsChecked = true;
        }

        private void ChartTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _hideTimer.Interval = new TimeSpan(0, 5, 0);
            if (_isResultsOpen)
            {
                var radioButton = sender as RadioButton;
                if (radioButton == null)
                    return;

                if (radioButton.Name == RadioAltitude.Name)
                {
                    WindowsChartArea.Visibility = Visibility.Visible;
                    RouteMapPanel.Visibility = Visibility.Collapsed;

                    FlightRecorderUtil.UpdateChartVerticalProfile(ChartFlightRecorder);

                    _currentJobViewModel.FlightRecorderAnalise.AverageFuelConsumption = 
                        FlightRecorderUtil.GetAverageFuelConsumption(_currentJobViewModel.Dist);
                    
                    _currentJobViewModel.FlightRecorderAnalise.AveragePlaneSpeed = FlightRecorderUtil.GetAverageSpeed();
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
}
