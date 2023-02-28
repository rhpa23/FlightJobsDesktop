namespace FlightJobs.Model.Models
{
    public class SearchJobTipsModel
    {
        public long IdJob { get; set; }
        public string AirportICAO { get; set; }
        public string AirportName { get; set; }
        public int AirportRunwaySize { get; set; }
        public int AirportElevation { get; set; }
        public int AirportTrasition { get; set; }
        public long Distance { get; set; }
        public long Pax { get; set; }
        public long Cargo { get; set; }
        public long Payload { get; set; }
        public long Pay { get; set; }
    }
}
