using FluentAssertions;
using MeAjudaAi.Modules.Ratings.Application.ModuleApi;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
    public async Task GetProviderRatingAsync_WhenQueryThrows_ShouldReturnFailure()
    {
        // Arrange
        _reviewQueriesMock
            .Setup(x => x.GetAverageRatingForProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        // Act
        var result = await _sut.GetProviderRatingAsync(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error retrieving rating data.");
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_WhenQueryThrows_ShouldReturnFailure()
    {
        // Arrange
        _reviewQueriesMock
            .Setup(x => x.GetByProviderAndCustomerAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        // Act
        var result = await _sut.HasCustomerReviewedProviderAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
