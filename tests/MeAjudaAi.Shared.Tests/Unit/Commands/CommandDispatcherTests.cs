using FluentAssertions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FunctionalUnit = MeAjudaAi.Shared.Functional.Unit;

namespace MeAjudaAi.Shared.Tests.UnitTests.Commands;

public class CommandDispatcherTests : IDisposable
{
    private readonly Mock<ILogger<CommandDispatcher>> _loggerMock;
    private readonly ServiceCollection _services;
    private ServiceProvider? _serviceProvider;

    public CommandDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<CommandDispatcher>>();
        _services = new ServiceCollection();
        _services.AddSingleton(_loggerMock.Object);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Test commands
    public record TestCommand(Guid CorrelationId, string Data) : ICommand;
    public record TestCommandWithResult(Guid CorrelationId, string Data) : ICommand<string>;

    // Test handlers
    private class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public bool WasCalled { get; private set; }
        public TestCommand? ReceivedCommand { get; private set; }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCommand = command;
            return Task.CompletedTask;
        }
    }

    private class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, string>
    {
        public bool WasCalled { get; private set; }
        public TestCommandWithResult? ReceivedCommand { get; private set; }

        public Task<string> HandleAsync(TestCommandWithResult command, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCommand = command;
            return Task.FromResult($"Processed: {command.Data}");
        }
    }

    // Test pipeline behaviors
    private class TestPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public bool WasCalled { get; private set; }
        public int CallOrder { get; private set; }
        private static int _globalCallCounter;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            WasCalled = true;
            CallOrder = ++_globalCallCounter;
            return await next();
        }

        public static void ResetCounter() => _globalCallCounter = 0;
    }

    [Fact]
    public async Task SendAsync_WithCommandWithoutResult_ShouldInvokeHandler()
    {
        // Arrange
        var handler = new TestCommandHandler();
        _services.AddSingleton<ICommandHandler<TestCommand>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test data");

        // Act
        await dispatcher.SendAsync(command);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.ReceivedCommand.Should().Be(command);
    }

    [Fact]
    public async Task SendAsync_WithCommandWithResult_ShouldInvokeHandlerAndReturnResult()
    {
        // Arrange
        var handler = new TestCommandWithResultHandler();
        _services.AddSingleton<ICommandHandler<TestCommandWithResult, string>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommandWithResult(Guid.NewGuid(), "test data");

        // Act
        var result = await dispatcher.SendAsync<TestCommandWithResult, string>(command);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.ReceivedCommand.Should().Be(command);
        result.Should().Be("Processed: test data");
    }

    [Fact]
    public async Task SendAsync_WithoutResult_ShouldLogCommandExecution()
    {
        // Arrange
        var handler = new TestCommandHandler();
        _services.AddSingleton<ICommandHandler<TestCommand>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var correlationId = Guid.NewGuid();
        var command = new TestCommand(correlationId, "test");

        // Act
        await dispatcher.SendAsync(command);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Executing command") &&
                    v.ToString()!.Contains("TestCommand") &&
                    v.ToString()!.Contains(correlationId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithResult_ShouldLogCommandExecution()
    {
        // Arrange
        var handler = new TestCommandWithResultHandler();
        _services.AddSingleton<ICommandHandler<TestCommandWithResult, string>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var correlationId = Guid.NewGuid();
        var command = new TestCommandWithResult(correlationId, "test");

        // Act
        await dispatcher.SendAsync<TestCommandWithResult, string>(command);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Executing command") &&
                    v.ToString()!.Contains("TestCommandWithResult") &&
                    v.ToString()!.Contains(correlationId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithPipelineBehavior_ShouldExecuteBehaviorBeforeHandler()
    {
        // Arrange
        TestPipelineBehavior<TestCommand, FunctionalUnit>.ResetCounter();
        var handler = new TestCommandHandler();
        var behavior = new TestPipelineBehavior<TestCommand, FunctionalUnit>();

        _services.AddSingleton<ICommandHandler<TestCommand>>(handler);
        _services.AddSingleton<IPipelineBehavior<TestCommand, FunctionalUnit>>(behavior);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test");

        // Act
        await dispatcher.SendAsync(command);

        // Assert
        behavior.WasCalled.Should().BeTrue();
        handler.WasCalled.Should().BeTrue();
        behavior.CallOrder.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_WithMultiplePipelineBehaviors_ShouldExecuteInReverseRegistrationOrder()
    {
        // Arrange
        TestPipelineBehavior<TestCommand, FunctionalUnit>.ResetCounter();
        var handler = new TestCommandHandler();
        var behavior1 = new TestPipelineBehavior<TestCommand, FunctionalUnit>();
        var behavior2 = new TestPipelineBehavior<TestCommand, FunctionalUnit>();
        var behavior3 = new TestPipelineBehavior<TestCommand, FunctionalUnit>();

        _services.AddSingleton<ICommandHandler<TestCommand>>(handler);
        _services.AddSingleton<IPipelineBehavior<TestCommand, FunctionalUnit>>(behavior1);
        _services.AddSingleton<IPipelineBehavior<TestCommand, FunctionalUnit>>(behavior2);
        _services.AddSingleton<IPipelineBehavior<TestCommand, FunctionalUnit>>(behavior3);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test");

        // Act
        await dispatcher.SendAsync(command);

        // Assert
        behavior1.WasCalled.Should().BeTrue();
        behavior2.WasCalled.Should().BeTrue();
        behavior3.WasCalled.Should().BeTrue();
        handler.WasCalled.Should().BeTrue();

        // Behaviors execute in FIFO order after Reverse() call in dispatcher (so 1, 2, 3)
        behavior1.CallOrder.Should().BeLessThan(behavior2.CallOrder);
        behavior2.CallOrder.Should().BeLessThan(behavior3.CallOrder);
    }

    [Fact]
    public async Task SendAsync_WithPipelineBehaviorForResultCommand_ShouldExecuteBehaviorAndReturnResult()
    {
        // Arrange
        TestPipelineBehavior<TestCommandWithResult, string>.ResetCounter();
        var handler = new TestCommandWithResultHandler();
        var behavior = new TestPipelineBehavior<TestCommandWithResult, string>();

        _services.AddSingleton<ICommandHandler<TestCommandWithResult, string>>(handler);
        _services.AddSingleton<IPipelineBehavior<TestCommandWithResult, string>>(behavior);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommandWithResult(Guid.NewGuid(), "test");

        // Act
        var result = await dispatcher.SendAsync<TestCommandWithResult, string>(command);

        // Assert
        behavior.WasCalled.Should().BeTrue();
        handler.WasCalled.Should().BeTrue();
        result.Should().Be("Processed: test");
    }

    [Fact]
    public async Task SendAsync_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var handlerMock = new Mock<ICommandHandler<TestCommand>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync(command));
    }

    [Fact]
    public async Task SendAsync_WithResult_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var handlerMock = new Mock<ICommandHandler<TestCommandWithResult, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestCommandWithResult>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommandWithResult(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.SendAsync<TestCommandWithResult, string>(command));
    }

    [Fact]
    public async Task SendAsync_WhenHandlerNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _serviceProvider = _services.BuildServiceProvider();
        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync(command));
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        var handlerMock = new Mock<ICommandHandler<TestCommand>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<TestCommand, CancellationToken>((_, ct) => receivedToken = ct)
            .Returns(Task.CompletedTask);

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test");

        // Act
        await dispatcher.SendAsync(command, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task SendAsync_WithResult_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        var handlerMock = new Mock<ICommandHandler<TestCommandWithResult, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestCommandWithResult>(), It.IsAny<CancellationToken>()))
            .Callback<TestCommandWithResult, CancellationToken>((_, ct) => receivedToken = ct)
            .ReturnsAsync("result");

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommandWithResult(Guid.NewGuid(), "test");

        // Act
        await dispatcher.SendAsync<TestCommandWithResult, string>(command, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task SendAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handlerMock = new Mock<ICommandHandler<TestCommand>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(_serviceProvider, _loggerMock.Object);
        var command = new TestCommand(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dispatcher.SendAsync(command, cts.Token));
    }
}
