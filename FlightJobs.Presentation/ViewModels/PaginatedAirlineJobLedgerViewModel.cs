using FlightJobs.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class PaginatedAirlineJobLedgerViewModel : INotifyPropertyChanged
    {
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }
        public IList<AirlineJobLedgerViewModel> AirlineJobs { get; set; } = new List<AirlineJobLedgerViewModel>();
        public FilterAirlineJobLedger Filter { get; set; } = new FilterAirlineJobLedger();

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FilterAirlineJobLedger : INotifyPropertyChanged
    {
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string ModelDescription { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AirlineJobLedgerViewModel
    {
        public long Id { get; set; }
        public AirlineViewModel Airline { get; set; }
        public CurrentJobViewModel Job { get; set; }
        public long JobDebtValue { get; set; }
        public double FuelPrice { get; set; }
        public string FuelPriceComplete { get { return string.Format("F{0:C}", FuelPrice); } }
        public double FuelCost { get; set; }
        public string FuelCostComplete { get { return string.Format("F{0:C}", FuelCost); } }
        public double FuelCostPerNM { get; set; }
        public string FuelCostPerNmComplete { get { return string.Format("F{0:C}", FuelCostPerNM); } }
        public double GroundCrewCost { get; set; }
        public string GroundCrewCostComplete { get { return string.Format("F{0:C}", GroundCrewCost); } }
        public double FlightCrewCost { get; set; }
        public string FlightCrewCostComplete { get { return string.Format("F{0:C}", FlightCrewCost); } }
        public double FlightAttendantCost { get; set; }
        public string FlightAttendantCostComplete { get { return string.Format("F{0:C}", FlightAttendantCost); } }
        public double TotalCrewCostLabor { get; set; }
        public string TotalCrewCostLaborComplete { get { return string.Format("F{0:C}", TotalCrewCostLabor); } }
        public double TotalFlightCost { get; set; }
        public string TotalFlightCostComplete { get { return string.Format("F{0:C}", TotalFlightCost); } }
        public double RevenueEarned { get; set; }
        public string RevenueEarnedComplete { get { return string.Format("F{0:C}", RevenueEarned); } }
        public double FlightIncome { get; set; }
        public string FlightIncomeComplete { get { return string.Format("F{0:C}", FlightIncome); } }
        public string WeightUnit { get; set; }
    }
}
