using FlightJobsDesktop.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class GenerateJobViewModel : ObservableObject
    {
        public GenerateJobViewModel()
        {
            DepartureICAO = "";
            ArrivalICAO = "";
            AlternativeICAO = "";
        }
        public string DepartureICAO { get; set; }

        public string ArrivalICAO { get; set; }

        public string AlternativeICAO { get; set; }

        public long Dist { get; set; }
        public string DistDesc { get { return Dist + " NM"; } }
        public IList<CurrentJobViewModel> PendingJobs { get; set; }
        public CurrentJobViewModel PendingSelectedJob { get; set; }
    }
}
