using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.ValidationRules;
using FlightJobsDesktop.ViewModels;
using Notification.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlightJobsDesktop.Views.Account
{
    /// <summary>
    /// Lógica interna para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private IJobService _jobService;
        private IUserAccessService _userAccessService;
        private NotificationManager _notificationManager;
        private LoginResponseModel _loginData;
        private MainWindow _mainWindow;

        public Login(IAbstractFactory<IJobService> factoryJob, 
                     IAbstractFactory<IUserAccessService> factoryUser, 
                     MainWindow mainWindow)
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();

            _jobService = factoryJob.Create();
            _userAccessService = factoryUser.Create();
            _mainWindow = mainWindow;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            //new Register(_flightJobsConnectorClientAPI).Show();
            this.Hide();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            new ForgotPassword().Show();
            this.Hide();
        }

        private async void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = (AspnetUserViewModel)DataContext;

            if (await SignIn(userViewModel, false))
            {
                this.Hide();
                _mainWindow.Show();
            }
            else
            {
                _notificationManager.Show("Warning", "Wrong login data. Check you email and password.", NotificationType.Warning, "WindowArea");
            }
        }

        private void EnableControls(bool enabled)
        {
            txbEmail.IsEnabled = enabled;
            txbPassword.IsEnabled = enabled;
            btnSignIn.IsEnabled = enabled;
        }

        private async Task<bool> SignIn(AspnetUserViewModel userViewModel, bool discreteLogin)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowArea");
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                EnableControls(false);
                _loginData = await _userAccessService.Login(userViewModel.Email, userViewModel.Password);
                if (_loginData != null)
                {
                    await _userAccessService.GetUserStatistics(_loginData.UserId);

                    if (!discreteLogin)
                    {
                        _notificationManager.Show("Success", $"Welcome capitan {_loginData.UserName}", NotificationType.Success, "WindowArea");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        SaveLoginData();
                    }
                    await LoadUserJobList(_loginData.UserId);
                    return true;
                }
            }
            catch (Exception)
            {
                //log.Error($"btnLogin_Click failed.", ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                progress.Dispose();
                Mouse.OverrideCursor = Cursors.Arrow;
                EnableControls(true);
            }
            
            return false;
        }

        private bool LoadLoginData()
        {
            bool loaded = false;
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var lines = File.ReadLines(System.IO.Path.Combine(path, "ResourceData\\LoginSavedData.ini"));
                var line = lines?.FirstOrDefault();
                var info = line?.Split('|');
                if (info?.Length == 2)
                {
                    txbEmail.Text = info[0];
                    txbPassword.Password = info[1];
                    loaded = EmailValidationRule.IsValidEmail(txbEmail.Text) && !string.IsNullOrEmpty(txbPassword.Password);
                }
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Cannot load the login data.", NotificationType.Error, "WindowArea");
                //log.Error($"LoadLoginData failed.", ex);
            }
            return loaded;
        }

        private void SaveLoginData()
        {
            try
            {
                var path = System.AppDomain.CurrentDomain.BaseDirectory;
                path = System.IO.Path.Combine(path, "ResourceData/LoginSavedData.ini");
                string createText = $"{txbEmail.Text}|{txbPassword.Password}";
                File.WriteAllText(path, createText);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Cannot save the login data.", NotificationType.Error, "WindowArea");
                //log.Error($"SaveLoginData failed.", ex);
            }
        }

        private async Task LoadUserJobList(string userId)
        {
            try
            {
                await _jobService.GetAllUserJobs(_loginData.UserId);
            }
            catch
            {
                _notificationManager.Show("Error", "User jobs could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoadLoginData())
            {
                var userViewModel = (AspnetUserViewModel)DataContext;

                if (await SignIn(userViewModel, true))
                {
                    this.Hide();
                    _mainWindow.Show();
                }
            }
        }
    }
}
