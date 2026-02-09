using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Integration;

/// <summary>
/// Testes de integração para a infraestrutura de SearchProviders - casos extremos e condições de contorno
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "SearchProviders")]
[Trait("Component", "Infrastructure")]
public class SearchProvidersInfrastructureIntegrationTests : SearchProvidersIntegrationTestBase
{
    [Fact]
    public async Task SearchAsync_WithNoProvidersInDatabase_ShouldReturnEmptyResult()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();
        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act
        var result = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Should().NotBeNull();
        result.Providers.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithZeroRadius_ShouldReturnEmptyResult()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("Provider SP", -23.5505, -46.6333);
        await PersistSearchableProviderAsync(provider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333); // Exact same location

        // Act
        var result = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 0,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithVeryLargeRadius_ShouldReturnAllProviders()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider1 = CreateTestSearchableProvider("Provider SP", -23.5505, -46.6333);
        var provider2 = CreateTestSearchableProvider("Provider RJ", -22.9068, -43.1729);
        var provider3 = CreateTestSearchableProvider("Provider BH", -19.9167, -43.9345);

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);
        await PersistSearchableProviderAsync(provider3);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - 1000km radius should include all Brazilian cities
        var result = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 1000,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        // Create 15 providers in the same location
        for (int i = 0; i < 15; i++)
        {
            var provider = CreateTestSearchableProvider($"Provider {i}", -23.5505, -46.6333);
            await PersistSearchableProviderAsync(provider);
        }

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Get page 1 (skip 0, take 10)
        var page1 = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Act - Get page 2 (skip 10, take 10)
        var page2 = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 10,
            take: 10);

        // Assert
        page1.Providers.Should().HaveCount(10);
        page1.TotalCount.Should().Be(15);

        page2.Providers.Should().HaveCount(5); // Only 5 remaining
        page2.TotalCount.Should().Be(15);

        // Ensure no overlap between pages
        var page1Ids = page1.Providers.Select(p => p.Id).ToHashSet();
        var page2Ids = page2.Providers.Select(p => p.Id).ToHashSet();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [Fact]
    public async Task SearchAsync_OrderBySubscriptionTier_ShouldPrioritizePremiumOverStandard()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var standardProvider = CreateTestSearchableProvider(
            "Standard Provider",
            -23.5505,
            -46.6333,
            ESubscriptionTier.Standard);

        var goldProvider = CreateTestSearchableProvider(
            "Gold Provider",
            -23.5505,
            -46.6333,
            ESubscriptionTier.Gold); // Higher tier

        await PersistSearchableProviderAsync(standardProvider);
        await PersistSearchableProviderAsync(goldProvider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act
        var result = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().HaveCount(2);
        result.Providers.First().Id.Value.Should().Be(goldProvider.Id.Value); // Gold should come first
        result.Providers.Last().Id.Value.Should().Be(standardProvider.Id.Value);
    }

    [Fact]
    public async Task SearchAsync_WithSameTierAndRating_ShouldOrderByDistance()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var nearProvider = CreateTestSearchableProvider(
            "Near Provider",
            -23.5505,
            -46.6333, // São Paulo Centro
            ESubscriptionTier.Standard);

        var farProvider = CreateTestSearchableProvider(
            "Far Provider",
            -23.5629,
            -46.7011, // São Paulo Pinheiros (~10km away)
            ESubscriptionTier.Standard);

        await PersistSearchableProviderAsync(nearProvider);
        await PersistSearchableProviderAsync(farProvider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333); // Search from Centro

        // Act
        var result = await repository.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().HaveCount(2);
        result.Providers.First().Id.Value.Should().Be(nearProvider.Id.Value); // Nearest should come first
        result.Providers.Last().Id.Value.Should().Be(farProvider.Id.Value);
    }

    [Fact]
    public async Task SearchAsync_WithNonExistentServiceIds_ShouldReturnEmptyResult()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("Provider SP", -23.5505, -46.6333);
        provider.UpdateServices(new[] { Guid.NewGuid(), Guid.NewGuid() });
        await PersistSearchableProviderAsync(provider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);
        var nonExistentServiceIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await repository.SearchAsync(
            searchLocation,
            50,
            null,
            serviceIds: nonExistentServiceIds,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().BeEmpty();
    }



    [Fact]
    public async Task SearchAsync_WithSubscriptionTierFilter_ShouldFilterCorrectly()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var standardProvider = CreateTestSearchableProvider(
            "Standard",
            -23.5505,
            -46.6333,
            ESubscriptionTier.Standard);

        var goldProvider = CreateTestSearchableProvider(
            "Gold",
            -23.5505,
            -46.6333,
            ESubscriptionTier.Gold);

        await PersistSearchableProviderAsync(standardProvider);
        await PersistSearchableProviderAsync(goldProvider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act
        var result = await repository.SearchAsync(
            searchLocation,
            50,
            null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: new[] { ESubscriptionTier.Gold },
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().HaveCount(1);
        result.Providers.First().Id.Value.Should().Be(goldProvider.Id.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();
        var nonExistentId = new SearchableProviderId(Guid.NewGuid());

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldRetrieveSameProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("Test Provider", -23.5505, -46.6333);
        var serviceIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        provider.UpdateServices(serviceIds);

        // Act
        await repository.AddAsync(provider);
        await repository.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(provider.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(provider.Id);
        retrieved.Name.Should().Be("Test Provider");
        retrieved.ServiceIds.Should().Equal(serviceIds);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("Original Name", -23.5505, -46.6333);
        await repository.AddAsync(provider);
        await repository.SaveChangesAsync();

        // Act - Update services
        var newServiceIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        provider.UpdateServices(newServiceIds);
        await repository.UpdateAsync(provider);
        await repository.SaveChangesAsync();

        // Assert
        var retrieved = await repository.GetByIdAsync(provider.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ServiceIds.Should().Equal(newServiceIds);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("To Delete", -23.5505, -46.6333);
        await repository.AddAsync(provider);
        await repository.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(provider);
        await repository.SaveChangesAsync();

        // Assert
        var retrieved = await repository.GetByIdAsync(provider.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ConcurrentSearches_ShouldReturnConsistentResults()
    {
        // Arrange
        await CleanupDatabase();

        for (int i = 0; i < 10; i++)
        {
            var provider = CreateTestSearchableProvider($"Provider {i}", -23.5505, -46.6333);
            await PersistSearchableProviderAsync(provider);
        }

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Execute 10 concurrent searches
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(async () =>
            {
                using var innerScope = CreateScope();
                var innerRepo = innerScope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();
                return await innerRepo.SearchAsync(
                    searchLocation,
                    radiusInKm: 50,
                    term: null,
                    serviceIds: null,
                    minRating: null,
                    subscriptionTiers: null,
                    skip: 0,
                    take: 10);
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - All concurrent searches should return same count
        results.Should().AllSatisfy(result =>
        {
            result.Providers.Should().HaveCount(10);
            result.TotalCount.Should().Be(10);
        });
    }
}
