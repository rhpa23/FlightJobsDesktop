namespace FlightJobs.Model.Models
{
    public class GenerateJobModel
    {
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string Alternative { get; set; }
        public string AviationType { get; set; }
        public CustomPlaneCapacityModel CustomPlaneCapacity { get; set; }
        public string UserId { get; set; }
    }
}
