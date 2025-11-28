using Bogus;
using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Testes unitários para SearchableProviderRepository.
/// Verifica operações CRUD (EF Core) usando InMemory database.
/// Nota: SearchAsync usa Dapper e requer testes de integração separados.
/// </summary>
public class SearchableProviderRepositoryTests : IDisposable, IAsyncDisposable
{
    private readonly SearchProvidersDbContext _context;
    private readonly Mock<IDapperConnection> _mockDapper;
    private readonly ISearchableProviderRepository _repository;
    private readonly Faker _faker;

    public SearchableProviderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<SearchProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: $"SearchProvidersTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new SearchProvidersDbContext(options);
        _mockDapper = new Mock<IDapperConnection>();
        _repository = new SearchableProviderRepository(_context, _mockDapper.Object);
        _faker = new Faker();
    }

    private SearchableProvider CreateTestProvider(
        Guid? providerId = null,
        string? name = null,
        double? latitude = null,
        double? longitude = null,
        ESubscriptionTier? tier = null)
    {
        var location = new GeoPoint(
            latitude ?? _faker.Address.Latitude(),
            longitude ?? _faker.Address.Longitude()
        );

        return SearchableProvider.Create(
            providerId ?? Guid.CreateVersion7(),
            name ?? _faker.Company.CompanyName(),
            location,
            tier ?? ESubscriptionTier.Free,
            _faker.Lorem.Sentence(),
            _faker.Address.City(),
            _faker.Address.StateAbbr()
        );
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnProvider()
    {
        // Arrange
        var provider = CreateTestProvider();
        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(provider.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
        result.Name.Should().Be(provider.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = new SearchableProviderId(Guid.CreateVersion7());

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingProviderId_ShouldReturnProvider()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var provider = CreateTestProvider(providerId: providerId);
        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithNonExistingProviderId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingProviderId = Guid.CreateVersion7();

        // Act
        var result = await _repository.GetByProviderIdAsync(nonExistingProviderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithMultipleProviders_ShouldReturnCorrectOne()
    {
        // Arrange
        var providerId1 = Guid.CreateVersion7();
        var providerId2 = Guid.CreateVersion7();
        var provider1 = CreateTestProvider(providerId: providerId1);
        var provider2 = CreateTestProvider(providerId: providerId2);

        await _context.SearchableProviders.AddRangeAsync(provider1, provider2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProviderIdAsync(providerId1);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId1);
        result.Id.Should().Be(provider1.Id);
    }

    [Fact]
    public async Task AddAsync_WithValidProvider_ShouldAddToContext()
    {
        // Arrange
        var provider = CreateTestProvider();

        // Act
        await _repository.AddAsync(provider);

        // Assert
        var entry = _context.Entry(provider);
        entry.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task AddAsync_WithValidProvider_ShouldPersistAfterSaveChanges()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var provider = CreateTestProvider(providerId: providerId);

        // Act
        await _repository.AddAsync(provider);
        await _repository.SaveChangesAsync();

        // Assert
        var persisted = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);

        persisted.Should().NotBeNull();
        persisted!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task UpdateAsync_WithValidProvider_ShouldMarkAsModified()
    {
        // Arrange
        var provider = CreateTestProvider(name: "Original Name");
        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Detach to simulate update from different context
        _context.Entry(provider).State = EntityState.Detached;

        // Modify provider
        provider.UpdateBasicInfo("Updated Name", "Updated Description", "São Paulo", "SP");

        // Act
        await _repository.UpdateAsync(provider);

        // Assert
        var entry = _context.Entry(provider);
        entry.State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task UpdateAsync_WithValidProvider_ShouldPersistChanges()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var provider = CreateTestProvider(providerId: providerId, name: "Original Name");
        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Modify provider
        provider.UpdateBasicInfo("Updated Name", "Updated Description", "Rio de Janeiro", "RJ");

        // Act
        await _repository.UpdateAsync(provider);
        await _repository.SaveChangesAsync();

        // Assert
        var updated = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_WithValidProvider_ShouldMarkAsDeleted()
    {
        // Arrange
        var provider = CreateTestProvider();
        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(provider);

        // Assert
        var entry = _context.Entry(provider);
        entry.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_WithValidProvider_ShouldRemoveFromDatabase()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var provider = CreateTestProvider(providerId: providerId);
        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(provider);
        await _repository.SaveChangesAsync();

        // Assert
        var deleted = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnNumberOfAffectedRows()
    {
        // Arrange
        var provider1 = CreateTestProvider();
        var provider2 = CreateTestProvider();
        await _repository.AddAsync(provider1);
        await _repository.AddAsync(provider2);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoChanges_ShouldReturnZero()
    {
        // Arrange - no changes made

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleOperations_ShouldPersistAll()
    {
        // Arrange
        var provider1 = CreateTestProvider();
        var provider2 = CreateTestProvider();
        var provider3 = CreateTestProvider();

        await _repository.AddAsync(provider1);
        await _repository.AddAsync(provider2);
        await _repository.AddAsync(provider3);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var count = await _context.SearchableProviders.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task UpdateLocation_ShouldPersistNewCoordinates()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var originalLocation = new GeoPoint(-23.5505, -46.6333); // São Paulo
        var provider = CreateTestProvider(
            providerId: providerId,
            latitude: originalLocation.Latitude,
            longitude: originalLocation.Longitude
        );

        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Update location
        var newLocation = new GeoPoint(-22.9068, -43.1729); // Rio de Janeiro
        provider.UpdateLocation(newLocation);

        // Act
        await _repository.UpdateAsync(provider);
        await _repository.SaveChangesAsync();

        // Assert
        var updated = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);

        updated.Should().NotBeNull();
        updated!.Location.Latitude.Should().BeApproximately(newLocation.Latitude, 0.0001);
        updated.Location.Longitude.Should().BeApproximately(newLocation.Longitude, 0.0001);
    }

    [Fact]
    public async Task UpdateRating_ShouldPersistNewValues()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var provider = CreateTestProvider(providerId: providerId);

        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Update rating
        provider.UpdateRating(4.5m, 100);

        // Act
        await _repository.UpdateAsync(provider);
        await _repository.SaveChangesAsync();

        // Assert
        var updated = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);

        updated.Should().NotBeNull();
        updated!.AverageRating.Should().Be(4.5m);
        updated.TotalReviews.Should().Be(100);
    }

    [Fact]
    public async Task Activate_Deactivate_ShouldToggleStatus()
    {
        // Arrange
        var providerId = Guid.CreateVersion7();
        var provider = CreateTestProvider(providerId: providerId);

        await _context.SearchableProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Deactivate
        provider.Deactivate();
        await _repository.UpdateAsync(provider);
        await _repository.SaveChangesAsync();

        var deactivated = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);
        deactivated!.IsActive.Should().BeFalse();

        // Activate
        provider.Activate();
        await _repository.UpdateAsync(provider);
        await _repository.SaveChangesAsync();

        // Assert
        var activated = await _context.SearchableProviders
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);

        activated!.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
