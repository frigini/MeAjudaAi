using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;
using MeAjudaAi.Shared.Database;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration;

[Collection("ServiceCatalogsIntegrationTests")]
public class ServiceCategoryRepositoryIntegrationTests : ServiceCatalogsIntegrationTestBase
{
    private IUnitOfWork _uow = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _uow = GetService<IUnitOfWork>();
    }

    [Fact]
    public async Task AddAsync_WithValidCategory_ShouldPersistCategory()
    {
        var category = ServiceCategoryEntity.Create("New Category", "Description", 1);

        _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        var persisted = await _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().TryFindAsync(category.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task TryFindAsync_WithExistingCategory_ShouldReturnCategory()
    {
        var category = ServiceCategoryEntity.Create("Test Category");
        _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        var result = await _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().TryFindAsync(category.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedCategory_ShouldPersistChanges()
    {
        var category = ServiceCategoryEntity.Create("Original Name");
        _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        category.Update("Updated Name", null, 1);
        await _uow.SaveChangesAsync();

        var persisted = await _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().TryFindAsync(category.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_ShouldRemoveCategory()
    {
        var category = ServiceCategoryEntity.Create("To Delete");
        _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().Delete(category);
        await _uow.SaveChangesAsync();

        var persisted = await _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>().TryFindAsync(category.Id);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task TryFindAsync_WithNonExistentCategory_ShouldReturnNull()
    {
        var result = await _uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>()
            .TryFindAsync(ServiceCategoryId.From(Guid.NewGuid()));

        result.Should().BeNull();
    }
}
