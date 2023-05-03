using FlightJobs.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IAirlineService
    {
        Task<PaginatedAirlinersModel> GetByFilter(string sortOrder, string currentSort, int pageNumber, PaginatedAirlinersFilterModel filterModel);
        Task<IList<UserModel>> GetAirlinePilotsHired(int airlineId);
    }
}
