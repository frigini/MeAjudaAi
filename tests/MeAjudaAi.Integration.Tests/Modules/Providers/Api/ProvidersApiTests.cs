using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Providers.Api;

/// <summary>
/// Testes de integração para a API do módulo Providers.
/// Valida formato de resposta e estrutura da API.
/// </summary>
public class ProvidersApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers | TestModule.ServiceCatalogs | TestModule.Users | TestModule.SearchProviders;

    [Fact]
    public async Task ProvidersEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        jsonDocument.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    public async Task ProvidersModule_ShouldBeProperlyRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();

        // Act & Assert
        var dbContext = scope.ServiceProvider.GetService<ProvidersDbContext>();
        dbContext.Should().NotBeNull("ProvidersDbContext should be registered");

        var uow = scope.ServiceProvider.GetService<IUnitOfWork>();
        uow.Should().NotBeNull("IUnitOfWork should be registered");

        var queries = scope.ServiceProvider.GetService<IProviderQueries>();
        queries.Should().NotBeNull("IProviderQueries should be registered");
    }
}


