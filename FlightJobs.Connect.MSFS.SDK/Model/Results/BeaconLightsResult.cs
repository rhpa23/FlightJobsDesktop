using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class BeaconLightsResult : ResultBase
    {
        private static ResultEnum Validade(bool lightBeaconOn, bool _engOneRunning)
        {
            if (!lightBeaconOn && _engOneRunning)
                return ResultEnum.Bad;
            
            return ResultEnum.Good;
        }
        public static int GetScore(bool lightBeaconOn, bool _engOneRunning)
        {
            switch (Validade(lightBeaconOn, _engOneRunning))
            {
                case ResultEnum.Good:
                    return 0;
                case ResultEnum.Normal:
                    return -5;
                case ResultEnum.Bad:
                    return -5;
                default:
                    return 0;
            }
        }
    }
}
