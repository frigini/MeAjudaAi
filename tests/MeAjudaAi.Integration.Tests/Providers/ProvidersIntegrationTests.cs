using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Providers;

/// <summary>
/// 🧪 TESTES DE INTEGRAÇÃO PARA O MÓDULO PROVIDERS
/// 
/// Valida as funcionalidades implementadas do módulo Providers:
/// - Criação de prestadores de serviços
/// - Consulta de prestadores
/// - Soft Delete de prestadores
/// - Gerenciamento de documentos e qualificações
/// </summary>
public class ProvidersIntegrationTests(ITestOutputHelper testOutput) : InstanceApiTestBase
{
    [Fact]
    public async Task CreateProvider_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var providerData = new
        {
            userId = Guid.NewGuid(),
            name = "Test Provider Integration",
            type = 0, // Individual
            businessProfile = new
            {
                legalName = "Test Company LTDA",
                contactInfo = new
                {
                    email = "test@provider.com",
                    phoneNumber = "+55 11 99999-9999",
                    website = "https://www.provider.com"
                },
                primaryAddress = new
                {
                    street = "Rua Teste",
                    number = "123",
                    neighborhood = "Centro",
                    city = "São Paulo",
                    state = "SP",
                    zipCode = "01234-567",
                    country = "Brasil"
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers", providerData);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created,
            "POST requests that create resources should return 201 Created");
        // Optionally verify Location:
        // response.Headers.Location.Should().NotBeNull();

        var responseJson = JsonSerializer.Deserialize<JsonElement>(content);

        // Verifica se é uma response estruturada (com data)
        var dataElement = GetResponseData(responseJson);
        dataElement.TryGetProperty("id", out _).Should().BeTrue(
            $"Response data should contain 'id' property. Full response: {content}");
        dataElement.TryGetProperty("name", out var nameProperty).Should().BeTrue();
        nameProperty.GetString().Should().Be("Test Provider Integration");

        // Cleanup - attempt to delete created provider
        var idElement = GetResponseData(responseJson);
        if (idElement.TryGetProperty("id", out var idProperty))
        {
            var providerId = idProperty.GetString();
            var deleteResponse = await Client.DeleteAsync($"/api/v1/providers/{providerId}");
            if (!deleteResponse.IsSuccessStatusCode)
            {
                testOutput.WriteLine($"Cleanup failed: Could not delete provider {providerId}. Status: {deleteResponse.StatusCode}");
            }
        }
        // When not success, the previous assertion will fail and surface content in the test log.
    }

    [Fact]
    public async Task GetProviders_ShouldReturnProvidersList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        var providers = JsonSerializer.Deserialize<JsonElement>(content);

        // Expect a consistent API response format - should be an object with data property
        providers.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");
        providers.TryGetProperty("data", out var dataElement).Should().BeTrue(
            "Response should contain 'data' property for consistency");
        dataElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        dataElement.ValueKind.Should().NotBe(JsonValueKind.Null,
            "Data property should contain either an array of providers or a paginated response object");
    }

    [Fact]
    public async Task GetProviderById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var randomId = Guid.NewGuid(); // Use random ID that definitely doesn't exist

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/{randomId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound,
            "API should return 404 when provider ID does not exist");
    }

    [Fact]
    public async Task GetProvidersByType_ShouldReturnOnlyIndividualProviders()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Seed test data: create Individual and Company providers
        var individualProvider = await CreateTestProvider("Individual Provider", type: 0); // Individual
        var companyProvider = await CreateTestProvider("Company Provider", type: 1); // Company

        try
        {
            // Act - Test filtering by Individual type
            var response = await Client.GetAsync("/api/v1/providers/by-type/Individual");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
                $"because the endpoint should return OK. Response: {content}");

            var providers = JsonSerializer.Deserialize<JsonElement>(content);
            var dataElement = GetResponseData(providers);

            if (dataElement.ValueKind == JsonValueKind.Array)
            {
                // Verify only Individual providers are returned
                foreach (var providerElement in dataElement.EnumerateArray())
                {
                    if (providerElement.TryGetProperty("type", out var typeProperty))
                    {
                        typeProperty.GetInt32().Should().Be(0, "Only Individual providers (type 0) should be returned");
                    }
                }
            }
        }
        finally
        {
            // Cleanup
            await CleanupProvider(individualProvider);
            await CleanupProvider(companyProvider);
        }
    }

    [Fact]
    public async Task GetProvidersByVerificationStatus_ShouldReturnOnlyPendingProviders()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Note: Since we can't directly set verification status during creation,
        // this test validates response structure and format only
        // TODO: [ISSUE] Enhance test to verify actual filtering behavior when verification 
        // status management endpoints are available (update/approve/reject provider verification)

        // Act - Test filtering by Pending verification status
        var response = await Client.GetAsync("/api/v1/providers/by-verification-status/Pending");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            $"because the endpoint should return OK. Response: {content}");

        var providers = JsonSerializer.Deserialize<JsonElement>(content);

        // Accept empty list or proper response structure
        var isValidResponse = providers.ValueKind == JsonValueKind.Array ||
                              (providers.ValueKind == JsonValueKind.Object &&
                                  providers.TryGetProperty("data", out _));

        isValidResponse.Should().BeTrue($"Invalid response format. Content: {content}");
    }

    [Fact]
    public async Task ProvidersEndpoints_AdminUser_ShouldNotReturnAuthorizationOrServerErrors()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/providers",
            "/api/v1/providers/by-type/Individual",
            "/api/v1/providers/by-verification-status/Pending"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                testOutput.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}. Body: {body}");
            }

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
                $"Authenticated admin requests to {endpoint} should succeed.");
        }
    }

    private static JsonElement GetResponseData(JsonElement response)
    {
        return response.TryGetProperty("data", out var dataElement)
            ? dataElement
            : response;
    }

    private async Task<string?> CreateTestProvider(string name, int type)
    {
        var providerData = new
        {
            userId = Guid.NewGuid(),
            name = name,
            type = type,
            businessProfile = new
            {
                legalName = $"{name} LTDA",
                contactInfo = new
                {
                    email = $"test-{Guid.NewGuid():N}@provider.com",
                    phoneNumber = "+55 11 99999-9999",
                    website = "https://www.provider.com"
                },
                primaryAddress = new
                {
                    street = "Rua Teste",
                    number = "123",
                    neighborhood = "Centro",
                    city = "São Paulo",
                    state = "SP",
                    zipCode = "01234-567",
                    country = "Brasil"
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/v1/providers", providerData);
        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(content);
            var dataElement = GetResponseData(responseJson);
            if (dataElement.TryGetProperty("id", out var idProperty))
            {
                return idProperty.GetString();
            }
        }

        testOutput.WriteLine($"Failed to create test provider {name}. Status: {response.StatusCode}");
        return null;
    }

    private async Task CleanupProvider(string? providerId)
    {
        if (string.IsNullOrEmpty(providerId)) return;

        var deleteResponse = await Client.DeleteAsync($"/api/v1/providers/{providerId}");
        if (!deleteResponse.IsSuccessStatusCode)
        {
            testOutput.WriteLine($"Cleanup failed: Could not delete provider {providerId}. Status: {deleteResponse.StatusCode}");
        }
    }
}
