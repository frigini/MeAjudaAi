using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.ValueObjects;

public sealed class ProviderIdTests
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
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var act = () => new ProviderId(emptyGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ProviderId cannot be empty");
    }

    [Fact]
    public void New_ShouldGenerateValidProviderId()
    {
        // Act
        var providerId = ProviderId.New();

        // Assert
        providerId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId = new ProviderId(guid);

        // Act
        Guid convertedGuid = providerId;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldCreateProviderId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        ProviderId providerId = guid;

        // Assert
        providerId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId1 = new ProviderId(guid);
        var providerId2 = new ProviderId(guid);

        // Act & Assert
        providerId1.Should().Be(providerId2);
        (providerId1 == providerId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var providerId1 = ProviderId.New();
        var providerId2 = ProviderId.New();

        // Act & Assert
        providerId1.Should().NotBe(providerId2);
        (providerId1 != providerId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var providerId1 = new ProviderId(guid);
        var providerId2 = new ProviderId(guid);

        // Act & Assert
        providerId1.GetHashCode().Should().Be(providerId2.GetHashCode());
    }
}
