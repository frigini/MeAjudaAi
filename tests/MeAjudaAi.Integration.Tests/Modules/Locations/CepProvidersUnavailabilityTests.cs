using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Integration.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

public sealed class CepProvidersUnavailabilityTests : BaseApiTest
{
    protected override bool UseMockGeographicValidation => false;
    protected override TestModule RequiredModules => TestModule.Locations;

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
                        "logradouro": "Rua Francisco Severiano",
                        "bairro": "Centro",
                        "localidade": "Muriaé",
                        "uf": "MG",
                        "ibge": "3143906"
                    }
                    """));

        // IBGE Mock for geographic validation
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .AtPriority(1)
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3143906,
                        "nome": "Muriaé",
                        "microrregiao": {
                            "id": 31054,
                            "nome": "Muriaé",
                            "mesorregiao": {
                                "id": 3107,
                                "nome": "Zona da Mata",
                                "UF": {
                                    "id": 31,
                                    "sigla": "MG",
                                    "nome": "Minas Gerais",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should succeed via OpenCEP fallback
        result.IsSuccess.Should().BeTrue($"Expected success but failed with error: {result.Error}");
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("Muriaé");
        result.Value.State.Should().Be("MG");
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
                        "state": "MG",
                        "city": "Muriaé",
                        "neighborhood": "Centro",
                        "street": "Rua Francisco Severiano"
                    }
                    """));

        // IBGE Mock for geographic validation
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .AtPriority(1)
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithBody("""
                    [{
                        "id": 3143906,
                        "nome": "Muriaé",
                        "microrregiao": {
                            "id": 31054,
                            "nome": "Muriaé",
                            "mesorregiao": {
                                "id": 3107,
                                "nome": "Zona da Mata",
                                "UF": {
                                    "id": 31,
                                    "sigla": "MG",
                                    "nome": "Minas Gerais",
                                    "regiao": { "id": 3, "sigla": "SE", "nome": "Sudeste" }
                                }
                            }
                        }
                    }]
                    """));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert - Should fallback to BrasilAPI
        result.IsSuccess.Should().BeTrue($"Expected success but failed with error: {result.Error}");
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("Muriaé");
        result.Value.State.Should().Be("MG");
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
