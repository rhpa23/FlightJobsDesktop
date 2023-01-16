using FlightJobs.Domain.Entities;
using FlightJobs.Infrastructure.Account;
using FlightJobs.Infrastructure.Persistence;
using FlightJobs.Infrastructure.Services.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services
{
    public class UserAccessService : IUserAccessService
    {
        private ApplicationContext _applicationContext;

        public UserAccessService() 
        {
            _applicationContext = ApplicationContext.Create();
        }

        public Aspnetuser GetAspnetuser(string email) =>
             _applicationContext.AspnetUsers.FirstOrDefault(u => u.Email == email);

        public async Task<ApplicationUser> RegisterUserAsync(ApplicationUser applicationUser, string password)
        {
            var appManager = ApplicationUserManager.Create(new Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<ApplicationUserManager>());

            var result = await appManager.CreateAsync(applicationUser, password);
            if (result.Succeeded)
            {
                return applicationUser;
            }
            return null;
        }
    }
}
