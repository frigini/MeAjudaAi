using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Repositories;
using MeAjudaAi.Shared.Tests.Base;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Integration;

public class AllowedCityRepositoryIntegrationTests : DatabaseTestBase
{
    private AllowedCityRepository _repository = null!;
    private LocationsDbContext _context = null!;

    public AllowedCityRepositoryIntegrationTests() : base(new TestDatabaseOptions
    {
        DatabaseName = "locations_test",
        Username = "test_user",
        Password = "test_password",
        Schema = "locations"
    })
    {
    }

    private async Task InitializeInternalAsync()
    {
        await base.InitializeAsync();

        var options = new DbContextOptionsBuilder<LocationsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _context = new LocationsDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new AllowedCityRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WithValidCity_ShouldPersistCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);

        // Act
        await AddCityAndSaveAsync(city);

        // Assert
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
        // Arrange
        var activeCity1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        var activeCity2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270);
        var inactiveCity = new AllowedCity("São Paulo", "SP", "admin@test.com", 3550308, false);

        await AddCityAndSaveAsync(activeCity1);
        await AddCityAndSaveAsync(activeCity2);
        await AddCityAndSaveAsync(inactiveCity);

        // Act
        var result = await _repository.GetAllActiveAsync();

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
        var activeCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        var inactiveCity = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, false);

        await AddCityAndSaveAsync(activeCity);
        await AddCityAndSaveAsync(inactiveCity);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.IsActive);
        result.Should().Contain(c => !c.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCity_ShouldReturnCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        var result = await _repository.GetByIdAsync(city.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(city.Id);
        result.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
        result.IbgeCode.Should().Be(3143906);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingCity_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithExistingCityAndState_ShouldReturnCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        var result = await _repository.GetByCityAndStateAsync("Muriaé", "MG");

        // Assert
        result.Should().NotBeNull();
        result!.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
    }

    [Fact]
    public async Task GetByCityAndStateAsync_WithNonExistingCityAndState_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByCityAndStateAsync("Não Existe", "XX");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithActiveCity_ShouldReturnTrue()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        var result = await _repository.IsCityAllowedAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithInactiveCity_ShouldReturnFalse()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, false);
        await AddCityAndSaveAsync(city);

        // Act
        var result = await _repository.IsCityAllowedAsync("Muriaé", "MG");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCityAllowedAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.IsCityAllowedAsync("Não Existe", "XX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        city.Update("Itaperuna", "RJ", 3302270, true, "admin2@test.com");
        await UpdateCityAndSaveAsync(city);

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
    public async Task DeleteAsync_WithExistingCity_ShouldRemoveCity()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        await _repository.DeleteAsync(city);
        await _context.SaveChangesAsync();

        // Assert
        var deletedCity = await _context.AllowedCities.FirstOrDefaultAsync(c => c.Id == city.Id);
        deletedCity.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingCity_ShouldReturnTrue()
    {
        // Arrange
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        var result = await _repository.ExistsAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingCity_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync("Não Existe", "XX");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOrderedByCityNameAndState()
    {
        // Arrange
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        var city2 = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270);
        var city3 = new AllowedCity("Bom Jesus do Itabapoana", "RJ", "admin@test.com", 3300704);

        await AddCityAndSaveAsync(city1);
        await AddCityAndSaveAsync(city2);
        await AddCityAndSaveAsync(city3);

        // Act
        var result = await _repository.GetAllActiveAsync();

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
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city);

        // Act
        var result1 = await _repository.GetByCityAndStateAsync("MURIAÉ", "mg");
        var result2 = await _repository.GetByCityAndStateAsync("muriaé", "MG");

        // Assert
        result1.Should().NotBeNull();
        result1!.CityName.Should().Be("Muriaé");
        result2.Should().NotBeNull();
        result2!.CityName.Should().Be("Muriaé");
    }

    [Fact]
    public async Task AddAsync_WithDuplicateCityAndState_ShouldThrowException()
    {
        // Arrange
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city1);

        var city2 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);

        // Act
        var act = async () =>
        {
            await _repository.AddAsync(city2);
            await _context.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task AddAsync_WithDuplicateIbgeCode_ShouldThrowException()
    {
        // Arrange
        var city1 = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        await AddCityAndSaveAsync(city1);

        var city2 = new AllowedCity("Outra Cidade", "SP", "admin@test.com", 3143906);

        // Act
        var act = async () =>
        {
            await _repository.AddAsync(city2);
            await _context.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    public override async ValueTask InitializeAsync()
    {
        await InitializeInternalAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await DisposeInternalAsync();
    }

    private async Task DisposeInternalAsync()
    {
        await _context.DisposeAsync();
        await base.DisposeAsync();
    }

    private async Task AddCityAndSaveAsync(AllowedCity city)
    {
        await _repository.AddAsync(city);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateCityAndSaveAsync(AllowedCity city)
    {
        await _repository.UpdateAsync(city);
        await _context.SaveChangesAsync();
    }
}
