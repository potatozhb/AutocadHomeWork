
using AutoMapper;
using WeatherSrv.Dtos;
using WeatherSrv.Models;

namespace WeatherSrv.Profiles
{
    public class WeatherProfile : Profile
    {
        public WeatherProfile()
        {
            //S -> D
            CreateMap<Weather, WeatherReadDto>()
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.UpdateTime));

            var utcNow = DateTime.UtcNow;
            CreateMap<WeatherCreateDto, Weather>()
                .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdateTime, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<WeatherUpdateDto, Weather>()
                .ForMember(dest => dest.UpdateTime, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}