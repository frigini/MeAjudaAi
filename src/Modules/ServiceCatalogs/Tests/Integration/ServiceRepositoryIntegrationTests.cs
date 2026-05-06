using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;
using MeAjudaAi.Shared.Database;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;
using CategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration;

[Collection("ServiceCatalogsIntegrationTests")]
public class ServiceRepositoryIntegrationTests : ServiceCatalogsIntegrationTestBase
{
    private IUnitOfWork _uow = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _uow = GetService<IUnitOfWork>();
    }

    [Fact]
    public async Task AddAsync_WithValidService_ShouldPersistService()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        _uow.GetRepository<CategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("New Service")
            .Build();

        _uow.GetRepository<ServiceEntity, ServiceId>().Add(service);
        await _uow.SaveChangesAsync();

        var persisted = await _uow.GetRepository<ServiceEntity, ServiceId>().TryFindAsync(service.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("New Service");
    }

    [Fact]
    public async Task TryFindAsync_WithExistingService_ShouldReturnService()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        _uow.GetRepository<CategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Test Service")
            .Build();

        _uow.GetRepository<ServiceEntity, ServiceId>().Add(service);
        await _uow.SaveChangesAsync();

        var result = await _uow.GetRepository<ServiceEntity, ServiceId>().TryFindAsync(service.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(service.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedService_ShouldPersistChanges()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        _uow.GetRepository<CategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original")
            .Build();

        _uow.GetRepository<ServiceEntity, ServiceId>().Add(service);
        await _uow.SaveChangesAsync();

        service.Update("Updated", null, 1);
        await _uow.SaveChangesAsync();

        var persisted = await _uow.GetRepository<ServiceEntity, ServiceId>().TryFindAsync(service.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingService_ShouldRemoveService()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        _uow.GetRepository<CategoryEntity, ServiceCategoryId>().Add(category);
        await _uow.SaveChangesAsync();

        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("ToDelete")
            .Build();

        _uow.GetRepository<ServiceEntity, ServiceId>().Add(service);
        await _uow.SaveChangesAsync();

        _uow.GetRepository<ServiceEntity, ServiceId>().Delete(service);
        await _uow.SaveChangesAsync();

        var persisted = await _uow.GetRepository<ServiceEntity, ServiceId>().TryFindAsync(service.Id);
        persisted.Should().BeNull();
    }
}
