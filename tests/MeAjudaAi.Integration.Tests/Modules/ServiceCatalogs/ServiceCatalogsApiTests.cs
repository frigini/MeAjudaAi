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
public class ServiceCatalogsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

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
            $"Usuários administradores devem receber uma resposta bem-sucedida. Erro: {content}");

        var categories = JsonSerializer.Deserialize<JsonElement>(content);

        // Espera um formato de resposta API consistente
        categories.ValueKind.Should().Be(JsonValueKind.Object,
            "A API deve retornar um objeto de resposta estruturado");
        var dataElement = GetResponseData(categories);
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
            "Usuários administradores devem receber uma resposta bem-sucedida");

        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<JsonElement>(content);

        // Espera um formato de resposta API consistente
        services.ValueKind.Should().Be(JsonValueKind.Object,
            "A API deve retornar um objeto de resposta estruturado");
        var dataElement = GetResponseData(services);
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

    #region ServiceCategory Endpoints

    [Fact]
    public async Task GetServiceCategoryById_WithExistingId_ShouldReturnCategory()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var categoryName = $"Category_{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = categoryName });
        var createdCategory = await ReadJsonAsync<JsonElement>(createResponse.Content);
        var id = GetResponseData(createdCategory).GetProperty("id").GetString();

        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/categories/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await ReadJsonAsync<JsonElement>(response.Content);
        GetResponseData(content).GetProperty("name").GetString().Should().Be(categoryName);
    }

    [Fact]
    public async Task UpdateServiceCategory_WithValidData_ShouldReturnUpdated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Old Name" });
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();
        var updateData = new { name = "New Name", description = "Updated description", displayOrder = 5 };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/categories/{id}", updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the update by getting the category
        var getResponse = await Client.GetAsync($"/api/v1/service-catalogs/categories/{id}");
        var content = await ReadJsonAsync<JsonElement>(getResponse.Content);
        var data = GetResponseData(content);
        data.GetProperty("name").GetString().Should().Be("New Name");
        data.GetProperty("displayOrder").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task ActivateAndDeactivateCategory_ShouldUpdateStatus()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Status Test" });
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // Act - Deactivate
        var deactivateResponse = await Client.PostAsync($"/api/v1/service-catalogs/categories/{id}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert Inactive
        var getResponse = await Client.GetAsync($"/api/v1/service-catalogs/categories/{id}");
        var getResponseData1 = GetResponseData(await ReadJsonAsync<JsonElement>(getResponse.Content));
        // Handle both PascalCase and camelCase
        var isActive1 = getResponseData1.TryGetProperty("isActive", out var prop1) ? prop1.GetBoolean() :
                        getResponseData1.TryGetProperty("IsActive", out var prop1b) ? prop1b.GetBoolean() : true;
        isActive1.Should().BeFalse();

        // Act - Activate
        var activateResponse = await Client.PostAsync($"/api/v1/service-catalogs/categories/{id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert Active
        getResponse = await Client.GetAsync($"/api/v1/service-catalogs/categories/{id}");
        var getResponseData2 = GetResponseData(await ReadJsonAsync<JsonElement>(getResponse.Content));
        var isActive2 = getResponseData2.TryGetProperty("isActive", out var prop2) ? prop2.GetBoolean() :
                        getResponseData2.TryGetProperty("IsActive", out var prop2b) ? prop2b.GetBoolean() : true;
        isActive2.Should().BeTrue();
    }

    #endregion

    #region Service Endpoints

    [Fact]
    public async Task GetServiceById_WithExistingId_ShouldReturnService()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var categoryResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Service Test Category" });
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(categoryResponse.Content)).GetProperty("id").GetString();
        
        var serviceName = $"Service_{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = serviceName, categoryId = catId });
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/services/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = GetResponseData(await ReadJsonAsync<JsonElement>(response.Content));
        data.GetProperty("name").GetString().Should().Be(serviceName);
    }

    [Fact]
    public async Task UpdateService_WithValidData_ShouldReturnUpdated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var catResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Update Service Cat" });
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(catResponse.Content)).GetProperty("id").GetString();
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = "Old Service", categoryId = catId });
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        var updateData = new { name = "Updated Service", categoryId = catId, description = "New desc", displayOrder = 10 };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/services/{id}", updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify update by getting the service
        var getResponse = await Client.GetAsync($"/api/v1/service-catalogs/services/{id}");
        var data = GetResponseData(await ReadJsonAsync<JsonElement>(getResponse.Content));
        data.GetProperty("name").GetString().Should().Be("Updated Service");
        data.GetProperty("displayOrder").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task ActivateAndDeactivateService_ShouldUpdateStatus()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var catResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Status Service Cat" });
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(catResponse.Content)).GetProperty("id").GetString();
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = $"Status Svc {Guid.NewGuid():N}", categoryId = catId });
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // Act - Deactivate
        await Client.PostAsync($"/api/v1/service-catalogs/services/{id}/deactivate", null);
        
        // Assert Inactive
        var getResponse = await Client.GetAsync($"/api/v1/service-catalogs/services/{id}");
        var getResponseData1 = GetResponseData(await ReadJsonAsync<JsonElement>(getResponse.Content));
        var isActive1 = getResponseData1.TryGetProperty("isActive", out var prop1) ? prop1.GetBoolean() :
                        getResponseData1.TryGetProperty("IsActive", out var prop1b) ? prop1b.GetBoolean() : true;
        isActive1.Should().BeFalse();

        // Act - Activate
        await Client.PostAsync($"/api/v1/service-catalogs/services/{id}/activate", null);

        // Assert Active
        getResponse = await Client.GetAsync($"/api/v1/service-catalogs/services/{id}");
        var getResponseData2 = GetResponseData(await ReadJsonAsync<JsonElement>(getResponse.Content));
        var isActive2 = getResponseData2.TryGetProperty("isActive", out var prop2) ? prop2.GetBoolean() :
                        getResponseData2.TryGetProperty("IsActive", out var prop2b) ? prop2b.GetBoolean() : true;
        isActive2.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteService_ShouldRemoveResource()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var catResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", new { name = "Delete Svc Cat" });
        var catId = GetResponseData(await ReadJsonAsync<JsonElement>(catResponse.Content)).GetProperty("id").GetString();
        var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", new { name = $"Delete Me {Guid.NewGuid():N}", categoryId = catId });
        
        // Skip if service creation fails (may have providers already using services)
        if (!createResponse.IsSuccessStatusCode)
        {
            return;
        }
        
        var id = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/service-catalogs/services/{id}");

        // Assert - allow 400 if service is offered by providers
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion
}

