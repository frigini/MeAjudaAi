using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes de funcionalidades implementadas do módulo Providers
/// </summary>
/// <remarks>
/// Verifica se as funcionalidades principais estão funcionando:
/// - Endpoints estão acessíveis
/// - Respostas estão no formato correto
/// - Autorização está funcionando
/// - Dados são persistidos corretamente
/// </remarks>
public class ImplementedFeaturesTests : ApiTestBase
{
    [Fact]
    public async Task ProvidersEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProvidersEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Admin users should receive a successful response");

        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Should be either a list or a paginated result
        jsonDocument.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);

        if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
        {
            // If it's an object, it should have pagination properties
            var hasItems = jsonDocument.RootElement.TryGetProperty("items", out _);
            var hasTotalCount = jsonDocument.RootElement.TryGetProperty("totalCount", out _);
            var hasPage = jsonDocument.RootElement.TryGetProperty("page", out _);

            // Check if it's wrapped in a "data" property (API response wrapper)
            var hasDataWrapper = jsonDocument.RootElement.TryGetProperty("data", out var dataElement);
            if (hasDataWrapper && dataElement.ValueKind == JsonValueKind.Object)
            {
                var dataHasItems = dataElement.TryGetProperty("items", out _);
                var dataHasTotalCount = dataElement.TryGetProperty("totalCount", out _);
                var dataHasPage = dataElement.TryGetProperty("page", out _);

                (dataHasItems || dataHasTotalCount || dataHasPage).Should().BeTrue("Should be a paginated result in data wrapper");
            }
            else
            {
                (hasItems || hasTotalCount || hasPage).Should().BeTrue("Should be a paginated result");
            }
        }
    }

    // NOTE: ProvidersEndpoint_ShouldSupportPagination removed - duplicates ProvidersApiTests.ProvidersEndpoint_ShouldSupportPagination

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportFilters()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?name=test&type=1&verificationStatus=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Filter parameters should be accepted under admin");
    }

    // NOTE: GetProviderById_Endpoint_ShouldExist removed - duplicates ProvidersApiTests.GetProviderById_Endpoint_ShouldExist
    // NOTE: CreateProvider_Endpoint_ShouldExist removed - duplicates ProvidersApiTests.CreateProvider_Endpoint_ShouldExist
    // NOTE: ProvidersModule_ShouldBeProperlyRegistered removed - duplicates ProvidersApiTests.ProvidersModule_ShouldBeProperlyRegistered
    // NOTE: HealthCheck_ShouldIncludeProvidersDatabase removed - duplicates ProvidersApiTests.HealthCheck_ShouldIncludeProvidersDatabase
}
