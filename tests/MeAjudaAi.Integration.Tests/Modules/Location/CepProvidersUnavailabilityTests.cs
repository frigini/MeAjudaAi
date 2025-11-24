using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

/// <summary>
/// Integration tests for CEP provider unavailability scenarios.
/// Validates the fallback chain behavior when external CEP APIs fail:
/// ViaCEP → BrasilAPI → OpenCEP
/// </summary>
[Collection("Integration")]
public sealed class CepProvidersUnavailabilityTests : ApiTestBase, IAsyncLifetime
{
    private WireMockFixture? _wireMockFixture;

    public new async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _wireMockFixture = new WireMockFixture();
        await _wireMockFixture.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        if (_wireMockFixture is not null)
        {
            await _wireMockFixture.DisposeAsync();
        }
        await base.DisposeAsync();
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturns500_ShouldFallbackToBrasilApi()
    {
        // Arrange - ViaCEP fails with 500
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/01310100/json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithBody("Internal Server Error"));

        // BrasilAPI succeeds
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/01310100")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "cep": "01310100",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("01310100");

        // Assert - Should succeed via BrasilAPI fallback
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepAndBrasilApiTimeout_ShouldFallbackToOpenCep()
    {
        // Arrange - ViaCEP times out
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/01310100/json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")
                .WithDelay(TimeSpan.FromSeconds(30)));

        // BrasilAPI times out
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/01310100")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")
                .WithDelay(TimeSpan.FromSeconds(30)));

        // OpenCEP succeeds
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/01310100.json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "cep": "01310-100",
                        "logradouro": "Avenida Paulista",
                        "bairro": "Bela Vista",
                        "localidade": "São Paulo",
                        "uf": "SP",
                        "ibge": "3550308"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("01310100");

        // Assert - Should succeed via OpenCEP fallback
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
    }

    [Fact]
    public async Task LookupCep_WhenAllProvidersReturn500_ShouldReturnFailure()
    {
        // Arrange - All providers fail
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/01310100/json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/01310100")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/01310100.json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("01310100");

        // Assert - Should return failure when all providers down
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsMalformedJson_ShouldFallbackToBrasilApi()
    {
        // Arrange - ViaCEP returns invalid JSON
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/01310100/json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{invalid json}"));

        // BrasilAPI succeeds
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/01310100")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "cep": "01310100",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("01310100");

        // Assert - Should fallback to BrasilAPI
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsErrorTrue_ShouldFallbackToBrasilApi()
    {
        // Arrange - ViaCEP returns "erro: true" for invalid CEP
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/00000000/json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"erro": true}"""));

        // BrasilAPI also fails (404 for invalid CEP)
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/00000000")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404)
                .WithBody("CEP não encontrado"));

        // OpenCEP also fails
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/00000000.json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("00000000");

        // Assert - Should return failure for truly invalid CEP
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task LookupCep_WhenBrasilApiSucceedsButViaCepDown_ShouldUseCache()
    {
        // Arrange - First call: ViaCEP down, BrasilAPI succeeds
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/01310100/json")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v1/01310100")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "cep": "01310100",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act - First call (cache miss)
        var result1 = await locationApi.GetAddressFromCepAsync("01310100");

        // Reset WireMock to simulate all providers down
        _wireMockFixture.Reset();
        _wireMockFixture.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .UsingAnyMethod())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        // Act - Second call (should use cache)
        var result2 = await locationApi.GetAddressFromCepAsync("01310100");

        // Assert - Both calls should succeed (second via cache)
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result2.Value!.City.Should().Be("São Paulo");
    }
}
