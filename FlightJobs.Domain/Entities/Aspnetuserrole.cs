using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Aspnetuserrole
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public virtual Aspnetrole Role { get; set; }
        public virtual Aspnetuser User { get; set; }
    }
}
