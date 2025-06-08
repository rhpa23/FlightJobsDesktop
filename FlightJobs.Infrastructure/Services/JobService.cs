using FlightJobs.Infrastructure.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FlightJobs.Model.Models;
using System;

namespace FlightJobs.Infrastructure.Services
{
    public class JobService : ServiceBase, IJobService
    {
        public async Task<StartJobResponseModel> StartJob(DataModel JobSimData)
        {
            return await _flightJobsConnectorClientAPI.StartJob(JobSimData);
        }

        public async Task<FinishJobResponseModel> FinishJob(DataModel JobSimData)
        {
            return await _flightJobsConnectorClientAPI.FinishJob(JobSimData);
        }

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

        public async Task<IList<CustomPlaneCapacityModel>> GetPlaneCapacities(string userId)
        {
            return await _flightJobsConnectorClientAPI.GetPlaneCapacities(userId);
        }

        public async Task SavePlaneCapacity(CustomPlaneCapacityModel capacityModel)
        {
            await _flightJobsConnectorClientAPI.SavePlaneCapacity(capacityModel);
        }

        public async Task UpdatePlaneCapacity(CustomPlaneCapacityModel capacityModel)
        {
            await _flightJobsConnectorClientAPI.UpdatePlaneCapacity(capacityModel);
        }

        public async Task<IList<JobListItemModel>> GenerateConfirmJobs(GenerateJobModel generateJobData)
        {
            return await _flightJobsConnectorClientAPI.GenerateConfirmJobs(generateJobData);
        }

        public async Task RemovePlaneCapacity(CustomPlaneCapacityModel capacityModel)
        {
            await _flightJobsConnectorClientAPI.RemovePlaneCapacity(capacityModel);
        }

        public async Task ConfirmJob(ConfirmJobModel confirmJobModel)
        {
            await _flightJobsConnectorClientAPI.ConfirmJob(confirmJobModel);
        }

        public async Task RemoveJob(string userId, long jobId)
        {
            await _flightJobsConnectorClientAPI.RemoveJob(userId, jobId);
        }

        public async Task<PaginatedJobsModel> GetLogbookUserJobs(string sortOrder, string currentSort, int pageNumber, FilterJobsModel filterModel)
        {
            return await _flightJobsConnectorClientAPI.GetLogbookUserJobs(sortOrder, currentSort, pageNumber, filterModel);
        }
    }
}
