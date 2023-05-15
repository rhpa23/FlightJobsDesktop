using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
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
        private NotificationManager _notificationManager;

        public AirlinesView()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _notificationManager = new NotificationManager();
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

        private void BtnJoinAirliner_Click(object sender, RoutedEventArgs e)
        {
            var modal = new JoinAirlineModal();
            ShowModal("Search airline to join", modal);
        }

        private void BtnBuyAirline_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppProperties.UserStatistics.Airline != null)
                {
                    LoadAirlineData();
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirline");
            }
        }

        private void LoadAirlineData()
        {
            _userAirlineView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<AirlineModel, AirlineViewModel>(AppProperties.UserStatistics.Airline);

            _userAirlineView.OwnerUserName = AppProperties.UserStatistics.Airline.OwnerUser?.UserName;
            _userAirlineView.HiredPilotsNumber = AppProperties.UserStatistics.Airline.HiredPilots.Count();
            _userAirlineView.HiredFobsNumber = AppProperties.UserStatistics.Airline.HiredFBOs.Count();
            
            DirectoryInfo directoryImgInfo = new DirectoryInfo("img");
            var imgLogoPath = $"{directoryImgInfo.FullName}\\{_userAirlineView.Logo}";
            _userAirlineView.Logo = File.Exists(imgLogoPath) ? imgLogoPath : $"{directoryImgInfo.FullName}\\LogoDefault.png";
            
            _userAirlineView.CountryFlag = string.IsNullOrEmpty(_userAirlineView.Country) ? "" : $"/img/flags/{_userAirlineView.Country.Replace(" ", "_")}.jpg";
            DataContext = _userAirlineView;

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

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppProperties.UserStatistics.Airline != null)
                {
                    if (ShowModal("Edit airline", new AirlineEditModal()).Value)
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
    }
}
