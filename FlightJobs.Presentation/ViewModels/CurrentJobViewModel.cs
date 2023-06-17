using FlightJobs.Connect.MSFS.SDK.Model;
using System.Windows;
using System.Windows.Media;

namespace FlightJobsDesktop.ViewModels
{
    public class CurrentJobViewModel : ObservableObject
    {
        public object User { get; set; }
        public int PaxWeight { get; set; } = 88;
        public int Id { get; set; }
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string AlternativeICAO { get; set; }
        public long Dist { get; set; }
        public string DistComplete { get { return Dist + " NM"; } }
        public long Pax { get; set; }
        public string PaxComplete { get { return $"Pax: ({Pax} * {PaxWeight} {WeightUnit})"; } }
        public string PaxTotalWeight { get { return $"{Pax * PaxWeight} {WeightUnit}"; } }
        public long Cargo { get; set; }
        public string CargoComplete { get { return $"{Cargo} {WeightUnit}"; } }
        public long Payload { get; set; }
        public string PayloadComplete { get { return $"{Payload} {WeightUnit}"; } }
        public long PayloadDisplay { get; set; }
        public long Pay { get; set; }
        public string PayComplete { get { return string.Format("F{0:C}", Pay); } }
        public bool FirstClass { get; set; }
        public bool IsDone { get; set; }
        public bool IsActivated { get; set; }
        public bool InProgress { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ModelName { get; set; }
        public string ModelDescription { get; set; }
        public long StartFuelWeight { get; set; }
        public long StartFuelWeightDisplay { get; set; }
        public string StartFuelWeightComplete { get { return $"{StartFuelWeightDisplay} {WeightUnit}"; } }
        public long FinishFuelWeight { get; set; }
        public int AviationType { get; set; }
        public long UsedFuelWeight { get; set; }
        public long UsedFuelWeightDisplay { get; set; }
        public string UsedFuelWeightComplete { get { return $"{UsedFuelWeightDisplay} {WeightUnit}"; } }
        public string Month { get; set; }
        public string VideoUrl { get; set; }
        public string VideoDescription { get; set; }
        public string ChallengeCreatorUserId { get; set; }
        public bool IsChallenge { get; set; }
        public bool IsChallengeFromCurrentUser { get; set; }
        public string ChallengeExpirationDate { get; set; }
        public int ChallengeType { get; set; }
        public string WeightUnit { get; set; }
        private string _sliderMessage { get; set; }
        public string SliderMessage
        {
            get { return _sliderMessage; }
            set { _sliderMessage = value; OnPropertyChanged("SliderMessage"); }
        }

        private string _flightTime { get; set; }
        public string FlightTime
        {
            get { return _flightTime; }
            set { _flightTime = value; OnPropertyChanged("FlightTime"); }
        }
        public int FlightTimeHours { get; set; }

        private long _score { get; set; }
        public long Score
        {
            get { return _score; }
            set { _score = value; OnPropertyChanged("Score"); }
        }

        public LastJobViewModel LastJob { get; set; } = new LastJobViewModel();
        public PlaneModel PlaneSimData { get; set; }
        public SimDataModel SimData { get; set; }

        public FlightResultsViewModel FlightResults { get; set; } = new FlightResultsViewModel();

        private Visibility _isConnectedVisibility;
        public Visibility IsConnectedVisibility
        {
            get { return _isConnectedVisibility; }
            set { _isConnectedVisibility = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _payloadLabelColor;
        public SolidColorBrush PayloadLabelColor
        {
            get { return _payloadLabelColor; }
            set { _payloadLabelColor = value; OnPropertyChanged("PayloadLabelColor"); }
        }

        private string _jobSummary;
        public string JobSummary
        {
            get { return _jobSummary; }
            set { _jobSummary = value; OnPropertyChanged("JobSummary"); }
        }

        private string _sliderTopTitle = "Waiting for start job..";
        public string SliderTopTitle
        {
            get { return _sliderTopTitle; }
            set { _sliderTopTitle = value; OnPropertyChanged("SliderTopTitle"); }
        }

        public MessagesResultViewModel MsgResults { get; set; } = new MessagesResultViewModel();
       
    }
}
