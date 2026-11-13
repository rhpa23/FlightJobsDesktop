using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Domain.Navdata.Utils
{
    public class GeoCalculationsUtil
    {
        public static double CalcDistance(double actualLat, double actualLon, double verifyLat, double verifyLon)
        {
            var actualCoord = new GeoCoordinate(actualLat, actualLon);
            var verifyCoord = new GeoCoordinate(verifyLat, verifyLon);
            return verifyCoord.GetDistanceTo(actualCoord);
        }

        public static bool CheckClosestLocation(double actualLat, double actualLon, double verifyLat, double verifyLon)
        {
            return CalcDistance(actualLat, actualLon, verifyLat, verifyLon) < 10000;
        }

        public static double GetTriangleHeight(double triangleBase, double triangleSideA, double triangleSideB)
        {
            if (triangleBase <= 0 || triangleSideA <= 0 || triangleSideB <= 0) return 0;
            double semiPerimeter = (triangleBase + triangleSideA + triangleSideB) / 2;
            double height = 2 / triangleBase * Math.Sqrt(semiPerimeter * (semiPerimeter - triangleBase) * (semiPerimeter - triangleSideA) * (semiPerimeter - triangleSideB));
            return height;
        }
    }
}
