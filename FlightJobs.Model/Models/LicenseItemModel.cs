using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class LicenseItemModel
    {
        public long Id { get; set; }

        public PilotLicenseItemModel PilotLicenseItem { get; set; }

        public bool IsBought { get; set; }
    }

    public class PilotLicenseItemModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long Price { get; set; }

        public string Image { get; set; }
    }
}
