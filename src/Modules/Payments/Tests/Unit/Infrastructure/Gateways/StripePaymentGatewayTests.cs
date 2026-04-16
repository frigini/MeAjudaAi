using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Gateways;

public class StripePaymentGatewayTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<StripePaymentGateway>> _loggerMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly StripePaymentGateway _gateway;

    public StripePaymentGatewayTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<StripePaymentGateway>>();
        _stripeServiceMock = new Mock<IStripeService>();

        _configurationMock.Setup(c => c["Stripe:ApiKey"]).Returns("sk_test_123");
        _configurationMock.Setup(c => c["Payments:SuccessUrl"]).Returns("https://success.com");
        _configurationMock.Setup(c => c["Payments:CancelUrl"]).Returns("https://cancel.com");

        _gateway = new StripePaymentGateway(_configurationMock.Object, _loggerMock.Object, _stripeServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenApiKeyIsMissing()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Stripe:ApiKey"]).Returns((string)null!);

        // Act
        var act = () => new StripePaymentGateway(config.Object, _loggerMock.Object, _stripeServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Stripe API key is not configured*");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnFailure_WhenAmountIsFractionalForZeroDecimalCurrency()
    {
        // Arrange
        var amount = Money.FromDecimal(10.5m, "JPY"); // JPY is zero-decimal

        // Act
        var result = await _gateway.CreateSubscriptionAsync(Guid.NewGuid(), "plan_123", amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Zero-decimal currency (JPY) does not accept fractional amounts");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnFailure_WhenPriceMismatchDetected()
    {
        // Arrange
        var amount = Money.FromDecimal(100.00m, "USD");
        var price = new Price { UnitAmount = 5000L, Currency = "usd" }; // 50.00 USD != 100.00 USD
        
        _stripeServiceMock.Setup(s => s.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);

        // Act
        var result = await _gateway.CreateSubscriptionAsync(Guid.NewGuid(), "plan_123", amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("plan amount or currency does not match");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnSuccess_WhenEverythingIsCorrect()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "price_123";
        var amount = Money.FromDecimal(99.90m, "BRL");
        var price = new Price { UnitAmount = 9990L, Currency = "brl" };
        var session = new Stripe.Checkout.Session { Url = "https://stripe.checkout/session/123" };

        _stripeServiceMock.Setup(s => s.GetPriceAsync(planId, It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);
        
        _stripeServiceMock.Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<Stripe.Checkout.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _gateway.CreateSubscriptionAsync(providerId, planId, amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.CheckoutUrl.Should().Be(session.Url);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnFailure_WhenStripeExceptionOccurs()
    {
        // Arrange
        _stripeServiceMock.Setup(s => s.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Stripe error"));

        // Act
        var result = await _gateway.CreateSubscriptionAsync(Guid.NewGuid(), "plan_123", Money.FromDecimal(10, "USD"), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Payment provider communication failure");
    }

    [Fact]
    public async Task CancelSubscriptionAsync_ShouldReturnTrue_WhenSuccess()
    {
        // Arrange
        _stripeServiceMock.Setup(s => s.CancelSubscriptionAsync("sub_123", It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _gateway.CancelSubscriptionAsync("sub_123", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelSubscriptionAsync_ShouldReturnFalse_WhenStripeExceptionOccurs()
    {
        // Arrange
        _stripeServiceMock.Setup(s => s.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Error"));

        // Act
        var result = await _gateway.CancelSubscriptionAsync("sub_123", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBillingPortalSessionAsync_ShouldReturnUrl_WhenSuccess()
    {
        // Arrange
        var expectedUrl = "https://billing.portal/abc";
        _stripeServiceMock.Setup(s => s.CreateBillingPortalSessionAsync(It.IsAny<Stripe.BillingPortal.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _gateway.CreateBillingPortalSessionAsync("cus_123", "https://return.com", CancellationToken.None);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task CreateBillingPortalSessionAsync_ShouldReturnNull_WhenStripeExceptionOccurs()
    {
        // Arrange
        _stripeServiceMock.Setup(s => s.CreateBillingPortalSessionAsync(It.IsAny<Stripe.BillingPortal.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Error"));

        // Act
        var result = await _gateway.CreateBillingPortalSessionAsync("cus_123", "https://return.com", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
