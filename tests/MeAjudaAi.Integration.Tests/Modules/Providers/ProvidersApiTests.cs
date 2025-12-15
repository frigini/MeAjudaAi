using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes de integração para a API do módulo Providers.
/// Valida formato de resposta e estrutura da API.
/// </summary>
/// <remarks>
/// Testes de endpoints, autenticação e CRUD são cobertos por ProvidersIntegrationTests.cs
/// </remarks>
public class ProvidersApiTests : ApiTestBase
{
    // NOTE: ProvidersEndpoint_ShouldBeAccessible removed - low value smoke test

    [Fact]
    public async Task ProvidersEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        // Não deve ser um erro de servidor independentemente de sucesso/falha
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "Server should not crash on basic endpoint access");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);

            // Deve ser uma lista ou um resultado paginado
            jsonDocument.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);

            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
            {
                // Se for um objeto, deve ter propriedades de paginação
                var hasItems = jsonDocument.RootElement.TryGetProperty("items", out _);
                var hasTotalCount = jsonDocument.RootElement.TryGetProperty("totalCount", out _);
                var hasPage = jsonDocument.RootElement.TryGetProperty("page", out _);

                // Verifica se está envolvido em uma propriedade "data" (wrapper de resposta da API)
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
    }

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportPagination()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest,
            "Should accept pagination parameters");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "Should not crash with pagination parameters");
    }

    [Fact]
    public async Task ProvidersEndpoint_ShouldSupportFilters()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?name=test&type=1&verificationStatus=1");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest,
            "Should accept filter parameters");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "Should not crash with filter parameters");
    }

    [Fact]
    public async Task GetProviderById_Endpoint_ShouldExist()
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/{testId}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed,
            "GET by ID endpoint should exist");

        // Pode ser 404 (not found), 401 (unauthorized), 400 (bad request), ou 200 (ok), mas não 405 ou 500
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProvider_Endpoint_ShouldExist()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
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
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed,
            "POST endpoint should exist");

        // Pode falhar com vários erros, mas o método deve ser permitido
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProvidersModule_ShouldBeProperlyRegistered()
    {
        // Arrange
        using var scope = Services.CreateScope();

        // Act & Assert - verifica se os serviços principais estão registrados
        var dbContext = scope.ServiceProvider.GetService<ProvidersDbContext>();
        dbContext.Should().NotBeNull("ProvidersDbContext should be registered");

        var repository = scope.ServiceProvider.GetService<IProviderRepository>();
        repository.Should().NotBeNull("IProviderRepository should be registered");

        var queryService = scope.ServiceProvider.GetService<IProviderQueryService>();
        queryService.Should().NotBeNull("IProviderQueryService should be registered");
    }

    [Fact]
    public async Task HealthCheck_ShouldIncludeProvidersDatabase()
    {
        // Act - /health/ready includes database checks, /health only has external services
        var response = await Client.GetAsync("/health/ready");

        // Assert
        var allowedStatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable };
        response.StatusCode.Should().BeOneOf(allowedStatusCodes,
            because: "ready check can return 200 (healthy) or 503 (database unavailable)");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            because: "health check should not crash with 500");

        var content = await response.Content.ReadAsStringAsync();

        // Analisa como JSON para garantir que está bem formado
        var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);

        // Verifica se o status de nível superior existe
        healthResponse.TryGetProperty("status", out var statusElement).Should().BeTrue(
            because: "health response should have status property");
        var status = statusElement.GetString();
        status.Should().NotBeNullOrEmpty(because: "health status should be present");

        // Verifica se a entrada do health check de database existe
        // API retorna 'checks' ao invés de 'entries'
        if (healthResponse.TryGetProperty("checks", out var checks) &&
            checks.ValueKind == JsonValueKind.Array)
        {
            var checksArray = checks.EnumerateArray().ToArray();
            checksArray.Should().NotBeEmpty(because: "/health/ready should have health checks");
            
            var databaseCheck = checksArray
                .FirstOrDefault(check =>
                {
                    if (check.TryGetProperty("name", out var nameElement))
                    {
                        var name = nameElement.GetString() ?? string.Empty;
                        return name.Contains("database", StringComparison.OrdinalIgnoreCase) ||
                               name.Contains("postgres", StringComparison.OrdinalIgnoreCase) ||
                               name.Contains("npgsql", StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                });
            
            databaseCheck.ValueKind.Should().NotBe(JsonValueKind.Undefined,
                because: "/health/ready should include database/postgres health check");

            // Optionally verify the database check's status for stronger guarantees
            if (response.StatusCode == HttpStatusCode.OK &&
                databaseCheck.TryGetProperty("status", out var dbStatusElement))
            {
                var dbStatus = dbStatusElement.GetString();
                dbStatus.Should().NotBeNullOrEmpty(
                    because: "database health check should report a status when readiness is OK");
            }
        }
        else
        {
            // If checks structure is missing/unexpected, fail explicitly with full response
            Assert.Fail(
                $"Health check response missing expected 'checks' array. " +
                $"This may indicate a breaking change in the health check API. " +
                $"Raw response: {content}");
        }
    }

    // NOTE: GetProviderById_WithNonExistentId is covered by ProvidersIntegrationTests.cs

    [Fact]
    public async Task ProvidersEndpoint_WithPaginationParameters_ShouldAcceptThem()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest,
            "Valid pagination parameters should be accepted");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ProvidersEndpoint_WithFilterParameters_ShouldAcceptThem()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act - test with common filter parameters
        var response = await Client.GetAsync("/api/v1/providers?city=SaoPaulo&state=SP");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest,
            "Valid filter parameters should be accepted");
    }

    [Fact]
    public async Task ProvidersEndpoint_UnauthorizedUser_ShouldReturnUnauthorized()
    {
        // Arrange - no authentication configured

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProvider_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var createRequest = new { Name = "Test Provider" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers", createRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }
}

