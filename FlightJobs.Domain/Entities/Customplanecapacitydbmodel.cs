using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Customplanecapacitydbmodel
    {
        public Customplanecapacitydbmodel()
        {
            Statisticsdbmodels = new HashSet<Statisticsdbmodel>();
        }

        public long Id { get; set; }
        public int CustomPassengerCapacity { get; set; }
        public int CustomCargoCapacityWeight { get; set; }
        public string CustomNameCapacity { get; set; }
        public long CustomPaxWeight { get; set; }
        public string UserId { get; set; }

        public virtual Aspnetuser User { get; set; }
        public virtual ICollection<Statisticsdbmodel> Statisticsdbmodels { get; set; }
    }
}
