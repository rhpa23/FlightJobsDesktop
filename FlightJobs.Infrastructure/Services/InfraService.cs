using ConnectorClientAPI;
using FlightJobs.Infrastructure.Services.Interfaces;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class InfraService : ServiceBase, IInfraService
    {
        public void SetApiUrl(string siteUrl)
        {
            FlightJobsConnectorClientAPI.SiteUrl = siteUrl;
        }

        public string GetApiUrl()
        {
            return FlightJobsConnectorClientAPI.SiteUrl;
        }

        public async Task<bool> PingUrl(string url)
        {
            return await _flightJobsConnectorClientAPI.PingUrl(url);
        }
        public string GetAccessToken()
        {
            return FlightJobsConnectorClientAPI.GetAccessToken();
        }

        public string GetRefreshToken()
        {
            return FlightJobsConnectorClientAPI.GetRefreshToken();
        }
    }
}
