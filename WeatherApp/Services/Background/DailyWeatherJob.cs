using Quartz;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Services.Background;

public class DailyWeatherJob : IJob
{
    private readonly IWeatherService _weatherService;
    private readonly IEmailService _emailService;
    private readonly IAIService _aiService; // 👈 NEW: Inject AI Service
    private readonly WeatherApp.Data.WeatherDbContext _context;

    public DailyWeatherJob(
        IWeatherService weatherService, 
        IEmailService emailService, 
        IAIService aiService, 
        WeatherApp.Data.WeatherDbContext context)
    {
        _weatherService = weatherService;
        _emailService = emailService;
        _aiService = aiService;
        _context = context;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // 1. Get all subscribed users
        // Note: In a real app, you'd loop through users. For now, we assume one "Main User" or grab the first one.
        var user = _context.Users.FirstOrDefault(u => u.IsSubscribed && !string.IsNullOrEmpty(u.Email));

        if (user == null) return; // No one to email

        // 2. Get Real Weather (Hardcoded to Coimbatore for now, or user.City if you have it)
        var city = "Coimbatore"; 
        var weather = await _weatherService.GetWeatherAsync(city);

        if (weather != null)
        {
            // 3. 🤖 Get AI Advice
            var advice = await _aiService.GetFashionAdviceAsync(
                weather.City, 
                weather.Weather, 
                weather.Temperature, 
                weather.AQI
            );

            // 4. Send Email (Now with AI Advice!)
            await _emailService.SendWeatherEmailAsync(user.Email, weather, advice);
        }
    }
}