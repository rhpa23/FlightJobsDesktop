using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Statisticsdbmodel
    {
        public Statisticsdbmodel()
        {
            Statisticcertificatesdbmodels = new HashSet<Statisticcertificatesdbmodel>();
        }

        public int Id { get; set; }
        public long BankBalance { get; set; }
        public long PilotScore { get; set; }
        public string Logo { get; set; }
        public bool SendLicenseWarning { get; set; }
        public bool SendAirlineBillsWarning { get; set; }
        public bool LicenseWarningSent { get; set; }
        public bool AirlineBillsWarningSent { get; set; }
        public bool UseCustomPlaneCapacity { get; set; }
        public string WeightUnit { get; set; }
        public int? AirlineId { get; set; }
        public long? CustomPlaneCapacityId { get; set; }
        public string UserId { get; set; }

        public virtual Airlinedbmodel Airline { get; set; }
        public virtual Customplanecapacitydbmodel CustomPlaneCapacity { get; set; }
        public virtual Aspnetuser User { get; set; }
        public virtual ICollection<Statisticcertificatesdbmodel> Statisticcertificatesdbmodels { get; set; }
    }
}
