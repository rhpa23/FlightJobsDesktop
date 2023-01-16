using FlightJobs.Infrastructure.Services;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.Views.Account;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            var mapCfg = ViewModelToDbModelMapper.Initialize();
            var userAccess = new UserAccessService();

            services.AddSingleton<MainWindow>();
            services.AddSingleton(userAccess);
            services.AddSingleton(new Login(userAccess));
            services.AddSingleton(new Register(userAccess, mapCfg));

        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
