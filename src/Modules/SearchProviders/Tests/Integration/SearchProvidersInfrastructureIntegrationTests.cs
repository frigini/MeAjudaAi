using MeAjudaAi.Shared.Database.Abstractions;
using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Integration;

/// <summary>
/// Testes de integração para a infraestrutura de SearchProviders - casos extremos e condições de contorno.
/// Agora utiliza IUnitOfWork e ISearchableProviderQueries seguindo a refatoração da Fase 3.
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();
        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act
        var result = await queries.SearchAsync(
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider = CreateTestSearchableProvider("Provider SP", -23.5505, -46.6333);
        await PersistSearchableProviderAsync(provider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333); // Exact same location

        // Act
        var result = await queries.SearchAsync(
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider1 = CreateTestSearchableProvider("Provider SP", -23.5505, -46.6333);
        var provider2 = CreateTestSearchableProvider("Provider RJ", -22.9068, -43.1729);
        var provider3 = CreateTestSearchableProvider("Provider BH", -19.9167, -43.9345);

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);
        await PersistSearchableProviderAsync(provider3);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - 1000km radius should include all Brazilian cities
        var result = await queries.SearchAsync(
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        // Create 15 providers in the same location
        for (int i = 0; i < 15; i++)
        {
            var provider = CreateTestSearchableProvider($"Provider {i}", -23.5505, -46.6333);
            await PersistSearchableProviderAsync(provider);
        }

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Get page 1 (skip 0, take 10)
        var page1 = await queries.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Act - Get page 2 (skip 10, take 10)
        var page2 = await queries.SearchAsync(
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

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
        var result = await queries.SearchAsync(
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

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
        var result = await queries.SearchAsync(
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider = CreateTestSearchableProvider("Provider SP", -23.5505, -46.6333);
        provider.UpdateServices(new[] { Guid.NewGuid(), Guid.NewGuid() });
        await PersistSearchableProviderAsync(provider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);
        var nonExistentServiceIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await queries.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

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
        var result = await queries.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: null,
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
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();
        var nonExistentId = new SearchableProviderId(Guid.NewGuid());

        // Act
        var result = await queries.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_ThenGetById_ShouldRetrieveSameProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(MeAjudaAi.Shared.Database.Constants.ModuleKeys.SearchProviders);
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider = CreateTestSearchableProvider("Test Provider", -23.5505, -46.6333);
        var serviceIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        provider.UpdateServices(serviceIds);

        // Act
        uow.GetRepository<SearchableProvider, SearchableProviderId>().Add(provider);
        await uow.SaveChangesAsync();

        var retrieved = await queries.GetByIdAsync(provider.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(provider.Id);
        retrieved.Name.Should().Be("Test Provider");
        retrieved.ServiceIds.Should().Equal(serviceIds);
    }

    [Fact]
    public async Task Update_ShouldPersistChangesViaTracking()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(MeAjudaAi.Shared.Database.Constants.ModuleKeys.SearchProviders);
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider = CreateTestSearchableProvider("Original Name", -23.5505, -46.6333);
        await PersistSearchableProviderAsync(provider);

        // Act - Fetch with tracking, then update
        var tracked = await queries.GetByProviderIdAsync(provider.ProviderId, track: true);
        tracked.Should().NotBeNull();
        
        var newServiceIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        tracked!.UpdateServices(newServiceIds);
        await uow.SaveChangesAsync();

        // Assert
        var retrieved = await queries.GetByIdAsync(provider.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ServiceIds.Should().Equal(newServiceIds);
    }

    [Fact]
    public async Task Delete_ShouldRemoveProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(MeAjudaAi.Shared.Database.Constants.ModuleKeys.SearchProviders);
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider = CreateTestSearchableProvider("To Delete", -23.5505, -46.6333);
        await PersistSearchableProviderAsync(provider);

        // Act
        var repo = uow.GetRepository<SearchableProvider, SearchableProviderId>();
        var tracked = await repo.TryFindAsync(provider.Id);
        tracked.Should().NotBeNull();
        repo.Delete(tracked!);
        await uow.SaveChangesAsync();

        // Assert
        var retrieved = await queries.GetByIdAsync(provider.Id);
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
                var innerQueries = innerScope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();
                return await innerQueries.SearchAsync(
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
    [Fact]
    public async Task SearchAsync_WithTerm_ShouldFilterCaseInsensitive()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<ISearchableProviderQueries>();

        var provider1 = CreateTestSearchableProvider("João Silva", -23.5505, -46.6333);
        var provider2 = CreateTestSearchableProvider("Maria Santos", -23.5505, -46.6333);

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act
        var result = await queries.SearchAsync(
            searchLocation,
            radiusInKm: 50,
            term: "silva", // Lowercase search
            serviceIds: null,
            minRating: null,
            subscriptionTiers: null,
            skip: 0,
            take: 10);

        // Assert
        result.Providers.Should().HaveCount(1);
        result.Providers.First().Name.Should().Be("João Silva");
    }
}




