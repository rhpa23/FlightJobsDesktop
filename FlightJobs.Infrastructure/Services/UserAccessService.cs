using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System.Threading.Tasks;
using System.Linq;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService : ServiceBase, IUserAccessService
    {
        public async Task LoadUserStatisticsProperties()
        {
            var userStatisticsData = await _flightJobsConnectorClientAPI.GetUserStatistics();

            AppProperties.UserStatistics = userStatisticsData;
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
    }
}
