using ConnectorClientAPI;
using ConnectorClientAPI.Exceptions;
using ConnectorClientAPI.Models;
using FlightJobsDesktop.ViewModels;
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
        private FlightJobsConnectorClientAPI _flightJobsConnectorClientAPI;
        private NotificationManager _notificationManager;

        private UserSettingsViewModel userSettings;

        public SettingsView()
        {
            InitializeComponent();
            _flightJobsConnectorClientAPI = new FlightJobsConnectorClientAPI();
            _notificationManager = new NotificationManager();
        }

        private void SaveSettings()
        {
            string jsonData = JsonConvert.SerializeObject(userSettings, Formatting.None);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "ResourceData/Settings.json");
            File.WriteAllText(path, jsonData);
        }

        private void ThemeSwitch_Toggled(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ThemeSwitch.IsOn)
            {
                userSettings.ThemeName = "Light";
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
            }
            else
            {
                userSettings.ThemeName = "Dark";
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
            }
            SaveSettings();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            userSettings = new UserSettingsViewModel() { SimConnectStatus = "Waiting for sim start...", ThemeName = "Light" };
            var settingsData = Application.Current.Properties[AppConstants.KEY_SETTINGS_DATA];
            if (settingsData != null)
            {
                userSettings = (UserSettingsViewModel)settingsData;
            }

            var loginData = Application.Current.Properties[AppConstants.KEY_LOGIN_DATA];
            var userStatistics = (UserStatisticsModel)Application.Current.Properties[AppConstants.KEY_USER_STATISTICS_DATA];
            if (loginData != null && userStatistics != null)
            {
                var loginModel = (LoginResponseModel)loginData;
                userSettings.Username = loginModel.UserName;
                bool isKg = (bool)(userStatistics.WeightUnit?.Contains("kg"));
                userSettings.IsWeightUnitKg = isKg;
                userSettings.IsWeightUnitPounds = !isKg;
                userSettings.WeightUnit = userStatistics.WeightUnit;
                userSettings.ReceiveAlertEmails = userStatistics.SendAirlineBillsWarning && userStatistics.SendLicenseWarning;
            }

            DataContext = userSettings;
        }

        private void btnUpdateApp_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Saving...", false, true, "WindowArea");
            try
            {
                var loginData = Application.Current.Properties[AppConstants.KEY_LOGIN_DATA];
                if (loginData != null)
                {
                    var loginModel = (LoginResponseModel)loginData;
                    var user = new UserSettingsModel() // TODO: AutoMapper
                    { 
                        UserId = loginModel.UserId, 
                        UserName = userSettings.Username,
                        Password = userSettings.Password, 
                        WeightUnit = userSettings.WeightUnit,
                        ReceiveAlertEmails = userSettings.ReceiveAlertEmails
                    };
                    var result = await _flightJobsConnectorClientAPI.UpdateUserSettings(user);
                    
                    _notificationManager.Show("Success", "Settings saved!", NotificationType.Success, "WindowArea");
                }
            }
            catch (ApiException ex)
            {
                _notificationManager.Show("Error", ex.ErrorMessage, NotificationType.Error, "WindowArea");
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Error when try to save FlightJobs settings. Please verify your internet connection.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private void ckbBase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to save application settings. Please verify your administrator permissions.", NotificationType.Error, "WindowArea");
            }
        }
    }
}
