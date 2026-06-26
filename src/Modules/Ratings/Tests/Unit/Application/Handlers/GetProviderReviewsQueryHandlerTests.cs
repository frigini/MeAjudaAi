using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "Application")]
public class GetProviderReviewsQueryHandlerTests
{
    private readonly Mock<IReviewQueries> _queriesMock;
    private readonly Mock<ILogger<GetProviderReviewsQueryHandler>> _loggerMock;
    private readonly GetProviderReviewsQueryHandler _handler;

    public GetProviderReviewsQueryHandlerTests()
    {
        _queriesMock = new Mock<IReviewQueries>();
        _loggerMock = new Mock<ILogger<GetProviderReviewsQueryHandler>>();
        _handler = new GetProviderReviewsQueryHandler(_queriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithReviews_ShouldReturnSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            CreateApprovedReview(providerId, 5, "Excellent!"),
            CreateApprovedReview(providerId, 4, "Very good")
        };

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task HandleAsync_NoReviews_ShouldReturnEmptyList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviews = new List<Review>();

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithPagination_ShouldPassCorrectValues()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            CreateApprovedReview(providerId, 5, "Great!")
        };

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        var query = new GetProviderReviewsQuery(providerId, 2, 5, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapReviewFieldsCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var review = CreateApprovedReview(providerId, 3, "Average service");

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Review> { review });

        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Items.First();
        dto.Rating.Should().Be(3);
        dto.Comment.Should().Be("Average service");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private static Review CreateApprovedReview(Guid providerId, int rating, string? comment)
    {
        var review = Review.Create(providerId, Guid.NewGuid(), rating, comment);
        review.Approve();
        return review;
    }
}
