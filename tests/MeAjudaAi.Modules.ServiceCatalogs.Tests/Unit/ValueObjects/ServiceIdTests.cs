using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.ValueObjects;

public sealed class ServiceIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateServiceId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var serviceId = new ServiceId(guid);

        // Assert
        serviceId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var act = () => new ServiceId(emptyGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ServiceId cannot be empty");
    }

    [Fact]
    public void New_ShouldGenerateValidServiceId()
    {
        // Act
        var serviceId = ServiceId.New();

        // Assert
        serviceId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void From_WithValidGuid_ShouldCreateServiceId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var serviceId = ServiceId.From(guid);

        // Assert
        serviceId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var serviceId = new ServiceId(guid);

        // Act
        Guid convertedGuid = serviceId;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldCreateServiceId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        ServiceId serviceId = guid;

        // Assert
        serviceId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidAsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var serviceId = new ServiceId(guid);

        // Act
        var result = serviceId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var serviceId1 = new ServiceId(guid);
        var serviceId2 = new ServiceId(guid);

        // Act & Assert
        serviceId1.Should().Be(serviceId2);
        (serviceId1 == serviceId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var serviceId1 = ServiceId.New();
        var serviceId2 = ServiceId.New();

        // Act & Assert
        serviceId1.Should().NotBe(serviceId2);
        (serviceId1 != serviceId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var serviceId1 = new ServiceId(guid);
        var serviceId2 = new ServiceId(guid);

        // Act & Assert
        serviceId1.GetHashCode().Should().Be(serviceId2.GetHashCode());
    }
}
