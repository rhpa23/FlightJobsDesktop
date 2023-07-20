using System;

namespace FlightJobs.Connect.MSFS.SDK.Model
{
    public class FlightRecorderModel : ObservableObject
    {
        public FlightRecorderModel() { }
        public FlightRecorderModel(PlaneModel planeModel)
        {
            Altitude = planeModel.CurrentAltitude;
            Speed = planeModel.GroundSpeed;
            Latitude = planeModel.Latitude;
            Longitude = planeModel.Longitude;
            LightLandingOn = planeModel.LightLandingOn;
            LightBeaconOn = planeModel.LightBeaconOn;
            LightNavigationOn = planeModel.LightNavigationOn;
            AltimeterInMillibars = planeModel.AltimeterInMillibars;
            Heading = planeModel.HeadingTrue;
            OnGround = planeModel.OnGround;
            FuelWeightKilograms = planeModel.FuelWeightKilograms;
        }
        public bool OnGround { get; set; }
        public long Altitude { get; set; }
        public int Speed { get; set; }
        public double FuelWeightKilograms { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool LightLandingOn { get; set; }
        public bool LightBeaconOn { get; set; }
        public bool LightNavigationOn { get; set; }
        public int AltimeterInMillibars { get; set; }
        public double Heading { get; set; }
        public DateTime TimeUtc { get; set; }
        public int FPS { get; set; }


    }
}
