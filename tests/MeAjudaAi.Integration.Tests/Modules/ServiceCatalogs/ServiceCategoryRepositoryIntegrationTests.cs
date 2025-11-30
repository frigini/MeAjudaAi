using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Integration tests for ServiceCategoryRepository with real database (TestContainers).
/// Tests actual persistence logic, EF mappings, and database constraints.
/// </summary>
public class ServiceCategoryRepositoryIntegrationTests : ApiTestBase
{
    private readonly Faker _faker = new("pt_BR");

    /// <summary>
    /// Adds a valid ServiceCategory via repository and verifies the category is persisted and retrievable by Id.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithValidCategory_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();
        var category = CreateValidCategory();

        // Act - AddAsync auto-saves internally
        await repository.AddAsync(category);

        // Assert
        var retrieved = await repository.GetByIdAsync(category.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(category.Id);
        retrieved.Name.Should().Be(category.Name);
        retrieved.Description.Should().Be(category.Description);
    }

    /// <summary>
    /// Retrieves a category by name and verifies the correct category is returned.
    /// </summary>
    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnCategory()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();
        var categoryName = _faker.Commerce.Department();
        var category = ServiceCategory.Create(categoryName, _faker.Lorem.Sentence());
        await repository.AddAsync(category);

        // Act
        var result = await repository.GetByNameAsync(categoryName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be(categoryName);
    }

    /// <summary>
    /// Retrieves all categories and verifies the count matches the expected number.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();

        var initialCount = (await repository.GetAllAsync()).Count;
        var category1 = CreateValidCategory();
        var category2 = CreateValidCategory();
        await repository.AddAsync(category1);
        await repository.AddAsync(category2);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(initialCount + 2);
    }

    /// <summary>
    /// Updates a category and verifies the changes are persisted to the database.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithModifiedCategory_ShouldPersistChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();
        var category = CreateValidCategory();
        await repository.AddAsync(category);

        // Act
        var newName = _faker.Commerce.Department();
        var newDescription = _faker.Lorem.Sentence();
        category.Update(newName, newDescription);
        await repository.UpdateAsync(category);

        // Assert
        var updated = await repository.GetByIdAsync(category.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(newName);
        updated.Description.Should().Be(newDescription);
    }

    /// <summary>
    /// Checks if a category exists by name and verifies the result is correct.
    /// </summary>
    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();
        var categoryName = _faker.Commerce.Department();
        var category = ServiceCategory.Create(categoryName, _faker.Lorem.Sentence());
        await repository.AddAsync(category);

        // Act
        var exists = await repository.ExistsWithNameAsync(categoryName);

        // Assert
        exists.Should().BeTrue();
    }

    private ServiceCategory CreateValidCategory()
    {
        return ServiceCategory.Create(
            _faker.Commerce.Department(),
            _faker.Lorem.Sentence(),
            _faker.Random.Int(0, 100));
    }
}
