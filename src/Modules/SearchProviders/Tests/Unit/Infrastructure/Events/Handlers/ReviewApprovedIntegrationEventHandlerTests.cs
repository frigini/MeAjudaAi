using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Infrastructure")]
public class ReviewApprovedIntegrationEventHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ISearchableProviderQueries> _queriesMock;
    private readonly Mock<ILogger<ReviewApprovedIntegrationEventHandler>> _loggerMock;
    private readonly ReviewApprovedIntegrationEventHandler _handler;

    public ReviewApprovedIntegrationEventHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _queriesMock = new Mock<ISearchableProviderQueries>();
        _loggerMock = new Mock<ILogger<ReviewApprovedIntegrationEventHandler>>();
        _handler = new ReviewApprovedIntegrationEventHandler(_uowMock.Object, _queriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenReviewApproved_ShouldUpdateProviderRating()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var newRating = 4.8m;
        var totalReviews = 15;
        var integrationEvent = new ReviewApprovedIntegrationEvent(
            Source: "Ratings",
            ProviderId: providerId,
            ReviewId: Guid.NewGuid(),
            NewAverageRating: newRating,
            TotalReviews: totalReviews,
            ReviewRating: 5,
            ReviewComment: "Excellent!",
            CreatedAt: DateTime.UtcNow);

        var provider = SearchableProvider.Create(
            providerId, "Provider 1", "p1", new GeoPoint(0, 0));
        provider.UpdateRating(4.5m, 14);

        _queriesMock.Setup(x => x.GetByProviderIdAsync(providerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        provider.AverageRating.Should().Be(newRating);
        provider.TotalReviews.Should().Be(totalReviews);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldLogWarningAndReturn()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent(
            Source: "Ratings",
            ProviderId: providerId,
            ReviewId: Guid.NewGuid(),
            NewAverageRating: 4.8m,
            TotalReviews: 15,
            ReviewRating: 5,
            ReviewComment: "Excellent!",
            CreatedAt: DateTime.UtcNow);

        _queriesMock.Setup(x => x.GetByProviderIdAsync(providerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchableProvider?)null);

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ReviewApprovedIntegrationEvent(
            Source: "Ratings",
            ProviderId: providerId,
            ReviewId: Guid.NewGuid(),
            NewAverageRating: 4.8m,
            TotalReviews: 15,
            ReviewRating: 5,
            ReviewComment: "Excellent!",
            CreatedAt: DateTime.UtcNow);

        _queriesMock.Setup(x => x.GetByProviderIdAsync(providerId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(integrationEvent));
    }
}
