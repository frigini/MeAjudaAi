using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Mappers;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Mappers;

[Trait("Category", "Unit")]
public class DomainEventMapperExtensionsTests
{
    [Fact]
    public void ToIntegrationEvent_FromUserRegisteredDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var domainEvent = new UserRegisteredDomainEvent(
            userId, 1, "john@example.com", new Username("johndoe"), "John", "Doe");

        // Act
        var result = domainEvent.ToIntegrationEvent();

        // Assert
        result.Source.Should().Be("Users");
        result.UserId.Should().Be(userId);
        result.Username.Should().Be("johndoe");
        result.Email.Should().Be("john@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.KeycloakId.Should().BeEmpty();
        result.Roles.Should().BeEmpty();
        result.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToIntegrationEvent_FromUserProfileUpdatedDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var domainEvent = new UserProfileUpdatedDomainEvent(
            userId, 1, "John", "Smith");
        var email = "john@example.com";

        // Act
        var result = domainEvent.ToIntegrationEvent(email);

        // Assert
        result.Source.Should().Be("Users");
        result.UserId.Should().Be(userId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be(email);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToIntegrationEvent_FromUserDeletedDomainEvent_ShouldMapCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        var result = domainEvent.ToIntegrationEvent();

        // Assert
        result.Source.Should().Be("Users");
        result.UserId.Should().Be(userId);
        result.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToIntegrationEvent_WhenUserRegisteredDomainEventIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserRegisteredDomainEvent domainEvent = null!;

        // Act
        var act = () => domainEvent.ToIntegrationEvent();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToIntegrationEvent_WhenUserProfileUpdatedDomainEventIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserProfileUpdatedDomainEvent domainEvent = null!;

        // Act
        var act = () => domainEvent.ToIntegrationEvent("email@test.com");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToIntegrationEvent_WhenEmailIsNullOrWhiteSpace_ShouldThrowArgumentException()
    {
        // Arrange
        var domainEvent = new UserProfileUpdatedDomainEvent(Guid.NewGuid(), 1, "A", "B");

        // Act
        var act = () => domainEvent.ToIntegrationEvent("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
