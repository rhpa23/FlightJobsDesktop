using FlightJobs.Connect.MSFS.SDK.Model;
using log4net;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FlightJobs.Connect.MSFS.SDK
{
    public class FlightJobsSimConnect
    {
        /// SimConnect object
        private SimConnect _simConnect = null;
        private bool _isConnected = false;

        private DispatcherTimer _oTimer = new DispatcherTimer();

        private SimDataModel _simDataModel;

        private static readonly ILog _log = LogManager.GetLogger(typeof(FlightJobsSimConnect));

        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;
        /// Window handle
        private IntPtr m_hWnd = new IntPtr(0);

        public FlightJobsSimConnect(SimDataModel simDataModel)
        {
            _simDataModel = simDataModel;
            _oTimer.Interval = new TimeSpan(0, 0, 0, 20, 0);
            _oTimer.Tick += new EventHandler(OnTick);
            _oTimer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_isConnected)
            {
                RequestSimVars();
            }
            else
            {
                Connect();
                RegisterSimVars();
            }
        }

        private void RequestSimVars()
        {
            try
            {
                if (!_isConnected)
                {
                    Connect();
                }

                _simConnect?.ReceiveMessage();
            }
            catch (Exception ex)
            {
                _log.Error($"RefreshSimVars failed.", ex);
                Disconnect();
            }
        }

        private void _simConnect_OnRecvEventFrame(SimConnect sender, SIMCONNECT_RECV_EVENT_FRAME data)
        {
            _simDataModel.FPS =  data.fFrameRate;
            _simDataModel.SimulationSpeed = data.fSimSpeed;
        }

        private void RegisterSimVars()
        {
            try
            {
                if (_simConnect != null)
                {
                    _simConnect.SubscribeToSystemEvent(SimVarsEnum.SIM_EVENTS, "Frame");

                    // IMPORTANT: register it with the simconnect managed wrapper marshaller
                    // if you skip this step, you will only receive a uint in the .dwData field.
                    _simConnect.RegisterDataDefineStruct<SimVarsStruct>(SimVarsEnum.SIM_EVENTS);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"RegisterSimVars failed.", ex);
            }
        }

        public void Connect()
        {
            try
            {
                if (!_isConnected)
                {
                    /// The constructor is similar to SimConnect_Open in the native API
                    _simConnect = new SimConnect("Simconnect", m_hWnd, WM_USER_SIMCONNECT, null, 0);

                    /// Listen to connect and quit msgs
                    _simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                    _simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                    /// Listen to exceptions
                    _simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                    /// Catch a simobject data request
                    _simConnect.OnRecvEventFrame += _simConnect_OnRecvEventFrame;
                    //_simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
                    //ReceiveSimConnectMessage();
                    _simConnect.ReceiveMessage();
                    _isConnected = true;
                    _oTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                }
            }
            catch (COMException)
            {
                // empty. Wait FS start.
            }
        }

        public void Disconnect()
        {
            try
            {
                _oTimer.Stop();

                if (_simConnect != null)
                {
                    /// Dispose serves the same purpose as SimConnect_Close()
                    _simConnect.Dispose();
                    _simConnect = null;
                }
                _isConnected = false;
            }
            catch (Exception ex)
            {
                _log.Error($"Disconnect failed.", ex);
            }
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            try
            {
                _oTimer.Start();
            }
            catch (Exception ex)
            {
                _log.Error($"SimConnect_OnRecvOpen failed.", ex);
            }
        }

        /// The case where the user closes game
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                _log.Error($"SimConnect_OnRecvQuit failed.", ex);
            }
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            _log.Error($"SimConnect_OnRecvException.{eException.ToString()}");
        }
    }

    public enum SimVarsEnum
    {
        SIM_EVENTS,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SimVarsStruct
    {
        public double FrameRate { get; set; }
        public double SimSpeed { get; set; }   
    };
}
