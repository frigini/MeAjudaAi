using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Stripe;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

public class StripePaymentGatewayTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<StripePaymentGateway>> _loggerMock;
    private readonly StripePaymentGateway _gateway;

    public StripePaymentGatewayTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<StripePaymentGateway>>();

        _configMock.Setup(x => x["Stripe:ApiKey"]).Returns("sk_test_mock");
        _configMock.Setup(x => x["Payments:SuccessUrl"]).Returns("https://success");
        _configMock.Setup(x => x["Payments:CancelUrl"]).Returns("https://cancel");

        _gateway = new StripePaymentGateway(_configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldReturnError_WhenZeroDecimalCurrencyHasFractionalAmount()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "price_123";
        // JPY is zero-decimal
        var amount = Money.FromDecimal(100.50m, "JPY");

        // Act
        var result = await _gateway.CreateSubscriptionAsync(providerId, planId, amount, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Zero-decimal currency JPY cannot have fractional amounts");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ShouldContinue_WhenZeroDecimalCurrencyHasNoFraction()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "price_123";
        var amount = Money.FromDecimal(100m, "JPY");

        // Act & Assert (Will fail later because of real Stripe API call with mock key, but we want to see it pass the fraction check)
        var act = () => _gateway.CreateSubscriptionAsync(providerId, planId, amount, CancellationToken.None);
        
        // It should pass the fractional check and hit Stripe API (which throws StripeException due to invalid key)
        var result = await act();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotContain("Zero-decimal currency");
    }
}
