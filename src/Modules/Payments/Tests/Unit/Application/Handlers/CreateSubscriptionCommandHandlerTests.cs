using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Moq;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

public class CreateSubscriptionCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly CreateSubscriptionCommandHandler _handler;

    public CreateSubscriptionCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _handler = new CreateSubscriptionCommandHandler(_repositoryMock.Object, _gatewayMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateSubscriptionAndReturnCheckoutUrl()
    {
        // Arrange
        var expectedProviderId = Guid.NewGuid();
        var expectedPlanId = "plan_123";
        var expectedAmount = 99.90m;
        var expectedCurrency = "BRL";
        var command = new CreateSubscriptionCommand(expectedProviderId, expectedPlanId, expectedAmount, expectedCurrency);
        var gatewayResult = new SubscriptionGatewayResult(true, null, "https://checkout.stripe.com/abc", null);

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.Is<Guid>(id => id == expectedProviderId),
            It.Is<string>(p => p == expectedPlanId),
            It.Is<Money>(m => m.Amount == expectedAmount && m.Currency == expectedCurrency),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(gatewayResult.CheckoutUrl);
        _gatewayMock.Verify(g => g.CreateSubscriptionAsync(
            It.Is<Guid>(id => id == expectedProviderId),
            It.Is<string>(p => p == expectedPlanId),
            It.Is<Money>(m => m.Amount == expectedAmount && m.Currency == expectedCurrency),
            It.IsAny<CancellationToken>()), Times.Once);

        _repositoryMock.Verify(r => r.AddAsync(It.Is<Subscription>(s => 
            s.ProviderId == expectedProviderId && 
            s.PlanId == expectedPlanId &&
            s.Amount.Amount == expectedAmount &&
            s.Amount.Currency == expectedCurrency), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowSubscriptionCreationException_WhenGatewayFails()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "plan_123", 99.90m, "BRL");
        var gatewayResult = new SubscriptionGatewayResult(false, null, null, "Error from gateway");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowSubscriptionCreationException_WhenCheckoutUrlIsMissing()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "plan_123", 99.90m, "BRL");
        var gatewayResult = new SubscriptionGatewayResult(true, null, null, null);

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseBrlCurrency_WhenDefaultCurrencyUsed()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "plan_456", 149.90m);
        var gatewayResult = new SubscriptionGatewayResult(true, null, "https://checkout.stripe.com/xyz", null);

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(),
            It.Is<Money>(m => m.Currency == "BRL"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(gatewayResult.CheckoutUrl);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Subscription>(s =>
            s.Amount.Currency == "BRL"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
