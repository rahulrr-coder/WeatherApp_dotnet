using System.Net.Http.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

// Note: No "public interface" here anymore!
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenWeather:ApiKey"] ?? throw new Exception("API Key Missing");
    }

    public async Task<WeatherModel?> GetWeatherAsync(string city)
    {
        // 1. Get Basic Weather
        var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
        
        OpenWeatherResponse? weatherResponse = null;
        try 
        {
            weatherResponse = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(weatherUrl);
        }
        catch 
        {
            return null; 
        }

        if (weatherResponse == null) return null;

        // 2. Get AQI
        var lat = weatherResponse.coord.lat;
        var lon = weatherResponse.coord.lon;
        var aqiUrl = $"https://api.openweathermap.org/data/2.5/air_pollution?lat={lat}&lon={lon}&appid={_apiKey}";
        
        int aqiLevel = 1; 
        try 
        {
            var aqiResponse = await _httpClient.GetFromJsonAsync<AirPollutionResponse>(aqiUrl);
            aqiLevel = aqiResponse?.list?.FirstOrDefault()?.main?.aqi ?? 1;
        }
        catch 
        {
            // Ignore AQI errors
        }

        return new WeatherModel
        {
            City = weatherResponse.name,
            Weather = weatherResponse.weather.FirstOrDefault()?.main ?? "Unknown",
            Temperature = (int)weatherResponse.main.temp,
            AQI = aqiLevel
        };
    }
}

// --- Helper Classes (Keep these at the bottom) ---
public class OpenWeatherResponse 
{
    public string name { get; set; } = "";
    public MainData main { get; set; } = new();
    public List<WeatherInfo> weather { get; set; } = new();
    public Coord coord { get; set; } = new();
}
public class MainData { public double temp { get; set; } }
public class WeatherInfo { public string main { get; set; } = ""; }
public class Coord { public double lat { get; set; } public double lon { get; set; } }

public class AirPollutionResponse
{
    public List<PollutionData> list { get; set; } = new();
}
public class PollutionData { public MainAqi main { get; set; } = new(); }
public class MainAqi { public int aqi { get; set; } }