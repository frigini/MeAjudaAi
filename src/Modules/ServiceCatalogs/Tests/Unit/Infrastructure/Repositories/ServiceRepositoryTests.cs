using Bogus;
using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
        // Arrange & Act
        var result = await _repository.GetByIdAsync(ServiceId.From(Guid.NewGuid()));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnService()
    {
        // Arrange
        var service = CreateTestService(name: "Unique Service Name");
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Unique Service Name");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Unique Service Name");
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByName()
    {
        // Arrange
        var services = new[]
        {
            CreateTestService(name: "C Service", displayOrder: 0),
            CreateTestService(name: "A Service", displayOrder: 0),
            CreateTestService(name: "B Service", displayOrder: 0)
        };
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result[0].Name.Should().Be("A Service");
        result[1].Name.Should().Be("B Service");
        result[2].Name.Should().Be("C Service");
    }

    [Fact]
    public async Task GetByCategoryAsync_WithExistingCategory_ShouldReturnServices()
    {
        // Arrange
        var category = CreateTestCategory();
        await _context.ServiceCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var services = new[]
        {
            CreateTestService(categoryId: category.Id, name: "S1"),
            CreateTestService(categoryId: category.Id, name: "S2")
        };
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(category.Id);

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.CategoryId == category.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetByCategoryAsync_WithActiveOnly_ShouldFilter()
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
    public async Task ExistsWithNameAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        var service = CreateTestService(name: "Existing Service");
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsWithNameAsync("Existing Service");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithNameAsync_WithNonExistingName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = await _repository.ExistsWithNameAsync("Non Existing");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WithValidService_ShouldPersist()
    {
        // Arrange
        var service = CreateTestService(name: "New Service");

        // Act
        await _repository.AddAsync(service);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Services.FindAsync(service.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Service");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingService_ShouldUpdate()
    {
        // Arrange
        var service = CreateTestService(name: "Old Name");
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var updatedService = await _context.Services.FindAsync(service.Id);
        updatedService!.GetType().GetProperty("Name")!.SetValue(updatedService, "New Name");
        
        await _repository.UpdateAsync(updatedService);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Services.FindAsync(service.Id);
        saved!.Name.Should().Be("New Name");
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
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.Services.FindAsync(service.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldNotThrow()
    {
        // Arrange & Act
        var act = async () => await _repository.DeleteAsync(ServiceId.From(Guid.NewGuid()));

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
        await _context.SaveChangesAsync();

        var deactivated = await _context.Services.FindAsync(service.Id);
        deactivated!.IsActive.Should().BeFalse();

        // Activate
        service.Activate();
        await _repository.UpdateAsync(service);
        await _context.SaveChangesAsync();

        // Assert
        var activated = await _context.Services.FindAsync(service.Id);
        activated!.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
