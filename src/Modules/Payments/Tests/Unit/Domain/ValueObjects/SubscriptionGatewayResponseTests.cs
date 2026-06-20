using MeAjudaAi.Modules.Payments.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.ValueObjects;

public class SubscriptionGatewayResponseTests
{
    [Fact]
    public void Succeeded_ShouldReturnSuccessResponseWithCorrectValues()
    {
        // Arrange
        var externalId = "sub_123";
        var checkoutUrl = "https://checkout.stripe.com/pay/session_123";

        // Act
        var result = SubscriptionGatewayResponse.Succeeded(externalId, checkoutUrl);

        // Assert
        result.Success.Should().BeTrue();
        result.ExternalSubscriptionId.Should().Be(externalId);
        result.CheckoutUrl.Should().Be(checkoutUrl);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Succeeded_ShouldAllowNullExternalSubscriptionId()
    {
        // Arrange & Act
        var result = SubscriptionGatewayResponse.Succeeded(null, "https://checkout.stripe.com/pay/session_123");

        // Assert
        result.ExternalSubscriptionId.Should().BeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Succeeded_ShouldThrow_WhenCheckoutUrlIsNull()
    {
        // Act
        var act = () => SubscriptionGatewayResponse.Succeeded("sub_123", null!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*checkoutUrl*");
    }

    [Fact]
    public void Succeeded_ShouldThrow_WhenCheckoutUrlIsEmpty()
    {
        // Act
        var act = () => SubscriptionGatewayResponse.Succeeded("sub_123", "");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*checkoutUrl*");
    }

    [Fact]
    public void Succeeded_ShouldThrow_WhenCheckoutUrlIsWhitespace()
    {
        // Act
        var act = () => SubscriptionGatewayResponse.Succeeded("sub_123", "   ");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*checkoutUrl*");
    }

    [Fact]
    public void Failed_ShouldReturnFailureResponseWithCorrectValues()
    {
        // Arrange
        var errorMessage = "Payment provider error";

        // Act
        var result = SubscriptionGatewayResponse.Failed(errorMessage);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ExternalSubscriptionId.Should().BeNull();
        result.CheckoutUrl.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldThrow_WhenErrorMessageIsNull()
    {
        // Act
        var act = () => SubscriptionGatewayResponse.Failed(null!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*errorMessage*");
    }

    [Fact]
    public void Failed_ShouldThrow_WhenErrorMessageIsEmpty()
    {
        // Act
        var act = () => SubscriptionGatewayResponse.Failed("");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*errorMessage*");
    }

    [Fact]
    public void Failed_ShouldThrow_WhenErrorMessageIsWhitespace()
    {
        // Act
        var act = () => SubscriptionGatewayResponse.Failed("   ");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*errorMessage*");
    }

    [Fact]
    public void Record_ShouldHaveValueEquality()
    {
        // Arrange
        var a = SubscriptionGatewayResponse.Succeeded("sub_123", "https://checkout.stripe.com/pay/session_123");
        var b = SubscriptionGatewayResponse.Succeeded("sub_123", "https://checkout.stripe.com/pay/session_123");

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Record_ShouldDistinguishDifferentValues()
    {
        // Arrange
        var a = SubscriptionGatewayResponse.Succeeded("sub_123", "https://checkout.stripe.com/pay/session_123");
        var b = SubscriptionGatewayResponse.Succeeded("sub_456", "https://checkout.stripe.com/pay/session_456");

        // Assert
        a.Should().NotBe(b);
    }
}
