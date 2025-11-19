using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.ValueObjects;

public class ServiceIdTests
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

        // Act & Assert
        var act = () => new ServiceId(emptyGuid);
        act.Should().Throw<ArgumentException>()
            .WithMessage("ServiceId cannot be empty*");
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
        serviceId1.GetHashCode().Should().Be(serviceId2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var serviceId1 = new ServiceId(Guid.NewGuid());
        var serviceId2 = new ServiceId(Guid.NewGuid());

        // Act & Assert
        serviceId1.Should().NotBe(serviceId2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
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
    public void ValueObject_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var serviceId1 = new ServiceId(guid);
        var serviceId2 = new ServiceId(guid);
        var serviceId3 = new ServiceId(Guid.NewGuid());

        // Act & Assert
        (serviceId1 == serviceId2).Should().BeTrue();
        (serviceId1 != serviceId3).Should().BeTrue();
        serviceId1.Equals(serviceId2).Should().BeTrue();
        serviceId1.Equals(serviceId3).Should().BeFalse();
        serviceId1.Equals(null).Should().BeFalse();
    }
}
