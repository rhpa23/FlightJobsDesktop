using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class MessagesResultViewModel : ObservableObject
    {
        private string _touchdownScore;
        public string TouchdownScore 
        {
            get { return _touchdownScore; }
            set
            {
                _touchdownScore = value; OnPropertyChanged();
            }
        }

        private string _gForceScore;
        public string GForceScore
        {
            get { return _gForceScore; }
            set
            {
                _gForceScore = value; OnPropertyChanged();
            }
        }

        private string _bounceScore;
        public string BounceScore
        {
            get { return _bounceScore; }
            set
            {
                _bounceScore = value; OnPropertyChanged();
            }
        }

        private string _landingDerivationScore;
        public string LandingDerivationScore
        {
            get { return _landingDerivationScore; }
            set
            {
                _landingDerivationScore = value; OnPropertyChanged();
            }
        }

        private string _landingDistanceScore;
        public string LandingDistanceScore
        {
            get { return _landingDistanceScore; }
            set
            {
                _landingDistanceScore = value; OnPropertyChanged();
            }
        }

        private string _takeoffDerivationScore;
        public string TakeoffDerivationScore
        {
            get { return _takeoffDerivationScore; }
            set
            {
                _takeoffDerivationScore = value; OnPropertyChanged();
            }
        }

        private string _upwindLandingScore;
        public string UpwindLandingScore
        {
            get { return _upwindLandingScore; }
            set
            {
                _upwindLandingScore = value; OnPropertyChanged();
            }
        }

        private string _beaconLightsScore;
        public string BeaconLightsScore
        {
            get { return _beaconLightsScore; }
            set
            {
                _beaconLightsScore = value; OnPropertyChanged();
            }
        }

        private string _landingLightsScore;
        public string LandingLightsScore
        {
            get { return _landingLightsScore; }
            set
            {
                _landingLightsScore = value; OnPropertyChanged();
            }
        }

        private string _navegationLightsScore;
        public string NavegationLightsScore
        {
            get { return _navegationLightsScore; }
            set
            {
                _navegationLightsScore = value; OnPropertyChanged();
            }
        }

        private string _altimeterScore;
        public string AltimeterScore
        {
            get { return _altimeterScore; }
            set
            {
                _altimeterScore = value; OnPropertyChanged();
            }
        }

        private string _totalScore;
        public string TotalScore
        {
            get { return _totalScore; }
            set
            {
                _totalScore = value; OnPropertyChanged();
            }
        }
    }
}
