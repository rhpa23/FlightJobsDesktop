using ConnectorClientAPI;
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

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interação lógica para ConnectorView.xam
    /// </summary>
    public partial class ConnectorView : UserControl
    {
        private FlightJobsConnectorClientAPI _flightJobsConnectorClientAPI;
        private NotificationManager _notificationManager;

        public ConnectorView()
        {
            InitializeComponent();
            _flightJobsConnectorClientAPI = new FlightJobsConnectorClientAPI();
            _notificationManager = new NotificationManager();
        }

        private void BtnRemoveJob_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var loginData = Application.Current.Properties[AppConstants.KEY_LOGIN_DATA];
            if (loginData != null)
            {
                var userId = ((LoginResponseModel)loginData).UserId;
                await LoadUserJobList(userId);
                LoadUserJobData(userId);
            }
        }

        private void LoadUserJobData(string userId)
        {
            var jobs = Application.Current.Properties[AppConstants.KEY_JOBS_DATA];
            if (jobs != null)
            {
                var currentJob = ((List<JobModel>)jobs).FirstOrDefault(x => x.IsActivated);
                var lastJob = ((List<JobModel>)jobs).OrderBy(x => x.EndTime).FirstOrDefault(x => x.IsDone);
                if (currentJob != null)
                {

                    var jobView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, CurrentJobViewModel>(currentJob);
                    jobView.JobSummary = $"Setup departure for aircraft on {jobView.DepartureDesc} and the total payload to {jobView.PayloadComplete} then fly to {jobView.ArrivalDesc}.";
                    //if (lastJob != null) TODO: Get last job from API
                    //{
                    //    var lastJobView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, LastJobViewModel>(currentJob);
                    //    currentJobView.LastJob = lastJobView;
                    //}
                    DataContext = jobView;
                }
                else
                {
                    DataContext = new CurrentJobViewModel() { JobSummary = "You don't have any job active. Click on Manager to generate your next job." };
                }
            }
        }

        private async Task LoadUserJobList(string userId)
        {
            try
            {
                var jobs = await _flightJobsConnectorClientAPI.GetUserJobs(userId);
                if (jobs != null && Application.Current.Properties[AppConstants.KEY_JOBS_DATA] == null)
                {
                    Application.Current.Properties.Add(AppConstants.KEY_JOBS_DATA, jobs);
                }
            }
            catch
            {
                _notificationManager.Show("Error", "User jobs could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }
    }
}
