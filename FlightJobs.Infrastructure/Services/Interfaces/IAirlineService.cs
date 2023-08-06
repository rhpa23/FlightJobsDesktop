using FlightJobs.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IAirlineService
    {
        Task<PaginatedAirlinersModel> GetByFilter(string sortOrder, string currentSort, int pageNumber, PaginatedAirlinersFilterModel filterModel);
        Task<IList<UserModel>> GetAirlinePilotsHired(int airlineId);
        Task<bool> UpdateAirline(AirlineModel airline, string userId);
        Task<AirlineModel> CreateAirline(AirlineModel airline, string userId);
        Task<bool> PayAirlineDebts(int airlineId, string userId);
        Task<PaginatedAirlineJobLedgerModel> GetAirlineLedger(int airlineId, int pageNumber, FilterJobsModel filterJob);
        Task<IList<AirlineFboDbModel>> GetAirlineFBOs(int airlineId);
        Task<IList<AirlineFboDbModel>> GetFbos(string icao, int airlineId);
        Task<IList<AirlineFboDbModel>> HireFbo(string icao, string userId);
        Task<bool> JoinAirline(int airlineId, string userId);
        Task<bool> ExitAirline(int id, string userId);
        Task<IList<AirlineModel>> GetRanking();
    }
}
