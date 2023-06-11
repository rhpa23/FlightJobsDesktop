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
    public class TakeoffHelper
    {
        private double _takeoffHeading;
        private double _takeoffLatitude;
        private double _takeoffLongitude;
        public TakeoffHelper(double takeoffHeading, double takeoffLatitude, double takeoffLongitude)
        {
            _takeoffHeading = takeoffHeading;
            _takeoffLatitude = takeoffLatitude;
            _takeoffLongitude = takeoffLongitude;
        }

        public RunwayEntity GetTakeoffRwy(IList<RunwayEntity> runways)
        {
            RunwayEntity rwyResult = runways[0];
            double centerDistance = 100000;
            foreach (var rwy in runways)
            {
                int range = 40;
                var headingRangeA = rwy.HeadingTrue + range > 360 ? rwy.HeadingTrue + range - 360 : rwy.HeadingTrue + range;
                var headingRangeB = rwy.HeadingTrue - range < 0 ? 360 - rwy.HeadingTrue - range : rwy.HeadingTrue - range;
                if (_takeoffHeading <= headingRangeA &&
                    _takeoffHeading >= headingRangeB)
                {
                    double tempCenterDistance = GetTakeoffCenterLineDistance(rwy);
                    if (tempCenterDistance < centerDistance) /// 500  5 200 
                    {
                        rwyResult = rwy;
                    }
                    centerDistance = tempCenterDistance;
                }

            }
            return rwyResult;
        }

        public double GettakeoffRunwayLength(RunwayEntity rwy)
        {
            return GeoCalculationsUtil.CalcDistance(rwy.PrimaryLaty, rwy.PrimaryLonx, rwy.SecondaryLaty, rwy.SecondaryLonx);
        }

        public double GetTakeoffCenterLineDistance(RunwayEntity rwy)
        {
            var rwyLen = GettakeoffRunwayLength(rwy);
            var distP = GeoCalculationsUtil.CalcDistance(rwy.PrimaryLaty, rwy.PrimaryLonx, _takeoffLatitude, _takeoffLongitude);
            var distS = GeoCalculationsUtil.CalcDistance(rwy.SecondaryLaty, rwy.SecondaryLonx, _takeoffLatitude, _takeoffLongitude);

            return Math.Round(GeoCalculationsUtil.GetTriangleHeight(rwyLen, distP, distS), 1);
        }

        public AirportEntity GetTakeoffJobAirport(ISqLiteDbContext sqLiteDbContext, string departureICAO)
        {
            var aptArrival = sqLiteDbContext.GetAirportByIcao(departureICAO);
            if (GeoCalculationsUtil.CheckClosestLocation(_takeoffLatitude, _takeoffLongitude, aptArrival.Laty, aptArrival.Lonx))
            {
                return aptArrival;
            }
            return null;
        }
    }
}
