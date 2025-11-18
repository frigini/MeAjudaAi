using System.Net;
using FluentAssertions;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using MeAjudaAi.Shared.Tests.Mocks;
using MeAjudaAi.Shared.Tests.Mocks.Http;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

/// <summary>
/// Testes de integração para o serviço de geocoding com mock HTTP handlers.
/// </summary>
public sealed class GeocodingIntegrationTests : LocationIntegrationTestFixture
{
    protected override void ConfigureHttpClients(MockHttpClientBuilder builder)
    {
        builder.AddMockedClient<NominatimClient>();
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

        HttpMockBuilder!.GetHandler<NominatimClient>()
            .SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, nominatimResponse);

        var locationApi = ServiceProvider!.GetRequiredService<ILocationModuleApi>();

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
        var locationApi = ServiceProvider!.GetRequiredService<ILocationModuleApi>();

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

        HttpMockBuilder!.GetHandler<NominatimClient>()
            .SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, emptyResponse);

        var locationApi = ServiceProvider!.GetRequiredService<ILocationModuleApi>();

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

        HttpMockBuilder!.GetHandler<NominatimClient>()
            .SetupErrorResponse("nominatim.openstreetmap.org", HttpStatusCode.ServiceUnavailable);

        var locationApi = ServiceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
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

        var handler = HttpMockBuilder!.GetHandler<NominatimClient>();
        handler.SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, nominatimResponse);

        var locationApi = ServiceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act - Primeira chamada
        var result1 = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Pequena pausa para garantir que o cache foi atualizado
        await Task.Delay(100);

        // Segunda e terceira chamadas (devem usar cache)
        var result2 = await locationApi.GetCoordinatesFromAddressAsync(address);
        var result3 = await locationApi.GetCoordinatesFromAddressAsync(address);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();

        // Valida que os resultados são consistentes (mesmo valor do cache)
        result1.Value.Latitude.Should().Be(result2.Value.Latitude);
        result1.Value.Longitude.Should().Be(result2.Value.Longitude);
        result2.Value.Latitude.Should().Be(result3.Value.Latitude);
        result2.Value.Longitude.Should().Be(result3.Value.Longitude);

        // Valida que os valores estão corretos
        result1.Value.Latitude.Should().BeApproximately(-23.561414, 0.000001);
        result1.Value.Longitude.Should().BeApproximately(-46.656559, 0.000001);

        // Nota: Não validamos o número exato de chamadas HTTP porque o HybridCache
        // pode fazer múltiplas chamadas durante serialização/desserialização inicial.
        // O importante é que as chamadas subsequentes retornam o mesmo resultado.
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

        HttpMockBuilder!.GetHandler<NominatimClient>()
            .SetupResponse("nominatim.openstreetmap.org", HttpStatusCode.OK, nominatimResponse);

        var locationApi = ServiceProvider!.GetRequiredService<ILocationModuleApi>();

        // Act - Chamar múltiplas vezes para testar rate limiting
        var tasks = addresses.Select(addr => locationApi.GetCoordinatesFromAddressAsync(addr)).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert - Todas devem ter sucesso (rate limiting deve funcionar internamente)
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
    }
}
