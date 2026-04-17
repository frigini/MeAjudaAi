using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class PaymentTransactionTests
{
    [Fact]
    public void Constructor_ShouldSetInitialValues()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var amount = Money.FromDecimal(50.00m, "BRL");

        // Act
        var tx = new PaymentTransaction(subId, amount);

        // Assert
        tx.SubscriptionId.Should().Be(subId);
        tx.Amount.Should().Be(amount);
        tx.Status.Should().Be(EPaymentStatus.Pending);
        tx.ExternalTransactionId.Should().BeNull();
    }

    [Fact]
    public void Settle_ShouldUpdateStatusAndReference()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));
        var externalId = "ch_123";

        // Act
        tx.Settle(externalId);

        // Assert
        tx.Status.Should().Be(EPaymentStatus.Succeeded);
        tx.ExternalTransactionId.Should().Be(externalId);
        tx.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Settle_ShouldThrow_WhenAlreadyProcessed()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));
        tx.Settle("id1");

        // Act
        var act = () => tx.Settle("id2");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*status*");
    }

    [Fact]
    public void Fail_ShouldUpdateStatus()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));

        // Act
        tx.Fail();

        // Assert
        tx.Status.Should().Be(EPaymentStatus.Failed);
        tx.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(-10)]
    public void Constructor_ShouldThrow_WhenAmountIsNegative(decimal val)
    {
        // Act
        var act = () => Money.FromDecimal(val);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
