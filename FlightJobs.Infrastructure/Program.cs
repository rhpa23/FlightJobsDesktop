using FlightJobs.Infrastructure.Persistence;
using FlightJobs.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var user =  new UserAccessService().GetAspnetUser("rhpa23@gmail.com");
            Console.WriteLine($"UserName: {user.UserName}");
        }
    }
}
