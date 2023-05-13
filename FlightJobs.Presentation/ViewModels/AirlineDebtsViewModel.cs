using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class AirlineDebtsViewModel
    {
        public int Id { get; set; }
        public long DebtValue { get; set; }
        public string DebtComplete { get { return string.Format("F{0:C}", DebtValue); } }
        public DateTime DebtMaturityDate { get; set; }
        public string MaturityDateComplete { get { return string.Format("{0:dd-MMM-yyyy}", DebtMaturityDate); } }
        public long BankBalance { get; set; }
        public string BankBalanceComplete { get { return string.Format("F{0:C}", BankBalance); } }
        public long BankBalanceForecast { get; set; }
        public string BankBalanceForecastComplete { get { return string.Format("F{0:C}", BankBalanceForecast); } }
    }
}
