using FluentAssertions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Tests.Unit.Events;

/// <summary>
/// Testes para DomainEvent - classe base abstrata para eventos de domínio
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Events")]
public class DomainEventTests
{
    // Concrete implementation for testing abstract class
    private record TestDomainEvent(Guid AggregateId, int Version) : DomainEvent(AggregateId, Version);

    [Fact]
    public void Constructor_ShouldSetAggregateIdAndVersion()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 5;

        // Act
        var domainEvent = new TestDomainEvent(aggregateId, version);

        // Assert
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();

        // Act
        var event1 = new TestDomainEvent(aggregateId, 1);
        var event2 = new TestDomainEvent(aggregateId, 1);

        // Assert
        event1.Id.Should().NotBe(Guid.Empty);
        event2.Id.Should().NotBe(Guid.Empty);
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void Constructor_ShouldSetOccurredAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1);

        // Assert
        var after = DateTime.UtcNow;
        domainEvent.OccurredAt.Should().BeOnOrAfter(before);
        domainEvent.OccurredAt.Should().BeOnOrBefore(after);
        domainEvent.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void EventType_ShouldReturnTypeName()
    {
        // Act
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1);

        // Assert
        domainEvent.EventType.Should().Be(nameof(TestDomainEvent));
    }

    [Fact]
    public void Id_ShouldBeUuidV7()
    {
        // Act
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1);

        // Assert
        domainEvent.Id.Should().NotBe(Guid.Empty);

        // UUID v7 tem versão 7 no nibble apropriado
        var bytes = domainEvent.Id.ToByteArray();
        var versionByte = bytes[7];
        var version = (versionByte >> 4) & 0x0F;
        version.Should().Be(7, "DomainEvent uses UuidGenerator.NewId() which generates UUID v7");
    }

    [Fact]
    public void Constructor_WithVersion0_ShouldAccept()
    {
        // Act
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 0);

        // Assert
        domainEvent.Version.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNegativeVersion_ShouldAccept()
    {
        // Act
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), -1);

        // Assert
        domainEvent.Version.Should().Be(-1);
    }

    [Fact]
    public void DomainEvent_ShouldImplementIDomainEvent()
    {
        // Act
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1);

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void Record_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var event1 = new TestDomainEvent(aggregateId, 1);
        var event2 = new TestDomainEvent(aggregateId, 1);

        // Act & Assert
        // Records compare by value, but Id and OccurredAt are different
        event1.AggregateId.Should().Be(event2.AggregateId);
        event1.Version.Should().Be(event2.Version);
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void MultipleEvents_ShouldHaveMonotonicallyIncreasingIds()
    {
        // Arrange & Act
        var events = new List<TestDomainEvent>();
        for (int i = 0; i < 10; i++)
        {
            events.Add(new TestDomainEvent(Guid.NewGuid(), i));
            Thread.Sleep(1); // Small delay to ensure different timestamps
        }

        // Assert
        var sortedIds = events.Select(e => e.Id).OrderBy(id => id).ToList();
        sortedIds.Should().Equal(events.Select(e => e.Id),
            "UUID v7 should be sortable by timestamp");
    }

    [Fact]
    public void EventType_ShouldMatchActualTypeName()
    {
        // Arrange
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1);

        // Act
        var eventType = domainEvent.EventType;
        var actualType = domainEvent.GetType().Name;

        // Assert
        eventType.Should().Be(actualType);
    }

    [Fact]
    public void Constructor_ShouldSetAllRequiredProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int version = 3;

        // Act
        var domainEvent = new TestDomainEvent(aggregateId, version);

        // Assert
        domainEvent.Id.Should().NotBe(Guid.Empty);
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.Version.Should().Be(version);
        domainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        domainEvent.EventType.Should().NotBeNullOrEmpty();
    }
}
