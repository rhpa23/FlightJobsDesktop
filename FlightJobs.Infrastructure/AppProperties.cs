using ConnectorClientAPI;
using FlightJobs.Model.Models;
using System.Collections.Generic;

namespace FlightJobs.Infrastructure
{
    public class AppProperties
    {
        public static IList<JobModel> UserJobs { get; set; } = new List<JobModel>();
        public static UserStatisticsModel UserStatistics { get; set; }
        public static LoginResponseModel UserLogin { get; set; }
        public static UserSettingsModel UserSettings { get; set; }
    }
}
