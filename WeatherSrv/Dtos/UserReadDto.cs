
using System.ComponentModel.DataAnnotations;

namespace WeatherSrv.Dtos
{
    public class UserReadDto
    {
        [Required]
        [StringLength(100)]
        public string? Username { get; set; }
    }
}