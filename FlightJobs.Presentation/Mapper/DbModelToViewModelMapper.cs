using AutoMapper;
using FlightJobs.Model.Models;
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
                    cfg.CreateMap<SearchJobTipsModel, TipsDataGridViewModel>();
                    cfg.CreateMap<UserSettingsModel, UserSettingsViewModel>()
                          .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                          .ForPath(dest => dest.AutoFinishJob, opt => opt.MapFrom(src => src.LocalSettings.AutoFinishJob))
                          .ForPath(dest => dest.AutoStartJob, opt => opt.MapFrom(src => src.LocalSettings.AutoStartJob))
                          .ForPath(dest => dest.ExitWithFS, opt => opt.MapFrom(src => src.LocalSettings.ExitWithFS))
                          .ForPath(dest => dest.ShowLandingData, opt => opt.MapFrom(src => src.LocalSettings.ShowLandingData))
                          .ForPath(dest => dest.SimConnectStatus, opt => opt.MapFrom(src => src.LocalSettings.SimConnectStatus))
                          .ForPath(dest => dest.StartInSysTray, opt => opt.MapFrom(src => src.LocalSettings.StartInSysTray))
                          .ForPath(dest => dest.ThemeName, opt => opt.MapFrom(src => src.LocalSettings.ThemeName));
                    //    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.NickName));
                });

                _isInitialized = true;
                return MapperCfg;
            }
            return null;
        }
    }
}
