using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.Locations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Integration tests for CEP provider unavailability scenarios.
/// Validates the fallback chain behavior when external CEP APIs fail:
/// ViaCEP → BrasilAPI → OpenCEP
/// </summary>
[Collection("Integration")]
public sealed class CepProvidersUnavailabilityTests : ApiTestBase
{

    [Fact]
    public async Task LookupCep_WhenViaCepReturns500_ShouldFallbackToBrasilApi()
    {
        // Arrange - Use unique CEP to avoid conflicts with default stubs
        var uniqueCep = "23456789";

        // ViaCEP fails with 500
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithBody("Internal Server Error"));

        // BrasilAPI succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should succeed via BrasilAPI fallback
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepAndBrasilApiReturnInvalidJson_ShouldFallbackToOpenCep()
    {
        // Arrange - Use unique CEP to avoid conflicts with default stubs
        var uniqueCep = "34567890";

        // ViaCEP returns invalid/empty JSON (200 with "{}" triggers deserialization failure)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")
                .WithDelay(TimeSpan.FromSeconds(2))); // Delay to simulate slow response

        // BrasilAPI returns invalid/empty JSON (200 with "{}" triggers deserialization failure)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")
                .WithDelay(TimeSpan.FromSeconds(2))); // Delay to simulate slow response

        // OpenCEP succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/v1/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "logradouro": "Avenida Paulista",
                        "bairro": "Bela Vista",
                        "localidade": "São Paulo",
                        "uf": "SP",
                        "ibge": "3550308"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should succeed via OpenCEP fallback
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
    }

    [Fact]
    public async Task LookupCep_WhenAllProvidersReturn500_ShouldReturnFailure()
    {
        // Arrange - All providers fail for a unique CEP to avoid cache hits
        var uniqueCep = "88888888"; // CEP not used in other tests

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/v1/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should return failure when all providers down
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsMalformedJson_ShouldFallbackToBrasilApi()
    {
        // Arrange - Use unique CEP to avoid conflicts with default stubs
        var uniqueCep = "12345678";

        // ViaCEP returns malformed JSON
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{invalid json}"));

        // BrasilAPI succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should fallback to BrasilAPI
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsErrorTrueAndOthersFail_ShouldReturnFailure()
    {
        // Arrange - ViaCEP returns "erro: true" for invalid CEP
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/00000000/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"erro": true}"""));

        // BrasilAPI also fails (404 for invalid CEP - v2 behavior)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v2/00000000")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404)
                .WithBody("CEP não encontrado"));

        // OpenCEP also fails
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/v1/00000000")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("00000000");

        // Assert - Should return failure for truly invalid CEP
        result.IsSuccess.Should().BeFalse();
    }

    [Fact(Skip = "Caching is disabled in integration tests (Caching:Enabled = false). This test cannot validate cache behavior without enabling caching infrastructure.")]
    public async Task LookupCep_WhenBrasilApiSucceedsButViaCepDown_ShouldUseCache()
    {
        // Arrange - Use unique CEP to avoid conflicts with default stubs
        var uniqueCep = "45678901";

        // First call: ViaCEP down, BrasilAPI succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "state": "SP",
                        "city": "São Paulo",
                        "neighborhood": "Bela Vista",
                        "street": "Avenida Paulista"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationModuleApi>();

        // Act - First call (cache miss)
        var result1 = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Get request count before second call
        var requestCountBefore = WireMock.Server.LogEntries.Count();

        // Act - Second call (should use cache, no HTTP requests)
        // Note: We don't reconfigure stubs here to avoid WireMock mapping conflicts.
        // Cache behavior is validated by verifying no new HTTP requests were made.
        var result2 = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Get request count after second call
        var requestCountAfter = WireMock.Server.LogEntries.Count();

        // Assert - Both calls should succeed (second via cache)
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result2.Value!.City.Should().Be("São Paulo");

        // Verify no HTTP requests were made during cached call
        requestCountAfter.Should().Be(requestCountBefore,
            "Second call should use cache and not make HTTP requests");
    }
}
