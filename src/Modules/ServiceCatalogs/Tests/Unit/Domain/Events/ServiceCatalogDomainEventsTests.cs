using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class ServiceCatalogDomainEventsTests
{
    [Fact]
    public void ServiceCreatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var serviceId = ServiceId.New();
        var categoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCreatedDomainEvent(serviceId, categoryId);

        // Assert
        domainEvent.AggregateId.Should().Be(serviceId.Value);
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.CategoryId.Should().Be(categoryId);
        domainEvent.Version.Should().Be(1);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ServiceActivatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var serviceId = ServiceId.New();

        // Act
        var domainEvent = new ServiceActivatedDomainEvent(serviceId);

        // Assert
        domainEvent.AggregateId.Should().Be(serviceId.Value);
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.Version.Should().Be(1);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ServiceDeactivatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var serviceId = ServiceId.New();

        // Act
        var domainEvent = new ServiceDeactivatedDomainEvent(serviceId);

        // Assert
        domainEvent.AggregateId.Should().Be(serviceId.Value);
        domainEvent.ServiceId.Should().Be(serviceId);
        domainEvent.Version.Should().Be(1);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ServiceCategoryCreatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var categoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCategoryCreatedDomainEvent(categoryId);

        // Assert
        domainEvent.AggregateId.Should().Be(categoryId.Value);
        domainEvent.CategoryId.Should().Be(categoryId);
        domainEvent.Version.Should().Be(1);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ServiceCategoryActivatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var categoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCategoryActivatedDomainEvent(categoryId);

        // Assert
        domainEvent.AggregateId.Should().Be(categoryId.Value);
        domainEvent.CategoryId.Should().Be(categoryId);
        domainEvent.Version.Should().Be(1);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ServiceCategoryDeactivatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var categoryId = ServiceCategoryId.New();

        // Act
        var domainEvent = new ServiceCategoryDeactivatedDomainEvent(categoryId);

        // Assert
        domainEvent.AggregateId.Should().Be(categoryId.Value);
        domainEvent.CategoryId.Should().Be(categoryId);
        domainEvent.Version.Should().Be(1);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }
}
