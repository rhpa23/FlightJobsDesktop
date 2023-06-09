using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class AltimeterResult : ResultBase
    {
        private static ResultEnum Validade(long currentAltitude, int altimeterInMillibars)
        {
            if (currentAltitude > 19000 &&
                altimeterInMillibars != 1013)
                return ResultEnum.Bad;
            
            return ResultEnum.Good;
        }
        public static int GetScore(long currentAltitude, int altimeterInMillibars)
        {
            switch (Validade(currentAltitude, altimeterInMillibars))
            {
                case ResultEnum.Good:
                    return 0;
                case ResultEnum.Normal:
                    return -8;
                case ResultEnum.Bad:
                    return -8;
                default:
                    return 0;
            }
        }
    }
}
