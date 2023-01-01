using System;
using System.Collections.Generic;



namespace FlightJobs.Domain.Entities
{
    public partial class Airlinefbodbmodel
    {
        public long Id { get; set; }
        public string Icao { get; set; }
        public int Availability { get; set; }
        public int ScoreIncrease { get; set; }
        public double FuelPriceDiscount { get; set; }
        public double GroundCrewDiscount { get; set; }
        public int Price { get; set; }
        public int? AirlineId { get; set; }

        public virtual Airlinedbmodel Airline { get; set; }
    }
}
