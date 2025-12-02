using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// Integration tests for ServiceRepository with real database (TestContainers).
/// Tests actual persistence logic, EF mappings, and database constraints.
/// </summary>
public class ServiceRepositoryIntegrationTests : ApiTestBase
{
    private readonly Faker _faker = new("pt_BR");

    /// <summary>
    /// Adds a valid Service via repository and verifies the service is persisted and retrievable by Id.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithValidService_ShouldPersistToDatabase()
    {
        // Arrange & Act
        Service service;
        ServiceCategory category;
        using (var scope = Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
            var categoryRepository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();

            category = CreateValidCategory();
            await categoryRepository.AddAsync(category);

            service = Service.Create(
                category.Id,
                _faker.Commerce.ProductName(),
                _faker.Lorem.Sentence());

            await repository.AddAsync(service);
        }

        // Assert - using fresh scope to force DB round-trip
        using var verificationScope = Services.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var retrieved = await verificationRepository.GetByIdAsync(service.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(service.Id);
        retrieved.Name.Should().Be(service.Name);
        retrieved.CategoryId.Should().Be(category.Id);
    }

    /// <summary>
    /// Retrieves a service by name and verifies the correct service is returned.
    /// </summary>
    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnService()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var categoryRepository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();

        var category = CreateValidCategory();
        await categoryRepository.AddAsync(category);

        var serviceName = _faker.Commerce.ProductName();
        var service = Service.Create(category.Id, serviceName, _faker.Lorem.Sentence());
        await repository.AddAsync(service);

        // Act
        var result = await repository.GetByNameAsync(serviceName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(service.Id);
        result.Name.Should().Be(serviceName);
    }

    /// <summary>
    /// Retrieves services by category and verifies all services in that category are returned.
    /// </summary>
    [Fact]
    public async Task GetByCategoryAsync_WithMatchingCategory_ShouldReturnServices()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var categoryRepository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();

        var category = CreateValidCategory();
        await categoryRepository.AddAsync(category);

        var service1 = Service.Create(category.Id, _faker.Commerce.ProductName(), _faker.Lorem.Sentence());
        var service2 = Service.Create(category.Id, _faker.Commerce.ProductName(), _faker.Lorem.Sentence());
        await repository.AddAsync(service1);
        await repository.AddAsync(service2);

        // Act
        var result = await repository.GetByCategoryAsync(category.Id);

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().Contain(s => s.Id == service1.Id);
        result.Should().Contain(s => s.Id == service2.Id);
    }

    /// <summary>
    /// Retrieves all services and verifies the count matches the expected number.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllServices()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var categoryRepository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();

        var category = CreateValidCategory();
        await categoryRepository.AddAsync(category);

        var initialCount = (await repository.GetAllAsync()).Count;
        var service1 = Service.Create(category.Id, _faker.Commerce.ProductName(), _faker.Lorem.Sentence());
        var service2 = Service.Create(category.Id, _faker.Commerce.ProductName(), _faker.Lorem.Sentence());
        await repository.AddAsync(service1);
        await repository.AddAsync(service2);

        // Act
        var result = await repository.GetAllAsync();

        // Assert - relaxed to handle concurrent test runs
        result.Should().HaveCountGreaterThanOrEqualTo(initialCount + 2);
        result.Should().Contain(s => s.Id == service1.Id);
        result.Should().Contain(s => s.Id == service2.Id);
    }

    /// <summary>
    /// Checks if a service exists by name and verifies the result is correct.
    /// </summary>
    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        var categoryRepository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();

        var category = CreateValidCategory();
        await categoryRepository.AddAsync(category);

        var serviceName = _faker.Commerce.ProductName();
        var service = Service.Create(category.Id, serviceName, _faker.Lorem.Sentence());
        await repository.AddAsync(service);

        // Act
        var exists = await repository.ExistsWithNameAsync(serviceName);

        // Assert
        exists.Should().BeTrue();
    }

    private ServiceCategory CreateValidCategory()
    {
        // ServiceCategory.Create signature: (name, description, displayOrder)
        return ServiceCategory.Create(
            _faker.Commerce.Department(),
            _faker.Lorem.Sentence(),
            _faker.Random.Int(0, 100));
    }
}
