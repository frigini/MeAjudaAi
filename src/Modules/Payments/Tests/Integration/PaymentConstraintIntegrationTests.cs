using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MeAjudaAi.Modules.Payments.Tests.Integration;

[Collection("PaymentsIntegrationTests")]
public class PaymentConstraintIntegrationTests : PaymentsIntegrationTestBase
{
    [Fact]
    public async Task CreateSubscription_ShouldThrow_OnDuplicateProviderActiveSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalId = $"sub_{Guid.NewGuid():N}";

        using (var scope = CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

            var firstSubscription = new Subscription(providerId, "basic", Money.FromDecimal(29.90m, "BRL"));
            firstSubscription.Activate(externalId, $"cus_{providerId}");

            context.Subscriptions.Add(firstSubscription);
            await context.SaveChangesAsync();
        }

        // Act - insert duplicate
        using var actScope = CreateScope();
        var actContext = actScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var duplicateSubscription = new Subscription(providerId, "pro", Money.FromDecimal(79.90m, "BRL"));
        duplicateSubscription.Activate(externalId, $"cus_{providerId}");

        actContext.Subscriptions.Add(duplicateSubscription);

        // Assert - real PostgreSQL throws DbUpdateException with PostgresException inner
        var dbException = await actContext.Invoking(c => c.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
        dbException.Which.InnerException.Should().BeOfType<PostgresException>();
        ((PostgresException)dbException.Which.InnerException!).SqlState.Should().Be("23505");
    }

    [Fact]
    public async Task CreateSubscription_ShouldSucceed_WhenNoDuplicate()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalId = $"sub_{Guid.NewGuid():N}";

        // Act
        var subscription = await CreateSubscriptionAsync(providerId, "basic", externalId);

        // Assert - Entity was created correctly
        subscription.Should().NotBeNull();
        subscription.ProviderId.Should().Be(providerId);
        subscription.PlanId.Should().Be("basic");
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalId);

        // Assert - Persisted in database
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var saved = await context.Subscriptions.FindAsync(subscription.Id);
        saved.Should().NotBeNull();
        saved!.ProviderId.Should().Be(providerId);
        saved.ExternalSubscriptionId.Should().Be(externalId);
    }
}
