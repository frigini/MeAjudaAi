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
public sealed class CepProvidersUnavailabilityTests(ITestOutputHelper output) : BaseApiTest
{
    protected override bool UseMockGeographicValidation => false;
    protected override TestModule RequiredModules => TestModule.Locations;

    private void LogWireMockEntries()
    {
        output.WriteLine("--- WireMock Log Entries ---");
        foreach (var entry in WireMock.Server.LogEntries)
        {
            output.WriteLine($"Request: {entry.RequestMessage.Method} {entry.RequestMessage.Url}");
            output.WriteLine($"Response: {entry.ResponseMessage?.StatusCode}");
            output.WriteLine("----------------------------");
        }
    }

    [Fact]
    public async Task LookupCep_WhenViaCepFails_ShouldFallbackToBrasilApi()
    {
        try
        {
            // Arrange
            var uniqueCep = "11110001";
            AuthConfig.ConfigureAdmin();

            // 1. ViaCEP retorna erro 500
            WireMock.Server
                .Given(global::WireMock.RequestBuilders.Request.Create()
                    .WithPath($"/ws/{uniqueCep}/json/")
                    .UsingGet())
                .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                    .WithStatusCode(500));

            // 2. BrasilAPI retorna sucesso
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
                            "street": "Rua Fallback BrasilAPI",
                            "neighborhood": "Centro",
                            "city": "Muriaé",
                            "state": "MG"
                        }
                        """));

            var locationApi = Services.GetRequiredService<ILocationsModuleApi>();
            
            // Limpar cache
            var cache = Services.GetRequiredService<MeAjudaAi.Shared.Caching.ICacheService>();
            await cache.RemoveAsync($"cep:{uniqueCep}");

            // Act
            var result = await locationApi.GetAddressFromCepAsync(uniqueCep);

            // Assert
            result.IsSuccess.Should().BeTrue(because: $"Deveria ter caído no fallback do BrasilAPI. Erro: {result.Error}");
            result.Value!.Street.Should().Be("Rua Fallback BrasilAPI");
        }
        catch
        {
            LogWireMockEntries();
            throw;
        }
    }

    [Fact]
    public async Task LookupCep_WhenViaCepAndBrasilApiFail_ShouldFallbackToOpenCep()
    {
        try
        {
            // Arrange
            var uniqueCep = "22220002";
            AuthConfig.ConfigureAdmin();

            // 1. ViaCEP e BrasilAPI retornam erro
            WireMock.Server
                .Given(global::WireMock.RequestBuilders.Request.Create().WithPath($"/ws/{uniqueCep}/json/").UsingGet())
                .RespondWith(global::WireMock.ResponseBuilders.Response.Create().WithStatusCode(500));

            WireMock.Server
                .Given(global::WireMock.RequestBuilders.Request.Create().WithPath($"/api/cep/v2/{uniqueCep}").UsingGet())
                .RespondWith(global::WireMock.ResponseBuilders.Response.Create().WithStatusCode(500));

            // 2. OpenCEP retorna sucesso
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
                            "logradouro": "Rua Fallback OpenCEP",
                            "bairro": "Centro",
                            "localidade": "Muriaé",
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
        catch
        {
            LogWireMockEntries();
            throw;
        }
    }

    [Fact]
    public async Task LookupCep_WhenViaCepReturnsMalformedJson_ShouldFallbackToBrasilApi()
    {
        try
        {
            // Arrange
            var uniqueCep = "33330003";
            AuthConfig.ConfigureAdmin();

            // 1. ViaCEP retorna JSON inválido
            WireMock.Server
                .Given(global::WireMock.RequestBuilders.Request.Create().WithPath($"/ws/{uniqueCep}/json/").UsingGet())
                .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                    .WithStatusCode(200)
                    .WithBody("INVALID JSON { ..."));

            // 2. BrasilAPI retorna sucesso
            WireMock.Server
                .Given(global::WireMock.RequestBuilders.Request.Create().WithPath($"/api/cep/v2/{uniqueCep}").UsingGet())
                .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody($$"""
                        {
                            "cep": "{{uniqueCep}}",
                            "street": "Rua Apos ViaCep Invalido",
                            "neighborhood": "Centro",
                            "city": "Muriaé",
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
        catch
        {
            LogWireMockEntries();
            throw;
        }
    }
}

