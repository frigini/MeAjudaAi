using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Integration.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Contracts.Functional;
using System.Net;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Testes de integração para cenários de indisponibilidade de provedores de CEP.
/// Valida o mecanismo de fallback e resiliência entre ViaCEP, BrasilAPI e OpenCEP.
/// </summary>
public sealed class CepProvidersUnavailabilityTests : BaseApiTest
{
    protected override bool UseMockGeographicValidation => false;
    protected override TestModule RequiredModules => TestModule.Locations;

    private static string GetUniqueCep(string prefix)
    {
        // Garante CEPs únicos por teste para evitar poluição do cache
        return prefix + Random.Shared.Next(1000, 9999).ToString("D4");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepFails_ShouldFallbackToBrasilApi()
    {
        // Arrange
        var uniqueCep = GetUniqueCep("1111");
        AuthConfig.ConfigureAdmin();

        // ViaCEP retorna erro 500
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath(new global::WireMock.Matchers.RegexMatcher($".*/ws/{uniqueCep}/json/.*"))
                .UsingGet())
            .AtPriority(1)
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        // BrasilAPI retorna sucesso
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath(new global::WireMock.Matchers.RegexMatcher($".*/api/cep/v2/{uniqueCep}.*"))
                .UsingGet())
            .AtPriority(1)
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "street": "Rua Fallback BrasilAPI",
                        "neighborhood": "Centro",
                        "city": "Muriae",
                        "state": "MG",
                        "service": "brasilapi"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();
        
        // Limpar cache para garantir que a consulta chegue aos provedores
        var cache = Services.GetRequiredService<MeAjudaAi.Shared.Caching.ICacheService>();
        await cache.RemoveAsync($"cep:{uniqueCep}");

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Deveria ter caído no fallback do BrasilAPI. Erro: {result.Error}");
        result.Value!.Street.Should().Be("Rua Fallback BrasilAPI");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepAndBrasilApiFail_ShouldFallbackToOpenCep()
    {
        // Arrange
        var uniqueCep = GetUniqueCep("2222");
        AuthConfig.ConfigureAdmin();

        // ViaCEP e BrasilAPI retornam erro
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create().WithPath(new global::WireMock.Matchers.RegexMatcher($".*/ws/{uniqueCep}/.*")).UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create().WithStatusCode(500));

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create().WithPath(new global::WireMock.Matchers.RegexMatcher($".*/api/cep/v2/{uniqueCep}.*")).UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create().WithStatusCode(503));

        // OpenCEP retorna sucesso
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath(new global::WireMock.Matchers.RegexMatcher($".*/api/cep/{uniqueCep}.json.*"))
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "logradouro": "Rua Fallback OpenCEP",
                        "bairro": "Centro",
                        "localidade": "Muriae",
                        "uf": "MG"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();
        var cache = Services.GetRequiredService<MeAjudaAi.Shared.Caching.ICacheService>();
        await cache.RemoveAsync($"cep:{uniqueCep}");

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "Deveria ter caído no último fallback (OpenCEP)");
        result.Value!.Street.Should().Be("Rua Fallback OpenCEP");
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsMalformedJson_ShouldFallbackToBrasilApi()
    {
        // Arrange
        var uniqueCep = GetUniqueCep("3333");
        AuthConfig.ConfigureAdmin();

        // ViaCEP retorna JSON inválido
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create().WithPath(new global::WireMock.Matchers.RegexMatcher($".*/ws/{uniqueCep}/.*")).UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("INVALID JSON { ..."));

        // BrasilAPI retorna sucesso
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create().WithPath(new global::WireMock.Matchers.RegexMatcher($".*/api/cep/v2/{uniqueCep}.*")).UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                    {
                        "cep": "{{uniqueCep}}",
                        "street": "Rua Apos ViaCep Invalido",
                        "neighborhood": "Centro",
                        "city": "Muriae",
                        "state": "MG"
                    }
                    """));

        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();
        var cache = Services.GetRequiredService<MeAjudaAi.Shared.Caching.ICacheService>();
        await cache.RemoveAsync($"cep:{uniqueCep}");

        // Act
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "Deveria ter ignorado o JSON malformado do ViaCEP e usado o BrasilAPI");
        result.Value!.Street.Should().Be("Rua Apos ViaCep Invalido");
    }
}
