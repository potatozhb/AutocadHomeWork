
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WeatherSrv.Data;
using WeatherSrv.Models;

namespace WeatherSrv.Repos
{
    public class WeatherRepo : IWeatherRepo
    {
        private const int CacheTTLMin = 5;

        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WeatherRepo> _logger;
        public WeatherRepo(AppDbContext context, IMemoryCache cache, ILogger<WeatherRepo> logger)
        {
            this._context = context;
            this._cache = cache;
            this._logger = logger;
        }

        public void CreateWeather(Weather weather)
        {
            if (weather == null) throw new ArgumentNullException(nameof(weather));
            this._context.Weathers.Add(weather);
            this._cache.Set(weather.Id, weather, TimeSpan.FromMinutes(CacheTTLMin));
            this._logger.LogInformation($"--> Add new data");
        }

        public void DeleteWeather(Guid id)
        {
            var weather = GetWeatherAsync(id).Result;
            if (weather != null)
            {
                this._cache.Remove(weather.Id);
                this._context.Weathers.Remove(weather);

                this._logger.LogInformation($"--> Remove a data");
            }
        }

        public async Task<IEnumerable<Weather>> GetAllWeathersAsync()
        {
            return await this._context.Weathers.ToListAsync();
        }

        public async Task<IEnumerable<Weather>> GetAllWeathersByUserAsync(string userId)
        {
            return await this._context.Weathers.Where(w => w.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Weather>> GetAllWeathersByUserAsync(string userId, int start, int end)
        {
            return await this._context.Weathers
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.UpdateTime)
                .Skip(start)
                .Take(end - start)
                .ToListAsync();
        }

        public async Task<Weather?> GetWeatherAsync(Guid id)
        {
            if (this._cache.TryGetValue(id, out var data) && data is Weather weather)
            {
                this._logger.LogInformation($"--> Get a data from cache");
                return weather;
            }

            weather = await _context.Weathers.FirstOrDefaultAsync(w => w.Id == id);
            if (weather != null) this._cache.Set(id, weather, TimeSpan.FromMinutes(CacheTTLMin));

            this._logger.LogInformation($"--> Get a data from db");
            return weather;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await this._context.SaveChangesAsync() >= 0;
        }

        public void UpdateWeather(Weather weather)
        {
            if (weather == null) throw new ArgumentNullException(nameof(weather));

            var curWeather = this.GetWeatherAsync(weather.Id).Result;
            if (curWeather == null)
                throw new InvalidOperationException("Weather not found");

            curWeather.Rain = weather.Rain;
            curWeather.UserId = weather.UserId;
            curWeather.UpdateTime = weather.UpdateTime;
        }
    }
}