using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class UserStatisticsFlightsViewModel
    {
        public long BankBalance { get; set; }
        public string BankBalanceCurrency { get { return string.Format("F{0:C}", BankBalance); } }
        public long PilotScore { get; set; }
        public string GraduationPath { get; set; }
        public string GraduationAdaptPath 
        { 
            get { return !string.IsNullOrEmpty(GraduationPath) && GraduationPath.Contains("Content") ? GraduationPath.Replace("/Content", "") : GraduationPath; } 
        }
        public string GraduationDesc { get; set; }
        public string LastAircraft { get; set; }
        public DateTime LastFlight { get; set; }
        public string LastFlightShortDate { get { return LastFlight.ToShortDateString(); } }
        public string FavoriteAirplane { get; set; }
        public long NumberFlights { get; set; }
        public string FlightTimeTotal { get; set; }
        public Dictionary<string, long> DepartureRanking { get; set; }
        public Dictionary<string, long> DestinationRanking { get; set; }
        public ChartUserBankBalanceViewModel ChartModel { get; set; }
        public IList<PilotLicenseExpensesUserViewModel> LicensesOverdue { get; set; }
        public string LicenseStatus { get; set; }
        public string LicenseStatusColor { get { return LicensesOverdue?.Count > 0 ? "Red" : "Green"; } }
    }

    public class ChartUserBankBalanceViewModel
    {
        public Dictionary<string, double> Data { get; set; }

        public double PayamentTotal { get; set; }
        public string PayamentTotalCurrency { get { return string.Format("F{0:C}", PayamentTotal); } }

        public double PayamentMonthGoal { get; set; }
        public string PayamentMonthGoalCurrency { get { return string.Format("F{0:C}", PayamentMonthGoal); } }
    }

    public class PilotLicenseExpensesUserViewModel
    {
        public long Id { get; set; }

        public PilotLicenseExpensesViewModel PilotLicenseExpense { get; set; }

        public DateTime MaturityDate { get; set; }

        public bool OverdueProcessed { get; set; }
    }

    public class PilotLicenseExpensesViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int DaysMaturity { get; set; }

        public bool Mandatory { get; set; }
    }
}
