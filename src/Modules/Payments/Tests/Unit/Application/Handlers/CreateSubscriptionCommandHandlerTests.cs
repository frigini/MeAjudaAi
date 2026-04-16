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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

public class CreateSubscriptionCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<CreateSubscriptionCommandHandler>> _loggerMock;
    private readonly CreateSubscriptionCommandHandler _handler;

    public CreateSubscriptionCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CreateSubscriptionCommandHandler>>();
        
        SetupDefaultPlans();
        
        _handler = new CreateSubscriptionCommandHandler(
            _repositoryMock.Object, 
            _gatewayMock.Object, 
            _configurationMock.Object, 
            _loggerMock.Object);
    }

    private void SetupDefaultPlans()
    {
        _configurationMock.Setup(c => c["Payments:Plans:price_premium_monthly:Amount"]).Returns("99.90");
        _configurationMock.Setup(c => c["Payments:Plans:price_premium_monthly:Currency"]).Returns("BRL");

        _configurationMock.Setup(c => c["Payments:Plans:price_gold_monthly:Amount"]).Returns("199.90");
        _configurationMock.Setup(c => c["Payments:Plans:price_gold_monthly:Currency"]).Returns("BRL");

        // Mock para a seção (usado no check de existência)
        var premiumSection = new Mock<IConfigurationSection>();
        premiumSection.Setup(s => s.Value).Returns("exists"); 
        _configurationMock.Setup(c => c.GetSection("Payments:Plans:price_premium_monthly")).Returns(premiumSection.Object);

        var goldSection = new Mock<IConfigurationSection>();
        goldSection.Setup(s => s.Value).Returns("exists");
        _configurationMock.Setup(c => c.GetSection("Payments:Plans:price_gold_monthly")).Returns(goldSection.Object);
        
        var invalidSection = new Mock<IConfigurationSection>();
        _configurationMock.Setup(c => c.GetSection("Payments:Plans:invalid_plan")).Returns(invalidSection.Object);
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
        var gatewayResult = SubscriptionGatewayResult.Succeeded("sub_123", "https://checkout.stripe.com/xyz");

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
        var gatewayResult = SubscriptionGatewayResult.Failed("Gateway error");

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
        var exception = await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("Falha ao comunicar com o provedor de pagamento.");
        exception.WithInnerException<Exception>().WithMessage("Unexpected error");
    }

    [Fact]
    public async Task HandleAsync_ShouldRollbackGateway_WhenRepositoryFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand(providerId, "price_premium_monthly");
        var gatewayResult = SubscriptionGatewayResult.Succeeded("sub_rollback", "https://checkout");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _repositoryMock.Setup(r => r.AddAsync(It.Is<Subscription>(s => s.ProviderId == providerId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        _gatewayMock.Setup(g => g.CancelSubscriptionAsync("sub_rollback", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Operação revertida no gateway*");
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_rollback", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowSubscriptionCreationException_WhenPlanIdIsInvalid()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "invalid_plan");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Plano inválido*");
    }

    [Fact]
    public async Task HandleAsync_WithGoldPlan_ShouldSucceed()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_gold_monthly");
        var gatewayResult = SubscriptionGatewayResult.Succeeded("sub_123", "https://checkout");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), "price_gold_monthly", It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(gatewayResult.CheckoutUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldPropagateOperationCanceledException_WhenGatewayIsCanceled()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldPropagateOperationCanceledException_WhenRepositoryIsCanceled()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        var gatewayResult = SubscriptionGatewayResult.Succeeded("sub_123", "https://checkout");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCallCancel_WhenRepositoryFails_AndExternalSubIdIsNull()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        
        // Usa reflexão para simular o estado com ID nulo (factory não permite)
        var gatewayResult = (SubscriptionGatewayResult)typeof(SubscriptionGatewayResult)
            .GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, 
                new[] { typeof(bool), typeof(string), typeof(string), typeof(string) }, null)!
            .Invoke(new object?[] { true, null, "https://checkout", null });

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>();
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
