﻿using FlightJobs.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IJobService
    {
        Task<IList<JobModel>> GetAllUserJobs(string userId);
        Task<JobModel> GetLastUserJob(string userId);
        Task<IList<SearchJobTipsModel>> GetAlternativeTips(string arrival, int range);
        Task<IList<SearchJobTipsModel>> GetArrivalTips(string departure, string userId);
        Task<bool> CloneJob(long jobId, string userId);
        Task ActivateJob(string userId, long jobId);
    }
}