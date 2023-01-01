using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Statisticcertificatesdbmodel
    {
        public int Id { get; set; }
        public int? CertificateId { get; set; }
        public int? StatisticId { get; set; }

        public virtual Certificatedbmodel Certificate { get; set; }
        public virtual Statisticsdbmodel Statistic { get; set; }
    }
}
