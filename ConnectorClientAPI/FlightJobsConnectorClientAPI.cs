using FlightJobs.Model;
using FlightJobs.Model.DTOs;
using FlightJobs.Model.Enum;
using FlightJobs.Model.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ConnectorClientAPI
{
    public class LoginApiResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public UserApiResponse user { get; set; }
    }

    public class UserApiResponse
    {
        public string id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public object activeJob { get; set; }
    }

    public class FlightJobsConnectorClientAPI
    {
        public static string SiteUrl { get; set; } = "https://flightjobs.bsite.net/";
        public static string ApiBaseUrl { get; set; } = "http://localhost:3001/api/"; // TODO: Definir URL da nova API
        //public static string ApiBaseUrl { get; set; } = "https://flightjobs-api.vercel.app/api/";


        static HttpClient _client;
        private static string _accessToken;
        private static string _refreshToken;

        public FlightJobsConnectorClientAPI()
        {
            HttpClientHandler innerHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };

            // Cria o TokenRefreshHandler com o innerHandler
            TokenRefreshHandler tokenRefreshHandler = new TokenRefreshHandler(
                RefreshAccessTokenAsync
            );
            tokenRefreshHandler.InnerHandler = innerHandler;

            _client = new HttpClient(tokenRefreshHandler);
            _client.BaseAddress = new Uri(ApiBaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void SetAuthorizationHeader()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Renova o token de acesso usando o refresh token
        /// </summary>
        private async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_refreshToken))
                {
                    return false;
                }

                var url = $"{ApiBaseUrl}auth/refresh";
                var body = JsonConvert.SerializeObject(new { refreshToken = _refreshToken });

                // Cria uma requisição sem o handler de refresh para evitar recursão infinita
                using (var handler = new HttpClientHandler() { AllowAutoRedirect = false })
                using (var tempClient = new HttpClient(handler))
                {
                    tempClient.BaseAddress = new Uri(ApiBaseUrl);
                    tempClient.DefaultRequestHeaders.Accept.Clear();
                    tempClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    tempClient.DefaultRequestHeaders.Add("User-Agent", "FlightJobs Desktop");

                    HttpResponseMessage response = await tempClient.PostAsync(
                        new Uri(url),
                        new StringContent(body, Encoding.UTF8, "application/json")
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<RefreshTokenResponse>(json);

                        _accessToken = result.access_token;
                        _refreshToken = result.refresh_token;
                        var loginData = new LoginResponseModel()
                        {
                            AccessToken = result.access_token,
                            RefreshToken = result.refresh_token,
                            Email = result.User?.Email,
                            UserId = result.User?.Id,
                            UserName = result.User?.UserName,
                        };
                        SaveLoginData(loginData);
                        SetAuthorizationHeader();

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> PingUrl(string url)
        {
            try
            {
                var response = await _client.GetAsync($"{url}");
                return response != null && response.StatusCode == HttpStatusCode.Found;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<LoginResponseModel> Login(string email, string password)
        {
            try
            {
                var url = $"{ApiBaseUrl}auth/login";

                var body = JsonConvert.SerializeObject(new { email, password });

                HttpResponseMessage response = await _client.PostAsync(
                    new Uri(url),
                    new StringContent(body, Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginApiResponse>(json);

                    // Armazena o token e refresh token para requisições futuras
                    _accessToken = result.access_token;
                    _refreshToken = result.refresh_token;
                    SetAuthorizationHeader();

                    return new LoginResponseModel()
                    {
                        ActiveJobInfo = result.user?.activeJob != null ? result.user.activeJob.ToString() : "",
                        UserId = result.user?.id,
                        Email = result.user?.email,
                        AccessToken = result.access_token,
                        RefreshToken = result.refresh_token,
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Fail to connect FlightJobs API. Please try again and if the error persists contact the site administrator.", ex);
            }
        }

        public async Task<StartJobResponseModel> StartJob(DataModel data)
        {
            var url = $"{ApiBaseUrl}jobs/start";
            SetAuthorizationHeader();

            var startDto = new
            {
                latitude = data.Latitude,
                longitude = data.Longitude,
                payloadKilograms = data.PayloadKilograms,
                fuelWeightKilograms = data.FuelWeightKilograms
            };

            var body = JsonConvert.SerializeObject(startDto);
            HttpResponseMessage response = await _client.PostAsync(
                new Uri(url),
                new StringContent(body, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent, new Exception($"Error status code: {response.StatusCode}"));
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<StartJobResponseDto>(json);

            return new StartJobResponseModel()
            {
                ArrivalICAO = result.arrivalICAO,
                ResultMessage = result.message
            };
        }

        public async Task<FinishJobResponseModel> FinishJob(DataModel data)
        {
            var url = $"{ApiBaseUrl}jobs/finish";
            SetAuthorizationHeader();

            var finishDto = new
            {
                latitude = data.Latitude,
                longitude = data.Longitude,
                payloadKilograms = data.PayloadKilograms,
                fuelWeightKilograms = data.FuelWeightKilograms,
                modelName = data.Title,
                modelDescription = data.Title,
                resultMessages = data.ResultMessages,
                resultScore = data.ResultScore
            };

            var body = JsonConvert.SerializeObject(finishDto);
            HttpResponseMessage response = await _client.PostAsync(
                new Uri(url),
                new StringContent(body, Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();
            var finishedJob = JsonConvert.DeserializeObject<FinishJobResponseDto>(json);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(finishedJob?.message ?? "Error finishing job", new Exception($"Error status code: {response.StatusCode}"));
            }

            return new FinishJobResponseModel
            {
                // Mapear propriedades conforme necessário
                ResultMessage = finishedJob.message
            };
        }

        public async Task<IList<JobModel>> GetPendingUserJobs()
        {
            var url = $"{ApiBaseUrl}jobs/pending";
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent, new Exception($"Error status code: {response.StatusCode}"));
            }

            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonConvert.DeserializeObject<IList<JobModel>>(json);

            return jobs;
        }

        public async Task<JobModel> GetActiveUserJob()
        {
            var url = $"{ApiBaseUrl}jobs/active";
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent, new Exception($"Error status code: {response.StatusCode}"));
            }

            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.DeserializeObject<JobModel>(json);// startFuelWeight

            return job;
        }

        public async Task<JobModel> GetLastUserJob()
        {
            var url = $"{ApiBaseUrl}jobs/last-completed";
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.DeserializeObject<JobModel>(json);
            return job;
        }

        public async Task<bool> ActivateUserJob(long jobId)
        {
            var url = $"{ApiBaseUrl}jobs/{jobId}/activate";
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.PostAsync(
                new Uri(url),
                new StringContent("", Encoding.UTF8, "application/json")
            );
            return response.IsSuccessStatusCode;
        }

        public async Task<UserStatisticsModel> GetUserStatistics()
        {
            // Nova API: GET statistics/my-stats
            var url = $"{ApiBaseUrl}statistics/my-stats";
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent);
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserStatisticsModel>(json);
        }

        public async Task<UserStatisticsModel> GetUserStatisticsFlightsInfo()
        {
            // Nova API: GET statistics/my-stats (mesmo endpoint, dados consolidados)
            var url = $"{ApiBaseUrl}statistics/my-stats";
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent);
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserStatisticsModel>(json);
        }

        /// <summary>
        /// Restaura tokens previamente salvos para fazer auto-login
        /// </summary>
        public void RestoreSavedTokens(string accessToken, string refreshToken)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            SetAuthorizationHeader();
        }

        /// <summary>
        /// Retorna o access token atual
        /// </summary>
        public string GetAccessToken()
        {
            return _accessToken;
        }

        /// <summary>
        /// Retorna o refresh token atual
        /// </summary>
        public string GetRefreshToken()
        {
            return _refreshToken;
        }

        /// <summary>
        /// Limpa os tokens de acesso e refresh token
        /// </summary>
        public void Logout()
        {
            _accessToken = null;
            _refreshToken = null;
            _client.DefaultRequestHeaders.Authorization = null;
        }

        public void SaveLoginData(LoginResponseModel login)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlightJobsDesktop\\ResourceData");
            if (!Directory.Exists(path))
            {
                var dirInfo = Directory.CreateDirectory(path);
                path = Path.Combine(dirInfo.FullName, "LoginSavedData.ini");
            }
            else
            {
                path = Path.Combine(path, "LoginSavedData.ini");
            }

            string createText = $"{login.Email}|{login.AccessToken}|{login.RefreshToken}|{login.UserName}|{login.UserId}";
            File.WriteAllText(path, createText);
        }
    }
}
