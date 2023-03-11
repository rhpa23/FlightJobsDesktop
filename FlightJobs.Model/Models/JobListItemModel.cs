using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class JobListItemModel
    {
        public int Id { get; set; }
        public string PayloadView { get; set; }
        public string PayloadLabel { get; set; }
        public long Pax { get; set; }
        public long Cargo { get; set; }
        public long Pay { get; set; }
        public bool FirstClass { get; set; }
        public bool IsCargo { get; set; }
        public string AviationType { get; set; }
    }
}
