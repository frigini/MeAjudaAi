using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_ShouldSetInitialValues()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "plan_123";
        var amount = Money.FromDecimal(99.90m, "BRL");

        // Act
        var subscription = new Subscription(providerId, planId, amount);

        // Assert
        subscription.ProviderId.Should().Be(providerId);
        subscription.PlanId.Should().Be(planId);
        subscription.Amount.Should().Be(amount);
        subscription.Status.Should().Be(ESubscriptionStatus.Pending);
        subscription.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.ExternalSubscriptionId.Should().BeNull();
        subscription.ExternalCustomerId.Should().BeNull();
        subscription.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldUpdateStatusAndDates()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var externalId = "sub_stripe_123";
        var customerId = "cus_123";
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        // Act
        subscription.Activate(externalId, customerId, expiresAt);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalId);
        subscription.ExternalCustomerId.Should().Be(customerId);
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldBeIdempotent_WhenAlreadyActive()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        var originalUpdatedAt = subscription.UpdatedAt;

        // Act - Activate again
        subscription.Activate("sub_456", "cus_456", DateTime.UtcNow.AddMonths(1));

        // Assert - Should be idempotent, not change
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be("sub_123"); // Original value kept
        subscription.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldAcceptExpiresAt()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        // Act
        subscription.Activate("sub_123", "cus_123", expiresAt);

        // Assert
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
    }

    [Fact]
    public void Renew_ShouldUpdateExpiresAt()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.10m, "BRL"));
        var originalExpiresAt = DateTime.UtcNow.AddMonths(1);
        subscription.Activate("sub_123", "cus_123", originalExpiresAt);
        var newExpiresAt = originalExpiresAt.AddMonths(1);

        // Act
        subscription.Renew(newExpiresAt);

        // Assert
        subscription.ExpiresAt.Should().Be(newExpiresAt);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldUpdateStatus()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent_WhenAlreadyCanceled()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.Cancel();

        // Act - Cancel again
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void Cancel_ShouldWorkFromPendingStatus()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void Expire_ShouldUpdateStatus()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        // Act
        subscription.Expire();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Expire_ShouldBeIdempotent_WhenAlreadyExpired()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.Expire();

        // Act
        subscription.Expire();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
    }


    [Fact]
    public void FullLifecycle_Pending_Activate_Cancel()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_full", Money.FromDecimal(199.90m, "USD"));

        // Assert initial
        subscription.Status.Should().Be(ESubscriptionStatus.Pending);

        // Activate
        subscription.Activate("sub_ext", "cus_ext", DateTime.UtcNow.AddMonths(1));
        subscription.Status.Should().Be(ESubscriptionStatus.Active);

        // Cancel
        subscription.Cancel();
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void FullLifecycle_Pending_Activate_Expire()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_full", Money.FromDecimal(199.90m, "BRL"));

        // Activate
        subscription.Activate("sub_ext", "cus_ext", DateTime.UtcNow.AddMonths(6));
        subscription.Status.Should().Be(ESubscriptionStatus.Active);

        // Expire
        subscription.Expire();
        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
    }
}
