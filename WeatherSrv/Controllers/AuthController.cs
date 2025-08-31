using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WeatherSrv.Dtos;

namespace WeatherSrv.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController:ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config, IMapper mapper, ILogger<WeatherController> logger)
        {
            _config = config;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] UserCreateDto loginDto)
        {
            // use fixed password, not include enrollment
            if (loginDto.Password == "admin")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, loginDto.Username),
                    new Claim("role", "User")
                };

                if(loginDto.Username == "admin")
                {
                    claims = new[]
                    {
                        new Claim(ClaimTypes.Name, loginDto.Username),
                        new Claim("role", "Admin")
                    };
                }

                var issuer = _config.GetValue<string>("Issuer");
                var audience = _config.GetValue<string>("Audience");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _config.GetValue<string>("SuperSecretKey")));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            
            return Unauthorized();
        }
    }
}