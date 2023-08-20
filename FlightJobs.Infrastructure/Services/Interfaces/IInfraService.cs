using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IInfraService
    {
        void SetApiUrl(string siteUrl);
        string GetApiUrl();

        Task<bool> PingUrl(string url);
    }
}
