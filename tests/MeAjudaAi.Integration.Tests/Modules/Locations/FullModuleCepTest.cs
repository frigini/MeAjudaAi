using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using System.Net.Http;
using System.Reflection;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

public sealed class FullModuleCepTest : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations;

    [Fact]
    public async Task GetAddressFromCepAsync_ShouldWorkWithWireMock()
    {
        // Arrange
        var locationApi = Services.GetRequiredService<ILocationsModuleApi>();
        var uniqueCep = "01310000"; // CEP Real da Avenida Paulista para testar se está batendo na internet
        
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/ws/{uniqueCep}/json/")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)); // ViaCep Fails

        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"cep\":\"01310000\",\"street\":\"WIRE_MOCK_STUB\",\"neighborhood\":\"Test\",\"city\":\"Test\",\"state\":\"MG\"}"));
        
        // Act
        var client = Services.GetRequiredService<BrasilApiCepClient>();
        // Usar reflexão para extrair o HttpClient interno buscando por tipo, pois o nome do campo gerado por Primary Constructor é especial
        var httpClientField = typeof(BrasilApiCepClient)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(HttpClient));
        
        var httpClient = (HttpClient)httpClientField!.GetValue(client)!;
        System.Console.WriteLine($"[DEBUG] BrasilApiCepClient BaseAddress: {httpClient.BaseAddress}");

        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);
        
        // Assert
        if (!result.IsSuccess)
        {
            var logs = WireMock.Server.LogEntries;
            foreach (var log in logs)
            {
                System.Console.WriteLine($"[WireMock Log] Request: {log.RequestMessage.Method} {log.RequestMessage.Url}");
                System.Console.WriteLine($"[WireMock Log] Response: {log.ResponseMessage.StatusCode}");
            }
        }

        result.IsSuccess.Should().BeTrue(because: $"Deveria ter funcionado. Erro: {result.Error}");
        result.Value!.Street.Should().Be("WIRE_MOCK_STUB");
    }
}
