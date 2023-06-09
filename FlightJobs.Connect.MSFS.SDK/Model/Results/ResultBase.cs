using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class ResultBase
    {
        internal static string GetResultColor(ResultEnum r)
        {
            switch (r)
            {
                case ResultEnum.Good:
                    return "Green";
                case ResultEnum.Normal:
                    return "Orange";
                case ResultEnum.Bad:
                    return "Red";
                default:
                    return "Green";
            }
        }

    }
}
