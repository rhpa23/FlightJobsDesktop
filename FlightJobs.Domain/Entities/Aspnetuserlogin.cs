using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Aspnetuserlogin
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string UserId { get; set; }

        public virtual Aspnetuser User { get; set; }
    }
}
