using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

public sealed class BrasilApiConfigTest : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations;

    [Fact]
    public async Task BrasilApi_ShouldUseWireMockUrl()
    {
        // Arrange
        var client = Services.GetRequiredService<BrasilApiCepClient>();
        
        var uniqueCep = "36880000";
        // Stub no WireMock (acessível via propriedade protegida na base se estiver correta, ou via WireMockServer)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath($"/api/cep/v2/{uniqueCep}")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"cep\":\"36880000\",\"street\":\"Test\",\"neighborhood\":\"Test\",\"city\":\"Test\",\"state\":\"MG\"}"));
        
        // Act
        var cepVo = Cep.Create(uniqueCep);
        var address = await client.GetAddressAsync(cepVo!, default);
        
        // Assert
        address.Should().NotBeNull(because: "O cliente deveria ter atingido o WireMock e retornado o endereço mockado");
        address!.Street.Should().Be("Test");
    }
}
