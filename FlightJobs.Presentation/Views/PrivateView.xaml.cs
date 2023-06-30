using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using log4net;
using Microsoft.Win32;
using ModernWpf;
using Notification.Wpf;
using ScottPlot;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class PrivateView : UserControl
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PrivateView));
        private NotificationManager _notificationManager;

        private UserStatisticsFlightsViewModel _statisticsView = new UserStatisticsFlightsViewModel();

        private IPilotService _pilotService;

        public PrivateView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _pilotService = MainWindow.PilotServiceFactory.Create();

            var chartFont = new Font("Segoe UI", 10);
            ChartBankBalanceMonth.Series[0].Font = chartFont;
            ChartBankBalanceMonth.Series[0].LabelFormat = "F{0:C}";

            ChartBankBalanceMonth.ChartAreas[0].AxisX.LabelStyle.Font = chartFont;
            ChartBankBalanceMonth.ChartAreas[0].AxisY.LabelStyle.Font = chartFont;

            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
            {
                ChartBankBalanceMonth.BackColor = ColorTranslator.FromHtml("#FF2B2B2B");
                ChartBankBalanceMonth.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                ChartBankBalanceMonth.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
                ChartBankBalanceMonth.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
                ChartBankBalanceMonth.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;
                ChartBankBalanceMonth.ChartAreas[0].BackColor = Color.Black;
            }
        }

        private async Task LoadPilotData()
        {
            BtnLicenseBorder.IsEnabled = false;
            BtnTransferBorder.IsEnabled = false;
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            try
            {
                var userStatisticsModel = await _pilotService.GetUserStatisticsFlightsInfo(AppProperties.UserLogin.UserId);
                _statisticsView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                            .Map<UserStatisticsModel, UserStatisticsFlightsViewModel>(userStatisticsModel);

                _statisticsView.LicenseStatus = _statisticsView.LicensesOverdue?.Count > 0 ? "License is expired" : "License is up to date";

                AppProperties.UserStatistics.LicensesOverdue.Clear();
                AppProperties.UserStatistics.LicensesOverdue = userStatisticsModel.LicensesOverdue.ToList();
                MainWindow.SetLicenseOverdueEllipseVisibility();

                foreach (var licOverdue in _statisticsView.LicensesOverdue)
                {
                    licOverdue.PackagePrice = licOverdue.LicenseItems.Sum(x => x.PilotLicenseItem.Price);
                }

                UpdateChartBankBalanceMonthData();
                DataContext = _statisticsView;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                BtnLicenseBorder.IsEnabled = true;
                BtnTransferBorder.IsEnabled = true;
                progress.Dispose();
            }
        }

        private void ShowModal(string title, object content)
        {

            Window window = new Window
            {
                Title = title,
                Content = content,
                Width = ((UserControl)content).MinWidth,
                Height = ((UserControl)content).MinHeight + 40,
                //SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                WindowStyle = WindowStyle.ToolWindow
            };

            window.ShowDialog();
        }

        private void UpdateChartBankBalanceMonthData()
        {
            ChartBankBalanceMonth.Series[0].Points.Clear();
            if (_statisticsView.ChartModel != null && _statisticsView.ChartModel.Data != null)
            {
                foreach (var point in _statisticsView.ChartModel.Data)
                {
                    ChartBankBalanceMonth.Series[0].Points.Add(point.Value).AxisLabel = point.Key;
                }
            }
        }

        private void Avatar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            bool? result = file.ShowDialog();

            if (result.Value && File.Exists(file.FileName))
            {
                ImgAvatar.Source = new BitmapImage(new Uri(file.FileName, UriKind.Absolute));
            }
        }

        private void ImgGraduation_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadPilotData();
        }

        private async void BtnLicense_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var modal = new LicenseExpensesModal(_statisticsView);
                ShowModal("License Expenses", modal);
                if (modal.IsChanged)
                {
                    await LoadPilotData();
                }
                
                //if (modal.SelectedJobTip != null)
                //{
                //    txtAlternative.Text = GetIcaoInfo(modal.SelectedJobTip.AirportICAO);
                //    AutoSuggestBoxSuggestionChosen();
                //}
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
        }
    }
}
