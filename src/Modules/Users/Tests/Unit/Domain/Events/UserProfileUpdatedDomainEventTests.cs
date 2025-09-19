using MeAjudaAi.Modules.Users.Domain.Events;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Events;

public class UserProfileUpdatedDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var domainEvent = new UserProfileUpdatedDomainEvent(aggregateId, version, firstName, lastName);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.FirstName.Should().Be(firstName);
        domainEvent.LastName.Should().Be(lastName);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldSetOccurredAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new UserProfileUpdatedDomainEvent(Guid.NewGuid(), 1, "John", "Doe");

        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredAt.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredAt.Should().BeOnOrBefore(afterCreation);
        domainEvent.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldHaveSameProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var firstName = "John";
        var lastName = "Doe";

        var event1 = new UserProfileUpdatedDomainEvent(aggregateId, version, firstName, lastName);
        var event2 = new UserProfileUpdatedDomainEvent(aggregateId, version, firstName, lastName);

        // Act & Assert
        event1.AggregateId.Should().Be(event2.AggregateId);
        event1.Version.Should().Be(event2.Version);
        event1.FirstName.Should().Be(event2.FirstName);
        event1.LastName.Should().Be(event2.LastName);
        event1.EventType.Should().Be(event2.EventType);
    }

    [Fact]
    public void Equals_WithDifferentAggregateId_ShouldReturnFalse()
    {
        // Arrange
        var event1 = new UserProfileUpdatedDomainEvent(Guid.NewGuid(), 1, "John", "Doe");
        var event2 = new UserProfileUpdatedDomainEvent(Guid.NewGuid(), 1, "John", "Doe");

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void Equals_WithDifferentFirstName_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var event1 = new UserProfileUpdatedDomainEvent(aggregateId, 1, "John", "Doe");
        var event2 = new UserProfileUpdatedDomainEvent(aggregateId, 1, "Jane", "Doe");

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void Equals_WithDifferentLastName_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var event1 = new UserProfileUpdatedDomainEvent(aggregateId, 1, "John", "Doe");
        var event2 = new UserProfileUpdatedDomainEvent(aggregateId, 1, "John", "Smith");

        // Act & Assert
        event1.Should().NotBe(event2);
    }
}