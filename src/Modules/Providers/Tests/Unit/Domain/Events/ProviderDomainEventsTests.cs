using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class ProviderDomainEventsTests
{
    [Fact]
    public void ProviderActivatedDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 5;
        var userId = Guid.NewGuid();
        var name = "Healthcare Provider";
        var activatedBy = "admin@example.com";

        // Act
        var domainEvent = new ProviderActivatedDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            activatedBy);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.ActivatedBy.Should().Be(activatedBy);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderActivatedDomainEvent_WithNullActivatedBy_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 5;
        var userId = Guid.NewGuid();
        var name = "Healthcare Provider";

        // Act
        var domainEvent = new ProviderActivatedDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            null);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.ActivatedBy.Should().BeNull();
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProviderRegisteredDomainEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var userId = Guid.NewGuid();
        var name = "New Healthcare Provider";
        var type = EProviderType.Individual;
        var email = "provider@example.com";

        // Act
        var domainEvent = new ProviderRegisteredDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            type,
            email);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Name.Should().Be(name);
        domainEvent.Type.Should().Be(type);
        domainEvent.Email.Should().Be(email);
        domainEvent.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
    }
}
