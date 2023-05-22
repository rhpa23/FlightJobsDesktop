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

        private DispatcherTimer _timerCheckSimData = new DispatcherTimer();
        private bool _isJobStarted;
        private bool _isJobFinished;

        private static CurrentJobViewModel _currentJobView = new CurrentJobViewModel();

        public ConnectorView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();

            _timerCheckSimData = new DispatcherTimer();
            _timerCheckSimData.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timerCheckSimData.Tick += new EventHandler(OnTickCheckSimData);
            _timerCheckSimData.Start();
        }

        private async Task<bool> StartJob()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            btnStartBorder.IsEnabled = false;
            btnFinishBorder.IsEnabled = false;
            try
            {
                if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
                {
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(FlightJobsConnectSim.PlaneSimData);
                    simStatus.UserId = AppProperties.UserLogin.UserId; 
                    var startJobResponseInfo = await _jobService.StartJob(simStatus);
                    _notificationManager.Show("Success", startJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    btnFinishBorder.IsEnabled = true;
                    _isJobStarted = true;
                    return true;
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
                progress.Dispose();
            }
            return false;
        }

        private async Task<bool> FinishJob()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            btnStartBorder.IsEnabled = false;
            btnFinishBorder.IsEnabled = false;

            try
            {
                if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
                {
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(FlightJobsConnectSim.PlaneSimData);
                    simStatus.UserId = AppProperties.UserLogin.UserId;
                    var finishJobResponseInfo = await _jobService.FinishJob(simStatus);
                    _notificationManager.Show("Success", finishJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    SetFinishJobInfo();
                    LoadUserJobData();
                    // TODO Verify: await LoadJobListDataGrid();
                    _isJobFinished = true;
                    _isJobStarted = false;
                    return true;
                }
                else
                {
                    _notificationManager.Show("Warning", "MSFS is not connected or the plane is not on ground", NotificationType.Warning, "WindowArea");
                    btnStartBorder.IsEnabled = false;
                    btnFinishBorder.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                btnStartBorder.IsEnabled = false;
                btnFinishBorder.IsEnabled = true;
                _notificationManager.Show("Error", ex.Message, NotificationType.Error, "WindowArea");
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                progress.Dispose();
            }
            return false;
        }

        private void SetFinishJobInfo()
        {
            _currentJobView.JobSummary = $"Congratulations! Your Job from {_currentJobView.DepartureDesc} to {_currentJobView.ArrivalDesc} is finalized and your gain for that was: F${_currentJobView.Pay}";
            // TODO update Lastjob area
        }

        private async void CheckForStartJob()
        {
            if (!_isJobStarted)
            {
                var departureAirportInfo = AirportDatabaseFile.FindAirportInfo(_currentJobView.DepartureICAO);
                bool isCloseToDeparture = AirportDatabaseFile.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                   FlightJobsConnectSim.PlaneSimData.Longitude,
                                                                                   departureAirportInfo.Latitude, departureAirportInfo.Longitude);

                if (isCloseToDeparture && AppProperties.UserSettings.LocalSettings.AutoStartJob &&
                                          FlightJobsConnectSim.PlaneSimData.EngOneRunning)
                {
                    if (!await StartJob())
                    {
                        // TODO
                        //    // IN CASE OF START FAIL
                        //    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                        //    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    }
                }
            }
        }

        private async void CheckForFinishJob()
        {
            if (_isJobStarted && AppProperties.UserSettings.LocalSettings.AutoFinishJob && !FlightJobsConnectSim.PlaneSimData.EngOneRunning)
            {
                var arrivalAirportInfo = AirportDatabaseFile.FindAirportInfo(_currentJobView.ArrivalICAO);
                bool isCloseToArrivel = AirportDatabaseFile.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                 FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                 arrivalAirportInfo.Latitude, arrivalAirportInfo.Longitude);
                if (isCloseToArrivel)
                {
                    if (!_isJobFinished)
                    {
                        if (!await FinishJob())
                        {
                            // TODO
                            // IN CASE OF FINISH FAIL
                            Application.Current.MainWindow.WindowState = WindowState.Minimized;
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                        }
                    }
                }
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
                CheckForFinishJob();
            }

            SetCurrentPayloadColor();
            SetIsConnectedVisibility();

            ((TabItem)HomeView.TabHome.Items[1]).IsEnabled = !_isJobStarted;
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

        private async void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            await FinishJob();
        }
    }
}
