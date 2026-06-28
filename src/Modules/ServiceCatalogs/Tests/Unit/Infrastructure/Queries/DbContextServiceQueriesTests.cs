using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Infrastructure")]
public class DbContextServiceQueriesTests : BaseInMemoryDatabaseTest<ServiceCatalogsDbContext>
{
    public DbContextServiceQueriesTests()
        : base(options => new ServiceCatalogsDbContext(options))
    {
    }

    private async Task<ServiceCategory> CreateCategoryAsync(string name = "Test Category")
    {
        var category = ServiceCategory.Create(name, "Description", 1);
        DbContext.ServiceCategories.Add(category);
        await DbContext.SaveChangesAsync();
        return category;
    }

    private async Task<Service> CreateServiceAsync(ServiceCategoryId categoryId, string name = "Test Service", bool active = true)
    {
        var service = Service.Create(categoryId, name, "Description", 1);
        if (active) service.Activate();
        else service.Deactivate();
        DbContext.Services.Add(service);
        await DbContext.SaveChangesAsync();
        return service;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingService_ShouldReturnServiceWithCategory()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        var service = await CreateServiceAsync(category.Id);

        // Act
        var queries = new DbContextServiceQueries(DbContext);
        var result = await queries.GetByIdAsync(service.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Service");
        result.Category.Should().NotBeNull();
        result.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var result = await queries.GetByIdAsync(ServiceId.From(Guid.NewGuid()));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllServices()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Service A");
        await CreateServiceAsync(category.Id, "Service B");
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var result = await queries.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnly_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Active Service", active: true);
        await CreateServiceAsync(category.Id, "Inactive Service", active: false);
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var result = await queries.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Service");
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldFilterByCategory()
    {
        // Arrange
        var category1 = await CreateCategoryAsync("Category 1");
        var category2 = await CreateCategoryAsync("Category 2");
        await CreateServiceAsync(category1.Id, "Service 1");
        await CreateServiceAsync(category2.Id, "Service 2");
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var result = await queries.GetByCategoryAsync(category1.Id);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Service 1");
    }

    [Fact]
    public async Task GetByCategoryAsync_WithActiveOnly_ShouldFilterBothCategoryAndActive()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Active Service", active: true);
        await CreateServiceAsync(category.Id, "Inactive Service", active: false);
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var result = await queries.GetByCategoryAsync(category.Id, activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Service");
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnByMultipleIds()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        var service1 = await CreateServiceAsync(category.Id, "Service 1");
        var service2 = await CreateServiceAsync(category.Id, "Service 2");
        var service3 = await CreateServiceAsync(category.Id, "Service 3");
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var result = await queries.GetByIdsAsync(new[] { service1.Id, service3.Id });

        // Assert
        result.Should().HaveCount(2);
        result.Select(s => s.Id).Should().Contain(new[] { service1.Id, service3.Id });
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange & Act
        var result = await new DbContextServiceQueries(DbContext).GetByIdsAsync(Array.Empty<ServiceId>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNonExistingIds_ShouldReturnEmpty()
    {
        // Arrange & Act
        var result = await new DbContextServiceQueries(DbContext).GetByIdsAsync(new[] { ServiceId.From(Guid.NewGuid()) });

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "UniqueService");

        // Act
        var result = await new DbContextServiceQueries(DbContext).ExistsWithNameAsync("UniqueService", null, null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = await new DbContextServiceQueries(DbContext).ExistsWithNameAsync("NonExistent", null, null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldExcludeService()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        var service = await CreateServiceAsync(category.Id, "ToExclude");

        // Act
        var result = await new DbContextServiceQueries(DbContext).ExistsWithNameAsync("ToExclude", excludeId: service.Id, categoryId: null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithCategoryId_ShouldFilterByCategory()
    {
        // Arrange
        var category1 = await CreateCategoryAsync("Category 1");
        var category2 = await CreateCategoryAsync("Category 2");
        await CreateServiceAsync(category1.Id, "SameName");
        await CreateServiceAsync(category2.Id, "SameName");
        var queries = new DbContextServiceQueries(DbContext);

        // Act
        var resultInCat1 = await queries.ExistsWithNameAsync("SameName", null, category1.Id);
        var resultInCat2 = await queries.ExistsWithNameAsync("SameName", null, category2.Id);

        // Assert
        resultInCat1.Should().BeTrue();
        resultInCat2.Should().BeTrue();
    }

    [Fact]
    public async Task CountByCategoryAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Service 1");
        await CreateServiceAsync(category.Id, "Service 2");
        await CreateServiceAsync(category.Id, "Service 3");

        // Act
        var result = await new DbContextServiceQueries(DbContext).CountByCategoryAsync(category.Id);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountByCategoryAsync_WithActiveOnly_ShouldCountOnlyActive()
    {
        // Arrange
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Active 1", active: true);
        await CreateServiceAsync(category.Id, "Active 2", active: true);
        await CreateServiceAsync(category.Id, "Inactive", active: false);

        // Act
        var result = await new DbContextServiceQueries(DbContext).CountByCategoryAsync(category.Id, activeOnly: true);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task CountByCategoriesAsync_ShouldReturnDictionaryWithTotalAndActive()
    {
        // Arrange
        var category1 = await CreateCategoryAsync("Category 1");
        var category2 = await CreateCategoryAsync("Category 2");
        await CreateServiceAsync(category1.Id, "Cat1 Active 1", active: true);
        await CreateServiceAsync(category1.Id, "Cat1 Active 2", active: true);
        await CreateServiceAsync(category1.Id, "Cat1 Inactive", active: false);
        await CreateServiceAsync(category2.Id, "Cat2 Active", active: true);

        // Act
        var result = await new DbContextServiceQueries(DbContext).CountByCategoriesAsync(new[] { category1.Id, category2.Id });

        // Assert
        result.Should().HaveCount(2);
        result[category1.Id].Total.Should().Be(3);
        result[category1.Id].Active.Should().Be(2);
        result[category2.Id].Total.Should().Be(1);
        result[category2.Id].Active.Should().Be(1);
    }

    [Fact]
    public async Task CountByCategoriesAsync_WithEmptyList_ShouldReturnEmptyDictionary()
    {
        // Arrange & Act
        var result = await new DbContextServiceQueries(DbContext).CountByCategoriesAsync(Array.Empty<ServiceCategoryId>());

        // Assert
        result.Should().BeEmpty();
    }
}