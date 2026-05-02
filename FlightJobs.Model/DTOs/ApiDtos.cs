using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.DTOs
{
    // DTOs para Jobs
    public class StartJobResponseDto
    {
        public string arrivalICAO { get; set; }
        public string message { get; set; }
        public int jobId { get; set; }
    }

    public class FinishJobResponseDto
    {
        public string message { get; set; }
        public decimal earnings { get; set; }
        public decimal score { get; set; }
        public int experience { get; set; }
    }    
}
