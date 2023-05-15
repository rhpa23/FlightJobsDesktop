using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class AirlineFboViewModel
    {
        public long Id { get; set; }
        public string Icao { get; set; }
        public string Name { get; set; }
        public string IcaoAndName { get { return $"{Icao} - {Name}"; } }
        public int RunwaySize { get; set; }
        public string RunwaySizeComplete { get { return $"{RunwaySize} ft"; } }
        public int Elevation { get; set; }
        public string ElevationComplete { get { return $"{Elevation} ft"; } }
        public int Availability { get; set; }
        public int ScoreIncrease { get; set; }
        public double FuelPriceDiscount { get; set; }
        public double GroundCrewDiscount { get; set; }
        public int Price { get; set; }
        public string PriceComplete { get { return string.Format("F{0:C}", Price); } }

        public string AirlineNameAux { get; set; }
        public string AirlineBankBalanceAux { get; set; }
    }

    
}
