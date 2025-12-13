using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Controllers;
using WeatherApp.Data;
using WeatherApp.Models;
using Xunit; // The testing framework

namespace WeatherApp.Tests;

public class FavoritesControllerTests
{
    // --- HELPER METHOD ---
    // Creates a fresh, unique "RAM Database" for every single test.
    // This ensures Test A doesn't accidentally mess up data for Test B.
    private WeatherDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new WeatherDbContext(options);
    }

    // --- TEST 1: ADD (Success) ---
    [Fact] 
    public async Task Add_ShouldSaveCity_WhenCityIsNew()
    {
        // 1. Arrange (Setup)
        var db = GetInMemoryDb();
        var controller = new FavoritesController(db);
        var newCity = new FavoriteCity { Name = "Tokyo" };

        // 2. Act (The Logic)
        var result = await controller.Add(newCity);

        // 3. Assert (The Verification)
        // Did the API return "200 OK"?
        Assert.IsType<OkObjectResult>(result);

        // Did it actually save to the database?
        var cityInDb = await db.Favorites.FirstOrDefaultAsync(c => c.Name == "Tokyo");
        Assert.NotNull(cityInDb); // It should exist
        Assert.Equal("Tokyo", cityInDb.Name); // The name should match
    }

    // --- TEST 2: ADD (Duplicate Error) ---
    [Fact]
    public async Task Add_ShouldFail_WhenCityAlreadyExists()
    {
        // 1. Arrange
        var db = GetInMemoryDb();
        // Pre-fill the DB with "Paris" to simulate it already being there
        db.Favorites.Add(new FavoriteCity { Name = "Paris" });
        await db.SaveChangesAsync();

        var controller = new FavoritesController(db);
        var duplicateCity = new FavoriteCity { Name = "Paris" };

        // 2. Act
        var result = await controller.Add(duplicateCity);

        // 3. Assert
        // Should return "409 Conflict", not "200 OK"
        Assert.IsType<ConflictObjectResult>(result);
        
        // The count should still be 1 (Paris), not 2 (Paris, Paris)
        Assert.Equal(1, await db.Favorites.CountAsync());
    }

    // --- TEST 3: GET ALL ---
    [Fact]
    public async Task GetAll_ShouldReturnAllCities()
    {
        // 1. Arrange
        var db = GetInMemoryDb();
        // Add two dummy cities
        db.Favorites.Add(new FavoriteCity { Name = "Berlin" });
        db.Favorites.Add(new FavoriteCity { Name = "Mumbai" });
        await db.SaveChangesAsync();

        var controller = new FavoritesController(db);

        // 2. Act
        var result = await controller.GetAll();

        // 3. Assert
        // We expect an "OkObjectResult" which contains the list
        var okResult = Assert.IsType<OkObjectResult>(result);
        var cities = Assert.IsType<List<FavoriteCity>>(okResult.Value);
        
        // We put in 2 cities, so we should get 2 back
        Assert.Equal(2, cities.Count);
    }

    // --- TEST 4: DELETE (Success) ---
    [Fact]
    public async Task Delete_ShouldRemoveCity_WhenIdExists()
    {
        // 1. Arrange
        var db = GetInMemoryDb();
        var city = new FavoriteCity { Name = "Rome" };
        db.Favorites.Add(city);
        await db.SaveChangesAsync(); // Saves "Rome" with ID=1 (usually)

        var controller = new FavoritesController(db);

        // 2. Act
        // We use the ID from the saved city to ensure we delete the right one
        var result = await controller.Delete(city.Id);

        // 3. Assert
        Assert.IsType<OkResult>(result); // Should return 200 OK
        Assert.Empty(db.Favorites);      // The list should now be empty
    }

    // --- TEST 5: DELETE (Not Found) ---
    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // 1. Arrange
        var db = GetInMemoryDb(); // Empty database
        var controller = new FavoritesController(db);

        // 2. Act
        // Try to delete ID 999, which definitely doesn't exist
        var result = await controller.Delete(999);

        // 3. Assert
        // Should return "404 Not Found"
        Assert.IsType<NotFoundResult>(result);
    }
}