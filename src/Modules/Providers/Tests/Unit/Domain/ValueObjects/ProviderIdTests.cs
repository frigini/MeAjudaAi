using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.ValueObjects;

public class ProviderIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateProviderId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var providerId = new ProviderId(guid);

        // Assert
        providerId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new ProviderId(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ProviderId não pode ser vazio");
    }

    [Fact]
    public void New_ShouldCreateProviderIdWithNewGuid()
    {
        // Act
        var providerId = ProviderId.New();

        // Assert
        providerId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void New_CalledMultipleTimes_ShouldCreateDifferentIds()
    {
        // Act
        var id1 = ProviderId.New();
        var id2 = ProviderId.New();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Equals_WithSameGuid_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new ProviderId(guid);
        var id2 = new ProviderId(guid);

        // Act & Assert
        id1.Should().Be(id2);
        id1.Equals(id2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentGuid_ShouldReturnFalse()
    {
        // Arrange
        var id1 = ProviderId.New();
        var id2 = ProviderId.New();

        // Act & Assert
        id1.Should().NotBe(id2);
        id1.Equals(id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameGuid_ShouldReturnSameHash()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new ProviderId(guid);
        var id2 = new ProviderId(guid);

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void ImplicitOperator_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId = new ProviderId(guid);

        // Act
        Guid result = providerId;

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void ImplicitOperator_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProviderId? providerId = null;

        // Act
        var act = () =>
        {
            Guid result = providerId!;
            return result;
        };

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ImplicitOperator_FromGuid_ShouldCreateProviderId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        ProviderId providerId = guid;

        // Assert
        providerId.Value.Should().Be(guid);
    }

}
