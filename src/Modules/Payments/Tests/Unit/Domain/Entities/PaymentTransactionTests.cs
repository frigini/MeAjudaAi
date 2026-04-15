using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class PaymentTransactionTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var amount = Money.FromDecimal(100.50m, "BRL");

        // Act
        var transaction = new PaymentTransaction(subscriptionId, amount);

        // Assert
        transaction.Id.Should().NotBeEmpty();
        transaction.SubscriptionId.Should().Be(subscriptionId);
        transaction.Amount.Should().Be(amount);
        transaction.Status.Should().Be(EPaymentStatus.Pending);
        transaction.ProcessedAt.Should().BeNull();
        transaction.ExternalTransactionId.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldNotOverrideBaseEntityId()
    {
        // Arrange & Act
        var t1 = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(10m, "BRL"));
        var t2 = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(20m, "BRL"));

        // Assert
        t1.Id.Should().NotBe(t2.Id);
        t1.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Settle_ShouldUpdateStatusToSucceeded()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(50m, "BRL"));
        var externalId = "ext_123456";

        // Act
        transaction.Settle(externalId);

        // Assert
        transaction.Status.Should().Be(EPaymentStatus.Succeeded);
        transaction.ExternalTransactionId.Should().Be(externalId);
        transaction.ProcessedAt.Should().NotBeNull();
        transaction.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        transaction.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Settle_ShouldThrow_WhenAlreadySettled()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(50m, "BRL"));
        transaction.Settle("ext_123");

        // Act
        var act = () => transaction.Settle("ext_456");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot settle*Succeeded*");
    }

    [Fact]
    public void Settle_ShouldThrow_WhenAlreadyFailed()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(50m, "BRL"));
        transaction.Fail();

        // Act
        var act = () => transaction.Settle("ext_456");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot settle*Failed*");
    }

    [Fact]
    public void Fail_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(50m, "BRL"));

        // Act
        transaction.Fail();

        // Assert
        transaction.Status.Should().Be(EPaymentStatus.Failed);
        transaction.ProcessedAt.Should().NotBeNull();
        transaction.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        transaction.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldThrow_WhenAlreadyFailed()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(50m, "BRL"));
        transaction.Fail();

        // Act
        var act = () => transaction.Fail();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot fail*Failed*");
    }

    [Fact]
    public void Fail_ShouldThrow_WhenAlreadySettled()
    {
        // Arrange
        var transaction = new PaymentTransaction(Guid.NewGuid(), Money.FromDecimal(50m, "BRL"));
        transaction.Settle("ext_123");

        // Act
        var act = () => transaction.Fail();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot fail*Succeeded*");
    }

    [Fact]
    public void Settle_ShouldPreserveAmount()
    {
        // Arrange
        var amount = Money.FromDecimal(250.75m, "USD");
        var transaction = new PaymentTransaction(Guid.NewGuid(), amount);

        // Act
        transaction.Settle("ext_abc");

        // Assert
        transaction.Amount.Should().Be(amount);
        transaction.Amount.Currency.Should().Be("USD");
    }
}
