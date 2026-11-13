using System;
using System.Runtime.InteropServices;

namespace FlightJobs.MSFS2024.Navdata.Model
{
    public enum FacilityDefinitionEnum
    {
        AIRPORT = 1000,
        RUNWAY = 1001
    }

    public enum FacilityRequestEnum
    {
        AIRPORT_REQUEST = 2000,
        RUNWAY_REQUEST = 2001
    }

    /// <summary>
    /// Struct correspondente ao 'sAirport' do MSFS 2024 SDK Facility Data Definition
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SimAirportStruct
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

    /// <summary>
    /// Struct correspondente ao 'sRunway' do MSFS 2024 SDK Facility Data Definition
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SimRunwayStruct
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

    public static class RunwayDesignatorUtil
    {
        public static string FormatRunwayIdent(int number, int designator)
        {
            string num = number.ToString("D2");
            string des = "";
            switch (designator)
            {
                case 1: des = "L"; break;
                case 2: des = "R"; break;
                case 3: des = "C"; break;
                case 4: des = "W"; break;
                case 5: des = "A"; break;
                case 6: des = "B"; break;
            }
            return num + des;
        }
    }
}

