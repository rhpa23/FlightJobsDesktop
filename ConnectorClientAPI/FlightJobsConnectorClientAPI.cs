using ConnectorClientAPI.Exceptions;
using FlightJobs.Model.Models;
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
        public static string SITE_URL = "http://localhost:5646/";
        //public static string SITE_URL = "https://flightjobs.bsite.net/";
        //public static string SITE_URL = "https://flightjobs.somee.com/";
        static HttpClient client;

        public FlightJobsConnectorClientAPI()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            
            client = new HttpClient(handler);
            client.BaseAddress = new Uri(SITE_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<LoginResponseModel> Login(string email, string password)
        {
            try
            {
                var url = $"{SITE_URL}api/AuthenticationApi/Login";
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

        public async Task ConfirmJob(ConfirmJobModel confirmJobModel)
        {
            var url = $"{SITE_URL}api/SearchApi/ConfirmJobs";
            var body = JsonConvert.SerializeObject(confirmJobModel);
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task RemoveJob(string userId, long jobId)
        {
            var url = $"{SITE_URL}api/JobApi/RemoveJob?jobId={jobId}";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task<IList<JobListItemModel>> GenerateConfirmJobs(GenerateJobModel generateJobData)
        {
            var url = $"{SITE_URL}api/SearchApi/GenerateConfirmJobs";
            var body = JsonConvert.SerializeObject(generateJobData);
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IList<JobListItemModel>>(json.Replace("\\", ""));
            }
            else
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task RemovePlaneCapacity(CustomPlaneCapacityModel capacityModel)
        {
            var url = $"{SITE_URL}api/SearchApi/RemoveCapacity";
            var body = JsonConvert.SerializeObject(capacityModel);
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task SavePlaneCapacity(CustomPlaneCapacityModel capacityModel)
        {
            var url = $"{SITE_URL}api/SearchApi/SaveCapacity";
            var body = JsonConvert.SerializeObject(capacityModel);
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task<StartJobResponseModel> StartJob(DataModel data)
        {
            var url = $"{SITE_URL}api/JobApi/StartJobMSFS";
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
            var url = $"{SITE_URL}api/JobApi/FinishJobMSFS";
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
            var url = $"{SITE_URL}api/JobApi/GetUserJobs";
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("UserId", userId);

            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var jobs = JsonConvert.DeserializeObject<IList<JobModel>>(json);

            return jobs;
        }

        public async Task<JobModel> GetLastUserJob(string userId)
        {
            var url = $"{SITE_URL}api/JobApi/GetLastUserJob";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"{", "{").Replace("}\"", "}").Replace("\\", "");
            return JsonConvert.DeserializeObject<JobModel>(json);
        }

        public async Task<bool> ActivateUserJob(string userId, long jobId)
        {
            var url = $"{SITE_URL}api/JobApi/ActivateUserJob";
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("UserId", userId);
            client.DefaultRequestHeaders.Add("JobId", jobId.ToString());

            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            return response.IsSuccessStatusCode;
        }

        public async Task<UserStatisticsModel> GetUserStatistics(string userId)
        {
            var url = $"{SITE_URL}api/UserApi/GetUserStatistics";

            var body = JsonConvert.SerializeObject(new { id = userId });

            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            string json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<UserStatisticsModel>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
        }

        public async Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings)
        {
            var url = $"{SITE_URL}api/UserApi/UpdateUserSettings";

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

        public async Task<IList<SearchJobTipsModel>> GetArrivalTips(string departure, string userId)
        {
            var url = $"{SITE_URL}api/SearchApi/GetArrivalTips?departure={departure}";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IList<SearchJobTipsModel>>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
            }
            else
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task<IList<SearchJobTipsModel>> GetAlternativeTips(string arrival, int range)
        {
            var url = $"{SITE_URL}api/SearchApi/GetAlternativeTips?arrival={arrival}&range={range}";
            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IList<SearchJobTipsModel>>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
            }
            else
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task<bool> CloneJob(long jobId, string userId)
        {
            var url = $"{SITE_URL}api/SearchApi/CloneJob?jobId={jobId}";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
            return true;
        }

        public async Task<IList<CustomPlaneCapacityModel>> GetPlaneCapacities(string userId)
        {
            var url = $"{SITE_URL}api/JobApi/GetUserPlaneCapacities";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IList<CustomPlaneCapacityModel>>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
            }
            else
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
