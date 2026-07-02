using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure.Queries;

public class DbContextAllowedCityQueriesTests : LocationsIntegrationTestBase
{
    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveCities()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var activeCity1 = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        var activeCity2 = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").Build();
        var inactiveCity = AllowedCityBuilder.AsTestCity("São Paulo", "SP").AsInactive().Build();

        context.AllowedCities.Add(activeCity1);
        context.AllowedCities.Add(activeCity2);
        context.AllowedCities.Add(inactiveCity);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());
        result.Should().Contain(c => c.CityName == "Muriaé");
        result.Should().Contain(c => c.CityName == "Itaperuna");
        result.Should().NotContain(c => c.CityName == "São Paulo");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCities()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var activeCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        var inactiveCity = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").AsInactive().Build();

        context.AllowedCities.Add(activeCity);
        context.AllowedCities.Add(inactiveCity);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.IsActive);
        result.Should().Contain(c => !c.IsActive);
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithExistingCityAndState_ShouldReturnCity()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        context.AllowedCities.Add(city);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.GetByCityAndStateAsync("Muriaé", "MG");

        // Assert
        result.Should().NotBeNull();
        result!.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithNonExistingCityAndState_ShouldReturnNull()
    {
        // Arrange
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        // Act
        var result = await queries.GetByCityAndStateAsync("Não Existe", "XX");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithActiveCity_ShouldReturnTrue()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        context.AllowedCities.Add(city);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.IsCityAllowedAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithInactiveCity_ShouldReturnFalse()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").AsInactive().Build();
        context.AllowedCities.Add(city);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.IsCityAllowedAsync("Muriaé", "MG");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        // Arrange
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        // Act
        var result = await queries.IsCityAllowedAsync("Não Existe", "XX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingCity_ShouldReturnTrue()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        context.AllowedCities.Add(city);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.ExistsAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        // Arrange
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        // Act
        var result = await queries.ExistsAsync("Não Existe", "XX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOrderedByStateAndCityName()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var city1 = AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build();
        var city2 = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").WithIbgeCode(3302270).Build();
        var city3 = AllowedCityBuilder.AsTestCity("Bom Jesus do Itabapoana", "RJ").WithIbgeCode(3300704).Build();

        context.AllowedCities.Add(city1);
        context.AllowedCities.Add(city2);
        context.AllowedCities.Add(city3);
        await context.SaveChangesAsync();

        // Act
        var result = await queries.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].StateSigla.Should().Be("MG");
        result[0].CityName.Should().Be("Muriaé");
        result[1].StateSigla.Should().Be("RJ");
        result[1].CityName.Should().Be("Bom Jesus do Itabapoana");
        result[2].StateSigla.Should().Be("RJ");
        result[2].CityName.Should().Be("Itaperuna");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build();
        context.AllowedCities.Add(city);
        await context.SaveChangesAsync();

        // Act
        var result1 = await queries.GetByCityAndStateAsync("MURIAÉ", "mg");
        var result2 = await queries.GetByCityAndStateAsync("muriaé", "MG");

        // Assert
        result1.Should().NotBeNull();
        result1!.CityName.Should().Be("Muriaé");
        result2.Should().NotBeNull();
        result2!.CityName.Should().Be("Muriaé");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithNullCityName_ShouldReturnNull()
    {
        // Arrange
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        // Act
        var result = await queries.GetByCityAndStateAsync(null!, "MG");

        // Assert
        result.Should().BeNull("normalização de null deve resultar em string vazia e não encontrar registro");
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithNullCityName_ShouldReturnFalse()
    {
        // Arrange
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        // Act
        var result = await queries.IsCityAllowedAsync(null!, "MG");

        // Assert
        result.Should().BeFalse("normalização de null deve resultar em string vazia");
    }

    [Fact]
    public async Task ExistsAsync_WithNullCityName_ShouldReturnFalse()
    {
        // Arrange
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        // Act
        var result = await queries.ExistsAsync(null!, "MG");

        // Assert
        result.Should().BeFalse("normalização de null deve resultar em string vazia");
    }
}
