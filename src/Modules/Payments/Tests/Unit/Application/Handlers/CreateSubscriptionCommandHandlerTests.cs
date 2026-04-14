using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
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
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "plan_123", 99.90m);
        var gatewayResult = new SubscriptionGatewayResult(true, null, "https://checkout.stripe.com/abc", null);

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(gatewayResult.CheckoutUrl);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Subscription>(s => 
            s.ProviderId == command.ProviderId && 
            s.PlanId == command.PlanId &&
            s.Amount.Amount == command.Amount), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenGatewayFails()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "plan_123", 99.90m);
        var gatewayResult = new SubscriptionGatewayResult(false, null, null, "Error from gateway");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*Failed to create subscription*");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
