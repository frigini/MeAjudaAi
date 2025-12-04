using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Events.Service;

public class ServiceCategoryChangedDomainEventTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldCreateEventWithAllProperties()
    {
        // Arrange
        var serviceId = ServiceId.New();
        var oldCategoryId = ServiceCategoryId.New();
        var newCategoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCategoryChangedDomainEvent(serviceId, oldCategoryId, newCategoryId);

        // Assert
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.OldCategoryId.Should().Be(oldCategoryId);
        domainEvent.NewCategoryId.Should().Be(newCategoryId);
        domainEvent.AggregateId.Should().Be(serviceId.Value);
        domainEvent.Version.Should().Be(1);
        domainEvent.EventType.Should().Be("ServiceCategoryChangedDomainEvent");
        domainEvent.Id.Should().NotBeEmpty();
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldGenerateUniqueIdForEachInstance()
    {
        // Arrange
        var serviceId = ServiceId.New();
        var oldCategoryId = ServiceCategoryId.New();
        var newCategoryId = ServiceCategoryId.New();

        // Act
        var event1 = new ServiceCategoryChangedDomainEvent(serviceId, oldCategoryId, newCategoryId);
        var event2 = new ServiceCategoryChangedDomainEvent(serviceId, oldCategoryId, newCategoryId);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }
}
