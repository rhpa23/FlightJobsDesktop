using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Aspnetrole
    {
        public Aspnetrole()
        {
            Aspnetuserroles = new HashSet<Aspnetuserrole>();
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Aspnetuserrole> Aspnetuserroles { get; set; }
    }
}
