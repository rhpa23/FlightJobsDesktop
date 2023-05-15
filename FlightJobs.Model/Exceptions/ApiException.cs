using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model
{
    public class ApiException : Exception
    {
        public string ErrorMessage { get; set; }
        public ApiException() { }
        public ApiException(string msg) { ErrorMessage = msg; }
    }
}
