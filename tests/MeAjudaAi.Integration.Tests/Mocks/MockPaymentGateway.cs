using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Integration.Tests.Mocks;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var token = Guid.NewGuid().ToString("n");
        return Task.FromResult(new SubscriptionGatewayResult(
            true, 
            "sub_mock_" + token, 
            "https://checkout.stripe.com/mock_" + token, 
            null));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    public async Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult<string?>("https://billing.stripe.com/mock_portal_" + Guid.NewGuid().ToString("n"));
    }
}
