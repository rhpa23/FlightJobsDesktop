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
        private const double TARGET_RESULTS_WIDTH = 900;
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
            var widthValue = toShow ? TARGET_WIDTH : 4;
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
            // MOCK FOR DESIGN VISUALIZATION (Temporary testing)
            /*if (_currentJobViewModel != null)
            {
                if (_currentJobViewModel.PlaneSimData == null)
                {
                    _currentJobViewModel.PlaneSimData = new PlaneModel();
                }
                _currentJobViewModel.PlaneSimData.TouchdownRunwayLength = 2300;
                _currentJobViewModel.PlaneSimData.TouchdownThresholdDistance = 455;
                _currentJobViewModel.PlaneSimData.TouchdownFpm = -150;
                _currentJobViewModel.PlaneSimData.TouchdownGForce = 1.2;
            }*/

            if (_currentJobViewModel == null || _currentJobViewModel.PlaneSimData == null)
            {
                _log.Warn("BtnShowFlightResults_Click: _currentJobViewModel or PlaneSimData is null.");
                return;
            }

            AngularGaugeTouchdownFpm.Value = Math.Abs(_currentJobViewModel.PlaneSimData.TouchdownFpm);
            AngularGaugeGForce.Value = _currentJobViewModel.PlaneSimData.TouchdownGForce;

            var runwayLength = _currentJobViewModel.PlaneSimData.TouchdownRunwayLength;
            double touchdownRunwayLengthMaxLandZone;
            if (runwayLength < 800)
                touchdownRunwayLengthMaxLandZone = 150;
            else if (runwayLength <= 1200)
                touchdownRunwayLengthMaxLandZone = 250;
            else if (runwayLength <= 2400)
                touchdownRunwayLengthMaxLandZone = 300;
            else
                touchdownRunwayLengthMaxLandZone = 400;

            var touchdownZoneLenght = 350;

            if (runwayLength > 0)
            {
                LblValMax.Text = runwayLength.ToString("F0");
                var greenTopValue = touchdownRunwayLengthMaxLandZone + touchdownZoneLenght;
                var greenBottomValue = touchdownRunwayLengthMaxLandZone;
                LblValGreenTop.Text = greenTopValue.ToString("F0");
                LblValGreenBottom.Text = greenBottomValue.ToString("F0");

                // Calculate heights of the zones
                double topRedHeight = runwayLength - greenTopValue;
                double greenHeight = touchdownZoneLenght;
                double bottomRedHeight = greenBottomValue;

                // Protect against values <= 0
                if (topRedHeight < 0) topRedHeight = 0;
                if (greenHeight < 0) greenHeight = 0;
                if (bottomRedHeight < 0) bottomRedHeight = 0;

                // Set Grid Row heights
                RowTopRed.Height = new GridLength(topRedHeight, GridUnitType.Star);
                RowGreen.Height = new GridLength(greenHeight, GridUnitType.Star);
                RowBottomRed.Height = new GridLength(bottomRedHeight, GridUnitType.Star);

                // Position left labels vertically (canvas height is 180)
                double topPosGreenTop = 180.0 * (1.0 - greenTopValue / runwayLength) - 8.0;
                double topPosGreenBottom = 180.0 * (1.0 - greenBottomValue / runwayLength) - 8.0;

                Canvas.SetTop(LblValGreenTop, topPosGreenTop);
                Canvas.SetTop(LblValGreenBottom, topPosGreenBottom);

                // Position the current value indicator line and update its label
                double currentVal = _currentJobViewModel.PlaneSimData.TouchdownThresholdDistance;
                LblCurrentDistance.Text = currentVal.ToString("F0") + "m";

                double clampedVal = Math.Max(0.0, Math.Min(runwayLength, currentVal));
                double topPosIndicator = 180.0 * (1.0 - clampedVal / runwayLength) - 10.0;
                Canvas.SetTop(IndicatorGroup, topPosIndicator);
            }

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
                        FlightRecorderUtil.GetAverageFuelConsumption(_currentJobViewModel.Distance);
                    
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

                    InitializeAndNavigateRouteMap(htmlText);
                }
                else if (radioButton.Name == RadioFps.Name)
                {
                    WindowsChartArea.Visibility = Visibility.Visible;
                    RouteMapPanel.Visibility = Visibility.Collapsed;

                    FlightRecorderUtil.UpdateChartFPS(ChartFlightRecorder);
                }
            }
        }

        private async void InitializeAndNavigateRouteMap(string htmlText)
        {
            try
            {
                // Inicializa o WebView2 de forma assíncrona
                await RouteMapWebView.EnsureCoreWebView2Async(null);

                // Navega para o conteúdo HTML
                RouteMapWebView.NavigateToString(htmlText);
            }
            catch (Exception ex)
            {
                _log.Error($"Erro ao inicializar WebView2 para o mapa de rota: {ex.Message}", ex);
            }
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
