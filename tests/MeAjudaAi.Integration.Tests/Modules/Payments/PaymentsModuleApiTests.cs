using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Payments;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Payments;

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
}
