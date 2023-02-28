using ConnectorClientAPI;
using FlightJobs.Model.Models;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IUserAccessService
    {
        Task<LoginResponseModel> Login(string email, string password);

        Task<UserStatisticsModel> GetUserStatistics(string userId);

        Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings);
    }
}
