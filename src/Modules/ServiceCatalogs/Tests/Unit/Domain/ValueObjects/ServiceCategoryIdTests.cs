using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.ValueObjects;

[Trait("Category", "Unit")]
public class ServiceCategoryIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateServiceCategoryId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var categoryId = new ServiceCategoryId(guid);

        // Assert
        categoryId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var act = () => new ServiceCategoryId(emptyGuid);
        act.Should().Throw<ArgumentException>()
            .WithMessage("ServiceCategoryId cannot be empty*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId1 = new ServiceCategoryId(guid);
        var categoryId2 = new ServiceCategoryId(guid);

        // Act & Assert
        categoryId1.Should().Be(categoryId2);
        categoryId1.GetHashCode().Should().Be(categoryId2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var categoryId1 = new ServiceCategoryId(Guid.NewGuid());
        var categoryId2 = new ServiceCategoryId(Guid.NewGuid());

        // Act & Assert
        categoryId1.Should().NotBe(categoryId2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId = new ServiceCategoryId(guid);

        // Act
        var result = categoryId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void ValueObject_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId1 = new ServiceCategoryId(guid);
        var categoryId2 = new ServiceCategoryId(guid);
        var categoryId3 = new ServiceCategoryId(Guid.NewGuid());

        // Act & Assert
        (categoryId1 == categoryId2).Should().BeTrue();
        (categoryId1 != categoryId3).Should().BeTrue();
        categoryId1.Equals(categoryId2).Should().BeTrue();
        categoryId1.Equals(categoryId3).Should().BeFalse();
        categoryId1.Equals(null).Should().BeFalse();
    }
}
