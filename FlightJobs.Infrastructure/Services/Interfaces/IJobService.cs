using FlightJobs.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IJobService
    {
        Task<StartJobResponseModel> StartJob(DataModel JobSimData);
        Task<FinishJobResponseModel> FinishJob(DataModel JobSimData);
        Task<IList<JobModel>> GetAllUserJobs();
        Task<JobModel> GetLastUserJob();
        Task<JobModel> GetActiveUserJob();
        Task ActivateJob(long jobId);
    }
}
