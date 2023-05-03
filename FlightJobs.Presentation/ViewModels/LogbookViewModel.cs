using FlightJobs.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class LogbookViewModel : INotifyPropertyChanged
    {
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }
        public IList<LogbookUserJobViewModel> Jobs { get; set; } = new List<LogbookUserJobViewModel>();
        public FilterLogbook Filter { get; set; } = new FilterLogbook();

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FilterLogbook : INotifyPropertyChanged
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

    public class LogbookUserJobViewModel
    {
        public string DateDisplayFormat { get { return EndTime.ToString("yyyy/MM/dd"); } }
        public string DepartureDisplayFormat { get { return $"{DepartureICAO} {StartTime.ToString("(HH:mm)")}"; } }
        public string ArrivalDisplayFormat { get { return $"{ArrivalICAO} {EndTime.ToString("(HH:mm)")}"; } }
        public string ModelDisplayFormat { get { return $"{ModelDescription} - { ModelName}"; } }
        public string DistDisplayFormat { get { return $"{Dist} NM"; } }
        public string CargoDisplayFormat { get { return $"{Cargo} {AppProperties.UserStatistics.WeightUnit}"; } }
        public string PayloadDisplayFormat { get { return $"{Payload} {AppProperties.UserStatistics.WeightUnit}"; } }
        public string PayDisplayFormat { get { return string.Format("F{0:C0}", Pay); } }
        public string BurnedFuelDisplayFormat { get { return $"{UsedFuelWeightDisplay}  {AppProperties.UserStatistics.WeightUnit}"; } }


        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string ModelName { get; set; }
        public string ModelDescription { get; set; }
        public long Dist { get; set; }
        public long Pax { get; set; }
        public long Cargo { get; set; }
        public long Payload { get; set; }
        public long Pay { get; set; }
        public string FlightTime { get; set; }
        public long UsedFuelWeightDisplay { get; set; }

    }
}
