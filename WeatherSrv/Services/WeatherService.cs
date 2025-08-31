using AutoMapper;
using WeatherSrv.Dtos;
using WeatherSrv.Models;
using WeatherSrv.Repos;

namespace WeatherSrv.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IWeatherRepo _weatherRepo;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public WeatherService(IWeatherRepo weatherRepo, IMapper mapper, ILogger<AuthService> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _weatherRepo = weatherRepo;
        }

        public async Task<IEnumerable<Weather>> GetWeathersAsync()
        {
            _logger.LogInformation("--> Getting Weathers....");

            var weathers = await this._weatherRepo.GetAllWeathersAsync();

            return weathers;
        }

        public async Task<IEnumerable<WeatherReadDto>> GetWeathersAsync(string userId, int? start, int? end)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("--> userId can't be empty");
                throw new ArgumentException("Invalid userId");
            }

            IEnumerable<Weather> weathers;

            if (start.HasValue && end.HasValue)
            {
                if (start < 0 || end <= start)
                {
                    _logger.LogError("--> Invalid paging parameters");
                    throw new ArgumentException("Invalid paging parameters");
                }

                _logger.LogInformation($"--> Getting Weathers for user {userId} from index {start.Value} to {end.Value}....");
                weathers = await _weatherRepo.GetAllWeathersByUserAsync(userId, start.Value, end.Value);
            }
            else
            {
                _logger.LogInformation($"--> Getting Weathers for user {userId}....");
                weathers = await _weatherRepo.GetAllWeathersByUserAsync(userId);
            }

            if (!weathers.Any())
            {
                _logger.LogWarning($"--> User {userId} not found or has no weather data");
                return Enumerable.Empty<WeatherReadDto>();
            }

            return _mapper.Map<IEnumerable<WeatherReadDto>>(weathers);
        }

        public async Task<WeatherReadDto> CreateWeatherForUserAsync(string userId, WeatherCreateDto weatherCreateDto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("--> userId can't be empty");
                throw new ArgumentException("Invalid userId");
            }

            _logger.LogInformation($"--> Creating Weathers for user {userId}....");

            var weather = _mapper.Map<Weather>(weatherCreateDto);
            weather.UserId = userId;

            _weatherRepo.CreateWeather(weather);
            await _weatherRepo.SaveChangesAsync();

            return _mapper.Map<WeatherReadDto>(weather);
        }

    }
}