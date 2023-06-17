using FlightJobs.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IJobService
    {
        Task<StartJobResponseModel> StartJob(DataModel JobSimData);
        Task<FinishJobResponseModel> FinishJob(DataModel JobSimData);
        Task<IList<JobModel>> GetAllUserJobs(string userId);
        Task<JobModel> GetLastUserJob(string userId);
        Task<IList<SearchJobTipsModel>> GetAlternativeTips(string arrival, int range);
        Task<IList<SearchJobTipsModel>> GetArrivalTips(string departure, string userId);
        Task<bool> CloneJob(long jobId, string userId);
        Task ActivateJob(string userId, long jobId);
        Task<IList<CustomPlaneCapacityModel>> GetPlaneCapacities(string userId);
        Task SavePlaneCapacity(CustomPlaneCapacityModel capacityModel);
        Task RemovePlaneCapacity(CustomPlaneCapacityModel capacityModel);
        Task<IList<JobListItemModel>> GenerateConfirmJobs(GenerateJobModel generateJobData);
        Task ConfirmJob(ConfirmJobModel confirmJobModel);
        Task RemoveJob(string userId, long jobId);
        Task<PaginatedJobsModel> GetLogbookUserJobs(string sortOrder, string currentSort, int pageNumber, FilterJobsModel filterModel);
    }
}
