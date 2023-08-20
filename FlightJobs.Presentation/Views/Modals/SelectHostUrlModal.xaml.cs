using FlightJobs.Infrastructure;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using log4net;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para SelectHostUrlModal.xam
    /// </summary>
    public partial class SelectHostUrlModal : UserControl
    {
        private UserSettingsViewModel _userSettings;

        private NotificationManager _notificationManager;
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public SelectHostUrlModal()
        {
            InitializeComponent();
        }

        public SelectHostUrlModal(UserSettingsViewModel userSettings)
        {
            InitializeComponent();

            _notificationManager = new NotificationManager();
            _userSettings = userSettings;
        }

        private void SaveSettings()
        {
            string jsonData = JsonConvert.SerializeObject(_userSettings, Formatting.None);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = System.IO.Path.Combine(path, "ResourceData/Settings.json");
            File.WriteAllText(path, jsonData);

            var settingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
            AppProperties.UserSettings = settingsModel;
        }

        private void ApplyServiceUrl() 
        {
            var infraService = MainWindow.InfraServiceFactory.Create();
            var selectHost = new SelectHostViewModel();

            switch (_userSettings.SelectedHostOption)
            {
                case 1:
                    infraService.SetApiUrl(selectHost.Option1HostUrl);
                    break;
                case 2:
                    infraService.SetApiUrl(selectHost.Option2HostUrl);
                    break;
                case 3:
                    infraService.SetApiUrl(selectHost.Option3HostUrl);
                    break;
                case 4:
                    infraService.SetApiUrl(selectHost.Option4HostUrl);
                    break;
                default:
                    break;
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyServiceUrl();

                SaveSettings();
                _notificationManager.Show("Success", "Saved!", NotificationType.Success, "WindowArea");
                ((Window)Parent).Close();
            }
            catch (Exception ex)
            {
                _log.Error("SelectHostUrlModal SaveSettings", ex);
                _notificationManager.Show("Error", "Error when try to save host url.", NotificationType.Error, "WindowArea");
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "Notification");
            btnConfirmBorder.IsEnabled = false;
            try 
            {
                var selectHost = new SelectHostViewModel();

                switch (_userSettings.SelectedHostOption)
                {
                    case 1:
                        selectHost.Option1IsSelected = true;
                        break;
                    case 2:
                        selectHost.Option2IsSelected = true;
                        break;
                    case 3:
                        selectHost.Option3IsSelected = true;
                        break;
                    case 4:
                        selectHost.Option4IsSelected = true;
                        break;
                    default:
                        break;
                }

                var infraService = MainWindow.InfraServiceFactory.Create();
                selectHost.Option1IsOnline = await infraService.PingUrl(selectHost.Option1HostUrl);
                selectHost.Option2IsOnline = await infraService.PingUrl(selectHost.Option2HostUrl);
                selectHost.Option3IsOnline = await infraService.PingUrl(selectHost.Option3HostUrl);
                selectHost.Option4IsOnline = await infraService.PingUrl(selectHost.Option4HostUrl);

                DataContext = selectHost;
                btnConfirmBorder.IsEnabled = true;
            } 
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access FlightJobs.", NotificationType.Error, "Notification");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private void rbdOption_Checked(object sender, RoutedEventArgs e)
        {
            var selectHost = (SelectHostViewModel)DataContext;
            var rdb = (RadioButton)sender;

            if (rdb == rbdOption1) _userSettings.SelectedHostOption = 1;
            if (rdb == rbdOption2) _userSettings.SelectedHostOption = 2;
            if (rdb == rbdOption3) _userSettings.SelectedHostOption = 3;
            if (rdb == rbdOption4) _userSettings.SelectedHostOption = 4;
        }
    }
}
