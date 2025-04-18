﻿using FlightJobs.Connect.MSFS.SDK.Model.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model
{
    public class PlaneModel : ObservableObject
    {
        public bool EngOneRunning { get; set; }
        public bool OnGround { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged("Name"); }
        }

        private long _currentAltitude;
        public long CurrentAltitude
        {
            get { return _currentAltitude; }
            set { _currentAltitude = value; NotifyPropertyChanged("CurrentAltitude"); }
        }

        public int GroundSpeed { get; set; }

        private double _latitude;
        public double Latitude
        {
            get { return _latitude; }
            set { _latitude = value; NotifyPropertyChanged("Latitude"); }
        }

        private double _longitude;
        public double Longitude
        {
            get { return _longitude; }
            set { _longitude = value; NotifyPropertyChanged("Longitude"); }
        }
        public bool LightLandingOn { get; set; }
        public bool LightBeaconOn { get; set; }
        public bool LightNavigationOn { get; set; }
        public int AltimeterInMillibars { get; set; }

        private double _payloadPounds;
        public double PayloadPounds
        {
            get { return _payloadPounds; }
            set { _payloadPounds = value; PayloadPoundsText = $"{string.Format("{0:N0}", value)} Lb";
                NotifyPropertysChanged("PayloadPoundsText", "PayloadPoundsAndKilogramsText"); }
        }
        public string PayloadPoundsText { get; set; }
        public string PayloadPoundsAndKilogramsText {
            get { return string.IsNullOrEmpty(PayloadPoundsText) || string.IsNullOrEmpty(PayloadKilogramsText) ? "" : $"{PayloadPoundsText} / {PayloadKilogramsText}"; } }

        private double _fuelWeightPounds;
        public double FuelWeightPounds
        {
            get { return _fuelWeightPounds; }
            set { _fuelWeightPounds = value; FuelWeightPoundsText = $"{string.Format("{0:N0}", value)} Lb";
                NotifyPropertysChanged("FuelWeightPoundsText", "FuelWeightPoundsAndKilogramsText"); }
        }
        public string FuelWeightPoundsAndKilogramsText {
            get { return string.IsNullOrEmpty(FuelWeightPoundsText) || string.IsNullOrEmpty(FuelWeightKilogramsText) ? "" : $"{FuelWeightPoundsText} / {FuelWeightKilogramsText}"; } }
        public string FuelWeightPoundsText { get; set; }

        private double _payloadKilograms;
        public double PayloadKilograms
        {
            get { return _payloadKilograms; }
            set { _payloadKilograms = value; PayloadKilogramsText = $"{string.Format("{0:N0}", value)} Kg";
                NotifyPropertysChanged("PayloadKilogramsText", "PayloadPoundsAndKilogramsText"); }
        }
        public string PayloadKilogramsText { get; set; }

        private double _fuelWeightKilograms;
        public double FuelWeightKilograms
        {
            get { return _fuelWeightKilograms; }
            set { _fuelWeightKilograms = value; FuelWeightKilogramsText = $"{string.Format("{0:N0}", value)} Kg";
                NotifyPropertysChanged("FuelWeightKilogramsText", "FuelWeightPoundsAndKilogramsText"); }
        }
        public string FuelWeightKilogramsText { get; set; }

        private int _touchdownFpm;
        public int TouchdownFpm
        {
            get { return _touchdownFpm; }
            set { _touchdownFpm = value;
                NotifyPropertysChanged("TouchdownFpm", "ScoreTouchdown", "ColorResultTouchdownFpm"); }
        }

        private int _touchdownBounceCount;
        public int TouchdownBounceCount
        {
            get { return _touchdownBounceCount; }
            set { _touchdownBounceCount = value; NotifyPropertysChanged("TouchdownBounceCount", "ScoreBounce", "ColorResultBounceCount"); }
        }

        private double _touchdownAirspeed;
        public double TouchdownAirspeed
        {
            get { return _touchdownAirspeed; }
            set { _touchdownAirspeed = value; TouchdownAirspeedText = $"{_touchdownAirspeed} kts"; NotifyPropertyChanged("TouchdownAirspeedText"); }
        }
        public string TouchdownAirspeedText { get; set; }


        private double _touchdownGroundspeed;
        public double TouchdownGroundspeed
        {
            get { return _touchdownGroundspeed; }
            set { _touchdownGroundspeed = value; TouchdownGroundspeedText = $"{_touchdownGroundspeed} kts"; NotifyPropertyChanged("TouchdownGroundspeedText"); }
        }
        public string TouchdownGroundspeedText { get; set; }

        private double _touchdownwindSpeed;
        public double TouchdownWindSpeed
        {
            get { return _touchdownwindSpeed; }
            set
            {
                _touchdownwindSpeed = value; TouchdownWindSpeedText = $"{Math.Abs(_touchdownwindSpeed)} kts";
                NotifyPropertyChanged("TouchdownWindSpeedText");
                NotifyPropertyChanged("ColorResultTouchdownWindSpeed");
            }
        }
        public string TouchdownWindSpeedText { get; set; }

        private double _touchdownCrosswind;
        public double TouchdownCrosswind
        {
            get { return _touchdownCrosswind; }
            set { _touchdownCrosswind = value;
                NotifyPropertysChanged("TouchdownCrosswindText", "ScoreWindAngle", "ColorResultTouchdownWindAngle");
            }
        }

        private double _touchdownHeadwind;
        public double TouchdownHeadwind
        {
            get { return _touchdownHeadwind; }
            set { _touchdownHeadwind = value; TouchdownHeadwindText = $"{Math.Abs(_touchdownHeadwind)} kts";
                NotifyPropertysChanged("TouchdownHeadwindText", "ScoreWindAngle", "TouchdownWindAngle");
            }
        }
        public string TouchdownHeadwindText { get; set; }

        public int TouchdownWindAngle
        {
            get
            {
                double windangle = Math.Atan2(TouchdownCrosswind, TouchdownHeadwind) * 180 / Math.PI;
                return Convert.ToInt32(windangle);
            }
        }

        private double _touchdownGForce;
        public double TouchdownGForce
        {
            get { return _touchdownGForce; }
            set { _touchdownGForce = value; TouchdownGForceText = $"{_touchdownGForce}G";
                NotifyPropertysChanged("TouchdownGForceText", "ScoreGForce", "ColorResultGForce"); }
        }
        public string TouchdownGForceText { get; set; }


        private double _planeTouchdownLatitude;
        public double TouchdownLatitude
        {
            get { return _planeTouchdownLatitude; }
            set
            {
                _planeTouchdownLatitude = value; NotifyPropertyChanged("TouchdownLatitude");
            }
        }

        private double _planeTouchdownLongitude;
        public double TouchdownLongitude
        {
            get { return _planeTouchdownLongitude; }
            set
            {
                _planeTouchdownLongitude = value; NotifyPropertyChanged("TouchdownLongitude");
            }
        }

        private double _centerLineDerivation;
        public double TouchdownCenterDerivation
        {
            get { return _centerLineDerivation; }
            set
            {
                _centerLineDerivation = value;
                NotifyPropertysChanged("TouchdownCenterDerivation", "ScoreCenterDerivation", "ColorResultCenterDerivation");
            }
        }

        private double _thresholdDistance;
        public double TouchdownThresholdDistance
        {
            get { return _thresholdDistance; }
            set
            {
                _thresholdDistance = value;
                NotifyPropertysChanged("TouchdownThresholdDistance", "ScoreLandDistance", "ColorResultLandDistance");
            }
        }

        private double _headingTrue;
        public double HeadingTrue
        {
            get { return _headingTrue; }
            set
            {
                _headingTrue = value; NotifyPropertyChanged("HeadingTrue");
            }
        }

        private string _touchdownRunwayDesignator;
        public string TouchdownRunwayDesignator
        {
            get { return _touchdownRunwayDesignator; }
            set
            {
                _touchdownRunwayDesignator = value; NotifyPropertyChanged("TouchdownRunwayDesignator");
            }
        }

        public double TouchdownRunwayLength { get; set; }


        private double _takeoffLatitude;
        public double TakeoffLatitude
        {
            get { return _takeoffLatitude; }
            set
            {
                _takeoffLatitude = value; NotifyPropertyChanged("TakeoffLatitude");
            }
        }

        private double _takeoffLongitude;
        public double TakeoffLongitude
        {
            get { return _takeoffLongitude; }
            set
            {
                _takeoffLongitude = value; NotifyPropertyChanged("TakeoffLongitude");
            }
        }

        private double _centerLineTakeoffDerivation;
        public double TakeoffCenterDerivation
        {
            get { return _centerLineTakeoffDerivation; }
            set
            {
                _centerLineTakeoffDerivation = value;
                NotifyPropertysChanged("TakeoffCenterDerivation", "ScoreTakeoffCenterDerivation", "ColorResultTakeoffCenterDerivation");
            }
        }

        public string ColorResultTakeoffCenterDerivation
        {
            get { return CenterDerivationResult.GetColor(TakeoffCenterDerivation, 0); }
        }

        public string ColorResultGForce
        {
            get { return GForceResult.GetColor(TouchdownGForce); }
        }

        public int ScoreGForce
        {
            get { return GForceResult.GetScore(TouchdownGForce); }
        }

        public string ColorResultTouchdownFpm
        {
            get { return TouchdownResult.GetColor(TouchdownFpm); }
        }

        public int ScoreTouchdown
        {
            get { return TouchdownResult.GetScore(TouchdownFpm); }
        }

        public string ColorResultBounceCount
        {
            get { return BounceResult.GetColor(TouchdownBounceCount); }
        }

        public int ScoreBounce
        {
            get { return BounceResult.GetScore(TouchdownBounceCount); }
        }

        public string ColorResultTouchdownWindSpeed
        {
            get { return WindSpeedResult.GetColor(TouchdownWindSpeed); }
        }

        private string _colorResultTouchdownWindAngle;
        public string ColorResultTouchdownWindAngle
        {
            get { return _colorResultTouchdownWindAngle; }
            set
            {
                _colorResultTouchdownWindAngle = value; NotifyPropertyChanged("ColorResultTouchdownWindAngle");
            }
        }

        public int ScoreWindAngle
        {
            get { return WindAngleResult.GetScore(TouchdownWindAngle, TouchdownWindSpeed); }
        }

        public string ColorResultCenterDerivation
        {
            get { return CenterDerivationResult.GetColor(TouchdownCenterDerivation, TouchdownWindSpeed); }
        }

        public int ScoreCenterDerivation
        {
            get { return CenterDerivationResult.GetScore(TouchdownCenterDerivation, TouchdownWindSpeed); }
        }
        public int ScoreTakeoffCenterDerivation
        {
            get { return CenterDerivationResult.GetScore(TakeoffCenterDerivation, 0); }
        }
        public string ColorResultLandDistance
        {
            get { return LandDistanceResult.GetColor(TouchdownThresholdDistance, TouchdownRunwayLength); }
        }

        public int ScoreLandDistance
        {
            get { return LandDistanceResult.GetScore(TouchdownThresholdDistance, TouchdownRunwayLength); }
        }

        private int _scoreLightLandingOn;
        public int ScoreLightLandingOn
        {
            get { return _scoreLightLandingOn; }
            set
            {
                _scoreLightLandingOn = value; NotifyPropertyChanged();
            }
        }

        private int _scoreLightBeaconOn;
        public int ScoreLightBeaconOn
        {
            get { return _scoreLightBeaconOn; }
            set
            {
                _scoreLightBeaconOn = value; NotifyPropertyChanged();
            }
        }

        private int _scoreLightNavigationOn;
        public int ScoreLightNavigationOn
        {
            get { return _scoreLightNavigationOn; }
            set
            {
                _scoreLightNavigationOn = value; NotifyPropertyChanged();
            }
        }

        private int _scoreAltimeterSettings;
        public int ScoreAltimeterSettings
        {
            get { return _scoreAltimeterSettings; }
            set
            {
                _scoreAltimeterSettings = value; NotifyPropertyChanged();
            }
        }
    }
}
