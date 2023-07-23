using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class SimBriefModel
    {
        [JsonProperty("origin")]
        public AirportInfo DepartureICAO { get; set; }
        [JsonProperty("destination")]
        public AirportInfo ArrivalICAO { get; set; }
        [JsonProperty("alternate")]
        public AirportInfo AlternativeICAO { get; set; }
    }

    public class AirportInfo
    {
        [JsonProperty("icao_code")]
        public string IcaoCode { get; set; }
    }
}
