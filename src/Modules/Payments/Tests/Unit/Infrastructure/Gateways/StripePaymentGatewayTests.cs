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
    private StripePaymentGateway? _gateway;

    public StripePaymentGatewayTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<StripePaymentGateway>>();
        _stripeServiceMock = new Mock<IStripeService>();
        
        // Default valid setup
        _configurationMock.Setup(x => x["Stripe:ApiKey"]).Returns("sk_test_123");
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("https://meajudaai.com");
        _configurationMock.Setup(x => x["Payments:SuccessUrl"]).Returns("/success");
        _configurationMock.Setup(x => x["Payments:CancelUrl"]).Returns("/cancel");
    }

    private void CreateGateway() => _gateway = new StripePaymentGateway(_configurationMock.Object, _loggerMock.Object, _stripeServiceMock.Object);

    [Fact]
    public void Constructor_ShouldThrow_WhenApiKeyIsMissing()
    {
        _configurationMock.Setup(x => x["Stripe:ApiKey"]).Returns("");
        var act = () => new StripePaymentGateway(_configurationMock.Object, _loggerMock.Object, _stripeServiceMock.Object);
        act.Should().Throw<ArgumentException>().WithMessage("*ApiKey*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenClientBaseUrlIsMissing()
    {
        _configurationMock.Setup(x => x["Stripe:ApiKey"]).Returns("sk_test_123");
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("");
        var act = () => new StripePaymentGateway(_configurationMock.Object, _loggerMock.Object, _stripeServiceMock.Object);
        act.Should().Throw<ArgumentException>().WithMessage("*ClientBaseUrl*");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnSuccess_WhenEverythingIsCorrect()
    {
        // Arrange
        CreateGateway();
        var providerId = Guid.NewGuid();
        var planId = "plan_test";
        var amount = Money.FromDecimal(10, "BRL");
        
        var stripePrice = new Price { UnitAmount = 1000, Currency = "brl" }; // 10.00
        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripePrice);

        var stripeSession = new Stripe.Checkout.Session { Url = "https://checkout.stripe.com/pay/session_123", SubscriptionId = "sub_123" };
        _stripeServiceMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<Stripe.Checkout.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeSession);

        // Act
        var result = await _gateway!.CreateSubscriptionAsync(providerId, planId, amount, "idempotency-key", CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/pay/session_123");
        
        _stripeServiceMock.Verify(x => x.CreateCheckoutSessionAsync(
            It.Is<Stripe.Checkout.SessionCreateOptions>(o => 
                o.Metadata["provider_id"] == providerId.ToString() &&
                o.LineItems[0].Price == planId), // Assuming planId maps directly or via config (here directly)
            It.Is<RequestOptions>(ro => ro.IdempotencyKey == "idempotency-key"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnFailed_OnStripeException()
    {
        // Arrange
        CreateGateway();
        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Error"));

        // Act
        var result = await _gateway!.CreateSubscriptionAsync(Guid.NewGuid(), "plan_123", Money.FromDecimal(10), null, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("communication failure");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnFailed_WhenPriceMismatch()
    {
        // Arrange
        CreateGateway();
        var stripePrice = new Price { UnitAmount = 2000, Currency = "brl" }; // 20.00
        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripePrice);

        // Act
        var result = await _gateway!.CreateSubscriptionAsync(Guid.NewGuid(), "plan_123", Money.FromDecimal(10), null, CancellationToken.None); // Expected 10.00

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("match");
    }

    [Fact]
    public async Task CancelSubscriptionAsync_ShouldReturnFalse_OnException()
    {
        // Arrange
        CreateGateway();
        _stripeServiceMock.Setup(x => x.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Error"));

        // Act
        var result = await _gateway!.CancelSubscriptionAsync("sub_123", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBillingPortalSessionAsync_ShouldReturnNull_OnException()
    {
        // Arrange
        CreateGateway();
        _stripeServiceMock.Setup(x => x.CreateBillingPortalSessionAsync(It.IsAny<Stripe.BillingPortal.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Error"));

        // Act
        var result = await _gateway!.CreateBillingPortalSessionAsync("cus_123", "https://return", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
