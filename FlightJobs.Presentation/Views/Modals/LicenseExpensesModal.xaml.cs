using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobsDesktop.ViewModels;
using log4net;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interação lógica para LicenseExpensesModal.xam
    /// </summary>
    public partial class LicenseExpensesModal : UserControl
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PrivateView));
        private NotificationManager _notificationManager;

        private UserStatisticsFlightsViewModel _userStatisticsFlightsView;
        private LicenseExpensesViewModel _licenseExpensesView;
        private IPilotService _pilotService;

        public bool IsChanged { get; set; }

        public LicenseExpensesModal(UserStatisticsFlightsViewModel userStatisticsFlightsViewModel)
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _pilotService = MainWindow.PilotServiceFactory.Create();

            _userStatisticsFlightsView = userStatisticsFlightsViewModel;
            _licenseExpensesView = new LicenseExpensesViewModel();
            _licenseExpensesView.OverdueLicenses = _userStatisticsFlightsView.LicensesOverdue;
            _licenseExpensesView.BankBalance = AppProperties.UserStatistics.BankBalance;
            DataContext = _licenseExpensesView;
        }
        public LicenseExpensesModal()
        {
            InitializeComponent();
        }

        private void LsvOverdueLicenses_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _licenseExpensesView.BankBalanceProjection = AppProperties.UserStatistics.BankBalance - _licenseExpensesView.SelectedLicense.PackagePrice;
            LicenseItemsImageList.ItemsSource = _licenseExpensesView.SelectedLicense.LicenseItems;
            BtnBuyBorder.IsEnabled = _licenseExpensesView.OverdueLicenses.Count() > 0;
        }

        private async void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLicenseExpensesLoading");
            BtnBuyBorder.IsEnabled = false;
            try
            {
                var userStatistics = await _pilotService.BuyLicencePackage(AppProperties.UserLogin.UserId, 
                                                _licenseExpensesView.SelectedLicense.PilotLicenseExpense.Id);
                
                var selectedLicense = _licenseExpensesView.SelectedLicense;

                AppProperties.UserStatistics.BankBalance = userStatistics.BankBalance;

                _licenseExpensesView = new LicenseExpensesViewModel();
                _licenseExpensesView.BankBalance = AppProperties.UserStatistics.BankBalance;
                // Update OverdueLicenses
                _userStatisticsFlightsView.LicensesOverdue.Remove(selectedLicense);
                _licenseExpensesView.OverdueLicenses = _userStatisticsFlightsView.LicensesOverdue;
                LsvOverdueLicenses.ItemsSource = null;// Only reload Grid when set null before. ;)
                LsvOverdueLicenses.ItemsSource = _licenseExpensesView.OverdueLicenses;
                LicenseItemsImageList.ItemsSource = null;
                DataContext = _licenseExpensesView;

                IsChanged = true;

                _notificationManager.Show("Error", "License expense paid successfully.", NotificationType.Success, "WindowAreaLicenseExpenses");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaLicenseExpenses");
            }
            finally
            {
                progress.Dispose();
            }
        }
    }
}
