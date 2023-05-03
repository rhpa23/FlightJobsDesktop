using System.Collections;
using System.Collections.Generic;

namespace FlightJobsDesktop.ViewModels
{
    public class PilotsHiredViewModel
    {
        public IList<PilotHiredViewModel> PilotsHired { get; set; }
    }

    public class PilotHiredViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
    }
}
