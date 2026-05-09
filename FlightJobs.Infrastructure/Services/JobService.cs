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

        public async Task<IList<JobModel>> GetAllUserJobs()
        {
            //var pendingJobs = await _flightJobsConnectorClientAPI.GetPendingUserJobs();
            var activeJob = await _flightJobsConnectorClientAPI.GetActiveUserJob();            
            AppProperties.UserJobs.Clear();
            //if (pendingJobs != null)
            //{
            //    ((List<JobModel>)AppProperties.UserJobs).AddRange(pendingJobs);
            //}
            if (activeJob != null)
            {
                ((List<JobModel>)AppProperties.UserJobs).Add(activeJob);
            }
            return AppProperties.UserJobs;
        }

        public async Task<JobModel> GetActiveUserJob()
        {
            var activeJob = await _flightJobsConnectorClientAPI.GetActiveUserJob();
            return activeJob;
        }

        public async Task<JobModel> GetLastUserJob()
        {
            return await _flightJobsConnectorClientAPI.GetLastUserJob();
        }

        public async Task ActivateJob(long jobId)
        {
            await _flightJobsConnectorClientAPI.ActivateUserJob(jobId);
        }
    }
}
