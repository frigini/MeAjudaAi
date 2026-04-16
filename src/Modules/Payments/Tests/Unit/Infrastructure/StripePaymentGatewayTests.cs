using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Stripe;
using Stripe.Checkout;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

public class StripePaymentGatewayTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<StripePaymentGateway>> _loggerMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly StripePaymentGateway _gateway;

    public StripePaymentGatewayTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<StripePaymentGateway>>();
        _stripeServiceMock = new Mock<IStripeService>();

        _configMock.Setup(x => x["Stripe:ApiKey"]).Returns("sk_test_mock");
        _configMock.Setup(x => x["Payments:SuccessUrl"]).Returns("https://success");
        _configMock.Setup(x => x["Payments:CancelUrl"]).Returns("https://cancel");

        _gateway = new StripePaymentGateway(_configMock.Object, _loggerMock.Object, _stripeServiceMock.Object);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnError_WhenZeroDecimalCurrencyHasFractionalAmount()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "price_123";
        var amount = Money.FromDecimal(100.50m, "JPY");

        // Act
        var result = await _gateway.CreateSubscriptionAsync(providerId, planId, amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não aceitam valores fracionados");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnSuccess_WhenEverythingIsValid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "price_123";
        var amount = Money.FromDecimal(100m, "BRL");
        
        var mockPrice = new Price { UnitAmount = 10000, Currency = "brl" };
        var mockSession = new Session { Url = "https://checkout.stripe.com/mock" };

        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrice);
        
        _stripeServiceMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession);

        // Act
        var result = await _gateway.CreateSubscriptionAsync(providerId, planId, amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/mock");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnError_WhenPriceMismatches()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "price_123";
        var amount = Money.FromDecimal(100m, "BRL");
        
        var mockPrice = new Price { UnitAmount = 15000, Currency = "brl" }; // 150 vs 100

        _stripeServiceMock.Setup(x => x.GetPriceAsync(It.IsAny<string>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrice);

        // Act
        var result = await _gateway.CreateSubscriptionAsync(providerId, planId, amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não coincide");
    }
}
