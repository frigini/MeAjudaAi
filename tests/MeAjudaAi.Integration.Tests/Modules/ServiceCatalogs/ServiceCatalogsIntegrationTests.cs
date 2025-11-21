using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Testes de integração completos para o módulo ServiceCatalogs.
/// Valida fluxos end-to-end de criação, atualização, consulta e remoção.
/// </summary>
public class ServiceCatalogsIntegrationTests(ITestOutputHelper testOutput) : ApiTestBase
{
    [Fact]
    public async Task CreateServiceCategory_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var categoryData = new
        {
            name = $"Test Category {Guid.NewGuid():N}",
            description = "Test Category",
            displayOrder = 1
        };

        string? categoryId = null;

        try
        {
            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryData);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                "POST requests that create resources should return 201 Created");

            var responseJson = JsonSerializer.Deserialize<JsonElement>(content);
            var dataElement = GetResponseData(responseJson);
            dataElement.TryGetProperty("id", out _).Should().BeTrue(
                $"Response data should contain 'id' property. Full response: {content}");
            dataElement.TryGetProperty("name", out var nameProperty).Should().BeTrue();
            nameProperty.GetString().Should().Be(categoryData.name);

            if (dataElement.TryGetProperty("id", out var idProperty))
            {
                categoryId = idProperty.GetString();
            }
        }
        finally
        {
            // Cleanup
            if (categoryId is not null)
            {
                var deleteResponse = await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");
                if (!deleteResponse.IsSuccessStatusCode)
                {
                    testOutput.WriteLine($"Cleanup failed: Could not delete category {categoryId}. Status: {deleteResponse.StatusCode}");
                }
            }
        }
    }

    [Fact]
    public async Task GetServiceCategories_ShouldReturnCategoriesList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/service-catalogs/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        var categories = JsonSerializer.Deserialize<JsonElement>(content);
        categories.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");
        categories.TryGetProperty("data", out var dataElement).Should().BeTrue(
            "Response should contain 'data' property for consistency");
        dataElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    public async Task GetServiceCategoryById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var randomId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/categories/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "API should return 404 when service category ID does not exist");
    }

    [Fact]
    public async Task GetServiceById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var randomId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/service-catalogs/services/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "API should return 404 when service ID does not exist");
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
                testOutput.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}. Body: {body}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Authenticated admin requests to {endpoint} should succeed.");
        }
    }

    [Fact]
    public async Task ServiceCategoryWorkflow_CreateUpdateDelete_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var categoryData = new
        {
            name = $"Category {uniqueId}",
            description = "Test Description",
            displayOrder = 1
        };

        string? categoryId = null;

        try
        {
            // Act 1: Create Category
            var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryData);

            // Assert 1: Creation successful
            var createContent = await createResponse.Content.ReadAsStringAsync();
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
                $"Category creation should succeed. Response: {createContent}");

            var createResponseJson = JsonSerializer.Deserialize<JsonElement>(createContent);
            var createdCategory = GetResponseData(createResponseJson);
            createdCategory.TryGetProperty("id", out var idProperty).Should().BeTrue();
            categoryId = idProperty.GetString()!;

            // Act 2: Update Category
            var updateData = new
            {
                name = $"Updated Category {uniqueId}",
                description = "Updated Description",
                displayOrder = 2
            };

            var updateResponse = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/categories/{categoryId}", updateData);

            // Assert 2: Update successful
            updateResponse.StatusCode.Should().BeOneOf(
                [HttpStatusCode.OK, HttpStatusCode.NoContent],
                "Update should succeed for existing categories");

            // Act 3: Get Category by ID
            var getResponse = await Client.GetAsync($"/api/v1/service-catalogs/categories/{categoryId}");

            // Assert 3: Can retrieve created category with updated fields
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getResponseJson = JsonSerializer.Deserialize<JsonElement>(getContent);
            var retrievedCategory = GetResponseData(getResponseJson);
            retrievedCategory.TryGetProperty("id", out var retrievedIdProperty).Should().BeTrue();
            retrievedIdProperty.GetString().Should().Be(categoryId);
            retrievedCategory.TryGetProperty("name", out var retrievedNameProperty).Should().BeTrue();
            retrievedNameProperty.GetString().Should().Be(updateData.name, "Updated name should be reflected");
            retrievedCategory.TryGetProperty("description", out var retrievedDescProperty).Should().BeTrue();
            retrievedDescProperty.GetString().Should().Be(updateData.description, "Updated description should be reflected");
            retrievedCategory.TryGetProperty("displayOrder", out var retrievedOrderProperty).Should().BeTrue();
            retrievedOrderProperty.GetInt32().Should().Be(updateData.displayOrder, "Updated displayOrder should be reflected");
        }
        catch (Exception ex)
        {
            testOutput.WriteLine($"Category workflow test failed: {ex.Message}");
            throw;
        }
        finally
        {
            // Act 4: Delete Category (in finally to ensure cleanup)
            if (categoryId is not null)
            {
                var deleteResponse = await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");

                // Assert 4: Deletion successful
                deleteResponse.StatusCode.Should().BeOneOf(
                    [HttpStatusCode.OK, HttpStatusCode.NoContent],
                    "Delete should succeed for existing categories");
            }
        }
    }

    [Fact]
    public async Task ServiceWorkflow_CreateWithCategoryUpdateDelete_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // First create a category
        var categoryData = new
        {
            name = $"Test Category {uniqueId}",
            description = "Test Description",
            displayOrder = 1
        };

        var categoryResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryData);
        var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var categoryJson = JsonSerializer.Deserialize<JsonElement>(categoryContent);
        var categoryDataElement = GetResponseData(categoryJson);
        categoryDataElement.TryGetProperty("id", out var categoryIdProperty).Should().BeTrue();
        var categoryId = categoryIdProperty.GetString()!;

        try
        {
            // Act 1: Create Service
            var serviceData = new
            {
                name = $"Test Service {uniqueId}",
                description = "Test Service Description",
                categoryId = categoryId
            };

            var createResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceData);

            // Assert 1: Creation successful
            var createContent = await createResponse.Content.ReadAsStringAsync();
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
                $"Service creation should succeed. Response: {createContent}");

            var createResponseJson = JsonSerializer.Deserialize<JsonElement>(createContent);
            var createdService = GetResponseData(createResponseJson);
            createdService.TryGetProperty("id", out var serviceIdProperty).Should().BeTrue();
            var serviceId = serviceIdProperty.GetString()!;

            // Act 2: Update Service
            var updateData = new
            {
                name = $"Updated Service {uniqueId}",
                description = "Updated Service Description",
                categoryId = categoryId,
                isActive = false
            };

            var updateResponse = await Client.PutAsJsonAsync($"/api/v1/service-catalogs/services/{serviceId}", updateData);

            // Assert 2: Update successful
            updateResponse.StatusCode.Should().BeOneOf(
                [HttpStatusCode.OK, HttpStatusCode.NoContent],
                "Update should succeed for existing services");

            // Act 3: Get Service by ID
            var getResponse = await Client.GetAsync($"/api/v1/service-catalogs/services/{serviceId}");

            // Assert 3: Can retrieve created service with updated fields
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getResponseJson = JsonSerializer.Deserialize<JsonElement>(getContent);
            var retrievedService = GetResponseData(getResponseJson);
            retrievedService.TryGetProperty("id", out var retrievedServiceIdProperty).Should().BeTrue();
            retrievedServiceIdProperty.GetString().Should().Be(serviceId);
            retrievedService.TryGetProperty("name", out var retrievedNameProperty).Should().BeTrue();
            retrievedNameProperty.GetString().Should().Be(updateData.name, "Updated name should be reflected");
            retrievedService.TryGetProperty("description", out var retrievedDescProperty).Should().BeTrue();
            retrievedDescProperty.GetString().Should().Be(updateData.description, "Updated description should be reflected");

            // Act 4: Delete Service
            var deleteResponse = await Client.DeleteAsync($"/api/v1/service-catalogs/services/{serviceId}");

            // Assert 4: Deletion successful
            deleteResponse.StatusCode.Should().BeOneOf(
                [HttpStatusCode.OK, HttpStatusCode.NoContent],
                "Delete should succeed for existing services");
        }
        catch (Exception ex)
        {
            testOutput.WriteLine($"Service workflow test failed: {ex.Message}");
            throw;
        }
        finally
        {
            // Cleanup category
            var deleteCategoryResponse = await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");
            if (!deleteCategoryResponse.IsSuccessStatusCode)
            {
                testOutput.WriteLine($"Cleanup failed: Could not delete category {categoryId}. Status: {deleteCategoryResponse.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task GetServicesByCategoryId_ShouldReturnFilteredServices()
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
        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Category creation should succeed");
        var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
        var categoryJson = JsonSerializer.Deserialize<JsonElement>(categoryContent);
        var categoryDataElement = GetResponseData(categoryJson);
        categoryDataElement.TryGetProperty("id", out var categoryIdProperty).Should().BeTrue();
        var categoryId = categoryIdProperty.GetString()!;

        try
        {
            // Create a service in the category
            var serviceData = new
            {
                name = $"Test Service {Guid.NewGuid():N}",
                description = "Test Service",
                categoryId = categoryId
            };

            var serviceResponse = await Client.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceData);
            serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Service creation should succeed");

            var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
            var serviceJson = JsonSerializer.Deserialize<JsonElement>(serviceContent);
            var serviceDataElement = GetResponseData(serviceJson);
            serviceDataElement.TryGetProperty("id", out var serviceIdProperty).Should().BeTrue();
            var serviceId = serviceIdProperty.GetString()!;

            try
            {
                // Act: Get services by category
                var response = await Client.GetAsync($"/api/v1/service-catalogs/services/category/{categoryId}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var content = await response.Content.ReadAsStringAsync();
                var services = JsonSerializer.Deserialize<JsonElement>(content);

                services.ValueKind.Should().Be(JsonValueKind.Object);
                services.TryGetProperty("data", out var dataElement).Should().BeTrue();

                // Should contain at least the service we just created
                if (dataElement.ValueKind == JsonValueKind.Array)
                {
                    dataElement.GetArrayLength().Should().BeGreaterThanOrEqualTo(1,
                        "Response should contain at least the created service");

                    // Verify the created service is in the results
                    var foundService = false;
                    foreach (var item in dataElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("id", out var itemId) && itemId.GetString() == serviceId)
                        {
                            foundService = true;
                            item.TryGetProperty("categoryId", out var itemCategoryId).Should().BeTrue();
                            itemCategoryId.GetString().Should().Be(categoryId,
                                "Service should belong to the correct category");
                            break;
                        }
                    }
                    foundService.Should().BeTrue($"Created service {serviceId} should be in the filtered results");
                }
            }
            finally
            {
                // Cleanup service
                await Client.DeleteAsync($"/api/v1/service-catalogs/services/{serviceId}");
            }
        }
        finally
        {
            // Cleanup category
            await Client.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");
        }
    }

    private static JsonElement GetResponseData(JsonElement response)
    {
        return response.TryGetProperty("data", out var dataElement)
            ? dataElement
            : response;
    }
}
