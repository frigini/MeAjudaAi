using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Domain.ValueObjects;

public class SearchableProviderIdTests
{
    [Fact]
    public void New_ShouldCreateNewId()
    {
        // Act
        var id = SearchableProviderId.New();

        // Assert
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_CalledMultipleTimes_ShouldReturnDifferentIds()
    {
        // Act
        var id1 = SearchableProviderId.New();
        var id2 = SearchableProviderId.New();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void From_ShouldCreateIdFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = SearchableProviderId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = SearchableProviderId.From(guid);

        // Act
        Guid convertedGuid = id;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void RecordEquality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = SearchableProviderId.From(guid);
        var id2 = SearchableProviderId.From(guid);

        // Act & Assert
        id1.Should().Be(id2);
        id1.Equals(id2).Should().BeTrue();
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = SearchableProviderId.New();
        var id2 = SearchableProviderId.New();

        // Act & Assert
        id1.Should().NotBe(id2);
        id1.Equals(id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = SearchableProviderId.From(guid);
        var id2 = SearchableProviderId.From(guid);

        // Act
        var hash1 = id1.GetHashCode();
        var hash2 = id2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ToString_ShouldReturnReadableRepresentation()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = SearchableProviderId.From(guid);

        // Act
        var stringValue = id.ToString();

        // Assert
        stringValue.Should().NotBeNullOrEmpty();
        stringValue.Should().Contain(guid.ToString());
    }

    [Fact]
    public void Value_ShouldReturnUnderlyingGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = SearchableProviderId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }
}
