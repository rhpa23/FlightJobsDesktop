﻿using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using Notification.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para ConnectorView.xam
    /// </summary>
    public partial class ConnectorView : UserControl
    {
        private NotificationManager _notificationManager;
        private IJobService _jobService;

        public ConnectorView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();

            //LoadUserJobData();
        }

        private void BtnRemoveJob_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        internal async void LoadUserJobData()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");

            try
            {
                var currentJobView = new CurrentJobViewModel();

                var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                if (currentJob != null)
                {
                    PanelNoJob.Visibility = Visibility.Collapsed;
                    PanelCurrentJob.Visibility = Visibility.Visible;

                    currentJobView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, CurrentJobViewModel>(currentJob);
                    currentJobView.JobSummary = $"Setup departure for aircraft on {currentJobView.DepartureDesc} and the total payload to {currentJobView.PayloadComplete} then fly to {currentJobView.ArrivalDesc}.";
                }
                else
                {
                    PanelNoJob.Visibility = Visibility.Visible;
                    PanelCurrentJob.Visibility = Visibility.Collapsed;
                }

                var lastJob = await _jobService.GetLastUserJob(AppProperties.UserLogin.UserId);
                if (lastJob != null)
                {
                    var lastJobView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, LastJobViewModel>(lastJob);
                    currentJobView.LastJob = lastJobView;
                }

                DataContext = currentJobView;
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserJobData();
        }

        private void BtnShowAddJobs_Click(object sender, RoutedEventArgs e)
        {
            HomeView.TabHome.SelectedIndex = 1;
        }
    }
}