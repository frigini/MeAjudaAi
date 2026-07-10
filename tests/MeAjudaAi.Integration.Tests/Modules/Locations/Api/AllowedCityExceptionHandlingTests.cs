using Bogus;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;

namespace MeAjudaAi.Integration.Tests.Modules.Locations.Api;

public class AllowedCityExceptionHandlingTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations;

    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task GetNonExistingCity_ShouldReturnNull()
    {
        using var scope = Services.CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();
        var nonExistingId = Guid.NewGuid();

        var city = await queries.GetByIdAsync(nonExistingId);

        city.Should().BeNull("cidade não existe e queries deve retornar null");
    }

    [Fact]
    public async Task GetExistingCity_ShouldReturnTrue_WhenExists()
    {
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.Locations);
        var queries = scope.ServiceProvider.GetRequiredService<IAllowedCityQueries>();
        var repository = uow.GetRepository<AllowedCity, Guid>();

        var cityName = _faker.Address.City();
        var state = "SP";
        var createdBy = _faker.Internet.Email();

        var city = new AllowedCity(cityName, state, createdBy);
        repository.Add(city);
        await uow.SaveChangesAsync();

        var exists = await queries.ExistsAsync(cityName, state);

        exists.Should().BeTrue("existent city should be detected by ExistsAsync");
    }

    [Fact]
    public async Task ValidRepository_ShouldHaveAllMethods()
    {
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.Locations);
        var repository = uow.GetRepository<AllowedCity, Guid>();

        repository.Should().NotBeNull();
        repository.Should().BeAssignableTo<IRepository<AllowedCity, Guid>>();
    }
}