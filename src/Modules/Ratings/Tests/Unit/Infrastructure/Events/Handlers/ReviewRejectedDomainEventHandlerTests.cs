using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

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

    [Theory]
    [InlineData("Spam")]
    [InlineData("Offensive Content")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_ShouldLogExpectedMessages(string? reason)
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var domainEvent = new ReviewRejectedDomainEvent(reviewId, 0, providerId, reason!);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        // Verifica o Warning (sempre deve conter o ID do review)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(reviewId.ToString())),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verifica o Debug (deve conter o motivo se presente e não for apenas whitespace)
        if (!string.IsNullOrWhiteSpace(reason))
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(reason)),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        else
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    [Fact]
    public async Task HandleAsync_ShouldTruncateLongReasonInDebugLog()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var longReason = new string('A', 150);
        var expectedPreview = longReason.Substring(0, 100) + "...";
        var domainEvent = new ReviewRejectedDomainEvent(reviewId, 0, providerId, longReason);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedPreview)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCompleteSuccessfully_WithoutCoupling()
    {
        // Arrange
        var domainEvent = new ReviewRejectedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), "Offensive");

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNullReason()
    {
        // Arrange
        var domainEvent = new ReviewRejectedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), null!);

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
