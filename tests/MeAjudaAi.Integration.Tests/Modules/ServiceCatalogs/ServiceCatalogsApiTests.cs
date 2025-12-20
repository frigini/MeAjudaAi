using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Testes de integração para a API do módulo ServiceCatalogs.
/// Valida formato de resposta e estrutura da API.
/// </summary>
/// <remarks>
/// Testes de endpoints, autenticação e CRUD são cobertos por ServiceCatalogsIntegrationTests.cs
/// </remarks>
public class ServiceCatalogsApiTests : ApiTestBase
{
    [Fact]
    public async Task ServiceCategoriesEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/categories");

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Log error details if not successful
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content: {content}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Admin users should receive a successful response. Error: {content}");

        var categories = JsonSerializer.Deserialize<JsonElement>(content);

        // Expect a consistent API response format
        categories.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");
        categories.TryGetProperty("data", out var dataElement).Should().BeTrue(
            "Response should contain 'data' property for consistency");
        dataElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    public async Task ServicesEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Admin users should receive a successful response");

        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<JsonElement>(content);

        // Expect a consistent API response format
        services.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");
        services.TryGetProperty("data", out var dataElement).Should().BeTrue(
            "Response should contain 'data' property for consistency");
        dataElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    public async Task CreateServiceCategory_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var categoryData = new
        {
            name = $"Test Category {Guid.NewGuid():N}",
            description = "Test Description",
            displayOrder = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryData);

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"POST requests that create resources should return 201 Created. Response: {content}");

        var responseJson = JsonSerializer.Deserialize<JsonElement>(content);
        var dataElement = GetResponseData(responseJson);
        dataElement.TryGetProperty("id", out _).Should().BeTrue(
            $"Response data should contain 'id' property. Full response: {content}");
        dataElement.TryGetProperty("name", out var nameProperty).Should().BeTrue();
        nameProperty.GetString().Should().Be(categoryData.name);

        // Cleanup
        if (dataElement.TryGetProperty("id", out var idProperty))
        {
            var categoryId = idProperty.GetString();
            await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");
        }
    }

    [Fact]
    public async Task CreateService_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // First create a category
        var categoryData = new
        {
            name = $"Test Category {Guid.NewGuid():N}",
            description = "Test Description",
            displayOrder = 1
        };

        var categoryResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryData);
        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
        var categoryJson = JsonSerializer.Deserialize<JsonElement>(categoryContent);
        var categoryDataElement = GetResponseData(categoryJson);
        categoryDataElement.TryGetProperty("id", out var categoryIdProperty).Should().BeTrue();
        var categoryId = categoryIdProperty.GetString()!;

        try
        {
            // Now create a service
            var serviceData = new
            {
                name = $"Test Service {Guid.NewGuid():N}",
                description = "Test Service Description",
                categoryId = categoryId
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceData);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                $"POST requests that create resources should return 201 Created. Response: {content}");

            var responseJson = JsonSerializer.Deserialize<JsonElement>(content);
            var dataElement = GetResponseData(responseJson);
            dataElement.TryGetProperty("id", out _).Should().BeTrue(
                $"Response data should contain 'id' property. Full response: {content}");
            dataElement.TryGetProperty("name", out var nameProperty).Should().BeTrue();
            nameProperty.GetString().Should().Be(serviceData.name);

            // Cleanup service
            if (dataElement.TryGetProperty("id", out var serviceIdProperty))
            {
                var serviceId = serviceIdProperty.GetString();
                await Client.DeleteAsync($"/api/v1/service-catalogs/services/{serviceId}");
            }
        }
        finally
        {
            // Cleanup category
            await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");
        }
    }

    [Fact]
    public async Task CatalogsEndpoints_AdminUser_ShouldNotReturnAuthorizationOrServerErrors()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/service-catalogs/categories",
            "/api/v1/service-catalogs/services"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}. Body: {body}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Authenticated admin requests to {endpoint} should succeed.");
        }
    }

    [Fact]
    public async Task ServicesEndpoint_WithPagination_ShouldAcceptParameters()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/services?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest,
            "Valid pagination parameters should be accepted");
    }

    [Fact]
    public async Task CategoriesEndpoint_WithPagination_ShouldAcceptParameters()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/categories?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest,
            "Valid pagination parameters should be accepted");
    }

    [Fact]
    public async Task CreateCategory_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var createRequest = new { name = "Test Category" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", createRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    private static JsonElement GetResponseData(JsonElement response)
    {
        return response.TryGetProperty("data", out var dataElement)
            ? dataElement
            : response;
    }
}

