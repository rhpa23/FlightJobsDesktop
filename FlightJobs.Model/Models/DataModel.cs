using System.Collections.Generic;

namespace FlightJobs.Model.Models
{
    public class DataModel
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double PayloadPounds { get; set; }
        public double FuelWeightPounds { get; set; }
        public double PayloadKilograms { get; set; }
        public double FuelWeightKilograms { get; set; }
        public double Pressure { get; set; }
        public double WindDirection { get; set; }
        public double WindVelocity { get; set; }
        public double Temperature { get; set; }
        public double Visibility { get; set; }
        public bool ParkingBrakeOn { get; set; }
        public bool EngOneRunning { get; set; }
        public bool OnGround { get; set; }
        public IList<string> ResultMessages { get; set; }
        public long ResultScore { get; set; }
    }
}
