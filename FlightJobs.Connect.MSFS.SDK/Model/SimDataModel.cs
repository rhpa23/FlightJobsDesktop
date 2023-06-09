using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model
{
    public class SimDataModel : ObservableObject
    {
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; NotifyPropertyChanged("IsConnectedText"); IsConnectedText = value ? "MSFS is connected" : "Waiting for sim start..."; }
        }
        public string IsConnectedText { get; set; }

        private double _seaLevelPressureMillibars;
        public double SeaLevelPressureMillibars
        {
            get { return _seaLevelPressureMillibars; }
            set { _seaLevelPressureMillibars = value; 
                SeaLevelPressureMillibarsText = $"{Math.Round(value, 0)} hPa";
                SeaLevelPressureInchesText = $"{(Math.Round(value, 0) * 0.02953).ToString("0.00")} inHg";
                NotifyPropertyChanged("SeaLevelPressureMillibarsText");
                NotifyPropertyChanged("SeaLevelPressureInchesText");
            }
        }
        public string SeaLevelPressureMillibarsText { get; set; }
        public string SeaLevelPressureInchesText { get; set; }

        private double _windDirectionDegrees;
        public double WindDirectionDegrees
        {
            get { return _windDirectionDegrees; }
            set { _windDirectionDegrees = value; WindDirectionDegreesText = $"{Math.Round(value, 0)}º"; NotifyPropertyChanged("WindDirectionDegreesText"); }
        }
        public string WindDirectionDegreesText { get; set; }

        private double _windVelocityKnots;
        public double WindVelocityKnots
        {
            get { return _windVelocityKnots; }
            set { _windVelocityKnots = value; WindVelocityKnotsText = $"{Math.Round(value, 0)} Kts"; NotifyPropertyChanged("WindVelocityKnotsText"); }
        }
        public string WindVelocityKnotsText { get; set; }

        private double _temperatureCelsius;
        public double TemperatureCelsius
        {
            get { return _temperatureCelsius; }
            set { _temperatureCelsius = value; TemperatureCelsiusText = $"{Math.Round(value, 0)}º"; NotifyPropertyChanged("TemperatureCelsiusText"); }
        }
        public string TemperatureCelsiusText { get; set; }

        private double _visibilityMeters;
        public double VisibilityMeters
        {
            get { return _visibilityMeters; }
            set { _visibilityMeters = value; VisibilityMetersText = $"{Math.Round(value, 0)} meters";  NotifyPropertyChanged("VisibilityMetersText"); }
        }
        public string VisibilityMetersText { get; set; }

    }
}
