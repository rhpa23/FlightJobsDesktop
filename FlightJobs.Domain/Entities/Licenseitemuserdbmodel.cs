using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Licenseitemuserdbmodel
    {
        public long Id { get; set; }
        public bool IsBought { get; set; }
        public long? PilotLicenseItemId { get; set; }
        public string UserId { get; set; }

        public virtual Pilotlicenseitemdbmodel PilotLicenseItem { get; set; }
        public virtual Aspnetuser User { get; set; }
    }
}
