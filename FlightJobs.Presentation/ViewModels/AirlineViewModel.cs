using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class AirlineViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public string OwnerUserName { get; set; }
        public int HiredPilotsNumber { get; set; }
        public int HiredFobsNumber { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public long AirlineScore { get; set; }
        public long BankBalance { get; set; }
        public int MinimumScoreToHire { get; set; }
        public long DebtValue { get; set; }
        public string DebtValueFormated
        {
            get { return string.Format("F{0:C}", DebtValue);  }
        }
        public string BankBalanceFormated
        {
            get { return string.Format("F{0:C}", BankBalance); }
        }

        public string DebtColor
        {
            get { return DebtValue > 0 ? "Red" : "Green"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
