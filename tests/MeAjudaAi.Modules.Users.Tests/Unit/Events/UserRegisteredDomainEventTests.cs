using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Events;

public sealed class UserRegisteredDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var email = "user@example.com";
        var username = new Username("testuser");
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var @event = new UserRegisteredDomainEvent(
            aggregateId,
            version,
            email,
            username,
            firstName,
            lastName);

        // Assert
        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.Email.Should().Be(email);
        @event.Username.Should().Be(username);
        @event.FirstName.Should().Be(firstName);
        @event.LastName.Should().Be(lastName);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var email = "user@example.com";
        var username = new Username("testuser");
        var firstName = "John";
        var lastName = "Doe";

        var event1 = new UserRegisteredDomainEvent(aggregateId, version, email, username, firstName, lastName);
        var event2 = new UserRegisteredDomainEvent(aggregateId, version, email, username, firstName, lastName);

        // Act & Assert
        event1.Should().Be(event2);
        event1.Equals(event2).Should().BeTrue();
        (event1 == event2).Should().BeTrue();
    }

    [Fact]
    public void Deconstruct_ShouldExtractAllProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var email = "user@example.com";
        var username = new Username("testuser");
        var firstName = "John";
        var lastName = "Doe";

        var @event = new UserRegisteredDomainEvent(aggregateId, version, email, username, firstName, lastName);

        // Act
        var (id, ver, mail, user, first, last) = @event;

        // Assert
        id.Should().Be(aggregateId);
        ver.Should().Be(version);
        mail.Should().Be(email);
        user.Should().Be(username);
        first.Should().Be(firstName);
        last.Should().Be(lastName);
    }
}
