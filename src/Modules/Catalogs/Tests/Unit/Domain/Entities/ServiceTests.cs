using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Events;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Domain.Entities;

public class ServiceTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateService()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var name = "Plumbing Repair";
        var description = "Fix leaks and pipes";
        var displayOrder = 1;

        // Act
        var service = Service.Create(categoryId, name, description, displayOrder);

        // Assert
        service.Should().NotBeNull();
        service.Id.Should().NotBeNull();
        service.Id.Value.Should().NotBe(Guid.Empty);
        service.CategoryId.Should().Be(categoryId);
        service.Name.Should().Be(name);
        service.Description.Should().Be(description);
        service.DisplayOrder.Should().Be(displayOrder);
        service.IsActive.Should().BeTrue();
        service.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Services are created (domain events are raised internally but not exposed publicly)
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowCatalogDomainException(string? invalidName)
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());

        // Act & Assert
        var act = () => Service.Create(categoryId, invalidName!, null, 0);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var longName = new string('a', 151);

        // Act & Assert
        var act = () => Service.Create(categoryId, longName, null, 0);
        act.Should().Throw<CatalogDomainException>();
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());

        // Act & Assert
        var act = () => Service.Create(categoryId, "Valid Name", null, -1);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("*Display order cannot be negative*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateService()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Original Name", "Original Description", 1);

        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDisplayOrder = 2;

        // Act
        service.Update(newName, newDescription, newDisplayOrder);

        // Assert
        service.Name.Should().Be(newName);
        service.Description.Should().Be(newDescription);
        service.DisplayOrder.Should().Be(newDisplayOrder);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithNegativeDisplayOrder_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);

        // Act & Assert
        var act = () => service.Update("Valid Name", null, -5);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("*Display order cannot be negative*");
    }

    [Fact]
    public void ChangeCategory_WithDifferentCategory_ShouldChangeCategory()
    {
        // Arrange
        var originalCategoryId = new ServiceCategoryId(Guid.NewGuid());
        var newCategoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(originalCategoryId, "Test Service", null, 0);

        // Act
        service.ChangeCategory(newCategoryId);

        // Assert
        service.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void ChangeCategory_WithSameCategory_ShouldNotChange()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);

        // Act
        service.ChangeCategory(categoryId);

        // Assert
        service.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateService()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);
        service.Deactivate();

        // Act
        service.Activate();

        // Assert
        service.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);

        // Act
        service.Activate();

        // Assert
        service.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateService()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);

        // Act
        service.Deactivate();

        // Assert
        service.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var categoryId = new ServiceCategoryId(Guid.NewGuid());
        var service = Service.Create(categoryId, "Test Service", null, 0);
        service.Deactivate();

        // Act
        service.Deactivate();

        // Assert
        service.IsActive.Should().BeFalse();
    }
}
