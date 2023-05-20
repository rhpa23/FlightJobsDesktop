using CTrue.FsConnect;
using FlightJobs.Connect.MSFS.SDK.Enum;
using FlightJobs.Connect.MSFS.SDK.Model;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        public double PlaneLatitude;
        public double PlaneLongitude;
        public double PlaneFuelWeightPounds;
        public double PlaneBrakeInicator;
        public double PlaneEngineOneIndicator;
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
        public static SimDataModel CommonSimData { get; set; } = new SimDataModel();
        public static PlaneModel PlaneSimData { get; set; } = new PlaneModel();

        private static FsConnect _fsConnect = new FsConnect();
        private static List<SimVar> _planeDataDefinitionList = new List<SimVar>();
        private static List<SimVar> _planePayloadDataDefinitionList = new List<SimVar>();
        private static List<SimVar> _simDefinitionList = new List<SimVar>();
        private static bool _isSafeToRead = true;

        DispatcherTimer _timerReadSimData = new DispatcherTimer();
        private DispatcherTimer _timerSimConnection = new DispatcherTimer();
        private BackgroundWorker _backgroundConnector = new BackgroundWorker();

        public void Initialize()
        {
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

            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE LATITUDE", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "PLANE LONGITUDE", Unit = "degree", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "FUEL TOTAL QUANTITY WEIGHT", Unit = "pounds", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "BRAKE INDICATOR", Unit = "number", DataType = SIMCONNECT_DATATYPE.FLOAT64 });
            _planeDataDefinitionList.Add(new SimVar() { Name = "ENG COMBUSTION:1", Unit = "number", DataType = SIMCONNECT_DATATYPE.FLOAT64 });

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
            _timerSimConnection.Start();
        }

        private void TimerReadSimData_Tick(object sender, EventArgs e)
        {
            //if (!ShowLanding)
            if (true)
            {
                try
                {
                    _fsConnect.RequestData(RequestDefinitionEnum.PlaneData, DefineEnum.PlaneDefineId);
                    _fsConnect.RequestData(RequestDefinitionEnum.PlanePayloadData, DefineEnum.PlanePayloadDefineId);
                    _fsConnect.RequestData(RequestDefinitionEnum.SimData, DefineEnum.SimDefinedId);
                }
                catch
                {
                }
            }
            else
            {
                //calculateLanding();
                //int BOUNCE_TIMER = Properties.Settings.Default.CloseAfterLanding * 1000;
                //timerBounce.Interval = new TimeSpan(0, 0, 0, 0, BOUNCE_TIMER);
                //timerBounce.Start();
            }
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
                catch 
                { /* Empyt because the sim is not running */ }
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
            }
            else
            {
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

                    //if (!ShowLanding)
                    //{
                    //    PlaneInfoResponse r = (PlaneInfoResponse)e.Data;
                    //    //ignore when noone is flying
                    //    if (r.ForwardSpeed < 4) //if less then 4kt, it's not a landing or out to menu
                    //    {
                    //        _isSafeToRead = true;
                    //        return;
                    //    }
                    //    if (r.OnGround)
                    //    {
                    //        Onground.Add(r);
                    //        if (Onground.Count > BUFFER_SIZE)
                    //        {
                    //            Onground.RemoveAt(0);
                    //            if (Inair.Count == BUFFER_SIZE)
                    //            {
                    //                ShowLanding = true;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Inair.Add(r);
                    //        if (Inair.Count > BUFFER_SIZE)
                    //        {
                    //            Inair.RemoveAt(0);
                    //        }
                    //        Onground.Clear();
                    //    }
                    //    if (Inair.Count > BUFFER_SIZE || Onground.Count > BUFFER_SIZE) //maximum 1 for race condition
                    //    {
                    //        Inair.Clear();
                    //        Onground.Clear();
                    //        throw new Exception("this baaad");
                    //    }
                    //    // POnGround = r.OnGround;
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            _isSafeToRead = true;
        }
    }
}
