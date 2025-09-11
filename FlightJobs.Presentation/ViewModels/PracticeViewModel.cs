using System.ComponentModel;

namespace FlightJobsDesktop.ViewModels
{
    public class PracticeViewModel : INotifyPropertyChanged
    {
        public string ArrivalICAO { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
