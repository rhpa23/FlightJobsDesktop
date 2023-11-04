using FlightJobs.Domain.Navdata.Entities;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Mapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Domain.Navdata
{
    public class SqLiteDbContext : ISqLiteDbContext
    {
        private static SQLiteConnection _sqliteConnection;

        public SqLiteDbContext()
        {
            _sqliteConnection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["NavdataContext"].ConnectionString);
        }

        public IList<AirportEntity> GetAirportsByIcaoAndName(string term)
        {
            var airports = new List<AirportEntity>();
            DataTable dt = new DataTable();
            using (var cmd = _sqliteConnection.CreateCommand())
            {
                term = term.Replace("\'", "");
                cmd.CommandText = $"SELECT * FROM airport WHERE ident like '{term}%' OR name LIKE '{term}%';";
                var da = new SQLiteDataAdapter(cmd.CommandText, _sqliteConnection);
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var airport = EntityDbMapper.CreateItemFromRow<AirportEntity>(dt.Rows[i]);
                    airports.Add(airport);
                }
                return airports;
            }
        }

        public AirportEntity GetAirportByIcao(string icao)
        {
            DataTable dt = new DataTable();
            using (var cmd = _sqliteConnection.CreateCommand())
            {
                icao = icao.Replace("\'", "");
                cmd.CommandText = $"SELECT * FROM airport WHERE UPPER(ident) = '{icao?.ToUpper()}'";
                var da = new SQLiteDataAdapter(cmd.CommandText, _sqliteConnection);
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    var airport = EntityDbMapper.CreateItemFromRow<AirportEntity>(dt.Rows[0]);
                    airport.Runways = GetRunwaysByIcao(icao);
                    return airport;
                }
                return null;
            }
        }

        public IList<RunwayEntity> GetRunwaysByIcao(string icao)
        {
            var runways = new List<RunwayEntity>();
            DataTable dt = new DataTable();
            using (var cmd = _sqliteConnection.CreateCommand())
            {
                icao = icao.Replace("\'", "");
                cmd.CommandText = $@"SELECT r.*, re.name, re.offset_threshold, re.heading AS heading_true, re.end_type FROM runway r
                                        INNER JOIN airport a ON r.airport_id = a.airport_id
                                        INNER JOIN runway_end re ON r.primary_end_id = re.runway_end_id OR r.secondary_end_id = re.runway_end_id
                                        WHERE a.ident = '{icao}'";
                var da = new SQLiteDataAdapter(cmd.CommandText, _sqliteConnection);
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var rwyData = EntityDbMapper.CreateItemFromRow<RunwayEntity>(dt.Rows[i]);
                    runways.Add(rwyData);
                }
                return runways;
            }
        }
    }
}
