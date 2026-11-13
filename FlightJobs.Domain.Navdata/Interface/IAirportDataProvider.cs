using FlightJobs.Domain.Navdata.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightJobs.Domain.Navdata.Interface
{
    public interface IAirportDataProvider
    {
        Task<AirportEntity> GetAirportAsync(string icao);
        Task<IList<RunwayEntity>> GetRunwaysAsync(string icao);
        Task<bool> LoadFlightDataAsync(string departureIcao, string arrivalIcao, string alternativeIcao = null);
        AirportEntity GetCachedAirport(string icao);
    }
}