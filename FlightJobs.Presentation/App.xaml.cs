using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.Views;
using FlightJobsDesktop.Views.Account;
using FlightJobsDesktop.Views.SlidersWindows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FlightJobsDesktop
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

            DbModelToViewModelMapper.Initialize();
            var mapCfg = ViewModelToDbModelMapper.Initialize();

            services.AddAbstractFactory<IJobService, JobService>();
            services.AddAbstractFactory<IUserAccessService, UserAccessService>();
            services.AddAbstractFactory<IInfraService, InfraService>();
            services.AddAbstractFactory<IAirlineService, AirlineService>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<Login>();
            services.AddSingleton<Register>();

            
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
//            new CurrentJobDataWindow().Show();
            var loginWindow = _serviceProvider.GetService<Login>();
            loginWindow.Show();
        }
    }
}
