using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Interfaces
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherForecast();
    }
}
