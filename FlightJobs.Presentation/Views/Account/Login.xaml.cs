using FlightJobs.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FlightJobsDesktop.Views.Account
{
    /// <summary>
    /// Lógica interna para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private IUserAccessService _userAccessService;
        public Login(IUserAccessService userAccessService)
        {
            InitializeComponent();
            _userAccessService = userAccessService;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            //new Register(_userAccessService).Show();
            this.Close();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            new ForgotPassword().Show();
            this.Close();
        }
    }
}
