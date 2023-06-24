using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class ChartUserBankBalanceModel
    {
        public Dictionary<string, double> Data { get; set; }

        public double PayamentTotal { get; set; }

        public double PayamentMonthGoal { get; set; }
    }
}
