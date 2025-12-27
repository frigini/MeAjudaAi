using MeAjudaAi.Modules.Locations.API;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Time.Testing;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Http;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Classe base para testes de integração do módulo Locations.
/// Fornece configuração compartilhada de DI para testes de CEP e geocodificação.
/// </summary>
public abstract class LocationIntegrationTestFixture : IAsyncLifetime
{
    protected ServiceProvider? ServiceProvider;
    protected MockHttpClientBuilder? HttpMockBuilder;

    public virtual async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();

        // Adiciona logging
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        // Adiciona provedor de data/hora
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        // Adiciona cache em memória para testes
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

        // Configura clientes HTTP com mocks
        HttpMockBuilder = new MockHttpClientBuilder(services);
        ConfigureHttpClients(HttpMockBuilder);

        // Adiciona serviços do módulo Locations
        var configuration = new ConfigurationBuilder().Build();
        MeAjudaAi.Modules.Locations.API.Extensions.AddLocationsModule(services, configuration);

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
    /// Configura os mocks de clientes HTTP específicos para o cenário de teste.
    /// </summary>
    protected abstract void ConfigureHttpClients(MockHttpClientBuilder builder);
}
