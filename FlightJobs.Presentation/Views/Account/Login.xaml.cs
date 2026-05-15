using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.ValidationRules;
using FlightJobsDesktop.ViewModels;
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

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var destinationurl = $"{_infraService.GetApiUrl()}login";
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
                    await _userAccessService.LoadUserStatisticsProperties();

                    if (!discreteLogin)
                    {
                        _notificationManager.Show("Success", $"Welcome capitan {_loginData.UserName}", NotificationType.Success, "WindowArea");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        userViewModel.Id = _loginData.UserId;
                        userViewModel.NickName = _loginData.UserName;
                        _userAccessService.SaveLoginData(_loginData);
                    }
                    await LoadUserJobList();
                    
                    // Verifica se há job pendente para recuperação
                    await CheckAndRecoverPendingJob();
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
                _log.Error($"SignIn failed.", ex);
                HideLoading();
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            finally
            {
                HideLoading();
                Mouse.OverrideCursor = Cursors.Arrow;
                EnableControls(true);
            }
            
            return false;
        }

        /// <summary>
        /// Verifica e recupera dados de job pendente salvo localmente
        /// </summary>
        private async Task CheckAndRecoverPendingJob()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlightJobsDesktop\\ResourceData\\PendingJob.json");
                
                if (File.Exists(path))
                {
                    _log.Info("Pending job data found. Attempting to recover...");
                    
                    // Notifica usuário sobre job pendente
                    _notificationManager.Show(
                        "Pending Job Found",
                        "You have a pending job that was saved. Trying to recover it...",
                        NotificationType.Information,
                        "WindowArea",
                        TimeSpan.FromSeconds(3)
                    );
                    
                    // Aguarda um pouco para o usuário ver a notificação
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    
                    // Tenta recuperar o job usando a propriedade estática
                    var connectorView = HomeView.ConnectorViewInstance;
                    if (connectorView != null)
                    {
                        bool recovered = await connectorView.TryRecoverPendingJob();
                        
                        if (recovered)
                        {
                            _notificationManager.Show(
                                "Job Recovered",
                                "Your pending job has been recovered. You can now try to finish it.",
                                NotificationType.Success,
                                "WindowArea"
                            );
                        }
                        else
                        {
                            _notificationManager.Show(
                                "Recovery Failed",
                                "Could not recover the pending job. Please try again or contact support.",
                                NotificationType.Warning,
                                "WindowArea"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Failed to check pending job data", ex);
            }
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

        private async Task LoadUserJobList()
        {
            try
            {
                await _jobService.GetAllUserJobs();
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
