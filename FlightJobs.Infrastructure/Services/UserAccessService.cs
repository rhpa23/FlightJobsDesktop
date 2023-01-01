using FlightJobs.Domain.Entities;
using FlightJobs.Infrastructure.Persistence;
using FlightJobs.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService //: IUserAccessService
    {
        private ApplicationContext _applicationContext;

        public UserAccessService() 
        {
            _applicationContext = ApplicationContext.Create();
        }

        public Aspnetuser GetAspnetUser(string email) =>
             _applicationContext.AspnetUsers.FirstOrDefault(u => u.Email == email);
    }
}
