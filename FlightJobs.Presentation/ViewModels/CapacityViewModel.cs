using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.ViewModels
{
    public class CapacityViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PassengersNumber { get; set; }
        public int CargoWeight { get; set; }
        public long PassengerWeight { get; set; }
        public string ImagePath { get; set; }
    }
}
