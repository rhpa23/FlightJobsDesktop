using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class PilotService : ServiceBase, IPilotService
    {
        public async Task<UserStatisticsModel> BuyLicencePackage(string userId, long licenseExpenseId)
        {
            return await _flightJobsConnectorClientAPI.BuyLicencePackage(userId, licenseExpenseId);
        }

        public async Task<UserStatisticsModel> GetUserStatisticsFlightsInfo(string userId)
        {
            return await _flightJobsConnectorClientAPI.GetUserStatisticsFlightsInfo(userId);
        }
    }
}
