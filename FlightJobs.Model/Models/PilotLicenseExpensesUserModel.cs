using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class PilotLicenseExpensesUserModel
    {
        public long Id { get; set; }

        public PilotLicenseExpensesModel PilotLicenseExpense { get; set; }

        public DateTime MaturityDate { get; set; }

        public bool OverdueProcessed { get; set; }

        public IList<LicenseItemModel> LicenseItems { get; set; } = new List<LicenseItemModel>();
    }
}
