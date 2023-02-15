using AutoMapper;
using ConnectorClientAPI;
using FlightJobsDesktop.ViewModels;

namespace FlightJobsDesktop.Mapper
{
    public class DbModelToViewModelMapper
    {
        private static bool _isInitialized;


        internal static MapperConfiguration MapperCfg;
        public static MapperConfiguration Initialize()
        {
            if (!_isInitialized)
            {
                MapperCfg = new MapperConfiguration(cfg => {
                    cfg.CreateMap<JobModel, CurrentJobViewModel>();
                    cfg.CreateMap<JobModel, LastJobViewModel>();
                    //    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.NickName));
                });

                _isInitialized = true;
                return MapperCfg;
            }
            return null;
        }
    }
}
