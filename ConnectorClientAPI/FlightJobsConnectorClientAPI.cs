using ConnectorClientAPI.Exceptions;
using ConnectorClientAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ConnectorClientAPI
{
    public class FlightJobsConnectorClientAPI
    {
        string _siteUrl = "http://localhost:5646/";
        //string _siteUrl = "https://flightjobs.bsite.net/";
        //string _siteUrl = "https://flightjobs.somee.com/";
        static HttpClient client;

        public FlightJobsConnectorClientAPI()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            
            client = new HttpClient(handler);
            client.BaseAddress = new Uri(_siteUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<LoginResponseModel> Login(string email, string password)
        {
            try
            {
                var url = $"{_siteUrl}api/AuthenticationApi/Login";
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Email", email);
                client.DefaultRequestHeaders.Add("Password", password);
                HttpResponseMessage response = await client.GetAsync(new Uri(url));

                if (response.IsSuccessStatusCode)
                {
                    var activeJobInfo = response.Headers.GetValues("active_job_info");
                    var username = response.Headers.FirstOrDefault(x => x.Key == "username").Value;
                    return new LoginResponseModel()
                    {
                        ActiveJobInfo = activeJobInfo != null ? activeJobInfo.First() : "",
                        UserId = response.Content.ReadAsStringAsync().Result,
                        UserName = username != null ? username.First() : "<no user name>"
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
            var url = $"{_siteUrl}api/JobApi/StartJobMSFS";
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("UserId", data.UserId);
            client.DefaultRequestHeaders.Add("PlaneDescription", data.Title);
            client.DefaultRequestHeaders.Add("Latitude", data.Latitude.ToString());
            client.DefaultRequestHeaders.Add("Longitude", data.Longitude.ToString());
            client.DefaultRequestHeaders.Add("Payload", data.PayloadKilograms.ToString());
            client.DefaultRequestHeaders.Add("FuelWeight", data.FuelWeightKilograms.ToString());

            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }
            
            return new StartJobResponseModel()
            {
                ArrivalICAO = response.Headers.GetValues("arrival-icao").First(),
                ResultMessage = response.Content.ReadAsStringAsync().Result
            };
        }

        public async Task<StartJobResponseModel> FinishJob(DataModel data)
        {
            var url = $"{_siteUrl}api/JobApi/FinishJobMSFS";
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("UserId", data.UserId);
            client.DefaultRequestHeaders.Add("PlaneDescription", data.Title);
            client.DefaultRequestHeaders.Add("Latitude", data.Latitude.ToString());
            client.DefaultRequestHeaders.Add("Longitude", data.Longitude.ToString());
            client.DefaultRequestHeaders.Add("Payload", data.PayloadKilograms.ToString());
            client.DefaultRequestHeaders.Add("FuelWeight", data.FuelWeightKilograms.ToString());

            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            return new StartJobResponseModel()
            {
                ResultMessage = response.Content.ReadAsStringAsync().Result
            };
        }

        public async Task<IList<JobModel>> GetUserJobs(string userId)
        {
            var url = $"{_siteUrl}api/JobApi/GetUserJobs";
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("UserId", userId);

            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result;
            var jobs = JsonConvert.DeserializeObject<IList<JobModel>>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));

            return jobs;
        }

        public async Task<bool> ActivateUserJob(string userId, int jobId)
        {
            var url = $"{_siteUrl}api/JobApi/ActivateUserJob";
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("UserId", userId);
            client.DefaultRequestHeaders.Add("JobId", jobId.ToString());

            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            return response.IsSuccessStatusCode;
        }

        public async Task<UserStatisticsModel> GetUserStatistics(string userId)
        {
            var url = $"{_siteUrl}api/UserApi/GetUserStatistics";

            var body = JsonConvert.SerializeObject(new { id = userId });

            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            string json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<UserStatisticsModel>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
        }

        public async Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings)
        {
            var url = $"{_siteUrl}api/UserApi/UpdateUserSettings";

            var body = JsonConvert.SerializeObject(userSettings);

            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<UserStatisticsModel>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
            }
            else
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
