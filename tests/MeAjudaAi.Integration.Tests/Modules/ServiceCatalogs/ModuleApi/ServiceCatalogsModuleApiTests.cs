using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs.ModuleApi;

public class ServiceCatalogsModuleApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.ServiceCatalogs;

    #region GetServiceCategoryByIdAsync

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WhenCategoryExists_ReturnsDto()
    {
        // Arrange
        var categoryId = ServiceCategoryId.New();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Limpeza", "Serviços de limpeza", 1);
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServiceCategoryByIdAsync(categoryId.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(categoryId.Value);
        result.Value.Name.Should().Be("Limpeza");
        result.Value.Description.Should().Be("Serviços de limpeza");
        result.Value.IsActive.Should().BeTrue();
        result.Value.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WhenCategoryNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServiceCategoryByIdAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WhenEmptyGuid_ReturnsFailure()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServiceCategoryByIdAsync(Guid.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region GetAllServiceCategoriesAsync

    [Fact]
    public async Task GetAllServiceCategoriesAsync_WithActiveOnly_ReturnsOnlyActive()
    {
        // Arrange
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var active = ServiceCategory.Create("Ativa", "Categoria ativa");
            var inactive = ServiceCategory.Create("Inativa", "Categoria inativa");
            inactive.Deactivate();
            db.ServiceCategories.AddRange(active, inactive);
            await db.SaveChangesAsync();
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetAllServiceCategoriesAsync(activeOnly: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GetAllServiceCategoriesAsync_WithAll_ReturnsActiveAndInactive()
    {
        // Arrange
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var active = ServiceCategory.Create("Ativa");
            var inactive = ServiceCategory.Create("Inativa");
            inactive.Deactivate();
            db.ServiceCategories.AddRange(active, inactive);
            await db.SaveChangesAsync();
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetAllServiceCategoriesAsync(activeOnly: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetServiceByIdAsync

    [Fact]
    public async Task GetServiceByIdAsync_WhenServiceExists_ReturnsDto()
    {
        // Arrange
        var serviceId = ServiceId.New();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Reparos");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var service = Service.Create(category.Id, "Conserto de Torneira", "Reparo completo");
            db.Services.Add(service);
            await db.SaveChangesAsync();
            serviceId = service.Id;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServiceByIdAsync(serviceId.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(serviceId.Value);
        result.Value.Name.Should().Be("Conserto de Torneira");
        result.Value.Description.Should().Be("Reparo completo");
        result.Value.CategoryName.Should().Be("Reparos");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenServiceNotFound_ReturnsNull()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServiceByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenEmptyGuid_ReturnsFailure()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServiceByIdAsync(Guid.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region GetAllServicesAsync

    [Fact]
    public async Task GetAllServicesAsync_WithActiveOnly_ReturnsOnlyActive()
    {
        // Arrange
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Limpeza");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var active = Service.Create(category.Id, "Limpeza Residencial");
            var inactive = Service.Create(category.Id, "Limpeza Industrial");
            inactive.Deactivate();
            db.Services.AddRange(active, inactive);
            await db.SaveChangesAsync();
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetAllServicesAsync(activeOnly: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task GetAllServicesAsync_WithAll_ReturnsActiveAndInactive()
    {
        // Arrange
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Reparos");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var active = Service.Create(category.Id, "Eletricista");
            var inactive = Service.Create(category.Id, "Encanador");
            inactive.Deactivate();
            db.Services.AddRange(active, inactive);
            await db.SaveChangesAsync();
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetAllServicesAsync(activeOnly: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetServicesByCategoryAsync

    [Fact]
    public async Task GetServicesByCategoryAsync_WhenCategoryHasServices_ReturnsServices()
    {
        // Arrange
        var categoryId = ServiceCategoryId.New();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Limpeza");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var s1 = Service.Create(category.Id, "Limpeza Residencial");
            var s2 = Service.Create(category.Id, "Limpeza Comercial");
            db.Services.AddRange(s1, s2);
            await db.SaveChangesAsync();
            categoryId = category.Id;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServicesByCategoryAsync(categoryId.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(s => s.CategoryId == categoryId.Value);
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_WhenEmptyGuid_ReturnsFailure()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.GetServicesByCategoryAsync(Guid.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region IsServiceActiveAsync

    [Fact]
    public async Task IsServiceActiveAsync_WhenServiceIsActive_ReturnsTrue()
    {
        // Arrange
        var serviceId = ServiceId.New();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Reparos");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var service = Service.Create(category.Id, "Eletricista");
            db.Services.Add(service);
            await db.SaveChangesAsync();
            serviceId = service.Id;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.IsServiceActiveAsync(serviceId.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WhenServiceIsInactive_ReturnsFalse()
    {
        // Arrange
        var serviceId = ServiceId.New();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Limpeza");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var service = Service.Create(category.Id, "Limpeza Pesada");
            service.Deactivate();
            db.Services.Add(service);
            await db.SaveChangesAsync();
            serviceId = service.Id;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.IsServiceActiveAsync(serviceId.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WhenServiceNotFound_ReturnsFalse()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.IsServiceActiveAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WhenEmptyGuid_ReturnsFailure()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.IsServiceActiveAsync(Guid.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    #endregion

    #region ValidateServicesAsync

    [Fact]
    public async Task ValidateServicesAsync_WhenAllValid_ReturnsAllValid()
    {
        // Arrange
        Guid s1, s2;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Reparos");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var service1 = Service.Create(category.Id, "Eletricista");
            var service2 = Service.Create(category.Id, "Encanador");
            db.Services.AddRange(service1, service2);
            await db.SaveChangesAsync();
            s1 = service1.Id.Value;
            s2 = service2.Id.Value;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.ValidateServicesAsync(new[] { s1, s2 });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenSomeInvalid_ReturnsInvalidIds()
    {
        // Arrange
        Guid validId;
        var invalidId = Guid.NewGuid();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Limpeza");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var service = Service.Create(category.Id, "Limpeza Geral");
            db.Services.Add(service);
            await db.SaveChangesAsync();
            validId = service.Id.Value;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.ValidateServicesAsync(new[] { validId, invalidId });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().Contain(invalidId);
        result.Value.InvalidServiceIds.Should().NotContain(validId);
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenSomeInactive_ReturnsInactiveIds()
    {
        // Arrange
        Guid activeId, inactiveId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Reparos");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var active = Service.Create(category.Id, "Ativo");
            var inactive = Service.Create(category.Id, "Inativo");
            inactive.Deactivate();
            db.Services.AddRange(active, inactive);
            await db.SaveChangesAsync();
            activeId = active.Id.Value;
            inactiveId = inactive.Id.Value;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.ValidateServicesAsync(new[] { activeId, inactiveId });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().Contain(inactiveId);
        result.Value.InactiveServiceIds.Should().NotContain(activeId);
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenDuplicateIds_Deduplicates()
    {
        // Arrange
        Guid serviceId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            var category = ServiceCategory.Create("Limpeza");
            db.ServiceCategories.Add(category);
            await db.SaveChangesAsync();

            var service = Service.Create(category.Id, "Limpeza Pós-Obra");
            db.Services.Add(service);
            await db.SaveChangesAsync();
            serviceId = service.Id.Value;
        }

        using var scope2 = Services.CreateScope();
        var moduleApi = scope2.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act - Pass same ID 3 times
        var result = await moduleApi.ValidateServicesAsync(new[] { serviceId, serviceId, serviceId });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AllValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenEmptyCollection_ReturnsAllValid()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.ValidateServicesAsync(Array.Empty<Guid>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenNullCollection_ReturnsFailure()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.ValidateServicesAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenContainsEmptyGuid_ReturnsInvalid()
    {
        using var scope = Services.CreateScope();
        var moduleApi = scope.ServiceProvider.GetRequiredService<IServiceCatalogsModuleApi>();

        // Act
        var result = await moduleApi.ValidateServicesAsync(new[] { Guid.Empty, Guid.NewGuid() });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().Contain(Guid.Empty);
    }

    #endregion
}
