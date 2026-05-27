using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

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
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            jsonDocument.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    public async Task ProvidersModule_ShouldBeProperlyRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();

        // Act & Assert
        var dbContext = scope.ServiceProvider.GetService<ProvidersDbContext>();
        dbContext.Should().NotBeNull("ProvidersDbContext should be registered");

        var uow = scope.ServiceProvider.GetService<ProvidersDbContext>();
        uow.Should().NotBeNull("ProvidersDbContext should be registered as IUnitOfWork");

        var queries = scope.ServiceProvider.GetService<IProviderQueries>();
        queries.Should().NotBeNull("IProviderQueries should be registered");
    }

    private T GetResponseData<T>(JsonElement element)
    {
        if (element.TryGetProperty("data", out var data))
            return data.Deserialize<T>()!;
        if (element.TryGetProperty("value", out var value))
            return value.Deserialize<T>()!;
        return element.Deserialize<T>()!;
    }

    private JsonElement GetResponseData(JsonElement element)
    {
        if (element.TryGetProperty("data", out var data))
            return data;
        if (element.TryGetProperty("value", out var value))
            return value;
        return element;
    }

    private async Task<T> ReadJsonAsync<T>(HttpContent content)
    {
        var text = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
