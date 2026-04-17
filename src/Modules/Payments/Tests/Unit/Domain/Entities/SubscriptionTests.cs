using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Shared.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

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
    public void Activate_ShouldUpdateStatusAndDates_AndAddDomainEvent()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var externalId = "sub_stripe_123";
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        // Act
        subscription.Activate(externalId, "cus_123", expiresAt);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalId);
        subscription.ExternalCustomerId.Should().Be("cus_123");
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.UpdatedAt.Should().NotBeNull();
        subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionActivatedDomainEvent);
    }

    [Fact]
    public void Activate_ShouldBeIdempotent_WhenAlreadyActive()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.ClearDomainEvents();
        var originalUpdatedAt = subscription.UpdatedAt;

        // Act
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be("sub_123");
        subscription.UpdatedAt.Should().Be(originalUpdatedAt);
        subscription.DomainEvents.Should().BeEmpty();
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
    public void Activate_ShouldThrowArgumentException_WhenExternalSubscriptionIdIsMissing()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));

        // Act
        var act = () => subscription.Activate("", "cus_123", DateTime.UtcNow.AddMonths(1));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*ExternalSubscriptionId*");
    }

    [Fact]
    public void Activate_ShouldThrowArgumentException_WhenExternalCustomerIdIsMissing()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));

        // Act
        var act = () => subscription.Activate("sub_123", "", DateTime.UtcNow.AddMonths(1));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*ExternalCustomerId*");
    }

    [Fact]
    public void Activate_ShouldThrowArgumentException_WhenExpiresAtIsInvalid()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));

        // Act
        var act = () => subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*ExpiresAt*");
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
    public void Cancel_ShouldUpdateStatus_AndAddDomainEvent()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.ClearDomainEvents();

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
        subscription.UpdatedAt.Should().NotBeNull();
        subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionCanceledDomainEvent);
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent_WhenAlreadyCanceled()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));
        subscription.Cancel();
        subscription.ClearDomainEvents();

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
        subscription.DomainEvents.Should().BeEmpty();
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
        subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionCanceledDomainEvent);
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

        // Act & Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Pending);

        subscription.Activate("sub_ext", "cus_ext", DateTime.UtcNow.AddMonths(1));
        subscription.Status.Should().Be(ESubscriptionStatus.Active);

        subscription.Cancel();
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProviderIdIsEmpty()
    {
        // Act
        var act = () => new Subscription(Guid.Empty, "plan", Money.FromDecimal(10));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*ProviderId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenPlanIdIsInvalid(string planId)
    {
        // Act
        var act = () => new Subscription(Guid.NewGuid(), planId, Money.FromDecimal(10));

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*PlanId*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNull()
    {
        // Act
        var act = () => new Subscription(Guid.NewGuid(), "plan", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*Amount*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenAmountIsNonPositive(decimal amount)
    {
        // Act
        var act = () => new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(amount));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Amount*");
    }

    [Fact]
    public void Activate_ShouldThrow_WhenStatusIsCanceled()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Cancel();

        // Act
        var act = () => subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*canceled*");
    }

    [Fact]
    public void Activate_ShouldThrow_WhenStatusIsExpired()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        subscription.Expire();

        // Act
        var act = () => subscription.Activate("sub2", "cus2", DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*expired*");
    }

    [Fact]
    public void Renew_ShouldThrow_WhenStatusIsCanceled()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Cancel();

        // Act
        var act = () => subscription.Renew(DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Canceled*");
    }

    [Fact]
    public void Renew_ShouldThrow_WhenStatusIsExpired()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        subscription.Expire();

        // Act
        var act = () => subscription.Renew(DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Expired*");
    }

    [Fact]
    public void Renew_ShouldThrow_WhenDateIsNotInFuture()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddMonths(1));

        // Act
        var act = () => subscription.Renew(DateTime.UtcNow.AddSeconds(-1));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*future date*");
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent_WhenExpired()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddDays(1));
        subscription.Expire();

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Expired);
    }

    [Fact]
    public void Renew_ShouldThrow_WhenStatusIsPending()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));

        // Act
        var act = () => subscription.Renew(DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Only Active subscriptions can be renewed*");
    }

    [Fact]
    public void Activate_ShouldUpdateExternalIds_WhenStatusIsActiveButIdsAreDifferent()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate("old_sub", "old_cus", DateTime.UtcNow.AddDays(1));
        var originalUpdatedAt = subscription.UpdatedAt;

        // Act
        subscription.Activate("new_sub", "new_cus", DateTime.UtcNow.AddDays(2));

        // Assert
        subscription.ExternalSubscriptionId.Should().Be("new_sub");
        subscription.ExternalCustomerId.Should().Be("new_cus");
        subscription.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Fact]
    public void Renew_ShouldThrow_WhenNewExpiresAtIsNotAfterCurrent()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        var currentExpiresAt = DateTime.UtcNow.AddMonths(1);
        subscription.Activate("sub", "cus", currentExpiresAt);
        
        // Act
        var act = () => subscription.Renew(currentExpiresAt);
        
        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*after current expiration date*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MaskExternalId_ShouldReturnEmpty_WhenInputIsNullOrEmpty(string? input)
    {
        // Act & Assert
        Subscription.MaskExternalId(input!).Should().BeEmpty();
    }

    [Fact]
    public void MaskExternalId_ShouldMaskShortId()
    {
        // Act
        var result = Subscription.MaskExternalId("abc123");

        // Assert
        result.Should().Be("****c123");
    }

    [Fact]
    public void MaskExternalId_ShouldMaskLongId()
    {
        // Act
        var result = Subscription.MaskExternalId("sub_abc123xyz789");

        // Assert
        result.Should().Be("sub_****z789");
    }
}
