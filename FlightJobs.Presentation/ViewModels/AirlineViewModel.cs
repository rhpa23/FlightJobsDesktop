using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class AirlineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public long AirlineScore { get; set; }
        public long BankBalance { get; set; }
        public int MinimumScoreToHire { get; set; }
    }
}
