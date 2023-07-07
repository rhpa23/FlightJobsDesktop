using AutoMapper;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ValidationRules;
using FlightJobsDesktop.ViewModels;
using ModernWpf;
using Notification.Wpf;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FlightJobsDesktop.Views.Account
{
    /// <summary>
    /// Lógica interna para Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        private IUserAccessService _userAccessService;
        private NotificationManager _notificationManager;
        private AspnetUserViewModel _aspnetUser;
        private int _loadingCount;

        public Register(IAbstractFactory<IUserAccessService> factoryUser)
        {
            InitializeComponent();

            _userAccessService = factoryUser.Create();
            _notificationManager = new NotificationManager();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;
            ShowLoading();
            try
            {
                _aspnetUser = (AspnetUserViewModel)DataContext;
                var userModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<AspnetUserViewModel, UserRegisterModel>(_aspnetUser);
                await _userAccessService.UserRegister(userModel);
                HideLoading();
                _notificationManager.Show("Success", $"Welcome capitan {userModel.Name}.", NotificationType.Success, "WindowArea");
                _notificationManager.Show("Success", $"Verify your email for account activation", NotificationType.Success, "WindowArea");
                await Task.Delay(4000);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", ex.Message, NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
                btnRegister.IsEnabled = true;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            btnRegister.IsEnabled = EmailValidationRule.IsValidEmail(txbEmail.Text) &&
                                    MinimumCharacterRule.IsValid(txbNickName.Text, 6) &&
                                    MinimumCharacterRule.IsValid(txbPassword.Password, 6) &&
                                    MinimumCharacterRule.IsValid(txbPasswordConfirm.Password, 6) &&
                                    Regex.IsMatch(txbPassword.Password, @"\d") && 
                                    txbPassword.Password == txbPasswordConfirm.Password;
        }
    }
}
