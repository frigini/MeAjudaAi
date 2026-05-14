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
    private IUnitOfWork _uow = null!;
    private IAllowedCityQueries _queries = null!;
    private LocationsDbContext _context = null!;

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
        var options = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        _context = new LocationsDbContext(options);
        await _context.Database.MigrateAsync();

        _uow = _context;
        _queries = new DbContextAllowedCityQueries(_context);
    }

    private IRepository<AllowedCity, Guid> GetRepository() => _uow.GetRepository<AllowedCity, Guid>();

    [Fact]
    public async Task Add_WithValidCity_ShouldPersistCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);

        // Act
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        var savedCity = await _context.AllowedCities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == city.Id);
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
        // Arrange
        var activeCity1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var activeCity2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
        var inactiveCity = new AllowedCity("São Paulo", "SP", "admin@test.com", 3550308, 0, 0, 0, false);

        GetRepository().Add(activeCity1);
        GetRepository().Add(activeCity2);
        GetRepository().Add(inactiveCity);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.GetAllActiveAsync();

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
        var activeCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var inactiveCity = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0, false);

        GetRepository().Add(activeCity);
        GetRepository().Add(inactiveCity);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.IsActive);
        result.Should().Contain(c => !c.IsActive);
    }

    [Fact]
    public async Task TryFindAsync_WithExistingCity_ShouldReturnCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        var result = await GetRepository().TryFindAsync(city.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(city.Id);
        result.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
        result.IbgeCode.Should().Be(3143906);
    }

    [Fact]
    public async Task TryFindAsync_WithNonExistingCity_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await GetRepository().TryFindAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithExistingCityAndState_ShouldReturnCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.GetByCityAndStateAsync("Muriaé", "MG");

        // Assert
        result.Should().NotBeNull();
        result!.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithNonExistingCityAndState_ShouldReturnNull()
    {
        // Act
        var result = await _queries.GetByCityAndStateAsync("Não Existe", "XX");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithActiveCity_ShouldReturnTrue()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.IsCityAllowedAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithInactiveCity_ShouldReturnFalse()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0, false);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.IsCityAllowedAsync("Muriaé", "MG");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        // Act
        var result = await _queries.IsCityAllowedAsync("Não Existe", "XX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        city.Update("Itaperuna", "RJ", 3302270, 0, 0, 0, true, "admin2@test.com");
        await _uow.SaveChangesAsync();

        // Assert
        var updatedCity = await _context.AllowedCities.FirstOrDefaultAsync(c => c.Id == city.Id);
        updatedCity.Should().NotBeNull();
        updatedCity!.CityName.Should().Be("Itaperuna");
        updatedCity.StateSigla.Should().Be("RJ");
        updatedCity.IbgeCode.Should().Be(3302270);
        updatedCity.UpdatedBy.Should().Be("admin2@test.com");
        updatedCity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_WithExistingCity_ShouldRemoveCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        GetRepository().Delete(city);
        await _uow.SaveChangesAsync();

        // Assert
        var deletedCity = await _context.AllowedCities.FirstOrDefaultAsync(c => c.Id == city.Id);
        deletedCity.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingCity_ShouldReturnTrue()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        // Act
        var result = await _queries.ExistsAsync("Não Existe", "XX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOrderedByCityNameAndState()
    {
        // Arrange
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var city2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
        var city3 = new AllowedCity("Bom Jesus do Itabapoana", "RJ", "admin@test.com", 3300704, 0, 0, 0);

        GetRepository().Add(city1);
        GetRepository().Add(city2);
        GetRepository().Add(city3);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(3);
        // Repository orders by StateSigla first, then CityName
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
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city);
        await _uow.SaveChangesAsync();

        // Act
        var result1 = await _queries.GetByCityAndStateAsync("MURIAÉ", "mg");
        var result2 = await _queries.GetByCityAndStateAsync("muriaé", "MG");

        // Assert
        result1.Should().NotBeNull();
        result1!.CityName.Should().Be("Muriaé");
        result2.Should().NotBeNull();
        result2!.CityName.Should().Be("Muriaé");
    }

    [Fact]
    public async Task Add_WithDuplicateCityAndState_ShouldThrowException()
    {
        // Arrange
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city1);
        await _uow.SaveChangesAsync();

        var city2 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);

        // Act
        var act = async () =>
        {
            GetRepository().Add(city2);
            await _uow.SaveChangesAsync();
        };

        // Assert
        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        var postgresException = exception.Which.InnerException as Npgsql.PostgresException;
        postgresException.Should().NotBeNull();
        postgresException!.SqlState.Should().Be("23505"); // UniqueViolation
    }

    [Fact]
    public async Task Add_WithDuplicateIbgeCode_ShouldThrowException()
    {
        // Arrange
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        GetRepository().Add(city1);
        await _uow.SaveChangesAsync();

        var city2 = new AllowedCity("Outra Cidade", "SP", "admin@test.com", 3143906, 0, 0, 0);

        // Act
        var act = async () =>
        {
            GetRepository().Add(city2);
            await _uow.SaveChangesAsync();
        };

        // Assert
        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        var postgresException = exception.Which.InnerException as Npgsql.PostgresException;
        postgresException.Should().NotBeNull();
        postgresException!.SqlState.Should().Be("23505"); // UniqueViolation
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
