using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Infrastructure")]
public class DbContextServiceQueriesTests : IDisposable
{
    private readonly ServiceCatalogsDbContext _dbContext;
    private readonly DbContextServiceQueries _queries;

    public DbContextServiceQueriesTests()
    {
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase(databaseName: "ServiceQueriesTest_" + Guid.NewGuid())
            .Options;

        _dbContext = new ServiceCatalogsDbContext(options);
        _queries = new DbContextServiceQueries(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private async Task<ServiceCategory> CreateCategoryAsync(string name = "Test Category")
    {
        var category = ServiceCategory.Create(name, "Description", 1);
        _dbContext.ServiceCategories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    private async Task<Service> CreateServiceAsync(ServiceCategoryId categoryId, string name = "Test Service", bool active = true)
    {
        var service = Service.Create(categoryId, name, "Description", 1);
        if (active) service.Activate();
        else service.Deactivate();
        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();
        return service;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingService_ShouldReturnServiceWithCategory()
    {
        var category = await CreateCategoryAsync();
        var service = await CreateServiceAsync(category.Id);

        var result = await _queries.GetByIdAsync(service.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Service");
        result.Category.Should().NotBeNull();
        result.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        var result = await _queries.GetByIdAsync(ServiceId.From(Guid.NewGuid()));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllServices()
    {
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Service A");
        await CreateServiceAsync(category.Id, "Service B");

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnly_ShouldReturnOnlyActiveServices()
    {
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Active Service", active: true);
        await CreateServiceAsync(category.Id, "Inactive Service", active: false);

        var result = await _queries.GetAllAsync(activeOnly: true);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Service");
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldFilterByCategory()
    {
        var category1 = await CreateCategoryAsync("Category 1");
        var category2 = await CreateCategoryAsync("Category 2");
        await CreateServiceAsync(category1.Id, "Service 1");
        await CreateServiceAsync(category2.Id, "Service 2");

        var result = await _queries.GetByCategoryAsync(category1.Id);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Service 1");
    }

    [Fact]
    public async Task GetByCategoryAsync_WithActiveOnly_ShouldFilterBothCategoryAndActive()
    {
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Active Service", active: true);
        await CreateServiceAsync(category.Id, "Inactive Service", active: false);

        var result = await _queries.GetByCategoryAsync(category.Id, activeOnly: true);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Service");
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnByMultipleIds()
    {
        var category = await CreateCategoryAsync();
        var service1 = await CreateServiceAsync(category.Id, "Service 1");
        var service2 = await CreateServiceAsync(category.Id, "Service 2");
        var service3 = await CreateServiceAsync(category.Id, "Service 3");

        var result = await _queries.GetByIdsAsync(new[] { service1.Id, service3.Id });

        result.Should().HaveCount(2);
        result.Select(s => s.Id).Should().Contain(new[] { service1.Id, service3.Id });
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ShouldReturnEmpty()
    {
        var result = await _queries.GetByIdsAsync(Array.Empty<ServiceId>());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNonExistingIds_ShouldReturnEmpty()
    {
        var result = await _queries.GetByIdsAsync(new[] { ServiceId.From(Guid.NewGuid()) });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenExists_ShouldReturnTrue()
    {
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "UniqueService");

        var result = await _queries.ExistsWithNameAsync("UniqueService", null, null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _queries.ExistsWithNameAsync("NonExistent", null, null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldExcludeService()
    {
        var category = await CreateCategoryAsync();
        var service = await CreateServiceAsync(category.Id, "ToExclude");

        var result = await _queries.ExistsWithNameAsync("ToExclude", excludeId: service.Id, categoryId: null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithCategoryId_ShouldFilterByCategory()
    {
        var category1 = await CreateCategoryAsync("Category 1");
        var category2 = await CreateCategoryAsync("Category 2");
        await CreateServiceAsync(category1.Id, "SameName");
        var serviceInCat2 = await CreateServiceAsync(category2.Id, "SameName");

        var result = await _queries.ExistsWithNameAsync("SameName", null, category1.Id);

        result.Should().BeTrue();

        var resultInCat2 = await _queries.ExistsWithNameAsync("SameName", null, category2.Id);

        resultInCat2.Should().BeTrue();
    }

    [Fact]
    public async Task CountByCategoryAsync_ShouldReturnCorrectCount()
    {
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Service 1");
        await CreateServiceAsync(category.Id, "Service 2");
        await CreateServiceAsync(category.Id, "Service 3");

        var result = await _queries.CountByCategoryAsync(category.Id);

        result.Should().Be(3);
    }

    [Fact]
    public async Task CountByCategoryAsync_WithActiveOnly_ShouldCountOnlyActive()
    {
        var category = await CreateCategoryAsync();
        await CreateServiceAsync(category.Id, "Active 1", active: true);
        await CreateServiceAsync(category.Id, "Active 2", active: true);
        await CreateServiceAsync(category.Id, "Inactive", active: false);

        var result = await _queries.CountByCategoryAsync(category.Id, activeOnly: true);

        result.Should().Be(2);
    }

    [Fact]
    public async Task CountByCategoriesAsync_ShouldReturnDictionaryWithTotalAndActive()
    {
        var category1 = await CreateCategoryAsync("Category 1");
        var category2 = await CreateCategoryAsync("Category 2");
        await CreateServiceAsync(category1.Id, "Cat1 Active 1", active: true);
        await CreateServiceAsync(category1.Id, "Cat1 Active 2", active: true);
        await CreateServiceAsync(category1.Id, "Cat1 Inactive", active: false);
        await CreateServiceAsync(category2.Id, "Cat2 Active", active: true);

        var result = await _queries.CountByCategoriesAsync(new[] { category1.Id, category2.Id });

        result.Should().HaveCount(2);
        result[category1.Id].Total.Should().Be(3);
        result[category1.Id].Active.Should().Be(2);
        result[category2.Id].Total.Should().Be(1);
        result[category2.Id].Active.Should().Be(1);
    }

    [Fact]
    public async Task CountByCategoriesAsync_WithEmptyList_ShouldReturnEmptyDictionary()
    {
        var result = await _queries.CountByCategoriesAsync(Array.Empty<ServiceCategoryId>());

        result.Should().BeEmpty();
    }
}