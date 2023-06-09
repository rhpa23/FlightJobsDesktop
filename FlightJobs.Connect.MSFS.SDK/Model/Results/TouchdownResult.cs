namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class TouchdownResult : ResultBase
    {
        private static ResultEnum Validade(int fpm)
        {
            if (fpm > -180)
                return ResultEnum.Good;
            else if (fpm > -260)
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(int fpm)
        {
            return GetResultColor(Validade(fpm));
        }

        public static int GetScore(int fpm)
        {
            switch (Validade(fpm))
            {
                case ResultEnum.Good:
                    return 3;
                case ResultEnum.Normal:
                    return -4;
                case ResultEnum.Bad:
                    return -8;
                default:
                    return 0;
            }
        }
    }
}
