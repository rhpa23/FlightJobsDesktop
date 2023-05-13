using System;
using System.Collections.Generic;

namespace FlightJobs.Model.Models
{
    public class AirlineModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string Country { get; set; }

        public long Salary { get; set; }

        public long Score { get; set; }

        public string Logo { get; set; }

        public long BankBalance { get; set; }

        public long AirlineScore { get; set; }

        public bool AlowEdit { get; set; }

        public bool AlowExit { get; set; }

        public string UserId { get; set; }

        public UserModel OwnerUser { get; set; }

        public IList<UserModel> HiredPilots { get; set; } = new List<UserModel>();

        public long DebtValue { get; set; }

        public DateTime DebtMaturityDate { get; set; }

        public IList<AirlineFboDbModel> HiredFBOs { get; set; } = new List<AirlineFboDbModel>();
    }
}
