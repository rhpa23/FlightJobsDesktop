using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class AirlineService : ServiceBase, IAirlineService
    {
        public async Task<IList<UserModel>> GetAirlinePilotsHired(int airlineId)
        {
            return await _flightJobsConnectorClientAPI.GetAirlinePilotsHired(airlineId);
        }

        public async Task<PaginatedAirlinersModel> GetByFilter(string sortOrder, string currentSort, int pageNumber, PaginatedAirlinersFilterModel filterModel)
        {
            return await _flightJobsConnectorClientAPI.GetAirliners(sortOrder, currentSort, pageNumber, filterModel);
        }
    }
}
