using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK
{
    public class UnitsConverterUtil
    {
        public static double PoundsToKilograms(double weightPounds)
        {
            return Math.Round(weightPounds * 0.453592, 0);
        }
    }
}
