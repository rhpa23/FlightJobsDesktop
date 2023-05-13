using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System.Threading.Tasks;
using System.Linq;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService : ServiceBase, IUserAccessService
    {
        public async Task<UserStatisticsModel> GetUserStatistics(string userId)
        {
            var userStatisticsData = await _flightJobsConnectorClientAPI.GetUserStatistics(userId);
            if (userStatisticsData.Airline != null)
            {
                userStatisticsData.Airline.HiredPilots = await new AirlineService().GetAirlinePilotsHired(userStatisticsData.Airline.Id);
                userStatisticsData.Airline.HiredFBOs = await new AirlineService().GetAirlineFBOs(userStatisticsData.Airline.Id);
                userStatisticsData.Airline.OwnerUser = userStatisticsData.Airline.HiredPilots.FirstOrDefault(x => x.Id == userStatisticsData.Airline.UserId);
            }
            AppProperties.UserStatistics = userStatisticsData;
            return userStatisticsData;
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
    }
}
