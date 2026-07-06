using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Integration;

[Collection("LocationsIntegrationTests")]
public class AllowedCityILikeIntegrationTests : LocationsIntegrationTestBase
{
    [Fact]
    public async Task GetByCityAndStateAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var city = new AllowedCity(
            cityName: "Muriaé",
            stateSigla: "MG",
            createdBy: "test",
            latitude: -21.1353,
            longitude: -42.3696,
            isActive: true);

        using (var arrangeScope = CreateScope())
        {
            var dbContext = arrangeScope.ServiceProvider.GetRequiredService<LocationsDbContext>();
            dbContext.AllowedCities.Add(city);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using (var actScope = CreateScope())
        {
            var queries = actScope.ServiceProvider.GetRequiredService<Application.Queries.Interfaces.IAllowedCityQueries>();

            var lowerResult = await queries.GetByCityAndStateAsync("muriaé", "MG");
            var upperResult = await queries.GetByCityAndStateAsync("MURIAÉ", "MG");
            var mixedResult = await queries.GetByCityAndStateAsync("MuRiAé", "MG");

            // Assert
            lowerResult.Should().NotBeNull();
            lowerResult!.CityName.Should().Be("Muriaé");

            upperResult.Should().NotBeNull();
            upperResult!.CityName.Should().Be("Muriaé");

            mixedResult.Should().NotBeNull();
            mixedResult!.CityName.Should().Be("Muriaé");
        }
    }

    [Fact]
    public async Task ILike_ShouldMatchPartialNames()
    {
        // Arrange
        var cities = new[]
        {
            new AllowedCity("São Paulo", "SP", "test", latitude: -23.5505, longitude: -46.6333, isActive: true),
            new AllowedCity("São José dos Campos", "SP", "test", latitude: -23.1791, longitude: -45.8872, isActive: true),
            new AllowedCity("São Bernardo do Campo", "SP", "test", latitude: -23.6914, longitude: -46.5646, isActive: true)
        };

        using (var arrangeScope = CreateScope())
        {
            var dbContext = arrangeScope.ServiceProvider.GetRequiredService<LocationsDbContext>();
            dbContext.AllowedCities.AddRange(cities);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using (var actScope = CreateScope())
        {
            var context = actScope.ServiceProvider.GetRequiredService<LocationsDbContext>();

            var results = await context.AllowedCities
                .Where(x => EF.Functions.ILike(x.CityName, "%São%"))
                .ToListAsync();

            // Assert
            results.Should().HaveCount(3);
            results.Should().AllSatisfy(x => x.StateSigla.Should().Be("SP"));
        }
    }

    [Fact]
    public async Task GetByCityAndStateAsync_ShouldReturnNull_WhenNoMatch()
    {
        // Arrange
        var city = new AllowedCity(
            cityName: "Juiz de Fora",
            stateSigla: "MG",
            createdBy: "test",
            latitude: -21.7597,
            longitude: -43.3398,
            isActive: true);

        using (var arrangeScope = CreateScope())
        {
            var dbContext = arrangeScope.ServiceProvider.GetRequiredService<LocationsDbContext>();
            dbContext.AllowedCities.Add(city);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using (var actScope = CreateScope())
        {
            var queries = actScope.ServiceProvider.GetRequiredService<Application.Queries.Interfaces.IAllowedCityQueries>();

            var result = await queries.GetByCityAndStateAsync("NonExistentCity", "MG");

            // Assert
            result.Should().BeNull();
        }
    }
}
