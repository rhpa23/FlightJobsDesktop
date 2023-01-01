using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Jobdbmodel
    {
        public Jobdbmodel()
        {
            Jobairlinedbmodels = new HashSet<Jobairlinedbmodel>();
        }

        public int Id { get; set; }
        public int PaxWeight { get; set; }
        public string DepartureIcao { get; set; }
        public string ArrivalIcao { get; set; }
        public string AlternativeIcao { get; set; }
        public long Dist { get; set; }
        public long Pax { get; set; }
        public long Cargo { get; set; }
        public long Pay { get; set; }
        public bool FirstClass { get; set; }
        public bool IsDone { get; set; }
        public bool IsActivated { get; set; }
        public bool InProgress { get; set; }
        public string ModelName { get; set; }
        public string ModelDescription { get; set; }
        public long StartFuelWeight { get; set; }
        public long FinishFuelWeight { get; set; }
        public int AviationType { get; set; }
        public string VideoUrl { get; set; }
        public string VideoDescription { get; set; }
        public string ChallengeCreatorUserId { get; set; }
        public bool IsChallenge { get; set; }
        public int ChallengeType { get; set; }
        public string UserId { get; set; }

        public virtual Aspnetuser User { get; set; }
        public virtual ICollection<Jobairlinedbmodel> Jobairlinedbmodels { get; set; }
    }
}
