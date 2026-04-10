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
        // Arrange - Usa um CEP único para evitar conflitos com stubs padrão
        var uniqueCep = "34567890";

        // ViaCEP retorna JSON inválido/vazio (campos obrigatórios ausentes disparam falha de desserialização)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .AtPriority(1) // Prioridade maior que os stubs padrão
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")); // JSON vazio carece de campos obrigatórios, causando falha na validação

        // BrasilAPI também retorna JSON inválido/vazio
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")); // Empty JSON lacks required fields, causing validation to fail

        // OpenCEP retorna sucesso
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

        // Mock do IBGE para validação geográfica
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", new global::WireMock.Matchers.RegexMatcher("(?i)^muria(%C3%A9|\u00E9|e)$", true))
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
        // Arrange - Todos os provedores falham para um CEP único para evitar hits de cache
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

        // Assert - Deve retornar falha quando todos os provedores estão fora do ar
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsMalformedJson_ShouldFallbackToBrasilApi()
    {
        // Arrange - Usa CEP único para evitar conflitos com stubs padrão
        var uniqueCep = "12345678";

        // ViaCEP retorna JSON malformado
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{invalid json}"));

        // BrasilAPI retorna sucesso
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

        // Mock do IBGE para validação geográfica
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", new global::WireMock.Matchers.RegexMatcher("(?i)^muria(%C3%A9|\u00E9|e)$", true))
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

        // Assert - Deve fazer fallback para BrasilAPI
        result.IsSuccess.Should().BeTrue($"Expected success but failed with error: {result.Error}");
        result.Value.Should().NotBeNull();
        result.Value!.City.Should().Be("Muriaé");
        result.Value.State.Should().Be("MG");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsErrorTrueAndOthersFail_ShouldReturnFailure()
    {
        // Arrange - ViaCEP retorna "erro: true" para CEP inválido
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/ws/00000000/json/")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"erro": true}"""));

        // BrasilAPI também falha (404 para CEP inválido - comportamento v2)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/cep/v2/00000000")
                .UsingGet())
            .AtPriority(1) // Higher priority than default stubs
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(404)
                .WithBody("CEP não encontrado"));

        // OpenCEP também falha
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

        // Assert - Deve retornar falha para um CEP realmente inválido
        result.IsSuccess.Should().BeFalse();
    }
}
