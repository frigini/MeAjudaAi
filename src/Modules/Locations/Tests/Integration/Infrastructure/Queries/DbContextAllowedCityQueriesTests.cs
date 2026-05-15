using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure.Queries;

public class DbContextAllowedCityQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private LocationsDbContext _context = null!;
    private IAllowedCityQueries _queries = null!;

    public DbContextAllowedCityQueriesTests()
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

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveCities()
    {
        var activeCity1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var activeCity2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
        var inactiveCity = new AllowedCity("São Paulo", "SP", "admin@test.com", 3550308, 0, 0, 0, false);

        _context.AllowedCities.Add(activeCity1);
        _context.AllowedCities.Add(activeCity2);
        _context.AllowedCities.Add(inactiveCity);
        await _context.SaveChangesAsync();

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
        var activeCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var inactiveCity = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0, false);

        _context.AllowedCities.Add(activeCity);
        _context.AllowedCities.Add(inactiveCity);
        await _context.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.IsActive);
        result.Should().Contain(c => !c.IsActive);
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithExistingCityAndState_ShouldReturnCity()
    {
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        _context.AllowedCities.Add(city);
        await _context.SaveChangesAsync();

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
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        _context.AllowedCities.Add(city);
        await _context.SaveChangesAsync();

        var result = await _queries.IsCityAllowedAsync("Muriaé", "MG");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithInactiveCity_ShouldReturnFalse()
    {
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0, false);
        _context.AllowedCities.Add(city);
        await _context.SaveChangesAsync();

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
    public async Task ExistsAsync_WithExistingCity_ShouldReturnTrue()
    {
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        _context.AllowedCities.Add(city);
        await _context.SaveChangesAsync();

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
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var city2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
        var city3 = new AllowedCity("Bom Jesus do Itabapoana", "RJ", "admin@test.com", 3300704, 0, 0, 0);

        _context.AllowedCities.Add(city1);
        _context.AllowedCities.Add(city2);
        _context.AllowedCities.Add(city3);
        await _context.SaveChangesAsync();

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
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        _context.AllowedCities.Add(city);
        await _context.SaveChangesAsync();

        var result1 = await _queries.GetByCityAndStateAsync("MURIAÉ", "mg");
        var result2 = await _queries.GetByCityAndStateAsync("muriaé", "MG");

        result1.Should().NotBeNull();
        result1!.CityName.Should().Be("Muriaé");
        result2.Should().NotBeNull();
        result2!.CityName.Should().Be("Muriaé");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithNullCityName_ShouldReturnNull()
    {
        var result = await _queries.GetByCityAndStateAsync(null!, "MG");

        result.Should().BeNull("normalização de null deve resultar em string vazia e não encontrar registro");
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithNullCityName_ShouldReturnFalse()
    {
        var result = await _queries.IsCityAllowedAsync(null!, "MG");

        result.Should().BeFalse("normalização de null deve resultar em string vazia");
    }

    [Fact]
    public async Task ExistsAsync_WithNullCityName_ShouldReturnFalse()
    {
        var result = await _queries.ExistsAsync(null!, "MG");

        result.Should().BeFalse("normalização de null deve resultar em string vazia");
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var dbContextOptions = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        _context = new LocationsDbContext(dbContextOptions);
        await _context.Database.MigrateAsync();

        _queries = new DbContextAllowedCityQueries(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}