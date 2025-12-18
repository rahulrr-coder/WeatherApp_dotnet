using System.Net;
using System.Net.Mail;
using System.Text.Json; // Added for parsing
using WeatherApp.Models;

namespace WeatherApp.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendWeatherEmailAsync(string toEmail, WeatherModel weather, string aiAdvice);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromEmail = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];

        if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("❌ Email credentials missing.");
            return;
        }

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, "Atmosphere Daily"), // Added Display Name
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        try 
        {
            await smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine($"✅ Email sent to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email: {ex.Message}");
        }
    }

    public async Task SendWeatherEmailAsync(string toEmail, WeatherModel weather, string aiAdvice)
    {
        var subject = $"The Atmosphere in {weather.City} ☁️";
        
        // 1. Parse the AI JSON (Summary, Outfit, Safety)
        string summary = "Enjoy your day.";
        string outfit = "Dress comfortably.";
        string safety = "No specific hazards.";

        try 
        {
            var cleanJson = aiAdvice.Replace("```json", "").Replace("```", "").Trim();
            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("summary", out var s)) summary = s.GetString() ?? summary;
            if (root.TryGetProperty("outfit", out var o)) outfit = o.GetString() ?? outfit;
            if (root.TryGetProperty("safety", out var safe)) safety = safe.GetString() ?? safety;
        }
        catch 
        {
            // Fallback: Use the raw text if parsing fails
            summary = aiAdvice; 
        }

        
        var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ margin: 0; padding: 0; font-family: 'Georgia', serif; background-color: #f4f4f4; }}
                .container {{ max-width: 600px; margin: 40px auto; background-color: #fdfbf7; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.05); }}
                .header {{ background-color: #1a1a1a; color: white; padding: 40px 30px; text-align: center; }}
                .logo {{ font-size: 24px; font-weight: bold; letter-spacing: -1px; margin-bottom: 10px; display: block; }}
                .city-title {{ font-size: 36px; margin: 0; font-weight: normal; }}
                .content {{ padding: 40px 30px; color: #333; }}
                .temp-hero {{ font-size: 64px; font-weight: bold; text-align: center; margin: 20px 0; color: #1a1a1a; }}
                .temp-sub {{ font-size: 18px; text-align: center; color: #666; font-style: italic; margin-bottom: 40px; display: block; }}
                
                .grid {{ display: flex; border-top: 1px solid #eee; border-bottom: 1px solid #eee; padding: 20px 0; margin-bottom: 40px; }}
                .grid-item {{ flex: 1; text-align: center; border-right: 1px solid #eee; }}
                .grid-item:last-child {{ border-right: none; }}
                .label {{ font-family: 'Helvetica', sans-serif; font-size: 10px; letter-spacing: 1px; color: #999; text-transform: uppercase; display: block; margin-bottom: 5px; }}
                .value {{ font-family: 'Helvetica', sans-serif; font-size: 16px; font-weight: bold; color: #333; }}

                .section-title {{ font-family: 'Helvetica', sans-serif; font-size: 12px; letter-spacing: 2px; color: #d4a373; text-transform: uppercase; font-weight: bold; margin-bottom: 15px; border-bottom: 2px solid #d4a373; display: inline-block; padding-bottom: 5px; }}
                .advice-box {{ margin-bottom: 30px; }}
                .advice-text {{ font-size: 16px; line-height: 1.6; color: #444; margin: 0 0 10px 0; }}
                
                .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #aaa; font-family: 'Helvetica', sans-serif; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <span class='logo'>Atmosphere.</span>
                    <h1 class='city-title'>{weather.City}</h1>
                </div>
                
                <div class='content'>
                    <div class='temp-hero'>{weather.CurrentTemp:F0}°</div>
                    <span class='temp-sub'>{weather.CurrentCondition}</span>

                    <div class='grid'>
                        <div class='grid-item'>
                            <span class='label'>Humidity</span>
                            <span class='value'>{weather.Humidity}%</span>
                        </div>
                        <div class='grid-item'>
                            <span class='label'>Wind</span>
                            <span class='value'>{weather.WindSpeed} km/h</span>
                        </div>
                        <div class='grid-item'>
                            <span class='label'>AQI</span>
                            <span class='value'>{weather.AQI}</span>
                        </div>
                    </div>

                    <div class='advice-box'>
                        <div class='section-title'>Daily Briefing</div>
                        <p class='advice-text' style='font-size: 18px; font-style: italic;'>""{summary}""</p>
                    </div>

                    <div class='advice-box'>
                        <div class='section-title'>Outfit</div>
                        <p class='advice-text'>{outfit}</p>
                    </div>

                    <div class='advice-box'>
                        <div class='section-title'>Advisory</div>
                        <p class='advice-text'>{safety}</p>
                    </div>
                </div>

                <div class='footer'>
                    Have a wonderful day.<br>
                    <a href='http://localhost:5173' style='color: #d4a373; text-decoration: none;'>View Dashboard</a>
                </div>
            </div>
        </body>
        </html>";

        await SendEmailAsync(toEmail, subject, body);
    }
}