using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure.Events.Handlers;

public class ReviewRejectedDomainEventHandlerTests
{
    private readonly Mock<ILogger<ReviewRejectedDomainEventHandler>> _loggerMock;
    private readonly ReviewRejectedDomainEventHandler _handler;

    public ReviewRejectedDomainEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ReviewRejectedDomainEventHandler>>();
        _handler = new ReviewRejectedDomainEventHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var domainEvent = new ReviewRejectedDomainEvent(reviewId, 0, providerId, "Inappropriate content");

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_ShouldLogWarning()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var domainEvent = new ReviewRejectedDomainEvent(reviewId, 0, providerId, "Spam");

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleLongReason_TruncatingInDebugLog()
    {
        // Arrange
        var longReason = new string('x', 200);
        var domainEvent = new ReviewRejectedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), longReason);

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().NotThrowAsync();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(longReason.Substring(0, 100) + "...")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleShortReason()
    {
        // Arrange
        var domainEvent = new ReviewRejectedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), "Bad");

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var domainEvent = new ReviewRejectedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), "Offensive");

        // Act
        var task = _handler.HandleAsync(domainEvent);

        // Assert
        task.IsCompleted.Should().BeTrue();
        await task; // Should not throw
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNullReason()
    {
        // Arrange - Reason é anulável com base no código do handler que usa ?.Length
        var domainEvent = new ReviewRejectedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), null!);

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
