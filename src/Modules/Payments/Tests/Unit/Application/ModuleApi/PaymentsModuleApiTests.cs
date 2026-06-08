using FluentAssertions;
using MeAjudaAi.Modules.Payments.Application.ModuleApi;
using MeAjudaAi.Modules.Payments.Application.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
        result.Error.Message.Should().Be("Error retrieving subscription data.");
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
}
