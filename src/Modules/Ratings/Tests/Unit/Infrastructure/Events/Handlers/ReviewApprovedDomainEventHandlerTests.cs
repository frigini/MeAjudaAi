using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure.Events.Handlers;

public class ReviewApprovedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<IReviewRepository> _repositoryMock;
    private readonly Mock<ILogger<ReviewApprovedDomainEventHandler>> _loggerMock;
    private readonly ReviewApprovedDomainEventHandler _handler;

    public ReviewApprovedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _repositoryMock = new Mock<IReviewRepository>();
        _loggerMock = new Mock<ILogger<ReviewApprovedDomainEventHandler>>();
        _handler = new ReviewApprovedDomainEventHandler(
            _messageBusMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent_WithCorrectData()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var rating = 5;
        var comment = "Excellent service!";
        var domainEvent = new ReviewApprovedDomainEvent(reviewId, 0, providerId, rating, comment);

        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((4.5m, 10));

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<ReviewApprovedIntegrationEvent>(e =>
                e.ProviderId == providerId &&
                e.ReviewId == reviewId &&
                e.NewAverageRating == 4.5m &&
                e.TotalReviews == 10 &&
                e.ReviewRating == rating &&
                e.ReviewComment == comment &&
                e.Source == "Ratings"),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCalculateAverageRating_BeforePublishing()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ReviewApprovedDomainEvent(Guid.NewGuid(), 0, providerId, 3, null);
        var seq = new MockSequence();

        _repositoryMock.InSequence(seq).Setup(r => r.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((3.75m, 4));

        _messageBusMock.InSequence(seq).Setup(m => m.PublishAsync(
            It.IsAny<ReviewApprovedIntegrationEvent>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _repositoryMock.Verify(r => r.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<ReviewApprovedIntegrationEvent>(e => e.NewAverageRating == 3.75m && e.TotalReviews == 4),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNullComment()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ReviewApprovedDomainEvent(Guid.NewGuid(), 0, providerId, 4, null);

        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((4.0m, 1));

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<ReviewApprovedIntegrationEvent>(e => e.ReviewComment == null),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRethrow_WhenRepositoryFails()
    {
        // Arrange
        var domainEvent = new ReviewApprovedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), 5, "Great");

        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
        _messageBusMock.Verify(m => m.PublishAsync(
            It.IsAny<ReviewApprovedIntegrationEvent>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldRethrow_WhenMessageBusFails()
    {
        // Arrange
        var domainEvent = new ReviewApprovedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), 5, "Great");

        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((5.0m, 1));

        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<ReviewApprovedIntegrationEvent>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus unavailable"));

        // Act
        var act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Message bus unavailable");
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var domainEvent = new ReviewApprovedDomainEvent(Guid.NewGuid(), 0, Guid.NewGuid(), 4, "Good");

        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), cts.Token))
            .ReturnsAsync((4.0m, 5));

        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<ReviewApprovedIntegrationEvent>(), It.IsAny<string?>(), cts.Token))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        _repositoryMock.Verify(r => r.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), cts.Token), Times.Once);
        _messageBusMock.Verify(m => m.PublishAsync(It.IsAny<ReviewApprovedIntegrationEvent>(), It.IsAny<string?>(), cts.Token), Times.Once);
    }
}
