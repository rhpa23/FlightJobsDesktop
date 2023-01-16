using AutoMapper;
using FlightJobs.Domain.Entities;
using FlightJobs.Infrastructure.Persistence;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobsDesktop.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace FlightJobsDesktop.Views.Account
{
    /// <summary>
    /// Lógica interna para Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        private IUserAccessService _userAccessService;
        private MapperConfiguration _mapper;
        public Register(IUserAccessService userAccessService, MapperConfiguration mapper)
        {
            InitializeComponent();
            _userAccessService = userAccessService;
            _mapper= mapper;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = (AspnetUserViewModel)DataContext;
                var user = new AutoMapper.Mapper(_mapper).Map<AspnetUserViewModel, ApplicationUser>(viewModel);

                var userSaved = _userAccessService.RegisterUserAsync(user, viewModel.PasswordConfirmed);
                //Console.WriteLine($"UserName: {userSaved.Id}");

                if (userSaved.Exception != null)
                {
                    MessageBox.Show(userSaved.Exception.InnerException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
