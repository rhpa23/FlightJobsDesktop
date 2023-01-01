using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Pilotlicenseexpensesdbmodel
    {
        public Pilotlicenseexpensesdbmodel()
        {
            Pilotlicenseexpensesuserdbmodels = new HashSet<Pilotlicenseexpensesuserdbmodel>();
            Pilotlicenseitemdbmodels = new HashSet<Pilotlicenseitemdbmodel>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public int DaysMaturity { get; set; }
        public bool Mandatory { get; set; }

        public virtual ICollection<Pilotlicenseexpensesuserdbmodel> Pilotlicenseexpensesuserdbmodels { get; set; }
        public virtual ICollection<Pilotlicenseitemdbmodel> Pilotlicenseitemdbmodels { get; set; }
    }
}
