using MeAjudaAi.Modules.Ratings.Application.ModuleApi;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Ratings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "Application")]
public class RatingsModuleApiTests
{
    private readonly Mock<IReviewQueries> _reviewQueriesMock;
    private readonly Mock<ILogger<RatingsModuleApi>> _loggerMock;
    private readonly RatingsModuleApi _sut;

    public RatingsModuleApiTests()
    {
        _reviewQueriesMock = new Mock<IReviewQueries>();
        _loggerMock = new Mock<ILogger<RatingsModuleApi>>();
        _sut = new RatingsModuleApi(_reviewQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProviderRatingAsync_HappyPath_ShouldReturnCorrectRating()
    {
        var providerId = Guid.NewGuid();
        _reviewQueriesMock
            .Setup(x => x.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((4.5m, 10));

        var result = await _sut.GetProviderRatingAsync(providerId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProviderId.Should().Be(providerId);
        result.Value.AverageRating.Should().Be(4.5m);
        result.Value.TotalReviews.Should().Be(10);
    }

    [Fact]
    public async Task GetProviderRatingAsync_NoReviews_ShouldReturnZeroes()
    {
        var providerId = Guid.NewGuid();
        _reviewQueriesMock
            .Setup(x => x.GetAverageRatingForProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0m, 0));

        var result = await _sut.GetProviderRatingAsync(providerId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AverageRating.Should().Be(0m);
        result.Value.TotalReviews.Should().Be(0);
    }

    [Fact]
    public async Task GetProviderRatingAsync_WhenQueryThrows_ShouldReturnFailure()
    {
        _reviewQueriesMock
            .Setup(x => x.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var result = await _sut.GetProviderRatingAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Error retrieving rating data.");
    }

    [Fact]
    public async Task GetProviderRatingAsync_WhenCancelled_ShouldThrow()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _reviewQueriesMock
            .Setup(x => x.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => _sut.GetProviderRatingAsync(Guid.NewGuid(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_ReviewExists_ShouldReturnTrue()
    {
        var customerId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var review = new ReviewBuilder().WithProviderId(providerId).WithCustomerId(customerId).WithRating(5).WithComment("Great").Build();

        _reviewQueriesMock
            .Setup(x => x.GetByProviderAndCustomerAsync(providerId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var result = await _sut.HasCustomerReviewedProviderAsync(customerId, providerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_NoReview_ShouldReturnFalse()
    {
        var customerId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        _reviewQueriesMock
            .Setup(x => x.GetByProviderAndCustomerAsync(providerId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Ratings.Domain.Entities.Review?)null);

        var result = await _sut.HasCustomerReviewedProviderAsync(customerId, providerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_WhenQueryThrows_ShouldReturnFailure()
    {
        _reviewQueriesMock
            .Setup(x => x.GetByProviderAndCustomerAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var result = await _sut.HasCustomerReviewedProviderAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Error checking review status.");
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_WhenCancelled_ShouldThrow()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _reviewQueriesMock
            .Setup(x => x.GetByProviderAndCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => _sut.HasCustomerReviewedProviderAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsAvailableAsync_CanConnect_ShouldReturnTrue()
    {
        _reviewQueriesMock
            .Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_CannotConnect_ShouldReturnFalse()
    {
        _reviewQueriesMock
            .Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.IsAvailableAsync();

        result.Should().BeFalse();
    }
}
