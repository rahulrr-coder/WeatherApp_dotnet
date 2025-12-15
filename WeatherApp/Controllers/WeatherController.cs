using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IAIService _aiService; // 👈 Added AI Service

    // Inject BOTH services here
    public WeatherController(IWeatherService weatherService, IAIService aiService)
    {
        _weatherService = weatherService;
        _aiService = aiService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        var weather = await _weatherService.GetWeatherAsync(city);
        if (weather == null) return NotFound("City not found");
        return Ok(weather);
    }

   // In WeatherController.cs

[HttpGet("advice")] // Renamed from 'test-ai' for clarity
public async Task<IActionResult> GetWeatherAdvice(string city)
{
    var weather = await _weatherService.GetWeatherAsync(city);
    if (weather == null) return NotFound("City not found");

    var advice = await _aiService.GetFashionAdviceAsync(
        weather.City, 
        weather.Weather, 
        weather.Temperature, 
        weather.AQI
    );

    return Ok(new { advice });
}
}