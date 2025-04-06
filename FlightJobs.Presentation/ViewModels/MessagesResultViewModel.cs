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
        public string TouchdownScoreTitle 
        {
            get { return _touchdownScore; }
            set
            {
                _touchdownScore = value; OnPropertyChanged();
            }
        }

        private string _gForceScore;
        public string GForceScoreTitle
        {
            get { return _gForceScore; }
            set
            {
                _gForceScore = value; OnPropertyChanged();
            }
        }

        private string _bounceScore;
        public string BounceScoreTitle
        {
            get { return _bounceScore; }
            set
            {
                _bounceScore = value; OnPropertyChanged();
            }
        }

        private string _landingDerivationScore;
        public string LandingDerivationScoreTitle
        {
            get { return _landingDerivationScore; }
            set
            {
                _landingDerivationScore = value; OnPropertyChanged();
            }
        }

        private string _landingDistanceScore;
        public string LandingDistanceScoreTitle
        {
            get { return _landingDistanceScore; }
            set
            {
                _landingDistanceScore = value; OnPropertyChanged();
            }
        }

        private string _takeoffDerivationScore;
        public string TakeoffDerivationScoreTitle
        {
            get { return _takeoffDerivationScore; }
            set
            {
                _takeoffDerivationScore = value; OnPropertyChanged();
            }
        }

        private string _upwindLandingScore;
        public string UpwindLandingScoreTitle
        {
            get { return _upwindLandingScore; }
            set
            {
                _upwindLandingScore = value; OnPropertyChanged();
            }
        }

        private string _beaconLightsScore;
        public string BeaconLightsScoreTitle
        {
            get { return _beaconLightsScore; }
            set
            {
                _beaconLightsScore = value; OnPropertyChanged();
            }
        }

        private string _landingLightsScore;
        public string LandingLightsScoreTitle
        {
            get { return _landingLightsScore; }
            set
            {
                _landingLightsScore = value; OnPropertyChanged();
            }
        }

        private string _navegationLightsScore;
        public string NavegationLightsScoreTitle
        {
            get { return _navegationLightsScore; }
            set
            {
                _navegationLightsScore = value; OnPropertyChanged();
            }
        }

        private string _altimeterScore;
        public string AltimeterScoreTitle
        {
            get { return _altimeterScore; }
            set
            {
                _altimeterScore = value; OnPropertyChanged();
            }
        }

        private string _totalScore;
        public string TotalScoreTitle
        {
            get { return _totalScore; }
            set
            {
                _totalScore = value; OnPropertyChanged();
            }
        }

        /// SUB_TITLES
        /// 
        private string _touchdownScoreSubTitle;
        public string TouchdownScoreSubTitle
        {
            get { return _touchdownScoreSubTitle; }
            set
            {
                _touchdownScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _gForceScoreSubTitle;
        public string GForceScoreSubTitle
        {
            get { return _gForceScoreSubTitle; }
            set
            {
                _gForceScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _bounceScoreSubTitle;
        public string BounceScoreSubTitle
        {
            get { return _bounceScoreSubTitle; }
            set
            {
                _bounceScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _landingDerivationScoreSubTitle;
        public string LandingDerivationScoreSubTitle
        {
            get { return _landingDerivationScoreSubTitle; }
            set
            {
                _landingDerivationScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _landingDistanceScoreSubTitle;
        public string LandingDistanceScoreSubTitle
        {
            get { return _landingDistanceScoreSubTitle; }
            set
            {
                _landingDistanceScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _takeoffDerivationScoreSubTitle;
        public string TakeoffDerivationScoreSubTitle
        {
            get { return _takeoffDerivationScoreSubTitle; }
            set
            {
                _takeoffDerivationScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _upwindLandingScoreSubTitle;
        public string UpwindLandingScoreSubTitle
        {
            get { return _upwindLandingScoreSubTitle; }
            set
            {
                _upwindLandingScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _beaconLightsScoreSubTitle;
        public string BeaconLightsScoreSubTitle
        {
            get { return _beaconLightsScoreSubTitle; }
            set
            {
                _beaconLightsScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _landingLightsScoreSubTitle;
        public string LandingLightsScoreSubTitle
        {
            get { return _landingLightsScoreSubTitle; }
            set
            {
                _landingLightsScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _navegationLightsScoreSubTitle;
        public string NavegationLightsScoreSubTitle
        {
            get { return _navegationLightsScoreSubTitle; }
            set
            {
                _navegationLightsScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _altimeterScoreSubTitle;
        public string AltimeterScoreSubTitle
        {
            get { return _altimeterScoreSubTitle; }
            set
            {
                _altimeterScoreSubTitle = value; OnPropertyChanged();
            }
        }

        private string _totalScoreSubTitle;
        public string TotalScoreSubTitle
        {
            get { return _totalScoreSubTitle; }
            set
            {
                _totalScoreSubTitle = value; OnPropertyChanged();
            }
        }
    }
}
