using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Infrastructure")]
public class DbContextServiceCategoryQueriesTests : BaseInMemoryDatabaseTest<ServiceCatalogsDbContext>
{
    private readonly DbContextServiceCategoryQueries _queries;

    public DbContextServiceCategoryQueriesTests()
        : base(options => new ServiceCatalogsDbContext(options))
    {
        _queries = new DbContextServiceCategoryQueries(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ShouldReturnCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", "Description", 1);
        DbContext.ServiceCategories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange & Act
        var result = await _queries.GetByIdAsync(ServiceCategoryId.From(Guid.NewGuid()));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        DbContext.ServiceCategories.AddRange(
            ServiceCategory.Create("Category A", "Desc", 1),
            ServiceCategory.Create("Category B", "Desc", 2)
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnly_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var active = ServiceCategory.Create("Active", "Desc", 1);
        active.Activate();
        var inactive = ServiceCategory.Create("Inactive", "Desc", 2);
        inactive.Deactivate();
        DbContext.ServiceCategories.AddRange(active, inactive);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var category = ServiceCategory.Create("UniqueName", "Desc", 1);
        DbContext.ServiceCategories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsWithNameAsync("UniqueName", null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = await _queries.ExistsWithNameAsync("NonExistent", null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldExcludeCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("ToExclude", "Desc", 1);
        DbContext.ServiceCategories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsWithNameAsync("ToExclude", excludeId: category.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllWithServiceCountAsync_ShouldReturnCategoriesWithCount()
    {
        // Arrange
        var category = ServiceCategory.Create("With Services", "Desc", 1);
        DbContext.ServiceCategories.Add(category);
        DbContext.Services.Add(Service.Create(category.Id, "Service 1", "Desc", 1));
        DbContext.Services.Add(Service.Create(category.Id, "Service 2", "Desc", 2));
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _queries.GetAllWithServiceCountAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].ServiceCount.Should().Be(2);
    }
}