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

        public PilotLicenseExpensesViewModel PilotLicenseExpense { get; set; } = new PilotLicenseExpensesViewModel();

        public DateTime MaturityDate { get; set; }
        public string MaturityDateShort { get { return MaturityDate.ToShortDateString(); } }

        public bool OverdueProcessed { get; set; }

        public long PackagePrice { get; set; }
        public string PackagePriceCurrency { get { return string.Format("F{0:C}", PackagePrice); } }

        public IList<LicenseItemViewModel> LicenseItems { get; set; } = new List<LicenseItemViewModel>();
    }

    public class LicenseItemViewModel
    {
        public long Id { get; set; }
        public PilotLicenseItemViewModel PilotLicenseItem { get; set; }
        public bool IsBought { get; set; }
    }

    public class PilotLicenseExpensesViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int DaysMaturity { get; set; }

        public bool Mandatory { get; set; }
    }

    public class PilotLicenseItemViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long Price { get; set; }
        public string PriceCurrency { get { return string.Format("F{0:C}", Price); } }

        public string Image { get; set; }

        public string ImageAdaptPath
        {
            get { return !string.IsNullOrEmpty(Image) && Image.Contains("Content") ? Image.Replace("/Content", "") : Image; }
        }

    }

    public class LicenseExpensesViewModel : ObservableObject
    {
        public long _bankBalance { get; set; }
        public long BankBalance
        {
            get { return _bankBalance; }
            set { _bankBalance = value; OnPropertyChanged(); OnPropertyChanged("BankBalanceCurrency"); }
        }
        public string BankBalanceCurrency { get { return string.Format("F{0:C}", BankBalance); } }


        public long _bankBalanceProjection;
        public long BankBalanceProjection
        {
            get { return _bankBalanceProjection; }
            set { _bankBalanceProjection = value; OnPropertyChanged(); OnPropertyChanged("BankBalanceProjectionCurrency"); }
        }
        public string BankBalanceProjectionCurrency { get { return string.Format("F{0:C}", BankBalanceProjection); } }


        public PilotLicenseExpensesUserViewModel _selectedLicense = new PilotLicenseExpensesUserViewModel();
        public PilotLicenseExpensesUserViewModel SelectedLicense
        {
            get { return _selectedLicense; }
            set { _selectedLicense = value; OnPropertyChanged(); }
        }
        
        public IList<PilotLicenseExpensesUserViewModel> _overdueLicenses = new List<PilotLicenseExpensesUserViewModel>();
        public IList<PilotLicenseExpensesUserViewModel> OverdueLicenses
        {
            get { return _overdueLicenses; }
            set { _overdueLicenses = value; OnPropertyChanged(); }
        }
    }
}
