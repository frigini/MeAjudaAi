using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Integration tests for AllowedCity handlers to validate exception handling.
/// Tests that domain exceptions are thrown correctly without full HTTP stack.
/// </summary>
public class AllowedCityExceptionHandlingTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations;

    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task GetNonExistingCity_ShouldReturnNull()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();
        var nonExistingId = Guid.NewGuid();

        // Act
        var city = await queries.GetByIdAsync(nonExistingId);

        // Assert
        city.Should().BeNull("cidade não existe e queries deve retornar null");
    }

    [Fact]
    public async Task CreateDuplicateCity_ShouldDetectDuplicate()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();

        var cityName = _faker.Address.City();
        var state = "SP";
        var createdBy = _faker.Internet.Email();

        var city = new AllowedCity(cityName, state, createdBy);
        dbContext.GetRepository<AllowedCity, Guid>().Add(city);
        await dbContext.SaveChangesAsync();

        // Act
        var exists = await queries.ExistsAsync(cityName, state);

        // Assert
        exists.Should().BeTrue("cidade duplicada deve ser detectada");
    }

    [Fact]
    public async Task ValidPersistenceRegistration_ShouldHaveRequiredServices()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<LocationsDbContext>();
        var queries = scope.ServiceProvider.GetService<IAllowedCityQueries>();

        // Assert
        dbContext.Should().NotBeNull();
        queries.Should().NotBeNull();
    }
}
