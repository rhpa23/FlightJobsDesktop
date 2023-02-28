namespace FlightJobs.Model.Models
{
    public class StartJobResponseModel
    {
        public string ArrivalICAO { get; set; }
        public double ArrivalLAT { get; set; }
        public double ArrivalLON { get; set; }

        public string ResultMessage { get; set; }
    }
}
