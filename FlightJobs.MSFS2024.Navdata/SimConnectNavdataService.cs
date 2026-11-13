using FlightJobs.Domain.Navdata.Entities;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.MSFS2024.Navdata.Model;
using FlightJobs.MSFS2024.Navdata.Native;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FlightJobs.MSFS2024.Navdata
{
    /// <summary>
    /// Implementação de IAirportDataProvider que obtém dados completos de aeroporto e pista
    /// direto do Microsoft Flight Simulator (MSFS 2024) através do SimConnect Facility Data.
    /// Os dados obtidos são salvos em cache local JSON para consultas instantâneas posteriores.
    /// </summary>
    public class SimConnectNavdataService : IAirportDataProvider
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(SimConnectNavdataService));

        private readonly ConcurrentDictionary<string, AirportEntity> _airportCache;
        private readonly string _cacheFilePath;
        private readonly object _saveLock = new object();
        private static int _requestCounter = 1;

        public SimConnectNavdataService()
        {
            _airportCache = new ConcurrentDictionary<string, AirportEntity>(StringComparer.OrdinalIgnoreCase);

            _cacheFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FlightJobsDesktop", "Cache", "AirportsCache.json");

            //LoadCacheFromDisk();
        }

        private void LoadCacheFromDisk()
        {
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    var json = File.ReadAllText(_cacheFilePath);
                    var cached = JsonConvert.DeserializeObject<Dictionary<string, AirportEntity>>(json);
                    if (cached != null)
                    {
                        foreach (var kvp in cached)
                        {
                            if (kvp.Value != null)
                            {
                                if (kvp.Value.Runways == null)
                                    kvp.Value.Runways = new List<RunwayEntity>();
                                _airportCache[kvp.Key] = kvp.Value;
                            }
                        }

                        _log.Info($"Loaded {_airportCache.Count} airports from cache ({_cacheFilePath}).");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to load airports cache from disk.", ex);
            }
        }

        private void SaveCacheToDisk()
        {
            try
            {
                lock (_saveLock)
                {
                    var dir = Path.GetDirectoryName(_cacheFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var json = JsonConvert.SerializeObject(_airportCache, Formatting.Indented);
                    File.WriteAllText(_cacheFilePath, json);
                }
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to save airports cache to disk.", ex);
            }
        }

        /// <inheritdoc/>
        public AirportEntity GetCachedAirport(string icao)
        {
            if (string.IsNullOrWhiteSpace(icao))
                return null;

            _airportCache.TryGetValue(icao.Trim().ToUpperInvariant(), out var airport);
            return airport;
        }

        /// <inheritdoc/>
        public async Task<AirportEntity> GetAirportAsync(string icao)
        {
            if (string.IsNullOrWhiteSpace(icao))
                return null;

            icao = icao.Trim().ToUpperInvariant();

            // Retorna do cache se já tiver runways carregadas
            if (_airportCache.TryGetValue(icao, out var cached) && cached != null && cached.Runways != null && cached.Runways.Count > 0)
                return cached;

            var airport = await FetchFromSimConnectNativeAsync(icao);
            if (airport != null)
            {
                _airportCache[icao] = airport;
                SaveCacheToDisk();
                return airport;
            }

            // Se falhou ao buscar via SimConnect (ex: sim fechado), retorna o que estiver em cache se houver
            if (cached != null)
                return cached;

            return null;
        }

        /// <inheritdoc/>
        public async Task<IList<RunwayEntity>> GetRunwaysAsync(string icao)
        {
            var airport = await GetAirportAsync(icao);
            return airport?.Runways ?? new List<RunwayEntity>();
        }

        /// <inheritdoc/>
        public async Task<bool> LoadFlightDataAsync(string departureIcao, string arrivalIcao, string alternativeIcao = null)
        {
            if (string.IsNullOrWhiteSpace(departureIcao) || string.IsNullOrWhiteSpace(arrivalIcao))
                return false;

            var depTask = GetAirportAsync(departureIcao);
            var arrTask = GetAirportAsync(arrivalIcao);

            Task<AirportEntity> altTask = null;
            if (!string.IsNullOrWhiteSpace(alternativeIcao))
                altTask = GetAirportAsync(alternativeIcao);

            if (altTask != null)
                await Task.WhenAll(depTask, arrTask, altTask);
            else
                await Task.WhenAll(depTask, arrTask);

            var dep = await depTask;
            var arr = await arrTask;

            bool success = dep != null && arr != null && dep.Runways.Count > 0 && arr.Runways.Count > 0;
            if (success)
                _log.Info($"Navdata loaded successfully: DEP={dep.Ident} ({dep.Runways.Count} runways), ARR={arr.Ident} ({arr.Runways.Count} runways)");
            else
                _log.Warn($"Navdata load incomplete: DEP={dep?.Ident ?? "NULL"} (rwy: {dep?.Runways?.Count ?? 0}), ARR={arr?.Ident ?? "NULL"} (rwy: {arr?.Runways?.Count ?? 0})");

            return success;
        }

        /// <summary>
        /// Consulta o MSFS 2024 via P/Invoke do SimConnect nativo para obter dados completos do aeroporto e suas pistas.
        /// </summary>
        private async Task<AirportEntity> FetchFromSimConnectNativeAsync(string icao)
        {
            return await Task.Run(() =>
            {
                IntPtr hSimConnect = IntPtr.Zero;
                try
                {
                    int hr = SimConnectNative.SimConnect_Open(out hSimConnect, "FlightJobsNavdataNative", IntPtr.Zero, 0, IntPtr.Zero, 0);
                    if (hr != 0 || hSimConnect == IntPtr.Zero)
                    {
                        _log.Info($"SimConnect not available for airport {icao} (MSFS not running). hr={hr}");
                        return null;
                    }

                    // Registra a definição de Facility Data para AIRPORT e RUNWAY
                    uint defId = SimConnectNative.FACILITY_DATA_DEF_AIRPORT;
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "OPEN AIRPORT");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "LATITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "LONGITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "ALTITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "MAGVAR");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "NAME");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "TOWER_LATITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "TOWER_LONGITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "TOWER_ALTITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "N_RUNWAYS");

                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "OPEN RUNWAY");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "LATITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "LONGITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "ALTITUDE");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "HEADING");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "LENGTH");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "WIDTH");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "PRIMARY_ILS_ICAO");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "PRIMARY_ILS_REGION");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "PRIMARY_NUMBER");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "PRIMARY_DESIGNATOR");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "SECONDARY_ILS_ICAO");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "SECONDARY_ILS_REGION");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "SECONDARY_NUMBER");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "SECONDARY_DESIGNATOR");
                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "CLOSE RUNWAY");

                    SimConnectNative.SimConnect_AddToFacilityDefinition(hSimConnect, defId, "CLOSE AIRPORT");

                    uint reqId = (uint)System.Threading.Interlocked.Increment(ref _requestCounter);

                    AirportEntity airport = new AirportEntity
                    {
                        Ident = icao,
                        Runways = new List<RunwayEntity>()
                    };

                    bool isCompleted = false;
                    bool receivedAirport = false;

                    // Delegate de callback
                    SimConnectNative.DispatchProc dispatchProc = (IntPtr pData, uint cbData, IntPtr pContext) =>
                    {
                        if (pData == IntPtr.Zero || cbData < Marshal.SizeOf(typeof(SIMCONNECT_RECV)))
                            return;

                        var header = (SIMCONNECT_RECV)Marshal.PtrToStructure(pData, typeof(SIMCONNECT_RECV));

                        if (header.dwID == SimConnectNative.SIMCONNECT_RECV_ID_FACILITY_DATA)
                        {
                            var facHeader = (SIMCONNECT_RECV_FACILITY_DATA)Marshal.PtrToStructure(pData, typeof(SIMCONNECT_RECV_FACILITY_DATA));
                            if (facHeader.UserRequestId == reqId)
                            {
                                int headerSize = Marshal.SizeOf(typeof(SIMCONNECT_RECV_FACILITY_DATA));
                                IntPtr dataPtr = new IntPtr(pData.ToInt64() + headerSize);

                                if (facHeader.Type == SimConnectNative.SIMCONNECT_FACILITY_DATA_AIRPORT)
                                {
                                    var aptData = (NativeAirportData)Marshal.PtrToStructure(dataPtr, typeof(NativeAirportData));
                                    airport.Name = aptData.Name?.Trim();
                                    airport.Laty = aptData.Latitude;
                                    airport.Lonx = aptData.Longitude;
                                    airport.Altitude = (long)aptData.Altitude;
                                    airport.MagVar = aptData.MagVar;
                                    airport.NumRunways = aptData.NRunways;
                                    receivedAirport = true;
                                }
                                else if (facHeader.Type == SimConnectNative.SIMCONNECT_FACILITY_DATA_RUNWAY)
                                {
                                    var rwyData = (NativeRunwayData)Marshal.PtrToStructure(dataPtr, typeof(NativeRunwayData));
                                    AddRunwayPair(airport, rwyData);
                                }
                            }
                        }
                        else if (header.dwID == SimConnectNative.SIMCONNECT_RECV_ID_FACILITY_DATA_END)
                        {
                            var endHeader = (SIMCONNECT_RECV_FACILITY_DATA_END)Marshal.PtrToStructure(pData, typeof(SIMCONNECT_RECV_FACILITY_DATA_END));
                            if (endHeader.RequestId == reqId)
                            {
                                isCompleted = true;
                            }
                        }
                        else if (header.dwID == SimConnectNative.SIMCONNECT_RECV_ID_EXCEPTION)
                        {
                            _log.Warn($"SimConnect exception received during facility request for {icao}");
                            isCompleted = true;
                        }
                    };

                    int requestHr = SimConnectNative.SimConnect_RequestFacilityData(hSimConnect, defId, reqId, icao, "");
                    if (requestHr != 0)
                    {
                        _log.Warn($"SimConnect_RequestFacilityData failed for {icao}. hr={requestHr}");
                        return null;
                    }

                    // Loop de despacho de mensagens (timeout de 8 segundos)
                    var startTime = DateTime.UtcNow;
                    while (!isCompleted && (DateTime.UtcNow - startTime).TotalMilliseconds < 8000)
                    {
                        SimConnectNative.SimConnect_CallDispatch(hSimConnect, dispatchProc, IntPtr.Zero);
                        System.Threading.Thread.Sleep(30);
                    }

                    // Mantém a referência do delegate viva durante o loop
                    GC.KeepAlive(dispatchProc);

                    if (receivedAirport)
                    {
                        _log.Info($"Successfully loaded airport {icao} ({airport.Name}) with {airport.Runways.Count} runway ends from MSFS 2024.");
                        return airport;
                    }

                    _log.Warn($"Airport {icao} not found in MSFS 2024 or timed out.");
                    return null;
                }
                catch (Exception ex)
                {
                    _log.Error($"Error querying SimConnect native for {icao}", ex);
                    return null;
                }
                finally
                {
                    if (hSimConnect != IntPtr.Zero)
                    {
                        try { SimConnectNative.SimConnect_Close(hSimConnect); }
                        catch { }
                    }
                }
            });
        }

        private static void AddRunwayPair(AirportEntity airport, NativeRunwayData rwy)
        {
            var halfLen = rwy.Length / 2.0;

            var primaryCoord = CalculateDestinationPoint(rwy.Latitude, rwy.Longitude, (rwy.Heading + 180.0) % 360.0, halfLen);
            var secondaryCoord = CalculateDestinationPoint(rwy.Latitude, rwy.Longitude, rwy.Heading, halfLen);

            var primaryIdent = RunwayDesignatorUtil.FormatRunwayIdent(rwy.PrimaryNumber, rwy.PrimaryDesignator);
            var secondaryIdent = RunwayDesignatorUtil.FormatRunwayIdent(rwy.SecondaryNumber, rwy.SecondaryDesignator);

            var pRwy = new RunwayEntity
            {
                Name = primaryIdent,
                HeadingTrue = rwy.Heading,
                Length = rwy.Length,
                Width = rwy.Width,
                Altitude = (long)rwy.Altitude,
                Laty = rwy.Latitude,
                Lonx = rwy.Longitude,
                PrimaryLaty = primaryCoord.lat,
                PrimaryLonx = primaryCoord.lon,
                SecondaryLaty = secondaryCoord.lat,
                SecondaryLonx = secondaryCoord.lon,
                EndType = "P"
            };

            var sRwy = new RunwayEntity
            {
                Name = secondaryIdent,
                HeadingTrue = (rwy.Heading + 180.0) % 360.0,
                Length = rwy.Length,
                Width = rwy.Width,
                Altitude = (long)rwy.Altitude,
                Laty = rwy.Latitude,
                Lonx = rwy.Longitude,
                PrimaryLaty = primaryCoord.lat,
                PrimaryLonx = primaryCoord.lon,
                SecondaryLaty = secondaryCoord.lat,
                SecondaryLonx = secondaryCoord.lon,
                EndType = "S"
            };

            airport.Runways.Add(pRwy);
            airport.Runways.Add(sRwy);
        }

        private static (double lat, double lon) CalculateDestinationPoint(double startLat, double startLon, double bearingDegrees, double distanceMeters)
        {
            double r = 6371000.0;
            double d = distanceMeters / r;
            double brng = bearingDegrees * Math.PI / 180.0;
            double lat1 = startLat * Math.PI / 180.0;
            double lon1 = startLon * Math.PI / 180.0;

            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d) + Math.Cos(lat1) * Math.Sin(d) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat1), Math.Cos(d) - Math.Sin(lat1) * Math.Sin(lat2));

            return (lat2 * 180.0 / Math.PI, lon2 * 180.0 / Math.PI);
        }
    }
}
