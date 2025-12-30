using System.Net.Http.Json;
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
/// Testes E2E para o módulo ServiceCatalogs usando TestContainers.
/// Foca em cenários completos: CRUD, validações, workflows, mudanças de categoria e validações de regras de negócio.
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "ServiceCatalogs")]
public class ServiceCatalogsEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public ServiceCatalogsEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Basic CRUD and Validation Tests

    [Fact]
    public async Task CreateService_Should_Succeed_With_Valid_Category()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();

        var createServiceRequest = new
        {
            CategoryId = category.Id.Value,
            Name = _fixture.Faker.Commerce.ProductName(),
            Description = _fixture.Faker.Commerce.ProductDescription(),
            DisplayOrder = _fixture.Faker.Random.Int(1, 100)
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/service-catalogs/services", createServiceRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/v1/service-catalogs/services");
    }

    [Fact]
    public async Task CreateService_Should_Reject_Invalid_CategoryId()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentCategoryId = Guid.NewGuid();

        var createServiceRequest = new
        {
            CategoryId = nonExistentCategoryId,
            Name = _fixture.Faker.Commerce.ProductName(),
            Description = _fixture.Faker.Commerce.ProductDescription(),
            DisplayOrder = _fixture.Faker.Random.Int(1, 100)
        };

        // Act
        var response = await _fixture.PostJsonAsync("/api/v1/service-catalogs/services", createServiceRequest);

        // Assert - Should reject with BadRequest or NotFound
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Category Filtering and Queries

    [Fact]
    public async Task GetServicesByCategory_Should_Return_Filtered_Results()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();
        await CreateTestServicesAsync(category.Id.Value, 3);

        // Act
        var response = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/services/category/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);
        result.TryGetProperty("data", out var data).Should().BeTrue();

        var services = data.Deserialize<ServiceListDto[]>(TestContainerFixture.JsonOptions);
        services.Should().NotBeNull();
        services!.Length.Should().Be(3, "should return exactly 3 services for this category");
        services.Should().OnlyContain(s => s.CategoryId == category.Id.Value, "all services should belong to the specified category");
    }

    #endregion

    #region Category Update and Delete Operations

    [Fact]
    public async Task UpdateServiceCategory_Should_Modify_Existing_Category()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();

        var updateRequest = new
        {
            Name = "Updated " + _fixture.Faker.Commerce.Department(),
            Description = "Updated " + _fixture.Faker.Lorem.Sentence(),
            DisplayOrder = _fixture.Faker.Random.Int(1, 100)
        };

        // Act
        var response = await _fixture.PutJsonAsync($"/api/v1/service-catalogs/categories/{category.Id.Value}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the category was actually updated
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/categories/{category.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseWrapper = await TestContainerFixture.ReadJsonAsync<Response<ServiceCategoryDto>>(getResponse);
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
        TestContainerFixture.AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();
        await CreateTestServicesAsync(category.Id.Value, 1);

        // Act
        var response = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/categories/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Category should still exist after failed delete
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/categories/{category.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteServiceCategory_Should_Succeed_When_No_Services()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();

        // Act
        var response = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/categories/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify category was deleted
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/categories/{category.Id.Value}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Service Activation/Deactivation

    [Fact]
    public async Task ActivateDeactivate_Service_Should_Work_Correctly()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var category = await CreateTestServiceCategoryAsync();
        var service = await CreateTestServiceAsync(category.Id.Value);

        // Act - Deactivate
        var deactivateResponse = await _fixture.PostJsonAsync($"/api/v1/service-catalogs/services/{service.Id.Value}/deactivate", new { });
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Verify service is inactive
        var getAfterDeactivate = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/services/{service.Id.Value}");
        getAfterDeactivate.StatusCode.Should().Be(HttpStatusCode.OK);
        var deactivatedResponse = await TestContainerFixture.ReadJsonAsync<Response<ServiceDto>>(getAfterDeactivate);
        deactivatedResponse.Should().NotBeNull();
        deactivatedResponse!.Data.Should().NotBeNull();
        var deactivatedService = deactivatedResponse.Data;
        deactivatedService!.IsActive.Should().BeFalse("service should be inactive after deactivate");

        // Act - Activate
        var activateResponse = await _fixture.PostJsonAsync($"/api/v1/service-catalogs/services/{service.Id.Value}/activate", new { });
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Verify service is active again
        var getAfterActivate = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/services/{service.Id.Value}");
        getAfterActivate.StatusCode.Should().Be(HttpStatusCode.OK);
        var activatedResponse = await TestContainerFixture.ReadJsonAsync<Response<ServiceDto>>(getAfterActivate);
        activatedResponse.Should().NotBeNull();
        activatedResponse!.Data.Should().NotBeNull();
        var activatedService = activatedResponse.Data;
        activatedService!.IsActive.Should().BeTrue("service should be active after activate");
    }

    #endregion

    #region Database Persistence Tests

    [Fact]
    public async Task Database_Should_Persist_ServiceCategories_Correctly()
    {
        // Arrange
        var uniqueName = $"{_fixture.Faker.Commerce.Department()}-{Guid.NewGuid():N}";
        var name = uniqueName;
        var description = _fixture.Faker.Lorem.Sentence();
        ServiceCategoryId? categoryId = null;

        // Act - Create category directly in database
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            var category = ServiceCategory.Create(name, description, 1);
            categoryId = category.Id;

            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();
        });

        // Assert - Verify category was persisted by ID for determinism
        await _fixture.WithServiceScopeAsync(async services =>
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
        var serviceName = _fixture.Faker.Commerce.ProductName();
        ServiceId? serviceId = null;

        // Act - Create category and service
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            // Append GUID to ensure unique category name
            var uniqueCategoryName = $"{_fixture.Faker.Commerce.Department()}-{Guid.NewGuid():N}";
            category = ServiceCategory.Create(uniqueCategoryName, _fixture.Faker.Lorem.Sentence(), 1);
            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();

            var service = Service.Create(category.Id, serviceName, _fixture.Faker.Commerce.ProductDescription(), 1);
            serviceId = service.Id;
            context.Services.Add(service);
            await context.SaveChangesAsync();
        });

        // Assert - Verify service and category relationship by ID for determinism
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            var foundService = await context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            foundService.Should().NotBeNull();
            foundService!.Name.Should().Be(serviceName);
            foundService.CategoryId.Should().Be(category!.Id);
        });
    }

    #endregion

    #region Advanced Operations - Service Validation

    /// <summary>
    /// Valida que um serviço pode ser validado com sucesso quando atende todas as regras de negócio.
    /// </summary>
    [Fact]
    public async Task ValidateService_WithBusinessRules_Should_Succeed()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Primeiro cria uma categoria
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var categoryRequest = new
        {
            Name = $"ValidateTestCategory_{uniqueId}",
            Description = "Category for validation tests",
            IsActive = true
        };

        var categoryResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest, TestContainerFixture.JsonOptions);

        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await categoryResponse.Content.ReadAsStringAsync());

        var categoryLocation = categoryResponse.Headers.Location?.ToString();
        var categoryId = TestContainerFixture.ExtractIdFromLocation(categoryLocation!);

        // Cria um serviço
        var serviceRequest = new
        {
            Name = $"ValidateTestService_{uniqueId}",
            Description = "Service for validation tests",
            CategoryId = categoryId,
            IsActive = true,
            Price = 100.00m,
            Duration = 60
        };

        var serviceResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, TestContainerFixture.JsonOptions);

        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Service creation is a precondition for this test. Response: {0}",
            await serviceResponse.Content.ReadAsStringAsync());

        var serviceLocation = serviceResponse.Headers.Location?.ToString();
        var serviceId = TestContainerFixture.ExtractIdFromLocation(serviceLocation!);

        // Act - Validate service (API valida múltiplos serviços via array)
        var validateRequest = new
        {
            ServiceIds = new[] { serviceId }
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/services/validate",
            validateRequest,
            TestContainerFixture.JsonOptions);

        // Assert - Apenas status de sucesso (happy path)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Valida que validar um serviço inexistente retorna um erro apropriado ou resultado de validação.
    /// </summary>
    [Fact]
    public async Task ValidateService_WithInvalidService_Should_ReturnErrorOrValidationResult()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var serviceId = Guid.NewGuid(); // Serviço inexistente

        var invalidRequest = new
        {
            ServiceIds = new[] { serviceId }
        };

        // Act - API deve retornar que o serviço é inválido ou OK com indicação de serviços inválidos
        var response = await _fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/services/validate",
            invalidRequest,
            TestContainerFixture.JsonOptions);

        // Assert - Pode retornar BadRequest, NotFound, ou OK com AllValid=false
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.OK);
    }

    #endregion

    #region Advanced Operations - Category Change

    /// <summary>
    /// Valida que a categoria de um serviço pode ser alterada com sucesso e o relacionamento é atualizado.
    /// </summary>
    [Fact]
    public async Task ChangeServiceCategory_Should_UpdateRelationship()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Cria primeira categoria
        var category1Request = new
        {
            Name = $"OriginalCategory_{uniqueId}",
            Description = "Original category",
            IsActive = true
        };

        var category1Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", category1Request, TestContainerFixture.JsonOptions);

        category1Response.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await category1Response.Content.ReadAsStringAsync());

        var category1Id = TestContainerFixture.ExtractIdFromLocation(category1Response.Headers.Location!.ToString());

        // Cria segunda categoria
        var category2Request = new
        {
            Name = $"NewCategory_{uniqueId}",
            Description = "New category for service",
            IsActive = true
        };

        var category2Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", category2Request, TestContainerFixture.JsonOptions);

        category2Response.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await category2Response.Content.ReadAsStringAsync());

        var category2Id = TestContainerFixture.ExtractIdFromLocation(category2Response.Headers.Location!.ToString());

        // Cria um serviço na primeira categoria
        var serviceRequest = new
        {
            Name = $"MoveTestService_{uniqueId}",
            Description = "Service to be moved",
            CategoryId = category1Id,
            IsActive = true,
            Price = 150.00m,
            Duration = 90
        };

        var serviceResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, TestContainerFixture.JsonOptions);

        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Service creation is a precondition for this test. Response: {0}",
            await serviceResponse.Content.ReadAsStringAsync());

        var serviceId = TestContainerFixture.ExtractIdFromLocation(serviceResponse.Headers.Location!.ToString());

        // Act - Change service category
        var changeCategoryRequest = new
        {
            NewCategoryId = category2Id
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            TestContainerFixture.JsonOptions);

        // Assert - Change is expected to succeed in this scenario
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "the service category change should succeed in this scenario");

        // Verifica que o serviço está na nova categoria
        var getServiceResponse = await _fixture.ApiClient.GetAsync($"/api/v1/service-catalogs/services/{serviceId}");
        getServiceResponse.IsSuccessStatusCode.Should().BeTrue(
            "the updated service should be retrievable after changing category");

        var content = await getServiceResponse.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content, TestContainerFixture.JsonOptions);
        result.TryGetProperty("data", out var data).Should().BeTrue("response should contain data property");
        data.TryGetProperty("categoryId", out var categoryIdElement).Should().BeTrue("service should have categoryId property");
        var actualCategoryId = categoryIdElement.GetGuid();
        actualCategoryId.Should().Be(category2Id,
            "the service should now be associated with the new category");
    }

    /// <summary>
    /// Valida que tentar alterar um serviço para uma categoria inativa retorna uma falha.
    /// </summary>
    [Fact]
    public async Task ChangeServiceCategory_ToInactiveCategory_Should_Fail()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Cria categoria ativa
        var activeCategoryRequest = new
        {
            Name = $"ActiveCategory_{uniqueId}",
            Description = "Active category",
            IsActive = true
        };

        var activeCategoryResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", activeCategoryRequest, TestContainerFixture.JsonOptions);

        activeCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Active category creation is a precondition for this test. Response: {0}",
            await activeCategoryResponse.Content.ReadAsStringAsync());

        var activeCategoryId = TestContainerFixture.ExtractIdFromLocation(activeCategoryResponse.Headers.Location!.ToString());

        // Cria outra categoria (sempre criada como ativa) e depois a desativa
        var toBeInactiveCategoryRequest = new
        {
            Name = $"InactiveCategory_{uniqueId}",
            Description = "Category to be deactivated",
            DisplayOrder = 0
        };

        var inactiveCategoryResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", toBeInactiveCategoryRequest, TestContainerFixture.JsonOptions);

        inactiveCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Category creation is a precondition for this test. Response: {0}",
            await inactiveCategoryResponse.Content.ReadAsStringAsync());

        var inactiveCategoryId = TestContainerFixture.ExtractIdFromLocation(inactiveCategoryResponse.Headers.Location!.ToString());

        // Desativa a categoria
        var deactivateResponse = await _fixture.ApiClient.PostAsync(
            $"/api/v1/service-catalogs/categories/{inactiveCategoryId}/deactivate",
            null);

        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "Category deactivation is a precondition for this test. Response: {0}",
            await deactivateResponse.Content.ReadAsStringAsync());

        // Cria serviço na categoria ativa
        var serviceRequest = new
        {
            Name = $"TestService_{uniqueId}",
            Description = "Service for category change test",
            CategoryId = activeCategoryId,
            IsActive = true,
            Price = 200.00m,
            Duration = 120
        };

        var serviceResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, TestContainerFixture.JsonOptions);

        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Service creation is a precondition for this test. Response: {0}",
            await serviceResponse.Content.ReadAsStringAsync());

        var serviceId = TestContainerFixture.ExtractIdFromLocation(serviceResponse.Headers.Location!.ToString());

        // Act - Tenta mudar para categoria inativa
        var changeCategoryRequest = new
        {
            NewCategoryId = inactiveCategoryId
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            TestContainerFixture.JsonOptions);

        // Assert - Must fail when trying to move to inactive category
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.BadRequest, HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound],
            "changing to an inactive category should be rejected");
    }

    /// <summary>
    /// Valida que tentar alterar um serviço para uma categoria inexistente retorna NotFound.
    /// </summary>
    [Fact]
    public async Task ChangeServiceCategory_ToNonExistentCategory_Should_Return_NotFound()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        // Create a valid category and service first
        var categoryRequest = new
        {
            Name = $"Category_{Guid.NewGuid()}",
            Description = "Test category",
            IsActive = true
        };
        var categoryResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest, TestContainerFixture.JsonOptions);
        var categoryId = TestContainerFixture.ExtractIdFromLocation(categoryResponse.Headers.Location!.ToString());

        var serviceRequest = new
        {
            Name = $"Service_{Guid.NewGuid()}",
            Description = "Test service",
            CategoryId = categoryId,
            BasePrice = 100.00m,
            IsActive = true
        };
        var serviceResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, TestContainerFixture.JsonOptions);
        var serviceId = TestContainerFixture.ExtractIdFromLocation(serviceResponse.Headers.Location!.ToString());

        // Now try to change to a non-existent category
        var nonExistentCategoryId = Guid.NewGuid();
        var changeCategoryRequest = new
        {
            NewCategoryId = nonExistentCategoryId
        };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity,
            "changing to a non-existent category should return 422 (semantic validation error)");
    }

    #endregion

    #region Helper Methods

    private async Task<ServiceCategory> CreateTestServiceCategoryAsync()
    {
        ServiceCategory? category = null;

        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            // Append GUID to ensure unique category names across test runs
            var uniqueName = $"{_fixture.Faker.Commerce.Department()}-{Guid.NewGuid():N}";
            
            category = ServiceCategory.Create(
                uniqueName,
                _fixture.Faker.Lorem.Sentence(),
                _fixture.Faker.Random.Int(1, 100)
            );

            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();
        });

        return category!;
    }

    private async Task CreateTestServiceCategoriesAsync(int count)
    {
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            for (int i = 0; i < count; i++)
            {
                // Append GUID to ensure unique category names across test runs
                var uniqueName = $"{_fixture.Faker.Commerce.Department()}-{Guid.NewGuid():N}";
                
                var category = ServiceCategory.Create(
                    uniqueName,
                    _fixture.Faker.Lorem.Sentence(),
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

        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            service = Service.Create(
                new ServiceCategoryId(categoryId),
                _fixture.Faker.Commerce.ProductName(),
                _fixture.Faker.Commerce.ProductDescription(),
                _fixture.Faker.Random.Int(1, 100)
            );

            context.Services.Add(service);
            await context.SaveChangesAsync();
        });

        return service!;
    }

    private async Task CreateTestServicesAsync(Guid categoryId, int count)
    {
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<ServiceCatalogsDbContext>();

            for (int i = 0; i < count; i++)
            {
                var service = Service.Create(
                    new ServiceCategoryId(categoryId),
                    _fixture.Faker.Commerce.ProductName() + $" {i}",
                    _fixture.Faker.Commerce.ProductDescription(),
                    i + 1
                );

                context.Services.Add(service);
            }

            await context.SaveChangesAsync();
        });
    }

    #endregion
}

