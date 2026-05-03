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

        /// <summary>
        /// Faz login automático usando tokens salvos, sem necessidade de email/senha
        /// </summary>
        //public async Task<bool> AutoLoginWithSavedTokens(LoginResponseModel savedLoginData)
        //{
        //    try
        //    {
        //        if (savedLoginData == null || 
        //            string.IsNullOrEmpty(savedLoginData.AccessToken) ||
        //            string.IsNullOrEmpty(savedLoginData.RefreshToken))
        //        {
        //            return false;
        //        }

        //        // Restaura os tokens no client API
        //        _flightJobsConnectorClientAPI.RestoreSavedTokens(
        //            savedLoginData.AccessToken,
        //            savedLoginData.RefreshToken
        //        );

        //        // Tenta carregar as estatísticas do usuário para validar o token
        //        await LoadUserStatisticsProperties();

        //        // Se chegou aqui, o token é válido
        //        AppProperties.UserLogin = savedLoginData;
        //        return true;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Token pode ter expirado ou outro erro de requisição
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

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
            try
            {
                if (AppProperties.UserLogin != null)
                {
                    //const int maxRetries = 3;

                    //for (int attempt = 1; attempt <= maxRetries; attempt++)
                    //{
                    // Tenta fazer auto-login com os tokens salvos
                    //bool autoLoginSuccess = await AutoLoginWithSavedTokens(AppProperties.UserLogin);

                    // Tenta carregar as estatísticas do usuário para validar o token
                    await LoadUserStatisticsProperties();
                    return true;

                    //if (autoLoginSuccess)
                    //{
                    //    await LoadUserStatisticsProperties();
                    //    return true;
                    //}

                    //AppProperties.UserLogin.AccessToken = _flightJobsConnectorClientAPI.GetAccessToken();
                    //AppProperties.UserLogin.RefreshToken = _flightJobsConnectorClientAPI.GetRefreshToken();

                    // Se chegou na última tentativa, desiste
                    //if (attempt == maxRetries)
                    //{
                    //    break;
                    //}
                    //}
                }
            }
            catch (HttpRequestException)
            {
                // Token pode ter expirado ou outro erro de requisição
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public bool LoadLoginData()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var lines = File.ReadLines(Path.Combine(path, "FlightJobsDesktop\\ResourceData\\LoginSavedData.ini"));
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
