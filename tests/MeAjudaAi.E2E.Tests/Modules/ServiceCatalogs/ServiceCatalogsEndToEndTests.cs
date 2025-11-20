using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Testes E2E para o módulo ServiceCatalogs usando TestContainers
/// </summary>
public class ServiceCatalogsEndToEndTests : TestContainerTestBase
{
    [Fact]
    public async Task CreateServiceCategory_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();

        var createCategoryRequest = new
        {
            Name = Faker.Commerce.Department(),
            Description = Faker.Lorem.Sentence(),
            DisplayOrder = Faker.Random.Int(1, 100)
        };

        // Act
        var response = await PostJsonAsync("/api/v1/catalogs/categories", createCategoryRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/v1/catalogs/categories");
    }

    [Fact]
    public async Task GetServiceCategories_Should_Return_All_Categories()
    {
        // Arrange
        AuthenticateAsAdmin();
        await CreateTestServiceCategoriesAsync(3);

        // Act
        var response = await ApiClient.GetAsync("/api/v1/catalogs/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        result.TryGetProperty("data", out var data).Should().BeTrue();

        var categories = data.Deserialize<ServiceCategoryDto[]>(JsonOptions);
        categories.Should().NotBeNull();
        categories!.Length.Should().BeGreaterThanOrEqualTo(3, "should have at least the 3 created categories");
    }

    [Fact]
    public async Task CreateService_Should_Succeed_With_Valid_Category()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();

        var createServiceRequest = new
        {
            CategoryId = category.Id.Value,
            Name = Faker.Commerce.ProductName(),
            Description = Faker.Commerce.ProductDescription(),
            DisplayOrder = Faker.Random.Int(1, 100)
        };

        // Act
        var response = await PostJsonAsync("/api/v1/catalogs/services", createServiceRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/v1/catalogs/services");
    }

    [Fact]
    public async Task CreateService_Should_Reject_Invalid_CategoryId()
    {
        // Arrange
        AuthenticateAsAdmin();
        var nonExistentCategoryId = Guid.NewGuid();

        var createServiceRequest = new
        {
            CategoryId = nonExistentCategoryId,
            Name = Faker.Commerce.ProductName(),
            Description = Faker.Commerce.ProductDescription(),
            DisplayOrder = Faker.Random.Int(1, 100)
        };

        // Act
        var response = await PostJsonAsync("/api/v1/catalogs/services", createServiceRequest);

        // Assert - Should reject with BadRequest or NotFound
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetServicesByCategory_Should_Return_Filtered_Results()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();
        await CreateTestServicesAsync(category.Id.Value, 3);

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/catalogs/services/category/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        result.TryGetProperty("data", out var data).Should().BeTrue();

        var services = data.Deserialize<ServiceListDto[]>(JsonOptions);
        services.Should().NotBeNull();
        services!.Length.Should().Be(3, "should return exactly 3 services for this category");
        services.Should().OnlyContain(s => s.CategoryId == category.Id.Value, "all services should belong to the specified category");
    }

    [Fact]
    public async Task UpdateServiceCategory_Should_Modify_Existing_Category()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();

        var updateRequest = new
        {
            Name = "Updated " + Faker.Commerce.Department(),
            Description = "Updated " + Faker.Lorem.Sentence(),
            DisplayOrder = Faker.Random.Int(1, 100)
        };

        // Act
        var response = await PutJsonAsync($"/api/v1/catalogs/categories/{category.Id.Value}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the category was actually updated
        var getResponse = await ApiClient.GetAsync($"/api/v1/catalogs/categories/{category.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseWrapper = await ReadJsonAsync<Response<ServiceCategoryDto>>(getResponse);
        responseWrapper.Should().NotBeNull();
        responseWrapper!.Data.Should().NotBeNull();
        
        var updatedCategory = responseWrapper.Data;
        updatedCategory!.Name.Should().Be(updateRequest.Name);
        updatedCategory.Description.Should().Be(updateRequest.Description);
        updatedCategory.DisplayOrder.Should().Be(updateRequest.DisplayOrder);
    }

    [Fact]
    public async Task DeleteServiceCategory_Should_Fail_If_Has_Services()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();
        await CreateTestServicesAsync(category.Id.Value, 1);

        // Act
        var response = await ApiClient.DeleteAsync($"/api/v1/catalogs/categories/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Category should still exist after failed delete
        var getResponse = await ApiClient.GetAsync($"/api/v1/catalogs/categories/{category.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteServiceCategory_Should_Succeed_When_No_Services()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();

        // Act
        var response = await ApiClient.DeleteAsync($"/api/v1/catalogs/categories/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify category was deleted
        var getResponse = await ApiClient.GetAsync($"/api/v1/catalogs/categories/{category.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateDeactivate_Service_Should_Work_Correctly()
    {
        // Arrange
        AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();
        var service = await CreateTestServiceAsync(category.Id.Value);

        // Act - Deactivate
        var deactivateResponse = await PostJsonAsync($"/api/v1/catalogs/services/{service.Id.Value}/deactivate", new { });
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Verify service is inactive
        var getAfterDeactivate = await ApiClient.GetAsync($"/api/v1/catalogs/services/{service.Id.Value}");
        getAfterDeactivate.StatusCode.Should().Be(HttpStatusCode.OK);
        var deactivatedResponse = await ReadJsonAsync<Response<ServiceDto>>(getAfterDeactivate);
        deactivatedResponse.Should().NotBeNull();
        deactivatedResponse!.Data.Should().NotBeNull();
        var deactivatedService = deactivatedResponse.Data;
        deactivatedService!.IsActive.Should().BeFalse("service should be inactive after deactivate");

        // Act - Activate
        var activateResponse = await PostJsonAsync($"/api/v1/catalogs/services/{service.Id.Value}/activate", new { });
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Verify service is active again
        var getAfterActivate = await ApiClient.GetAsync($"/api/v1/catalogs/services/{service.Id.Value}");
        getAfterActivate.StatusCode.Should().Be(HttpStatusCode.OK);
        var activatedResponse = await ReadJsonAsync<Response<ServiceDto>>(getAfterActivate);
        activatedResponse.Should().NotBeNull();
        activatedResponse!.Data.Should().NotBeNull();
        var activatedService = activatedResponse.Data;
        activatedService!.IsActive.Should().BeTrue("service should be active after activate");
    }

    [Fact]
    public async Task Database_Should_Persist_ServiceCategories_Correctly()
    {
        // Arrange
        var name = Faker.Commerce.Department();
        var description = Faker.Lorem.Sentence();
        ServiceCategoryId? categoryId = null;

        // Act - Create category directly in database
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            var category = ServiceCategory.Create(name, description, 1);
            categoryId = category.Id;

            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();
        });

        // Assert - Verify category was persisted by ID for determinism
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            var foundCategory = await context.ServiceCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            foundCategory.Should().NotBeNull();
            foundCategory!.Name.Should().Be(name);
            foundCategory.Description.Should().Be(description);
        });
    }

    [Fact]
    public async Task Database_Should_Persist_Services_With_Category_Relationship()
    {
        // Arrange
        ServiceCategory? category = null;
        var serviceName = Faker.Commerce.ProductName();
        ServiceId? serviceId = null;

        // Act - Create category and service
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            category = ServiceCategory.Create(Faker.Commerce.Department(), Faker.Lorem.Sentence(), 1);
            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();

            var service = Service.Create(category.Id, serviceName, Faker.Commerce.ProductDescription(), 1);
            serviceId = service.Id;
            context.Services.Add(service);
            await context.SaveChangesAsync();
        });

        // Assert - Verify service and category relationship by ID for determinism
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            var foundService = await context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            foundService.Should().NotBeNull();
            foundService!.Name.Should().Be(serviceName);
            foundService.CategoryId.Should().Be(category!.Id);
        });
    }

    #region Helper Methods

    private async Task<ServiceCategory> CreateTestServiceCategoryAsync()
    {
        ServiceCategory? category = null;

        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            category = ServiceCategory.Create(
                Faker.Commerce.Department(),
                Faker.Lorem.Sentence(),
                Faker.Random.Int(1, 100)
            );

            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();
        });

        return category!;
    }

    private async Task CreateTestServiceCategoriesAsync(int count)
    {
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            for (int i = 0; i < count; i++)
            {
                var category = ServiceCategory.Create(
                    Faker.Commerce.Department() + $" {i}",
                    Faker.Lorem.Sentence(),
                    i + 1
                );

                context.ServiceCategories.Add(category);
            }

            await context.SaveChangesAsync();
        });
    }

    private async Task<Service> CreateTestServiceAsync(Guid categoryId)
    {
        Service? service = null;

        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            service = Service.Create(
                new ServiceCategoryId(categoryId),
                Faker.Commerce.ProductName(),
                Faker.Commerce.ProductDescription(),
                Faker.Random.Int(1, 100)
            );

            context.Services.Add(service);
            await context.SaveChangesAsync();
        });

        return service!;
    }

    private async Task CreateTestServicesAsync(Guid categoryId, int count)
    {
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            for (int i = 0; i < count; i++)
            {
                var service = Service.Create(
                    new ServiceCategoryId(categoryId),
                    Faker.Commerce.ProductName() + $" {i}",
                    Faker.Commerce.ProductDescription(),
                    i + 1
                );

                context.Services.Add(service);
            }

            await context.SaveChangesAsync();
        });
    }

    #endregion
}
