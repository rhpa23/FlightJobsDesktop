using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using log4net;
using ModernWpf;
using ModernWpf.Controls;
using Notification.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para AirlinesView.xam
    /// </summary>
    public partial class AirlinesView : UserControl
    {
        private AirlineViewModel _userAirlineView;
        private IAirlineService _airlineService;
        private IUserAccessService _userAccessService;
        private NotificationManager _notificationManager;
        private Flyout _flyoutConfirmExit;
        private Flyout _flyoutActions;
        private static readonly ILog _log = LogManager.GetLogger(typeof(ConnectorView));

        public AirlinesView()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _userAccessService = MainWindow.UserServiceFactory.Create();
            _notificationManager = new NotificationManager();

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
                ChartBankBalanceMonth.ChartAreas[0].AxisY.LabelStyle.Enabled = false;

            }
        }
        private void HideConfirmExitPopup()
        {
            if (_flyoutConfirmExit != null)
            {
                _flyoutConfirmExit.Hide();
            }
        }

        private void FlyoutConfirmExit_Opened(object sender, object e)
        {
            _flyoutConfirmExit = (Flyout)sender;
        }

        private void HideActionsPopup()
        {
            if (_flyoutActions != null)
            {
                _flyoutActions.Hide();
            }
        }

        private void FlyoutActions_Opened(object sender, object e)
        {
            _flyoutActions = (Flyout)sender;
        }

        private bool? ShowModal(string title, object content, bool canMaximize = false)
        {

            Window window = new Window
            {
                Title = title,
                Content = content,
                Width = ((UserControl)content).MinWidth,
                Height = ((UserControl)content).MinHeight + 40,
                //SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                WindowStyle = canMaximize ? WindowStyle.None : WindowStyle.ToolWindow
            };

            return window.ShowDialog();
        }

        private async void BtnJoinAirliner_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading(true);
            var modal = new AirlineJoinModal();
            if (ShowModal("Search airline to join", modal).Value)
            {
                await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                await _userAccessService.LoadUserAirlineProperties();
                LoadAirlineData();
                _notificationManager.Show("Success", $"Congratulations, you signed contract with {AppProperties.UserStatistics.Airline.Name} airline.", NotificationType.Success, "WindowAreaAirline");
            }
            MainWindow.HideLoading();
        }

        private async void BtnBuyAirline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading(true);
                var createAirlineModal = new AirlineEditModal() { IsCreateAirline = true };
                if (ShowModal("Create Airline", createAirlineModal).Value)
                {
                    await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                    LoadAirlineData();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void LoadAirlineData()
        {
            if (AppProperties.UserStatistics.Airline == null)
            {
                //BtnActionsBorder.Visibility = Visibility.Collapsed;
                PanelAirlineInfo.Visibility = Visibility.Collapsed;
                PanelNoAirline.Visibility = Visibility.Visible;
                _userAirlineView = new AirlineViewModel();
            }
            else
            {
                
                //BtnActionsBorder.Visibility = Visibility.Visible;
                PanelAirlineInfo.Visibility = Visibility.Visible;
                PanelNoAirline.Visibility = Visibility.Collapsed;
                _userAirlineView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<AirlineModel, AirlineViewModel>(AppProperties.UserStatistics.Airline);

                _userAirlineView.OwnerUserName = AppProperties.UserStatistics.Airline.OwnerUser?.UserName;
                _userAirlineView.HiredPilotsNumber = AppProperties.UserStatistics.Airline.HiredPilots != null ? AppProperties.UserStatistics.Airline.HiredPilots.Count() : 0;
                _userAirlineView.HiredFobsNumber = AppProperties.UserStatistics.Airline.HiredFBOs != null ? AppProperties.UserStatistics.Airline.HiredFBOs.Count() : 0;

                DirectoryInfo directoryImgInfo = new DirectoryInfo("img");
                var imgLogoPath = $"{directoryImgInfo.FullName}\\{_userAirlineView.Logo}";
                _userAirlineView.Logo = File.Exists(imgLogoPath) ? imgLogoPath : $"{directoryImgInfo.FullName}\\LogoDefault.png";

                _userAirlineView.CountryFlag = string.IsNullOrEmpty(_userAirlineView.Country) ? "" : $"/img/flags/{_userAirlineView.Country.Replace(" ", "_")}.jpg";
                

                if (AppProperties.UserStatistics.Airline.OwnerUser?.Id == AppProperties.UserLogin.UserId)
                {
                    BtnEdit.Visibility = BtnFbo.Visibility = Visibility.Visible;

                    if (AppProperties.UserStatistics.Airline.DebtValue <= 0)
                    {
                        BtnDebts.Visibility = Visibility.Collapsed;
                        BtnDebtText.IsEnabled = false;
                    }
                    else
                    {
                        BtnDebts.Visibility = Visibility.Visible;
                        BtnDebtText.IsEnabled = true;
                    }
                }
            }
            DataContext = _userAirlineView;
        }

        private void UpdateChartBankBalanceMonthData()
        {
            ChartBankBalanceMonth.Series[0].Points.Clear();
            if (_userAirlineView.ChartModel != null && _userAirlineView.ChartModel.Data != null)
            {
                foreach (var point in _userAirlineView.ChartModel.Data)
                {
                    ChartBankBalanceMonth.Series[0].Points.Add(point.Value).AxisLabel = point.Key;
                }
                AddGoalStripLine();
            }
        }

        private void AddGoalStripLine()
        {
            StripLine stripLineGoal = new StripLine();

            // Set threshold line so that it is only shown once  
            stripLineGoal.Interval = 0;
            stripLineGoal.IntervalOffset = _userAirlineView.ChartModel.PayamentMonthGoal;

            stripLineGoal.BackColor = Color.Green;
            stripLineGoal.StripWidth = 3;

            // Set text properties for the threshold line  
            //stripLineGoal.Text = $" {_userAirlineView.ChartModel.PayamentMonthGoalCurrency} ";
            //stripLineGoal.ForeColor = Color.Green;
            //stripLineGoal.BackColor = Color.Green;

            ChartBankBalanceMonth.ChartAreas[0].AxisY.StripLines.Add(stripLineGoal);
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading(true);
                if (AppProperties.UserStatistics.Airline != null)
                {
                    if (ShowModal("Edit airline", new AirlineEditModal()).Value)
                    {
                        await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                        LoadAirlineData();
                        _notificationManager.Show("Saved", "Airline saved with success.", NotificationType.Success, "WindowAreaAirline");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void BtnDebts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading(true);
                if (AppProperties.UserStatistics.Airline != null)
                {
                    if (AppProperties.UserStatistics.Airline.DebtValue <= 0)
                    {
                        _notificationManager.Show("Warning", "Airline don't have debts to pay.", NotificationType.Warning, "WindowAreaAirline");
                        return;
                    }

                    if (ShowModal("Pay airline debts", new AirlineDebtModal()).Value)
                    {
                        LoadAirlineData();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private async void BtnPilotsHired_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            try
            {
                var pilotsHired = await _airlineService.GetAirlinePilotsHired(AppProperties.UserStatistics.Airline.Id);
                var pilotsHiredViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<IList<UserModel>, IList<PilotHiredViewModel>>(pilotsHired);

                MainWindow.HideLoading();
                var modal = new PilotsHiredModal();
                modal.DataContext = new PilotsHiredViewModel() { PilotsHired = pilotsHiredViewModel };
                MainWindow.ShowLoading(true);
                ShowModal("List of pilot hired", modal);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void BtnLedger_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading(true);
                if (AppProperties.UserStatistics.Airline != null)
                {
                    ShowModal("Airline Ledger", new AirlineJobsLedgerModal(), true);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void BtnDebtText_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnDebts_Click(sender, e);
        }

        private async void BtnFbo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppProperties.UserStatistics.Airline != null)
                {
                    MainWindow.ShowLoading(true);
                    var airlineFBOs = new AirlineFBOs();
                    while (ShowModal("Airline FBOs", airlineFBOs).Value)
                    {
                        airlineFBOs.ShowHireNotification = ShowModal("Hire FBOs", new AirlineHireFboModal()).Value;
                        await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                        await _userAccessService.LoadUserAirlineProperties();
                        LoadAirlineData();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private async void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            try
            {
                var exit = await _airlineService.ExitAirline(AppProperties.UserStatistics.Airline.Id, AppProperties.UserLogin.UserId);
                if (exit)
                {
                    await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                    await _userAccessService.LoadUserAirlineProperties();
                    LoadAirlineData();
                    _notificationManager.Show("Success", "You left the airline", NotificationType.Success, "WindowAreaAirline");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                MainWindow.HideLoading();
                HideConfirmExitPopup();
                HideActionsPopup();
            }
        }

        private void BtnNoExit_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmExitPopup();
            HideActionsPopup();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            Mouse.OverrideCursor = Cursors.Wait;
            WindowsChartArea.Visibility = Visibility.Hidden;
            try
            {
                await _userAccessService.LoadUserAirlineProperties();
                LoadAirlineData();
                UpdateChartBankBalanceMonthData();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                WindowsChartArea.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = Cursors.Arrow;
                MainWindow.HideLoading();
            }
        }

        private void BtnScoreRank_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowModal("Airlines score ranking", new AirlineScoreRankingModal(), false);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Airline score rank data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
        }
    }
}
