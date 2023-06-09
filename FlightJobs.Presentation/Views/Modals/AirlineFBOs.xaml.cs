using FlightJobs.Infrastructure;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using Notification.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para AirlineFBOs.xam
    /// </summary>
    public partial class AirlineFBOs : UserControl
    {
        private NotificationManager _notificationManager;

        public bool ShowHireNotification { get; set; }

        public AirlineFBOs()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
        }

        private void LoadFbosData()
        {
            var hiredFBOs = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                                        .Map<AirlineModel, HiredFBOsViewModel>(AppProperties.UserStatistics.Airline);

            DataContext = hiredFBOs;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadFbosData();
                if (ShowHireNotification)
                {
                    _notificationManager.Show("Success", "Congratulations your airline hire a new FBO", NotificationType.Success, "WindowAreaFOB");
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaFOB");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).DialogResult = false;
        }

        private void BtnShowHireFbo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Window)this.Parent).DialogResult = true;
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaFOB");
            }
            
        }
    }
}
