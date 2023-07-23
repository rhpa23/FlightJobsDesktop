using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.Views;
using FlightJobsDesktop.Views.Account;
using FlightJobsDesktop.Views.SlidersWindows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Linq;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata;
using log4net.Config;
using FlightJobs.Domain.Navdata.Utils;
using FlightJobsDesktop.Views.POC;
using ModernWpf;
using System.Diagnostics;

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
            XmlConfigurator.Configure();
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
            services.AddAbstractFactory<IPilotService, PilotService>();

            services.AddAbstractFactory<ISqLiteDbContext, SqLiteDbContext>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<Login>();
            services.AddSingleton<Register>();

            
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            //            new CurrentJobDataWindow().Show();
            //new ChartsPoC().Show();

            SingleInstanceCheck();

            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            var loginWindow = _serviceProvider.GetService<Login>();
            if (loginWindow.LoadLoginData())
            {
                var mainWindow = _serviceProvider.GetService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                loginWindow.Show();
            }
        }

        private void SingleInstanceCheck()
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Where(p =>
                p.ProcessName == proc.ProcessName).Count();

            if (count > 1) // Single Instance check
            {
                MessageBox.Show($"You already have an instance of {proc.ProcessName} running.", "FlightJobs Desktop", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.Current.Shutdown();
            }
        }
    }
}
