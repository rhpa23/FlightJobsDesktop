using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class WindAngleResult : ResultBase
    {
        private static ResultEnum Validade(int windAngle)
        {
            if (windAngle < -140 && windAngle > -220 ||
                windAngle > 140 && windAngle < 220)
                return ResultEnum.Good;
            else if (windAngle < -90 && windAngle > -270 ||
                    (windAngle > 90 && windAngle < 270))
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(int windAngle)
        {
            if (Validade(windAngle) == ResultEnum.Bad)
            {
                return "Red";
            }
            return "Green";
        }

        public static int GetScore(int windAngle)
        {
            switch (Validade(windAngle))
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
