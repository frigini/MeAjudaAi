using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

[Trait("Category", "Unit")]
public class MessageRetryMiddlewareFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<IDeadLetterService> _deadLetterMock = new();
    private readonly Mock<ILogger<MessageRetryMiddleware<string>>> _loggerMock = new();
    private readonly MessageRetryMiddlewareFactory _sut;

    public MessageRetryMiddlewareFactoryTests()
    {
        _serviceProviderMock.Setup(s => s.GetService(typeof(IDeadLetterService)))
            .Returns(_deadLetterMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(ILogger<MessageRetryMiddleware<string>>)))
            .Returns(_loggerMock.Object);
            
        _sut = new MessageRetryMiddlewareFactory(_serviceProviderMock.Object);
    }

    [Fact]
    public void CreateMiddleware_ShouldReturnValidInstance()
    {
        // Act
        var result = _sut.CreateMiddleware<string>("TestHandler", "test-queue");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<MessageRetryMiddleware<string>>();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldCallNext_WhenSuccessful()
    {
        // Arrange
        var middleware = _sut.CreateMiddleware<string>("TestHandler", "test-queue");
        var called = false;
        Task Handler(string msg, CancellationToken ct) 
        {
            called = true;
            return Task.CompletedTask;
        }

        // Act
        await middleware.ExecuteWithRetryAsync("message", Handler);

        // Assert
        called.Should().BeTrue();
    }
}
