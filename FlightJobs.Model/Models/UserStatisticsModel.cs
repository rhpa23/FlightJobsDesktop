using System;
using System.Collections.Generic;

namespace FlightJobs.Model.Models
{
    public class UserStatisticsModel
    {
        public CustomPlaneCapacityModel CustomPlaneCapacity { get; set; }

        public virtual AirlineModel Airline { get; set; }

        public long BankBalance { get; set; }

        public long PilotScore { get; set; }

        public string Logo { get; set; }

        public bool SendLicenseWarning { get; set; }

        public bool SendAirlineBillsWarning { get; set; }

        public bool LicenseWarningSent { get; set; }

        public bool AirlineBillsWarningSent { get; set; }

        //public virtual CustomPlaneCapacityDbModel CustomPlaneCapacity { get; set; }

        public bool UseCustomPlaneCapacity { get; set; }

        public long NumberFlights { get; set; }

        public string FlightTimeTotal { get; set; }

        public string PayloadTotal { get; set; }

        public DateTime LastFlight { get; set; }

        public string LastAircraft { get; set; }

        public string FavoriteAirplane { get; set; }

        //public List<StatisticsDbModel> AirlinePilotsHired { get; set; }

        public string GraduationPath { get; set; }

        public string GraduationDesc { get; set; }

        public ChartUserBankBalanceModel ChartModel { get; set; }

        public Dictionary<string, long> DepartureRanking { get; set; }

        public Dictionary<string, long> DestinationRanking { get; set; }
        public string WeightUnit { get; set; }

        public IList<PilotLicenseExpensesUserModel> LicensesOverdue { get; set; }
    }
}
