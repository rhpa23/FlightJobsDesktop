using FlightJobs.Domain.Entities;
using FlightJobs.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IUserAccessService
    {
        Aspnetuser GetAspnetuser(string email);
        Task<ApplicationUser> RegisterUserAsync(ApplicationUser applicationUser, string password);
    }
}
