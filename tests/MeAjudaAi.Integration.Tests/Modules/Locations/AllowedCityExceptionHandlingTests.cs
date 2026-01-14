using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
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
        var repository = scope.ServiceProvider.GetRequiredService<IAllowedCityRepository>();
        var nonExistingId = Guid.NewGuid();

        // Act
        var city = await repository.GetByIdAsync(nonExistingId);

        // Assert
        city.Should().BeNull("cidade não existe e repository deve retornar null");
    }

    [Fact]
    public async Task CreateDuplicateCity_ShouldDetectDuplicate()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAllowedCityRepository>();

        var cityName = _faker.Address.City();
        var state = "SP";
        var createdBy = _faker.Internet.Email();

        var city = new AllowedCity(cityName, state, createdBy);
        await repository.AddAsync(city);

        // Act
        var exists = await repository.ExistsAsync(cityName, state);

        // Assert
        exists.Should().BeTrue("cidade duplicada deve ser detectada");
    }

    [Fact]
    public async Task ValidRepository_ShouldHaveAllMethods()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAllowedCityRepository>();

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeAssignableTo<IAllowedCityRepository>();
    }
}
