using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
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

        public AirlinesView()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _userAccessService = MainWindow.UserServiceFactory.Create();
            _notificationManager = new NotificationManager();
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
            var modal = new AirlineJoinModal();
            if (ShowModal("Search airline to join", modal).Value)
            {
                AppProperties.UserStatistics = await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                LoadAirlineData();
                _notificationManager.Show("Success", $"Congratulations, you signed contract with {AppProperties.UserStatistics.Airline.Name} airline.", NotificationType.Success, "WindowAreaAirline");
            }
        }

        private async void BtnBuyAirline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var createAirlineModal = new AirlineEditModal() { IsCreateAirline = true };
                if (ShowModal("Create Airline", createAirlineModal).Value)
                {
                    AppProperties.UserStatistics = await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                    LoadAirlineData();
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadAirlineData();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private void LoadAirlineData()
        {
            if (AppProperties.UserStatistics.Airline == null)
            {
                BtnActionsBorder.Visibility = Visibility.Collapsed;
                PanelAirlineInfo.Visibility = Visibility.Collapsed;
                PanelNoAirline.Visibility = Visibility.Visible;
                _userAirlineView = new AirlineViewModel();
            }
            else
            {
                BtnActionsBorder.Visibility = Visibility.Visible;
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

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppProperties.UserStatistics.Airline != null)
                {
                    if (ShowModal("Edit airline", new AirlineEditModal()).Value)
                    {
                        AppProperties.UserStatistics = await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                        LoadAirlineData();
                        _notificationManager.Show("Saved", "Airline saved with success.", NotificationType.Success, "WindowAreaAirline");
                    }
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private void BtnDebts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private async void BtnPilotsHired_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            try
            {
                var pilotsHired = await _airlineService.GetAirlinePilotsHired(AppProperties.UserStatistics.Airline.Id);
                var pilotsHiredViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<IList<UserModel>, IList<PilotHiredViewModel>>(pilotsHired);

                progress.Dispose();
                var modal = new PilotsHiredModal();
                modal.DataContext = new PilotsHiredViewModel() { PilotsHired = pilotsHiredViewModel };
                ShowModal("List of pilot hired", modal);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private void BtnLedger_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppProperties.UserStatistics.Airline != null)
                {
                    ShowModal("Airline Ledger", new AirlineJobsLedgerModal(), true);
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private void BtnDebtText_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnDebts_Click(sender, e);
        }

        private void BtnFbo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppProperties.UserStatistics.Airline != null)
                {
                    var airlineFBOs = new AirlineFBOs();
                    while (ShowModal("Airline FBOs", airlineFBOs).Value)
                    {
                        airlineFBOs.ShowHireNotification = ShowModal("Hire FBOs", new AirlineHireFboModal()).Value;
                    }
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private async void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            try
            {
                var exit = await _airlineService.ExitAirline(AppProperties.UserStatistics.Airline.Id, AppProperties.UserLogin.UserId);
                if (exit)
                {
                    AppProperties.UserStatistics = await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                    LoadAirlineData();
                    _notificationManager.Show("Success", "You left the airline", NotificationType.Success, "WindowAreaAirline");
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirline");
            }
            finally
            {
                progress.Dispose();
                HideConfirmExitPopup();
                HideActionsPopup();
            }
        }

        private void BtnNoExit_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmExitPopup();
            HideActionsPopup();
        }
    }
}
