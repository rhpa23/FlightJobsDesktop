﻿using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Connect.MSFS.SDK.Model;
using FlightJobs.Connect.MSFS.SDK.Model.Results;
using FlightJobs.Domain.Navdata.Entities;
using FlightJobs.Domain.Navdata.Helpers;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Utils;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Common;
using FlightJobsDesktop.Const;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.SlidersWindows;
using log4net;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private BackgroundWorker _backgroundCheck = new BackgroundWorker();
        private static bool _isJobStarted;
        private static bool _stopCheckJobStart;
        private static bool _stopCheckJobFinish;
        private static bool _hasConnected;
        private static DateTime _jumpCheckStartTime = DateTime.Now.AddSeconds(5);
        private static DateTime _jumpCheckFinishTime = DateTime.Now.AddSeconds(5);
        private static DateTime _jumpValidationsTime = DateTime.Now.AddSeconds(5);
        private static DateTime _jumpFlightDataTime = DateTime.Now.AddSeconds(10);

        private static CurrentJobViewModel _currentJob = new CurrentJobViewModel();
        private static CurrentJobDataWindow _siderJobWindow;
        private static TouchdownWindow _sliderTouchdownWindow = new TouchdownWindow();
        private static IList<string> _resultMessages;

        public ConnectorView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
            _userAccessService = MainWindow.UserServiceFactory.Create();
            _sqLiteDbContext = MainWindow.SqLiteContextFactory.Create();

            _backgroundCheck.DoWork += _backgroundCheck_DoWork;

            _timerCheckSimData = new DispatcherTimer();
            _timerCheckSimData.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timerCheckSimData.Tick += new EventHandler(OnTickCheckSimData);
            _timerCheckSimData.Start();
        }

        private async Task<bool> StartJob()
        {
            MainWindow.ShowLoading();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _currentJob.StartIsEnable = false;
            _currentJob.FinishIsEnable = false;
            try
            {
                if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
                {
                    _stopCheckJobStart = true;
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(_currentJob.PlaneSimData);
                    if (simStatus == null) return false;
                    simStatus.UserId = AppProperties.UserLogin.UserId; 
                    var startJobResponseInfo = await _jobService.StartJob(simStatus);
                    _notificationManager.Show("Success", startJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    _currentJob.SliderTopTitle = "Job started";
                    if (_siderJobWindow != null)
                    {
                        _siderJobWindow.StartedIcon.Visibility = Visibility.Visible;
                        _siderJobWindow.GridLanding.Visibility = Visibility.Collapsed;
                        _siderJobWindow.GridResults.Visibility = Visibility.Collapsed;
                        _siderJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            _siderJobWindow.ToggleSlider(true, 8);
                    }
                    _isJobStarted = true;
                    EnableDisableNavegation(false);
                    _currentJob.StartIsEnable = !_isJobStarted;
                    _currentJob.FinishIsEnable = _isJobStarted;
                    FlightRecorderUtil.FlightRecorderList.Clear();
                    _log.Info("Job started");
                    return true;
                }
                else
                {
                    _notificationManager.Show("Warning", "MSFS is not connected or the plane is not on ground", NotificationType.Warning, "WindowArea");
                    _currentJob.StartIsEnable = true;
                    _currentJob.FinishIsEnable = false;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _currentJob.StartIsEnable = true;
                _currentJob.FinishIsEnable = false;
                _notificationManager.Show("Error", ex.Message.Replace("\"", ""), NotificationType.Error, "WindowArea");
                _currentJob.SliderMessage = ex.Message.Replace("\"", "");
                if (_siderJobWindow != null) _siderJobWindow.GridMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                MainWindow.HideLoading();
            }
            return false;
        }

        private async Task<bool> FinishJob()
        {
            MainWindow.ShowLoading();
            Mouse.OverrideCursor = Cursors.Wait;
            _currentJob.StartIsEnable = false;
            _currentJob.FinishIsEnable = false;

            try
            {
                if (FlightJobsConnectSim.CommonSimData.IsConnected && FlightJobsConnectSim.PlaneSimData.OnGround)
                {
                    _stopCheckJobFinish = true;
                    var simStatus = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<PlaneModel, DataModel>(_currentJob.PlaneSimData);
                    if (simStatus == null) return false;
                    simStatus.UserId = AppProperties.UserLogin.UserId;
                    simStatus.ResultMessages = _resultMessages;
                    simStatus.ResultScore = _currentJob.Score;
                    var finishJobResponseInfo = await _jobService.FinishJob(simStatus);
                    _notificationManager.Show("Success", finishJobResponseInfo.ResultMessage, NotificationType.Success, "WindowArea");
                    _currentJob.SliderTopTitle = "Job finished";
                    if (_siderJobWindow != null)
                    {
                        _siderJobWindow.StartedIcon.Visibility = Visibility.Hidden;
                        _siderJobWindow.GridSimData.Visibility = Visibility.Collapsed;
                        _siderJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                        _siderJobWindow.GridLanding.Visibility = Visibility.Visible;
                        _siderJobWindow.GridResults.Visibility = Visibility.Visible;

                        if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            _siderJobWindow.ToggleSlider(true, 10);
                    }
                    SetFinishJobInfo();
                    _isJobStarted = false;
                    var currentJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                    AppProperties.UserJobs.Remove(currentJob);
                    // Reload Airline data
                    await _userAccessService.LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);
                    await _userAccessService.LoadUserAirlineProperties();
                    await LoadUserJobData(false);
                    EnableDisableNavegation(true);
                    _currentJob.StartIsEnable = !_isJobStarted;
                    _currentJob.FinishIsEnable = _isJobStarted;
                    FlightRecorderUtil.SaveFlightRecorderFile(_currentJob);
                    _log.Info("Job finished");
                    return true;
                }
                else
                {
                    _notificationManager.Show("Warning", "MSFS is not connected or the plane is not on ground", NotificationType.Warning, "WindowArea");
                    _currentJob.StartIsEnable = false;
                    _currentJob.FinishIsEnable = true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _currentJob.StartIsEnable = false;
                _currentJob.FinishIsEnable = true;
                _notificationManager.Show("Error", ex.Message, NotificationType.Error, "WindowArea");
                _currentJob.SliderMessage = ex.Message;
                _siderJobWindow.GridMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                MainWindow.HideLoading();
            }
            return false;
        }

        private void SetFinishJobInfo()
        {
            _currentJob.JobSummary = $"Congratulations! Your Job from {_currentJob.DepartureICAO} to {_currentJob.ArrivalICAO} is finalized and your gain for that was: F${_currentJob.Pay}";
        }

        private void EnableDisableNavegation(bool isEnabled)
        {
            ((TabItem)HomeView.TabHome.Items[1]).IsEnabled = isEnabled;
            foreach (var menu in MainWindow.NavigationBar.MenuItems)
            {
                if (menu is NavigationViewItem)
                {
                    ((NavigationViewItem)menu).IsEnabled = isEnabled;
                }
            }
        }

        private void ChecktakeoffData()
        {
            try
            {
                if (FlightJobsConnectSim.TakeoffDataCaptured)
                {
                    Application.Current.Dispatcher.Invoke((Action) delegate
                    {
                        var takeoffHelper = new RunwayHelper(_currentJob.PlaneSimData.HeadingTrue,
                                                              _currentJob.PlaneSimData.TakeoffLatitude,
                                                              _currentJob.PlaneSimData.TakeoffLongitude);

                        var takeoffAirport = takeoffHelper.GetJobAirport(_sqLiteDbContext, _currentJob.DepartureICAO, null);
                        if (takeoffAirport != null)
                        {
                            var rwy = takeoffHelper.GetRunway(takeoffAirport.Runways);

                            _currentJob.PlaneSimData.TakeoffCenterDerivation = takeoffHelper.GetCenterLineDistance(rwy);

                            // add pecision to takeoff Flight recorder
                            var fRecorder = new FlightRecorderViewModel(FlightJobsConnectSim.PlaneSimData);
                            fRecorder.TimeUtc = DateTime.UtcNow;
                            fRecorder.OnGround = false;
                            fRecorder.FPS = FlightJobsConnectSim.CommonSimData.FPS;
                            lock (FlightRecorderUtil.FlightRecorderList)
                            {
                                FlightRecorderUtil.FlightRecorderList.Add(fRecorder);
                            }
                        }
                        else
                        {
                            _currentJob.SliderMessage = "Wrong departure airport!!!";
                            if (_siderJobWindow != null)
                                _siderJobWindow.GridMessage.Visibility = Visibility.Visible;
                        }

                        FlightJobsConnectSim.TakeoffDataCaptured = false;
                    });
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
                if (FlightJobsConnectSim.LandingDataCaptured)
                {
                    Application.Current.Dispatcher.Invoke((Action) delegate
                    {
                        var landingHelper = new RunwayHelper(_currentJob.PlaneSimData.HeadingTrue,
                                                                  _currentJob.PlaneSimData.TouchdownLatitude,
                                                                  _currentJob.PlaneSimData.TouchdownLongitude);

                        var landAirport = landingHelper.GetJobAirport(_sqLiteDbContext, _currentJob.ArrivalICAO, _currentJob.AlternativeICAO);
                        if (landAirport != null)
                        {
                            var rwy = landingHelper.GetRunway(landAirport.Runways);

                            _currentJob.PlaneSimData.TouchdownRunwayLength = landingHelper.GetRunwayLength(rwy);
                            _currentJob.PlaneSimData.TouchdownCenterDerivation = landingHelper.GetCenterLineDistance(rwy);
                            _currentJob.PlaneSimData.TouchdownThresholdDistance = landingHelper.GetTouchdownThresholdDistance(rwy);
                            _currentJob.PlaneSimData.TouchdownRunwayDesignator = rwy.Name;
                            _currentJob.PlaneSimData.ColorResultTouchdownWindAngle = WindAngleResult.GetColor(_currentJob.PlaneSimData.TouchdownWindAngle, _currentJob.PlaneSimData.TouchdownWindSpeed);
                            CalculateScoreData();
                            SetupResultsMessags();

                            if (AppProperties.UserSettings.LocalSettings.ShowLandingData)
                            {
                                _sliderTouchdownWindow.ToggleSlider(true, 15);
                            }                            
                            _sliderTouchdownWindow.DataContext = _currentJob;

                            // add pecision to landing Flight recorder
                            var fRecorder = new FlightRecorderViewModel(FlightJobsConnectSim.PlaneSimData);
                            fRecorder.TimeUtc = DateTime.UtcNow;
                            fRecorder.OnGround = true;
                            fRecorder.FPS = FlightJobsConnectSim.CommonSimData.FPS;

                            lock (FlightRecorderUtil.FlightRecorderList)
                            {
                                FlightRecorderUtil.FlightRecorderList.Add(fRecorder);
                            }
                        }
                        else
                        {
                            _currentJob.SliderMessage = "Wrong airport to land!!!";
                            _siderJobWindow.GridMessage.Visibility = Visibility.Visible;
                        }

                        FlightJobsConnectSim.LandingDataCaptured = false;
                    });
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

            // TITLES
            _currentJob.MsgResults.TouchdownScoreTitle =
                string.Format(MessagesConst.MSG_TOUCHDOWN_TITLE, _currentJob.PlaneSimData.TouchdownFpm);

            _currentJob.MsgResults.GForceScoreTitle =
                string.Format(MessagesConst.MSG_GFORCE_TITLE, _currentJob.PlaneSimData.TouchdownGForceText);

            _currentJob.MsgResults.BounceScoreTitle =
                string.Format(MessagesConst.MSG_BOUNCE_TITLE, _currentJob.PlaneSimData.TouchdownBounceCount);

            _currentJob.MsgResults.LandingDerivationScoreTitle =
                string.Format(MessagesConst.MSG_LANDING_DERIVATION_TITLE, _currentJob.PlaneSimData.TouchdownCenterDerivation);

            _currentJob.MsgResults.LandingDistanceScoreTitle =
                string.Format(MessagesConst.MSG_LANDING_DISTANCE_TITLE, _currentJob.PlaneSimData.TouchdownThresholdDistance);

            _currentJob.MsgResults.TakeoffDerivationScoreTitle =
                string.Format(MessagesConst.MSG_TAKEOFF_DERIVATION_TITLE, _currentJob.PlaneSimData.TakeoffCenterDerivation);

            _currentJob.MsgResults.UpwindLandingScoreTitle = MessagesConst.MSG_UPWIND_LANDING_TITLE;
            _currentJob.MsgResults.BeaconLightsScoreTitle = MessagesConst.MSG_BEACON_LIGHTS_TITLE;
            _currentJob.MsgResults.LandingLightsScoreTitle = MessagesConst.MSG_LANDING_LIGHTS_TITLE;
            _currentJob.MsgResults.NavegationLightsScoreTitle = MessagesConst.MSG_NAVEGATION_LIGHTS_TITLE;
            _currentJob.MsgResults.AltimeterScoreTitle = MessagesConst.MSG_ST_ALTIMETER_TITLE;
            _currentJob.MsgResults.TotalScoreTitle = MessagesConst.MSG_TOTAL_SCORE_TITLE;

            // SUB TITLES
            _currentJob.MsgResults.TouchdownScoreSubTitle = MessagesConst.MSG_TOUCHDOWN_SUB_TITLE;
            _currentJob.MsgResults.GForceScoreSubTitle = MessagesConst.MSG_GFORCE_SUB_TITLE;
            _currentJob.MsgResults.BounceScoreSubTitle = MessagesConst.MSG_BOUNCE_SUB_TITLE;
            _currentJob.MsgResults.LandingDerivationScoreSubTitle = MessagesConst.MSG_LANDING_DERIVATION_SUB_TITLE;
            _currentJob.MsgResults.LandingDistanceScoreSubTitle = MessagesConst.MSG_LANDING_DISTANCE_SUB_TITLE;
            _currentJob.MsgResults.TakeoffDerivationScoreSubTitle = MessagesConst.MSG_TAKEOFF_DERIVATION_SUB_TITLE;
            _currentJob.MsgResults.UpwindLandingScoreSubTitle = MessagesConst.MSG_UPWIND_LANDING_SUB_TITLE;
            _currentJob.MsgResults.BeaconLightsScoreSubTitle = MessagesConst.MSG_BEACON_LIGHTS_SUB_TITLE;
            _currentJob.MsgResults.LandingLightsScoreSubTitle = MessagesConst.MSG_LANDING_LIGHTS_SUB_TITLE;
            _currentJob.MsgResults.NavegationLightsScoreSubTitle = MessagesConst.MSG_NAVEGATION_LIGHTS_SUB_TITLE;
            _currentJob.MsgResults.AltimeterScoreSubTitle = MessagesConst.MSG_ST_ALTIMETER_SUB_TITLE;
            _currentJob.MsgResults.TotalScoreSubTitle = MessagesConst.MSG_TOTAL_SCORE_SUB_TITLE;

            // Result Messages List
            _resultMessages.Add(_currentJob.MsgResults.TouchdownScoreTitle + _currentJob.MsgResults.TouchdownScoreSubTitle + _currentJob.PlaneSimData.ScoreTouchdown);
            _resultMessages.Add(_currentJob.MsgResults.GForceScoreTitle + _currentJob.MsgResults.GForceScoreSubTitle + _currentJob.PlaneSimData.ScoreGForce);
            _resultMessages.Add(_currentJob.MsgResults.BounceScoreTitle + _currentJob.MsgResults.BounceScoreSubTitle + _currentJob.PlaneSimData.ScoreBounce);
            _resultMessages.Add(_currentJob.MsgResults.LandingDerivationScoreTitle + _currentJob.MsgResults.LandingDerivationScoreSubTitle + _currentJob.PlaneSimData.ScoreCenterDerivation);
            _resultMessages.Add(_currentJob.MsgResults.LandingDistanceScoreTitle + _currentJob.MsgResults.LandingDistanceScoreSubTitle + _currentJob.PlaneSimData.ScoreLandDistance);
            _resultMessages.Add(_currentJob.MsgResults.TakeoffDerivationScoreTitle + _currentJob.MsgResults.TakeoffDerivationScoreSubTitle + _currentJob.PlaneSimData.ScoreTakeoffCenterDerivation);
            _resultMessages.Add(_currentJob.MsgResults.UpwindLandingScoreSubTitle + _currentJob.PlaneSimData.ScoreWindAngle);
            _resultMessages.Add(_currentJob.MsgResults.BeaconLightsScoreSubTitle + _currentJob.PlaneSimData.ScoreLightBeaconOn);
            _resultMessages.Add(_currentJob.MsgResults.LandingLightsScoreSubTitle + _currentJob.PlaneSimData.ScoreLightLandingOn);
            _resultMessages.Add(_currentJob.MsgResults.NavegationLightsScoreSubTitle + _currentJob.PlaneSimData.ScoreLightNavigationOn);
            _resultMessages.Add(_currentJob.MsgResults.AltimeterScoreSubTitle + _currentJob.PlaneSimData.ScoreAltimeterSettings);
            _resultMessages.Add(_currentJob.MsgResults.TotalScoreTitle + _currentJob.Score);

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

        private void CheckForStartJob()
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)async delegate
                {
                    // your code

                    if (!_isJobStarted && !_stopCheckJobStart &&
                        _currentJob.DepartureICAO != null && _currentJob.ArrivalICAO != null)
                    {
                        bool isCloseToDeparture = GeoCalculationsUtil.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                           FlightJobsConnectSim.PlaneSimData.Longitude,
                                                                                           _currentJob.DepartureLatitude, _currentJob.DepartureLongitude);

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
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }

        private void CheckForFinishJob()
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)async delegate
                {
                    if (_isJobStarted && !_stopCheckJobFinish && AppProperties.UserSettings.LocalSettings.AutoFinishJob &&
                        !FlightJobsConnectSim.PlaneSimData.EngOneRunning &&
                        _currentJob.DepartureICAO != null && _currentJob.ArrivalICAO != null)
                    {
                        bool isCloseToArrivel = GeoCalculationsUtil.CheckClosestLocation(FlightJobsConnectSim.PlaneSimData.Latitude,
                                                                                         FlightJobsConnectSim.PlaneSimData.Longitude,
                                                                                         _currentJob.ArrivalLatitude, _currentJob.ArrivalLongitude);
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
                });
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
                    LandingLightsResult.GetScore(_currentJob.PlaneSimData.LightLandingOn, _currentJob.PlaneSimData.OnGround, _currentJob.PlaneSimData.EngOneRunning, _currentJob.PlaneSimData.CurrentAltitude);

            if (_currentJob.PlaneSimData.ScoreLightNavigationOn == 0)
                _currentJob.PlaneSimData.ScoreLightNavigationOn =
                    NavigationLightsResult.GetScore(_currentJob.PlaneSimData.LightNavigationOn, _currentJob.PlaneSimData.EngOneRunning);
        }

        private void OnTickCheckSimData(object sender, EventArgs e)
        {
            if (FlightJobsConnectSim.CommonSimData.IsConnected)
            {
                if (!_backgroundCheck.IsBusy)
                    _backgroundCheck.RunWorkerAsync();

                SetCurrentPayloadColor();
                //ValidateLightsResults();
                //ValidateAltimeterResults();

                _currentJob.IsConnectedColor = Brushes.White;
                _hasConnected = true;
            }
            else if (_hasConnected && AppProperties.UserSettings.LocalSettings.ExitWithFS)
            {
                App.Current.Shutdown();
            }
        }

        private void _backgroundCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            if (FlightJobsConnectSim.PlaneSimData.OnGround)
            {
                CheckShowLanding();

                if (DateTime.Now > _jumpCheckStartTime) // wait 3 seconds to check again
                {
                    _jumpCheckStartTime = DateTime.Now.AddSeconds(3);
                    CheckForStartJob();
                }

                if (DateTime.Now > _jumpCheckFinishTime) // wait 3 seconds to check again
                {
                    _jumpCheckFinishTime = DateTime.Now.AddSeconds(3);
                    CheckForFinishJob();
                }
            }
            else
            {
                ChecktakeoffData();
            }

            if (DateTime.Now > _jumpValidationsTime) // wait 5 seconds to check again
            {
                _jumpValidationsTime = DateTime.Now.AddSeconds(5);
                ValidateLightsResults();
                ValidateAltimeterResults();
            }

            if (DateTime.Now > _jumpFlightDataTime && _isJobStarted)
            {
                _jumpFlightDataTime = DateTime.Now.AddSeconds(10);
                var fRecorder = new FlightRecorderViewModel(FlightJobsConnectSim.PlaneSimData);
                fRecorder.TimeUtc = DateTime.UtcNow;
                fRecorder.FPS = FlightJobsConnectSim.CommonSimData.FPS;
                lock (FlightRecorderUtil.FlightRecorderList)
                {
                    FlightRecorderUtil.FlightRecorderList.Add(fRecorder);
                }
            }
        }

        private void LoadThumbImg(string filename)
        {
            try
            {
                if (_siderJobWindow == null) return;

                if (filename != null)
                {
                    filename = new FileInfo(filename).Name;

                    var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var imgPath = Path.Combine(localPath, $"FlightJobsDesktop\\images");

                    DirectoryInfo directoryInfo = new DirectoryInfo(imgPath);
                    _siderJobWindow.TxbCapacityName.Text = AppProperties.UserStatistics.CustomPlaneCapacity.CustomNameCapacity;

                    var imgLocalPath = $"{directoryInfo.FullName}\\{filename}";
                    if (File.Exists(imgLocalPath))
                    {
                        _siderJobWindow.ImgCapacity.Source = new BitmapImage(new Uri(imgLocalPath));
                    }
                }
                else
                {
                    _siderJobWindow.ImgCapacity.Source = new BitmapImage(new Uri("/img/background/default_thumb-capacity.jpg", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }

        internal async Task LoadUserJobData(bool closeSiderJob = true)
        {
            MainWindow.ShowLoading();
            try
            {
                var activeJob = AppProperties.UserJobs.FirstOrDefault(x => x.IsActivated);
                if (activeJob != null)
                {
                    PanelNoJob.Visibility = Visibility.Collapsed;
                    PanelCurrentJob.Visibility = Visibility.Visible;
                    
                    _currentJob = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, CurrentJobViewModel>(activeJob);
                    _currentJob.JobSummary = $"Setup aircraft departure on {_currentJob.DepartureICAO} then fly to {_currentJob.ArrivalICAO} with this total payload";
                    var arrivalEntity = _sqLiteDbContext.GetAirportByIcao(_currentJob.ArrivalICAO);
                    var departureEntity = _sqLiteDbContext.GetAirportByIcao(_currentJob.DepartureICAO);
                    var alternativeEntity = _sqLiteDbContext.GetAirportByIcao(_currentJob.AlternativeICAO);
                    _currentJob.DepartureLatitude = departureEntity.Laty;
                    _currentJob.DepartureLongitude = departureEntity.Lonx;
                    _currentJob.ArrivalLatitude = arrivalEntity.Laty;
                    _currentJob.ArrivalLongitude = arrivalEntity.Lonx;
                    if (alternativeEntity != null)
                    {
                        _currentJob.AlternativeLatitude = alternativeEntity.Laty;
                        _currentJob.AlternativeLongitude = alternativeEntity.Lonx;
                    }

                    if (_siderJobWindow != null) _siderJobWindow.Close();
                    
                    _siderJobWindow = new CurrentJobDataWindow(_currentJob);
                    if (!_siderJobWindow.IsLoaded)
                        _siderJobWindow.Show();
                    if (!_sliderTouchdownWindow.IsLoaded)
                        _sliderTouchdownWindow.Show();

                    _siderJobWindow.GridSimData.Visibility = Visibility.Visible;// FlightJobsConnectSim.CommonSimData.IsConnected ? Visibility.Visible : Visibility.Collapsed;
                    _siderJobWindow.StartedIcon.Visibility = Visibility.Hidden;
                    _siderJobWindow.GridMessage.Visibility = Visibility.Collapsed;
                    _siderJobWindow.GridLanding.Visibility = Visibility.Collapsed;
                    _siderJobWindow.GridResults.Visibility = Visibility.Collapsed;
                    _stopCheckJobFinish = false;
                    _stopCheckJobStart = false;
                    _currentJob.StartIsEnable = !_isJobStarted;
                    _currentJob.FinishIsEnable = _isJobStarted;
                    _log.Info("CurrentJob was set");
                }
                else
                {
                    PanelNoJob.Visibility = Visibility.Visible;
                    PanelCurrentJob.Visibility = Visibility.Collapsed;
                    if (closeSiderJob && _siderJobWindow != null) _siderJobWindow.Hide();
                    if (closeSiderJob && _sliderTouchdownWindow != null) _sliderTouchdownWindow.Hide();
                }

                var lastJob = await _jobService.GetLastUserJob(AppProperties.UserLogin.UserId);
                if (lastJob != null)
                {
                    _log.Info("LastJob was found and set");
                    var lastJobView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<JobModel, LastJobViewModel>(lastJob);
                    _currentJob.LastJob.DepartureICAO = lastJobView.DepartureICAO;
                    _currentJob.LastJob.ArrivalICAO = lastJobView.ArrivalICAO;
                    _currentJob.LastJob.Dist = lastJobView.Dist;
                    _currentJob.LastJob.FlightTime = lastJobView.FlightTime;
                    _currentJob.LastJob.ModelDescription = lastJobView.ModelDescription;
                    _currentJob.LastJob.EndTime = lastJobView.EndTime;
                    _currentJob.LastJob.PilotScore = lastJobView.PilotScore;
                }

                if (AppProperties.UserStatistics.CustomPlaneCapacity?.ImagePath != null)
                    LoadThumbImg(AppProperties.UserStatistics.CustomPlaneCapacity.ImagePath);

                // Moved to FinishJob                await _userAccessService.GetUserStatistics(AppProperties.UserLogin.UserId);
                HomeView.SetEllipseAirlinesVisibility();

                _currentJob.PlaneSimData = FlightJobsConnectSim.PlaneSimData;
                _currentJob.SimData = FlightJobsConnectSim.CommonSimData;
                DataContext = _currentJob;
                if (_siderJobWindow != null) 
                    _siderJobWindow.DataContext = _currentJob;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isJobStarted)
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
