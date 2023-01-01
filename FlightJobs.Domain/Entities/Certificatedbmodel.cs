using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Certificatedbmodel
    {
        public Certificatedbmodel()
        {
            Airlinecertificatesdbmodels = new HashSet<Airlinecertificatesdbmodel>();
            Statisticcertificatesdbmodels = new HashSet<Statisticcertificatesdbmodel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public long Score { get; set; }
        public string Logo { get; set; }

        public virtual ICollection<Airlinecertificatesdbmodel> Airlinecertificatesdbmodels { get; set; }
        public virtual ICollection<Statisticcertificatesdbmodel> Statisticcertificatesdbmodels { get; set; }
    }
}
