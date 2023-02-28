using System;

namespace FlightJobsDesktop.ViewModels
{
    public class LastJobViewModel
    {
        private string _departureICAO = "NA";
        private string _arrivalICAO = "NA";
        private long _dist = 0;
        private string _modelDescription = "NA";
        private string _endTime = "NA";
        private string _flightTime = "NA";

        public string DepartureICAO 
        {
            get { return _departureICAO; }
            set { _departureICAO = value; } 
        }

        public string ArrivalICAO
        {
            get { return _arrivalICAO; }
            set { _arrivalICAO = value; }
        }
        public long Dist
        {
            get { return _dist; }
            set { _dist = value; }
        }
        public string DistComplete { get { return _dist + " NM"; } }
        
        public string ModelDescription
        {
            get { return _modelDescription; }
            set { _modelDescription = value; }
        }
        public string EndTime
        {
            get { return Convert.ToDateTime(_endTime).ToString("dd-MMMM-yyyy") ; }
            set { _endTime = value; }
        }

        public string FlightTime
        {
            get { return _flightTime; }
            set { _flightTime = value; }
        }
}
}
