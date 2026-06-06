using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Infrastructure")]
public class DbContextSearchableProviderQueriesTests : IDisposable
{
    private readonly SearchProvidersDbContext _dbContext;
    private readonly Mock<IDapperConnection> _dapperMock;
    private readonly DbContextSearchableProviderQueries _queries;

    public DbContextSearchableProviderQueriesTests()
    {
        var options = new DbContextOptionsBuilder<SearchProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: "SearchQueriesTest_" + Guid.NewGuid())
            .Options;

        _dbContext = new SearchProvidersDbContext(options);
        _dapperMock = new Mock<IDapperConnection>();
        _queries = new DbContextSearchableProviderQueries(_dbContext, _dapperMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private async Task<SearchableProvider> CreateProviderAsync(string name = "Test Provider", Guid? providerId = null, Guid[]? serviceIds = null)
    {
        var provider = SearchableProvider.Create(
            providerId: providerId ?? Guid.NewGuid(),
            name: name,
            slug: name.ToLower().Replace(" ", "-"),
            location: new GeoPoint(-23.561, -46.656),
            subscriptionTier: ESubscriptionTier.Standard
        );

        if (serviceIds != null && serviceIds.Length > 0)
        {
            provider.UpdateServices(serviceIds);
        }

        _dbContext.SearchableProviders.Add(provider);
        await _dbContext.SaveChangesAsync();
        
        // Detach to start clean for query tests
        _dbContext.Entry(provider).State = EntityState.Detached;

        return provider;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProvider_ShouldReturnProviderAsNoTracking()
    {
        var provider = await CreateProviderAsync();

        var result = await _queries.GetByIdAsync(provider.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
        _dbContext.Entry(result).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithTrackFalse_ShouldReturnProviderAsNoTracking()
    {
        var providerId = Guid.NewGuid();
        var provider = await CreateProviderAsync(providerId: providerId);

        var result = await _queries.GetByProviderIdAsync(providerId, track: false);

        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
        _dbContext.Entry(result).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithTrackTrue_ShouldReturnTrackedProvider()
    {
        var providerId = Guid.NewGuid();
        var provider = await CreateProviderAsync(providerId: providerId);

        var result = await _queries.GetByProviderIdAsync(providerId, track: true);

        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
        _dbContext.Entry(result).State.Should().Be(EntityState.Unchanged); // Tracked and unmodified
    }

    [Fact]
    public async Task GetByServiceIdAsync_WithTrackFalse_ShouldReturnProvidersAsNoTracking()
    {
        var serviceId = Guid.NewGuid();
        await CreateProviderAsync(name: "Provider A", serviceIds: [serviceId]);
        await CreateProviderAsync(name: "Provider B", serviceIds: [serviceId]);

        var result = await _queries.GetByServiceIdAsync(serviceId, track: false);

        result.Should().HaveCount(2);
        _dbContext.Entry(result[0]).State.Should().Be(EntityState.Detached);
        _dbContext.Entry(result[1]).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetByServiceIdAsync_WithTrackTrue_ShouldReturnTrackedProviders()
    {
        var serviceId = Guid.NewGuid();
        await CreateProviderAsync(name: "Provider A", serviceIds: [serviceId]);

        var result = await _queries.GetByServiceIdAsync(serviceId, track: true);

        result.Should().HaveCount(1);
        _dbContext.Entry(result[0]).State.Should().Be(EntityState.Unchanged); // Tracked
    }

    [Fact]
    public async Task SearchAsync_WithInvalidRadius_ShouldReturnEmptyResultDirectly()
    {
        var location = new GeoPoint(-23.561, -46.656);
        
        var result = await _queries.SearchAsync(location, radiusInKm: -5.0);

        result.Should().NotBeNull();
        result.Providers.Should().BeEmpty();
        result.DistancesInKm.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

        // Verify Dapper was not queried
        _dapperMock.VerifyNoOtherCalls();
    }
}


