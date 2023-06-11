using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.Const
{
    public class MessagesConst
    {
        public const string MSG_TOUCHDOWN = "Touchdown ({0}fpm). Smooth landings results in a better score: ";
        public const string MSG_GFORCE = "GForce ({0}). G-Force closer to 1 results in a better score: ";
        public const string MSG_BOUNCE = "Bounce(s) ({0}). Landings without bounces result in a better score: ";
        public const string MSG_LANDING_DERIVATION = "Center derivation ({0}m). Landing closer to the runway centerline results in a better score: ";
        public const string MSG_LANDING_DISTANCE = "Landing distance ({0}m). Small distance from runway threshold results in a better score: ";
        public const string MSG_TAKEOFF_DERIVATION = "Center derivation ({0}m). Takeoff closer to the runway centerline results in a better score: ";
        public const string MSG_UPWIND_LANDING = "Upwind landings result in a better score: ";
        public const string MSG_BEACON_LIGHTS = "Beacon lights must be on while the engine(s) is running: ";
        public const string MSG_LANDING_LIGHTS = "Landing lights must be on below FL100: ";
        public const string MSG_NAVEGATION_LIGHTS = "Navegation lights must be on while the engine(s) is running: ";
        public const string MSG_ST_ALTIMETER = "Altimeter should be set to Standard(1013mb/29.92Hg) when above the transition level: ";
        public const string MSG_TOTAL_SCORE = "The distance of the flight and the results define the total score: ";
    }
}
