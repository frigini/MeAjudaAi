using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Http;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

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

        // Configura configuração para testes
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cache:Enabled"] = "false",
                ["Caching:Enabled"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=locations_test",
                ["ConnectionStrings:Locations"] = "Host=localhost;Database=locations_test"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        var environment = new Mock<IHostEnvironment>().Object;

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
        services.AddSingleton<ICacheMetrics, CacheMetrics>();
        services.AddSingleton<ICacheService, HybridCacheService>();

        // Adiciona serialização (ISerializer keyed services) - necessário para ViaCepClient, NominatimClient, etc.
        services.AddCustomSerialization();

        // Adiciona localização (necessário para IStringLocalizer<Strings> nos ModuleApis)
        var localizerMock = new Mock<IStringLocalizer<Strings>>();
        localizerMock.Setup(x => x[It.Is<string>(s => s == "InvalidCep"), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"CEP inválido: {args[0]}"));
        localizerMock.Setup(x => x[It.Is<string>(s => s == "CepNotFound"), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"CEP {args[0]} não encontrado."));
        localizerMock.Setup(x => x[It.Is<string>(s => s == "AddressCannotBeEmpty")])
            .Returns(new LocalizedString("AddressCannotBeEmpty", "Endereço não pode ser vazio."));
        localizerMock.Setup(x => x[It.Is<string>(s => s == "CoordinatesNotFoundForAddress"), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"Coordenadas não encontradas para o endereço: {args[0]}"));
        localizerMock.Setup(x => x[It.Is<string>(s => s == "ErrorFetchingCityId")])
            .Returns(new LocalizedString("ErrorFetchingCityId", "Erro ao buscar ID da cidade."));
        services.AddSingleton<IStringLocalizer<Strings>>(localizerMock.Object);

        // Adiciona serviços do módulo Locations PRIMEIRO
        MeAjudaAi.Modules.Locations.API.Extensions.AddLocationsModule(services, configuration, environment);

        // DEPOIS configura os clientes HTTP com mocks (sobrescreve os handlers)
        HttpMockBuilder = new MockHttpClientBuilder(services);
        ConfigureHttpClients(HttpMockBuilder);

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
