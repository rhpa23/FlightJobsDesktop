using System;

namespace FlightJobs.Model.Models
{
    public class AirlineModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        //[Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        public long Salary { get; set; }

        public long Score { get; set; }

        public string Logo { get; set; }

        public long BankBalance { get; set; }

        public long AirlineScore { get; set; }

        public bool AlowEdit { get; set; }

        public bool AlowExit { get; set; }

        public string UserId { get; set; }

        //public StatisticsDbModel OwnerUserStatistics { get; set; }

        public long DebtValue { get; set; }

        public DateTime DebtMaturityDate { get; set; }

        //public List<AirlineFboDbModel> HiredFBOs { get; internal set; }
    }
}
