using Bogus;
using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Testes unitários para ServiceCategoryRepository.
/// Verifica operações CRUD e consultas específicas usando InMemory database.
/// </summary>
public class ServiceCategoryRepositoryTests : IDisposable
{
    private readonly ServiceCatalogsDbContext _context;
    private readonly IServiceCategoryRepository _repository;
    private readonly Faker _faker;

    public ServiceCategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase(databaseName: $"ServiceCategoriesTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ServiceCatalogsDbContext(options);
        _repository = new ServiceCategoryRepository(_context);
        _faker = new Faker();
    }

    private ServiceCategory CreateTestCategory(
        string? name = null,
        bool? isActive = null,
        int? displayOrder = null)
    {
        var category = ServiceCategory.Create(
            name ?? _faker.Commerce.Department(),
            _faker.Lorem.Sentence(),
            displayOrder ?? _faker.Random.Int(1, 100)
        );

        if (isActive == false)
            category.Deactivate();

        return category;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ShouldReturnCategory()
    {
        // Arrange
        var category = CreateTestCategory(name: "Test Category");
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingCategory_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = new ServiceCategoryId(Guid.NewGuid());

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnCategory()
    {
        // Arrange
        var categoryName = "Unique Category Name";
        var category = CreateTestCategory(name: categoryName);
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(categoryName);
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistingName_ShouldReturnNull()
    {
        // Arrange
        var nonExistingName = "Non Existing Category";

        // Act
        var result = await _repository.GetByNameAsync(nonExistingName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldTrimAndNormalize()
    {
        // Arrange
        var categoryName = "Test Category";
        var category = CreateTestCategory(name: categoryName);
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act - search with extra whitespace
        var result = await _repository.GetByNameAsync("  Test Category  ");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(categoryName);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var category1 = CreateTestCategory(displayOrder: 1);
        var category2 = CreateTestCategory(displayOrder: 2);
        var category3 = CreateTestCategory(displayOrder: 3);

        await _context.ServiceCategories.AddRangeAsync(category1, category2, category3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnlyTrue_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var activeCategory1 = CreateTestCategory(isActive: true);
        var activeCategory2 = CreateTestCategory(isActive: true);
        var inactiveCategory = CreateTestCategory(isActive: false);

        await _context.ServiceCategories.AddRangeAsync(activeCategory1, activeCategory2, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(c => c.Id == inactiveCategory.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByDisplayOrderThenByName()
    {
        // Arrange
        var category1 = CreateTestCategory(name: "B Category", displayOrder: 2);
        var category2 = CreateTestCategory(name: "A Category", displayOrder: 2);
        var category3 = CreateTestCategory(name: "C Category", displayOrder: 1);

        await _context.ServiceCategories.AddRangeAsync(category1, category2, category3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(category3.Id); // DisplayOrder 1
        result[1].Id.Should().Be(category2.Id); // DisplayOrder 2, name "A Category"
        result[2].Id.Should().Be(category1.Id); // DisplayOrder 2, name "B Category"
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var categoryName = "Existing Category";
        var category = CreateTestCategory(name: categoryName);
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsWithNameAsync(categoryName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithNonExistingName_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingName = "Non Existing Category";

        // Act
        var result = await _repository.ExistsWithNameAsync(nonExistingName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldExcludeThatCategory()
    {
        // Arrange
        var categoryName = "Category Name";
        var category = CreateTestCategory(name: categoryName);
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act - exclude the existing category
        var result = await _repository.ExistsWithNameAsync(categoryName, excludeId: category.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldFindOtherCategories()
    {
        // Arrange
        var categoryName = "Same Name";
        var category1 = CreateTestCategory(name: categoryName);
        var category2 = CreateTestCategory(name: categoryName);

        await _context.ServiceCategories.AddRangeAsync(category1, category2);
        await _context.SaveChangesAsync();

        // Act - exclude category1, should still find category2
        var result = await _repository.ExistsWithNameAsync(categoryName, excludeId: category1.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_WithValidCategory_ShouldPersist()
    {
        // Arrange
        var category = CreateTestCategory(name: "New Category");

        // Act
        await _repository.AddAsync(category);

        // Assert
        var persisted = await _context.ServiceCategories.FindAsync(category.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task UpdateAsync_WithValidCategory_ShouldPersistChanges()
    {
        // Arrange
        var category = CreateTestCategory(name: "Original Name");
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Modify the category
        category.Update("Updated Name", "Updated Description", 99);

        // Act
        await _repository.UpdateAsync(category);

        // Assert
        var updated = await _context.ServiceCategories.FindAsync(category.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.DisplayOrder.Should().Be(99);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_ShouldRemove()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(category.Id);

        // Assert
        var deleted = await _context.ServiceCategories.FindAsync(category.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingCategory_ShouldNotThrow()
    {
        // Arrange
        var nonExistingId = new ServiceCategoryId(Guid.NewGuid());

        // Act
        var act = async () => await _repository.DeleteAsync(nonExistingId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Activate_Deactivate_ShouldToggleStatus()
    {
        // Arrange
        var category = CreateTestCategory(isActive: true);
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Deactivate
        category.Deactivate();
        await _repository.UpdateAsync(category);

        var deactivated = await _context.ServiceCategories.FindAsync(category.Id);
        deactivated!.IsActive.Should().BeFalse();

        // Activate
        category.Activate();
        await _repository.UpdateAsync(category);

        // Assert
        var activated = await _context.ServiceCategories.FindAsync(category.Id);
        activated!.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
