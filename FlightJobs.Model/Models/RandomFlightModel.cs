using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class RandomFlightModel
    {
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string AlternativeICAO { get; set; }
    }
}
