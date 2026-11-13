using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.MSFS2024.Navdata;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views;
using FlightJobsDesktop.Views.Account;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace FlightJobsDesktop
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
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
            services.AddAbstractFactory<IPilotService, PilotService>();

            services.AddAbstractFactory<IAirportDataProvider, SimConnectNavdataService>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<Login>();
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            SingleInstanceCheck();

            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

            var userAccessService = _serviceProvider.GetService<IUserAccessService>();

            var isLoginDataLoaded = userAccessService.LoadLoginData();
            var isTryAutoLoginSuccess = await userAccessService.TryAutoLoginWithSavedTokens();

            if (isLoginDataLoaded && isTryAutoLoginSuccess)
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlightJobsDesktop\\ResourceData\\Settings.json");
                var jsonSettings = File.ReadAllText(path);
                var userSettings = JsonConvert.DeserializeObject<UserSettingsViewModel>(jsonSettings);

                if (!userSettings.StartInSysTray)
                {
                    var mainWindow = new MainWindow(
                        _serviceProvider.GetService<IAbstractFactory<IInfraService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IJobService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IUserAccessService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IPilotService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IAirportDataProvider>>());
                    mainWindow.Show();
                }
                else
                {
                    new NotificationManager().ShowButtonWindow("FlightJobs is running. Double-click to start.");
                }
            }
            else
            {
                var loginWindow = new Login(
                    _serviceProvider.GetService<IAbstractFactory<IInfraService>>(),
                    _serviceProvider.GetService<IAbstractFactory<IJobService>>(),
                    _serviceProvider.GetService<IAbstractFactory<IUserAccessService>>(),
                    new MainWindow(
                        _serviceProvider.GetService<IAbstractFactory<IInfraService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IJobService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IUserAccessService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IPilotService>>(),
                        _serviceProvider.GetService<IAbstractFactory<IAirportDataProvider>>()
                    )
                );
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
                System.Windows.Forms.MessageBox.Show($"You already have an instance of {proc.ProcessName} running.", 
                    "FlightJobs Desktop");
                App.Current.Shutdown();
            }
        }
    }
}
