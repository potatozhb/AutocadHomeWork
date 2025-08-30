namespace WeatherSrv.Dtos
{
    public class WeatherReadResponse
    {
        public IEnumerable<WeatherReadDto> Data { get; set; }
    }
}
