
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WeatherSrv.Dtos;
using WeatherSrv.Models;
using WeatherSrv.Repos;

namespace WeatherSrv.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherRepo _weatherRepo;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public WeatherController(IWeatherRepo weatherRepo, IMapper mapper, ILogger<WeatherController> logger)
        {
            _weatherRepo = weatherRepo;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all users data, Not in the requirement, just for test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherReadDto>>> GetWeathers()
        {
            _logger.LogInformation("--> Getting Weathers....");
            Console.WriteLine("--> Getting Weathers....");

            var weathers = await this._weatherRepo.GetAllWeathersAsync();

            return Ok(weathers);
        }

        [HttpGet("data")]
        public async Task<ActionResult<IEnumerable<WeatherReadDto>>> GetUserWeathers(
            [FromHeader(Name = "x-userId")] string userId,
            [FromQuery] int? start = null,
            [FromQuery] int? end = null)
        {
            IEnumerable<Weather> weathers;

            if (start.HasValue && end.HasValue)
            {
                _logger.LogInformation($"--> Getting Weathers for user {userId} from index {start.Value} to {end.Value}....");
                Console.WriteLine($"--> Getting Weathers for user {userId} from index {start.Value} to {end.Value}....");
                if (start < 0 || end <= start)
                    return BadRequest("Invalid paging parameters");

                weathers = await this._weatherRepo.GetAllWeathersByUserAsync(userId, start.Value, end.Value);
            }
            else
            {
                _logger.LogInformation($"--> Getting Weathers for user {userId}....");
                Console.WriteLine($"--> Getting Weathers for user {userId}....");
                weathers = await this._weatherRepo.GetAllWeathersByUserAsync(userId);
            }

            if (weathers.Count() == 0) return NotFound($"User {userId} not found");
            var weathersReadDto = _mapper.Map<IEnumerable<WeatherReadDto>>(weathers);

            return Ok(new WeatherReadResponse() { Data = weathersReadDto });
        }

        [HttpPost("data")]
        public async Task<ActionResult<WeatherReadDto>> CreateWeatherForUser(
                                [FromHeader(Name = "x-userId")] string userId,
                                [FromBody] WeatherCreateDto weatherCreateDto)
        {
            _logger.LogInformation($"--> Creating Weathers for user {userId}....");
            Console.WriteLine($"--> Creating Weathers for user {userId}....");
            /*
             * Suppose we just record one place weather.
             1. if different users create opposite data, how to handle?
                [since we retrieve the data by user, I will record all data]
             2. can one day record multiple records?
                [one day can record multiple records, timestamp is different]
             */
            var weather = this._mapper.Map<Weather>(weatherCreateDto);
            weather.UserId = userId;
            this._weatherRepo.CreateWeather(weather);
            var weathersReadDto = _mapper.Map<WeatherReadDto>(weather);

            await this._weatherRepo.SaveChangesAsync();

            return Created("", weathersReadDto);
        }
    }

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    public class WeatherV2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Weather API v2 example");
    }
}