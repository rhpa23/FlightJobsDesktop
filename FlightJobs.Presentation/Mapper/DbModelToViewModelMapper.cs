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

                    cfg.CreateMap<PilotLicenseItemModel, PilotLicenseItemViewModel>();
                    cfg.CreateMap<LicenseItemModel, LicenseItemViewModel>();
                    cfg.CreateMap<PilotLicenseExpensesUserModel, PilotLicenseExpensesUserViewModel>();
                    cfg.CreateMap<PilotLicenseExpensesModel, PilotLicenseExpensesViewModel>();
                    cfg.CreateMap<ChartUserBankBalanceModel, ChartUserBankBalanceViewModel>();
                    cfg.CreateMap<ChartAirlineBankBalanceModel, ChartAirlineBankBalanceViewModel>();
                    cfg.CreateMap<UserStatisticsModel, UserStatisticsFlightsViewModel>();
                    cfg.CreateMap<AirlineFboDbModel, AirlineFboViewModel>();
                    cfg.CreateMap<AirlineModel, HiredFBOsViewModel>()
                        .ForMember(dest => dest.AirlineName, opt => opt.MapFrom(src => src.Name));
                    cfg.CreateMap<PaginatedAirlineJobLedgerModel, PaginatedAirlineJobLedgerViewModel>();
                    cfg.CreateMap<AirlineJobLedgerModel, AirlineJobLedgerViewModel>();
                    cfg.CreateMap<AirlineModel, AirlineDebtsViewModel>();
                    cfg.CreateMap<UserModel, PilotHiredViewModel>();
                    cfg.CreateMap<AirlineModel, AirlineViewModel>()
                          .ForMember(dest => dest.OwnerUserId, opt => opt.MapFrom(src => src.UserId))
                          .ForMember(dest => dest.MinimumScoreToHire, opt => opt.MapFrom(src => src.Score));
                    cfg.CreateMap<PaginatedAirlinersModel, AirlineFilterViewModel>();
                    cfg.CreateMap<JobModel, CurrentJobViewModel>();
                    cfg.CreateMap<JobModel, LastJobViewModel>();
                    cfg.CreateMap<JobModel, LogbookUserJobViewModel>();
                    cfg.CreateMap<PaginatedJobsModel, LogbookViewModel>();
                    cfg.CreateMap<SearchJobTipsModel, TipsDataGridViewModel>();
                    cfg.CreateMap<JobListItemModel, JobItemViewModel>();
                    cfg.CreateMap<CustomPlaneCapacityModel, CapacityViewModel>()
                          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CustomNameCapacity))
                          .ForMember(dest => dest.PassengersNumber, opt => opt.MapFrom(src => src.CustomPassengerCapacity))
                          .ForMember(dest => dest.PassengerWeight, opt => opt.MapFrom(src => src.CustomPaxWeight))
                          .ForMember(dest => dest.CargoWeight, opt => opt.MapFrom(src => src.CustomCargoCapacityWeight))
                          .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath));
                    cfg.CreateMap<UserSettingsModel, UserSettingsViewModel>()
                          .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                          .ForPath(dest => dest.AutoFinishJob, opt => opt.MapFrom(src => src.LocalSettings.AutoFinishJob))
                          .ForPath(dest => dest.AutoStartJob, opt => opt.MapFrom(src => src.LocalSettings.AutoStartJob))
                          .ForPath(dest => dest.ExitWithFS, opt => opt.MapFrom(src => src.LocalSettings.ExitWithFS))
                          .ForPath(dest => dest.ShowLandingData, opt => opt.MapFrom(src => src.LocalSettings.ShowLandingData))
                          .ForPath(dest => dest.StartInSysTray, opt => opt.MapFrom(src => src.LocalSettings.StartInSysTray))
                          .ForPath(dest => dest.SimbriefUsername, opt => opt.MapFrom(src => src.LocalSettings.SimbriefUsername))
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
