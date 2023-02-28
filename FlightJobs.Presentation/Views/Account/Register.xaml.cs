using AutoMapper;
using FlightJobsDesktop.ViewModels;
using System;
using System.Windows;

namespace FlightJobsDesktop.Views.Account
{
    /// <summary>
    /// Lógica interna para Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = (AspnetUserViewModel)DataContext;
                //var user = new AutoMapper.Mapper(_mapper).Map<AspnetUserViewModel, ApplicationUser>(viewModel);

                //var userSaved = _userAccessService.RegisterUserAsync(user, viewModel.PasswordConfirmed);
                //Console.WriteLine($"UserName: {userSaved.Id}");

                //if (userSaved.Exception != null)
                //{
                //    MessageBox.Show(userSaved.Exception.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
