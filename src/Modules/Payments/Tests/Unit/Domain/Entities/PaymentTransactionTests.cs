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
        transaction.UpdatedAt.Should().NotBeNull();
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
        transaction.UpdatedAt.Should().NotBeNull();
    }
}
