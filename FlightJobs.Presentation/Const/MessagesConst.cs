using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobsDesktop.Const
{
    public class MessagesConst
    {
        public const string MSG_TOUCHDOWN_TITLE = "Touchdown ({0}fpm) ";
        public const string MSG_GFORCE_TITLE = "GForce ({0}) ";
        public const string MSG_BOUNCE_TITLE = "Bounce(s) ({0}) ";
        public const string MSG_LANDING_DERIVATION_TITLE = "Center derivation ({0}m) ";
        public const string MSG_LANDING_DISTANCE_TITLE = "Landing distance ({0}m) ";
        public const string MSG_TAKEOFF_DERIVATION_TITLE = "Center derivation ({0}m) ";
        public const string MSG_UPWIND_LANDING_TITLE = "Upwind ";
        public const string MSG_BEACON_LIGHTS_TITLE = "Beacon lights ";
        public const string MSG_LANDING_LIGHTS_TITLE = "Landing lights ";
        public const string MSG_NAVEGATION_LIGHTS_TITLE = "Navegation lights ";
        public const string MSG_ST_ALTIMETER_TITLE = "Altimeter ";
        public const string MSG_TOTAL_SCORE_TITLE = "Total score ";
        
        public const string MSG_TOUCHDOWN_SUB_TITLE = "Smooth landings results in a better score ";
        public const string MSG_GFORCE_SUB_TITLE = "G-Force closer to 1 results in a better score ";
        public const string MSG_BOUNCE_SUB_TITLE = "Landings without bounces result in a better score ";
        public const string MSG_LANDING_DERIVATION_SUB_TITLE = "Landing closer to the runway centerline results in a better score ";
        public const string MSG_LANDING_DISTANCE_SUB_TITLE = "Small distance from runway threshold results in a better score ";
        public const string MSG_TAKEOFF_DERIVATION_SUB_TITLE = "Takeoff closer to the runway centerline results in a better score ";
        public const string MSG_UPWIND_LANDING_SUB_TITLE = "Upwind landings result in a better score ";
        public const string MSG_BEACON_LIGHTS_SUB_TITLE = "Beacon lights must be on while the engine(s) is running ";
        public const string MSG_LANDING_LIGHTS_SUB_TITLE = "Landing lights must be on below FL100 ";
        public const string MSG_NAVEGATION_LIGHTS_SUB_TITLE = "Navegation lights must be on while the engine(s) is running ";
        public const string MSG_ST_ALTIMETER_SUB_TITLE = "Altimeter should be set to Standard(1013mb/29.92Hg) when above the transition level ";
        public const string MSG_TOTAL_SCORE_SUB_TITLE = "The distance of the flight and the results define the total score ";
    }
}
