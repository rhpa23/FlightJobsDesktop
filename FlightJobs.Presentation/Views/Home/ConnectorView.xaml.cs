using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Connect.MSFS.SDK.Model;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.Utils;
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
using System.Windows.Threading;

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para ConnectorView.xam
    /// </summary>
    public partial class ConnectorView : UserControl
    {
        private NotificationManager _notificationManager;
        private IJobService _jobService;

        private static DispatcherTimer _timerCheckSimData;

        private static CurrentJobViewModel _currentJobView = new CurrentJobViewModel();

        public ConnectorView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();

            if (_timerCheckSimData == null)
            {
                _timerCheckSimData = new DispatcherTimer();
                _timerCheckSimData.Interval = new TimeSpan(0, 0, 0, 1, 0);
                _timerCheckSimData.Tick += new EventHandler(OnTickCheckSimData);
                _timerCheckSimData.Start();
            }
        }

        private async Task<bool> StartJob()
        {
            bool started = false;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            btnStartBorder.IsEnabled = false;
            btnFinishBorder.IsEnabled = false;
            //Thread.Sleep(500);
            try
            {
                if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
                {
                    //_stopCheckToStart = true;
                    //_simVarsModel.UserId = _userId;
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(FlightJobsConnectSim.PlaneSimData);
                    simStatus.UserId = AppProperties.UserLogin.UserId; 
                    var startJobResponseInfo = await _jobService.StartJob(simStatus);
                    var arrivalInfo = AirportDatabaseFile.FindAirportInfo(startJobResponseInfo.ArrivalICAO);
                    startJobResponseInfo.ArrivalLAT = arrivalInfo.Latitude;
                    startJobResponseInfo.ArrivalLON = arrivalInfo.Longitude;
                    _notificationManager.Show("Success", startJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    btnFinishBorder.IsEnabled = true;
                    started = true;
                }
                else
                {
                    _notificationManager.Show("Warning", "MSFS is not connected or the plane is not on ground", NotificationType.Warning, "WindowArea");
                    btnStartBorder.IsEnabled = true;
                    btnFinishBorder.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                btnStartBorder.IsEnabled = true;
                btnFinishBorder.IsEnabled = false;
                _notificationManager.Show("Error", ex.Message, NotificationType.Error, "WindowArea");
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
            return started;
        }


        private void CheckForStartJob()
        {
            var departureAirportInfo = AirportDatabaseFile.FindAirportInfo(_currentJobView.DepartureICAO);
            bool isCloseToDeparture = AirportDatabaseFile.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                               FlightJobsConnectSim.PlaneSimData.Longitude,
                                                                               departureAirportInfo.Latitude, departureAirportInfo.Longitude);

            if (isCloseToDeparture && AppProperties.UserSettings.LocalSettings.AutoStartJob && 
                                      FlightJobsConnectSim.PlaneSimData.EngOneRunning)
            {
                // TODO
                //if (!await StartJob())
                //{
                //    // IN CASE OF START FAIL
                //    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                //    Application.Current.MainWindow.WindowState = WindowState.Normal;
                //}
            }
        }

        private void SetCurrentPayloadColor()
        {
            var simPayload = AppProperties.UserSettings.WeightUnit.ToLower() == "kg" ? FlightJobsConnectSim.PlaneSimData.PayloadKilograms : FlightJobsConnectSim.PlaneSimData.PayloadPounds;

            // Check payload
            if (simPayload >= (_currentJobView.Payload + 100) || simPayload <= (_currentJobView.Payload - 100))
            {
                _currentJobView.PayloadLabelColor = Brushes.Orange;
            }
            else
            {
                _currentJobView.PayloadLabelColor = Brushes.LightGreen;
            }
        }

        private void SetIsConnectedVisibility()
        {
            if (FlightJobsConnectSim.CommonSimData.IsConnected)
            {
                _currentJobView.IsConnectedVisibility = Visibility.Visible;
            }
            else
            {
                _currentJobView.IsConnectedVisibility = Visibility.Hidden;
            }
        }

        private void OnTickCheckSimData(object sender, EventArgs e)
        {
            if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
            {
                CheckForStartJob();
                SetCurrentPayloadColor();
                //CheckWindowPopup();
            }
            SetIsConnectedVisibility();
        }

        internal async void LoadUserJobData()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");

            try
            {
                _currentJobView = new CurrentJobViewModel();

                var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                if (currentJob != null)
                {
                    PanelNoJob.Visibility = Visibility.Collapsed;
                    PanelCurrentJob.Visibility = Visibility.Visible;

                    _currentJobView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, CurrentJobViewModel>(currentJob);
                    _currentJobView.JobSummary = $"Setup aircraft departure on {_currentJobView.DepartureDesc} then fly to {_currentJobView.ArrivalDesc} with this total payload";
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
                    _currentJobView.LastJob = lastJobView;
                }

                _currentJobView.PlaneSimData = FlightJobsConnectSim.PlaneSimData;
                _currentJobView.SimData = FlightJobsConnectSim.CommonSimData;
                DataContext = _currentJobView;
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

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await StartJob();
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
