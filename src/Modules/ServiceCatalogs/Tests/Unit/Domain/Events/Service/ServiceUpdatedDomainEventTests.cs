using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Events.Service;

public class ServiceUpdatedDomainEventTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldCreateEventWithServiceId()
    {
        // Arrange
        var serviceId = ServiceId.New();

        // Act
        var domainEvent = new ServiceUpdatedDomainEvent(serviceId);

        // Assert
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.AggregateId.Should().Be(serviceId.Value);
        domainEvent.Version.Should().Be(1);
        domainEvent.EventType.Should().Be(nameof(ServiceUpdatedDomainEvent));
        domainEvent.Id.Should().NotBeEmpty();
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldGenerateUniqueIdForEachInstance()
    {
        // Arrange
        var serviceId = ServiceId.New();

        // Act
        var event1 = new ServiceUpdatedDomainEvent(serviceId);
        var event2 = new ServiceUpdatedDomainEvent(serviceId);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }
}
