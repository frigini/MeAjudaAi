using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Tests.Mocks;
using MeAjudaAi.Shared.Tests.Mocks.Http;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

/// <summary>
/// Base fixture for Location module integration tests.
/// Provides shared DI setup for CEP lookup and geocoding tests.
/// </summary>
public abstract class LocationIntegrationTestFixture : IAsyncLifetime
{
    protected ServiceProvider? ServiceProvider;
    protected MockHttpClientBuilder? HttpMockBuilder;

    public virtual async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        // Add time provider
        services.AddSingleton<IDateTimeProvider>(new MockDateTimeProvider());

        // Add caching (in-memory for tests)
        services.AddMemoryCache();
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromMinutes(30)
            };
        });
        services.AddSingleton<CacheMetrics>();
        services.AddSingleton<ICacheService, HybridCacheService>();

        // Configure HTTP clients with mocks
        HttpMockBuilder = new MockHttpClientBuilder(services);
        ConfigureHttpClients(HttpMockBuilder);

        // Add Location module services
        var configuration = new ConfigurationBuilder().Build();
        MeAjudaAi.Modules.Location.Infrastructure.Extensions.AddLocationModule(services, configuration);

        ServiceProvider = services.BuildServiceProvider();
        await Task.CompletedTask;
    }

    public virtual async ValueTask DisposeAsync()
    {
        HttpMockBuilder?.ResetAll();
        if (ServiceProvider != null)
        {
            await ServiceProvider.DisposeAsync();
        }
    }

    /// <summary>
    /// Configure HTTP client mocks specific to the test scenario.
    /// </summary>
    protected abstract void ConfigureHttpClients(MockHttpClientBuilder builder);
}
