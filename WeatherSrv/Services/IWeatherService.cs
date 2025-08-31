using WeatherSrv.Dtos;
using WeatherSrv.Models;

namespace WeatherSrv.Services
{
    public interface IWeatherService
    {
        Task<IEnumerable<Weather>> GetWeathersAsync();
        Task<IEnumerable<WeatherReadDto>> GetWeathersAsync(string userId, int? start, int? end);
        Task<WeatherReadDto> CreateWeatherForUserAsync(string userId, WeatherCreateDto weatherCreateDto);
    }
}