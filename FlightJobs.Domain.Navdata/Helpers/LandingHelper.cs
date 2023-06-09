using FlightJobs.Domain.Navdata.Entities;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Domain.Navdata.Helpers
{
    public class LandingHelper
    {
        private double _touchdownHeading;
        private double _touchdownLatitude;
        private double _touchdownLongitude;
        public LandingHelper(double touchdownHeading, double touchdownLatitude, double touchdownLongitude)
        {
            _touchdownHeading = touchdownHeading;
            _touchdownLatitude = touchdownLatitude;
            _touchdownLongitude = touchdownLongitude;
        }

        public RunwayEntity GetLandingRwy(IList<RunwayEntity> runways)
        {
            RunwayEntity rwyResult = runways[0];
            double centerDistance = 100000;
            foreach (var rwy in runways)
            {
                int range = 40;
                var headingRangeA = rwy.HeadingTrue + range > 360 ? rwy.HeadingTrue + range - 360 : rwy.HeadingTrue + range;
                var headingRangeB = rwy.HeadingTrue - range < 0 ? 360 - rwy.HeadingTrue - range : rwy.HeadingTrue - range;
                if (_touchdownHeading <= headingRangeA &&
                    _touchdownHeading >= headingRangeB)
                {
                    double tempCenterDistance = GetTouchdownCenterLineDistance(rwy);
                    if (tempCenterDistance < centerDistance) /// 500  5 200 
                    {
                        rwyResult = rwy;
                    }
                    centerDistance = tempCenterDistance;
                }

            }
            return rwyResult;
        }

        public double GetTouchdownRunwayLength(RunwayEntity rwy)
        {
            return GeoCalculationsUtil.CalcDistance(rwy.PrimaryLaty, rwy.PrimaryLonx, rwy.SecondaryLaty, rwy.SecondaryLonx);
        }

        public double GetTouchdownCenterLineDistance(RunwayEntity rwy)
        {
            var rwyLen = GetTouchdownRunwayLength(rwy);
            var distP = GeoCalculationsUtil.CalcDistance(rwy.PrimaryLaty, rwy.PrimaryLonx, _touchdownLatitude, _touchdownLongitude);
            var distS = GeoCalculationsUtil.CalcDistance(rwy.SecondaryLaty, rwy.SecondaryLonx, _touchdownLatitude, _touchdownLongitude);

            return Math.Round(GeoCalculationsUtil.GetTriangleHeight(rwyLen, distP, distS), 1);
        }

        public double GetTouchdownThresholdDistance(RunwayEntity rwy)
        {
            var rwyLat = rwy.PrimaryLaty;
            var rwyLon = rwy.PrimaryLonx;

            if (rwy.EndType == "S") // P = Primary    S = Secondary
            {
                rwyLat = rwy.SecondaryLaty;
                rwyLon = rwy.SecondaryLonx;
            }
            var offsetThresholdMeters = DataConversionUtil.ConvertFeetToMeters(rwy.OffsetThreshold);

            var dist = GeoCalculationsUtil.CalcDistance(rwyLat, rwyLon, _touchdownLatitude, _touchdownLongitude) - offsetThresholdMeters;

            return Math.Round(dist, 0);
        }

        public AirportEntity GetLandingJobAirport(ISqLiteDbContext sqLiteDbContext, string arrivalICAO, string alternativeICAO)
        {
            var aptArrival = sqLiteDbContext.GetAirportByIcao(arrivalICAO);
            if (GeoCalculationsUtil.CheckClosestLocation(_touchdownLatitude, _touchdownLongitude, aptArrival.Laty, aptArrival.Lonx))
            {
                return aptArrival;
            }
            else
            {
                var aptAlternative = sqLiteDbContext.GetAirportByIcao(alternativeICAO);
                if (GeoCalculationsUtil.CheckClosestLocation(_touchdownLatitude, _touchdownLongitude, aptAlternative.Laty, aptAlternative.Lonx))
                {
                    return aptAlternative;
                }
            }
            return null;
        }
    }
}
