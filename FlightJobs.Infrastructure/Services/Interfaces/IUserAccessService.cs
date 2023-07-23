using ConnectorClientAPI;
using FlightJobs.Model.Models;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IUserAccessService
    {
        Task<LoginResponseModel> Login(string email, string password);

        Task LoadUserStatisticsProperties(string userId);
        Task LoadUserAirlineProperties();

        Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings);
        Task UserRegister(UserRegisterModel userModel);

        Task<SimBriefModel> GetSimBriefData(string simbriefUserName);
        Task<RandomFlightModel> GetRandomFlight(string departure, string destination);
    }
}
