using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.ValueObjects;

public sealed class SearchableProviderIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateSearchableProviderId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var providerId = new SearchableProviderId(guid);

        // Assert
        providerId.Value.Should().Be(guid);
    }

    [Fact]
    public void New_ShouldGenerateValidSearchableProviderId()
    {
        // Act
        var providerId = SearchableProviderId.New();

        // Assert
        providerId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void From_WithValidGuid_ShouldCreateSearchableProviderId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var providerId = SearchableProviderId.From(guid);

        // Assert
        providerId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId = new SearchableProviderId(guid);

        // Act
        Guid convertedGuid = providerId;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId1 = new SearchableProviderId(guid);
        var providerId2 = new SearchableProviderId(guid);

        // Act & Assert
        providerId1.Should().Be(providerId2);
        providerId1.Equals(providerId2).Should().BeTrue();
        (providerId1 == providerId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var providerId1 = SearchableProviderId.New();
        var providerId2 = SearchableProviderId.New();

        // Act & Assert
        providerId1.Should().NotBe(providerId2);
        providerId1.Equals(providerId2).Should().BeFalse();
        (providerId1 != providerId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId1 = new SearchableProviderId(guid);
        var providerId2 = new SearchableProviderId(guid);

        // Act & Assert
        providerId1.GetHashCode().Should().Be(providerId2.GetHashCode());
    }

    [Fact]
    public void Record_WithExpression_ShouldCreateNewInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId = new SearchableProviderId(guid);
        var newGuid = Guid.NewGuid();

        // Act
        var newProviderId = providerId with { Value = newGuid };

        // Assert
        newProviderId.Value.Should().Be(newGuid);
        providerId.Value.Should().Be(guid); // original unchanged
    }

    [Fact]
    public void Deconstruct_ShouldExtractValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId = new SearchableProviderId(guid);

        // Act
        var (value) = providerId;

        // Assert
        value.Should().Be(guid);
    }
}
