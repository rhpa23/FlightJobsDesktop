using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class PaginatedJobsModel
    {
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }
        public IList<JobModel> Jobs { get; set; }
    }

    public class FilterJobsModel
    {
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string ModelDescription { get; set; }
        public string UserId { get; set; }
    }
}
