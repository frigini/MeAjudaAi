using FluentAssertions;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

using DomainSubscription = MeAjudaAi.Modules.Payments.Domain.Entities.Subscription;

public class GetBillingPortalCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<GetBillingPortalCommandHandler>> _loggerMock;
    private readonly GetBillingPortalCommandHandler _handler;

    public GetBillingPortalCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<GetBillingPortalCommandHandler>>();

        // Setup base config for allowed hosts
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s.GetChildren()).Returns(Enumerable.Empty<IConfigurationSection>());
        _configurationMock.Setup(c => c.GetSection("Payments:AllowedReturnHosts")).Returns(section.Object);
        
        _handler = new GetBillingPortalCommandHandler(
            _repositoryMock.Object, 
            _gatewayMock.Object, 
            _configurationMock.Object, 
            _loggerMock.Object);
    }

    private void SetupAllowedHost(string host)
    {
        var item = new Mock<IConfigurationSection>();
        item.Setup(s => s.Value).Returns(host);

        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s.GetChildren()).Returns(new[] { item.Object });
        _configurationMock.Setup(c => c.GetSection("Payments:AllowedReturnHosts")).Returns(section.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPortalUrl_WhenSubscriptionIsActive()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var returnUrl = "https://localhost/return";
        var expectedPortalUrl = "https://billing.stripe.com/p/session/abc";
        
        var subscription = new DomainSubscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        var command = new GetBillingPortalCommand(providerId, returnUrl);

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync("cus_123", returnUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPortalUrl);

        SetupAllowedHost("localhost");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedPortalUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundException_WhenSubscriptionNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new GetBillingPortalCommand(providerId, "https://localhost/account");

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription)null!);

        SetupAllowedHost("localhost");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*was not found*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowBusinessRuleException_WhenGatewayReturnsNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var subscription = new DomainSubscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        var command = new GetBillingPortalCommand(providerId, "https://localhost/return");

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _gatewayMock.Setup(g => g.CreateBillingPortalSessionAsync("cus_123", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null!);

        SetupAllowedHost("localhost");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessRuleException>();
        exception.Which.RuleName.Should().Be("GATEWAY_SESSION_FAILURE");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowBusinessRuleException_WhenSubscriptionHasNoExternalCustomerId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        
        var subscription = new DomainSubscription(providerId, "plan_premium", Money.FromDecimal(99.90m, "BRL"));
        
        var statusField = typeof(DomainSubscription).GetProperty("Status");
        statusField?.SetValue(subscription, ESubscriptionStatus.Active);

        var command = new GetBillingPortalCommand(providerId, "https://localhost/return");

        _repositoryMock.Setup(r => r.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        SetupAllowedHost("localhost");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessRuleException>();
        exception.Which.RuleName.Should().Be("MISSING_EXTERNAL_CUSTOMER_ID");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowBusinessRuleException_WhenReturnUrlIsInvalid()
    {
        // Arrange
        var command = new GetBillingPortalCommand(Guid.NewGuid(), "not-a-url");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessRuleException>();
        exception.Which.RuleName.Should().Be("INVALID_RETURN_URL");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowBusinessRuleException_WhenReturnUrlIsUntrusted()
    {
        // Arrange
        var command = new GetBillingPortalCommand(Guid.NewGuid(), "https://malicious.com/hack");
        SetupAllowedHost("localhost");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessRuleException>();
        exception.Which.RuleName.Should().Be("UNTRUSTED_RETURN_HOST");
    }
}
