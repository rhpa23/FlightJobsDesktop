﻿using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
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

        private void ThemeSwitch_Toggled(object sender, System.Windows.RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                _log.Error("SettingsView ThemeSwitch", ex);
                _notificationManager.Show("Error", "Error when try to apply Theme Settings.", NotificationType.Error, "WindowArea");
            }
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

        private void btnUpdateApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
                _notificationManager.Show("Success", "Application settings saved!", NotificationType.Success, "WindowArea");
            }
            catch (Exception ex)
            {
                _log.Error("SettingsView UpdateApp", ex);
                _notificationManager.Show("Error", "Error when try to save FlightJobs settings. Please your administrator rights.", NotificationType.Error, "WindowArea");
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            try
            {
                var userSettingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
                userSettingsModel.UserId = AppProperties.UserLogin.UserId;
                var result = await _userAccessService.UpdateUserSettings(userSettingsModel);
                AppProperties.UserSettings = userSettingsModel;
                _notificationManager.Show("Success", "Settings saved!", NotificationType.Success, "WindowArea");
            }
            catch (Exception ex)
            {
                _log.Error("SettingsView btnUpdate", ex);
                _notificationManager.Show("Error", "Error when try to save FlightJobs settings. Please verify your internet connection.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
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

        private void btnSelectHost_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Select Host (Confirm and restart for take effect)", new SelectHostUrlModal(_userSettings));
            //MainWindow.loa
        }

        private void ShowModal(string title, object content)
        {

            Window window = new Window
            {
                Title = title,
                Content = content,
                Width = ((UserControl)content).MinWidth,
                Height = ((UserControl)content).MinHeight + 40,
                //SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                WindowStyle = WindowStyle.ToolWindow,
                Topmost = true,
            };

            window.ShowDialog();
        }
    }
}
