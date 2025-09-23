using MeAjudaAi.Modules.Users.Domain.Events;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Events;

public class UserRegisteredDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var email = "test@example.com";
        var username = "testuser";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var domainEvent = new UserRegisteredDomainEvent(
            aggregateId, 
            version, 
            email, 
            username, 
            firstName, 
            lastName);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.Email.Should().Be(email);
        domainEvent.Username.Value.Should().Be(username);
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
        var domainEvent = new UserRegisteredDomainEvent(
            Guid.NewGuid(), 
            1, 
            "test@example.com", 
            "testuser", 
            "John", 
            "Doe");

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
        var email = "test@example.com";
        var username = "testuser";
        var firstName = "John";
        var lastName = "Doe";

        var event1 = new UserRegisteredDomainEvent(aggregateId, version, email, username, firstName, lastName);
        var event2 = new UserRegisteredDomainEvent(aggregateId, version, email, username, firstName, lastName);

        // Act & Assert
        event1.AggregateId.Should().Be(event2.AggregateId);
        event1.Version.Should().Be(event2.Version);
        event1.Email.Should().Be(event2.Email);
        event1.Username.Should().Be(event2.Username);
        event1.FirstName.Should().Be(event2.FirstName);
        event1.LastName.Should().Be(event2.LastName);
        event1.EventType.Should().Be(event2.EventType);
    }

    [Fact]
    public void Equals_WithDifferentAggregateId_ShouldReturnFalse()
    {
        // Arrange
        var event1 = new UserRegisteredDomainEvent(Guid.NewGuid(), 1, "test@example.com", "testuser", "John", "Doe");
        var event2 = new UserRegisteredDomainEvent(Guid.NewGuid(), 1, "test@example.com", "testuser", "John", "Doe");

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var event1 = new UserRegisteredDomainEvent(aggregateId, 1, "test1@example.com", "testuser", "John", "Doe");
        var event2 = new UserRegisteredDomainEvent(aggregateId, 1, "test2@example.com", "testuser", "John", "Doe");

        // Act & Assert
        event1.Should().NotBe(event2);
    }

    [Fact]
    public void Equals_WithDifferentUsername_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var event1 = new UserRegisteredDomainEvent(aggregateId, 1, "test@example.com", "testuser1", "John", "Doe");
        var event2 = new UserRegisteredDomainEvent(aggregateId, 1, "test@example.com", "testuser2", "John", "Doe");

        // Act & Assert
        event1.Should().NotBe(event2);
    }
}