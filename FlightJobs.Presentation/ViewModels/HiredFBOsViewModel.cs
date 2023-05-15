using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class HiredFBOsViewModel
    {
        public string AirlineName { get; set; }
        public IList<AirlineFboViewModel> HiredFBOs { get; set; }
        public AirlineFboViewFilterModel Filter { get; set; } = new AirlineFboViewFilterModel();

    }

    public class AirlineFboViewFilterModel : INotifyPropertyChanged
    {
        public string Icao { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
