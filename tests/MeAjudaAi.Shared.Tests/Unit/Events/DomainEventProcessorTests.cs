using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Unit.Events;

public class DomainEventProcessorTests : IDisposable
{
    private readonly ServiceCollection _services;
    private ServiceProvider? _serviceProvider;

    public DomainEventProcessorTests()
    {
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Test domain events
    public record TestDomainEvent(Guid AggregateId, int Version, string Data) : IDomainEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public string EventType { get; init; } = nameof(TestDomainEvent);
    }

    public record AnotherTestDomainEvent(Guid AggregateId, int Version, int Value) : IDomainEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public string EventType { get; init; } = nameof(AnotherTestDomainEvent);
    }

    // Test event handlers
    public class TestEventHandler : IEventHandler<TestDomainEvent>
    {
        public bool WasCalled { get; private set; }
        public TestDomainEvent? ReceivedEvent { get; private set; }
        public int CallCount { get; private set; }

        public Task HandleAsync(TestDomainEvent @event, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedEvent = @event;
            CallCount++;
            return Task.CompletedTask;
        }
    }

    public class SecondTestEventHandler : IEventHandler<TestDomainEvent>
    {
        public bool WasCalled { get; private set; }
        public TestDomainEvent? ReceivedEvent { get; private set; }

        public Task HandleAsync(TestDomainEvent @event, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedEvent = @event;
            return Task.CompletedTask;
        }
    }

    public class AnotherTestEventHandler : IEventHandler<AnotherTestDomainEvent>
    {
        public bool WasCalled { get; private set; }
        public AnotherTestDomainEvent? ReceivedEvent { get; private set; }

        public Task HandleAsync(AnotherTestDomainEvent @event, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedEvent = @event;
            return Task.CompletedTask;
        }
    }

    public class ThrowingEventHandler : IEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent @event, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Handler error");
        }
    }

    public class CancellableEventHandler : IEventHandler<TestDomainEvent>
    {
        public CancellationToken ReceivedToken { get; private set; }

        public Task HandleAsync(TestDomainEvent @event, CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithSingleEvent_ShouldInvokeHandler()
    {
        // Arrange
        var handler = new TestEventHandler();
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test data");
        var events = new[] { domainEvent };

        // Act
        await processor.ProcessDomainEventsAsync(events);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.ReceivedEvent.Should().Be(domainEvent);
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithMultipleHandlersForSameEvent_ShouldInvokeAllHandlers()
    {
        // Arrange
        var handler1 = new TestEventHandler();
        var handler2 = new SecondTestEventHandler();

        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler1);
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler2);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test data");
        var events = new[] { domainEvent };

        // Act
        await processor.ProcessDomainEventsAsync(events);

        // Assert
        handler1.WasCalled.Should().BeTrue();
        handler1.ReceivedEvent.Should().Be(domainEvent);

        handler2.WasCalled.Should().BeTrue();
        handler2.ReceivedEvent.Should().Be(domainEvent);
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithMultipleEvents_ShouldProcessAllEvents()
    {
        // Arrange
        var handler = new TestEventHandler();
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var event1 = new TestDomainEvent(Guid.NewGuid(), 1, "event 1");
        var event2 = new TestDomainEvent(Guid.NewGuid(), 2, "event 2");
        var event3 = new TestDomainEvent(Guid.NewGuid(), 3, "event 3");
        var events = new[] { event1, event2, event3 };

        // Act
        await processor.ProcessDomainEventsAsync(events);

        // Assert
        handler.CallCount.Should().Be(3);
        handler.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithDifferentEventTypes_ShouldInvokeCorrectHandlers()
    {
        // Arrange
        var testHandler = new TestEventHandler();
        var anotherHandler = new AnotherTestEventHandler();

        _services.AddSingleton<IEventHandler<TestDomainEvent>>(testHandler);
        _services.AddSingleton<IEventHandler<AnotherTestDomainEvent>>(anotherHandler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var testEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test");
        var anotherEvent = new AnotherTestDomainEvent(Guid.NewGuid(), 1, 42);
        var events = new IDomainEvent[] { testEvent, anotherEvent };

        // Act
        await processor.ProcessDomainEventsAsync(events);

        // Assert
        testHandler.WasCalled.Should().BeTrue();
        testHandler.ReceivedEvent.Should().Be(testEvent);

        anotherHandler.WasCalled.Should().BeTrue();
        anotherHandler.ReceivedEvent.Should().Be(anotherEvent);
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithNoHandlers_ShouldCompleteWithoutError()
    {
        // Arrange
        _serviceProvider = _services.BuildServiceProvider();
        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test");
        var events = new[] { domainEvent };

        // Act
        var act = async () => await processor.ProcessDomainEventsAsync(events);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithEmptyEventsList_ShouldCompleteWithoutError()
    {
        // Arrange
        _serviceProvider = _services.BuildServiceProvider();
        var processor = new DomainEventProcessor(_serviceProvider);
        var events = Array.Empty<IDomainEvent>();

        // Act
        var act = async () => await processor.ProcessDomainEventsAsync(events);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var throwingHandler = new ThrowingEventHandler();
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(throwingHandler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test");
        var events = new[] { domainEvent };

        // Act
        var exception = await Assert.ThrowsAsync<System.Reflection.TargetInvocationException>(() =>
            processor.ProcessDomainEventsAsync(events));

        // Assert
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WhenFirstHandlerThrows_ShouldNotInvokeSecondHandler()
    {
        // Arrange
        var throwingHandler = new ThrowingEventHandler();
        var normalHandler = new TestEventHandler();

        _services.AddSingleton<IEventHandler<TestDomainEvent>>(throwingHandler);
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(normalHandler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test");
        var events = new[] { domainEvent };

        // Act
        var exception = await Assert.ThrowsAsync<TargetInvocationException>(() =>
            processor.ProcessDomainEventsAsync(events));

        // Assert
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
        normalHandler.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithCancellationToken_ShouldPassTokenToHandlers()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var handler = new CancellableEventHandler();

        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test");
        var events = new[] { domainEvent };

        // Act
        await processor.ProcessDomainEventsAsync(events, cts.Token);

        // Assert
        handler.ReceivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handler = new CancellableEventHandler();
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var processor = new DomainEventProcessor(_serviceProvider);
        var domainEvent = new TestDomainEvent(Guid.NewGuid(), 1, "test");
        var events = new[] { domainEvent };

        // Act
        var exception = await Assert.ThrowsAsync<TargetInvocationException>(() =>
            processor.ProcessDomainEventsAsync(events, cts.Token));

        // Assert
        exception.InnerException.Should().BeOfType<OperationCanceledException>();
    }

    [Fact]
    public async Task ProcessDomainEventsAsync_ShouldProcessEventsSequentially()
    {
        // Arrange
        var handler = new TestEventHandler();
        _services.AddSingleton<IEventHandler<TestDomainEvent>>(handler);

        _serviceProvider = _services.BuildServiceProvider();
        var processor = new DomainEventProcessor(_serviceProvider);

        var event1 = new TestDomainEvent(Guid.NewGuid(), 1, "event1");
        var event2 = new TestDomainEvent(Guid.NewGuid(), 2, "event2");
        var events = new[] { event1, event2 };

        // Act
        await processor.ProcessDomainEventsAsync(events);

        // Assert - if events are processed sequentially, both events should be handled
        handler.CallCount.Should().Be(2);
    }
}
