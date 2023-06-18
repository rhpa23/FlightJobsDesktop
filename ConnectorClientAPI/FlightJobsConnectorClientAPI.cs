using FlightJobs.Model;
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
        //public static string SITE_URL = "http://localhost:5646/";
        public static string SITE_URL = "https://flightjobs.bsite.net/";
        //public static string SITE_URL = "https://flightjobs.somee.com/";
        static HttpClient _client;

        public FlightJobsConnectorClientAPI()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            
            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri(SITE_URL);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<LoginResponseModel> Login(string email, string password)
        {
            try
            {
                var url = $"{SITE_URL}api/AuthenticationApi/Login";
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("Email", email);
                _client.DefaultRequestHeaders.Add("Password", password);
                HttpResponseMessage response = await _client.GetAsync(new Uri(url));

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

        public async Task UserRegister(UserRegisterModel userModel)
        {
            var url = $"{SITE_URL}api/AuthenticationApi/UserRegister";

            var body = JsonConvert.SerializeObject(userModel);

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task ConfirmJob(ConfirmJobModel confirmJobModel)
        {
            var url = $"{SITE_URL}api/SearchApi/ConfirmJobs";
            var body = JsonConvert.SerializeObject(confirmJobModel);
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task RemoveJob(string userId, long jobId)
        {
            var url = $"{SITE_URL}api/JobApi/RemoveJob?jobId={jobId}";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task<IList<JobListItemModel>> GenerateConfirmJobs(GenerateJobModel generateJobData)
        {
            var url = $"{SITE_URL}api/SearchApi/GenerateConfirmJobs";
            var body = JsonConvert.SerializeObject(generateJobData);
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
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
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task SavePlaneCapacity(CustomPlaneCapacityModel capacityModel)
        {
            var url = $"{SITE_URL}api/SearchApi/SaveCapacity";
            var body = JsonConvert.SerializeObject(capacityModel);
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.Content.ReadAsStringAsync().Result);
            }
        }

        public async Task<StartJobResponseModel> StartJob(DataModel data)
        {
            var url = $"{SITE_URL}api/JobApi/StartJobMSFS";
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Add("UserId", data.UserId);
            _client.DefaultRequestHeaders.Add("PlaneDescription", data.Title);
            _client.DefaultRequestHeaders.Add("Latitude", data.Latitude.ToString());
            _client.DefaultRequestHeaders.Add("Longitude", data.Longitude.ToString());
            _client.DefaultRequestHeaders.Add("Payload", data.PayloadKilograms.ToString());
            _client.DefaultRequestHeaders.Add("FuelWeight", data.FuelWeightKilograms.ToString());

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
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

        public async Task<FinishJobResponseModel> FinishJob(DataModel data)
        {
            var url = $"{SITE_URL}api/JobApi/FinishJobMsfsPost";
            var body = JsonConvert.SerializeObject(data);

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            string json = response.Content.ReadAsStringAsync().Result.Replace("\"{", "{").Replace("}\"", "}").Replace("\\", "");
            var finishedJob = JsonConvert.DeserializeObject<FinishJobResponseModel>(json);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(finishedJob.ResultMessage, new Exception($"Error status code: {response.StatusCode}"));
            }

            return finishedJob;
        }

        public async Task<IList<JobModel>> GetUserJobs(string userId)
        {
            var url = $"{SITE_URL}api/JobApi/GetUserJobs";
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Add("UserId", userId);

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var jobs = JsonConvert.DeserializeObject<IList<JobModel>>(json);

            return jobs;
        }

        public async Task<PaginatedJobsModel> GetLogbookUserJobs(string sortOrder, string currentSort, int pageNumber, FilterJobsModel filterModel)
        {
            var url = $"{SITE_URL}api/JobApi/GetUserJobsPaged?sortOrder={sortOrder}&currentSort={currentSort}&pageNumber={pageNumber}";
            var body = JsonConvert.SerializeObject(filterModel);

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var jobs = JsonConvert.DeserializeObject<PaginatedJobsModel>(json);

            return jobs;
        }

        public async Task<JobModel> GetLastUserJob(string userId)
        {
            var url = $"{SITE_URL}api/JobApi/GetLastUserJob";
            var body = JsonConvert.SerializeObject(new { id = userId });
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"{", "{").Replace("}\"", "}").Replace("\\", "");
            return JsonConvert.DeserializeObject<JobModel>(json);
        }

        public async Task<bool> ActivateUserJob(string userId, long jobId)
        {
            var url = $"{SITE_URL}api/JobApi/ActivateUserJob";
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Add("UserId", userId);
            _client.DefaultRequestHeaders.Add("JobId", jobId.ToString());

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            return response.IsSuccessStatusCode;
        }

        public async Task<UserStatisticsModel> GetUserStatistics(string userId)
        {
            var url = $"{SITE_URL}api/UserApi/GetUserStatistics";

            var body = JsonConvert.SerializeObject(new { id = userId });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            string json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<UserStatisticsModel>(json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", ""));
        }

        public async Task<UserStatisticsModel> UpdateUserSettings(UserSettingsModel userSettings)
        {
            var url = $"{SITE_URL}api/UserApi/UpdateUserSettings";

            var body = JsonConvert.SerializeObject(userSettings);

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
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
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
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
            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
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
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
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
            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
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

        public async Task<PaginatedAirlinersModel> GetAirliners(string sortOrder, string currentSort, int pageNumber, PaginatedAirlinersFilterModel filterModel)
        {
            var url = $"{SITE_URL}api/AirlineApi/GetAirliners?sortOrder={sortOrder}&currentSort={currentSort}&pageNumber={pageNumber}";
            var body = JsonConvert.SerializeObject(filterModel);

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var airlines = JsonConvert.DeserializeObject<PaginatedAirlinersModel>(json);

            return airlines;
        }

        public async Task<AirlineModel> CreateAirline(AirlineModel airline, string userId)
        {
            var url = $"{SITE_URL}api/AirlineApi/CreateAirline";
            var body = JsonConvert.SerializeObject(new { userId = userId, airline = airline });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var airlineResult = JsonConvert.DeserializeObject<AirlineModel>(json);

            return airlineResult;
        }

        public async Task<bool> UpdateAirline(AirlineModel airline, string userId)
        {
            var url = $"{SITE_URL}api/AirlineApi/UpdateAirline";
            var body = JsonConvert.SerializeObject(new { userId = userId, airline = airline });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            return true;
        }

        public async Task<IList<UserModel>> GetAirlinePilotsHired(int airlineId)
        {
            var url = $"{SITE_URL}api/AirlineApi/GetPilotsHired?id={airlineId}";

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var userModels = JsonConvert.DeserializeObject<IList<UserModel>>(json);

            return userModels;
        }

        public async Task<IList<AirlineFboDbModel>> GetAirlineFBOs(int airlineId)
        {
            var url = $"{SITE_URL}api/AirlineApi/GetAirlineFBOs?id={airlineId}";

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var fbosModels = JsonConvert.DeserializeObject<IList<AirlineFboDbModel>>(json);

            return fbosModels;
        }

        public async Task<bool> PayAirlineDebts(AirlineModel airline, string userId)
        {
            var url = $"{SITE_URL}api/AirlineApi/PayAirlineDebts";
            var body = JsonConvert.SerializeObject(new { userId = userId, airline });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            return true;
        }

        public async Task<PaginatedAirlineJobLedgerModel> GetAirlineLedger(int airlineId, int pageNumber, FilterJobsModel filterJob)
        {
            var url = $"{SITE_URL}api/AirlineApi/GetAirlineLedger?airlineId={airlineId}&pageNumber={pageNumber}";

            var body = JsonConvert.SerializeObject(filterJob);

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var airlineJobsLedger = JsonConvert.DeserializeObject<PaginatedAirlineJobLedgerModel>(json);

            return airlineJobsLedger;
        }

        public async Task<List<AirlineFboDbModel>> GetFbos(string icao, int airlineId)
        {
            var url = $"{SITE_URL}api/AirlineApi/GetFOBs?icao={icao}&airlineId={airlineId}";

            HttpResponseMessage response = await _client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var airlineFBOs = JsonConvert.DeserializeObject<List<AirlineFboDbModel>>(json);

            return airlineFBOs;
        }

        public async Task<IList<AirlineFboDbModel>> HireFbo(string icao, string userId)
        {
            var url = $"{SITE_URL}api/AirlineApi/HireAirlineFbo";

            var body = JsonConvert.SerializeObject(new { icao = icao, userId = userId });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new ApiException(response.Content.ReadAsStringAsync().Result);
                }

                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            string json = response.Content.ReadAsStringAsync().Result.Replace("\"[", "[").Replace("]\"", "]").Replace("\\", "");
            var airlineFbosHired = JsonConvert.DeserializeObject<IList<AirlineFboDbModel>>(json);

            return airlineFbosHired;
        }

        public async Task<bool> JoinAirline(AirlineModel airline, string userId)
        {
            var url = $"{SITE_URL}api/AirlineApi/JoinAirline";

            var body = JsonConvert.SerializeObject(new { userId = userId, airline });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ApiException(response.Content.ReadAsStringAsync().Result.Replace("\"", ""));
                }

                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            return true;
        }

        public async Task<bool> ExitAirline(AirlineModel airline, string userId)
        {
            var url = $"{SITE_URL}api/AirlineApi/ExitAirline";

            var body = JsonConvert.SerializeObject(new { userId = userId, airline });

            HttpResponseMessage response = await _client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result, new Exception($"Error status code: {response.StatusCode}"));
            }

            return true;
        }
    }
}
