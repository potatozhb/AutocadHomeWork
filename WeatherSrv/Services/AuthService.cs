using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using WeatherSrv.Dtos;

namespace WeatherSrv.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config, IMapper mapper, ILogger<AuthService> logger)
        {
            _config = config;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<string?> AuthenticateAsync(UserCreateDto loginDto)
        {
            // TODO: replace with DB / Identity provider in real life
            if (loginDto.Password != "admin")
                return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginDto.Username),
                new Claim("role", loginDto.Username == "admin" ? "Admin" : "User")
            };

            var issuer = _config["Issuer"];
            var audience = _config["Audience"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["SuperSecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JWT issued for user {User}", loginDto.Username);

            return await Task.FromResult(jwt);
        }
    }
}