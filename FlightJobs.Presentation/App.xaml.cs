using FlightJobs.Infrastructure.Services;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.Views;
using FlightJobsDesktop.Views.Account;
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
            //var userAccess = new UserAccessService();
            var jobService = new JobService();
            var userAccessService = new UserAccessService();

            services.AddSingleton<MainWindow>();
            //services.AddSingleton(userAccess);
            services.AddSingleton(new Login(userAccessService, jobService));
            services.AddSingleton(new Register());
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var loginWindow = _serviceProvider.GetService<Login>();
            loginWindow.Show();
        }
    }
}
