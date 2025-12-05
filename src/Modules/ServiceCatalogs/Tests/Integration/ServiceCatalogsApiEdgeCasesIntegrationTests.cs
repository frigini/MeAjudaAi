using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration;

/// <summary>
/// Additional integration tests for ServiceCatalogs API - edge cases, error scenarios, and complex workflows
/// </summary>
[Collection("ServiceCatalogsIntegrationTests")]
[Trait("Category", "Integration")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Component", "API")]
public class ServiceCatalogsApiEdgeCasesIntegrationTests : ServiceCatalogsIntegrationTestBase
{
    private IServiceCatalogsModuleApi _moduleApi = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _moduleApi = GetService<IServiceCatalogsModuleApi>();
    }

    #region Category Edge Cases

    [Fact]
    public async Task GetAllCategoriesAsync_WithNoCategories_ShouldReturnEmptyList()
    {
        // Act
        var result = await _moduleApi.GetAllServiceCategoriesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WithMultipleConcurrentRequests_ShouldReturnConsistentResults()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Concurrent Category", "Description", 1);

        // Act - Execute 10 concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _moduleApi.GetServiceCategoryByIdAsync(category.Id.Value))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - All should return the same category
        results.Should().AllSatisfy(result =>
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(category.Id.Value);
            result.Value.Name.Should().Be(category.Name);
        });
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WithManyCategories_ShouldReturnAllInCorrectOrder()
    {
        // Arrange - Create categories with different display orders
        var category1 = await CreateServiceCategoryAsync("Category Z", "Last", displayOrder: 3);
        var category2 = await CreateServiceCategoryAsync("Category A", "First", displayOrder: 1);
        var category3 = await CreateServiceCategoryAsync("Category M", "Middle", displayOrder: 2);

        // Act
        var result = await _moduleApi.GetAllServiceCategoriesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        
        // Should be ordered by DisplayOrder
        var orderedCategories = result.Value.ToList();
        orderedCategories[0].Name.Should().Be("Category A"); // DisplayOrder 1
        orderedCategories[1].Name.Should().Be("Category M"); // DisplayOrder 2
        orderedCategories[2].Name.Should().Be("Category Z"); // DisplayOrder 3
    }

    #endregion

    #region Service Edge Cases

    [Fact]
    public async Task GetAllServicesAsync_WithNoServices_ShouldReturnEmptyList()
    {
        // Act
        var result = await _moduleApi.GetAllServicesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_WithNonExistentCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentCategoryId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.GetServicesByCategoryAsync(nonExistentCategoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_WithCategoryWithoutServices_ShouldReturnEmptyList()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Empty Category");

        // Act
        var result = await _moduleApi.GetServicesByCategoryAsync(category.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_WithMixedActiveAndInactiveServices_ShouldReturnAll()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var activeService = await CreateServiceAsync(category.Id, "Active Service");
        var inactiveService = await CreateServiceAsync(category.Id, "Inactive Service");

        inactiveService.Deactivate();
        var repository = GetService<Domain.Repositories.IServiceRepository>();
        await repository.UpdateAsync(inactiveService);

        // Act
        var result = await _moduleApi.GetServicesByCategoryAsync(category.Id.Value);

        // Assert - Should return both active and inactive
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithNonExistentService_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.IsServiceActiveAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region Validation Edge Cases

    [Fact]
    public async Task ValidateServicesAsync_WithEmptyList_ShouldReturnAllValid()
    {
        // Act
        var result = await _moduleApi.ValidateServicesAsync(Array.Empty<Guid>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithAllInvalidServices_ShouldReturnAllInvalid()
    {
        // Arrange
        var invalidId1 = UuidGenerator.NewId();
        var invalidId2 = UuidGenerator.NewId();
        var invalidId3 = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.ValidateServicesAsync(new[] { invalidId1, invalidId2, invalidId3 });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().HaveCount(3);
        result.Value.InvalidServiceIds.Should().Contain(invalidId1);
        result.Value.InvalidServiceIds.Should().Contain(invalidId2);
        result.Value.InvalidServiceIds.Should().Contain(invalidId3);
    }

    [Fact]
    public async Task ValidateServicesAsync_WithAllInactiveServices_ShouldReturnAllInactive()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service1 = await CreateServiceAsync(category.Id, "Service 1");
        var service2 = await CreateServiceAsync(category.Id, "Service 2");

        service1.Deactivate();
        service2.Deactivate();
        
        var repository = GetService<Domain.Repositories.IServiceRepository>();
        await repository.UpdateAsync(service1);
        await repository.UpdateAsync(service2);

        // Act
        var result = await _moduleApi.ValidateServicesAsync(
            new[] { service1.Id.Value, service2.Id.Value });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InactiveServiceIds.Should().HaveCount(2);
        result.Value.InactiveServiceIds.Should().Contain(service1.Id.Value);
        result.Value.InactiveServiceIds.Should().Contain(service2.Id.Value);
        result.Value.InvalidServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithMixedValidInactiveAndInvalid_ShouldCategorizeCorrectly()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var activeService = await CreateServiceAsync(category.Id, "Active");
        var inactiveService = await CreateServiceAsync(category.Id, "Inactive");
        var invalidServiceId = UuidGenerator.NewId();

        inactiveService.Deactivate();
        var repository = GetService<Domain.Repositories.IServiceRepository>();
        await repository.UpdateAsync(inactiveService);

        // Act
        var result = await _moduleApi.ValidateServicesAsync(new[]
        {
            activeService.Id.Value,
            inactiveService.Id.Value,
            invalidServiceId
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().HaveCount(1);
        result.Value.InvalidServiceIds.Should().Contain(invalidServiceId);
        result.Value.InactiveServiceIds.Should().HaveCount(1);
        result.Value.InactiveServiceIds.Should().Contain(inactiveService.Id.Value);
    }

    [Fact]
    public async Task ValidateServicesAsync_WithDuplicateIds_ShouldHandleCorrectly()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Service");

        // Act - Pass the same ID multiple times
        var result = await _moduleApi.ValidateServicesAsync(new[]
        {
            service.Id.Value,
            service.Id.Value,
            service.Id.Value
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateServicesAsync_ConcurrentValidations_ShouldReturnConsistentResults()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service1 = await CreateServiceAsync(category.Id, "Service 1");
        var service2 = await CreateServiceAsync(category.Id, "Service 2");

        var serviceIds = new[] { service1.Id.Value, service2.Id.Value };

        // Act - Execute 10 concurrent validations
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _moduleApi.ValidateServicesAsync(serviceIds))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - All should return consistent results
        results.Should().AllSatisfy(result =>
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.AllValid.Should().BeTrue();
            result.Value.InvalidServiceIds.Should().BeEmpty();
            result.Value.InactiveServiceIds.Should().BeEmpty();
        });
    }

    #endregion

    #region Complex Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateCategoryAndServices_ThenValidate()
    {
        // Arrange & Act - Create category
        var category = await CreateServiceCategoryAsync(
            "Electrical Services",
            "Professional electrical services",
            displayOrder: 1);

        // Act - Create multiple services
        var service1 = await CreateServiceAsync(category.Id, "Residential Wiring");
        var service2 = await CreateServiceAsync(category.Id, "Circuit Installation");
        var service3 = await CreateServiceAsync(category.Id, "Panel Upgrade");

        // Act - Deactivate one service
        service2.Deactivate();
        var repository = GetService<Domain.Repositories.IServiceRepository>();
        await repository.UpdateAsync(service2);

        // Act - Validate all services
        var validationResult = await _moduleApi.ValidateServicesAsync(new[]
        {
            service1.Id.Value,
            service2.Id.Value,
            service3.Id.Value
        });

        // Assert category
        var categoryResult = await _moduleApi.GetServiceCategoryByIdAsync(category.Id.Value);
        categoryResult.IsSuccess.Should().BeTrue();
        categoryResult.Value.Should().NotBeNull();

        // Assert services
        var servicesResult = await _moduleApi.GetServicesByCategoryAsync(category.Id.Value);
        servicesResult.IsSuccess.Should().BeTrue();
        servicesResult.Value.Should().HaveCount(3);

        // Assert validation
        validationResult.IsSuccess.Should().BeTrue();
        validationResult.Value.AllValid.Should().BeFalse();
        validationResult.Value.InactiveServiceIds.Should().HaveCount(1);
        validationResult.Value.InactiveServiceIds.Should().Contain(service2.Id.Value);
    }

    [Fact]
    public async Task CompleteWorkflow_MultipleCategories_WithServicesDistribution()
    {
        // Arrange - Create multiple categories
        var electricalCategory = await CreateServiceCategoryAsync("Electrical", displayOrder: 1);
        var plumbingCategory = await CreateServiceCategoryAsync("Plumbing", displayOrder: 2);
        var carpentryCategory = await CreateServiceCategoryAsync("Carpentry", displayOrder: 3);

        // Act - Create services in different categories
        var electricalService1 = await CreateServiceAsync(electricalCategory.Id, "Wiring");
        var electricalService2 = await CreateServiceAsync(electricalCategory.Id, "Lighting");
        
        var plumbingService1 = await CreateServiceAsync(plumbingCategory.Id, "Pipe Repair");
        var plumbingService2 = await CreateServiceAsync(plumbingCategory.Id, "Drain Cleaning");
        
        var carpentryService = await CreateServiceAsync(carpentryCategory.Id, "Furniture Assembly");

        // Assert - Verify all categories
        var allCategories = await _moduleApi.GetAllServiceCategoriesAsync();
        allCategories.Value.Should().HaveCount(3);

        // Assert - Verify all services
        var allServices = await _moduleApi.GetAllServicesAsync();
        allServices.Value.Should().HaveCount(5);

        // Assert - Verify services per category
        var electricalServices = await _moduleApi.GetServicesByCategoryAsync(electricalCategory.Id.Value);
        electricalServices.Value.Should().HaveCount(2);

        var plumbingServices = await _moduleApi.GetServicesByCategoryAsync(plumbingCategory.Id.Value);
        plumbingServices.Value.Should().HaveCount(2);

        var carpentryServices = await _moduleApi.GetServicesByCategoryAsync(carpentryCategory.Id.Value);
        carpentryServices.Value.Should().HaveCount(1);
    }

    #endregion
}
