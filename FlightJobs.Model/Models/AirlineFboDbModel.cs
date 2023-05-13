using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Model.Models
{
    public class AirlineFboDbModel
    {
        public long Id { get; set; }
        public string Icao { get; set; }
        public string Name { get; set; }
        public int RunwaySize { get; set; }
        public int Elevation { get; set; }
        public AirlineModel Airline { get; set; }
        public int Availability { get; set; }
        public int ScoreIncrease { get; set; }
        public double FuelPriceDiscount { get; set; }
        public double GroundCrewDiscount { get; set; }
        public int Price { get; set; }
    }
}
