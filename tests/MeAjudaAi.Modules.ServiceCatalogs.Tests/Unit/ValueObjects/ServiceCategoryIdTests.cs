using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.ValueObjects;

public sealed class ServiceCategoryIdTests
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

        // Act
        var act = () => new ServiceCategoryId(emptyGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ServiceCategoryId cannot be empty");
    }

    [Fact]
    public void New_ShouldGenerateValidServiceCategoryId()
    {
        // Act
        var categoryId = ServiceCategoryId.New();

        // Assert
        categoryId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void From_WithValidGuid_ShouldCreateServiceCategoryId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var categoryId = ServiceCategoryId.From(guid);

        // Assert
        categoryId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId = new ServiceCategoryId(guid);

        // Act
        Guid convertedGuid = categoryId;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldCreateServiceCategoryId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        ServiceCategoryId categoryId = guid;

        // Assert
        categoryId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidAsString()
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
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId1 = new ServiceCategoryId(guid);
        var categoryId2 = new ServiceCategoryId(guid);

        // Act & Assert
        categoryId1.Should().Be(categoryId2);
        (categoryId1 == categoryId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var categoryId1 = ServiceCategoryId.New();
        var categoryId2 = ServiceCategoryId.New();

        // Act & Assert
        categoryId1.Should().NotBe(categoryId2);
        (categoryId1 != categoryId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId1 = new ServiceCategoryId(guid);
        var categoryId2 = new ServiceCategoryId(guid);

        // Act & Assert
        categoryId1.GetHashCode().Should().Be(categoryId2.GetHashCode());
    }
}
