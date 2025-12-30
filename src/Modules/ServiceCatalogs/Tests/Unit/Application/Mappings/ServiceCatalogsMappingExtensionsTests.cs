using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ServiceCatalogsMappingExtensionsTests
{
    #region ToListDto Tests

    [Fact]
    public void ToListDto_WithValidService_ShouldMapAllProperties()
    {
        // Arrange
        var service = new ServiceBuilder()
            .WithName("Test Service")
            .WithDescription("Test Description")
            .WithDisplayOrder(5)
            .Build();

        service.Activate();

        // Act
        var dto = service.ToListDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(service.Id.Value);
        dto.CategoryId.Should().Be(service.CategoryId.Value);
        dto.Name.Should().Be("Test Service");
        dto.Description.Should().Be("Test Description");
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToListDto_WithInactiveService_ShouldMapIsActiveFalse()
    {
        // Arrange
        var service = new ServiceBuilder().Build();
        service.Deactivate();

        // Act
        var dto = service.ToListDto();

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    #endregion

    #region ToDto (Service) Tests

    [Fact]
    public void ToDto_Service_WithValidServiceAndCategory_ShouldMapAllProperties()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Test Category")
            .WithDescription("Category Description")
            .Build();

        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Test Service")
            .WithDescription("Service Description")
            .WithDisplayOrder(10)
            .Build();

        // Manually set Category navigation property via reflection
        var categoryProperty = typeof(Service).GetProperty("Category");
        categoryProperty!.SetValue(service, category);

        service.Activate();

        // Act
        var dto = service.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(service.Id.Value);
        dto.CategoryId.Should().Be(service.CategoryId.Value);
        dto.CategoryName.Should().Be("Test Category");
        dto.Name.Should().Be("Test Service");
        dto.Description.Should().Be("Service Description");
        dto.IsActive.Should().BeTrue();
        dto.DisplayOrder.Should().Be(10);
        dto.CreatedAt.Should().Be(service.CreatedAt);
        dto.UpdatedAt.Should().Be(service.UpdatedAt);
    }

    [Fact]
    public void ToDto_Service_WithNullCategory_ShouldUseUnknownCategoryName()
    {
        // Arrange
        var service = new ServiceBuilder()
            .WithName("Orphan Service")
            .Build();

        // Act
        var dto = service.ToDto();

        // Assert
        dto.CategoryName.Should().Be(ValidationMessages.Catalogs.UnknownCategoryName);
        dto.Id.Should().Be(service.Id.Value);
        dto.CategoryId.Should().Be(service.CategoryId.Value);
    }

    [Fact]
    public void ToDto_Service_WithInactiveService_ShouldMapIsActiveFalse()
    {
        // Arrange
        var service = new ServiceBuilder()
            .WithName("Inactive Service")
            .AsInactive()
            .Build();

        // Act
        var dto = service.ToDto();

        // Assert
        dto.IsActive.Should().BeFalse();
        dto.CategoryName.Should().Be(ValidationMessages.Catalogs.UnknownCategoryName);
    }

    [Fact]
    public void ToDto_Service_WithMinimalData_ShouldMapSuccessfully()
    {
        // Arrange
        var service = new ServiceBuilder().Build();

        // Act
        var dto = service.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.CategoryId.Should().NotBeEmpty();
        dto.CreatedAt.Should().NotBe(default);
    }

    #endregion

    #region ToDto (ServiceCategory) Tests

    [Fact]
    public void ToDto_ServiceCategory_WithValidCategory_ShouldMapAllProperties()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Health Services")
            .WithDescription("Medical and health-related services")
            .WithDisplayOrder(3)
            .Build();

        category.Activate();

        // Act
        var dto = category.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(category.Id.Value);
        dto.Name.Should().Be("Health Services");
        dto.Description.Should().Be("Medical and health-related services");
        dto.IsActive.Should().BeTrue();
        dto.DisplayOrder.Should().Be(3);
        dto.CreatedAt.Should().Be(category.CreatedAt);
        dto.UpdatedAt.Should().Be(category.UpdatedAt);
    }

    [Fact]
    public void ToDto_ServiceCategory_WithInactiveCategory_ShouldMapIsActiveFalse()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Deprecated Category")
            .Build();

        category.Deactivate();

        // Act
        var dto = category.ToDto();

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToDto_ServiceCategory_WithMinimalData_ShouldMapSuccessfully()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().Build();

        // Act
        var dto = category.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().NotBeNullOrWhiteSpace();
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void ToDto_ServiceCategory_WithZeroDisplayOrder_ShouldMapCorrectly()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithDisplayOrder(0)
            .Build();

        // Act
        var dto = category.ToDto();

        // Assert
        dto.DisplayOrder.Should().Be(0);
    }

    #endregion
}
