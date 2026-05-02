using AutoMapper;
using FlightJobs.Connect.MSFS.SDK.Model;
using FlightJobs.Model.Models;
using FlightJobsDesktop.ViewModels;

namespace FlightJobsDesktop.Mapper
{
    public static class ViewModelToDbModelMapper
    {
        private static bool _isInitialized;

        internal static MapperConfiguration MapperCfg;

        public static MapperConfiguration Initialize()
        {
            if (!_isInitialized)
            {
                MapperCfg = new MapperConfiguration(cfg => {
                    cfg.CreateMap<PlaneModel, DataModel>()
                        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name));
                    cfg.CreateMap<CapacityViewModel, CustomPlaneCapacityModel>()
                          .ForMember(dest => dest.CustomCargoCapacityWeight, opt => opt.MapFrom(src => src.CargoWeight))
                          .ForMember(dest => dest.CustomNameCapacity, opt => opt.MapFrom(src => src.Name))
                          .ForMember(dest => dest.CustomPassengerCapacity, opt => opt.MapFrom(src => src.PassengersNumber))
                          .ForMember(dest => dest.CustomPaxWeight, opt => opt.MapFrom(src => src.PassengerWeight))
                          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                          .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath));
                    cfg.CreateMap<UserSettingsViewModel, UserSettingsModel>()
                          .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                          .ForPath(dest => dest.LocalSettings.AutoFinishJob, opt => opt.MapFrom(src => src.AutoFinishJob))
                          .ForPath(dest => dest.LocalSettings.AutoStartJob, opt => opt.MapFrom(src => src.AutoStartJob))
                          .ForPath(dest => dest.LocalSettings.ExitWithFS, opt => opt.MapFrom(src => src.ExitWithFS))
                          .ForPath(dest => dest.LocalSettings.ShowLandingData, opt => opt.MapFrom(src => src.ShowLandingData))
                          .ForPath(dest => dest.LocalSettings.StartInSysTray, opt => opt.MapFrom(src => src.StartInSysTray))
                          .ForPath(dest => dest.LocalSettings.SimbriefUsername, opt => opt.MapFrom(src => src.SimbriefUsername))
                          .ForPath(dest => dest.LocalSettings.ThemeName, opt => opt.MapFrom(src => src.ThemeName));
                    //    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.NickName));
                });

                _isInitialized = true;
                return MapperCfg;
            }
            return null;
        }
    }
}
