using FlightJobs.Domain.Common;
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
        private const string CredentialsSecretKey = "FlightJobsDesktopCredentialsKey_2026";
        private static readonly string CredentialsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlightJobsDesktop\\ResourceData\\UserCredentials.dat");

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

        public SavedCredentialsModel LoadSavedCredentials()
        {
            try
            {
                if (!File.Exists(CredentialsFilePath))
                {
                    return new SavedCredentialsModel { RememberCredentials = false };
                }

                var content = File.ReadAllText(CredentialsFilePath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new SavedCredentialsModel { RememberCredentials = false };
                }

                var parts = content.Split(new[] { "||" }, StringSplitOptions.None);
                if (parts.Length >= 2)
                {
                    var email = parts[0];
                    var encryptedPassword = parts[1];
                    var decryptedPassword = encryptedPassword.Decrypt(CredentialsSecretKey);

                    return new SavedCredentialsModel
                    {
                        Email = email,
                        Password = decryptedPassword,
                        RememberCredentials = true
                    };
                }
            }
            catch
            {
                // Fallback silencioso em caso de arquivo ilegível/corrompido
            }

            return new SavedCredentialsModel { RememberCredentials = false };
        }

        public void SaveCredentials(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return;

                var directory = Path.GetDirectoryName(CredentialsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var encryptedPassword = password.Encrypt(CredentialsSecretKey);
                var data = $"{email}||{encryptedPassword}";
                File.WriteAllText(CredentialsFilePath, data);
            }
            catch
            {
                // Falha de salvamento tratada
            }
        }

        public void ClearSavedCredentials()
        {
            try
            {
                if (File.Exists(CredentialsFilePath))
                {
                    File.Delete(CredentialsFilePath);
                }
            }
            catch
            {
                // Tratamento silencioso
            }
        }
    }
}
