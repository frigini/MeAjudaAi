using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Entities;

[Trait("Category", "Unit")]
public class ServiceCategoryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateServiceCategory()
    {
        // Arrange
        var name = "Home Repairs";
        var description = "General home repair services";
        var displayOrder = 1;
        var before = DateTime.UtcNow;

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
        category.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);

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
    public void Create_WithNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthName = new string('a', ValidationConstants.CatalogLimits.ServiceCategoryNameMaxLength);

        // Act
        var category = ServiceCategory.Create(maxLengthName, null, 0);

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().HaveLength(ValidationConstants.CatalogLimits.ServiceCategoryNameMaxLength);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var tooLongName = new string('a', ValidationConstants.CatalogLimits.ServiceCategoryNameMaxLength + 1);

        // Act & Assert
        var act = () => ServiceCategory.Create(tooLongName, null, 0);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage($"*cannot exceed {ValidationConstants.CatalogLimits.ServiceCategoryNameMaxLength} characters*");
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
    public void Create_WithLeadingAndTrailingSpacesInName_ShouldTrimSpaces()
    {
        // Arrange
        var nameWithSpaces = "  Test Category  ";
        var expectedName = "Test Category";

        // Act
        var category = ServiceCategory.Create(nameWithSpaces, null, 0);

        // Assert
        category.Name.Should().Be(expectedName);
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpacesInDescription_ShouldTrimSpaces()
    {
        // Arrange
        var descriptionWithSpaces = "  Test Description  ";
        var expectedDescription = "Test Description";

        // Act
        var category = ServiceCategory.Create("Test", descriptionWithSpaces, 0);

        // Assert
        category.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void Create_WithDescriptionAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthDescription = new string('a', ValidationConstants.CatalogLimits.ServiceCategoryDescriptionMaxLength);

        // Act
        var category = ServiceCategory.Create("Test", maxLengthDescription, 0);

        // Assert
        category.Should().NotBeNull();
        category.Description.Should().HaveLength(ValidationConstants.CatalogLimits.ServiceCategoryDescriptionMaxLength);
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldThrowCatalogDomainException()
    {
        // Arrange
        var tooLongDescription = new string('a', ValidationConstants.CatalogLimits.ServiceCategoryDescriptionMaxLength + 1);

        // Act & Assert
        var act = () => ServiceCategory.Create("Test", tooLongDescription, 0);
        act.Should().Throw<CatalogDomainException>()
            .WithMessage($"*cannot exceed {ValidationConstants.CatalogLimits.ServiceCategoryDescriptionMaxLength} characters*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateServiceCategory()
    {
        // Arrange
        var category = ServiceCategory.Create("Original Name", "Original Description", 1);
        var before = DateTime.UtcNow;

        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newDisplayOrder = 2;

        // Act
        category.Update(newName, newDescription, newDisplayOrder);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.DisplayOrder.Should().Be(newDisplayOrder);
        category.UpdatedAt.Should().NotBeNull()
            .And.Subject.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Update_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var category = ServiceCategory.Create("Original", "Original Description", 1);

        // Act
        category.Update("  Updated Name  ", "  Updated Description  ", 2);

        // Assert
        category.Name.Should().Be("Updated Name");
        category.Description.Should().Be("Updated Description");
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
    public void Activate_WhenInactive_ShouldActivateCategoryAndUpdateTimestamp()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        category.Deactivate();
        var before = DateTime.UtcNow;

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
        category.UpdatedAt.Should().NotBeNull()
            .And.Subject.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActiveWithoutUpdatingTimestamp()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        var originalUpdatedAt = category.UpdatedAt;

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
        category.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCategoryAndUpdateTimestamp()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        var before = DateTime.UtcNow;

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
        category.UpdatedAt.Should().NotBeNull()
            .And.Subject.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldRemainInactiveWithoutUpdatingTimestamp()
    {
        // Arrange
        var category = ServiceCategory.Create("Test Category", null, 0);
        category.Deactivate();
        var updatedAtAfterFirstDeactivate = category.UpdatedAt;

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
        category.UpdatedAt.Should().Be(updatedAtAfterFirstDeactivate);
    }
}
