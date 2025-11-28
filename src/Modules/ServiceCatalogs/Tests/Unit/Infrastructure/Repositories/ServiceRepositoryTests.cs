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
/// Testes unitários para ServiceRepository.
/// Verifica operações CRUD e consultas específicas usando InMemory database.
/// </summary>
public class ServiceRepositoryTests : IDisposable
{
    private readonly ServiceCatalogsDbContext _context;
    private readonly IServiceRepository _repository;
    private readonly Faker _faker;

    public ServiceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase(databaseName: $"ServicesTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ServiceCatalogsDbContext(options);
        _repository = new ServiceRepository(_context);
        _faker = new Faker();
    }

    private ServiceCategory CreateTestCategory(string? name = null)
    {
        return ServiceCategory.Create(
            name ?? _faker.Commerce.Department(),
            _faker.Lorem.Sentence(),
            _faker.Random.Int(1, 100)
        );
    }

    private Service CreateTestService(
        ServiceCategoryId? categoryId = null,
        string? name = null,
        bool? isActive = null,
        int? displayOrder = null)
    {
        // Create a default category if none provided
        ServiceCategory category;
        if (categoryId == null)
        {
            category = CreateTestCategory();
            _context.ServiceCategories.Add(category);
            _context.SaveChanges();
            categoryId = category.Id;
        }

        var service = Service.Create(
            categoryId,
            name ?? _faker.Commerce.ProductName(),
            _faker.Lorem.Sentence(),
            displayOrder ?? _faker.Random.Int(1, 100)
        );

        if (isActive == false)
            service.Deactivate();

        return service;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingService_ShouldReturnService()
    {
        // Arrange
        var service = CreateTestService(name: "Test Service");
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(service.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(service.Id);
        result.Name.Should().Be("Test Service");
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingService_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = new ServiceId(Guid.NewGuid());

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_WithMultipleIds_ShouldReturnMatchingServices()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var service1 = CreateTestService(categoryId: category.Id);
        var service2 = CreateTestService(categoryId: category.Id);
        var service3 = CreateTestService(categoryId: category.Id);

        await _context.Services.AddRangeAsync(service1, service2, service3);
        await _context.SaveChangesAsync();

        var ids = new[] { service1.Id, service2.Id };

        // Act
        var result = await _repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == service1.Id);
        result.Should().Contain(s => s.Id == service2.Id);
        result.Should().NotContain(s => s.Id == service3.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyIds = Array.Empty<ServiceId>();

        // Act
        var result = await _repository.GetByIdsAsync(emptyIds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNullList_ShouldReturnEmptyList()
    {
        // Arrange
        IEnumerable<ServiceId>? nullIds = null;

        // Act
        var result = await _repository.GetByIdsAsync(nullIds!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnService()
    {
        // Arrange
        var serviceName = "Unique Service Name";
        var service = CreateTestService(name: serviceName);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync(serviceName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(serviceName);
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistingName_ShouldReturnNull()
    {
        // Arrange
        var nonExistingName = "Non Existing Service";

        // Act
        var result = await _repository.GetByNameAsync(nonExistingName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldTrimAndNormalize()
    {
        // Arrange
        var serviceName = "Test Service";
        var service = CreateTestService(name: serviceName);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act - search with extra whitespace
        var result = await _repository.GetByNameAsync("  Test Service  ");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(serviceName);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllServices()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var service1 = CreateTestService(categoryId: category.Id);
        var service2 = CreateTestService(categoryId: category.Id);
        var service3 = CreateTestService(categoryId: category.Id);

        await _context.Services.AddRangeAsync(service1, service2, service3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnlyTrue_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var activeService1 = CreateTestService(categoryId: category.Id, isActive: true);
        var activeService2 = CreateTestService(categoryId: category.Id, isActive: true);
        var inactiveService = CreateTestService(categoryId: category.Id, isActive: false);

        await _context.Services.AddRangeAsync(activeService1, activeService2, inactiveService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(s => s.Id == inactiveService.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByName()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var service1 = CreateTestService(categoryId: category.Id, name: "C Service");
        var service2 = CreateTestService(categoryId: category.Id, name: "A Service");
        var service3 = CreateTestService(categoryId: category.Id, name: "B Service");

        await _context.Services.AddRangeAsync(service1, service2, service3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("A Service");
        result[1].Name.Should().Be("B Service");
        result[2].Name.Should().Be("C Service");
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnServicesFromSpecificCategory()
    {
        // Arrange
        var category1 = CreateTestCategory();
        var category2 = CreateTestCategory();
        await _context.ServiceCategories.AddRangeAsync(category1, category2);
        await _context.SaveChangesAsync();

        var service1 = CreateTestService(categoryId: category1.Id);
        var service2 = CreateTestService(categoryId: category1.Id);
        var service3 = CreateTestService(categoryId: category2.Id);

        await _context.Services.AddRangeAsync(service1, service2, service3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(category1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == service1.Id);
        result.Should().Contain(s => s.Id == service2.Id);
        result.Should().NotContain(s => s.Id == service3.Id);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithActiveOnlyTrue_ShouldFilterInactiveServices()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var activeService = CreateTestService(categoryId: category.Id, isActive: true);
        var inactiveService = CreateTestService(categoryId: category.Id, isActive: false);

        await _context.Services.AddRangeAsync(activeService, inactiveService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(category.Id, activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(activeService.Id);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldOrderByDisplayOrderThenByName()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var service1 = CreateTestService(categoryId: category.Id, name: "B Service", displayOrder: 2);
        var service2 = CreateTestService(categoryId: category.Id, name: "A Service", displayOrder: 2);
        var service3 = CreateTestService(categoryId: category.Id, name: "C Service", displayOrder: 1);

        await _context.Services.AddRangeAsync(service1, service2, service3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(category.Id);

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(service3.Id); // DisplayOrder 1
        result[1].Id.Should().Be(service2.Id); // DisplayOrder 2, name "A Service"
        result[2].Id.Should().Be(service1.Id); // DisplayOrder 2, name "B Service"
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var serviceName = "Existing Service";
        var service = CreateTestService(name: serviceName);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsWithNameAsync(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithNonExistingName_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingName = "Non Existing Service";

        // Act
        var result = await _repository.ExistsWithNameAsync(nonExistingName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithExcludeId_ShouldExcludeThatService()
    {
        // Arrange
        var serviceName = "Service Name";
        var service = CreateTestService(name: serviceName);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act - exclude the existing service
        var result = await _repository.ExistsWithNameAsync(serviceName, excludeId: service.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithCategoryId_ShouldRestrictToCategory()
    {
        // Arrange
        var category1 = CreateTestCategory();
        var category2 = CreateTestCategory();
        await _context.ServiceCategories.AddRangeAsync(category1, category2);
        await _context.SaveChangesAsync();

        var serviceName = "Same Service Name";
        var service1 = CreateTestService(categoryId: category1.Id, name: serviceName);
        var service2 = CreateTestService(categoryId: category2.Id, name: serviceName);

        await _context.Services.AddRangeAsync(service1, service2);
        await _context.SaveChangesAsync();

        // Act - check category1 only
        var result = await _repository.ExistsWithNameAsync(serviceName, categoryId: category1.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_WithValidService_ShouldPersist()
    {
        // Arrange
        var service = CreateTestService(name: "New Service");

        // Act
        await _repository.AddAsync(service);

        // Assert
        var persisted = await _context.Services.FindAsync(service.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("New Service");
    }

    [Fact]
    public async Task UpdateAsync_WithValidService_ShouldPersistChanges()
    {
        // Arrange
        var service = CreateTestService(name: "Original Name");
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Modify the service
        service.Update("Updated Name", "Updated Description", 99);

        // Act
        await _repository.UpdateAsync(service);

        // Assert
        var updated = await _context.Services.FindAsync(service.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.DisplayOrder.Should().Be(99);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingService_ShouldRemove()
    {
        // Arrange
        var service = CreateTestService();
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(service.Id);

        // Assert
        var deleted = await _context.Services.FindAsync(service.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingService_ShouldNotThrow()
    {
        // Arrange
        var nonExistingId = new ServiceId(Guid.NewGuid());

        // Act
        var act = async () => await _repository.DeleteAsync(nonExistingId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Activate_Deactivate_ShouldToggleStatus()
    {
        // Arrange
        var service = CreateTestService(isActive: true);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Deactivate
        service.Deactivate();
        await _repository.UpdateAsync(service);

        var deactivated = await _context.Services.FindAsync(service.Id);
        deactivated!.IsActive.Should().BeFalse();

        // Activate
        service.Activate();
        await _repository.UpdateAsync(service);

        // Assert
        var activated = await _context.Services.FindAsync(service.Id);
        activated!.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
