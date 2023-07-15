using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using Notification.Wpf;
using System;
using System.Collections.Generic;
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

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para AirlineDebtModal.xam
    /// </summary>
    public partial class AirlineDebtModal : UserControl
    {
        private NotificationManager _notificationManager;
        private IAirlineService _airlineService;
        private IUserAccessService _userAccessService;
        private AirlineDebtsViewModel _airlineDebtsViewModel;

        public AirlineDebtModal()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _userAccessService = MainWindow.UserServiceFactory.Create();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _airlineDebtsViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<AirlineModel, AirlineDebtsViewModel>(AppProperties.UserStatistics.Airline);
                _airlineDebtsViewModel.BankBalanceForecast = _airlineDebtsViewModel.BankBalance - _airlineDebtsViewModel.DebtValue;
                DataContext = _airlineDebtsViewModel;
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineDebt");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).DialogResult = false;
        }

        private async void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaAirlineDebt");
            try
            {
                BtnPayBorder.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                var result = await _airlineService.PayAirlineDebts(_airlineDebtsViewModel.Id, AppProperties.UserLogin.UserId);

                if (result)
                {
                    // Reload Airline data
                    await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                    await _userAccessService.LoadUserAirlineProperties();
                    _notificationManager.Show("Paid", "Airline bills were paid with success.", NotificationType.Success, "WindowArea");
                    HomeView.SetEllipseAirlinesVisibility();
                    ((Window)this.Parent).DialogResult = result;
                }
                else
                {
                    _notificationManager.Show("Error", "Error when try to pay Airline bills.", NotificationType.Error, "WindowAreaAirlineDebt");
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineDebt");
            }
            finally
            {
                progress.Dispose();
                Mouse.OverrideCursor = Cursors.Arrow;
                BtnPayBorder.IsEnabled = true;
            }
        }
    }
}
