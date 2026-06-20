using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Exceptions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers.Subscriptions.Commands;

public class CreateSubscriptionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Subscription, Guid>> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ILogger<CreateSubscriptionCommandHandler>> _loggerMock;
    private readonly CreateSubscriptionCommandHandler _handler;

    public CreateSubscriptionCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Subscription, Guid>>();
        _uowMock.Setup(u => u.GetRepository<Subscription, Guid>()).Returns(_repositoryMock.Object);
        _gatewayMock = new Mock<IPaymentGateway>();
        _messageBusMock = new Mock<IMessageBus>();
        _loggerMock = new Mock<ILogger<CreateSubscriptionCommandHandler>>();

        // Configuração de plano válida
        var options = new PaymentsOptions
        {
            Plans = new Dictionary<string, PlanOptions>
            {
                {
                    "premium", new PlanOptions
                    {
                        Amount = 99.90m,
                        Currency = "BRL",
                        StripePriceId = "price_premium_monthly"
                    }
                }
            }
        };

        _handler = new CreateSubscriptionCommandHandler(
            _uowMock.Object,
            _gatewayMock.Object,
            options,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCheckoutUrl_WhenSucceeds()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        var checkoutUrl = "https://stripe.com/checkout/123";

        var gatewayResult = SubscriptionGatewayResponse.Succeeded("sub_123", checkoutUrl);
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(checkoutUrl);
        _repositoryMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenIdempotencyKeyExceeds255Chars()
    {
        // Arrange
        var longKey = new string('x', 256);
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium", longKey);

        // Act
        Func<Task> act = () => _handler.HandleAsync(command);

        // Assert
        var ex = await act.Should().ThrowAsync<SubscriptionCreationException>();
        ex.Which.Message.Should().Contain("255");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenPlanIsInvalid()
    {
        // Arrange
        var options = new PaymentsOptions
        {
            Plans = new Dictionary<string, PlanOptions>()
        };
        var handler = new CreateSubscriptionCommandHandler(
            _uowMock.Object,
            _gatewayMock.Object,
            options,
            _loggerMock.Object);
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "invalid_plan");

        // Act
        var act = () => handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Plano inválido*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenConfigIsIncomplete()
    {
        // Arrange
        var options = new PaymentsOptions
        {
            Plans = new Dictionary<string, PlanOptions>
            {
                { "premium", new PlanOptions { Amount = 99.90m, Currency = string.Empty } }
            }
        };
        var handler = new CreateSubscriptionCommandHandler(
            _uowMock.Object,
            _gatewayMock.Object,
            options,
            _loggerMock.Object);
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");

        // Act
        var act = () => handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Configuração incompleta*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenGatewayFails_AndCompensateIfPossible()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");

        // Simular sucesso no gateway mas sem URL de checkout (cenário de erro raro)
        var badResult = new SubscriptionGatewayResponse(true, "sub_123", null, null);
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
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
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        var gatewayResult = SubscriptionGatewayResponse.Succeeded("sub_123", "https://checkout");

        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Falha ao persistir*");
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenGatewayCommunicationThrows()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Falha ao comunicar*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenGatewayReturnsFailure()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        var failedResult = SubscriptionGatewayResponse.Failed("Stripe error");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>().WithMessage("*Falha ao criar assinatura no gateway*");
    }

    [Fact]
    public async Task HandleAsync_ShouldCompensateAndLogWarning_WhenCancelFails()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        var gatewayResult = SubscriptionGatewayResponse.Succeeded("sub_123", "https://checkout");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        _gatewayMock.Setup(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Cancel fails

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>();
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCompensateAndLogWarning_WhenCancelThrows()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        var gatewayResult = SubscriptionGatewayResponse.Succeeded("sub_123", "https://checkout");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gatewayResult);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        _gatewayMock.Setup(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cancel error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<SubscriptionCreationException>();
        _gatewayMock.Verify(g => g.CancelSubscriptionAsync("sub_123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowOperationCanceledException_WhenCanceledDuringGatewayCall()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowOperationCanceledException_WhenCanceledDuringRepositoryCall()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(Guid.NewGuid(), "premium");
        _gatewayMock.Setup(g => g.CreateSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubscriptionGatewayResponse.Succeeded("sub_123", "https://url"));

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}