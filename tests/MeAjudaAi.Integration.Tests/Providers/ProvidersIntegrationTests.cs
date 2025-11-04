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
                fantasyName = (string?)null,
                description = (string?)null,
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
                    complement = (string?)null,
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
        if (responseJson.TryGetProperty("data", out var dataElement))
        {
            dataElement.TryGetProperty("id", out _).Should().BeTrue(
                $"Response data should contain 'id' property. Full response: {content}");
            dataElement.TryGetProperty("name", out var nameProperty).Should().BeTrue();
            nameProperty.GetString().Should().Be("Test Provider Integration");
        }
        else
        {
            // Fallback para response direta
            responseJson.TryGetProperty("id", out _).Should().BeTrue(
                $"Response should contain 'id' property. Full response: {content}");
            responseJson.TryGetProperty("name", out var nameProperty).Should().BeTrue();
            nameProperty.GetString().Should().Be("Test Provider Integration");
        }

        // Cleanup - attempt to delete created provider
        var idElement = responseJson.TryGetProperty("data", out var data)
            ? data
            : responseJson;
        if (idElement.TryGetProperty("id", out var idProperty))
        {
            var providerId = idProperty.GetString();
            await Client.DeleteAsync($"/api/v1/providers/{providerId}");
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
    public async Task GetProviderById_WithRandomId_ShouldNotReturnServerError()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var randomId = Guid.NewGuid(); // Use random ID

        // Act
        var response = await Client.GetAsync($"/api/v1/providers/{randomId}");

        // Assert
        // Should not return server error - can be NotFound, OK, or other client errors
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden);

        // Should be a valid response (either found or not found, no validation errors)
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.NotFound,
            System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProvidersByType_ShouldReturnFilteredList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act - Testar busca por tipo Individual
        var response = await Client.GetAsync("/api/v1/providers/by-type/Individual");

        // Assert
        var errorContent = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            $"because the endpoint should return OK. Response: {errorContent}");

        var content = await response.Content.ReadAsStringAsync();
        var providers = JsonSerializer.Deserialize<JsonElement>(content);

        // Accept empty list or proper response structure
        // For empty list, the API may return an array or an object with data property
        var isValidResponse = providers.ValueKind == JsonValueKind.Array ||
                              (providers.ValueKind == JsonValueKind.Object && (
                                  providers.TryGetProperty("items", out _) ||
                                  providers.TryGetProperty("data", out _) ||
                                  providers.TryGetProperty("message", out _)
                              ));

        isValidResponse.Should().BeTrue($"Invalid response format. Content: {content}");
    }

    [Fact]
    public async Task GetProvidersByVerificationStatus_ShouldReturnFilteredList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act - Testar busca por status Pending
        var response = await Client.GetAsync("/api/v1/providers/by-verification-status/Pending");

        // Assert
        var errorContent = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            $"because the endpoint should return OK. Response: {errorContent}");

        var content = await response.Content.ReadAsStringAsync();
        var providers = JsonSerializer.Deserialize<JsonElement>(content);

        // Accept empty list or proper response structure
        var isValidResponse = providers.ValueKind == JsonValueKind.Array ||
                              (providers.ValueKind == JsonValueKind.Object && (
                                  providers.TryGetProperty("items", out _) ||
                                  providers.TryGetProperty("data", out _) ||
                                  providers.TryGetProperty("message", out _)
                              ));

        isValidResponse.Should().BeTrue($"Invalid response format. Content: {content}");
    }

    [Fact]
    public async Task ProvidersEndpoints_ShouldNotReturn500Errors()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/providers",
            "/api/v1/providers/by-type/Individual",
            "/api/v1/providers/by-verification-status/Pending",
            "/api/v1/providers/by-city/São Paulo",
            "/api/v1/providers/by-state/SP"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);

            // O importante é que não seja erro 500 (erro de servidor)
            response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);
            response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.MethodNotAllowed);
            // Accept OK/Unauthorized/Forbidden/NotFound depending on auth/seed state

            // Log do status para debugging se necessário
            if (!response.IsSuccessStatusCode)
            {
                testOutput.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}");
            }
        }
    }
}
