﻿using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
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
        private NotificationManager _notificationManager;

        private UserSettingsViewModel _userSettings;

        public SettingsView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
        }

        private void SaveSettings()
        {
            string jsonData = JsonConvert.SerializeObject(_userSettings, Formatting.None);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "ResourceData/Settings.json");
            File.WriteAllText(path, jsonData);

            var settingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
            AppProperties.UserSettings = settingsModel;
        }

        private void ThemeSwitch_Toggled(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ThemeSwitch.IsOn)
            {
                _userSettings.ThemeName = "Light";
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
            }
            else
            {
                _userSettings.ThemeName = "Dark";
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
            }

            SaveSettings();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _userSettings = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<UserSettingsModel, UserSettingsViewModel>(AppProperties.UserSettings);
            DataContext = _userSettings;
        }

        private void btnUpdateApp_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Saving...", false, true, "WindowArea");
            try
            {
                var userSettingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
                userSettingsModel.UserId = AppProperties.UserLogin.UserId;
                var result = await new UserAccessService().UpdateUserSettings(userSettingsModel);
                    
                _notificationManager.Show("Success", "Settings saved!", NotificationType.Success, "WindowArea");
            }
            catch (Exception)
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