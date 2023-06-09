namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class GForceResult : ResultBase
    {
        private static ResultEnum Validade(double gForce)
        {
            if (gForce < 1.2)
                return ResultEnum.Good;
            else if (gForce <= 1.4)
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(double gForce)
        {
            return GetResultColor(Validade(gForce));
        }

        public static int GetScore(double gForce)
        {
            switch (Validade(gForce))
            {
                case ResultEnum.Good:
                    return 5;
                case ResultEnum.Normal:
                    return -2;
                case ResultEnum.Bad:
                    return -5;
                default:
                    return 0;
            }
        }
    }
}
