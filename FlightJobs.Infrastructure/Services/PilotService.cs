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

        public async Task<UserStatisticsModel> GetUserStatisticsFlightsInfo()
        {
            return await _flightJobsConnectorClientAPI.GetUserStatisticsFlightsInfo();
        }
    }
}
