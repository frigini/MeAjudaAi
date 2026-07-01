using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "Application")]
public class GetReviewByIdQueryHandlerTests
{
    private readonly Mock<IReviewQueries> _queriesMock;
    private readonly Mock<ILogger<GetReviewByIdQueryHandler>> _loggerMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly GetReviewByIdQueryHandler _handler;

    public GetReviewByIdQueryHandlerTests()
    {
        _queriesMock = new Mock<IReviewQueries>();
        _loggerMock = new Mock<ILogger<GetReviewByIdQueryHandler>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();

        _localizerMock
            .Setup(x => x[It.Is<string>(s => s == "ReviewNotFound")])
            .Returns(new LocalizedString("ReviewNotFound", "Avaliação não encontrada."));

        _handler = new GetReviewByIdQueryHandler(_queriesMock.Object, _loggerMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ExistingApprovedReview_ShouldReturnSuccess()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Great service!");
        review.Approve();

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewByIdQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Rating.Should().Be(5);
        result.Value.Comment.Should().Be("Great service!");
    }

    [Fact]
    public async Task HandleAsync_NonExistingReview_ShouldReturnNotFound()
    {
        // Arrange
        var reviewId = ReviewId.New();
        _queriesMock.Setup(q => q.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var query = new GetReviewByIdQuery(reviewId.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_PendingReview_ShouldReturnNotFound()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Great service!");

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewByIdQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_RejectedReview_ShouldReturnNotFound()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Great service!");
        review.Reject("Inappropriate content");

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewByIdQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_FlaggedReview_ShouldReturnNotFound()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Great service!");
        review.MarkAsFlagged();

        _queriesMock.Setup(q => q.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var query = new GetReviewByIdQuery(review.Id.Value, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
    }
}
