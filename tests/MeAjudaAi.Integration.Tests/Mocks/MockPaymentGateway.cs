using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Integration.Tests.Mocks;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SubscriptionGatewayResult(
            true, 
            "sub_mock_" + Guid.NewGuid().ToString("n"), 
            "https://checkout.stripe.com/mock_" + Guid.NewGuid().ToString("n"), 
            null));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
