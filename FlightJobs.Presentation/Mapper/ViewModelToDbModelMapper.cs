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
                    cfg.CreateMap<PlaneModel, DataModel>();
                    cfg.CreateMap<FilterAirlineJobLedger, FilterJobsModel>();
                    cfg.CreateMap<AirlineViewModel, AirlineModel>()
                        .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.MinimumScoreToHire));
                    cfg.CreateMap<AirlineFilterViewModel, PaginatedAirlinersFilterModel>()
                            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.AirlineName))
                            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.AirlineCountry));
                    cfg.CreateMap<FilterLogbook, FilterJobsModel>();
                    cfg.CreateMap<AspnetUserViewModel, UserRegisterModel>()
                           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NickName))
                           .ForMember(dest => dest.ConfirmPassword, opt => opt.MapFrom(src => src.PasswordConfirmed))
                           .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
                    cfg.CreateMap<GenerateJobViewModel, ConfirmJobModel>()
                          .ForMember(dest => dest.Pax, opt => opt.MapFrom(src => src.SelectedPax))
                          .ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.SelectedCargo))
                          .ForMember(dest => dest.Pay, opt => opt.MapFrom(src => src.SelectedPay))
                          .ForMember(dest => dest.AviationType, opt => opt.MapFrom(src => src.AviationType.ToString()));
                    cfg.CreateMap<GenerateJobViewModel, GenerateJobModel>()
                          .ForMember(dest => dest.Departure, opt => opt.MapFrom(src => src.DepartureICAO))
                          .ForMember(dest => dest.Arrival, opt => opt.MapFrom(src => src.ArrivalICAO))
                          .ForMember(dest => dest.Alternative, opt => opt.MapFrom(src => src.AlternativeICAO))
                          .ForMember(dest => dest.CustomPlaneCapacity, opt => opt.MapFrom(src => src.Capacity))
                          .ForMember(dest => dest.AviationType, opt => opt.MapFrom(src => src.AviationType.ToString()));
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
