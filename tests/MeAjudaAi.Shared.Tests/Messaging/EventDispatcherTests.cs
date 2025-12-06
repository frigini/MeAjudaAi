using FluentAssertions;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Messaging;

public class EventDispatcherTests
{
    private readonly Mock<ILogger<EventDispatcher>> _mockLogger;
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcherTests()
    {
        _mockLogger = new Mock<ILogger<EventDispatcher>>();
        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task PublishAsync_ShouldInvokeAllHandlers_WhenMultipleHandlersRegistered()
    {
        // Arrange
        var handler1 = new Mock<IEventHandler<TestEvent>>();
        var handler2 = new Mock<IEventHandler<TestEvent>>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton(handler1.Object);
        services.AddSingleton(handler2.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new EventDispatcher(provider, _mockLogger.Object);
        var @event = new TestEvent();

        // Act
        await dispatcher.PublishAsync(@event);

        // Assert
        handler1.Verify(x => x.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(x => x.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldNotThrow_WhenNoHandlersRegistered()
    {
        // Arrange
        var dispatcher = new EventDispatcher(_serviceProvider, _mockLogger.Object);
        var @event = new TestEvent();

        // Act
        var act = async () => await dispatcher.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldContinueWithOtherHandlers_WhenOneHandlerFails()
    {
        // Arrange
        var handler1 = new Mock<IEventHandler<TestEvent>>();
        var handler2 = new Mock<IEventHandler<TestEvent>>();

        handler1.Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler 1 failed"));

        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton(handler1.Object);
        services.AddSingleton(handler2.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new EventDispatcher(provider, _mockLogger.Object);
        var @event = new TestEvent();

        // Act
        await dispatcher.PublishAsync(@event);

        // Assert
        handler2.Verify(x => x.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldLogError_WhenHandlerThrowsException()
    {
        // Arrange
        var handler = new Mock<IEventHandler<TestEvent>>();
        var exception = new InvalidOperationException("Test exception");

        handler.Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton(handler.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new EventDispatcher(provider, _mockLogger.Object);
        var @event = new TestEvent();

        // Act
        await dispatcher.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling event")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var handler = new Mock<IEventHandler<TestEvent>>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton(handler.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new EventDispatcher(provider, _mockLogger.Object);
        var events = new[] { new TestEvent(), new TestEvent(), new TestEvent() };

        // Act
        await dispatcher.PublishAsync(events);

        // Assert
        handler.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task PublishAsync_WithMultipleEvents_ShouldNotThrow_WhenCollectionIsEmpty()
    {
        // Arrange
        var dispatcher = new EventDispatcher(_serviceProvider, _mockLogger.Object);
        var events = Array.Empty<TestEvent>();

        // Act
        var act = async () => await dispatcher.PublishAsync(events);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldPassCancellationToken_ToHandlers()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var handler = new Mock<IEventHandler<TestEvent>>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton(handler.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new EventDispatcher(provider, _mockLogger.Object);
        var @event = new TestEvent();

        // Act
        await dispatcher.PublishAsync(@event, cts.Token);

        // Assert
        handler.Verify(x => x.HandleAsync(@event, cts.Token), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleEvents_ShouldHandlePartialFailures()
    {
        // Arrange
        var handler = new Mock<IEventHandler<TestEvent>>();
        var callCount = 0;

        handler.Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 2)
                    throw new InvalidOperationException("Second event failed");
                return Task.CompletedTask;
            });

        var services = new ServiceCollection();
        services.AddSingleton(_mockLogger.Object);
        services.AddSingleton(handler.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new EventDispatcher(provider, _mockLogger.Object);
        var events = new[] { new TestEvent(), new TestEvent(), new TestEvent() };

        // Act
        await dispatcher.PublishAsync(events);

        // Assert
        handler.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public class TestEvent : IEvent
    {
        public string EventType => "TestEvent";
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
