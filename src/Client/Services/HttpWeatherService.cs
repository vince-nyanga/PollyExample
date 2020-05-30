using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Interfaces;
using Newtonsoft.Json;

namespace Client.Services
{
    public class HttpWeatherService : IWeatherService
    {
        private readonly HttpClient _client;

        public HttpWeatherService(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecast()
        {
            var response = await _client.GetAsync("/api/weather");
            if (response.IsSuccessStatusCode)
            {
                var stringContent = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(stringContent);
            }
            else
            {
                return null;
            }
        }
    }
}
