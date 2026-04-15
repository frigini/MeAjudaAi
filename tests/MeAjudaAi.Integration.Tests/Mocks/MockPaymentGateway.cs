using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Integration.Tests.Mocks;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString("n");
        return Task.FromResult(new SubscriptionGatewayResult(
            true, 
            "sub_mock_" + token, 
            "https://checkout.stripe.com/mock_" + token, 
            null));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
