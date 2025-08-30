
using System.ComponentModel.DataAnnotations;

namespace WeatherSrv.Dtos
{
    public class WeatherReadDto
    {
        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public bool Rain { get; set; }
    }
}