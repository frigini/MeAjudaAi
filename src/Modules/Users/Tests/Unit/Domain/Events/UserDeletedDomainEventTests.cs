using MeAjudaAi.Modules.Users.Domain.Events;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Events;

public class UserDeletedDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;

        // Act
        var domainEvent = new UserDeletedDomainEvent(aggregateId, version);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldSetOccurredAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new UserDeletedDomainEvent(Guid.NewGuid(), 1);

        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredAt.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredAt.Should().BeOnOrBefore(afterCreation);
        domainEvent.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldHaveSameAggregateIdAndVersion()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;

        var event1 = new UserDeletedDomainEvent(aggregateId, version);
        var event2 = new UserDeletedDomainEvent(aggregateId, version);

        // Act & Assert
        event1.AggregateId.Should().Be(event2.AggregateId);
        event1.Version.Should().Be(event2.Version);
        event1.EventType.Should().Be(event2.EventType);
    }

    [Fact]
    public void Equals_WithDifferentAggregateId_ShouldReturnFalse()
    {
        // Arrange
        var event1 = new UserDeletedDomainEvent(Guid.NewGuid(), 1);
        var event2 = new UserDeletedDomainEvent(Guid.NewGuid(), 1);

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void Equals_WithDifferentVersion_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var event1 = new UserDeletedDomainEvent(aggregateId, 1);
        var event2 = new UserDeletedDomainEvent(aggregateId, 2);

        // Act & Assert
        event1.Should().NotBe(event2);
    }
}