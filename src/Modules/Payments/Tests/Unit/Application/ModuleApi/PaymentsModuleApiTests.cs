using MeAjudaAi.Modules.Payments.Application.ModuleApi;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Application")]
public class PaymentsModuleApiTests
{
    private readonly Mock<IPaymentsHealthQueries> _healthQueriesMock;
    private readonly Mock<ISubscriptionQueries> _subscriptionQueriesMock;
    private readonly PaymentsModuleApi _sut;

    public PaymentsModuleApiTests()
    {
        _healthQueriesMock = new Mock<IPaymentsHealthQueries>();
        _subscriptionQueriesMock = new Mock<ISubscriptionQueries>();
        _sut = new PaymentsModuleApi(_healthQueriesMock.Object, _subscriptionQueriesMock.Object);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckReturnsTrue_ShouldReturnTrue()
    {
        // Arrange
        _healthQueriesMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeTrue();
        _healthQueriesMock.Verify(x => x.CanConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckReturnsFalse_ShouldReturnFalse()
    {
        // Arrange
        _healthQueriesMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _healthQueriesMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.IsAvailableAsync(cts.Token));
    }
}
