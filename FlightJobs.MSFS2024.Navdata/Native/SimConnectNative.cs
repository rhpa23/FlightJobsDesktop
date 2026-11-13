using System;
using System.Runtime.InteropServices;

namespace FlightJobs.MSFS2024.Navdata.Native
{
    public static class SimConnectNative
    {
        private const string DLL_NAME = "SimConnect.dll";

        public const uint SIMCONNECT_RECV_ID_EXCEPTION = 1;
        public const uint SIMCONNECT_RECV_ID_OPEN = 2;
        public const uint SIMCONNECT_RECV_ID_QUIT = 3;
        public const uint SIMCONNECT_RECV_ID_FACILITY_DATA = 28;
        public const uint SIMCONNECT_RECV_ID_FACILITY_DATA_END = 29;

        public const uint SIMCONNECT_FACILITY_DATA_AIRPORT = 0;
        public const uint SIMCONNECT_FACILITY_DATA_RUNWAY = 1;

        public const uint FACILITY_DATA_DEF_AIRPORT = 1000;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DispatchProc(IntPtr pData, uint cbData, IntPtr pContext);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int SimConnect_Open(out IntPtr phSimConnect, string szName, IntPtr hWnd, uint UserEventWin32, IntPtr hEventHandle, uint ConfigIndex);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern int SimConnect_Close(IntPtr hSimConnect);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int SimConnect_AddToFacilityDefinition(IntPtr hSimConnect, uint DefineID, string FieldName);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int SimConnect_RequestFacilityData(IntPtr hSimConnect, uint DefineID, uint RequestID, string ICAO, string Region = "");

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern int SimConnect_CallDispatch(IntPtr hSimConnect, DispatchProc pfcnDispatch, IntPtr pContext);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern int SimConnect_GetNextDispatch(IntPtr hSimConnect, out IntPtr ppData, out uint pcbData);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SIMCONNECT_RECV
    {
        public uint dwSize;
        public uint dwVersion;
        public uint dwID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SIMCONNECT_RECV_FACILITY_DATA
    {
        public uint dwSize;
        public uint dwVersion;
        public uint dwID;
        public uint UserRequestId;
        public uint UniqueRequestId;
        public uint ParentUniqueRequestId;
        public uint Type;
        public uint IsListItem;
        public uint ItemIndex;
        public uint ListSize;
        // Data payload segue imediatamente aqui
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SIMCONNECT_RECV_FACILITY_DATA_END
    {
        public uint dwSize;
        public uint dwVersion;
        public uint dwID;
        public uint RequestId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct NativeAirportData
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public float MagVar;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Name;
        public double TowerLatitude;
        public double TowerLongitude;
        public double TowerAltitude;
        public int NRunways;
        public int NFrequencies;
        public int NHelipads;
        public int NApproaches;
        public int NDepartures;
        public int NArrivals;
        public int NTaxiPoints;
        public int NTaxiParkings;
        public int NTaxiPaths;
        public int NTaxiNames;
        public int NJetways;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct NativeRunwayData
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public float Heading;
        public float Length;
        public float Width;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string PrimaryIlsIcao;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string PrimaryIlsRegion;
        public int PrimaryNumber;
        public int PrimaryDesignator;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string SecondaryIlsIcao;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string SecondaryIlsRegion;
        public int SecondaryNumber;
        public int SecondaryDesignator;
    }
}

