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

        private double _payloadPounds;
        public double PayloadPounds
        {
            get { return _payloadPounds; }
            set { _payloadPounds = value; PayloadPoundsText = $"{string.Format("{0:N0}", value)} Lb"; NotifyPropertyChanged("PayloadPoundsText"); }
        }
        public string PayloadPoundsText { get; set; }

        private double _fuelWeightPounds;
        public double FuelWeightPounds
        {
            get { return _fuelWeightPounds; }
            set { _fuelWeightPounds = value; FuelWeightPoundsText = $"{string.Format("{0:N0}", value)} Lb"; NotifyPropertyChanged("FuelWeightPoundsText"); }
        }
        public string FuelWeightPoundsText { get; set; }

        private double _payloadKilograms;
        public double PayloadKilograms
        {
            get { return _payloadKilograms; }
            set { _payloadKilograms = value; PayloadKilogramsText = $"{string.Format("{0:N0}", value)} Kg"; NotifyPropertyChanged("PayloadKilogramsText"); }
        }
        public string PayloadKilogramsText { get; set; }

        private double _fuelWeightKilograms;
        public double FuelWeightKilograms
        {
            get { return _fuelWeightKilograms; }
            set { _fuelWeightKilograms = value; FuelWeightKilogramsText = $"{string.Format("{0:N0}", value)} Kg"; NotifyPropertyChanged("FuelWeightKilogramsText"); }
        }
        public string FuelWeightKilogramsText { get; set; }
    }
}
