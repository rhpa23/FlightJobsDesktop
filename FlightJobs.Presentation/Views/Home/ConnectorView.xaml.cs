using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Connect.MSFS.SDK.Model;
using FlightJobs.Connect.MSFS.SDK.Model.Results;
using FlightJobs.Domain.Navdata.Entities;
using FlightJobs.Domain.Navdata.Helpers;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Utils;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.SlidersWindows;
using log4net;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para ConnectorView.xam
    /// </summary>
    public partial class ConnectorView : UserControl
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ConnectorView));

        private NotificationManager _notificationManager;
        private IJobService _jobService;
        private IUserAccessService _userAccessService;
        private ISqLiteDbContext _sqLiteDbContext;

        private DispatcherTimer _timerCheckSimData = new DispatcherTimer();
        private bool _isJobStarted;
        private bool _stopCheckJobStart;
        private bool _stopCheckJobFinish;

        private static CurrentJobViewModel _currentJobView = new CurrentJobViewModel();
        private static CurrentJobDataWindow _sliderCurrentJobWindow; // TODO test remove static
        private TouchdownWindow _sliderTouchdownWindow;

        public ConnectorView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
            _userAccessService = MainWindow.UserServiceFactory.Create();
            _sqLiteDbContext = MainWindow.SqLiteContextFactory.Create();

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
                    _stopCheckJobStart = true;
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(FlightJobsConnectSim.PlaneSimData);
                    simStatus.UserId = AppProperties.UserLogin.UserId; 
                    var startJobResponseInfo = await _jobService.StartJob(simStatus);
                    _notificationManager.Show("Success", startJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    _currentJobView.SliderTopTitle = "Job started";
                    if (_sliderCurrentJobWindow != null)
                    {
                        _sliderCurrentJobWindow.StartedIcon.Visibility = Visibility.Visible;
                        _sliderCurrentJobWindow.GridLanding.Visibility = Visibility.Collapsed;
                        _sliderCurrentJobWindow.GridResults.Visibility = Visibility.Collapsed;
                        _sliderCurrentJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            _sliderCurrentJobWindow.ToggleSlider(true, 8);
                    }
                    btnFinishBorder.IsEnabled = true;
                    _isJobStarted = true;
                    _log.Info("Job started");
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
                _log.Error(ex);
                btnStartBorder.IsEnabled = true;
                btnFinishBorder.IsEnabled = false;
                _notificationManager.Show("Error", ex.Message.Replace("\"", ""), NotificationType.Error, "WindowArea");
                _currentJobView.SliderMessage = ex.Message.Replace("\"", "");
                if (_sliderCurrentJobWindow != null) _sliderCurrentJobWindow.GridMessage.Visibility = Visibility.Visible;
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
                    _stopCheckJobFinish = true;
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(FlightJobsConnectSim.PlaneSimData);
                    simStatus.UserId = AppProperties.UserLogin.UserId;
                    var finishJobResponseInfo = await _jobService.FinishJob(simStatus);
                    _notificationManager.Show("Success", finishJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    _currentJobView.SliderTopTitle = "Job finished";
                    if (_sliderCurrentJobWindow != null)
                    {
                        _sliderCurrentJobWindow.StartedIcon.Visibility = Visibility.Hidden;
                        _sliderCurrentJobWindow.GridSimData.Visibility = Visibility.Collapsed;
                        _sliderCurrentJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        _sliderCurrentJobWindow.GridLanding.Visibility = Visibility.Visible;
                        _sliderCurrentJobWindow.GridResults.Visibility = Visibility.Visible;

                        if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            _sliderCurrentJobWindow.ToggleSlider(true, 10);
                    }
                    SetFinishJobInfo();
                    var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                    AppProperties.UserJobs.Remove(currentJob);
                    await LoadUserJobData();
                    _isJobStarted = false;
                    _stopCheckJobFinish = false;
                    _stopCheckJobStart = false;
//TODO return Score                    _currentJobView.Score = finishJobResponseInfo.Score
//TODO return FlightTime                    _currentJobView.FlightTime = finishJobResponseInfo.FlightTime
                    _log.Info("Job finished");
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
                _log.Error(ex);
                btnStartBorder.IsEnabled = false;
                btnFinishBorder.IsEnabled = true;
                _notificationManager.Show("Error", ex.Message.Replace("\"", ""), NotificationType.Error, "WindowArea");
                _currentJobView.SliderMessage = ex.Message.Replace("\"", "");
                _sliderCurrentJobWindow.GridMessage.Visibility = Visibility.Visible;
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
            _currentJobView.JobSummary = $"Congratulations! Your Job from {_currentJobView.DepartureICAO} to {_currentJobView.ArrivalICAO} is finalized and your gain for that was: F${_currentJobView.Pay}";
        }

        private void CheckShowLanding()
        {
            try
            {
                if (FlightJobsConnectSim.LandingDataCaptured && AppProperties.UserSettings.LocalSettings.ShowLandingData &&
                    _currentJobView != null && _currentJobView.PlaneSimData != null)
                {

                    if (_sliderTouchdownWindow == null)
                    {
                        _sliderTouchdownWindow = new TouchdownWindow();
                        _sliderTouchdownWindow.Show();
                    }
                    var landingHelper = new LandingHelper(_currentJobView.PlaneSimData.TouchdownHeadingTrue,
                                                              _currentJobView.PlaneSimData.TouchdownLatitude,
                                                              _currentJobView.PlaneSimData.TouchdownLongitude);

                    var landAirport = landingHelper.GetLandingJobAirport(_sqLiteDbContext, _currentJobView.ArrivalICAO, _currentJobView.AlternativeICAO);
                    if (landAirport != null)
                    {
                        var rwy = landingHelper.GetLandingRwy(landAirport.Runways);

                        _currentJobView.PlaneSimData.TouchdownRunwayLength = landingHelper.GetTouchdownRunwayLength(rwy);
                        _currentJobView.PlaneSimData.TouchdownCenterDerivation = landingHelper.GetTouchdownCenterLineDistance(rwy);
                        _currentJobView.PlaneSimData.TouchdownThresholdDistance = landingHelper.GetTouchdownThresholdDistance(rwy);
                        _currentJobView.PlaneSimData.TouchdownRunwayDesignator = rwy.Name;
                        _currentJobView.PlaneSimData.ColorResultTouchdownWindAngle = WindAngleResult.GetColor(_currentJobView.PlaneSimData.TouchdownWindAngle);
                        CalculateScoreData();

                        _sliderTouchdownWindow.ToggleSlider(true, 15);
                        _sliderTouchdownWindow.DataContext = _currentJobView;
                    }
                    else
                    {
                        _currentJobView.SliderMessage = "Wrong airport to land!!!";
                        _sliderCurrentJobWindow.GridMessage.Visibility = Visibility.Visible;
                    }

                    FlightJobsConnectSim.LandingDataCaptured = false;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }

        private void CalculateScoreData()
        {
            if (_currentJobView == null || _currentJobView.PlaneSimData == null) return;

            var p = _currentJobView.PlaneSimData;
            _currentJobView.Score = (long)(_currentJobView.Dist * 0.35) +
                                    p.ScoreBounce + p.ScoreGForce + p.ScoreCenterDerivation +
                                    p.ScoreLandDistance + p.ScoreLightBeaconOn + p.ScoreLightLandingOn +
                                    p.ScoreLightNavigationOn + p.ScoreTouchdown + p.ScoreWindAngle;
            
            _currentJobView.FlightResults.ResultBeaconLightVisibility = p.ScoreLightBeaconOn == 0 ? Visibility.Collapsed : Visibility.Visible;
            _currentJobView.FlightResults.ResultLandingLightVisibility = p.ScoreLightLandingOn == 0 ? Visibility.Collapsed : Visibility.Visible;
            _currentJobView.FlightResults.ResultNavigationLightVisibility = p.ScoreLightNavigationOn == 0 ? Visibility.Collapsed : Visibility.Visible;
            _currentJobView.FlightResults.ResultAltimeterSettingVisibility = p.ScoreAltimeterSettings == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void CheckForStartJob()
        {
            try
            {
                if (!_isJobStarted && !_stopCheckJobStart && _currentJobView != null && 
                    _currentJobView.DepartureICAO != null && _currentJobView.ArrivalICAO != null)
                {
                    var departureAirportInfo = _sqLiteDbContext.GetAirportByIcao(_currentJobView.DepartureICAO);
                    bool isCloseToDeparture = GeoCalculationsUtil.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                       FlightJobsConnectSim.PlaneSimData.Longitude,
                                                                                       departureAirportInfo.Laty, departureAirportInfo.Lonx);

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
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }

        private async void CheckForFinishJob()
        {
            try
            {
                if (_isJobStarted && !_stopCheckJobFinish && AppProperties.UserSettings.LocalSettings.AutoFinishJob && 
                    !FlightJobsConnectSim.PlaneSimData.EngOneRunning && _currentJobView != null &&
                    _currentJobView.DepartureICAO != null && _currentJobView.ArrivalICAO != null)
                {
                    var arrivalAirportInfo = _sqLiteDbContext.GetAirportByIcao(_currentJobView.ArrivalICAO);
                    bool isCloseToArrivel = GeoCalculationsUtil.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                     FlightJobsConnectSim.PlaneSimData.Longitude,
                                                                                     arrivalAirportInfo.Laty, arrivalAirportInfo.Lonx);
                    if (isCloseToArrivel)
                    {
                        if (!await FinishJob())
                        {
                            // TODO
                            // IN CASE OF FINISH FAIL
                            //Application.Current.MainWindow.WindowState = WindowState.Minimized;
                            //Application.Current.MainWindow.WindowState = WindowState.Normal;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
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
                _currentJobView.IsConnectedVisibility = Visibility.Collapsed;
            }
        }

        private void ValidateAltimeterResults()
        {
            if (_currentJobView == null || _currentJobView.PlaneSimData == null) return;

            if (_currentJobView.PlaneSimData.ScoreAltimeterSettings == 0)
                _currentJobView.PlaneSimData.ScoreAltimeterSettings =
                    AltimeterResult.GetScore(_currentJobView.PlaneSimData.CurrentAltitude, _currentJobView.PlaneSimData.AltimeterInMillibars);
        }

        private void ValidateLightsResults()
        {
            if (_currentJobView == null || _currentJobView.PlaneSimData == null) return;

            if (_currentJobView.PlaneSimData.ScoreLightBeaconOn == 0)
                _currentJobView.PlaneSimData.ScoreLightBeaconOn =
                    BeaconLightsResult.GetScore(_currentJobView.PlaneSimData.LightBeaconOn, _currentJobView.PlaneSimData.EngOneRunning);

            if (_currentJobView.PlaneSimData.ScoreLightLandingOn == 0)
                _currentJobView.PlaneSimData.ScoreLightLandingOn =
                    LandingLightsResult.GetScore(_currentJobView.PlaneSimData.LightLandingOn, _currentJobView.PlaneSimData.OnGround, _currentJobView.PlaneSimData.CurrentAltitude);

            if (_currentJobView.PlaneSimData.ScoreLightNavigationOn == 0)
                _currentJobView.PlaneSimData.ScoreLightNavigationOn =
                    NavigationLightsResult.GetScore(_currentJobView.PlaneSimData.LightNavigationOn, _currentJobView.PlaneSimData.EngOneRunning);
        }

        private void OnTickCheckSimData(object sender, EventArgs e)
        {
            if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
            {
                CheckForStartJob();
                CheckForFinishJob();
                CheckShowLanding();
            }

            SetCurrentPayloadColor();
            SetIsConnectedVisibility();
            ValidateLightsResults();
            ValidateAltimeterResults();

            ((TabItem)HomeView.TabHome.Items[1]).IsEnabled = !_isJobStarted;
        }

        internal async Task LoadUserJobData()
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
                    _currentJobView.JobSummary = $"Setup aircraft departure on {_currentJobView.DepartureICAO} then fly to {_currentJobView.ArrivalICAO} with this total payload";
                    if (_sliderCurrentJobWindow == null)
                    {
                        _sliderCurrentJobWindow = new CurrentJobDataWindow(_currentJobView);
                        _sliderCurrentJobWindow.Show();
                        _sliderCurrentJobWindow.StartedIcon.Visibility = Visibility.Hidden;
                        _sliderCurrentJobWindow.GridSimData.Visibility = Visibility.Visible;
                        _sliderCurrentJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        _sliderCurrentJobWindow.GridLanding.Visibility = Visibility.Collapsed;
                        _sliderCurrentJobWindow.GridResults.Visibility = Visibility.Collapsed;
                    }
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

                // Reload Airline data
                await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                HomeView.SetEllipseAirlinesVIsibility();

                _currentJobView.PlaneSimData = FlightJobsConnectSim.PlaneSimData;
                _currentJobView.SimData = FlightJobsConnectSim.CommonSimData;
                DataContext = _currentJobView;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserJobData();
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
