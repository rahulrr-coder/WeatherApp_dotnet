using System.Net;
using System.Net.Mail;
using WeatherApp.Models; // Needed for WeatherModel

namespace WeatherApp.Services;

public interface IEmailService
{
    // 1. Basic raw email sender
    Task SendEmailAsync(string toEmail, string subject, string body);

    // 2. 👇 The new fancy method for Weather + AI
    Task SendWeatherEmailAsync(string toEmail, WeatherModel weather, string aiAdvice);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // The Base Method (Talks to Gmail)
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromEmail = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }

    // The Specific Method (Builds the HTML Template)
    public async Task SendWeatherEmailAsync(string toEmail, WeatherModel weather, string aiAdvice)
    {
        var subject = $"Your Daily Forecast for {weather.City} 🌤️";
        
        var body = $@"
        <div style='font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 12px; overflow: hidden; color: #333;'>
            
            <div style='background-color: #4CAF50; color: white; padding: 25px; text-align: center;'>
                <h1 style='margin: 0; font-size: 24px;'>Good Morning! ☀️</h1>
                <p style='margin: 5px 0 0; opacity: 0.9;'>Here is your update for {weather.City}</p>
            </div>

            <div style='padding: 25px;'>
                
                <div style='display: flex; justify-content: space-between; margin-bottom: 25px;'>
                    <div style='text-align: center; width: 48%; background: #f9f9f9; padding: 15px; border-radius: 8px;'>
                        <span style='font-size: 28px; display: block; font-weight: bold; color: #333;'>{weather.Temperature}°C</span>
                        <span style='color: #666; font-size: 14px;'>{weather.Weather}</span>
                    </div>
                    <div style='text-align: center; width: 48%; background: #f9f9f9; padding: 15px; border-radius: 8px;'>
                        <span style='font-size: 28px; display: block; font-weight: bold; color: {GetAqiColor(weather.AQI)};'>{weather.AQI}</span>
                        <span style='color: #666; font-size: 14px;'>Air Quality Index</span>
                    </div>
                </div>

                <hr style='border: 0; border-top: 1px dashed #ddd; margin: 20px 0;'>

                <h3 style='margin-top: 0; color: #2c3e50; font-size: 18px;'>💡 Our Recommendation</h3>
                <div style='background-color: #e8f5e9; border-left: 5px solid #4CAF50; padding: 15px; border-radius: 4px; font-style: italic; color: #555;'>
                    ""{aiAdvice}""
                </div>

                <p style='text-align: center; margin-top: 30px; font-size: 16px; color: #777;'>
                    Have a wonderful day ahead! 🌟
                </p>

            </div>

            <div style='background-color: #f5f5f5; text-align: center; padding: 15px; font-size: 12px; color: #999;'>
                Sent with ❤️ by WeatherApp • <a href='#' style='color: #999; text-decoration: underline;'>Unsubscribe</a>
            </div>
        </div>";

        await SendEmailAsync(toEmail, subject, body);
    }

    // Small helper to color-code the AQI number
    private string GetAqiColor(int aqi)
    {
        return aqi switch
        {
            1 => "#2ecc71", // Green (Good)
            2 => "#f1c40f", // Yellow (Fair)
            3 => "#e67e22", // Orange (Moderate)
            4 => "#e74c3c", // Red (Poor)
            _ => "#c0392b"  // Dark Red (Hazardous)
        };
    }
}