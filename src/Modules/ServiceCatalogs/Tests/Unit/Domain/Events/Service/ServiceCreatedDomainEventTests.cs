using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Events.Service;

public class ServiceCreatedDomainEventTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldCreateEventWithAllProperties()
    {
        // Arrange
        var serviceId = ServiceId.New();
        var categoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCreatedDomainEvent(serviceId, categoryId);

        // Assert
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.CategoryId.Should().Be(categoryId);
        domainEvent.AggregateId.Should().Be(serviceId.Value);
        domainEvent.Version.Should().Be(1);
        domainEvent.EventType.Should().Be(nameof(ServiceCreatedDomainEvent));
        domainEvent.Id.Should().NotBeEmpty();
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldGenerateUniqueIdForEachInstance()
    {
        // Arrange
        var serviceId = ServiceId.New();
        var categoryId = ServiceCategoryId.New();

        // Act
        var event1 = new ServiceCreatedDomainEvent(serviceId, categoryId);
        var event2 = new ServiceCreatedDomainEvent(serviceId, categoryId);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }
}
