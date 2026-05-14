using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Integration;

public class LocationsPersistenceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private LocationsDbContext _context = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IAllowedCityQueries _queries = null!;

    public LocationsPersistenceIntegrationTests()
    {
        var options = new TestDatabaseOptions
        {
            DatabaseName = "locations_test",
            Username = "test_user",
            Password = "test_password",
            Schema = "locations"
        };

        _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
            .WithDatabase(options.DatabaseName)
            .WithUsername(options.Username)
            .WithPassword(options.Password)
            .WithCleanUp(true)
            .Build();
    }

    private async Task InitializeInternalAsync()
    {
        var dbContextOptions = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        _context = new LocationsDbContext(dbContextOptions);
        await _context.Database.MigrateAsync();

        _unitOfWork = _context;
        _queries = new DbContextAllowedCityQueries(_context);
    }

    [Fact]
    public async Task Add_WithValidCity_ShouldPersistCity()
    {
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);

        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var savedCity = await _context.AllowedCities.FirstOrDefaultAsync(c => c.Id == city.Id);
        savedCity.Should().NotBeNull();
        savedCity!.CityName.Should().Be("Muriaé");
        savedCity.StateSigla.Should().Be("MG");
        savedCity.IbgeCode.Should().Be(3143906);
        savedCity.IsActive.Should().BeTrue();
        savedCity.CreatedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveCities()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var activeCity1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var activeCity2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
        var inactiveCity = new AllowedCity("São Paulo", "SP", "admin@test.com", 3550308, 0, 0, 0, false);

        repository.Add(activeCity1);
        repository.Add(activeCity2);
        repository.Add(inactiveCity);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.GetAllActiveAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());
        result.Should().Contain(c => c.CityName == "Muriaé");
        result.Should().Contain(c => c.CityName == "Itaperuna");
        result.Should().NotContain(c => c.CityName == "São Paulo");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCities()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var activeCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var inactiveCity = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0, false);

        repository.Add(activeCity);
        repository.Add(inactiveCity);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.IsActive);
        result.Should().Contain(c => !c.IsActive);
    }

    [Fact]
    public async Task TryFindAsync_WithExistingCity_ShouldReturnCity()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var result = await repository.TryFindAsync(city.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(city.Id);
        result.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
        result.IbgeCode.Should().Be(3143906);
    }

    [Fact]
    public async Task TryFindAsync_WithNonExistingCity_ShouldReturnNull()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();
        var nonExistingId = Guid.NewGuid();

        var result = await repository.TryFindAsync(nonExistingId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithExistingCityAndState_ShouldReturnCity()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.GetByCityAndStateAsync("Muriaé", "MG");

        result.Should().NotBeNull();
        result!.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithNonExistingCityAndState_ShouldReturnNull()
    {
        var result = await _queries.GetByCityAndStateAsync("Não Existe", "XX");

        result.Should().BeNull();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithActiveCity_ShouldReturnTrue()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.IsCityAllowedAsync("Muriaé", "MG");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithInactiveCity_ShouldReturnFalse()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0, false);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.IsCityAllowedAsync("Muriaé", "MG");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        var result = await _queries.IsCityAllowedAsync("Não Existe", "XX");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithExistingCity_ShouldRemoveCity()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        repository.Delete(city);
        await _unitOfWork.SaveChangesAsync();

        var deletedCity = await _context.AllowedCities.FirstOrDefaultAsync(c => c.Id == city.Id);
        deletedCity.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingCity_ShouldReturnTrue()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.ExistsAsync("Muriaé", "MG");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        var result = await _queries.ExistsAsync("Não Existe", "XX");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOrderedByStateAndCityName()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var city2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
        var city3 = new AllowedCity("Bom Jesus do Itabapoana", "RJ", "admin@test.com", 3300704, 0, 0, 0);

        repository.Add(city1);
        repository.Add(city2);
        repository.Add(city3);
        await _unitOfWork.SaveChangesAsync();

        var result = await _queries.GetAllActiveAsync();

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
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city);
        await _unitOfWork.SaveChangesAsync();

        var result1 = await _queries.GetByCityAndStateAsync("MURIAÉ", "mg");
        var result2 = await _queries.GetByCityAndStateAsync("muriaé", "MG");

        result1.Should().NotBeNull();
        result1!.CityName.Should().Be("Muriaé");
        result2.Should().NotBeNull();
        result2!.CityName.Should().Be("Muriaé");
    }

    [Fact]
    public async Task Add_WithDuplicateCityAndState_ShouldThrowException()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city1);
        await _unitOfWork.SaveChangesAsync();

        var city2 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);

        var act = async () =>
        {
            repository.Add(city2);
            await _unitOfWork.SaveChangesAsync();
        };

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Add_WithDuplicateIbgeCode_ShouldThrowException()
    {
        var repository = _unitOfWork.GetRepository<AllowedCity, Guid>();

        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        repository.Add(city1);
        await _unitOfWork.SaveChangesAsync();

        var city2 = new AllowedCity("Outra Cidade", "SP", "admin@test.com", 3143906, 0, 0, 0);

        var act = async () =>
        {
            repository.Add(city2);
            await _unitOfWork.SaveChangesAsync();
        };

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await InitializeInternalAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}