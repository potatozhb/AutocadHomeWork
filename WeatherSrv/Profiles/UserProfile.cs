
using AutoMapper;
using WeatherSrv.Dtos;
using WeatherSrv.Models;

namespace WeatherSrv.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            //S -> D
            CreateMap<User, UserReadDto>();
            CreateMap<UserCreateDto, User>();
        }
    }
}