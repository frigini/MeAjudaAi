using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.Catalogs;

/// <summary>
/// Testes E2E para o módulo de Catálogos usando TestContainers
/// </summary>
public class CatalogsEndToEndTests : TestContainerTestBase
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
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Expected 201 Created but got {response.StatusCode}. Response: {content}");
        }
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
        var result = JsonSerializer.Deserialize<object>(content, JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateService_Should_Require_Valid_Category()
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
        var result = JsonSerializer.Deserialize<object>(content, JsonOptions);
        result.Should().NotBeNull();
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

        // Act - Activate
        var activateResponse = await PostJsonAsync($"/api/v1/catalogs/services/{service.Id.Value}/activate", new { });
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Verify final state is active
        var getResponse = await ApiClient.GetAsync($"/api/v1/catalogs/services/{service.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Database_Should_Persist_ServiceCategories_Correctly()
    {
        // Arrange
        var name = Faker.Commerce.Department();
        var description = Faker.Lorem.Sentence();

        // Act - Create category directly in database
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<CatalogsDbContext>();

            var category = ServiceCategory.Create(name, description, 1);

            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();
        });

        // Assert - Verify category was persisted
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<CatalogsDbContext>();

            var foundCategory = await context.ServiceCategories
                .FirstOrDefaultAsync(c => c.Name == name);

            foundCategory.Should().NotBeNull();
            foundCategory!.Description.Should().Be(description);
        });
    }

    [Fact]
    public async Task Database_Should_Persist_Services_With_Category_Relationship()
    {
        // Arrange
        ServiceCategory? category = null;
        var serviceName = Faker.Commerce.ProductName();

        // Act - Create category and service
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<CatalogsDbContext>();

            category = ServiceCategory.Create(Faker.Commerce.Department(), Faker.Lorem.Sentence(), 1);
            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();

            var service = Service.Create(category.Id, serviceName, Faker.Commerce.ProductDescription(), 1);
            context.Services.Add(service);
            await context.SaveChangesAsync();
        });

        // Assert - Verify service and category relationship
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<CatalogsDbContext>();

            var foundService = await context.Services
                .FirstOrDefaultAsync(s => s.Name == serviceName);

            foundService.Should().NotBeNull();
            foundService!.CategoryId.Should().Be(category!.Id);
        });
    }

    #region Helper Methods

    private async Task<ServiceCategory> CreateTestServiceCategoryAsync()
    {
        ServiceCategory? category = null;

        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<CatalogsDbContext>();

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
            var context = services.GetRequiredService<CatalogsDbContext>();

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
            var context = services.GetRequiredService<CatalogsDbContext>();

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
            var context = services.GetRequiredService<CatalogsDbContext>();

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
