using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Gateways;

public class StripePaymentGatewayTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<StripePaymentGateway>> _loggerMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly PaymentsOptions _paymentsOptions;
    private StripePaymentGateway? _gateway;

    public StripePaymentGatewayTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<StripePaymentGateway>>();
        _stripeServiceMock = new Mock<IStripeService>();
        _paymentsOptions = new PaymentsOptions
        {
            SuccessUrl = "/success",
            CancelUrl = "/cancel",
            Plans = new Dictionary<string, PlanOptions>
            {
                { "plan_test", new PlanOptions { StripePriceId = "stripe_price_test", Amount = 10, Currency = "BRL" } }
            }
        };
        
        // Default valid setup
        _configurationMock.Setup(x => x["Stripe:ApiKey"]).Returns("sk_test_123");
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("https://meajudaai.com");
    }

    private void CreateGateway() => _gateway = new StripePaymentGateway(
        _configurationMock.Object, 
        _paymentsOptions, 
        _loggerMock.Object, 
        _stripeServiceMock.Object);

    [Fact]
    public void Constructor_ShouldThrow_WhenClientBaseUrlIsMissing()
    {
        _configurationMock.Setup(x => x["Stripe:ApiKey"]).Returns("sk_test_123");
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("");
        var act = () => new StripePaymentGateway(_configurationMock.Object, _paymentsOptions, _loggerMock.Object, _stripeServiceMock.Object);
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
                o.LineItems[0].Price == "stripe_price_test"),
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

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnFailed_WhenZeroDecimalCurrencyHasFractionalAmount()
    {
        // Arrange
        CreateGateway();
        var amount = new Money(1.50m, "JPY"); // JPY é zero-decimal, fração inválida
        
        // Act
        var result = await _gateway!.CreateSubscriptionAsync(Guid.NewGuid(), "plan_a", amount);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("fractional");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldAppendSessionId_WhenUrlHasNoPlaceholder()
    {
        // Arrange
        _paymentsOptions.SuccessUrl = "/success";
        CreateGateway();

        var stripePrice = new Price { UnitAmount = 1000, Currency = "brl" };
        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripePrice);

        var stripeSession = new Stripe.Checkout.Session { Url = "https://checkout.stripe.com/pay/s1" };
        _stripeServiceMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<Stripe.Checkout.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeSession);

        // Act
        await _gateway!.CreateSubscriptionAsync(Guid.NewGuid(), "plan_test", Money.FromDecimal(10, "BRL"), null, CancellationToken.None);

        // Assert
        _stripeServiceMock.Verify(x => x.CreateCheckoutSessionAsync(
            It.Is<Stripe.Checkout.SessionCreateOptions>(o =>
                o.SuccessUrl == "https://meajudaai.com/success?session_id={CHECKOUT_SESSION_ID}"),
            It.IsAny<RequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldNotDuplicateSessionId_WhenUrlAlreadyContainsPlaceholder()
    {
        // Arrange
        _paymentsOptions.SuccessUrl = "/payment/success?session_id={CHECKOUT_SESSION_ID}";
        CreateGateway();

        var stripePrice = new Price { UnitAmount = 1000, Currency = "brl" };
        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripePrice);

        var stripeSession = new Stripe.Checkout.Session { Url = "https://checkout.stripe.com/pay/s2" };
        _stripeServiceMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<Stripe.Checkout.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeSession);

        // Act
        await _gateway!.CreateSubscriptionAsync(Guid.NewGuid(), "plan_test", Money.FromDecimal(10, "BRL"), null, CancellationToken.None);

        // Assert - should NOT contain a second session_id
        _stripeServiceMock.Verify(x => x.CreateCheckoutSessionAsync(
            It.Is<Stripe.Checkout.SessionCreateOptions>(o =>
                o.SuccessUrl == "https://meajudaai.com/payment/success?session_id={CHECKOUT_SESSION_ID}"),
            It.IsAny<RequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldAppendSessionIdWithAmpersand_WhenUrlHasQueryButNoPlaceholder()
    {
        // Arrange
        _paymentsOptions.SuccessUrl = "/payment/success?ref=home";
        CreateGateway();

        var stripePrice = new Price { UnitAmount = 1000, Currency = "brl" };
        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripePrice);

        var stripeSession = new Stripe.Checkout.Session { Url = "https://checkout.stripe.com/pay/s3" };
        _stripeServiceMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<Stripe.Checkout.SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stripeSession);

        // Act
        await _gateway!.CreateSubscriptionAsync(Guid.NewGuid(), "plan_test", Money.FromDecimal(10, "BRL"), null, CancellationToken.None);

        // Assert
        _stripeServiceMock.Verify(x => x.CreateCheckoutSessionAsync(
            It.Is<Stripe.Checkout.SessionCreateOptions>(o =>
                o.SuccessUrl == "https://meajudaai.com/payment/success?ref=home&session_id={CHECKOUT_SESSION_ID}"),
            It.IsAny<RequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
