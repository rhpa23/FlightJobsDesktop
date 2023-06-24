using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class PilotLicenseExpensesModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int DaysMaturity { get; set; }

        public bool Mandatory { get; set; }
    }
}
