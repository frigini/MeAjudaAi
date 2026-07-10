using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Ratings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Handlers.Queries;

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

        _queriesMock.Setup(q => q.GetTotalApprovedCountByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_NoReviews_ShouldReturnEmptyList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviews = new List<Review>();

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        _queriesMock.Setup(q => q.GetTotalApprovedCountByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
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

        _queriesMock.Setup(q => q.GetTotalApprovedCountByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(8);

        var query = new GetProviderReviewsQuery(providerId, 2, 5, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(8);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapReviewFieldsCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var review = CreateApprovedReview(providerId, 3, "Average service");

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Review> { review });

        _queriesMock.Setup(q => q.GetTotalApprovedCountByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

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

    [Fact]
    public async Task HandleAsync_ShouldReturnCorrectTotalItems()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            CreateApprovedReview(providerId, 5, "Excellent"),
            CreateApprovedReview(providerId, 4, "Good")
        };

        _queriesMock.Setup(q => q.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        _queriesMock.Setup(q => q.GetTotalApprovedCountByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalItems.Should().Be(15,
            "TotalItems should reflect the total count from GetTotalApprovedCountByProviderIdAsync");
        result.Value.Items.Should().HaveCount(2,
            "Items should contain only the page results");
    }

    private static Review CreateApprovedReview(Guid providerId, int rating, string? comment)
    {
        return new ReviewBuilder()
            .WithProviderId(providerId)
            .WithRating(rating)
            .WithComment(comment)
            .AsApproved()
            .Build();
    }
}
