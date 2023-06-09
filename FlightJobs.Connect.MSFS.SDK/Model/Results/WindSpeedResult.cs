namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class WindSpeedResult : ResultBase
    {
        private static ResultEnum Validade(double windSpeed)
        {
            if (windSpeed < 12)
                return ResultEnum.Good;
            else if (windSpeed < 17)
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(double windSpeed)
        {
            return GetResultColor(Validade(windSpeed));
        }

        public static int GetScore(double windSpeed)
        {
            switch (Validade(windSpeed))
            {
                case ResultEnum.Good:
                    return 0;
                case ResultEnum.Normal:
                    return 2;
                case ResultEnum.Bad:
                    return 5;
                default:
                    return 0;
            }
        }
    }
}
