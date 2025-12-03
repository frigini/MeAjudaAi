using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Events;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Events;

public sealed class UserEmailChangedEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var oldEmail = "old@example.com";
        var newEmail = "new@example.com";

        // Act
        var @event = new UserEmailChangedEvent(
            aggregateId,
            version,
            oldEmail,
            newEmail);

        // Assert
        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.OldEmail.Should().Be(oldEmail);
        @event.NewEmail.Should().Be(newEmail);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var oldEmail = "old@example.com";
        var newEmail = "new@example.com";

        var event1 = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);
        var event2 = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);

        // Act & Assert
        event1.Should().Be(event2);
        event1.Equals(event2).Should().BeTrue();
        (event1 == event2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmails_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;

        var event1 = new UserEmailChangedEvent(aggregateId, version, "old1@example.com", "new1@example.com");
        var event2 = new UserEmailChangedEvent(aggregateId, version, "old2@example.com", "new2@example.com");

        // Act & Assert
        event1.Should().NotBe(event2);
        event1.Equals(event2).Should().BeFalse();
        (event1 != event2).Should().BeTrue();
    }

    [Fact]
    public void Deconstruct_ShouldExtractAllProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var oldEmail = "old@example.com";
        var newEmail = "new@example.com";

        var @event = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);

        // Act
        var (id, ver, old, @new) = @event;

        // Assert
        id.Should().Be(aggregateId);
        ver.Should().Be(version);
        old.Should().Be(oldEmail);
        @new.Should().Be(newEmail);
    }
}
