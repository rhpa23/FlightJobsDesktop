using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class AirlineFilterViewModel :  INotifyPropertyChanged
    {
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }

        private string _airlineName;
        public string AirlineName
        {
            get
            {
                return _airlineName;
            }
            set
            {
                _airlineName = value;
                NotifyPropertyChanged("AirlineName");
            }
        }

        private string _airlineCountry;
        public string AirlineCountry
        {
            get
            {
                return _airlineCountry;
            }
            set
            {
                _airlineCountry = value;
                NotifyPropertyChanged("AirlineCountry");
            }
        }

        public IList<AirlineViewModel> Airlines { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
