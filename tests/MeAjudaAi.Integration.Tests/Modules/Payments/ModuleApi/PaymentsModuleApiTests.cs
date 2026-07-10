using MeAjudaAi.Contracts.Modules.Payments;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

namespace MeAjudaAi.Integration.Tests.Modules.Payments.ModuleApi;

public class PaymentsModuleApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Payments;

    [Fact]
    public async Task HasActiveSubscriptionAsync_WhenNoSubscription_ReturnsFalse()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var paymentsApi = scope.ServiceProvider.GetRequiredService<IPaymentsModuleApi>();
        var providerId = Guid.NewGuid();

        // Act
        var result = await paymentsApi.HasActiveSubscriptionAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSubscriptionAsync_WhenActiveSubscriptionExists_ReturnsTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        using (var scope = Services.CreateScope())
        {
            var paymentsDb = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var subscription = new Subscription(providerId, "gold-plan", new MeAjudaAi.Shared.Domain.ValueObjects.Money(100, "BRL"));
            subscription.Activate("ext-id-123", "cust-123");
            paymentsDb.Subscriptions.Add(subscription);
            await paymentsDb.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var paymentsApi = scope.ServiceProvider.GetRequiredService<IPaymentsModuleApi>();
            
            // Act
            var result = await paymentsApi.HasActiveSubscriptionAsync(providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetActiveSubscriptionByProviderIdAsync_WhenNoActiveSubscription_ReturnsNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        using (var scope = Services.CreateScope())
        {
            var paymentsApi = scope.ServiceProvider.GetRequiredService<IPaymentsModuleApi>();
            
            // Act
            var result = await paymentsApi.GetActiveSubscriptionByProviderIdAsync(providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetActiveSubscriptionByProviderIdAsync_WhenActiveSubscriptionExists_ReturnsDto()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var planId = "gold-plan";
        using (var scope = Services.CreateScope())
        {
            var paymentsDb = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var subscription = new Subscription(providerId, planId, new MeAjudaAi.Shared.Domain.ValueObjects.Money(100, "BRL"));
            subscription.Activate("ext-id-123", "cust-123");
            paymentsDb.Subscriptions.Add(subscription);
            await paymentsDb.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var paymentsApi = scope.ServiceProvider.GetRequiredService<IPaymentsModuleApi>();
            
            // Act
            var result = await paymentsApi.GetActiveSubscriptionByProviderIdAsync(providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.ProviderId.Should().Be(providerId);
            result.Value.PlanId.Should().Be(planId);
            result.Value.Status.ToString().Should().Be("Active");
        }
    }
}
