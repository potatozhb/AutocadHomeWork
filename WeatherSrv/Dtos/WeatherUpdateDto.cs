using System.ComponentModel.DataAnnotations;

namespace WeatherSrv.Dtos
{
    public class WeatherUpdateDto
    {
        [Required]
        public bool Rain { get; set; }
    }
}
