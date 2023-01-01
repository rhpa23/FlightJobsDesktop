using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Pilotlicenseexpensesuserdbmodel
    {
        public long Id { get; set; }
        public bool OverdueProcessed { get; set; }
        public long? PilotLicenseExpenseId { get; set; }
        public string UserId { get; set; }

        public virtual Pilotlicenseexpensesdbmodel PilotLicenseExpense { get; set; }
        public virtual Aspnetuser User { get; set; }
    }
}
