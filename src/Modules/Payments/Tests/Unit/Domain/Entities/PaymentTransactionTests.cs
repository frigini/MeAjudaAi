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

    [Fact]
    public void Fail_ShouldThrow_WhenAlreadyProcessed()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));
        tx.Fail();

        // Act
        var act = () => tx.Fail();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Refund_ShouldUpdateStatus()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));
        tx.Settle("id1");

        // Act
        tx.Refund();

        // Assert
        tx.Status.Should().Be(EPaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_ShouldBeIdempotent_WhenAlreadyRefunded()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), new Money(100m, "BRL"));
        transaction.Settle("tx_ext_001");
        transaction.Refund(); // primeira chamada

        // Act
        Action act = () => transaction.Refund(); // segunda chamada

        // Assert
        act.Should().NotThrow();
        transaction.Status.Should().Be(EPaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_ShouldPreserveRefundedAt_OnSecondCall()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), new Money(50m, "BRL"));
        transaction.Settle("tx_ext_002");
        transaction.Refund();
        var firstRefundedAt = transaction.RefundedAt;

        Thread.Sleep(10); // garante delta de tempo

        // Act
        transaction.Refund(); // segunda chamada — no-op

        // Assert
        transaction.RefundedAt.Should().Be(firstRefundedAt);
    }

    [Fact]
    public void Refund_ShouldThrow_WhenNotSucceeded()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));

        // Act
        var act = () => tx.Refund();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Settle_ShouldThrow_WhenExternalIdIsEmpty()
    {
        // Arrange
        var tx = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10));

        // Act
        var act = () => tx.Settle("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
