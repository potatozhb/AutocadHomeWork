using WeatherSrv.Dtos;

namespace WeatherSrv.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(UserCreateDto loginDto);
    }
}