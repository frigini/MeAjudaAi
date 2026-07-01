using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Payments;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Tests.Integration;

public abstract class PaymentsIntegrationTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"payments_test_{GetType().Name}",
                Username = "test_user",
                Password = "test_password",
                Schema = "payments"
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddPaymentsTestInfrastructure(options);
    }

    protected MockPaymentGateway GetMockPaymentGateway() =>
        GetService<IPaymentGateway>() as MockPaymentGateway
        ?? throw new InvalidOperationException("IPaymentGateway is not MockPaymentGateway");

    protected async Task<Subscription> CreateSubscriptionAsync(
        Guid providerId, string planId, string? externalId = null,
        CancellationToken cancellationToken = default)
    {
        var subscription = new Subscription(providerId, planId, Money.FromDecimal(29.90m, "BRL"));

        if (externalId != null)
        {
            subscription.Activate(externalId, $"cus_{providerId}");
        }

        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync(cancellationToken);
        return subscription;
    }
}
