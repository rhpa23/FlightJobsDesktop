using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Pilotlicenseitemdbmodel
    {
        public Pilotlicenseitemdbmodel()
        {
            Licenseitemuserdbmodels = new HashSet<Licenseitemuserdbmodel>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public string Image { get; set; }
        public long? PilotLicenseExpenseId { get; set; }

        public virtual Pilotlicenseexpensesdbmodel PilotLicenseExpense { get; set; }
        public virtual ICollection<Licenseitemuserdbmodel> Licenseitemuserdbmodels { get; set; }
    }
}
