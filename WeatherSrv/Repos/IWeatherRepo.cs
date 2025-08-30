
using WeatherSrv.Models;

namespace WeatherSrv.Repos
{
    public interface IWeatherRepo
    {
        void CreateWeather(Weather weather);
        void UpdateWeather(Weather weather);
        void DeleteWeather(Guid id);

        Task<IEnumerable<Weather>> GetAllWeathersAsync();
        Task<IEnumerable<Weather>> GetAllWeathersByUserAsync(string userId);
        Task<IEnumerable<Weather>> GetAllWeathersByUserAsync(string userId, int start, int end);
        Task<Weather?> GetWeatherAsync(Guid id);

        Task<bool> SaveChangesAsync();
    }
}