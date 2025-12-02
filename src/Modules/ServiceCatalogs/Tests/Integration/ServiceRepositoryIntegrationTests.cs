using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;
using MeAjudaAi.Shared.Time;
using Domain = MeAjudaAi.Modules.ServiceCatalogs.Domain;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration;

[Collection("ServiceCatalogsIntegrationTests")]
public class ServiceRepositoryIntegrationTests : ServiceCatalogsIntegrationTestBase
{
    private IServiceRepository _repository = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _repository = GetService<IServiceRepository>();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingService_ShouldReturnService()
    {
        // Arrange
        var (category, service) = await CreateCategoryWithServiceAsync("Test Category", "Test Service");

        // Act
        var result = await _repository.GetByIdAsync(service.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(service.Id);
        result.Name.Should().Be(service.Name);
        result.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentService_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _repository.GetByIdAsync(new Domain.ValueObjects.ServiceId(nonExistentId));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleServices_ShouldReturnAllServices()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service1 = await CreateServiceAsync(category.Id, "Service 1", displayOrder: 1);
        var service2 = await CreateServiceAsync(category.Id, "Service 2", displayOrder: 2);
        var service3 = await CreateServiceAsync(category.Id, "Service 3", displayOrder: 3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Should().Contain(s => s.Name == service1.Name);
        result.Should().Contain(s => s.Name == service2.Name);
        result.Should().Contain(s => s.Name == service3.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnlyFilter_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var activeService = await CreateServiceAsync(category.Id, "Active Service");
        var inactiveService = await CreateServiceAsync(category.Id, "Inactive Service");

        inactiveService.Deactivate();
        await _repository.UpdateAsync(inactiveService);

        // Act
        var result = await _repository.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().Contain(s => s.Id == activeService.Id);
        result.Should().OnlyContain(s => s.IsActive, "all returned services should be active");
        result.Should().NotContain(s => s.Id == inactiveService.Id);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithExistingCategory_ShouldReturnCategoryServices()
    {
        // Arrange
        var category1 = await CreateServiceCategoryAsync("Category 1");
        var category2 = await CreateServiceCategoryAsync("Category 2");

        var service1 = await CreateServiceAsync(category1.Id, "Service 1-1");
        var service2 = await CreateServiceAsync(category1.Id, "Service 1-2");
        await CreateServiceAsync(category2.Id, "Service 2-1");

        // Act
        var result = await _repository.GetByCategoryAsync(category1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == service1.Id);
        result.Should().Contain(s => s.Id == service2.Id);
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Unique Service");

        // Act
        var result = await _repository.ExistsWithNameAsync(service.Name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithNonExistentName_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsWithNameAsync("Non Existent Service");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WithValidService_ShouldPersistService()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = Domain.Entities.Service.Create(category.Id, "New Service", "New Description", 10);

        // Act
        await _repository.AddAsync(service);

        // Assert
        var retrievedService = await _repository.GetByIdAsync(service.Id);
        retrievedService.Should().NotBeNull();
        retrievedService!.Name.Should().Be("New Service");
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedService_ShouldPersistChanges()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Original Name");

        // Act
        service.Update("Updated Name", "Updated Description", 5);
        await _repository.UpdateAsync(service);

        // Assert
        var retrievedService = await _repository.GetByIdAsync(service.Id);
        retrievedService.Should().NotBeNull();
        retrievedService!.Name.Should().Be("Updated Name");
        retrievedService.Description.Should().Be("Updated Description");
        retrievedService.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingService_ShouldRemoveService()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "To Be Deleted");

        // Act
        await _repository.DeleteAsync(service.Id);

        // Assert
        var retrievedService = await _repository.GetByIdAsync(service.Id);
        retrievedService.Should().BeNull();
    }

    [Fact]
    public async Task ChangeCategory_WithDifferentCategory_ShouldUpdateCategoryReference()
    {
        // Arrange
        var category1 = await CreateServiceCategoryAsync("Category 1");
        var category2 = await CreateServiceCategoryAsync("Category 2");
        var service = await CreateServiceAsync(category1.Id, "Test Service");

        // Act
        service.ChangeCategory(category2.Id);
        await _repository.UpdateAsync(service);

        // Assert
        var retrievedService = await _repository.GetByIdAsync(service.Id);
        retrievedService.Should().NotBeNull();
        retrievedService!.CategoryId.Should().Be(category2.Id);
    }
}
