using Microsoft.Extensions.DependencyInjection;
using Moq;
using MeAjudaAi.Shared.Events;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Events;

// Dummy events and handlers for testing
public record TestDomainEvent(string Data) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => nameof(TestDomainEvent);
    public Guid AggregateId { get; } = Guid.NewGuid();
    public int Version { get; } = 1;
}

public record AnotherDomainEvent(int Value) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => nameof(AnotherDomainEvent);
    public Guid AggregateId { get; } = Guid.NewGuid();
    public int Version { get; } = 1;
}

public interface ITestEventHandler : IEventHandler<TestDomainEvent> { }
public interface IAnotherEventHandler : IEventHandler<AnotherDomainEvent> { }

[Trait("Category", "Unit")]
public class DomainEventProcessorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly DomainEventProcessor _processor;

    public DomainEventProcessorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _processor = new DomainEventProcessor(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithMultipleEvents_ShouldInvokeCorrectHandlers()
    {
        // Arrange
        var event1 = new TestDomainEvent("Event 1");
        var event2 = new AnotherDomainEvent(42);
        var events = new List<IDomainEvent> { event1, event2 };

        var handler1Mock = new Mock<IEventHandler<TestDomainEvent>>();
        var handler2Mock = new Mock<IEventHandler<AnotherDomainEvent>>();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestDomainEvent>>)))
            .Returns(new List<IEventHandler<TestDomainEvent>> { handler1Mock.Object });

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<AnotherDomainEvent>>)))
            .Returns(new List<IEventHandler<AnotherDomainEvent>> { handler2Mock.Object });

        // Act
        await _processor.ProcessDomainEventsAsync(events, CancellationToken.None);

        // Assert
        handler1Mock.Verify(h => h.HandleAsync(event1, It.IsAny<CancellationToken>()), Times.Once);
        handler2Mock.Verify(h => h.HandleAsync(event2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithNoHandlers_ShouldNotThrow()
    {
        // Arrange
        var @event = new TestDomainEvent("No Handler");
        var events = new List<IDomainEvent> { @event };

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestDomainEvent>>)))
            .Returns(new List<IEventHandler<TestDomainEvent>>()); // Empty list

        // Act & Assert
        Func<Task> act = () => _processor.ProcessDomainEventsAsync(events, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithMultipleHandlersForSameEvent_ShouldInvokeAll()
    {
        // Arrange
        var @event = new TestDomainEvent("Multi Handler");
        var events = new List<IDomainEvent> { @event };

        var handler1Mock = new Mock<IEventHandler<TestDomainEvent>>();
        var handler2Mock = new Mock<IEventHandler<TestDomainEvent>>();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestDomainEvent>>)))
            .Returns(new List<IEventHandler<TestDomainEvent>> { handler1Mock.Object, handler2Mock.Object });

        // Act
        await _processor.ProcessDomainEventsAsync(events, CancellationToken.None);

        // Assert
        handler1Mock.Verify(h => h.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        handler2Mock.Verify(h => h.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }
}
