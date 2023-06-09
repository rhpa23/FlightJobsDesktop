using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class LandDistanceResult : ResultBase
    {
        private static ResultEnum Validade(double landDistance, double runwaylength)
        {
            double maxDistance = runwaylength * 0.25; // 25%
            if (landDistance < maxDistance)
                return ResultEnum.Good;
            else if (landDistance <= maxDistance + 200)
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(double landDistance, double runwaylength)
        {
            return GetResultColor(Validade(landDistance, runwaylength));
        }

        public static int GetScore(double landDistance, double runwaylength)
        {
            switch (Validade(landDistance, runwaylength))
            {
                case ResultEnum.Good:
                    return 5;
                case ResultEnum.Normal:
                    return -5;
                case ResultEnum.Bad:
                    return -15;
                default:
                    return 0;
            }
        }
    }
}
