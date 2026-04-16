using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_ShouldSetInitialValues()
    {
        var providerId = Guid.NewGuid();
        var planId = "plan_123";
        var amount = Money.FromDecimal(99.90m, "BRL");

        var subscription = new Subscription(providerId, planId, amount);

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
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var externalId = "sub_stripe_123";
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        subscription.Activate(externalId, "cus_123", expiresAt);

        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalId);
        subscription.ExternalCustomerId.Should().Be("cus_123");
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldBeIdempotent_WhenAlreadyActive()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        var originalUpdatedAt = subscription.UpdatedAt;

        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be("sub_123");
        subscription.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldAcceptExpiresAt()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        subscription.Activate("sub_123", "cus_123", expiresAt);

        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
    }

    [Fact]
    public void Activate_ShouldThrowArgumentException_WhenExternalSubscriptionIdIsMissing()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var act = () => subscription.Activate("", "cus_123", DateTime.UtcNow.AddMonths(1));
        act.Should().Throw<ArgumentException>().WithMessage("*ExternalSubscriptionId*");
    }

    [Fact]
    public void Activate_ShouldThrowArgumentException_WhenExternalCustomerIdIsMissing()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var act = () => subscription.Activate("sub_123", "", DateTime.UtcNow.AddMonths(1));
        act.Should().Throw<ArgumentException>().WithMessage("*ExternalCustomerId*");
    }

    [Fact]
    public void Activate_ShouldThrowArgumentException_WhenExpiresAtIsInvalid()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var act = () => subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddDays(-1));
        act.Should().Throw<ArgumentException>().WithMessage("*ExpiresAt*");
    }

    [Fact]
    public void Renew_ShouldUpdateExpiresAt()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.10m, "BRL"));
        var originalExpiresAt = DateTime.UtcNow.AddMonths(1);
        subscription.Activate("sub_123", "cus_123", originalExpiresAt);
        var newExpiresAt = originalExpiresAt.AddMonths(1);

        subscription.Renew(newExpiresAt);

        subscription.ExpiresAt.Should().Be(newExpiresAt);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldUpdateStatus()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        subscription.Cancel();

        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent_WhenAlreadyCanceled()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.Cancel();

        subscription.Cancel();

        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void Cancel_ShouldWorkFromPendingStatus()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));

        subscription.Cancel();

        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void Expire_ShouldUpdateStatus()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        subscription.Expire();

        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Expire_ShouldBeIdempotent_WhenAlreadyExpired()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.Expire();

        subscription.Expire();

        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
    }

    [Fact]
    public void FullLifecycle_Pending_Activate_Cancel()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan_full", Money.FromDecimal(199.90m, "USD"));

        subscription.Status.Should().Be(ESubscriptionStatus.Pending);

        subscription.Activate("sub_ext", "cus_ext", DateTime.UtcNow.AddMonths(1));
        subscription.Status.Should().Be(ESubscriptionStatus.Active);

        subscription.Cancel();
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProviderIdIsEmpty()
    {
        var act = () => new Subscription(Guid.Empty, "plan", Money.FromDecimal(10));
        act.Should().Throw<ArgumentException>().WithMessage("*ProviderId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenPlanIdIsInvalid(string planId)
    {
        var act = () => new Subscription(Guid.NewGuid(), planId, Money.FromDecimal(10));
        act.Should().Throw<ArgumentNullException>().WithMessage("*PlanId*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNull()
    {
        var act = () => new Subscription(Guid.NewGuid(), "plan", null!);
        act.Should().Throw<ArgumentNullException>().WithMessage("*Amount*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenAmountIsNonPositive(decimal amount)
    {
        var act = () => new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(amount));
        act.Should().Throw<ArgumentException>().WithMessage("*Amount*");
    }

    [Fact]
    public void Activate_ShouldThrow_WhenStatusIsCanceled()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Cancel();
        var act = () => subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>().WithMessage("*canceled*");
    }

    [Fact]
    public void Activate_ShouldThrow_WhenStatusIsExpired()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        subscription.Expire();
        var act = () => subscription.Activate("sub2", "cus2", DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>().WithMessage("*expired*");
    }

    [Fact]
    public void Renew_ShouldThrow_WhenStatusIsCanceled()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Cancel();
        var act = () => subscription.Renew(DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>().WithMessage("*Canceled*");
    }

    [Fact]
    public void Renew_ShouldThrow_WhenStatusIsExpired()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        subscription.Expire();
        var act = () => subscription.Renew(DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>().WithMessage("*Expired*");
    }

    [Fact]
    public void Renew_ShouldThrow_WhenDateIsNotInFuture()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddMonths(1));
        var act = () => subscription.Renew(DateTime.UtcNow.AddSeconds(-1));
        act.Should().Throw<ArgumentException>().WithMessage("*future date*");
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent_WhenExpired()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        subscription.Expire();
        subscription.Cancel();
        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
    }

    [Fact]
    public void Renew_ShouldThrow_WhenStatusIsPending()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        var act = () => subscription.Renew(DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidOperationException>().WithMessage("*Only Active subscriptions can be renewed*");
    }

    [Fact]
    public void Activate_ShouldUpdateExternalIds_WhenStatusIsActiveButIdsAreDifferent()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("old_sub", "old_cus", DateTime.UtcNow.AddDays(1));
        var originalUpdatedAt = subscription.UpdatedAt;

        subscription.Activate("new_sub", "new_cus", DateTime.UtcNow.AddDays(2));

        subscription.ExternalSubscriptionId.Should().Be("new_sub");
        subscription.ExternalCustomerId.Should().Be("new_cus");
        subscription.UpdatedAt.Should().BeAfter(originalUpdatedAt!.Value);
    }

    [Fact]
    public void Renew_ShouldThrow_WhenNewExpiresAtIsNotAfterCurrent()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        var currentExpiresAt = DateTime.UtcNow.AddMonths(1);
        subscription.Activate("sub", "cus", currentExpiresAt);
        
        var act = () => subscription.Renew(currentExpiresAt);
        
        act.Should().Throw<ArgumentException>().WithMessage("*after current expiration date*");
    }
}