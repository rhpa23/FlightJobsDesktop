using FlightJobs.Domain.Common;
using FlightJobs.Domain.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlightJobs.Infrastructure.Persistence
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Observe que o authenticationType deve corresponder àquele definido em CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Adicionar declarações de usuário personalizado aqui
            return userIdentity;
        }
    }

    internal partial class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        private const string DB_KEY = "hfjfbjnkds55ytr@99866543#xdghhggfdw";

        private ApplicationContext(DbConnection dbConnection) : base(dbConnection, true) {}

        internal static ApplicationContext Create()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["FlightJobsDatabase"].ConnectionString;
            var cs = CipherService.Decrypt(connectionString, DB_KEY);
            var conn = DbProviderFactories.GetFactory("MySql.Data.MySqlClient").CreateConnection();
            conn.ConnectionString = cs;

            return new ApplicationContext(conn);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Aspnetuser>().ToTable("aspnetusers");
            modelBuilder.Entity<Aspnetrole>().ToTable("aspnetroles");
            modelBuilder.Entity<Aspnetuserrole>().HasKey(k => new { k.UserId, k.RoleId }).ToTable("aspnetuserroles");
            modelBuilder.Entity<Aspnetuserclaim>().ToTable("aspnetuserclaims");
            modelBuilder.Entity<Aspnetuserlogin>().HasKey(r => new { r.UserId, r.LoginProvider, r.ProviderKey }).ToTable("aspnetuserlogins");
            modelBuilder.Entity<Statisticsdbmodel>().ToTable("statisticsdbmodels");

            modelBuilder.Entity<Jobdbmodel>().ToTable("jobdbmodels");
            modelBuilder.Entity<Airlinedbmodel>().ToTable("airlinedbmodels");
            modelBuilder.Entity<Airlinecertificatesdbmodel>().ToTable("airlinecertificatesdbmodels");
            modelBuilder.Entity<Statisticcertificatesdbmodel>().ToTable("statisticcertificatesdbmodels");
            modelBuilder.Entity<Certificatedbmodel>().ToTable("certificatedbmodels");
            modelBuilder.Entity<Jobairlinedbmodel>().ToTable("jobairlinedbmodels");
            modelBuilder.Entity<Pilotlicenseexpensesdbmodel>().ToTable("pilotlicenseexpensesdbmodels");
            modelBuilder.Entity<Pilotlicenseitemdbmodel>().ToTable("pilotlicenseitemdbmodels");
            modelBuilder.Entity<Licenseitemuserdbmodel>().ToTable("licenseitemuserdbmodels");
            modelBuilder.Entity<Pilotlicenseexpensesuserdbmodel>().ToTable("pilotlicenseexpensesuserdbmodels");
            modelBuilder.Entity<Airlinefbodbmodel>().ToTable("airlinefbodbmodels");
            modelBuilder.Entity<Customplanecapacitydbmodel>().ToTable("customplanecapacitydbmodels");

        }

        public DbSet<Aspnetuser> AspnetUsers { get; set; }
        public DbSet<Jobdbmodel> JobDbModels { get; set; }
        public DbSet<Airlinedbmodel> AirlineDbModels { get; set; }
        public DbSet<Airlinecertificatesdbmodel> AirlineCertificatesDbModels { get; set; }
        public DbSet<Statisticcertificatesdbmodel> StatisticCertificatesDbModels { get; set; }
        public DbSet<Certificatedbmodel> CertificateDbModels { get; set; }
        public DbSet<Statisticsdbmodel> StatisticsDbModels { get; set; }
        public DbSet<Jobairlinedbmodel> JobAirlineDbModels { get; set; }
        public DbSet<Pilotlicenseexpensesdbmodel> PilotLicenseExpenses { get; set; }
        public DbSet<Pilotlicenseitemdbmodel> PilotLicenseItem { get; set; }
        public DbSet<Licenseitemuserdbmodel> LicenseItemUser { get; set; }
        public DbSet<Pilotlicenseexpensesuserdbmodel> PilotLicenseExpensesUser { get; set; }
        public DbSet<Airlinefbodbmodel> AirlineFbo { get; set; }
        public DbSet<Customplanecapacitydbmodel> CustomPlaneCapacity { get; set; }

    }
}
