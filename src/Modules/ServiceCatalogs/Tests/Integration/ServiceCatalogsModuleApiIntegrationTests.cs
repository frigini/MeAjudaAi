using MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration;

[Collection("ServiceCatalogsIntegrationTests")]
public class ServiceCatalogsModuleApiIntegrationTests : ServiceCatalogsIntegrationTestBase
{
    private IServiceCatalogsModuleApi _moduleApi = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _moduleApi = GetService<IServiceCatalogsModuleApi>();
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WithExistingCategory_ShouldReturnCategory()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Test Category", "Test Description", 1);

        // Act
        var result = await _moduleApi.GetServiceCategoryByIdAsync(category.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(category.Id.Value);
        result.Value.Name.Should().Be("Test Category");
        result.Value.Description.Should().Be("Test Description");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WithNonExistentCategory_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.GetServiceCategoryByIdAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetAllServiceCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        await CreateServiceCategoryAsync("Category 1");
        await CreateServiceCategoryAsync("Category 2");
        await CreateServiceCategoryAsync("Category 3");

        // Act
        var result = await _moduleApi.GetAllServiceCategoriesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllServiceCategoriesAsync_WithActiveOnlyFilter_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var activeCategory = await CreateServiceCategoryAsync("Active Category");
        var inactiveCategory = await CreateServiceCategoryAsync("Inactive Category");

        inactiveCategory.Deactivate();
        var repository = GetService<Domain.Repositories.IServiceCategoryRepository>();
        await repository.UpdateAsync(inactiveCategory);

        // Act
        var result = await _moduleApi.GetAllServiceCategoriesAsync(activeOnly: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(c => c.Id == activeCategory.Id.Value);
        result.Value.Should().NotContain(c => c.Id == inactiveCategory.Id.Value);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WithExistingService_ShouldReturnService()
    {
        // Arrange
        var (category, service) = await CreateCategoryWithServiceAsync("Category", "Test Service");

        // Act
        var result = await _moduleApi.GetServiceByIdAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(service.Id.Value);
        result.Value.Name.Should().Be("Test Service");
        result.Value.CategoryId.Should().Be(category.Id.Value);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WithNonExistentService_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.GetServiceByIdAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetAllServicesAsync_ShouldReturnAllServices()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        await CreateServiceAsync(category.Id, "Service 1");
        await CreateServiceAsync(category.Id, "Service 2");
        await CreateServiceAsync(category.Id, "Service 3");

        // Act
        var result = await _moduleApi.GetAllServicesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_ShouldReturnCategoryServices()
    {
        // Arrange
        var category1 = await CreateServiceCategoryAsync("Category 1");
        var category2 = await CreateServiceCategoryAsync("Category 2");

        var service1 = await CreateServiceAsync(category1.Id, "Service 1-1");
        var service2 = await CreateServiceAsync(category1.Id, "Service 1-2");
        await CreateServiceAsync(category2.Id, "Service 2-1");

        // Act
        var result = await _moduleApi.GetServicesByCategoryAsync(category1.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(s => s.Id == service1.Id.Value);
        result.Value.Should().Contain(s => s.Id == service2.Id.Value);
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithActiveService_ShouldReturnTrue()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Active Service");

        // Act
        var result = await _moduleApi.IsServiceActiveAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithInactiveService_ShouldReturnFalse()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Inactive Service");

        service.Deactivate();
        var repository = GetService<Domain.Repositories.IServiceRepository>();
        await repository.UpdateAsync(service);

        // Act
        var result = await _moduleApi.IsServiceActiveAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithAllValidServices_ShouldReturnAllValid()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service1 = await CreateServiceAsync(category.Id, "Service 1");
        var service2 = await CreateServiceAsync(category.Id, "Service 2");

        // Act
        var result = await _moduleApi.ValidateServicesAsync(new[] { service1.Id.Value, service2.Id.Value });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithSomeInvalidServices_ShouldReturnMixedResult()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var validService = await CreateServiceAsync(category.Id, "Valid Service");
        var invalidServiceId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.ValidateServicesAsync(new[] { validService.Id.Value, invalidServiceId });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().HaveCount(1);
        result.Value.InvalidServiceIds.Should().Contain(invalidServiceId);
    }
}
