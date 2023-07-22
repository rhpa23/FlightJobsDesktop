using FlightJobs.Connect.MSFS.SDK.Model;
using System;

namespace FlightJobsDesktop.ViewModels
{
    public class FlightRecorderViewModel : ObservableObject
    {
        public FlightRecorderViewModel() { }
        public FlightRecorderViewModel(PlaneModel planeModel)
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

    public class FlightRecorderAnaliseViewModel : ObservableObject
    {
        private double _averageFuelConsumption;
        public double AverageFuelConsumption
        {
            get { return _averageFuelConsumption; }
            set { _averageFuelConsumption = value; OnPropertyChanged(); OnPropertyChanged("AverageFuelConsumptioText"); }
        }

        public string AverageFuelConsumptioText 
        {
            get { return $"{string.Format("{0:N0}", AverageFuelConsumption)} Kg/NM"; }
        }

        private double _averagePlaneSpeed;
        public double AveragePlaneSpeed
        {
            get { return _averagePlaneSpeed; }
            set { _averagePlaneSpeed = value; OnPropertyChanged();  OnPropertyChanged("AveragePlaneSpeedText"); }
        }

        public string AveragePlaneSpeedText
        {
            get { return $"{string.Format("{0:N0}", AveragePlaneSpeed)} Kts"; }
        }
    }
}
