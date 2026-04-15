using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure.Persistence.Repositories;

public class ReviewRepositoryContractTests
{
    private readonly Mock<IReviewRepository> _repositoryMock;

    public ReviewRepositoryContractTests()
    {
        _repositoryMock = new Mock<IReviewRepository>();
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Great service");

        // Act
        await _repositoryMock.Object.AddAsync(review);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Review>(rev =>
            rev.Rating == 5 && rev.Comment == "Great service"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReview_WhenExists()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 4, "Good");
        _repositoryMock.Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _repositoryMock.Object.GetByIdAsync(review.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
        result.Rating.Should().Be(4);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = new ReviewId(Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        // Act
        var result = await _repositoryMock.Object.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderIdAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            Review.Create(providerId, Guid.NewGuid(), 5, "Excellent"),
            Review.Create(providerId, Guid.NewGuid(), 4, "Good")
        };

        _repositoryMock.Setup(r => r.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        // Act
        var result = await _repositoryMock.Object.GetByProviderIdAsync(providerId, 1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.ProviderId.Should().Be(providerId));
    }

    [Fact]
    public async Task GetByProviderIdAsync_ShouldReturnEmpty_WhenNoReviews()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByProviderIdAsync(providerId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Review>());

        // Act
        var result = await _repositoryMock.Object.GetByProviderIdAsync(providerId, 1, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 3, "Average");

        // Act
        await _repositoryMock.Object.UpdateAsync(review);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(review, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByProviderAndCustomerAsync_ShouldReturnReview_WhenExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review = Review.Create(providerId, customerId, 4, "Nice work");

        _repositoryMock.Setup(r => r.GetByProviderAndCustomerAsync(providerId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _repositoryMock.Object.GetByProviderAndCustomerAsync(providerId, customerId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
        result.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task GetByProviderAndCustomerAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByProviderAndCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        // Act
        var result = await _repositoryMock.Object.GetByProviderAndCustomerAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_ShouldReturnCorrectValues()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((4.25m, 8));

        // Act
        var (average, total) = await _repositoryMock.Object.GetAverageRatingForProviderAsync(providerId);

        // Assert
        average.Should().Be(4.25m);
        total.Should().Be(8);
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_ShouldReturnZeros_WhenNoReviews()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0m, 0));

        // Act
        var (average, total) = await _repositoryMock.Object.GetAverageRatingForProviderAsync(providerId);

        // Assert
        average.Should().Be(0m);
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetByProviderIdAsync_ShouldRespectPagination()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var page2Reviews = new List<Review>
        {
            Review.Create(providerId, Guid.NewGuid(), 3, "Page 2 item")
        };

        _repositoryMock.Setup(r => r.GetByProviderIdAsync(providerId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page2Reviews);

        // Act
        var result = await _repositoryMock.Object.GetByProviderIdAsync(providerId, 2, 5);

        // Assert
        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetByProviderIdAsync(providerId, 2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
