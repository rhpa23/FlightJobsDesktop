using ConnectorClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class ServiceBase
    {
        internal FlightJobsConnectorClientAPI _flightJobsConnectorClientAPI;

        public ServiceBase()
        {
            _flightJobsConnectorClientAPI = new FlightJobsConnectorClientAPI();
        }
    }
}
