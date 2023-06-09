using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class CenterDerivationResult : ResultBase
    {
        private static ResultEnum Validade(double centerDerivation, double windSpped)
        {
            if (windSpped < 12)
            {
                if (centerDerivation < 3)
                    return ResultEnum.Good;
                else if (centerDerivation <= 6)
                    return ResultEnum.Normal;
                else
                    return ResultEnum.Bad;
            }
            else
            {
                if (centerDerivation < 5)
                    return ResultEnum.Good;
                else if (centerDerivation <= 8)
                    return ResultEnum.Normal;
                else
                    return ResultEnum.Bad;
            }
        }

        public static string GetColor(double centerDerivation, double windSpped)
        {
            return GetResultColor(Validade(centerDerivation, windSpped));
        }

        public static int GetScore(double centerDerivation, double windSpped)
        {
            switch (Validade(centerDerivation, windSpped))
            {
                case ResultEnum.Good:
                    return 2;
                case ResultEnum.Normal:
                    return -2;
                case ResultEnum.Bad:
                    return -8;
                default:
                    return 0;
            }
        }
    }
}
