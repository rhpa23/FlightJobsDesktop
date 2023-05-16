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

        public async Task<IList<AirlineFboDbModel>> GetAirlineFBOs(int airlineId)
        {
            return await _flightJobsConnectorClientAPI.GetAirlineFBOs(airlineId);
        }

        public async Task<PaginatedAirlinersModel> GetByFilter(string sortOrder, string currentSort, int pageNumber, PaginatedAirlinersFilterModel filterModel)
        {
            return await _flightJobsConnectorClientAPI.GetAirliners(sortOrder, currentSort, pageNumber, filterModel);
        }

        public async Task<bool> UpdateAirline(AirlineModel airline, string userId)
        {
            return await _flightJobsConnectorClientAPI.UpdateAirline(airline, userId);
        }

        public async Task<AirlineModel> CreateAirline(AirlineModel airline, string userId)
        {
            return await _flightJobsConnectorClientAPI.CreateAirline(airline, userId);
        }

        public async Task<bool> PayAirlineDebts(int airlineId, string userId)
        {
            return await _flightJobsConnectorClientAPI.PayAirlineDebts(new AirlineModel() { Id = airlineId }, userId);
        }

        public async Task<PaginatedAirlineJobLedgerModel> GetAirlineLedger(int airlineId, int pageNumber, FilterJobsModel filterJob)
        {
            if (filterJob == null) filterJob = new FilterJobsModel();
            return await _flightJobsConnectorClientAPI.GetAirlineLedger(airlineId, pageNumber, filterJob);
        }

        public async Task<IList<AirlineFboDbModel>> GetFbos(string icao, int airlineId)
        {
            return await _flightJobsConnectorClientAPI.GetFbos(icao, airlineId);
        }

        public async Task<IList<AirlineFboDbModel>> HireFbo(string icao, string userId)
        {
            return await _flightJobsConnectorClientAPI.HireFbo(icao, userId);
        }

        public async Task<bool> JoinAirline(int airlineId, string userId)
        {
            return await _flightJobsConnectorClientAPI.JoinAirline(new AirlineModel() { Id = airlineId }, userId);
        }

        public async Task<bool> ExitAirline(int airlineId, string userId)
        {
            return await _flightJobsConnectorClientAPI.ExitAirline(new AirlineModel() { Id = airlineId }, userId);
        }
    }
}
