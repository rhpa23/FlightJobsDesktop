using FlightJobs.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Services.Interfaces
{
    public interface IPilotService
    {
        Task<UserStatisticsModel> GetUserStatisticsFlightsInfo(string userId);

        Task<IList<PilotLicenseExpensesUserModel>> GetUserLicensesOverdue(string userId);
        Task SaveAvatar(string userId, string fileName);
        Task TranfersMoneyToAirline(string userId, int percent);
        Task<UserStatisticsModel> BuyLicencePackage(string userId, long licenseExpenseId);
    }
}
