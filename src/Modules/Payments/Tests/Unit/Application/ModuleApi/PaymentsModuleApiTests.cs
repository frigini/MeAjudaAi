using MeAjudaAi.Modules.Payments.Application.ModuleApi;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Application")]
public class PaymentsModuleApiTests
{
    private readonly Mock<ISubscriptionQueries> _subscriptionQueriesMock;
    private readonly Mock<ILogger<PaymentsModuleApi>> _loggerMock;
    private readonly PaymentsModuleApi _sut;

    public PaymentsModuleApiTests()
    {
        _subscriptionQueriesMock = new Mock<ISubscriptionQueries>();
        _loggerMock = new Mock<ILogger<PaymentsModuleApi>>();
        _sut = new PaymentsModuleApi(_subscriptionQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetActiveSubscriptionByProviderIdAsync_WhenQueryThrows_ShouldReturnFailure()
    {
        // Arrange
        _subscriptionQueriesMock
            .Setup(x => x.GetActiveByProviderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        // Act
        var result = await _sut.GetActiveSubscriptionByProviderIdAsync(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Error retrieving subscription data.");
    }

    [Fact]
    public async Task HasActiveSubscriptionAsync_WhenQueryThrows_ShouldReturnFailure()
    {
        // Arrange
        _subscriptionQueriesMock
            .Setup(x => x.GetActiveByProviderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        // Act
        var result = await _sut.HasActiveSubscriptionAsync(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenRepositoryResponds_ShouldReturnTrue()
    {
        // Arrange
        _subscriptionQueriesMock
            .Setup(x => x.GetActiveByProviderIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Payments.Domain.Entities.Subscription?)null);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
        _subscriptionQueriesMock.Verify(x => x.GetActiveByProviderIdAsync(Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenRepositoryThrows_ShouldReturnFalse()
    {
        // Arrange
        _subscriptionQueriesMock
            .Setup(x => x.GetActiveByProviderIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _subscriptionQueriesMock
            .Setup(x => x.GetActiveByProviderIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.IsAvailableAsync(cts.Token));
    }
}
