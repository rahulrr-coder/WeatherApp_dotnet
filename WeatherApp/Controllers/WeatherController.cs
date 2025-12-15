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

    // 👇 NEW TEST ENDPOINT
    [HttpGet("test-ai")]
    public async Task<IActionResult> TestAI(string city = "Coimbatore")
    {
        // 1. Get Real Weather Data
        var weather = await _weatherService.GetWeatherAsync(city);
        
        if (weather == null) 
            return NotFound($"Could not fetch weather for {city}");

        // 2. Ask Gemini for Advice
        // We pass the real AQI and Temp we just got
        var advice = await _aiService.GetFashionAdviceAsync(
            weather.City, 
            weather.Weather, 
            weather.Temperature, 
            weather.AQI
        );

        // 3. Show the result
        return Ok(new 
        { 
            City = weather.City,
            Temperature = weather.Temperature + "°C",
            AQI = weather.AQI,
            AI_Says = advice 
        });
    }
}