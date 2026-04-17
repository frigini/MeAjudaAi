using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

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
        _handler = new CreateSubscriptionCommandHandler(
            _repositoryMock.Object,
            _gatewayMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);

        // Valid plan config setup
        _configurationMock.Setup(x => x["Payments:Plans:price_premium_monthly:Amount"]).Returns("99.90");
        _configurationMock.Setup(x => x["Payments:Plans:price_premium_monthly:Currency"]).Returns("BRL");
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(x => x.Value).Returns("premium");
        _configurationMock.Setup(x => x.GetSection("Payments:Plans:price_premium_monthly")).Returns(sectionMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCheckoutUrl_WhenSucceeds()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        var checkoutUrl = "https://stripe.com/checkout/123";
        
        var gatewayResult = SubscriptionGatewayResult.Succeeded("sub_123", checkoutUrl);
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(checkoutUrl);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenPlanIsInvalid()
    {
        // Arrange
        _configurationMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "invalid_plan");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Plano inválido*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenConfigIsIncomplete()
    {
        // Arrange
        _configurationMock.Setup(x => x["Payments:Plans:price_premium_monthly:Amount"]).Returns(""); // Missing amount
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Configuração incompleta*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenGatewayFails_AndCompensateIfPossible()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        
        // Simular sucesso no gateway mas sem URL de checkout (cenário de erro raro)
        var badResult = new SubscriptionGatewayResult(true, "sub_123", null, null);
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badResult);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*URL de checkout ausente*");
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCompensate_WhenRepositoryFails()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        var gatewayResult = SubscriptionGatewayResult.Succeeded("sub_123", "https://checkout");
        
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Falha ao persistir*");
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCallCancel_WhenRepositoryFails_AndExternalSubIdIsNull()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "price_premium_monthly");
        
        var gatewayResult = SubscriptionGatewayResult.SucceededWithoutExternalId("https://checkout");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>();
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
