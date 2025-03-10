﻿using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System.Threading.Tasks;
using System.Linq;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService : ServiceBase, IUserAccessService
    {
        public async Task LoadUserStatisticsProperties(string userId)
        {
            var userStatisticsData = await _flightJobsConnectorClientAPI.GetUserStatistics(userId);
            userStatisticsData.LicensesOverdue = await new PilotService().GetUserLicensesOverdue(userId);

            AppProperties.UserStatistics = userStatisticsData;
        }

        public async Task LoadUserAirlineProperties()
        {
            var statistics = AppProperties.UserStatistics;
            if (statistics?.Airline != null)
            {
                statistics.Airline.HiredPilots = await new AirlineService().GetAirlinePilotsHired(statistics.Airline.Id);
                statistics.Airline.HiredFBOs = await new AirlineService().GetAirlineFBOs(statistics.Airline.Id);
                statistics.Airline.OwnerUser = statistics.Airline.HiredPilots.FirstOrDefault(x => x.Id == statistics.Airline.UserId);
            }
        }

        public async Task<LoginResponseModel> Login(string email, string password)
        {
            var loginData = await _flightJobsConnectorClientAPI.Login(email, password);
            if (loginData != null)
            {
                loginData.UserId = loginData.UserId.Replace("\"", "");
                AppProperties.UserLogin = loginData;
            }
            return loginData;
        }

        public async Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings)
        {
            return await _flightJobsConnectorClientAPI.UpdateUserSettings(userSettings);
        }

        public async Task UserRegister(UserRegisterModel userModel)
        {
            await _flightJobsConnectorClientAPI.UserRegister(userModel);
        }

        public async Task<SimBriefModel> GetSimBriefData(string simbriefUserName)
        {
            return await _flightJobsConnectorClientAPI.GetSimBriefData(simbriefUserName);
        }

        public async Task<RandomFlightModel> GetRandomFlight(string departure, string destination)
        {
            return await _flightJobsConnectorClientAPI.GetRandomFlight(departure, destination);
        }
    }
}
