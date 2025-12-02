using MeAjudaAi.Modules.SearchProviders.Domain.Events;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class SearchableProviderDomainEventsTests
{
    [Fact]
    public void SearchableProviderIndexedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var providerId = Guid.NewGuid();
        var name = "Healthcare Provider";
        var latitude = -21.7794;
        var longitude = -41.3397;

        // Act
        var domainEvent = new SearchableProviderIndexedDomainEvent(
            aggregateId,
            version,
            providerId,
            name,
            latitude,
            longitude);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.Name.Should().Be(name);
        domainEvent.Latitude.Should().Be(latitude);
        domainEvent.Longitude.Should().Be(longitude);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SearchableProviderUpdatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var providerId = Guid.NewGuid();

        // Act
        var domainEvent = new SearchableProviderUpdatedDomainEvent(
            aggregateId,
            version,
            providerId);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SearchableProviderRemovedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 3;
        var providerId = Guid.NewGuid();

        // Act
        var domainEvent = new SearchableProviderRemovedDomainEvent(
            aggregateId,
            version,
            providerId);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.ProviderId.Should().Be(providerId);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }
}
