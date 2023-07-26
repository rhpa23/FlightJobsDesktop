using FlightJobs.Domain.Navdata.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlightJobsDesktop.ViewModels
{
    public class TipsDataGridViewModel
    {

        public IList<TipsDataGridViewModel> Tips { get; set; } = new List<TipsDataGridViewModel>();

        public string AirportICAO { get; set; }

        public string AirportName { get; set; }

        public long Distance { get; set; }
        public long AirportRunwaySize { get; set; }
        public long AirportRunwaySizeMeters { get { return (long)DataConversionUtil.ConvertFeetToMeters(AirportRunwaySize); } }

        public int AirportElevation { get; set; }
        public string AirportElevationDesc { get { return $"{AirportElevation} ft"; } }

        public int AirportTrasition { get; set; }

        public int Pax { get; set; }

        public long Cargo { get; set; }

        public long Pay { get; set; }
        public string PayDesc { get { return string.Format("F{0:C}", Pay); } }

        public long IdJob { get; set; }

        public bool HasIdJob { get { return IdJob > 0; } }

        
    }
}
