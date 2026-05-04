using MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

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
        var category = await CreateServiceCategoryAsync("Test Category", "Test Description", 1);

        var result = await _moduleApi.GetServiceCategoryByIdAsync(category.Id.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(category.Id.Value);
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WithNonExistentCategory_ShouldReturnNull()
    {
        var result = await _moduleApi.GetServiceCategoryByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithActiveService_ShouldReturnTrue()
    {
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Active Service", isActive: true);

        var result = await _moduleApi.IsServiceActiveAsync(service.Id.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithInactiveService_ShouldReturnFalse()
    {
        var category = await CreateServiceCategoryAsync("Category");
        var service = await CreateServiceAsync(category.Id, "Inactive Service", isActive: false);

        var result = await _moduleApi.IsServiceActiveAsync(service.Id.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithNonExistentService_ShouldReturnFalse()
    {
        var result = await _moduleApi.IsServiceActiveAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithAllValidServices_ShouldReturnAllValid()
    {
        var category = await CreateServiceCategoryAsync("Category");
        var s1 = await CreateServiceAsync(category.Id, "Service 1", isActive: true);
        var s2 = await CreateServiceAsync(category.Id, "Service 2", isActive: true);

        var result = await _moduleApi.ValidateServicesAsync(new[] { s1.Id.Value, s2.Id.Value });

        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithSomeInvalidServices_ShouldReturnMixedResult()
    {
        var category = await CreateServiceCategoryAsync("Category");
        var s1 = await CreateServiceAsync(category.Id, "Service 1", isActive: true);
        var s2 = await CreateServiceAsync(category.Id, "Service 2", isActive: false);

        var result = await _moduleApi.ValidateServicesAsync(new[] { s1.Id.Value, s2.Id.Value });

        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InactiveServiceIds.Should().NotBeEmpty();
    }

    private async Task<ServiceCategoryEntity> CreateServiceCategoryAsync(string name, string? description = null, int displayOrder = 0)
    {
        var uow = GetService<MeAjudaAi.Shared.Database.IUnitOfWork>();
        var category = ServiceCategoryEntity.Create(name, description, displayOrder);
        uow.GetRepository<ServiceCategoryEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId>().Add(category);
        await uow.SaveChangesAsync();
        return category;
    }

    private async Task<ServiceEntity> CreateServiceAsync(MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceCategoryId categoryId, string name, bool isActive = true)
    {
        var uow = GetService<MeAjudaAi.Shared.Database.IUnitOfWork>();
        var service = ServiceEntity.Create(categoryId, name);
        if (isActive)
            service.Activate();
        else
            service.Deactivate();
        uow.GetRepository<ServiceEntity, MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects.ServiceId>().Add(service);
        await uow.SaveChangesAsync();
        return service;
    }
}