using FlightJobs.Infrastructure;
using FlightJobsDesktop.ViewModels;
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
        public TransferPilotMoneyToAirlineModal()
        {
            InitializeComponent();
        }

        public TransferPilotMoneyToAirlineModal(UserStatisticsFlightsViewModel userStatisticsFlightsViewModel)
        {
            InitializeComponent();

            userStatisticsFlightsViewModel.Transfer = new TransferToAirline() 
            { 
                BankBalance = userStatisticsFlightsViewModel.BankBalance,
                TransferPercent = 10,
                BankBalanceAirline = AppProperties.UserStatistics.Airline.BankBalance
            };

            DataContext = userStatisticsFlightsViewModel;
        }

        private void BtnTranfer_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
