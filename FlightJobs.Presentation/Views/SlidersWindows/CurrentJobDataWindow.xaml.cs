using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Infrastructure;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        private const double TARGET_WIDTH = 190;
        private const int SECONDS_TO_CLOSE = 180;
        private CurrentJobViewModel _currentJobViewModel; 

        public CurrentJobDataWindow(CurrentJobViewModel currentJobView)
        {
            InitializeComponent();
            _hideTimer.Tick += HideTimer_Tick;
            _hideTimer.Interval = new TimeSpan(0, 0, SECONDS_TO_CLOSE);
            _currentJobViewModel = currentJobView;
        }

        internal void ToggleSlider(bool toShow, int secondsToClose = SECONDS_TO_CLOSE)
        {
            var widthValue = toShow ? TARGET_WIDTH : 2;
            DoubleAnimation sliderAnimation = new DoubleAnimation(widthValue, new Duration(TimeSpan.FromSeconds(0.3)));
            this.BeginAnimation(WidthProperty, sliderAnimation);
            if (toShow)
            {
                _hideTimer.Interval = new TimeSpan(0, 0, secondsToClose);
                _hideTimer.Start();
            }
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            ToggleSlider(false);
            _hideTimer.Stop();
        }

        private void HideIco_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ToggleSlider(this.Width < 10);
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

        }

        private void BtnCloseResults_Click(object sender, RoutedEventArgs e)
        {
            Flyout f = FlyoutService.GetFlyout(BtnShowFlightResults) as Flyout;
            if (f != null)
            {
                f.Hide();
            }
        }
    }
}
