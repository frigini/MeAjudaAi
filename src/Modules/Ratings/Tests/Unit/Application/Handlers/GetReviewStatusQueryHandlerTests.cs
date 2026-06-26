using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using ContractsEnumEReviewStatus = MeAjudaAi.Contracts.Modules.Ratings.Enums.EReviewStatus;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "Application")]
public class GetReviewStatusQueryHandlerTests
{
    private readonly Mock<IReviewQueries> _queriesMock;
    private readonly Mock<ILogger<GetReviewStatusQueryHandler>> _loggerMock;
    private readonly GetReviewStatusQueryHandler _handler;

    public GetReviewStatusQueryHandlerTests()
    {
        _queriesMock = new Mock<IReviewQueries>();
        _loggerMock = new Mock<ILogger<GetReviewStatusQueryHandler>>();
        _handler = new GetReviewStatusQueryHandler(_queriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_PendingReview_ShouldReturnPendingStatus()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewStatusQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(ContractsEnumEReviewStatus.Pending);
    }

    [Fact]
    public async Task HandleAsync_ApprovedReview_ShouldReturnApprovedStatus()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        review.Approve();

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewStatusQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(ContractsEnumEReviewStatus.Approved);
    }

    [Fact]
    public async Task HandleAsync_RejectedReview_ShouldReturnRejectedStatus()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        review.Reject("Inappropriate");

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewStatusQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(ContractsEnumEReviewStatus.Rejected);
    }

    [Fact]
    public async Task HandleAsync_FlaggedReview_ShouldReturnFlaggedStatus()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        review.MarkAsFlagged();

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewStatusQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(ContractsEnumEReviewStatus.Flagged);
    }

    [Fact]
    public async Task HandleAsync_NonExistingReview_ShouldReturnNotFound()
    {
        // Arrange
        var reviewId = ReviewId.New();
        _queriesMock.Setup(q => q.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var query = new GetReviewStatusQuery(reviewId.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCorrectId()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        review.Approve();

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewStatusQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(review.Id.Value);
    }
}
