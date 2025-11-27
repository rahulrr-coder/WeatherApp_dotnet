using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;
    
    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("{cityName}")]
    public async Task<IActionResult> Get(string cityName)
    {
        WeatherModel data = await _weatherService.GetWeatherForCity(cityName);
        
        return Ok(data);
    }
}