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

        private void SaveLogoImg(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo("img\\avatar");
                    if (!Directory.Exists("img")) directoryInfo = Directory.CreateDirectory("img");

                    var destPath = $"{directoryInfo.FullName}\\{new FileInfo(filePath).Name}";
                    File.Copy(filePath, destPath, true);
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Avatar image could not be saved. Please run as administrator.", NotificationType.Error, "WindowArea");
            }
        }

        private void LoadLogoImg(string filename)
        {
            try
            {
                filename = new FileInfo(filename).Name;
                DirectoryInfo directoryInfo = new DirectoryInfo("img\\avatar");
                var imgLocalPath = $"{directoryInfo.FullName}\\{filename}";

                ImgAvatar.Source = File.Exists(imgLocalPath) ? new BitmapImage(new Uri(imgLocalPath)) : new BitmapImage(new Uri($"{directoryInfo.FullName}\\default.jpg"));

            }
            catch (Exception)
            {
                //empty
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

                //DirectoryInfo directoryImgInfo = new DirectoryInfo("img");
                //var imgLogoPath = $"{directoryImgInfo.FullName}\\{_statisticsView.Logo}";
                //_statisticsView.Logo = File.Exists(imgLogoPath) ? imgLogoPath : $"{directoryImgInfo.FullName}\\avatar\\default.jpg";

                LoadLogoImg(_statisticsView.Logo);

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

        private async void Avatar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var fileDialog = new OpenFileDialog() { Title = "Avatar", Filter = "Image Files|*.jpg;*.jpeg;*.png;..." };

                if (fileDialog.ShowDialog((Window)Parent).Value)
                {
                    SaveLogoImg(fileDialog.FileName);
                    LoadLogoImg(fileDialog.FileName);
                    await _pilotService.SaveAvatar(AppProperties.UserLogin.UserId, new FileInfo(fileDialog.FileName).Name);
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Avatar image could not be saved. Please run as administrator.", NotificationType.Error, "WindowArea");
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
