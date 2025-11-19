using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Infrastructure;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Catalogs.Tests.Integration;

[Collection("CatalogsIntegrationTests")]
public class ServiceCategoryRepositoryIntegrationTests : CatalogsIntegrationTestBase
{
    private IServiceCategoryRepository _repository = null!;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _repository = GetService<IServiceCategoryRepository>();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ShouldReturnCategory()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Test Category", "Test Description", 1);

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be("Test Category");
        result.Description.Should().Be("Test Description");
        result.DisplayOrder.Should().Be(1);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentCategory_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _repository.GetByIdAsync(ServiceCategoryId.From(nonExistentId));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleCategories_ShouldReturnAllCategories()
    {
        // Arrange
        await CreateServiceCategoryAsync("Category 1", displayOrder: 1);
        await CreateServiceCategoryAsync("Category 2", displayOrder: 2);
        await CreateServiceCategoryAsync("Category 3", displayOrder: 3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Should().Contain(c => c.Name == "Category 1");
        result.Should().Contain(c => c.Name == "Category 2");
        result.Should().Contain(c => c.Name == "Category 3");
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnlyFilter_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var activeCategory = await CreateServiceCategoryAsync("Active Category");
        var inactiveCategory = await CreateServiceCategoryAsync("Inactive Category");

        inactiveCategory.Deactivate();
        await _repository.UpdateAsync(inactiveCategory);

        // Act
        var result = await _repository.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().Contain(c => c.Id == activeCategory.Id);
        result.Should().NotContain(c => c.Id == inactiveCategory.Id);
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        await CreateServiceCategoryAsync("Unique Category");

        // Act
        var result = await _repository.ExistsWithNameAsync("Unique Category");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithNonExistentName_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsWithNameAsync("Non Existent Category");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WithValidCategory_ShouldPersistCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("New Category", "New Description", 10);

        // Act
        await _repository.AddAsync(category);

        // Assert
        var retrievedCategory = await _repository.GetByIdAsync(category.Id);
        retrievedCategory.Should().NotBeNull();
        retrievedCategory!.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedCategory_ShouldPersistChanges()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("Original Name");

        // Act
        category.Update("Updated Name", "Updated Description", 5);
        await _repository.UpdateAsync(category);

        // Assert
        var retrievedCategory = await _repository.GetByIdAsync(category.Id);
        retrievedCategory.Should().NotBeNull();
        retrievedCategory!.Name.Should().Be("Updated Name");
        retrievedCategory.Description.Should().Be("Updated Description");
        retrievedCategory.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_ShouldRemoveCategory()
    {
        // Arrange
        var category = await CreateServiceCategoryAsync("To Be Deleted");

        // Act
        await _repository.DeleteAsync(category.Id);

        // Assert
        var retrievedCategory = await _repository.GetByIdAsync(category.Id);
        retrievedCategory.Should().BeNull();
    }
}
