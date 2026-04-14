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
    }

    [Fact]
    public void Activate_ShouldUpdateStatusAndDates()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        var externalId = "sub_stripe_123";
        var startedAt = DateTime.UtcNow;

        // Act
        subscription.Activate(externalId, startedAt, null);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalId);
        subscription.StartedAt.Should().Be(startedAt);
        subscription.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldUpdateStatus()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "plan_123", Money.FromDecimal(99.90m));
        subscription.Activate("sub_123", DateTime.UtcNow, null);

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }
}
