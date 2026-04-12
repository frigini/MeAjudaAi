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
        var result = await locationApi.GetAddressFromCepAsync(uniqueCep);
        
        // Assert
        if (!result.IsSuccess)
        {
            var debugInfo = $"[ERROR] Result failed: {result.Error}\n";
            var logs = WireMock.Server.LogEntries;
            debugInfo += $"[DEBUG] WireMock Logs count: {logs.Count()}\n";
            foreach (var log in logs)
            {
                debugInfo += $"[WireMock Log] Request: {log.RequestMessage.Method} {log.RequestMessage.Url}\n";
                debugInfo += $"[WireMock Log] Response: {log.ResponseMessage.StatusCode}\n";
            }
            
            var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "debug_logs.txt");
            await System.IO.File.WriteAllTextAsync(logPath, debugInfo);
        }

        result.IsSuccess.Should().BeTrue(because: $"Deveria ter funcionado. Erro: {result.Error}");
        result.Value!.Street.Should().Be("WIRE_MOCK_STUB");
    }
}
