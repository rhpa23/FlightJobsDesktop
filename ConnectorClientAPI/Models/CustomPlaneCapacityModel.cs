using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorClientAPI.Models
{
    public class CustomPlaneCapacityModel
    {
        public int CustomPassengerCapacity { get; set; }
        public int CustomCargoCapacityWeight { get; set; }
        public string CustomNameCapacity { get; set; }
        public long CustomPaxWeight { get; set; }
        public string ImagePath { get; set; }
    }
}
