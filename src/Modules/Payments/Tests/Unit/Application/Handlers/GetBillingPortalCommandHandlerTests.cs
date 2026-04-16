using FluentAssertions;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

public class GetBillingPortalCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<ILogger<GetBillingPortalCommandHandler>> _loggerMock;
    private readonly GetBillingPortalCommandHandler _handler;

    public GetBillingPortalCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _loggerMock = new Mock<ILogger<GetBillingPortalCommandHandler>>();
        _handler = new GetBillingPortalCommandHandler(_repositoryMock.Object, _gatewayMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPortalUrl_WhenSubscriptionIsActive()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var returnUrl = "https://example.com/return";
        var expectedPortalUrl = "https://billing.stripe.com/p/session/abc";
        
        var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        var command = new GetBillingPortalCommand(providerId, returnUrl);

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync("cus_123", returnUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPortalUrl);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedPortalUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowApplicationException_WhenSubscriptionNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new GetBillingPortalCommand(providerId, "https://example.com");

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription)null!);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*Subscription not found or not active*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowApplicationException_WhenGatewayReturnsNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        var command = new GetBillingPortalCommand(providerId, "https://example.com");

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync("cus_123", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null!);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Failed to generate Billing Portal session.");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowApplicationException_WhenSubscriptionHasNoExternalCustomerId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        
        // Mock sub sem ExternalCustomerId (via reflection ou mockando props se fosse interface, 
        // mas aqui usamos a entidade real. Activate exige ExternalCustomerId, então vamos forçar via construtor ou outro meio se possível,
        // ou apenas aceitar que se Activate foi chamado corretamente, terá ID.
        // Mas o review pede para testar este branch do handler.)
        
        // Como Activate agora valida externalCustomerId, para testar esse branch no handler 
        // precisaríamos de uma sub que de alguma forma não tem o ID (ex: persistida incorretamente antes da validação).
        
        var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        // Não chamamos Activate, mas o repositório vai retornar ela se mockarmos. 
        // Porém o handler usa GetActiveByProviderIdAsync que filtra por Status == Active.
        // Vamos usar um truque de reflexão para simular o estado inválido para o teste de branch.
        
        var statusField = typeof(Subscription).GetProperty("Status");
        statusField?.SetValue(subscription, ESubscriptionStatus.Active);

        var command = new GetBillingPortalCommand(providerId, "https://example.com");

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage($"*has no ExternalCustomerId*");
    }
}
