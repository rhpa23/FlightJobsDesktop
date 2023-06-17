using System;

namespace FlightJobsDesktop.ViewModels
{
    public class LastJobViewModel : ObservableObject
    {
        private string _departureICAO;
        private string _arrivalICAO;
        private long _dist;
        private string _modelDescription;
        private string _endTime;
        private string _flightTime;

        public string DepartureICAO
        {
            get { return _departureICAO; }
            set { _departureICAO = value; OnPropertyChanged("DepartureICAO"); }
        }

        public string ArrivalICAO
        {
            get { return _arrivalICAO; }
            set { _arrivalICAO = value; OnPropertyChanged("ArrivalICAO"); }
        }
        public long Dist
        {
            get { return _dist; }
            set { _dist = value; OnPropertyChanged("DistComplete"); }
        }
        public string DistComplete { get { return _dist + " NM"; } }

        public string ModelDescription
        {
            get { return _modelDescription; }
            set { _modelDescription = value; OnPropertyChanged("ModelDescription"); }
        }
        public string EndTime
        {
            get { return Convert.ToDateTime(_endTime).ToString("dd-MMMM-yyyy"); }
            set { _endTime = value; OnPropertyChanged("EndTime"); }
        }

        public string FlightTime
        {
            get { return _flightTime; }
            set { _flightTime = value; OnPropertyChanged("FlightTime"); }
        }

    }
}
