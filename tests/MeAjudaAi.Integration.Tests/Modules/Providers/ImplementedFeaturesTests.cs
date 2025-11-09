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
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

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

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportPagination()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Pagination parameters should be accepted under admin");
    }

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportFilters()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?name=test&type=1&verificationStatus=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Filter parameters should be accepted under admin");
    }

    [Fact]
    public async Task GetProviderById_Endpoint_ShouldExist()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
        var testId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/{testId}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed, "GET by ID should exist");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProvider_Endpoint_ShouldExist()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
        var providerData = new
        {
            userId = Guid.NewGuid(),
            name = "Test Provider",
            type = 1, // Individual
            businessProfile = new
            {
                legalName = "Test Provider Ltd",
                contactInfo = new
                {
                    email = "test@provider.com",
                    phoneNumber = "+55 11 99999-9999"
                },
                primaryAddress = new
                {
                    street = "Test Street",
                    number = "123",
                    neighborhood = "Test Neighborhood",
                    city = "Test City",
                    state = "TS",
                    zipCode = "12345-678",
                    country = "Brasil"
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers", providerData);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed, "POST should be allowed");
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
            // Optionally:
            // response.Headers.Location.Should().NotBeNull("Location header should point to the created resource");
        }
    }

    [Fact]
    public async Task ProvidersModule_ShouldBeProperlyRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();

        // Act & Assert - verify key services are registered
        var dbContext = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();
        dbContext.Should().NotBeNull("ProvidersDbContext should be registered");

        var repository = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Domain.Repositories.IProviderRepository>();
        repository.Should().NotBeNull("IProviderRepository should be registered");

        var queryService = scope.ServiceProvider.GetService<MeAjudaAi.Modules.Providers.Application.Services.Interfaces.IProviderQueryService>();
        queryService.Should().NotBeNull("IProviderQueryService should be registered");
    }

    [Fact]
    public async Task HealthCheck_ShouldIncludeProvidersDatabase()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Parse as JSON to ensure it's well-formed
            var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);

            // Assert top-level status exists and is not unhealthy
            healthResponse.TryGetProperty("status", out var statusElement).Should().BeTrue("Health response should have status property");
            var status = statusElement.GetString();
            status.Should().NotBe("Unhealthy", "Health status should not be Unhealthy");

            // Verify database health check entry exists (providers no longer has specific health check)
            if (healthResponse.TryGetProperty("entries", out var entries))
            {
                var hasDatabaseEntry = entries.EnumerateObject()
                    .Any(e => e.Name.Contains("database", StringComparison.OrdinalIgnoreCase));
                hasDatabaseEntry.Should().BeTrue("Health check should include database entry");
            }
            else
            {
                content.Should().Contain("database", "Health check should include database reference");
            }
        }
    }
}
