using FlightJobs.Model.Enum;
using System.Collections.Generic;
using System.ComponentModel;

namespace FlightJobsDesktop.ViewModels
{
    public class GenerateJobViewModel : INotifyPropertyChanged
    {
        public GenerateJobViewModel()
        {
        }
        public string DepartureICAO { get { return Departure.Length > 3 ? Departure.Substring(0, 4) : ""; } }

        public string ArrivalICAO { get { return Arrival.Length > 3 ? Arrival.Substring(0, 4) : ""; } }

        public string AlternativeICAO { get { return Alternative.Length > 3 ? Alternative.Substring(0, 4) : ""; } }

        public string Departure { get; set; } = "";

        public string Arrival { get; set; } = "";

        public string Alternative { get; set; } = "";

        public AviationType AviationType { get; set; } = AviationType.AirTransport;

        public long Dist { get; set; }
        public string DistDesc { get { return Dist + " NM"; } }
        public IList<CurrentJobViewModel> PendingJobs { get; set; }
        public CurrentJobViewModel PendingSelectedJob { get; set; }

        public IList<JobItemViewModel> _jobItemList = new List<JobItemViewModel>();
        public IList<JobItemViewModel> JobItemList
        {
            get
            {
                return _jobItemList;
            }
            set
            {
                _jobItemList = value;
                NotifyPropertyChanged("JobItemList");
            }
        }

        private CapacityViewModel _capacityViewModel = new CapacityViewModel();
        public CapacityViewModel Capacity
        {
            get
            {
                return _capacityViewModel;
            }
            set
            {
                _capacityViewModel = value;
                NotifyPropertyChanged("Capacity");
            }
        }
        public IList<CapacityViewModel> _capacityList = new List<CapacityViewModel>();
        public IList<CapacityViewModel> CapacityList
        {
            get
            {
                return _capacityList;
            }
            set
            {
                _capacityList = value;
                NotifyPropertyChanged("CapacityList");
            }
        }

        private long _selectedPax = 0;
        public long SelectedPax 
        {
            get
            {
                return _selectedPax;
            }
            set
            {
                _selectedPax = value;
                NotifyPropertyChanged("SelectedPax");
            }
        }

        private long _selectedCargo = 0;
        public long SelectedCargo
        {
            get
            {
                return _selectedCargo;
            }
            set
            {
                _selectedCargo = value;
                NotifyPropertyChanged("SelectedCargo");
            }
        }

        private long _selectedPay = 0;
        public long SelectedPay
        {
            get
            {
                return _selectedPay;
            }
            set
            {
                _selectedPay = value;
                NotifyPropertyChanged("SelectedPay");
                NotifyPropertyChanged("SelectedPayFormat");
            }
        }

        public string SelectedPayFormat { get { return string.Format("F{0:C0}", _selectedPay); }  }

        private long _selectedTotalPayload = 0;
        public long SelectedTotalPayload
        {
            get
            {
                return _selectedTotalPayload;
            }
            set
            {
                _selectedTotalPayload = value;
                NotifyPropertyChanged("SelectedTotalPayload");
            }
        }

        private string _weightUnit = "kg";
        public string WeightUnit
        {
            get
            {
                return _weightUnit;
            }
            set
            {
                _weightUnit = value;
                NotifyPropertyChanged("WeightUnit");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
