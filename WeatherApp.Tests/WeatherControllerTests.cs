using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherApp.Controllers;
using WeatherApp.Models;
using WeatherApp.Services;
using Xunit;

namespace WeatherApp.Tests;

public class WeatherControllerTests
{
    // TEST 1: SUCCESS SCENARIO
    [Fact]
    public async Task Get_ShouldReturnOk_WhenCityIsFound()
    {
        // 1. ARRANGE (Setup)
        // Create a "Fake" Service
        var mockService = new Mock<IWeatherService>();
        
        // Teach the fake service what to do
        var expectedWeather = new WeatherModel 
        { 
            City = "London", 
            Temperature = 20, 
            Weather = "Sunny" 
        };
        
        mockService.Setup(s => s.GetWeatherForCity("London"))
                   .ReturnsAsync(expectedWeather);

        // Inject the fake service into the controller
        var controller = new WeatherController(mockService.Object);

        // 2. ACT (Run the method)
        var result = await controller.Get("London");

        // 3. ASSERT (Verify)
        // Did we get a 200 OK?
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        // Did we get the right data?
        var returnedData = Assert.IsType<WeatherModel>(okResult.Value);
        Assert.Equal("London", returnedData.City);
        Assert.Equal(20, returnedData.Temperature);
    }

    // TEST 2: FAILURE SCENARIO (City Not Found)
    [Fact]
    public async Task Get_ShouldReturnBadRequest_WhenCityNotFound()
    {
        // 1. ARRANGE
        var mockService = new Mock<IWeatherService>();
        
        // Your controller logic checks if Weather property is "City Not Found"
        var errorData = new WeatherModel 
        { 
            City = "UnknownCity", 
            Weather = "City Not Found" 
        };

        mockService.Setup(s => s.GetWeatherForCity("UnknownCity"))
                   .ReturnsAsync(errorData);

        var controller = new WeatherController(mockService.Object);

        // 2. ACT
        var result = await controller.Get("UnknownCity");

        // 3. ASSERT
        // Should return 400 Bad Request
        Assert.IsType<BadRequestObjectResult>(result);
    }
}