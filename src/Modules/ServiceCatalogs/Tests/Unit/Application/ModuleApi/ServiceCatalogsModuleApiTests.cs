using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ServiceCatalogsModuleApiTests
{
    private readonly Mock<IServiceCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<ILogger<ServiceCatalogsModuleApi>> _loggerMock;
    private readonly ServiceCatalogsModuleApi _sut;

    public ServiceCatalogsModuleApiTests()
    {
        _categoryRepositoryMock = new Mock<IServiceCategoryRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _loggerMock = new Mock<ILogger<ServiceCatalogsModuleApi>>();

        _sut = new ServiceCatalogsModuleApi(
            _categoryRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturn_ServiceCatalogs()
    {
        // Act
        var result = _sut.ModuleName;

        // Assert
        result.Should().Be("ServiceCatalogs");
    }

    [Fact]
    public void ApiVersion_ShouldReturn_Version1()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_WhenRepositoryResponds_ShouldReturnTrue()
    {
        // Arrange
        var categories = new List<ServiceCategory>
        {
            new ServiceCategoryBuilder().Build()
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
        _categoryRepositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenRepositoryThrows_ShouldReturnFalse()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.IsAvailableAsync(cts.Token));
    }

    #endregion

    #region GetServiceCategoryByIdAsync Tests

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Test Category")
            .WithDescription("Test Description")
            .WithDisplayOrder(1)
            .Build();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.GetServiceCategoryByIdAsync(category.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(category.Id.Value);
        result.Value.Name.Should().Be("Test Category");
        result.Value.Description.Should().Be("Test Description");
        result.Value.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.GetServiceCategoryByIdAsync(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Category id must be provided");
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _sut.GetServiceCategoryByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceCategoryByIdAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.GetServiceCategoryByIdAsync(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error retrieving service category");
    }

    #endregion

    #region GetAllServiceCategoriesAsync Tests

    [Fact]
    public async Task GetAllServiceCategoriesAsync_WithActiveOnly_ShouldReturnActiveCategories()
    {
        // Arrange
        var categories = new List<ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Category 1").Build(),
            new ServiceCategoryBuilder().WithName("Category 2").Build()
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.GetAllServiceCategoriesAsync(activeOnly: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(c => c.Name).Should().Contain(new[] { "Category 1", "Category 2" });
    }

    [Fact]
    public async Task GetAllServiceCategoriesAsync_WithActiveOnlyFalse_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Active").Build(),
            new ServiceCategoryBuilder().WithName("Inactive").AsInactive().Build()
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.GetAllServiceCategoriesAsync(activeOnly: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        _categoryRepositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllServiceCategoriesAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.GetAllServiceCategoriesAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error retrieving service categories");
    }

    #endregion

    #region GetServiceByIdAsync Tests

    [Fact]
    public async Task GetServiceByIdAsync_WithValidId_ShouldReturnService()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Category").Build();
        var service = new ServiceBuilder()
            .WithName("Test Service")
            .WithCategoryId(category.Id)
            .Build();

        // Manually set the Category navigation property using reflection
        var categoryProperty = typeof(Service).GetProperty("Category");
        categoryProperty!.SetValue(service, category);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetServiceByIdAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(service.Id.Value);
        result.Value.Name.Should().Be("Test Service");
        result.Value.CategoryName.Should().Be("Category");
    }

    [Fact]
    public async Task GetServiceByIdAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.GetServiceByIdAsync(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Service id must be provided");
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.GetServiceByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenCategoryIsNull_ShouldUseUnknownCategoryName()
    {
        // Arrange
        var service = new ServiceBuilder().WithName("Service").Build();
        
        // Force Category to null using reflection or by not setting it
        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetServiceByIdAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CategoryName.Should().Be(ValidationMessages.Catalogs.UnknownCategoryName);
    }

    #endregion

    #region GetAllServicesAsync Tests

    [Fact]
    public async Task GetAllServicesAsync_WithActiveOnly_ShouldReturnActiveServices()
    {
        // Arrange
        var services = new List<Service>
        {
            new ServiceBuilder().WithName("Service 1").Build(),
            new ServiceBuilder().WithName("Service 2").Build()
        };

        _serviceRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.GetAllServicesAsync(activeOnly: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(s => s.Name).Should().Contain(new[] { "Service 1", "Service 2" });
    }

    [Fact]
    public async Task GetAllServicesAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _serviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.GetAllServicesAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error retrieving services");
    }

    #endregion

    #region GetServicesByCategoryAsync Tests

    [Fact]
    public async Task GetServicesByCategoryAsync_WithValidCategoryId_ShouldReturnServices()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Test Category").Build();
        var categoryId = category.Id.Value;
        var services = new List<Service>
        {
            new ServiceBuilder().WithName("Service 1").WithCategoryId(categoryId).Build(),
            new ServiceBuilder().WithName("Service 2").WithCategoryId(categoryId).Build()
        };

        // Set Category navigation property
        foreach (var service in services)
        {
            var categoryProperty = typeof(Service).GetProperty("Category");
            categoryProperty!.SetValue(service, category);
        }

        _serviceRepositoryMock
            .Setup(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.GetServicesByCategoryAsync(categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(s => s.CategoryId.Should().Be(categoryId));
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.GetServicesByCategoryAsync(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Category id must be provided");
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _serviceRepositoryMock
            .Setup(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.GetServicesByCategoryAsync(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error retrieving services");
    }

    #endregion

    #region IsServiceActiveAsync Tests

    [Fact]
    public async Task IsServiceActiveAsync_WhenServiceIsActive_ShouldReturnTrue()
    {
        // Arrange
        var service = new ServiceBuilder().Build(); // Active by default

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.IsServiceActiveAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WhenServiceIsInactive_ShouldReturnFalse()
    {
        // Arrange
        var service = new ServiceBuilder().AsInactive().Build();

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.IsServiceActiveAsync(service.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WhenServiceNotFound_ShouldReturnFalse()
    {
        // Arrange
        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.IsServiceActiveAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceActiveAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.IsServiceActiveAsync(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Service id must be provided");
    }

    #endregion

    #region ValidateServicesAsync Tests

    [Fact]
    public async Task ValidateServicesAsync_WithAllValidActiveServices_ShouldReturnAllValid()
    {
        // Arrange
        var service1 = new ServiceBuilder().Build();
        var service2 = new ServiceBuilder().Build();
        var serviceIds = new[] { service1.Id.Value, service2.Id.Value };
        var services = new List<Service> { service1, service2 };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ServiceId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.ValidateServicesAsync(serviceIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithInvalidServiceId_ShouldReturnInvalidIds()
    {
        // Arrange
        var validService = new ServiceBuilder().Build();
        var validServiceId = validService.Id.Value;
        var invalidServiceId = Guid.NewGuid();
        var serviceIds = new[] { validServiceId, invalidServiceId };

        var services = new List<Service> { validService };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ServiceId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.ValidateServicesAsync(serviceIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().Contain(invalidServiceId);
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithInactiveService_ShouldReturnInactiveIds()
    {
        // Arrange
        var activeService = new ServiceBuilder().Build();
        var inactiveService = new ServiceBuilder().AsInactive().Build();
        var activeServiceId = activeService.Id.Value;
        var inactiveServiceId = inactiveService.Id.Value;
        var serviceIds = new[] { activeServiceId, inactiveServiceId };

        var services = new List<Service> { activeService, inactiveService };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ServiceId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.ValidateServicesAsync(serviceIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().Contain(inactiveServiceId);
    }

    [Fact]
    public async Task ValidateServicesAsync_WithEmptyCollection_ShouldReturnAllValid()
    {
        // Act
        var result = await _sut.ValidateServicesAsync(Array.Empty<Guid>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
        result.Value.InvalidServiceIds.Should().BeEmpty();
        result.Value.InactiveServiceIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateServicesAsync_WithNullCollection_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.ValidateServicesAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Service IDs collection cannot be null");
    }

    [Fact]
    public async Task ValidateServicesAsync_WithEmptyGuid_ShouldReturnInvalid()
    {
        // Arrange
        var validService = new ServiceBuilder().Build();
        var serviceIds = new[] { Guid.Empty, validService.Id.Value };

        // ServiceId.From(Guid.Empty) throws ArgumentException,
        // so the method catches it and marks Empty as invalid
        var services = new List<Service> { validService };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ServiceId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.ValidateServicesAsync(serviceIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeFalse();
        result.Value.InvalidServiceIds.Should().Contain(Guid.Empty);
    }

    [Fact]
    public async Task ValidateServicesAsync_WithDuplicateIds_ShouldDeduplicateBeforeValidation()
    {
        // Arrange
        var service = new ServiceBuilder().Build();
        var serviceId = service.Id.Value;
        var serviceIds = new[] { serviceId, serviceId, serviceId }; // Duplicates

        var services = new List<Service> { service };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ServiceId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.ValidateServicesAsync(serviceIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AllValid.Should().BeTrue();
        
        // Verify repository was called only once per unique ID
        _serviceRepositoryMock.Verify(
            x => x.GetByIdsAsync(
                It.Is<IEnumerable<ServiceId>>(ids => ids.Count() == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateServicesAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var serviceIds = new[] { Guid.NewGuid() };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<ServiceId>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.ValidateServicesAsync(serviceIds);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error validating services");
    }

    #endregion
}
