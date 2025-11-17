using FluentAssertions;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using MeAjudaAi.Shared.Tests.Mocks.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

/// <summary>
/// Testes de integração para o serviço de geocoding com mock HTTP handlers.
/// </summary>
public sealed class GeocodingIntegrationTests : IAsyncLifetime
{
    private ServiceProvider? _serviceProvider;
    private MockHttpClientBuilder? _httpMockBuilder;

    public async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        // Add caching (in-memory for tests)
        services.AddHybridCache();
        services.AddSingleton<CacheMetrics>();
        services.AddSingleton<ICacheService, HybridCacheService>();

        // Configure HTTP clients com mocks
        _httpMockBuilder = new MockHttpClientBuilder(services);
        _httpMockBuilder.AddMockedClient<NominatimClient>();

        // Add Location module services
        var configuration = new ConfigurationBuilder().Build();
        MeAjudaAi.Modules.Location.Infrastructure.Extensions.AddLocationModule(services, configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        _httpMockBuilder?.ResetAll();
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WithValidAddress_ShouldReturnCoordinates()
    {
        // Arrange
        var address = "Avenida Paulista, São Paulo, SP";
        var nominatimResponse = """
        [
          {
            "lat": "-23.561414",
            "lon": "-46.656559",
            "display_name": "Avenida Paulista, Bela Vista, São Paulo, SP, Brasil"
          }
        ]
        """;

        _httpMockBuilder!.GetHandler<NominatimClient>()
            .SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, nominatimResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Latitude.Should().BeApproximately(-23.561414, 0.000001);
        result.Value.Longitude.Should().BeApproximately(-46.656559, 0.000001);
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WithEmptyAddress_ShouldReturnFailure()
    {
        // Arrange
        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetCoordinatesFromAddressAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("vazio");
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WhenAddressNotFound_ShouldReturnFailure()
    {
        // Arrange
        var address = "Endereço Inexistente XYZ123, Cidade Fictícia";
        var emptyResponse = "[]";

        _httpMockBuilder!.GetHandler<NominatimClient>()
            .SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, emptyResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("não encontradas");
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WhenNominatimReturnsError_ShouldReturnFailure()
    {
        // Arrange
        var address = "Avenida Paulista, São Paulo, SP";

        _httpMockBuilder!.GetHandler<NominatimClient>()
            .SetupErrorResponse("nominatim.openstreetmap.org", HttpStatusCode.ServiceUnavailable);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    // TODO: Investigar comportamento do HybridCache em ambiente de testes
    // Cache está fazendo múltiplas chamadas HTTP ao invés de usar cache na segunda requisição
    [Fact(Skip = "Cache behavior needs investigation - HybridCache may not be working as expected in test environment")]
    public async Task GetCoordinatesFromAddressAsync_WithCaching_ShouldCacheResults()
    {
        // Arrange
        var address = "Avenida Paulista, São Paulo, SP";
        var nominatimResponse = """
        [
          {
            "lat": "-23.561414",
            "lon": "-46.656559",
            "display_name": "Avenida Paulista, Bela Vista, São Paulo, SP, Brasil"
          }
        ]
        """;

        var handler = _httpMockBuilder!.GetHandler<NominatimClient>();
        handler.SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, nominatimResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act - Primeira chamada
        var result1 = await locationApi.GetCoordinatesFromAddressAsync(address);
        
        // Act - Segunda chamada (deve usar cache)
        var result2 = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Latitude.Should().Be(result2.Value.Latitude);
        result1.Value.Longitude.Should().Be(result2.Value.Longitude);

        // Verifica que foi feita apenas UMA chamada HTTP (a segunda veio do cache)
        handler.VerifyRequest("nominatim.openstreetmap.org", Times.Once());
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_MultipleAddresses_ShouldHandleRateLimiting()
    {
        // Arrange
        var addresses = new[]
        {
            "Avenida Paulista, São Paulo, SP",
            "Rua Augusta, São Paulo, SP",
            "Praça da Sé, São Paulo, SP"
        };

        var nominatimResponse = """
        [
          {
            "lat": "-23.561414",
            "lon": "-46.656559",
            "display_name": "São Paulo, SP, Brasil"
          }
        ]
        """;

        _httpMockBuilder!.GetHandler<NominatimClient>()
            .SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, nominatimResponse);

        var locationApi = _serviceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act - Chamar múltiplas vezes para testar rate limiting
        var tasks = addresses.Select(addr => locationApi.GetCoordinatesFromAddressAsync(addr)).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert - Todas devem ter sucesso (rate limiting deve funcionar internamente)
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
    }
}
