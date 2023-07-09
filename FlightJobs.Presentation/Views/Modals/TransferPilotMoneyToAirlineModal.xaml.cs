using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobsDesktop.ViewModels;
using log4net;
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
    /// Interação lógica para TransferPilotMoneyToAirlineModal.xam
    /// </summary>
    public partial class TransferPilotMoneyToAirlineModal : UserControl
    {
        public bool IsChanged { get; set; }
        private static readonly ILog _log = LogManager.GetLogger(typeof(PrivateView));
        private NotificationManager _notificationManager;
        private IPilotService _pilotService;
        public TransferPilotMoneyToAirlineModal()
        {
            InitializeComponent();
        }

        public TransferPilotMoneyToAirlineModal(UserStatisticsFlightsViewModel userStatisticsFlightsViewModel)
        {
            InitializeComponent();
            _pilotService = MainWindow.PilotServiceFactory.Create();
            _notificationManager = new NotificationManager();

            userStatisticsFlightsViewModel.Transfer = new TransferToAirline() 
            { 
                BankBalance = userStatisticsFlightsViewModel.BankBalance,
                TransferPercent = 10,
                BankBalanceAirline = AppProperties.UserStatistics.Airline.BankBalance
            };

            DataContext = userStatisticsFlightsViewModel;
        }

        private async void BtnTranfer_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaTransferLoading");
            BtnTranferBorder.IsEnabled = false;
            try
            {
                var userStatisticsFlightsView = (UserStatisticsFlightsViewModel)DataContext;
                await _pilotService.TranfersMoneyToAirline(AppProperties.UserLogin.UserId, userStatisticsFlightsView.Transfer.TransferPercent);
                IsChanged = true;
                ((Window)Parent).Close();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _notificationManager.Show("Error", ex.Message, NotificationType.Success, "WindowAreaTransfer");
                BtnTranferBorder.IsEnabled = true;
            }
            finally
            {
                progress.Dispose();
            }
        }
    }
}
