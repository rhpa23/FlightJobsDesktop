using CTrue.FsConnect;
using FlightJobs.Connect.MSFS.SDK.Enum;
using FlightJobs.Connect.MSFS.SDK.Model;
using log4net;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace FlightJobs.Connect.MSFS.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneInfoResponseStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;
        public bool OnGround;
        public double WindLat;
        public double WindHead;
        public double AirspeedInd;
        public double GroundSpeed;
        public double LateralSpeed;
        public double ForwardSpeed;
        public double Gforce;
        public double LandingRate;
        public double PlaneTouchdownLatitude;
        public double PlaneTouchdownLongitude;
        public double PlaneHeadingTrue;
        public double PlaneLatitude;
        public double PlaneLongitude;
        public double PlaneFuelWeightPounds;
        public double PlaneBrakeInicator;
        public double PlaneEngineOneIndicator;
        public long IndicatedAltitude;
        public bool PlaneLightLandingOn;
        public bool PlaneLightBeaconOn;
        public bool PlaneLightNavigationOn;
        public double PlaneAltimeterMillibars;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlanePayloadResponseStruct
    {
        public double payload_station_weight_1;
        public double payload_station_weight_2;
        public double payload_station_weight_3;
        public double payload_station_weight_4;
        public double payload_station_weight_5;
        public double payload_station_weight_6;
        public double payload_station_weight_7;
        public double payload_station_weight_8;
        public double payload_station_weight_9;
        public double payload_station_weight_10;
        public double payload_station_weight_11;
        public double payload_station_weight_12;
        public double payload_station_weight_13;
        public double payload_station_weight_14;
        public double payload_station_weight_15;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SimInfoResponseStruct
    {
        public double SeaLevelPressure;
        public double WindDirection;
        public double WindVelocity;
        public double Temperature;
        public double Visibility;
    }

    public class FlightJobsConnectSim
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FlightJobsConnectSim));
        public static SimDataModel CommonSimData { get; set; } = new SimDataModel();
        public static PlaneModel PlaneSimData { get; set; } = new PlaneModel();
        public static bool ShowLanding { get; set; }
        public static bool ShowTakeoff { get; set; }
        public static bool LandingDataCaptured { get; set; }
        public static bool TakeoffDataCaptured { get; set; }

        private static FsConnect _fsConnect = new FsConnect();
        private static List<SimVar> _planeDataDefinitionList = new List<SimVar>();
        private static List<SimVar> _planePayloadDataDefinitionList = new List<SimVar>();
        private static List<SimVar> _simDefinitionList = new List<SimVar>();
        private static bool _isSafeToRead = true;
        private static List<PlaneInfoResponseStruct> _inAir = new List<PlaneInfoResponseStruct>();
        private static List<PlaneInfoResponseStruct> _onGround = new List<PlaneInfoResponseStruct>();
        private const int BUFFER = 2;
        private int _bounceCount = 0;

        private DispatcherTimer _timerTouchdownBounces = new DispatcherTimer();
        private DispatcherTimer _timerReadSimData = new DispatcherTimer();
        private DispatcherTimer _timerSimConnection = new DispatcherTimer();
        private BackgroundWorker _backgroundConnector = new BackgroundWorker();
        private FlightJobsSimConnect _flightJobsSimConnect;

        public void Initialize()
        {
            _log.Info("Initialize");
            _fsConnect.FsDataReceived += HandleReceivedSimData;
            _planeDataDefinitionList.Add(new SimVar() { Name = "TITLE", Unit = null, DataType = SIMCONNECT_DATATYPE.STRING256 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "SIM ON GROUND", Unit = "Bool", DataType = SIMCONNECT_DATATYPE.INT32 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "AIRCRAFT WIND X", Unit = "Knots", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "AIRCRAFT WIND Z", Unit = "Knots", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "AIRSPEED INDICATED", Unit = "Knots", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "GROUND VELOCITY", Unit = "Knots", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "VELOCITY BODY X", Unit = "Feet per second", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "VELOCITY BODY Z", Unit = "Feet per second", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "G FORCE", Unit = "GForce", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE TOUCHDOWN NORMAL VELOCITY", Unit = "Feet per second", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE TOUCHDOWN LATITUDE", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE TOUCHDOWN LONGITUDE", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE HEADING DEGREES MAGNETIC", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE LATITUDE", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE LONGITUDE", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "FUEL TOTAL QUANTITY WEIGHT", Unit = "pounds", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "BRAKE INDICATOR", Unit = "number", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "ENG COMBUSTION:1", Unit = "number", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "INDICATED ALTITUDE", Unit = "Feet", DataType = SIMCONNECT_DATATYPE.INT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "LIGHT LANDING ON", Unit = "Bool", DataType = SIMCONNECT_DATATYPE.INT32 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "LIGHT BEACON ON", Unit = "Bool", DataType = SIMCONNECT_DATATYPE.INT32 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "LIGHT NAV ON", Unit = "Bool", DataType = SIMCONNECT_DATATYPE.INT32 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "KOHLSMAN SETTING MB", Unit = "Millibars", DataType = SIMCONNECT_DATATYPE.FLOAT64 });

            _simDefinitionList.Add(new SimVar() { Name = "SEA LEVEL PRESSURE", Unit = "millibars", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _simDefinitionList.Add(new SimVar() { Name = "AMBIENT WIND DIRECTION", Unit = "degrees", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _simDefinitionList.Add(new SimVar() { Name = "AMBIENT WIND VELOCITY", Unit = "knots", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _simDefinitionList.Add(new SimVar() { Name = "AMBIENT TEMPERATURE", Unit = "celsius", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _simDefinitionList.Add(new SimVar() { Name = "AMBIENT VISIBILITY", Unit = "meters", DataType = SIMCONNECT_DATATYPE.FLOAT64 });

            for (int i = 1; i <= 15; i++)
            {
                _planePayloadDataDefinitionList.Add(new SimVar() { Name = $"PAYLOAD STATION WEIGHT:{i}", Unit = "pounds", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            }

            _timerReadSimData.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _timerReadSimData.Tick += TimerReadSimData_Tick;
            _timerSimConnection.Interval = new TimeSpan(0, 0, 0, 0, 300);
            _timerSimConnection.Tick += TimerConnection_Tick;
            _backgroundConnector.DoWork += BackgroundConnector_DoWork;
            _timerTouchdownBounces.Tick += TimerBounce_Tick;
            _timerSimConnection.Start();
        }

        private void SetTakeoffInfo()
        {
            try
            {
                PlaneSimData.TakeoffLatitude = _inAir.Last().PlaneLatitude;
                PlaneSimData.TakeoffLongitude = _inAir.Last().PlaneLongitude;
                PlaneSimData.HeadingTrue = _inAir.Last().PlaneHeadingTrue;
                TakeoffDataCaptured = true;

                ShowTakeoff = false;
            }
            catch (Exception ex) { /* ignore */ _log.Error(ex); }
        }

        private void SetTouchdownInfo()
        {
            try
            {
                if (_bounceCount == 0)
                {
                    double fpm = 60 * _onGround.ElementAt(0).LandingRate;
                    PlaneSimData.TouchdownFpm = Convert.ToInt32(-fpm);
                    PlaneSimData.TouchdownAirspeed = Convert.ToInt32(_inAir.Last().AirspeedInd);
                    PlaneSimData.TouchdownGroundspeed = Convert.ToInt32(_inAir.Last().GroundSpeed);
                    PlaneSimData.TouchdownCrosswind = Math.Round(_inAir.Last().WindLat, 2);
                    PlaneSimData.TouchdownHeadwind = Convert.ToInt32(_inAir.Last().WindHead);
                    PlaneSimData.TouchdownWindSpeed = Math.Round(CommonSimData.WindVelocityKnots, 0); 
                    PlaneSimData.TouchdownBounceCount = 0;
                    PlaneSimData.TouchdownGForce = Math.Round(_onGround.Max(x => x.Gforce), 2);
                    PlaneSimData.TouchdownLatitude = _onGround.Last().PlaneTouchdownLatitude;
                    PlaneSimData.TouchdownLongitude = _onGround.Last().PlaneTouchdownLongitude;
                    PlaneSimData.HeadingTrue = _onGround.Last().PlaneHeadingTrue;

                    LandingDataCaptured = true;

                    _bounceCount++;
                }
                else
                {
                    PlaneSimData.TouchdownBounceCount += 1;
                }
                _inAir.Clear();
                _onGround.Clear();
                ShowLanding = false;
            }
            catch (Exception ex) { /* ignore */ _log.Error(ex); }
        }

        private void TimerReadSimData_Tick(object sender, EventArgs e)
        {
            //if (!ShowLanding)
            try
            {
                if (_fsConnect.Connected)
                {
                    _fsConnect.RequestData(RequestDefinitionEnum.PlaneData, DefineEnum.PlaneDefineId);
                    _fsConnect.RequestData(RequestDefinitionEnum.PlanePayloadData, DefineEnum.PlanePayloadDefineId);
                    _fsConnect.RequestData(RequestDefinitionEnum.SimData, DefineEnum.SimDefinedId);
                }
            }
            catch (Exception ex)
            {
                // ignore
                _log.Error(ex);
            }

            if (ShowLanding)
            {
                SetTouchdownInfo();
                _timerTouchdownBounces.Interval = new TimeSpan(0, 0, 0, 5, 0);
                _timerTouchdownBounces.Start();
            }

            if (ShowTakeoff)
            {
                SetTakeoffInfo();
            }
        }

        private void TimerBounce_Tick(object sender, EventArgs e)
        {
            _bounceCount = 0;
            _timerTouchdownBounces.Stop();
        }

        private void BackgroundConnector_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_fsConnect.Connected)
            {
                try
                {
                    _fsConnect.Connect("FlightJobsConnectSim", "localhost", 500, SimConnectProtocol.Ipv4);
                    _fsConnect.RegisterDataDefinition<PlaneInfoResponseStruct>(RequestDefinitionEnum.PlaneData, _planeDataDefinitionList);
                    _fsConnect.RegisterDataDefinition<PlanePayloadResponseStruct>(RequestDefinitionEnum.PlanePayloadData, _planePayloadDataDefinitionList);
                    _fsConnect.RegisterDataDefinition<SimInfoResponseStruct>(RequestDefinitionEnum.SimData, _simDefinitionList);
                }
                catch (Exception ex)
                { /* Empyt because the sim is not running. PLEASE DON'T LOG HERE!!! */  }
            }
        }

        private void TimerConnection_Tick(object sender, EventArgs e)
        {
            if (!_backgroundConnector.IsBusy)
                _backgroundConnector.RunWorkerAsync();

            CommonSimData.IsConnected = _fsConnect.Connected;

            if (_fsConnect.Connected)
            {
                _timerReadSimData.Start();
                //notifyIcon.Icon = Properties.Resources.online;

                if (_flightJobsSimConnect == null)
                {
                    _flightJobsSimConnect = new FlightJobsSimConnect(CommonSimData);
                }
            }
            else
            {
                _timerReadSimData.Stop();
                //notifyIcon.Icon = Properties.Resources.offline;
            }
        }

        private static void HandleReceivedSimData(object sender, FsDataReceivedEventArgs e)
        {
            if (!_isSafeToRead)
            {
                return;
            }
            _isSafeToRead = false;
            try
            {
                if (e.RequestId == (uint)RequestDefinitionEnum.SimData)
                {
                    var sim = (SimInfoResponseStruct)e.Data[0];
                    CommonSimData.SeaLevelPressureMillibars = sim.SeaLevelPressure;
                    CommonSimData.TemperatureCelsius = sim.Temperature;
                    CommonSimData.VisibilityMeters = sim.Visibility;
                    CommonSimData.WindDirectionDegrees = sim.WindDirection;
                    CommonSimData.WindVelocityKnots = sim.WindVelocity;
                }
                if (e.RequestId == (uint)RequestDefinitionEnum.PlanePayloadData)
                {
                    var payload = (PlanePayloadResponseStruct)e.Data[0];
                    PlaneSimData.PayloadPounds = Math.Round(
                            payload.payload_station_weight_1 +
                            payload.payload_station_weight_2 +
                            payload.payload_station_weight_3 +
                            payload.payload_station_weight_4 +
                            payload.payload_station_weight_5 +
                            payload.payload_station_weight_6 +
                            payload.payload_station_weight_7 +
                            payload.payload_station_weight_8 +
                            payload.payload_station_weight_9 +
                            payload.payload_station_weight_10 +
                            payload.payload_station_weight_11 +
                            payload.payload_station_weight_12 +
                            payload.payload_station_weight_13 +
                            payload.payload_station_weight_14 +
                            payload.payload_station_weight_15, 0);

                    PlaneSimData.PayloadKilograms = UnitsConverterUtil.PoundsToKilograms(PlaneSimData.PayloadPounds);
                }
                if (e.RequestId == (uint)RequestDefinitionEnum.PlaneData)
                    {
                    var r = (PlaneInfoResponseStruct)e.Data[0];
                    PlaneSimData.Name = r.Name;
                    PlaneSimData.Latitude = r.PlaneLatitude;
                    PlaneSimData.Longitude = r.PlaneLongitude;
                    PlaneSimData.FuelWeightPounds = r.PlaneFuelWeightPounds;
                    PlaneSimData.FuelWeightKilograms = UnitsConverterUtil.PoundsToKilograms(r.PlaneFuelWeightPounds);
                    PlaneSimData.EngOneRunning  = Convert.ToBoolean(r.PlaneEngineOneIndicator);
                    PlaneSimData.OnGround  = r.OnGround;
                    PlaneSimData.CurrentAltitude = r.IndicatedAltitude;
                    PlaneSimData.LightLandingOn= r.PlaneLightLandingOn;
                    PlaneSimData.LightBeaconOn = r.PlaneLightBeaconOn;
                    PlaneSimData.LightNavigationOn = r.PlaneLightNavigationOn;
                    PlaneSimData.GroundSpeed = Convert.ToInt32(r.GroundSpeed);
                    PlaneSimData.AltimeterInMillibars = Convert.ToInt32(r.PlaneAltimeterMillibars);

                    if (!ShowLanding)
                    {
                        if (r.ForwardSpeed < 10) 
                        {
                            _isSafeToRead = true;
                            return;
                        }
                        if (r.OnGround)
                        {
                            _onGround.Add(r);
                            if (_onGround.Count > BUFFER)
                            {
                                _onGround.RemoveAt(0);
                                ShowLanding = _inAir.Count == BUFFER; // if already takeoff
                            }
                        }
                        else
                        {
                            _inAir.Add(r);
                            if (_inAir.Count > BUFFER)
                            {
                                _inAir.RemoveAt(0);
                            }

                            if (!ShowTakeoff)
                            {
                                ShowTakeoff = _onGround.Count > 0;
                            }

                            _onGround.Clear();
                        }
                        //if (_inAir.Count > BUFFER_SIZE || _onGround.Count > BUFFER_SIZE) //maximum 1 for race condition
                        //{
                        //    _inAir.Clear();
                        //    _onGround.Clear();
                        //    throw new Exception("this baaad");
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            _isSafeToRead = true;
        }
    }
}
