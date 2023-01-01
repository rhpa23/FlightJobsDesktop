using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Airlinedbmodel
    {
        public Airlinedbmodel()
        {
            Airlinecertificatesdbmodels = new HashSet<Airlinecertificatesdbmodel>();
            Airlinefbodbmodels = new HashSet<Airlinefbodbmodel>();
            Jobairlinedbmodels = new HashSet<Jobairlinedbmodel>();
            Statisticsdbmodels = new HashSet<Statisticsdbmodel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public long Salary { get; set; }
        public long Score { get; set; }
        public string Logo { get; set; }
        public long BankBalance { get; set; }
        public long AirlineScore { get; set; }
        public string UserId { get; set; }
        public long DebtValue { get; set; }

        public virtual ICollection<Airlinecertificatesdbmodel> Airlinecertificatesdbmodels { get; set; }
        public virtual ICollection<Airlinefbodbmodel> Airlinefbodbmodels { get; set; }
        public virtual ICollection<Jobairlinedbmodel> Jobairlinedbmodels { get; set; }
        public virtual ICollection<Statisticsdbmodel> Statisticsdbmodels { get; set; }
    }
}
