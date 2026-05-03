using ConnectorClientAPI;
using FlightJobs.Model.Models;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IUserAccessService
    {
        Task<LoginResponseModel> Login(string email, string password);

        Task<bool> LoadUserStatisticsProperties();

        void SetApiTokens(string accessToken, string refreshToken);

        Task<bool> TryAutoLoginWithSavedTokens();
        void SaveLoginData(LoginResponseModel login);
        bool LoadLoginData();
    }
}
