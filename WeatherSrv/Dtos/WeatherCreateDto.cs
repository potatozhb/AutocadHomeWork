
using System.ComponentModel.DataAnnotations;

namespace WeatherSrv.Dtos
{
    public class WeatherCreateDto
    {
        [Required]
        public bool? Rain { get; set; } //need nullable value to check the field exist
    }
}