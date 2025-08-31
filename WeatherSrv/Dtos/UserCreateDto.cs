
using System.ComponentModel.DataAnnotations;

namespace WeatherSrv.Dtos
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(100)]
        public string? Username { get; set; }

        [Required]
        [StringLength(100)]
        public string? Password { get; set; }

    }
}