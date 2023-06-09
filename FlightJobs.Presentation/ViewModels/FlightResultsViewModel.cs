using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlightJobsDesktop.ViewModels
{
    public class FlightResultsViewModel : ObservableObject
    {
        private Visibility _resultBeaconLightVisibility;
        public Visibility ResultBeaconLightVisibility
        {
            get { return _resultBeaconLightVisibility; }
            set { _resultBeaconLightVisibility = value; OnPropertyChanged("ResultBeaconLightVisibility"); }
        }

        private Visibility _resultLandingLightVisibility;
        public Visibility ResultLandingLightVisibility
        {
            get { return _resultLandingLightVisibility; }
            set { _resultLandingLightVisibility = value; OnPropertyChanged("ResultLandingLightVisibility"); }
        }

        private Visibility _resultNavigationLightVisibility;
        public Visibility ResultNavigationLightVisibility
        {
            get { return _resultNavigationLightVisibility; }
            set { _resultNavigationLightVisibility = value; OnPropertyChanged("ResultNavigationLightVisibility"); }
        }

        private Visibility _resultAltimeterSettingVisibility;
        public Visibility ResultAltimeterSettingVisibility
        {
            get { return _resultAltimeterSettingVisibility; }
            set { _resultAltimeterSettingVisibility = value; OnPropertyChanged("ResultAltimeterSettingVisibility"); }
        }
    }
}
