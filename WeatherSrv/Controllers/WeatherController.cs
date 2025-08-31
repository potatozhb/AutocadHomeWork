using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherSrv.Dtos;
using WeatherSrv.Models;
using WeatherSrv.Repos;
using WeatherSrv.Services;

namespace WeatherSrv.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger _logger;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users data, Not in the requirement, just for test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Weather>>> GetWeathers()
        {
            var weathers = await this._weatherService.GetWeathersAsync();
            return Ok(weathers);
        }

        [HttpGet("data")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WeatherReadDto>>> GetUserWeathers(
            [FromHeader(Name = "x-userId")] string userId,
            [FromQuery] int? start = null,
            [FromQuery] int? end = null)
        {
            try
            {
                var weatherDtos = await _weatherService.GetWeathersAsync(userId, start, end);

                if (!weatherDtos.Any())
                    return NotFound($"User {userId} not found");

                return Ok(new WeatherReadResponse { Data = weatherDtos });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching user weathers");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost("data")]
        [Authorize]
        public async Task<ActionResult<WeatherReadDto>> CreateWeatherForUser(
                                [FromHeader(Name = "x-userId")] string userId,
                                [FromBody] WeatherCreateDto weatherCreateDto)
        {
            /*
             * Suppose we just record one place weather.
             1. if different users create opposite data, how to handle?
                [since we retrieve the data by user, I will record all data]
             2. can one day record multiple records?
                [one day can record multiple records, timestamp is different]
             */
            try
            {
                var createdWeather = await _weatherService.CreateWeatherForUserAsync(userId, weatherCreateDto);
                return Created("", createdWeather);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating weather for user");
                return StatusCode(500, "An unexpected error occurred.");
            }
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