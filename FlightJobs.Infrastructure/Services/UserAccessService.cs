using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService : ServiceBase, IUserAccessService
    {
        public async Task<bool> LoadUserStatisticsProperties()
        {
            try
            {
                var userStatisticsData = await _flightJobsConnectorClientAPI.GetUserStatistics();

                AppProperties.UserStatistics = userStatisticsData;
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
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

        public void SetApiTokens(string accessToken, string refreshToken)
        {
            _flightJobsConnectorClientAPI.RestoreSavedTokens(
                    accessToken,
                    refreshToken
                );
        }

        /// <summary>
        /// Tenta fazer auto-login usando tokens salvos anteriormente
        /// Realiza até 3 tentativas, pois o interceptor já deve ter feito o refresh do token
        /// </summary>
        public async Task<bool> TryAutoLoginWithSavedTokens()
        {
            if (AppProperties.UserLogin != null)
            {
                int maxAttempts = 3;
                for ( int i = 0; i < maxAttempts; i++)
                {
                    var loaded = await LoadUserStatisticsProperties();
                    if (loaded)
                        return true;
                }
            }
            return false;
        }

        public bool LoadLoginData()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var filePath = Path.Combine(path, "FlightJobsDesktop\\ResourceData\\LoginSavedData.ini");
            if (!File.Exists(filePath))
                return false;

            var lines = File.ReadLines(filePath);
            var line = lines?.FirstOrDefault();
            var info = line?.Split('|');
            if (info?.Length >= 5)
            {
                AppProperties.UserLogin = new LoginResponseModel()
                {
                    Email = info[0],
                    AccessToken = info[1],
                    RefreshToken = info[2],
                    UserName = info[3],
                    UserId = info[4],
                };
                SetApiTokens(AppProperties.UserLogin.AccessToken, AppProperties.UserLogin.RefreshToken);
                return true;
            }
            return false;
        }

        public void SaveLoginData(LoginResponseModel login)
        {
            _flightJobsConnectorClientAPI.SaveLoginData(login);
        }
    }
}
