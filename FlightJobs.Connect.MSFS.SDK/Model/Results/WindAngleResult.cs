using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class WindAngleResult : ResultBase
    {
        private static ResultEnum Validade(int windAngle, double windSpped)
        {
            if (windAngle < -140 && windAngle > -220 ||
                windAngle > 140 && windAngle < 220 ||
                windSpped <= 3)
                return ResultEnum.Good;
            else if (windAngle < -90 && windAngle > -270 ||
                    (windAngle > 90 && windAngle < 270))
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(int windAngle, double windSpped)
        {
            if (Validade(windAngle, windSpped) == ResultEnum.Bad)
            {
                return "Red";
            }
            return "Green";
        }

        public static int GetScore(int windAngle, double windSpped)
        {
            switch (Validade(windAngle, windSpped))
            {
                case ResultEnum.Good:
                    return 0;
                case ResultEnum.Normal:
                    return 1;
                case ResultEnum.Bad:
                    return -8;
                default:
                    return 0;
            }
        }
    }
}
