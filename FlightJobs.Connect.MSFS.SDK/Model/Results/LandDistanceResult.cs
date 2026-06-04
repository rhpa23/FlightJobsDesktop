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
            double touchdownRunwayLengthMaxLandZone;
            if (runwaylength < 800)
                touchdownRunwayLengthMaxLandZone = 150;
            else if (runwaylength <= 1200)
                touchdownRunwayLengthMaxLandZone = 250;
            else if (runwaylength <= 2400)
                touchdownRunwayLengthMaxLandZone = 300;
            else
                touchdownRunwayLengthMaxLandZone = 400;

            double touchdownZoneLength = 350;

            if (landDistance < touchdownRunwayLengthMaxLandZone)
                return ResultEnum.Normal;
            else if (landDistance > touchdownRunwayLengthMaxLandZone + touchdownZoneLength)
                return ResultEnum.Bad;
            else
                return ResultEnum.Good;
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
                    return 15;
                case ResultEnum.Normal:
                    return -10;
                case ResultEnum.Bad:
                    return -25;
                default:
                    return 0;
            }
        }
    }
}
