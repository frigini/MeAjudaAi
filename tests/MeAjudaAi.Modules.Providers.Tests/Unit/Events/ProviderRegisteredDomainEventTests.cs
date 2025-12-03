using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Events;

public sealed class ProviderRegisteredDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var userId = Guid.NewGuid();
        var name = "John's Plumbing Services";
        var type = EProviderType.Individual;
        var email = "john@plumbing.com";
        var before = DateTime.UtcNow;

        // Act
        var @event = new ProviderRegisteredDomainEvent(
            aggregateId,
            version,
            userId,
            name,
            type,
            email);

        // Assert
        var after = DateTime.UtcNow;

        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.UserId.Should().Be(userId);
        @event.Name.Should().Be(name);
        @event.Type.Should().Be(type);
        @event.Email.Should().Be(email);
        @event.OccurredOn.Should().BeOnOrAfter(before);
        @event.OccurredOn.Should().BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(EProviderType.Individual)]
    [InlineData(EProviderType.Company)]
    public void Constructor_WithDifferentProviderTypes_ShouldStoreCorrectType(EProviderType providerType)
    {
        // Arrange & Act
        var @event = new ProviderRegisteredDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            "Test Provider",
            providerType,
            "test@example.com");

        // Assert
        @event.Type.Should().Be(providerType);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var userId = Guid.NewGuid();
        var name = "John's Plumbing Services";
        var type = EProviderType.Individual;
        var email = "john@plumbing.com";

        var event1 = new ProviderRegisteredDomainEvent(aggregateId, version, userId, name, type, email);
        var event2 = new ProviderRegisteredDomainEvent(aggregateId, version, userId, name, type, email);

        // Act & Assert
        event1.Should().Be(event2);
        event1.Equals(event2).Should().BeTrue();
        (event1 == event2).Should().BeTrue();
        event1.GetHashCode().Should().Be(event2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var event1 = new ProviderRegisteredDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            "Provider A",
            EProviderType.Individual,
            "a@example.com");

        var event2 = new ProviderRegisteredDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            "Provider B",
            EProviderType.Company,
            "b@example.com");

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
        var version = 1;
        var userId = Guid.NewGuid();
        var name = "John's Plumbing Services";
        var type = EProviderType.Individual;
        var email = "john@plumbing.com";

        var @event = new ProviderRegisteredDomainEvent(aggregateId, version, userId, name, type, email);

        // Act
        var (id, ver, user, providerName, providerType, providerEmail) = @event;

        // Assert
        id.Should().Be(aggregateId);
        ver.Should().Be(version);
        user.Should().Be(userId);
        providerName.Should().Be(name);
        providerType.Should().Be(type);
        providerEmail.Should().Be(email);
    }
}
