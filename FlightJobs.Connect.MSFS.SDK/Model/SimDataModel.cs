﻿using System;
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
                SeaLevelPressureText = $"{Math.Round(value, 0)} hPa / {(Math.Round(value, 0) * 0.02953).ToString("0.00")}";
                NotifyPropertyChanged("SeaLevelPressureText");
            }
        }
        public string SeaLevelPressureText { get; set; }

        private double _windDirectionDegrees;
        public double WindDirectionDegrees
        {
            get { return _windDirectionDegrees; }
            set { _windDirectionDegrees = value;
                WindDirectionAndSpeedText = $"{Math.Round(value, 0)}º / {Math.Round(WindVelocityKnots, 0)} Kts"; 
                NotifyPropertyChanged("WindDirectionAndSpeedText"); }
        }
        public string WindDirectionAndSpeedText { get; set; }

        private double _windVelocityKnots;
        public double WindVelocityKnots
        {
            get { return _windVelocityKnots; }
            set { _windVelocityKnots = value;
                WindDirectionAndSpeedText = $"{Math.Round(WindDirectionDegrees, 0)}º / {Math.Round(value, 0)} Kts";
                NotifyPropertyChanged("WindDirectionAndSpeedText"); }
        }

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

        private double _fps { get; set; }
        public double FPS
        {
            get { return _fps; }
            set { _fps = Math.Round(value, 0); NotifyPropertyChanged(); }
        }

        private double _simulationSpeed { get; set; }
        public double SimulationSpeed
        {
            get { return _simulationSpeed; }
            set { _simulationSpeed = value; NotifyPropertyChanged(); }
        }

    }
}
