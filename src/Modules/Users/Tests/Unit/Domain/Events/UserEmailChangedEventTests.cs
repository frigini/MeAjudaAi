using MeAjudaAi.Modules.Users.Domain.Events;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class UserEmailChangedEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 2;
        const string oldEmail = "old@example.com";
        const string newEmail = "new@example.com";

        // Act
        var domainEvent = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.OldEmail.Should().Be(oldEmail);
        domainEvent.NewEmail.Should().Be(newEmail);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithDifferentEmails_ShouldMaintainDistinctValues()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 3;
        const string oldEmail = "user@old-domain.com";
        const string newEmail = "user@new-domain.com";

        // Act
        var domainEvent = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);

        // Assert
        domainEvent.OldEmail.Should().Be(oldEmail);
        domainEvent.NewEmail.Should().Be(newEmail);
        domainEvent.OldEmail.Should().NotBe(domainEvent.NewEmail);
    }

    [Fact]
    public void DomainEvent_ShouldHaveCorrectEventType()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;
        const string oldEmail = "test@example.com";
        const string newEmail = "updated@example.com";

        // Act
        var domainEvent = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);

        // Assert
        domainEvent.Should().BeAssignableTo<MeAjudaAi.Shared.Events.DomainEvent>();
    }

    [Theory]
    [InlineData("", "new@example.com")]
    [InlineData("old@example.com", "")]
    public void Constructor_WithEmptyEmails_ShouldAllowEmptyStrings(string oldEmail, string newEmail)
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;

        // Act
        var domainEvent = new UserEmailChangedEvent(aggregateId, version, oldEmail, newEmail);

        // Assert
        domainEvent.OldEmail.Should().Be(oldEmail);
        domainEvent.NewEmail.Should().Be(newEmail);
    }

    [Fact]
    public void Constructor_WithSameEmails_ShouldStillCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;
        const string email = "same@example.com";

        // Act
        var domainEvent = new UserEmailChangedEvent(aggregateId, version, email, email);

        // Assert
        domainEvent.OldEmail.Should().Be(email);
        domainEvent.NewEmail.Should().Be(email);
        domainEvent.OldEmail.Should().Be(domainEvent.NewEmail);
    }
}
