using FluentAssertions;
using MeAjudaAi.Shared.Mediator;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.UnitTests.Queries;

public class QueryDispatcherTests : IDisposable
{
    private readonly Mock<ILogger<QueryDispatcher>> _loggerMock;
    private readonly ServiceCollection _services;
    private ServiceProvider? _serviceProvider;

    public QueryDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<QueryDispatcher>>();
        _services = new ServiceCollection();
        _services.AddSingleton(_loggerMock.Object);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Test queries
    public record TestQuery(Guid CorrelationId, string Data) : IQuery<string>;
    public record TestIntQuery(Guid CorrelationId, int Value) : IQuery<int>;

    // Test handlers
    private class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public bool WasCalled { get; private set; }
        public TestQuery? ReceivedQuery { get; private set; }

        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedQuery = query;
            return Task.FromResult($"Result: {query.Data}");
        }
    }

    private class TestIntQueryHandler : IQueryHandler<TestIntQuery, int>
    {
        public bool WasCalled { get; private set; }
        public TestIntQuery? ReceivedQuery { get; private set; }

        public Task<int> HandleAsync(TestIntQuery query, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedQuery = query;
            return Task.FromResult(query.Value * 2);
        }
    }

    // Test pipeline behavior
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
    public async Task QueryAsync_WithQuery_ShouldInvokeHandler()
    {
        // Arrange
        var handler = new TestQueryHandler();
        _services.AddSingleton<IQueryHandler<TestQuery, string>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test data");

        // Act
        var result = await dispatcher.QueryAsync<TestQuery, string>(query);

        // Assert
        handler.WasCalled.Should().BeTrue();
        handler.ReceivedQuery.Should().Be(query);
        result.Should().Be("Result: test data");
    }

    [Fact]
    public async Task QueryAsync_ShouldLogQueryExecution()
    {
        // Arrange
        var handler = new TestQueryHandler();
        _services.AddSingleton<IQueryHandler<TestQuery, string>>(handler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var correlationId = Guid.NewGuid();
        var query = new TestQuery(correlationId, "test");

        // Act
        await dispatcher.QueryAsync<TestQuery, string>(query);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Executing query") &&
                    v.ToString()!.Contains("TestQuery") &&
                    v.ToString()!.Contains(correlationId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task QueryAsync_WithPipelineBehavior_ShouldExecuteBehaviorBeforeHandler()
    {
        // Arrange
        TestPipelineBehavior<TestQuery, string>.ResetCounter();
        var handler = new TestQueryHandler();
        var behavior = new TestPipelineBehavior<TestQuery, string>();

        _services.AddSingleton<IQueryHandler<TestQuery, string>>(handler);
        _services.AddSingleton<IPipelineBehavior<TestQuery, string>>(behavior);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test");

        // Act
        await dispatcher.QueryAsync<TestQuery, string>(query);

        // Assert
        behavior.WasCalled.Should().BeTrue();
        handler.WasCalled.Should().BeTrue();
        behavior.CallOrder.Should().Be(1);
    }

    [Fact]
    public async Task QueryAsync_WithMultiplePipelineBehaviors_ShouldExecuteInRegistrationOrder()
    {
        // Arrange
        TestPipelineBehavior<TestQuery, string>.ResetCounter();
        var handler = new TestQueryHandler();
        var behavior1 = new TestPipelineBehavior<TestQuery, string>();
        var behavior2 = new TestPipelineBehavior<TestQuery, string>();
        var behavior3 = new TestPipelineBehavior<TestQuery, string>();

        _services.AddSingleton<IQueryHandler<TestQuery, string>>(handler);
        _services.AddSingleton<IPipelineBehavior<TestQuery, string>>(behavior1);
        _services.AddSingleton<IPipelineBehavior<TestQuery, string>>(behavior2);
        _services.AddSingleton<IPipelineBehavior<TestQuery, string>>(behavior3);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test");

        // Act
        await dispatcher.QueryAsync<TestQuery, string>(query);

        // Assert
        behavior1.WasCalled.Should().BeTrue();
        behavior2.WasCalled.Should().BeTrue();
        behavior3.WasCalled.Should().BeTrue();
        handler.WasCalled.Should().BeTrue();

        // Behaviors execute in FIFO order after Reverse() (1, 2, 3)
        behavior1.CallOrder.Should().BeLessThan(behavior2.CallOrder);
        behavior2.CallOrder.Should().BeLessThan(behavior3.CallOrder);
    }

    [Fact]
    public async Task QueryAsync_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.QueryAsync<TestQuery, string>(query));
    }

    [Fact]
    public async Task QueryAsync_WhenHandlerNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _serviceProvider = _services.BuildServiceProvider();
        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.QueryAsync<TestQuery, string>(query));
    }

    [Fact]
    public async Task QueryAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .Callback<TestQuery, CancellationToken>((_, ct) => receivedToken = ct)
            .ReturnsAsync("result");

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test");

        // Act
        await dispatcher.QueryAsync<TestQuery, string>(query, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task QueryAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _services.AddSingleton(handlerMock.Object);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var query = new TestQuery(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            dispatcher.QueryAsync<TestQuery, string>(query, cts.Token));
    }

    [Fact]
    public async Task QueryAsync_WithDifferentQueryTypes_ShouldInvokeCorrectHandlers()
    {
        // Arrange
        var stringHandler = new TestQueryHandler();
        var intHandler = new TestIntQueryHandler();

        _services.AddSingleton<IQueryHandler<TestQuery, string>>(stringHandler);
        _services.AddSingleton<IQueryHandler<TestIntQuery, int>>(intHandler);
        _serviceProvider = _services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(_serviceProvider, _loggerMock.Object);
        var stringQuery = new TestQuery(Guid.NewGuid(), "test");
        var intQuery = new TestIntQuery(Guid.NewGuid(), 42);

        // Act
        var stringResult = await dispatcher.QueryAsync<TestQuery, string>(stringQuery);
        var intResult = await dispatcher.QueryAsync<TestIntQuery, int>(intQuery);

        // Assert
        stringHandler.WasCalled.Should().BeTrue();
        stringHandler.ReceivedQuery.Should().Be(stringQuery);
        stringResult.Should().Be("Result: test");

        intHandler.WasCalled.Should().BeTrue();
        intHandler.ReceivedQuery.Should().Be(intQuery);
        intResult.Should().Be(84); // 42 * 2
    }
}
