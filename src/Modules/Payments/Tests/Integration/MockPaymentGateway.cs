using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Tests.Integration;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly Dictionary<string, SubscriptionGatewayResponse> _subscriptionResponses = new();
    private readonly Dictionary<string, bool> _cancelResults = new();
    private readonly Dictionary<string, string?> _portalSessionUrls = new();

    public void SetupCreateSubscriptionResponse(Guid providerId, SubscriptionGatewayResponse response)
        => _subscriptionResponses[providerId.ToString()] = response;

    public void SetupCancelSubscriptionResponse(string externalId, bool result)
        => _cancelResults[externalId] = result;

    public void SetupBillingPortalSession(string customerId, string? url)
        => _portalSessionUrls[customerId] = url;

    public Task<SubscriptionGatewayResponse> CreateSubscriptionAsync(
        Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        if (_subscriptionResponses.TryGetValue(providerId.ToString(), out var response))
            return Task.FromResult(response);

        return Task.FromResult(new SubscriptionGatewayResponse(
            Success: true,
            ExternalSubscriptionId: $"sub_test_{providerId}",
            CheckoutUrl: $"https://checkout.stripe.com/test_{providerId}",
            ErrorMessage: null));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        if (_cancelResults.TryGetValue(externalSubscriptionId, out var result))
            return Task.FromResult(result);

        return Task.FromResult(true);
    }

    public Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken = default)
    {
        if (_portalSessionUrls.TryGetValue(externalCustomerId, out var url))
            return Task.FromResult(url);

        return Task.FromResult($"https://billing.stripe.com/test_{externalCustomerId}");
    }
}
