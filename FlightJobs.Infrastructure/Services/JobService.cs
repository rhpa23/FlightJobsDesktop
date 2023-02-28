using FlightJobs.Infrastructure.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FlightJobs.Model.Models;

namespace FlightJobs.Infrastructure.Services
{
    public class JobService : ServiceBase, IJobService
    {
        public async Task<bool> CloneJob(long jobId, string userId)
        {
            return await _flightJobsConnectorClientAPI.CloneJob(jobId, userId);
        }

        public async Task<IList<JobModel>> GetAllUserJobs(string userId)
        {
            var jobs = await _flightJobsConnectorClientAPI.GetUserJobs(userId);
            AppProperties.UserJobs.Clear();
            ((List<JobModel>)AppProperties.UserJobs).AddRange(jobs);
            return jobs;
        }

        public async Task<JobModel> GetLastUserJob(string userId)
        {
            return await _flightJobsConnectorClientAPI.GetLastUserJob(userId);
        }

        public async Task<IList<SearchJobTipsModel>> GetAlternativeTips(string arrival, int range)
        {
            return await _flightJobsConnectorClientAPI.GetAlternativeTips(arrival, range);
        }

        public async Task<IList<SearchJobTipsModel>> GetArrivalTips(string departure, string userId)
        {
            return await _flightJobsConnectorClientAPI.GetArrivalTips(departure, userId);
        }

        public async Task ActivateJob(string userId, long jobId)
        {
            await _flightJobsConnectorClientAPI.ActivateUserJob(userId, jobId);
        }
    }
}
