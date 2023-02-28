using ConnectorClientAPI;
using FlightJobs.Infrastructure.Services.Interfaces;

namespace FlightJobs.Infrastructure.Services
{
    public class InfraService : ServiceBase, IInfraService
    {
        public string GetApiUrl()
        {
            return FlightJobsConnectorClientAPI.SITE_URL;
        }
    }
}
