using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Client.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Client.Controllers
{
    [ApiController]
    [Route("/")]
    public class WeatherForecastController : ControllerBase
    {
  
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherService _service;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
        {
            _logger.LogInformation("Fetching weather from API");

            try
            {

                var forecast = await _service.GetWeatherForecast();
                if (forecast != null)
                {
                    _logger.LogInformation("Weather successfully fetched");
                    return Ok(forecast);
                }
                else
                {
                    _logger.LogInformation("Failed to fetch weather");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get weather");
                return StatusCode(500, new { Error = "Something happened" });
            }
        }
    }
}
