using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Moq;
using FluentAssertions;
using Xunit;

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
    public async Task HandleAsync_ValidCommand_ShouldCreateSubscriptionAndAddInRepository()
    {
        // Arrange
        var expectedProviderId = Guid.NewGuid();
        var expectedPlanId = "price_premium_monthly";
        var expectedAmount = 99.90m;
        var expectedCurrency = "BRL";

        var command = new CreateSubscriptionCommand(expectedProviderId, expectedPlanId);
        var gatewayResult = new SubscriptionGatewayResult(true, "sub_123", "https://checkout.stripe.com/xyz", null);

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.Is<Guid>(p => p == expectedProviderId),
            It.Is<string>(p => p == expectedPlanId),
            It.Is<Money>(m => m.Amount == expectedAmount && m.Currency == expectedCurrency),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(gatewayResult.CheckoutUrl);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Subscription>(s =>
            s.ProviderId == expectedProviderId &&
            s.PlanId == expectedPlanId &&
            s.Amount.Amount == expectedAmount &&
            s.Amount.Currency == expectedCurrency), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenGatewayFails_ShouldThrowSubscriptionCreationException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        var gatewayResult = new SubscriptionGatewayResult(false, null, null, "Gateway error");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Gateway error*");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowSubscriptionCreationException_WhenCheckoutUrlIsMissing()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
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
    public async Task HandleAsync_ShouldThrowSubscriptionCreationException_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        var exception = await act.Should().ThrowAsync<SubscriptionCreationException>();
        exception.WithInnerException<Exception>().WithMessage("Unexpected error");
    }

    [Fact]
    public async Task HandleAsync_ShouldRollbackGateway_WhenRepositoryFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand(providerId, "price_premium_monthly");
        var gatewayResult = new SubscriptionGatewayResult(true, "sub_rollback", "https://checkout", null);

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _repositoryMock.Setup(r => r.AddAsync(It.Is<Subscription>(s => s.ProviderId == providerId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Operação revertida no gateway*");
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_rollback", It.IsAny<CancellationToken>()), Times.Once);
    }
}
