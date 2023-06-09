namespace FlightJobs.Connect.MSFS.SDK.Model.Results
{
    public class BounceResult : ResultBase
    {
        private static ResultEnum Validade(int count)
        {
            if (count == 0)
                return ResultEnum.Good;
            else if (count < 2)
                return ResultEnum.Normal;
            else
                return ResultEnum.Bad;
        }

        public static string GetColor(int count)
        {
            return GetResultColor(Validade(count));
        }

        public static int GetScore(int count)
        {
            switch (Validade(count))
            {
                case ResultEnum.Good:
                    return 2;
                case ResultEnum.Normal:
                    return 0;
                case ResultEnum.Bad:
                    return -10;
                default:
                    return 0;
            }
        }
    }
}
