using AutoMapper;
using FlightJobs.Domain.Entities;
using FlightJobs.Infrastructure.Persistence;
using FlightJobsDesktop.ViewModels;

namespace FlightJobsDesktop.Mapper
{
    public static class ViewModelToDbModelMapper
    {
        private static bool _isInitialized;
        public static MapperConfiguration Initialize()
        {
            if (!_isInitialized)
            {
                var configuration = new MapperConfiguration(cfg => {
                    cfg.CreateMap<AspnetUserViewModel, ApplicationUser>()
                        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.NickName));
                });

                _isInitialized = true;
                return configuration;
            }
            return null;
        }
    }
}
