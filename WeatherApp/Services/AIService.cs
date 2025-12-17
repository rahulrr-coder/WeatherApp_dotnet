using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherApp.Models;

namespace WeatherApp.Services;

public interface IAIService
{
    Task<string> GetFashionAdviceAsync(WeatherModel weather);
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

    public async Task<string> GetFashionAdviceAsync(WeatherModel weather)
    {
        // DEBUG LOG 1: Check if key is loaded
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) 
        {
            Console.WriteLine("🔴 AI Error: API Key is NULL or Empty!");
            return "{}";
        }
        else
        {
            // Print first 5 chars to verify it's the right key (safe to log locally)
            Console.WriteLine($"🟢 AI Key Loaded: {apiKey.Substring(0, 5)}...");
        }

        var forecastSummary = string.Join(", ", weather.DayParts.Select(p => $"{p.PartName}: {p.Temp:F0}°C {p.Condition}"));

       
       var prompt = $@"
            Context: {weather.City} is {weather.CurrentTemp:F0}°C ({weather.CurrentCondition}). 
            Humidity: {weather.Humidity}%. Wind: {weather.WindSpeed} m/s. AQI: {weather.AQI}.
            
            Task: Return valid JSON.
            STYLE GUIDE:
            - Tone: Elegant, helpful, concise. Like a personal assistant.
            - Summary: Max 2 sentences. strictly descriptive but warm.
            - NO 'Vibe' check. 
            
            JSON Keys:
            1. 'summary': 2 sentences describing the weather feel.
            2. 'outfit': Specific clothing advice (e.g., 'Linen shirt', 'Trench coat').
            3. 'safety': One specific tip (e.g. 'Carry an umbrella', 'Wear a mask'), or 'No hazards'.

            Example: 
            {{ 
                ""summary"": ""A consistently cloudy day with a gentle breeze. Temperatures will remain mild, making it perfect for a walk."", 
                ""outfit"": ""Light sweater and comfortable trousers."", 
                ""safety"": ""No hazards."" 
            }}
        ";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // gemini-2.5-flash
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-8b:generateContent?key={apiKey}";


        

        try 
        {
            var response = await _httpClient.PostAsync(url, content);
            
            // DEBUG LOG 2: Check HTTP Status
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔴 Google API Error: {response.StatusCode}");
                Console.WriteLine($"🔴 Details: {errorBody}");
                return "{}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseString);
            
            var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "{}";
            Console.WriteLine("🟢 AI Success! Response received.");
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔴 EXCEPTION in AIService: {ex.Message}");
            return "{}";
        }
    }
}

// Keep your Helper Classes (GeminiResponse, Candidate, etc.) below...
public class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
public class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
public class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
public class Part { [JsonPropertyName("text")] public string? Text { get; set; } }