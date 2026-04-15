using FluentAssertions;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

public class GetBillingPortalCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly GetBillingPortalCommandHandler _handler;

    public GetBillingPortalCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _handler = new GetBillingPortalCommandHandler(_repositoryMock.Object, _gatewayMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPortalUrl_WhenSubscriptionExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var returnUrl = "https://example.com/return";
        var expectedPortalUrl = "https://billing.stripe.com/p/session/abc";
        
        var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        subscription.Activate("sub_123", DateTime.UtcNow.AddMonths(1));

        var command = new GetBillingPortalCommand(providerId, returnUrl);

        _repositoryMock.Setup(r => r.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync("sub_123", returnUrl, It.IsAny<CancellationToken>()))
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

        _repositoryMock.Setup(r => r.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription)null!);

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*Subscription not found*");
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenExternalSubscriptionIdIsAvailable()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        
        var command = new GetBillingPortalCommand(providerId, "https://example.com");

        _repositoryMock.Setup(r => r.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://billing.stripe.com/mock");

        // Act & Assert
        var result = await _handler.HandleAsync(command);
        result.Should().NotBeNullOrEmpty();
    }
}