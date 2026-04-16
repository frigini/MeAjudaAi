using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.E2E.Tests.Infrastructure.Mocks;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString("n");
        return Task.FromResult(SubscriptionGatewayResult.Succeeded(
            "sub_mock_" + token, 
            "https://checkout.stripe.com/mock_" + token));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>("https://billing.stripe.com/mock_session");
    }
}
