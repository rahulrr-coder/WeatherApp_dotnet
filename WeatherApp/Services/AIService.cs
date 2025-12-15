using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherApp.Services;

public interface IAIService
{
    Task<string> GetFashionAdviceAsync(string city, string weatherCondition, double temperature, int aqi);
}

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    public async Task<string> GetFashionAdviceAsync(string city, string weatherCondition, double temperature, int aqi)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return "Wear a smile! (API Key missing)";

        // 👇 BETTER PROMPT ENGINEERING
        var prompt = $@"
            Context: The weather in {city} is {temperature}°C with condition '{weatherCondition}'. 
            Air Quality Index is {aqi} (on a scale of 1 to 5, where 5 is poor).

            Your Role: A witty, cheerful style assistant.
            
            Task: Give a short outfit recommendation (max 30 words).
            
            Guidelines:
            - If raining/drizzle: You MUST say 'Don't forget your umbrella!' ☔
            - If AQI is 4 or 5: Gently suggest a mask (e.g., 'Air's a bit dusty' or 'Mask might be good') but keep it chill.
            - If clear/sunny: Mention sunglasses.
            - Tone: Casual, fun, and human-like. No robotic words like 'Strictly' or 'Hazardous'.
        ";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
        
        try 
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseString);
            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text 
                   ?? "Look good, feel good!";
        }
        catch 
        {
            return "Fashion AI is napping. Wear whatever makes you happy!";
        }
    }
}

// 👇 Helper Classes to map Gemini's JSON response
public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")]
    public Content? Content { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public List<Part>? Parts { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}