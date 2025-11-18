using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Events;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Domain.Entities;

public class ServiceCategoryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateServiceCategory()
    {
        // Arrange
        var name = "Home Repairs";
        var description = "General home repair services";
        var displayOrder = 1;

        // Act
        var category = ServiceCategory.Create(name, description, displayOrder);

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().NotBeNull();
        category.Id.Value.Should().NotBe(Guid.Empty);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.DisplayOrder.Should().Be(displayOrder);
        category.IsActive.Should().BeTrue();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Service categories are created (domain events are raised internally but not exposed publicly)
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowCatalogDomainException(string? invalidName)
    {
        // Act & Assert
        var act = () => ServiceCategory.Create(invalidName!, null, 0);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var longName = new string('a', 201);

        // Act & Assert
        var act = () => ServiceCategory.Create(longName, null, 0);
        act.Should().Throw<CatalogDomainException>();
    }

    [Fact]
    public void Create_WithNegativeDisplayOrder_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var name = "Test Category";
        var negativeDisplayOrder = -1;

        // Act & Assert
        var act = () => ServiceCategory.Create(name, null, negativeDisplayOrder);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("*Display order cannot be negative*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateServiceCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("Original Name", "Original Description", 1);

        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDisplayOrder = 2;

        // Act
        category.Update(newName, newDescription, newDisplayOrder);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.DisplayOrder.Should().Be(newDisplayOrder);
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithNegativeDisplayOrder_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        var negativeDisplayOrder = -1;

        // Act & Assert
        var act = () => category.Update("Updated Name", null, negativeDisplayOrder);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage("*Display order cannot be negative*");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        category.Deactivate();

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        category.Deactivate();

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }
}
