using System;
using System.Collections.Generic;


namespace FlightJobs.Domain.Entities
{
    public partial class Airlinecertificatesdbmodel
    {
        public int Id { get; set; }
        public int? AirlineId { get; set; }
        public int? CertificateId { get; set; }

        public virtual Airlinedbmodel Airline { get; set; }
        public virtual Certificatedbmodel Certificate { get; set; }
    }
}
