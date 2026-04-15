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
    public async Task HandleAsync_ShouldReturnPortalUrl_WhenSubscriptionExistsWithExternalCustomerId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalCustomerId = "cus_123";
        var returnUrl = "https://example.com/return";
        var expectedPortalUrl = "https://billing.stripe.com/p/session/abc";
        
        var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        // Use reflection or a public method to set ExternalCustomerId if it's private set
        // Based on previous edits, it was updated in Activate() but for testing we might need to set it.
        // Let's assume we can set it via Activate or it's public for now (internal/private set with reflection is common in these tests).
        subscription.Activate("sub_123", externalCustomerId, DateTime.UtcNow.AddMonths(1));

        var command = new GetBillingPortalCommand(providerId, returnUrl);

        _repositoryMock.Setup(r => r.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync(externalCustomerId, returnUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPortalUrl);

    // Act
    var result = await _handler.HandleAsync(command);

    // Assert
    result.Should().Be(expectedPortalUrl);
    _gatewayMock.Verify(g => g.CreateBillingPortalSessionAsync(externalCustomerId, returnUrl, It.IsAny<CancellationToken>()), Times.Once);
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
        .WithMessage($"Subscription not found for provider {providerId}");
}

[Fact]
public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenExternalCustomerIdIsMissing()
{
    // Arrange
    var providerId = Guid.NewGuid();
    var subscription = new Subscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
    // No external customer ID set
    
    var command = new GetBillingPortalCommand(providerId, "https://example.com");

    _repositoryMock.Setup(r => r.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(subscription);

    // Act
    var act = () => _handler.HandleAsync(command);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("External Customer ID is missing.*");
}
}
