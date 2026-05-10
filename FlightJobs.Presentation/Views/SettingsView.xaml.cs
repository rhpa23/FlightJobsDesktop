using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using log4net;
using ModernWpf;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private NotificationManager _notificationManager;
        private IUserAccessService _userAccessService;

        private UserSettingsViewModel _userSettings;
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public SettingsView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _userAccessService = MainWindow.UserServiceFactory.Create();
        }

        private void SaveSettings()
        {
            string jsonData = JsonConvert.SerializeObject(_userSettings, Formatting.None);
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlightJobsDesktop\\ResourceData\\Settings.json");
            File.WriteAllText(path, jsonData);

            var settingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
            AppProperties.UserSettings = settingsModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _userSettings = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<UserSettingsModel, UserSettingsViewModel>(AppProperties.UserSettings);
                _userSettings.WeightUnit = _userSettings.WeightUnit == null ? "kg" : _userSettings.WeightUnit;
                DataContext = _userSettings;
            }
            catch (Exception ex)
            {
                _log.Error("SettingsView Loaded", ex);
                _notificationManager.Show("Error", "Error when try to load FlightJobs settings.", NotificationType.Error, "WindowArea");
            }
        }

        private void ckbBase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
            }
            catch (Exception ex) 
            { 
                _log.Error("SettingsView Checkbox click", ex); 
                _notificationManager.Show("Error", "Error when try to save application settings. Please verify your administrator permissions.", NotificationType.Error, "WindowArea"); 
            }
        }

    }
}
