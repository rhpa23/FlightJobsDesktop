using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Aspnetuser
    {
        public Aspnetuser()
        {
            Aspnetuserlogins = new HashSet<Aspnetuserlogin>();
            Customplanecapacitydbmodels = new HashSet<Customplanecapacitydbmodel>();
            Jobdbmodels = new HashSet<Jobdbmodel>();
            Statisticsdbmodels = new HashSet<Statisticsdbmodel>();
        }

        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }

        public virtual ICollection<Aspnetuserlogin> Aspnetuserlogins { get; set; }
        public virtual ICollection<Customplanecapacitydbmodel> Customplanecapacitydbmodels { get; set; }
        public virtual ICollection<Jobdbmodel> Jobdbmodels { get; set; }
        public virtual ICollection<Statisticsdbmodel> Statisticsdbmodels { get; set; }
    }
}
