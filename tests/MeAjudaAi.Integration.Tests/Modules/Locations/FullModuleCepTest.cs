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
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"cep\":\"01310000\",\"logradouro\":\"WIRE_MOCK_STUB\",\"complemento\":\"\",\"bairro\":\"Bela Vista\",\"localidade\":\"São Paulo\",\"uf\":\"SP\",\"erro\":false}"));
        
        // Act
        var client = Services.GetRequiredService<BrasilApiCepClient>(); // Just to check BaseAddress
        var httpClientField = typeof(BrasilApiCepClient)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(HttpClient));
        
        var httpClient = (HttpClient)httpClientField!.GetValue(client)!;
        var config = Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var debugInfo = $"[DEBUG] URL: {httpClient.BaseAddress}\n";
        debugInfo += $"[CONFIG] ViaCep: {config["Locations:ExternalApis:ViaCep:BaseUrl"]}\n";

        // Test manual deserialization to see if it works here
        try 
        {
            var testJson = "{\"cep\":\"01310000\",\"logradouro\":\"WIRE_MOCK_STUB\",\"complemento\":\"\",\"bairro\":\"Bela Vista\",\"localidade\":\"São Paulo\",\"uf\":\"SP\",\"erro\":false}";
            var testResponse = global::System.Text.Json.JsonSerializer.Deserialize<MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses.ViaCepResponse>(testJson, MeAjudaAi.Shared.Serialization.SerializationDefaults.Api);
            debugInfo += $"[DEBUG] Manual Deserialization Logradouro: {testResponse?.Logradouro}\n";
        } catch(Exception ex) {
            debugInfo += $"[DEBUG] Manual Deserialization FAILED: {ex.Message}\n";
        }

        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);
        
        // Final debug: Test client directly
        var viaClient = Services.GetRequiredService<MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.ViaCepClient>();
        var directAddress = await viaClient.GetAddressAsync(MeAjudaAi.Modules.Locations.Domain.ValueObjects.Cep.Create(uniqueCep)!, default);
        debugInfo += $"[DEBUG] Direct Client Address: {directAddress?.Street ?? "NULL"}\n";

        // Assert
        if (!result.IsSuccess)
        {
            debugInfo += $"[ERROR] Result failed: {result.Error}\n";
            var logs = WireMock.Server.LogEntries;
            debugInfo += $"[DEBUG] WireMock Logs count: {logs.Count()}\n";
            foreach (var log in logs)
            {
                debugInfo += $"[WireMock Log] Request: {log.RequestMessage.Method} {log.RequestMessage.Url}\n";
                debugInfo += $"[WireMock Log] Response: {log.ResponseMessage.StatusCode}\n";
            }
        }

        await System.IO.File.WriteAllTextAsync("C:\\Code\\MeAjudaAi\\tests\\debug_logs.txt", debugInfo);

        result.IsSuccess.Should().BeTrue(because: $"Deveria ter funcionado. Erro: {result.Error}");
        result.Value!.Street.Should().Be("WIRE_MOCK_STUB");
    }
}
