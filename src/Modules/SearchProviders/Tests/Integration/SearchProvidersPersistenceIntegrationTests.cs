using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Integration;

[Trait("Category", "Integration")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Infrastructure")]
public class SearchProvidersPersistenceIntegrationTests : SearchProvidersIntegrationTestBase
{
    private IServiceScope? _scope;
    private IServiceProvider ScopedProvider => (_scope ??= CreateScope()).ServiceProvider;

    private IUnitOfWork Uow => ScopedProvider.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.SearchProviders);
    private ISearchableProviderQueries Queries => ScopedProvider.GetRequiredService<ISearchableProviderQueries>();

    public override async ValueTask DisposeAsync()
    {
        _scope?.Dispose();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Queries_GetByProviderId_WithTrackTrue_ShouldReturnTrackedEntity()
    {
        // Arrange
        await CleanupDatabase();
        var providerId = Guid.NewGuid();
        var provider = CreateTestSearchableProviderWithProviderId(providerId, "Tracked Provider", -23.561, -46.656);
        await PersistSearchableProviderAsync(provider);

        // Act
        var result = await Queries.GetByProviderIdAsync(providerId, track: true);

        // Assert
        result.Should().NotBeNull();
        var dbContext = ScopedProvider.GetRequiredService<SearchProvidersDbContext>();
        dbContext.Entry(result!).State.Should().Be(EntityState.Unchanged);

        // Update tracking test
        result!.UpdateBasicInfo("Updated Tracked Provider", result.Slug, "New bio", "São Paulo", "SP");
        await Uow.SaveChangesAsync();

        // Verify persist
        var updated = await Queries.GetByProviderIdAsync(providerId, track: false);
        updated!.Name.Should().Be("Updated Tracked Provider");
    }

    [Fact]
    public async Task Queries_GetByServiceId_WithTrackTrue_ShouldReturnTrackedEntities()
    {
        // Arrange
        await CleanupDatabase();
        var serviceId = Guid.NewGuid();
        var provider = CreateTestSearchableProvider("Service Provider", -23.561, -46.656);
        provider.UpdateServices([serviceId]);
        await PersistSearchableProviderAsync(provider);

        // Act
        var result = await Queries.GetByServiceIdAsync(serviceId, track: true);

        // Assert
        result.Should().HaveCount(1);
        var dbContext = ScopedProvider.GetRequiredService<SearchProvidersDbContext>();
        dbContext.Entry(result[0]).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task Queries_SearchAsync_WithPostGIS_ShouldCalculateDistancesAndFilterCorrectly()
    {
        // Arrange
        await CleanupDatabase();
        
        // Center of Search: MASP, São Paulo (-23.561414, -46.656559)
        var center = new GeoPoint(-23.561414, -46.656559);

        // Near Provider: ~500m away (-23.565, -46.656)
        var nearProvider = CreateTestSearchableProvider("Near Gold", -23.565, -46.656, ESubscriptionTier.Gold);
        nearProvider.UpdateRating(4.8m, 15);
        nearProvider.UpdateServices([Guid.NewGuid()]);
        await PersistSearchableProviderAsync(nearProvider);

        // Far Provider: ~12km away (-23.660, -46.656)
        var farProvider = CreateTestSearchableProvider("Far Free", -23.660, -46.656, ESubscriptionTier.Free);
        await PersistSearchableProviderAsync(farProvider);

        // Act & Assert 1: Search within 2km (should find only near)
        var search1 = await Queries.SearchAsync(center, radiusInKm: 2.0);
        search1.TotalCount.Should().Be(1);
        search1.Providers.Should().HaveCount(1);
        search1.Providers[0].Name.Should().Be("Near Gold");
        search1.DistancesInKm[0].Should().BeLessThan(1.0); // Less than 1km

        // Act & Assert 2: Search within 15km (should find both, ordered by Tier then Distance)
        var search2 = await Queries.SearchAsync(center, radiusInKm: 15.0);
        search2.TotalCount.Should().Be(2);
        search2.Providers.Should().HaveCount(2);
        search2.Providers[0].Name.Should().Be("Near Gold"); // Gold
        search2.Providers[1].Name.Should().Be("Far Free");  // Free
        search2.DistancesInKm[1].Should().BeGreaterThan(10.0); // Far distance
    }
}



