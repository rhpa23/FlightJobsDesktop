using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService : ServiceBase, IUserAccessService
    {
        public async Task<UserStatisticsModel> GetUserStatistics(string userId)
        {
            var userStatisticsData = await _flightJobsConnectorClientAPI.GetUserStatistics(userId);
            AppProperties.UserStatistics = userStatisticsData;
            return userStatisticsData;
        }

        public async Task<LoginResponseModel> Login(string email, string password)
        {
            var loginData = await _flightJobsConnectorClientAPI.Login(email, password);
            loginData.UserId = loginData.UserId.Replace("\"", "");
            AppProperties.UserLogin = loginData;
            return loginData;
        }

        public async Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings)
        {
            return await _flightJobsConnectorClientAPI.UpdateUserSettings(userSettings);
        }
    }
}
