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
public sealed class CepProvidersUnavailabilityTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.None;

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
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithBody("Internal Server Error"));

        // BrasilAPI succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
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

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should succeed via BrasilAPI fallback (ViaCEP fails, BrasilAPI succeeds)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");

        // NOTE: Provider hit count assertions skipped due to WireMock shared state in parallel CI execution.
        // WireMock server is shared across test collections, making baseline counts unreliable even with unique CEPs.
        // The functional behavior (successful fallback) is validated above.
    }

    [Fact]
    public async Task LookupCep_WhenViaCepAndBrasilApiReturnInvalidJson_ShouldFallbackToOpenCep()
    {
        // Arrange - Use unique CEP to avoid conflicts with default stubs
        var uniqueCep = "34567890";

        // ViaCEP returns invalid/empty JSON (missing required fields triggers deserialization failure)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")); // Empty JSON lacks required fields, causing validation to fail

        // BrasilAPI also returns invalid/empty JSON
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")); // Empty JSON lacks required fields, causing validation to fail

        // OpenCEP succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/v1/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
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

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should succeed via OpenCEP fallback
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");

        // NOTE: Provider hit count assertions skipped due to WireMock shared state in parallel CI execution.
        // WireMock server is shared across test collections, making baseline counts unreliable even with unique CEPs.
        // The functional behavior (successful fallback to OpenCEP) is validated above.
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
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/v1/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

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
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{invalid json}"));

        // BrasilAPI succeeds
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
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

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should fallback to BrasilAPI
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsErrorTrueAndOthersFail_ShouldReturnFailure()
    {
        // Arrange - ViaCEP returns "erro: true" for invalid CEP
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/00000000/json/")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"erro": true}"""));

        // BrasilAPI also fails (404 for invalid CEP - v2 behavior)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v2/00000000")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404)
                .WithBody("CEP não encontrado"));

        // OpenCEP also fails
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/v1/00000000")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync("00000000");

        // Assert - Should return failure for truly invalid CEP
        result.IsSuccess.Should().BeFalse();
    }
}
