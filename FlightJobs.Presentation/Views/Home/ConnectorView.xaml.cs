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
using FlightJobsDesktop.Const;
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

        private static CurrentJobViewModel _currentJob = new CurrentJobViewModel();
        private static CurrentJobDataWindow _sideJobWindow; // TODO test remove static
        private TouchdownWindow _sliderTouchdownWindow;
        private IList<string> _resultMessages;

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
                    _currentJob.SliderTopTitle = "Job started";
                    if (_sideJobWindow != null)
                    {
                        _sideJobWindow.StartedIcon.Visibility = Visibility.Visible;
                        _sideJobWindow.GridLanding.Visibility = Visibility.Collapsed;
                        _sideJobWindow.GridResults.Visibility = Visibility.Collapsed;
                        _sideJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            _sideJobWindow.ToggleSlider(true, 8);
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
                _currentJob.SliderMessage = ex.Message.Replace("\"", "");
                if (_sideJobWindow != null) _sideJobWindow.GridMessage.Visibility = Visibility.Visible;
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
                    _currentJob.SliderTopTitle = "Job finished";
                    if (_sideJobWindow != null)
                    {
                        _sideJobWindow.StartedIcon.Visibility = Visibility.Hidden;
                        _sideJobWindow.GridSimData.Visibility = Visibility.Collapsed;
                        _sideJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        _sideJobWindow.GridLanding.Visibility = Visibility.Visible;
                        _sideJobWindow.GridResults.Visibility = Visibility.Visible;

                        if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            _sideJobWindow.ToggleSlider(true, 10);
                    }
                    SetFinishJobInfo();
                    var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                    AppProperties.UserJobs.Remove(currentJob);
                    await LoadUserJobData();
                    _isJobStarted = false;
                    _stopCheckJobFinish = false;
                    _stopCheckJobStart = false;
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
                _currentJob.SliderMessage = ex.Message.Replace("\"", "");
                _sideJobWindow.GridMessage.Visibility = Visibility.Visible;
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
            _currentJob.JobSummary = $"Congratulations! Your Job from {_currentJob.DepartureICAO} to {_currentJob.ArrivalICAO} is finalized and your gain for that was: F${_currentJob.Pay}";
        }

        private void ChecktakeoffData()
        {
            try
            {
                if (FlightJobsConnectSim.TakeoffDataCaptured &&
                    _currentJob != null && _currentJob.PlaneSimData != null)
                {
                    var takeoffHelper = new TakeoffHelper(_currentJob.PlaneSimData.HeadingTrue,
                                                              _currentJob.PlaneSimData.TakeoffLatitude,
                                                              _currentJob.PlaneSimData.TakeoffLongitude);

                    var takeoffAirport = takeoffHelper.GetTakeoffJobAirport(_sqLiteDbContext, _currentJob.DepartureICAO);
                    if (takeoffAirport != null)
                    {
                        var rwy = takeoffHelper.GetTakeoffRwy(takeoffAirport.Runways);

                        _currentJob.PlaneSimData.TakeoffCenterDerivation = takeoffHelper.GetTakeoffCenterLineDistance(rwy);
                    }
                    else
                    {
                        _currentJob.SliderMessage = "Wrong departure airport!!!";
                        _sideJobWindow.GridMessage.Visibility = Visibility.Visible;
                    }

                    FlightJobsConnectSim.TakeoffDataCaptured = false;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }

        private void CheckShowLanding()
        {
            try
            {
                if (FlightJobsConnectSim.LandingDataCaptured && AppProperties.UserSettings.LocalSettings.ShowLandingData &&
                    _currentJob != null && _currentJob.PlaneSimData != null)
                {

                    if (_sliderTouchdownWindow == null)
                    {
                        _sliderTouchdownWindow = new TouchdownWindow();
                        _sliderTouchdownWindow.Show();
                    }
                    var landingHelper = new LandingHelper(_currentJob.PlaneSimData.HeadingTrue,
                                                              _currentJob.PlaneSimData.TouchdownLatitude,
                                                              _currentJob.PlaneSimData.TouchdownLongitude);

                    var landAirport = landingHelper.GetLandingJobAirport(_sqLiteDbContext, _currentJob.ArrivalICAO, _currentJob.AlternativeICAO);
                    if (landAirport != null)
                    {
                        var rwy = landingHelper.GetLandingRwy(landAirport.Runways);

                        _currentJob.PlaneSimData.TouchdownRunwayLength = landingHelper.GetTouchdownRunwayLength(rwy);
                        _currentJob.PlaneSimData.TouchdownCenterDerivation = landingHelper.GetTouchdownCenterLineDistance(rwy);
                        _currentJob.PlaneSimData.TouchdownThresholdDistance = landingHelper.GetTouchdownThresholdDistance(rwy);
                        _currentJob.PlaneSimData.TouchdownRunwayDesignator = rwy.Name;
                        _currentJob.PlaneSimData.ColorResultTouchdownWindAngle = WindAngleResult.GetColor(_currentJob.PlaneSimData.TouchdownWindAngle);
                        CalculateScoreData();
                        SetupResultsMessags();

                        _sliderTouchdownWindow.ToggleSlider(true, 15);
                        _sliderTouchdownWindow.DataContext = _currentJob;
                    }
                    else
                    {
                        _currentJob.SliderMessage = "Wrong airport to land!!!";
                        _sideJobWindow.GridMessage.Visibility = Visibility.Visible;
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

        private IList<string> SetupResultsMessags()
        {
            _resultMessages =  new List<string>();

            _currentJob.MsgResults.TouchdownScore =
                string.Format(MessagesConst.MSG_TOUCHDOWN, _currentJob.PlaneSimData.TouchdownFpm);

            _currentJob.MsgResults.GForceScore =
                string.Format(MessagesConst.MSG_GFORCE, _currentJob.PlaneSimData.TouchdownGForceText);

            _currentJob.MsgResults.BounceScore =
                string.Format(MessagesConst.MSG_BOUNCE, _currentJob.PlaneSimData.TouchdownBounceCount);

            _currentJob.MsgResults.LandingDerivationScore =
                string.Format(MessagesConst.MSG_LANDING_DERIVATION, _currentJob.PlaneSimData.TouchdownCenterDerivation);

            _currentJob.MsgResults.LandingDistanceScore =
                string.Format(MessagesConst.MSG_LANDING_DISTANCE, _currentJob.PlaneSimData.TouchdownThresholdDistance);

            _currentJob.MsgResults.TakeoffDerivationScore =
                string.Format(MessagesConst.MSG_TAKEOFF_DERIVATION, _currentJob.PlaneSimData.TakeoffCenterDerivation);

            _currentJob.MsgResults.UpwindLandingScore = MessagesConst.MSG_UPWIND_LANDING;
            _currentJob.MsgResults.BeaconLightsScore = MessagesConst.MSG_BEACON_LIGHTS;
            _currentJob.MsgResults.LandingLightsScore = MessagesConst.MSG_LANDING_LIGHTS;
            _currentJob.MsgResults.NavegationLightsScore = MessagesConst.MSG_NAVEGATION_LIGHTS;
            _currentJob.MsgResults.AltimeterScore = MessagesConst.MSG_ST_ALTIMETER;
            _currentJob.MsgResults.TotalScore = MessagesConst.MSG_TOTAL_SCORE;

            _resultMessages.Add(_currentJob.MsgResults.TouchdownScore + _currentJob.PlaneSimData.ScoreTouchdown);
            _resultMessages.Add(_currentJob.MsgResults.GForceScore + _currentJob.PlaneSimData.ScoreGForce);
            _resultMessages.Add(_currentJob.MsgResults.BounceScore + _currentJob.PlaneSimData.ScoreBounce);
            _resultMessages.Add(_currentJob.MsgResults.LandingDerivationScore + _currentJob.PlaneSimData.ScoreCenterDerivation);
            _resultMessages.Add(_currentJob.MsgResults.LandingDistanceScore + _currentJob.PlaneSimData.ScoreLandDistance);
            _resultMessages.Add(_currentJob.MsgResults.TakeoffDerivationScore + _currentJob.PlaneSimData.ScoreTakeoffCenterDerivation);
            _resultMessages.Add(_currentJob.MsgResults.UpwindLandingScore + _currentJob.PlaneSimData.ScoreWindAngle);
            _resultMessages.Add(_currentJob.MsgResults.BeaconLightsScore + _currentJob.PlaneSimData.ScoreLightBeaconOn);
            _resultMessages.Add(_currentJob.MsgResults.LandingLightsScore + _currentJob.PlaneSimData.ScoreLightLandingOn);
            _resultMessages.Add(_currentJob.MsgResults.NavegationLightsScore + _currentJob.PlaneSimData.ScoreLightNavigationOn);
            _resultMessages.Add(_currentJob.MsgResults.AltimeterScore + _currentJob.PlaneSimData.ScoreAltimeterSettings);
            _resultMessages.Add(_currentJob.MsgResults.TotalScore + _currentJob.Score);

            return _resultMessages;
        }

        private void CalculateScoreData()
        {
            if (_currentJob == null || _currentJob.PlaneSimData == null) return;

            var p = _currentJob.PlaneSimData;
            _currentJob.Score = (long)(_currentJob.Dist * 0.35) +
                                    p.ScoreBounce + p.ScoreGForce + p.ScoreCenterDerivation + p.ScoreTakeoffCenterDerivation +
                                    p.ScoreLandDistance + p.ScoreLightBeaconOn + p.ScoreLightLandingOn +
                                    p.ScoreLightNavigationOn + p.ScoreTouchdown + p.ScoreWindAngle;
            
            _currentJob.FlightResults.ResultBeaconLightVisibility = p.ScoreLightBeaconOn == 0 ? Visibility.Collapsed : Visibility.Visible;
            _currentJob.FlightResults.ResultLandingLightVisibility = p.ScoreLightLandingOn == 0 ? Visibility.Collapsed : Visibility.Visible;
            _currentJob.FlightResults.ResultNavigationLightVisibility = p.ScoreLightNavigationOn == 0 ? Visibility.Collapsed : Visibility.Visible;
            _currentJob.FlightResults.ResultAltimeterSettingVisibility = p.ScoreAltimeterSettings == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void CheckForStartJob()
        {
            try
            {
                if (!_isJobStarted && !_stopCheckJobStart && _currentJob != null && 
                    _currentJob.DepartureICAO != null && _currentJob.ArrivalICAO != null)
                {
                    var departureAirportInfo = _sqLiteDbContext.GetAirportByIcao(_currentJob.DepartureICAO);
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
                    !FlightJobsConnectSim.PlaneSimData.EngOneRunning && _currentJob != null &&
                    _currentJob.DepartureICAO != null && _currentJob.ArrivalICAO != null)
                {
                    var arrivalAirportInfo = _sqLiteDbContext.GetAirportByIcao(_currentJob.ArrivalICAO);
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
            if (simPayload >= (_currentJob.Payload + 100) || simPayload <= (_currentJob.Payload - 100))
            {
                _currentJob.PayloadLabelColor = Brushes.Orange;
            }
            else
            {
                _currentJob.PayloadLabelColor = Brushes.LightGreen;
            }
        }

        private void SetIsConnectedVisibility()
        {
            _currentJob.IsConnectedVisibility = FlightJobsConnectSim.CommonSimData.IsConnected ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ValidateAltimeterResults()
        {
            if (_currentJob == null || _currentJob.PlaneSimData == null) return;

            if (_currentJob.PlaneSimData.ScoreAltimeterSettings == 0)
                _currentJob.PlaneSimData.ScoreAltimeterSettings =
                    AltimeterResult.GetScore(_currentJob.PlaneSimData.CurrentAltitude, _currentJob.PlaneSimData.AltimeterInMillibars);
        }

        private void ValidateLightsResults()
        {
            if (_currentJob == null || _currentJob.PlaneSimData == null) return;

            if (_currentJob.PlaneSimData.ScoreLightBeaconOn == 0)
                _currentJob.PlaneSimData.ScoreLightBeaconOn =
                    BeaconLightsResult.GetScore(_currentJob.PlaneSimData.LightBeaconOn, _currentJob.PlaneSimData.EngOneRunning);

            if (_currentJob.PlaneSimData.ScoreLightLandingOn == 0)
                _currentJob.PlaneSimData.ScoreLightLandingOn =
                    LandingLightsResult.GetScore(_currentJob.PlaneSimData.LightLandingOn, _currentJob.PlaneSimData.OnGround, _currentJob.PlaneSimData.CurrentAltitude);

            if (_currentJob.PlaneSimData.ScoreLightNavigationOn == 0)
                _currentJob.PlaneSimData.ScoreLightNavigationOn =
                    NavigationLightsResult.GetScore(_currentJob.PlaneSimData.LightNavigationOn, _currentJob.PlaneSimData.EngOneRunning);
        }

        private void OnTickCheckSimData(object sender, EventArgs e)
        {
            if (FlightJobsConnectSim.CommonSimData.IsConnected)
            {
                if (FlightJobsConnectSim.PlaneSimData.OnGround)
                {
                    CheckForStartJob();
                    CheckForFinishJob();
                    CheckShowLanding();
                }
                else
                {
                    ChecktakeoffData();
                }

                SetCurrentPayloadColor();
                SetIsConnectedVisibility();
                ValidateLightsResults();
                ValidateAltimeterResults();

                ((TabItem)HomeView.TabHome.Items[1]).IsEnabled = !_isJobStarted;
            }
        }

        internal async Task LoadUserJobData()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");

            try
            {
                _currentJob = new CurrentJobViewModel();

                var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                if (currentJob != null)
                {
                    PanelNoJob.Visibility = Visibility.Collapsed;
                    PanelCurrentJob.Visibility = Visibility.Visible;

                    _currentJob = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, CurrentJobViewModel>(currentJob);
                    _currentJob.JobSummary = $"Setup aircraft departure on {_currentJob.DepartureICAO} then fly to {_currentJob.ArrivalICAO} with this total payload";
                    if (_sideJobWindow == null)
                    {
                        _sideJobWindow = new CurrentJobDataWindow(_currentJob);
                        _sideJobWindow.Show();
                    }
                    _sideJobWindow.GridSimData.Visibility = FlightJobsConnectSim.CommonSimData.IsConnected ? Visibility.Visible : Visibility.Collapsed;
                    _sideJobWindow.StartedIcon.Visibility = Visibility.Hidden;
                    _sideJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                    _sideJobWindow.GridLanding.Visibility = Visibility.Collapsed;
                    _sideJobWindow.GridResults.Visibility = Visibility.Collapsed;
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
                    _currentJob.LastJob = lastJobView;
                    _currentJob.FlightTime = _currentJob.LastJob.FlightTime;
                }

                // Reload Airline data
                await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                HomeView.SetEllipseAirlinesVIsibility();

                _currentJob.PlaneSimData = FlightJobsConnectSim.PlaneSimData;
                _currentJob.SimData = FlightJobsConnectSim.CommonSimData;
                DataContext = _currentJob;
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
