using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.ValidationRules;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using log4net;
using ModernWpf;
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
        private IInfraService _infraService;
        private NotificationManager _notificationManager;
        private LoginResponseModel _loginData;
        private MainWindow _mainWindow;
        private int _loadingCount;
        private UserSettingsViewModel _userSettings;

        private readonly IAbstractFactory<IUserAccessService> _factoryUser;
        private static readonly ILog _log = LogManager.GetLogger(typeof(Login));

        public Login(IAbstractFactory<IInfraService> factoryInfra, 
                     IAbstractFactory<IJobService> factoryJob, 
                     IAbstractFactory<IUserAccessService> factoryUser, 
                     MainWindow mainWindow)
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _factoryUser = factoryUser;

            _jobService = factoryJob.Create();
            _userAccessService = factoryUser.Create();
            _infraService = factoryInfra.Create();
            _mainWindow = mainWindow;
            _userSettings = mainWindow.LoadSettingsFromFile();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (!new Register(_factoryUser).ShowDialog().Value)
            {
                Application.Current.Shutdown();
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var destinationurl = $"{_infraService.GetApiUrl()}Account/ForgotPassword";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }

        private async void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            var userViewModel = (AspnetUserViewModel)DataContext;

            if (await SignIn(userViewModel, false))
            {
                Hide();
                _mainWindow.Show();
            }
            else
            {
                _notificationManager.Show("Warning", "Wrong login data. Check you email and password.", NotificationType.Warning, "WindowArea");
            }
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

        private void EnableControls(bool enabled)
        {
            txbEmail.IsEnabled = enabled;
            txbPassword.IsEnabled = enabled;
            btnSignIn.IsEnabled = enabled;
        }

        private async Task<bool> SignIn(AspnetUserViewModel userViewModel, bool discreteLogin)
        {
            ShowLoading();
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                EnableControls(false);
                _loginData = await _userAccessService.Login(userViewModel.Email, userViewModel.Password);
                if (_loginData != null)
                {
                    await _userAccessService.LoadUserStatisticsProperties(_loginData.UserId);

                    if (!discreteLogin)
                    {
                        _notificationManager.Show("Success", $"Welcome capitan {_loginData.UserName}", NotificationType.Success, "WindowArea");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        userViewModel.Id = _loginData.UserId;
                        userViewModel.NickName = _loginData.UserName;
                        SaveLoginData(userViewModel);
                    }
                    await LoadUserJobList(_loginData.UserId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
                _log.Error($"SignIn failed.", ex);
                HideLoading();
                Mouse.OverrideCursor = Cursors.Arrow;
                ShowModal("Select Host", new SelectHostUrlModal(_userSettings));
            }
            finally
            {
                HideLoading();
                Mouse.OverrideCursor = Cursors.Arrow;
                EnableControls(true);
            }
            
            return false;
        }

        public bool LoadLoginData()
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var lines = File.ReadLines(System.IO.Path.Combine(path, "ResourceData\\LoginSavedData.ini"));
                var line = lines?.FirstOrDefault();
                var info = line?.Split('|');
                if (info?.Length == 4)
                {
                    AppProperties.UserLogin = new LoginResponseModel() 
                    {
                        UserId = info[3],
                        UserName = info[2]
                    };
                    return true;
                    //txbEmail.Text = info[0];
                    //txbPassword.Password = info[1];
                    //loaded = EmailValidationRule.IsValidEmail(txbEmail.Text) && !string.IsNullOrEmpty(txbPassword.Password);
                }
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Cannot load the login data.", NotificationType.Error, "WindowArea");
                _log.Error($"LoadLoginData failed.", ex);
            }
            return false;
        }

        public void ShowLoading()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            _loadingCount++;
        }

        public void HideLoading()
        {
            _loadingCount--;

            if (_loadingCount <= 0)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                _loadingCount = 0;
            }
        }

        private void SaveLoginData(AspnetUserViewModel userViewModel)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(path, "ResourceData/LoginSavedData.ini");
                string createText = $"{userViewModel.Email}|{userViewModel.Password}|{userViewModel.NickName}|{userViewModel.Id}";
                File.WriteAllText(path, createText);
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Cannot save the login data.", NotificationType.Error, "WindowArea");
                _log.Error($"SaveLoginData failed.", ex);
            }
        }

        private async Task LoadUserJobList(string userId)
        {
            try
            {
                await _jobService.GetAllUserJobs(userId);
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "User jobs could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
                _log.Error($"LoadUserJobList failed.", ex);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            btnSignIn.IsEnabled = EmailValidationRule.IsValidEmail(txbEmail.Text) && !string.IsNullOrEmpty(txbPassword.Password);
        }
    }
}
