﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class LandingLightsResult : ResultBase
    {
        private static ResultEnum Validade(bool lightLandingOn, bool onGround, long currentAltitude)
        {
            if (lightLandingOn && !onGround && currentAltitude > 11000)
                return ResultEnum.Normal;
            
            if (!lightLandingOn && !onGround && currentAltitude < 9000)
                return ResultEnum.Bad;

            return ResultEnum.Good;
        }
        public static int GetScore(bool lightLandingOn, bool onGround, long currentAltitude)
        {
            switch (Validade(lightLandingOn, onGround, currentAltitude))
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