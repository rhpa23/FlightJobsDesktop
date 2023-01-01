using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Jobairlinedbmodel
    {
        public long Id { get; set; }
        public long JobDebtValue { get; set; }
        public int? AirlineId { get; set; }
        public int? JobId { get; set; }

        public virtual Airlinedbmodel Airline { get; set; }
        public virtual Jobdbmodel Job { get; set; }
    }
}
