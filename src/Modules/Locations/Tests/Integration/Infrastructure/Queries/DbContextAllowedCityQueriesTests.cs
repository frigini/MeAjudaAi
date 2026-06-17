using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure.Queries;

public class DbContextAllowedCityQueriesTests : BaseDatabaseTest
{
    private LocationsDbContext _context = null!;
    private IAllowedCityQueries _queries = null!;

    public DbContextAllowedCityQueriesTests()
        : base(schema: "locations", databaseName: "locations_test")
    {
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var dbContextOptions = CreateDbContextOptions<LocationsDbContext>();
        _context = new LocationsDbContext(dbContextOptions);
        await _context.Database.MigrateAsync();
        await InitializeRespawnerAsync();

        _queries = new DbContextAllowedCityQueries(_context);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_context != null)
            await _context.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveCities()
    {
        var activeCity1 = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        var activeCity2 = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").Build();
        var inactiveCity = AllowedCityBuilder.AsTestCity("São Paulo", "SP").AsInactive().Build();

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
        var activeCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        var inactiveCity = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").AsInactive().Build();

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
        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
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
        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
        _context.AllowedCities.Add(city);
        await _context.SaveChangesAsync();

        var result = await _queries.IsCityAllowedAsync("Muriaé", "MG");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithInactiveCity_ShouldReturnFalse()
    {
        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").AsInactive().Build();
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
        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").Build();
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
        var city1 = AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build();
        var city2 = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").WithIbgeCode(3302270).Build();
        var city3 = AllowedCityBuilder.AsTestCity("Bom Jesus do Itabapoana", "RJ").WithIbgeCode(3300704).Build();

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
        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build();
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
}
