using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Domain.Events;

[Trait("Category", "Unit")]
public class UserUsernameChangedEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 2;
        var oldUsername = new Username("olduser");
        var newUsername = new Username("newuser");

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, oldUsername, newUsername);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.OldUsername.Should().Be(oldUsername);
        domainEvent.NewUsername.Should().Be(newUsername);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithDifferentUsernames_ShouldMaintainDistinctValues()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 3;
        var oldUsername = new Username("original_user");
        var newUsername = new Username("updated_user");

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, oldUsername, newUsername);

        // Assert
        domainEvent.OldUsername.Value.Should().Be("original_user");
        domainEvent.NewUsername.Value.Should().Be("updated_user");
        domainEvent.OldUsername.Should().NotBe(domainEvent.NewUsername);
    }

    [Fact]
    public void DomainEvent_ShouldHaveCorrectEventType()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;
        var oldUsername = new Username("testuser");
        var newUsername = new Username("updateduser");

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, oldUsername, newUsername);

        // Assert
        domainEvent.Should().BeAssignableTo<MeAjudaAi.Shared.Events.DomainEvent>();
    }

    [Fact]
    public void Constructor_WithSameUsernames_ShouldStillCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;
        var username = new Username("sameuser");

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, username, username);

        // Assert
        domainEvent.OldUsername.Should().Be(username);
        domainEvent.NewUsername.Should().Be(username);
        domainEvent.OldUsername.Should().Be(domainEvent.NewUsername);
    }

    [Fact]
    public void Constructor_WithValidUsernameFormats_ShouldPreserveFormatting()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 2;
        var oldUsername = new Username("user.name");
        var newUsername = new Username("user_name");

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, oldUsername, newUsername);

        // Assert
        domainEvent.OldUsername.Value.Should().Be("user.name");
        domainEvent.NewUsername.Value.Should().Be("user_name");
    }

    [Fact]
    public void Constructor_WithMinimumLengthUsernames_ShouldWork()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;
        var oldUsername = new Username("abc"); // mínimo 3 caracteres
        var newUsername = new Username("xyz"); // mínimo 3 caracteres

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, oldUsername, newUsername);

        // Assert
        domainEvent.OldUsername.Value.Should().Be("abc");
        domainEvent.NewUsername.Value.Should().Be("xyz");
    }

    [Fact]
    public void Constructor_WithMaximumLengthUsernames_ShouldWork()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 1;
        var oldUsername = new Username("a".PadRight(30, '1')); // exatamente 30 caracteres
        var newUsername = new Username("b".PadRight(30, '2')); // exatamente 30 caracteres

        // Act
        var domainEvent = new UserUsernameChangedEvent(aggregateId, version, oldUsername, newUsername);

        // Assert
        domainEvent.OldUsername.Value.Should().HaveLength(30);
        domainEvent.NewUsername.Value.Should().HaveLength(30);
        domainEvent.OldUsername.Should().NotBe(domainEvent.NewUsername);
    }
}