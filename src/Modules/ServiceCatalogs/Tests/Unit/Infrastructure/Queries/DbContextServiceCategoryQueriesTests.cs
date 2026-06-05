using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Infrastructure")]
public class DbContextServiceCategoryQueriesTests : IDisposable
{
    private readonly ServiceCatalogsDbContext _dbContext;
    private readonly DbContextServiceCategoryQueries _queries;

    public DbContextServiceCategoryQueriesTests()
    {
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase(databaseName: "ServiceCategoryQueriesTest_" + Guid.NewGuid())
            .Options;

        _dbContext = new ServiceCatalogsDbContext(options);
        _queries = new DbContextServiceCategoryQueries(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ShouldReturnCategory()
    {
        var category = ServiceCategory.Create("Test Category", "Description", 1);
        _dbContext.ServiceCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        var result = await _queries.GetByIdAsync(ServiceCategoryId.From(Guid.NewGuid()));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        _dbContext.ServiceCategories.AddRange(
            ServiceCategory.Create("Category A", "Desc", 1),
            ServiceCategory.Create("Category B", "Desc", 2)
        );
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnly_ShouldReturnOnlyActiveCategories()
    {
        var active = ServiceCategory.Create("Active", "Desc", 1);
        active.Activate();
        var inactive = ServiceCategory.Create("Inactive", "Desc", 2);
        inactive.Deactivate();
        _dbContext.ServiceCategories.AddRange(active, inactive);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetAllAsync(activeOnly: true);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenExists_ShouldReturnTrue()
    {
        var category = ServiceCategory.Create("UniqueName", "Desc", 1);
        _dbContext.ServiceCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.ExistsWithNameAsync("UniqueName", null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _queries.ExistsWithNameAsync("NonExistent", null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldExcludeCategory()
    {
        var category = ServiceCategory.Create("ToExclude", "Desc", 1);
        _dbContext.ServiceCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.ExistsWithNameAsync("ToExclude", excludeId: category.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllWithServiceCountAsync_ShouldReturnCategoriesWithCount()
    {
        var category = ServiceCategory.Create("With Services", "Desc", 1);
        _dbContext.ServiceCategories.Add(category);
        _dbContext.Services.Add(Service.Create(category.Id, "Service 1", "Desc", 1));
        _dbContext.Services.Add(Service.Create(category.Id, "Service 2", "Desc", 2));
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetAllWithServiceCountAsync();

        result.Should().HaveCount(1);
        result[0].ServiceCount.Should().Be(2);
    }
}